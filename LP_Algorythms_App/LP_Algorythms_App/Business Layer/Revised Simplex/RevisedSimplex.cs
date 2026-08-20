using System;
using System.Collections.Generic;
using System.Linq;

namespace LP_Algorythms_App.Business_Layer
{
    /// <summary>
    /// Revised Primal Simplex Algorithm, using the Product Form of the Inverse.
    ///
    /// Instead of pivoting a full tableau (as Reused_Algorythms.PrimalSimplex does),
    /// this keeps only the m x m matrix B^-1 and updates it each iteration by
    /// left-multiplying with an "eta" matrix - this running product IS the
    /// Product Form of the Inverse. Every iteration also performs the
    /// "Price Out" step (y = c_B * B^-1) to get the simplex multipliers used
    /// to price every non-basic column.
    ///
    /// This takes the StandardModel produced by DataConversion.ToStandard(), so it
    /// shares the exact same input parsing / sign-restriction handling as the rest
    /// of the app - no separate parser needed.
    ///
    /// IMPORTANT convention note: DataConversion.ToStandard() stores
    /// StandardModel.ObjectiveCoefficients as the NEGATIVE of the true objective
    /// coefficient (the usual "Z row" tableau convention, Z - c^T x = 0). This class
    /// recovers the true coefficient internally as -ObjectiveCoefficients[j].
    ///
    /// Artificial columns (VariableNames starting with "a") are penalized with a
    /// large Big-M cost so they are always the first to leave the basis, which lets
    /// this run in a single pass instead of needing the app's separate Two-Phase step.
    /// </summary>
    internal class RevisedSimplex
    {
        private const double BIG_M = 1_000_000;
        private const double TOL = 1e-9;
        private const int MAX_ITERATIONS = 500;

