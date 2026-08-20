using System;
using System.Collections.Generic;

namespace LP_Algorythms_App.Business_Layer
{
    /// <summary>
    /// One iteration of the Revised Primal Simplex Algorithm.
    /// Kept separate per iteration so the whole run can be displayed
    /// (grid, textbox, or output file) exactly as the assignment brief
    /// requires: "Display all Product Form and Price Out iterations."
    /// </summary>
    public class RevisedIteration
    {
        public int IterationNumber;
        public List<string> BasisVariableNames;   // which variable is basic in each row
        public List<double> XB;                   // current basic solution, B^-1 * b
        public List<double> Y;                     // simplex multipliers: y = c_B * B^-1   (the "Price Out" step)
        public List<double> ReducedCosts;           // c_j - y*A_j for every column j
        public string EnteringVariable;             // null if this iteration found the optimum
        public string LeavingVariable;               // null if unbounded or optimal
        public double[,] Eta;                        // the eta matrix used to update B^-1 this iteration (Product Form)
        public double[,] BinvAfter;                   // B^-1 after this iteration's pivot
    }

    /// <summary>Final outcome of a Revised Primal Simplex run.</summary>
    public class RevisedSimplexResult
    {
        public string Status;                       // "Optimal", "Unbounded", or "Infeasible"
        public double Z;
        public Dictionary<string, double> Solution;  // variable name -> value, decision variables only
        public List<RevisedIteration> Iterations = new List<RevisedIteration>();
        public StandardModel Model;                  // the standard-form model that was solved (for reference/reuse in Sensitivity Analysis)
        public List<int> FinalBasis;                 // column index basic in each row, at termination
        public double[,] FinalBinv;                   // B^-1 at termination - needed by Sensitivity Analysis / Duality
    }
}
