using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    public class SensitivityRange
    {
        public string Name;
        public double CurrentValue;
        public double AllowableDecrease;
        public double AllowableIncrease;
        public bool IsBasic;
    }

    public class SensitivityAnalysis
    {
        private const double Tolerance = 1e-9;

        public string CreateReport(RevisedSimplexResult result)
        {
            return CreateReport(result, "Revised Primal Simplex");
        }

        public string CreateReport(RevisedSimplexResult result, string methodName)
        {
            if (result == null || result.Model == null)
                throw new ArgumentException("A revised simplex result is required.");

            if (result.Status != "Optimal" || result.FinalBasis == null || result.FinalBinv == null)
                throw new InvalidOperationException("Sensitivity analysis requires an optimal revised simplex result.");

            StandardModel model = result.Model;
            int constraintCount = model.Constraints.Count;
            int variableCount = model.VariableNames.Count;
            bool isMax = model.ObjectiveType.Trim().ToLower() == "max";
            double sense = isMax ? 1.0 : -1.0;

            double[,] matrix = BuildMatrix(model, constraintCount, variableCount);
            double[] objective = model.ObjectiveCoefficients.Select(value => -value).ToArray();
            double[] basisObjective = result.FinalBasis.Select(index => sense * objective[index]).ToArray();
            double[] shadowPricesWorking = MultiplyRowByMatrix(basisObjective, result.FinalBinv);
            double[] shadowPrices = shadowPricesWorking.Select(value => sense * value).ToArray();
            double[] basicValues = MultiplyMatrixByVector(result.FinalBinv,
                model.Constraints.Select(constraint => constraint.RHS).ToArray());

            var report = new StringBuilder();
            report.AppendLine("SENSITIVITY ANALYSIS");
            report.AppendLine("Solver: " + (string.IsNullOrWhiteSpace(methodName) ? "Simplex" : methodName));
            report.AppendLine("Valid while the current optimal basis remains unchanged.");
            report.AppendLine();
            report.AppendLine("Objective value: " + Format(result.Z));
            report.AppendLine();
            report.AppendLine("Constraint sensitivity (RHS):");
            report.AppendLine("Constraint\tRHS\tShadow price\tAllowable decrease\tAllowable increase");

            for (int row = 0; row < constraintCount; row++)
            {
                double lowerDelta = double.NegativeInfinity;
                double upperDelta = double.PositiveInfinity;
                for (int basicRow = 0; basicRow < constraintCount; basicRow++)
                {
                    double coefficient = result.FinalBinv[basicRow, row];
                    if (coefficient > Tolerance)
                        lowerDelta = Math.Max(lowerDelta, -basicValues[basicRow] / coefficient);
                    else if (coefficient < -Tolerance)
                        upperDelta = Math.Min(upperDelta, -basicValues[basicRow] / coefficient);
                }

                double rhs = model.Constraints[row].RHS;
                report.AppendLine(string.Join("\t", row + 1, Format(rhs), Format(shadowPrices[row]),
                    Format(-lowerDelta), Format(upperDelta)));
            }

            report.AppendLine();
            report.AppendLine("Variable reduced costs:");
            report.AppendLine("Variable\tValue\tReduced cost");
            double[] reducedCostsWorking = new double[variableCount];
            for (int column = 0; column < variableCount; column++)
            {
                double pricedValue = 0;
                for (int row = 0; row < constraintCount; row++)
                    pricedValue += shadowPricesWorking[row] * matrix[row, column];
                reducedCostsWorking[column] = sense * objective[column] - pricedValue;

                double value = result.Solution != null && result.Solution.ContainsKey(model.VariableNames[column])
                    ? result.Solution[model.VariableNames[column]] : 0.0;
                report.AppendLine(string.Join("\t", model.VariableNames[column], Format(value),
                    Format(sense * reducedCostsWorking[column])));
            }

            report.AppendLine();
            report.AppendLine("Objective coefficient sensitivity (decision variables):");
            report.AppendLine("Variable\tObjective coefficient\tReduced cost\tAllowable decrease\tAllowable increase");
            for (int column = 0; column < variableCount; column++)
            {
                if (IsAuxiliaryVariable(model.VariableNames[column]))
                    continue;

                double lowerDeltaWorking;
                double upperDeltaWorking;
                int basisPosition = result.FinalBasis.IndexOf(column);
                if (basisPosition < 0)
                {
                    lowerDeltaWorking = double.NegativeInfinity;
                    upperDeltaWorking = -reducedCostsWorking[column];
                }
                else
                {
                    lowerDeltaWorking = double.NegativeInfinity;
                    upperDeltaWorking = double.PositiveInfinity;
                    for (int pricedColumn = 0; pricedColumn < variableCount; pricedColumn++)
                    {
                        if (result.FinalBasis.Contains(pricedColumn))
                            continue;

                        double coefficient = 0.0;
                        for (int row = 0; row < constraintCount; row++)
                            coefficient += result.FinalBinv[basisPosition, row] * matrix[row, pricedColumn];

                        if (coefficient > Tolerance)
                            lowerDeltaWorking = Math.Max(lowerDeltaWorking,
                                reducedCostsWorking[pricedColumn] / coefficient);
                        else if (coefficient < -Tolerance)
                            upperDeltaWorking = Math.Min(upperDeltaWorking,
                                reducedCostsWorking[pricedColumn] / coefficient);
                    }
                }

                double lowerDelta = lowerDeltaWorking / sense;
                double upperDelta = upperDeltaWorking / sense;
                if (lowerDelta > upperDelta)
                {
                    double swap = lowerDelta;
                    lowerDelta = upperDelta;
                    upperDelta = swap;
                }

                report.AppendLine(string.Join("\t", model.VariableNames[column], Format(objective[column]),
                    Format(sense * reducedCostsWorking[column]), Format(-lowerDelta), Format(upperDelta)));
            }

            report.AppendLine();
            report.AppendLine("Interpretation: a zero reduced cost indicates a basic variable or an alternate-optimum candidate.");
            report.AppendLine("RHS changes inside the allowable range preserve the current optimal basis.");
            report.AppendLine("Objective coefficient changes inside the allowable range preserve the current optimal basis.");
            return report.ToString();
        }

        public SensitivityRange GetConstraintRange(RevisedSimplexResult result, int constraintIndex)
        {
            ValidateResult(result);
            if (constraintIndex < 0 || constraintIndex >= result.Model.Constraints.Count)
                throw new ArgumentOutOfRangeException("constraintIndex");

            int rowCount = result.Model.Constraints.Count;
            double[] basicValues = MultiplyMatrixByVector(result.FinalBinv,
                result.Model.Constraints.Select(constraint => constraint.RHS).ToArray());
            double lower = double.NegativeInfinity;
            double upper = double.PositiveInfinity;
            for (int basicRow = 0; basicRow < rowCount; basicRow++)
            {
                double coefficient = result.FinalBinv[basicRow, constraintIndex];
                if (coefficient > Tolerance)
                    lower = Math.Max(lower, -basicValues[basicRow] / coefficient);
                else if (coefficient < -Tolerance)
                    upper = Math.Min(upper, -basicValues[basicRow] / coefficient);
            }

            return new SensitivityRange
            {
                Name = "Constraint " + (constraintIndex + 1),
                CurrentValue = result.Model.Constraints[constraintIndex].RHS,
                AllowableDecrease = -lower,
                AllowableIncrease = upper
            };
        }

        public SensitivityRange GetObjectiveCoefficientRange(RevisedSimplexResult result, int variableIndex)
        {
            ValidateResult(result);
            if (variableIndex < 0 || variableIndex >= result.Model.VariableNames.Count)
                throw new ArgumentOutOfRangeException("variableIndex");

            int rowCount = result.Model.Constraints.Count;
            int columnCount = result.Model.VariableNames.Count;
            double sense = result.Model.ObjectiveType.Trim().ToLower() == "max" ? 1.0 : -1.0;
            double[,] matrix = BuildMatrix(result.Model, rowCount, columnCount);
            double[] objective = result.Model.ObjectiveCoefficients.Select(value => -value).ToArray();
            double[] basisObjective = result.FinalBasis.Select(index => sense * objective[index]).ToArray();
            double[] prices = MultiplyRowByMatrix(basisObjective, result.FinalBinv);
            double[] reduced = new double[columnCount];
            for (int column = 0; column < columnCount; column++)
            {
                reduced[column] = sense * objective[column];
                for (int row = 0; row < rowCount; row++)
                    reduced[column] -= prices[row] * matrix[row, column];
            }

            double lowerWorking = double.NegativeInfinity;
            double upperWorking = double.PositiveInfinity;
            int basisPosition = result.FinalBasis.IndexOf(variableIndex);
            if (basisPosition < 0)
            {
                upperWorking = -reduced[variableIndex];
            }
            else
            {
                for (int column = 0; column < columnCount; column++)
                {
                    if (result.FinalBasis.Contains(column))
                        continue;
                    double coefficient = 0.0;
                    for (int row = 0; row < rowCount; row++)
                        coefficient += result.FinalBinv[basisPosition, row] * matrix[row, column];
                    if (coefficient > Tolerance)
                        lowerWorking = Math.Max(lowerWorking, reduced[column] / coefficient);
                    else if (coefficient < -Tolerance)
                        upperWorking = Math.Min(upperWorking, reduced[column] / coefficient);
                }
            }

            double lower = lowerWorking / sense;
            double upper = upperWorking / sense;
            if (lower > upper)
            {
                double swap = lower;
                lower = upper;
                upper = swap;
            }
            return new SensitivityRange
            {
                Name = result.Model.VariableNames[variableIndex],
                CurrentValue = objective[variableIndex],
                AllowableDecrease = -lower,
                AllowableIncrease = upper,
                IsBasic = basisPosition >= 0
            };
        }

        public RevisedSimplexResult ApplyConstraintRhsChange(RevisedSimplexResult result,
            int constraintIndex, double newRhs, out string report)
        {
            ValidateResult(result);
            StandardModel model = CloneModel(result.Model);
            model.Constraints[constraintIndex].RHS = newRhs;
            return SolveAndReport(model, "RHS change for Constraint " + (constraintIndex + 1), out report);
        }

        public RevisedSimplexResult ApplyObjectiveCoefficientChange(RevisedSimplexResult result,
            int variableIndex, double newCoefficient, out string report)
        {
            ValidateResult(result);
            StandardModel model = CloneModel(result.Model);
            model.ObjectiveCoefficients[variableIndex] = -newCoefficient;
            return SolveAndReport(model, "Objective coefficient change for " + model.VariableNames[variableIndex], out report);
        }

        public RevisedSimplexResult ApplyNonBasicColumnChange(RevisedSimplexResult result,
            int variableIndex, int constraintIndex, double newCoefficient, out string report)
        {
            ValidateResult(result);
            if (result.FinalBasis.Contains(variableIndex))
                throw new InvalidOperationException("Column sensitivity changes are supported for non-basic variables only.");
            StandardModel model = CloneModel(result.Model);
            model.Constraints[constraintIndex].Coefficients[variableIndex] = newCoefficient;
            return SolveAndReport(model, "Non-basic column change for " + model.VariableNames[variableIndex], out report);
        }

        public SensitivityRange GetNonBasicColumnRange(RevisedSimplexResult result,
            int variableIndex, int constraintIndex)
        {
            ValidateResult(result);
            if (variableIndex < 0 || variableIndex >= result.Model.VariableNames.Count)
                throw new ArgumentOutOfRangeException("variableIndex");
            if (constraintIndex < 0 || constraintIndex >= result.Model.Constraints.Count)
                throw new ArgumentOutOfRangeException("constraintIndex");
            if (result.FinalBasis.Contains(variableIndex))
                throw new InvalidOperationException("Column ranges are supported for non-basic variables only.");

            int rowCount = result.Model.Constraints.Count;
            int columnCount = result.Model.VariableNames.Count;
            bool isMax = result.Model.ObjectiveType.Trim().ToLower() == "max";
            double sense = isMax ? 1.0 : -1.0;
            double[,] matrix = BuildMatrix(result.Model, rowCount, columnCount);
            double[] objective = result.Model.ObjectiveCoefficients.Select(value => -value).ToArray();
            double[] basisObjective = result.FinalBasis.Select(index => sense * objective[index]).ToArray();
            double[] prices = MultiplyRowByMatrix(basisObjective, result.FinalBinv);
            double reduced = sense * objective[variableIndex];
            for (int row = 0; row < rowCount; row++)
                reduced -= prices[row] * matrix[row, variableIndex];

            double lowerDelta = double.NegativeInfinity;
            double upperDelta = double.PositiveInfinity;
            double price = prices[constraintIndex];
            if (price > Tolerance)
                upperDelta = reduced / price;
            else if (price < -Tolerance)
                lowerDelta = reduced / price;

            double current = matrix[constraintIndex, variableIndex];
            return new SensitivityRange
            {
                Name = result.Model.VariableNames[variableIndex] + " coefficient in Constraint " + (constraintIndex + 1),
                CurrentValue = current,
                AllowableDecrease = -lowerDelta,
                AllowableIncrease = upperDelta
            };
        }

        public RevisedSimplexResult AddActivity(RevisedSimplexResult result, string name,
            double objectiveCoefficient, IList<double> constraintCoefficients, out string report)
        {
            ValidateResult(result);
            if (constraintCoefficients == null || constraintCoefficients.Count != result.Model.Constraints.Count)
                throw new ArgumentException("One coefficient is required for every existing constraint.");
            StandardModel model = CloneModel(result.Model);
            model.VariableNames.Add(name);
            model.ObjectiveCoefficients.Add(-objectiveCoefficient);
            model.SignRestrictions.Add("none");
            for (int row = 0; row < model.Constraints.Count; row++)
                model.Constraints[row].Coefficients.Add(constraintCoefficients[row]);
            return SolveAndReport(model, "New activity " + name, out report);
        }

        public RevisedSimplexResult AddConstraint(RevisedSimplexResult result, string relation,
            double rhs, IList<double> coefficients, out string report)
        {
            ValidateResult(result);
            if (coefficients == null || coefficients.Count != result.Model.VariableNames.Count)
                throw new ArgumentException("One coefficient is required for every existing variable.");
            StandardModel model = CloneModel(result.Model);
            model.Constraints.Add(new Constraint
            {
                Relation = relation,
                RHS = rhs,
                Coefficients = new List<double>(coefficients)
            });
            return SolveAndReport(model, "New constraint", out report);
        }

        public StandardModel BuildDualModel(StandardModel primal)
        {
            if (primal == null || primal.Constraints == null || primal.VariableNames == null)
                throw new ArgumentException("A standard primal model is required.");
            List<int> decisionColumns = Enumerable.Range(0, primal.VariableNames.Count)
                .Where(index => !IsAuxiliaryVariable(primal.VariableNames[index])).ToList();
            StandardModel dual = new StandardModel
            {
                ObjectiveType = primal.ObjectiveType.Trim().ToLower() == "max" ? "min" : "max",
                ObjectiveCoefficients = new List<double>(),
                VariableNames = new List<string>(),
                SignRestrictions = new List<string>(),
                Constraints = new List<Constraint>()
            };
            for (int row = 0; row < primal.Constraints.Count; row++)
            {
                dual.VariableNames.Add("y" + (row + 1));
                dual.SignRestrictions.Add("none");
                dual.ObjectiveCoefficients.Add(-primal.Constraints[row].RHS);
            }
            for (int columnIndex = 0; columnIndex < decisionColumns.Count; columnIndex++)
            {
                int primalColumn = decisionColumns[columnIndex];
                Constraint constraint = new Constraint
                {
                    Relation = primal.ObjectiveType.Trim().ToLower() == "max" ? ">=" : "<=",
                    RHS = -primal.ObjectiveCoefficients[primalColumn],
                    Coefficients = new List<double>()
                };
                for (int row = 0; row < primal.Constraints.Count; row++)
                    constraint.Coefficients.Add(primal.Constraints[row].Coefficients[primalColumn]);
                dual.Constraints.Add(constraint);
            }
            return dual;
        }

        public RevisedSimplexResult SolveDual(RevisedSimplexResult primalResult, out string report)
        {
            ValidateResult(primalResult);
            StandardModel dual = BuildDualModel(primalResult.Model);
            return SolveAndReport(dual, "Dual model", out report);
        }

        private RevisedSimplexResult SolveAndReport(StandardModel model, string methodName, out string report)
        {
            RevisedSimplex solver = new RevisedSimplex();
            RevisedSimplexResult result;
            if (!solver.Solve(model, out result))
                throw new InvalidOperationException("The changed model could not be solved.");
            report = CreateReport(result, methodName);
            return result;
        }

        private static void ValidateResult(RevisedSimplexResult result)
        {
            if (result == null || result.Model == null || result.Status != "Optimal" ||
                result.FinalBasis == null || result.FinalBinv == null)
                throw new InvalidOperationException("Sensitivity analysis requires an optimal simplex result.");
        }

        private static StandardModel CloneModel(StandardModel source)
        {
            StandardModel clone = new StandardModel
            {
                ObjectiveType = source.ObjectiveType,
                ObjectiveFunctionRHS = source.ObjectiveFunctionRHS,
                ObjectiveCoefficients = new List<double>(source.ObjectiveCoefficients),
                VariableNames = new List<string>(source.VariableNames),
                SignRestrictions = new List<string>(source.SignRestrictions),
                Constraints = new List<Constraint>(),
                TwoPhaseObjective = source.TwoPhaseObjective
            };
            foreach (Constraint constraint in source.Constraints)
                clone.Constraints.Add(new Constraint
                {
                    Relation = constraint.Relation,
                    RHS = constraint.RHS,
                    Coefficients = new List<double>(constraint.Coefficients)
                });
            return clone;
        }

        private static bool IsAuxiliaryVariable(string variableName)
        {
            return variableName.StartsWith("s") || variableName.StartsWith("e") || variableName.StartsWith("a");
        }

        private static double[,] BuildMatrix(StandardModel model, int rows, int columns)
        {
            var matrix = new double[rows, columns];
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    matrix[row, column] = model.Constraints[row].Coefficients[column];
            return matrix;
        }

        private static double[] MultiplyRowByMatrix(double[] row, double[,] matrix)
        {
            int size = row.Length;
            var result = new double[size];
            for (int column = 0; column < size; column++)
                for (int index = 0; index < size; index++)
                    result[column] += row[index] * matrix[index, column];
            return result;
        }

        private static double[] MultiplyMatrixByVector(double[,] matrix, double[] vector)
        {
            int size = vector.Length;
            var result = new double[size];
            for (int row = 0; row < size; row++)
                for (int column = 0; column < size; column++)
                    result[row] += matrix[row, column] * vector[column];
            return result;
        }

        private static string Format(double value)
        {
            return double.IsPositiveInfinity(value) ? "infinity" :
                double.IsNegativeInfinity(value) ? "-infinity" :
                Math.Round(value, 3, MidpointRounding.AwayFromZero).ToString("0.###");
        }
    }
}
