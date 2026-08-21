using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    internal class SensitivityAnalysis
    {
        private const double Tolerance = 1e-9;

        public string CreateReport(RevisedSimplexResult result)
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
            report.AppendLine("Interpretation: a zero reduced cost indicates a basic variable or an alternate-optimum candidate.");
            report.AppendLine("RHS changes inside the allowable range preserve the current optimal basis.");
            return report.ToString();
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