        #region Public entry point
        public bool Solve(StandardModel standardModel, out RevisedSimplexResult result)
        {
            result = null;
            try
            {
                int m = standardModel.Constraints.Count;
                int n = standardModel.ObjectiveCoefficients.Count;

                // ---- build A, b, and the working (always-maximize) cost vector c ----
                var A = new double[m, n];
                var b = new double[m];
                for (int i = 0; i < m; i++)
                {
                    b[i] = standardModel.Constraints[i].RHS;
                    for (int j = 0; j < n; j++)
                        A[i, j] = standardModel.Constraints[i].Coefficients[j];
                }

                bool isMax = standardModel.ObjectiveType.Trim().ToLower() == "max";
                double sense = isMax ? 1.0 : -1.0;

                var c = new double[n];
                for (int j = 0; j < n; j++)
                {
                    double actualCoeff = -standardModel.ObjectiveCoefficients[j]; // undo the Z-row negation
                    c[j] = sense * actualCoeff; // internally we always maximize
                }
                // Big-M penalty for artificial columns (always penalized, regardless of sense)
                for (int j = 0; j < n; j++)
                {
                    if (standardModel.VariableNames[j].StartsWith("a"))
                        c[j] = -BIG_M;
                }

                // ---- initial basis: for each row, the slack/artificial column that DataConversion
                //      gave a coefficient of 1 in that row and 0 elsewhere (identity by construction) ----
                var basis = new int[m];
                for (int i = 0; i < m; i++)
                {
                    string relation = standardModel.Constraints[i].Relation;
                    string expectedName = (relation == "<=") ? "s" + (i + 1) : "a" + (i + 1);
                    int idx = standardModel.VariableNames.IndexOf(expectedName);
                    if (idx == -1)
                        throw new Exception($"Could not find expected basic column '{expectedName}' for row {i + 1}.");
                    basis[i] = idx;
                }

                var Binv = Identity(m);
                var iterations = new List<RevisedIteration>();

                int it = 0;
                while (true)
                {
                    it++;
                    if (it > MAX_ITERATIONS)
                        throw new Exception("Revised simplex did not converge within the iteration limit.");

                    var rec = new RevisedIteration
                    {
                        IterationNumber = it,
                        BasisVariableNames = basis.Select(bi => standardModel.VariableNames[bi]).ToList()
                    };

                    var cB = basis.Select(bi => c[bi]).ToArray();
                    var xB = MatVec(Binv, b, m);
                    var y = VecMat(cB, Binv, m);              // ---- PRICE OUT ----

                    var reduced = new double[n];
                    for (int j = 0; j < n; j++)
                    {
                        double yAj = 0;
                        for (int i = 0; i < m; i++) yAj += y[i] * A[i, j];
                        reduced[j] = c[j] - yAj;
                    }

                    rec.XB = xB.ToList();
                    rec.Y = y.ToList();
                    rec.ReducedCosts = reduced.ToList();

                    var basisSet = new HashSet<int>(basis);
                    int enter = -1;
                    double best = TOL;
                    for (int j = 0; j < n; j++)
                    {
                        if (basisSet.Contains(j)) continue;
                        if (reduced[j] > best) { best = reduced[j]; enter = j; }
                    }

                    if (enter == -1)
                    {
                        // Optimal - check for a positive artificial still in the basis (infeasible original model)
                        iterations.Add(rec);
                        for (int i = 0; i < m; i++)
                        {
                            if (standardModel.VariableNames[basis[i]].StartsWith("a") && xB[i] > 1e-6)
                            {
                                result = new RevisedSimplexResult
                                {
                                    Status = "Infeasible",
                                    Iterations = iterations,
                                    Model = standardModel
                                };
                                return true;
                            }
                        }

                        var solution = new Dictionary<string, double>();
                        for (int i = 0; i < m; i++)
                            solution[standardModel.VariableNames[basis[i]]] = xB[i];
                        foreach (var name in standardModel.VariableNames)
                            if (!solution.ContainsKey(name)) solution[name] = 0.0;

                        double Zworking = 0;
                        for (int i = 0; i < m; i++) Zworking += cB[i] * xB[i];

                        result = new RevisedSimplexResult
                        {
                            Status = "Optimal",
                            Z = Zworking * sense, // undo the max/min flip
                            Solution = solution,
                            Iterations = iterations,
                            Model = standardModel,
                            FinalBasis = basis.ToList(),
                            FinalBinv = Binv
                        };
                        return true;
                    }

                    rec.EnteringVariable = standardModel.VariableNames[enter];

                    var d = MatVec(Binv, GetColumn(A, enter, m), m);

                    int leave = -1;
                    double bestRatio = double.PositiveInfinity;
                    for (int i = 0; i < m; i++)
                    {
                        if (d[i] > TOL)
                        {
                            double ratio = xB[i] / d[i];
                            if (ratio < bestRatio - TOL)
                            {
                                bestRatio = ratio;
                                leave = i;
                            }
                        }
                    }

                    if (leave == -1)
                    {
                        iterations.Add(rec);
                        result = new RevisedSimplexResult
                        {
                            Status = "Unbounded",
                            Iterations = iterations,
                            Model = standardModel
                        };
                        return true;
                    }

                    rec.LeavingVariable = standardModel.VariableNames[basis[leave]];

                    // ---- build the eta matrix (Product Form) and update B^-1 = Eta * B^-1 ----
                    var eta = Identity(m);
                    for (int i = 0; i < m; i++)
                        eta[i, leave] = (i == leave) ? 1.0 / d[leave] : -d[i] / d[leave];

                    Binv = MatMat(eta, Binv, m);
                    rec.Eta = eta;
                    rec.BinvAfter = (double[,])Binv.Clone();

                    basis[leave] = enter;
                    iterations.Add(rec);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Revised Primal Simplex failed: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Small matrix helpers (no external dependencies)
        private static double[,] Identity(int m)
        {
            var I = new double[m, m];
            for (int i = 0; i < m; i++) I[i, i] = 1;
            return I;
        }
        private static double[] GetColumn(double[,] A, int j, int m)
        {
            var col = new double[m];
            for (int i = 0; i < m; i++) col[i] = A[i, j];
            return col;
        }
        private static double[] MatVec(double[,] M, double[] v, int m)
        {
            var r = new double[m];
            for (int i = 0; i < m; i++)
            {
                double s = 0;
                for (int j = 0; j < m; j++) s += M[i, j] * v[j];
                r[i] = s;
            }
            return r;
        }
        private static double[] VecMat(double[] v, double[,] M, int m)
        {
            var r = new double[m];
            for (int j = 0; j < m; j++)
            {
                double s = 0;
                for (int i = 0; i < m; i++) s += v[i] * M[i, j];
                r[j] = s;
            }
            return r;
        }
        private static double[,] MatMat(double[,] X, double[,] Y, int m)
        {
            var R = new double[m, m];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < m; j++)
                {
                    double s = 0;
                    for (int t = 0; t < m; t++) s += X[i, t] * Y[t, j];
                    R[i, j] = s;
                }
            return R;
        }
        #endregion
    }
}
