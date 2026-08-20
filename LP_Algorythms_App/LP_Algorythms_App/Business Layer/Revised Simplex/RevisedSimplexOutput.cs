using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LP_Algorythms_App.Business_Layer
{
    /// <summary>
    /// Formats a RevisedSimplexResult for display/export. Mirrors DataHandler's
    /// existing style (RawToGrid / StandardToGrid) so it drops in next to it.
    /// Satisfies the "Output file format" requirement: canonical form + all
    /// iterations, all decimals rounded to 3 places.
    /// </summary>
    internal class RevisedSimplexOutput
    {
        #region Build the full report as text (for the output file / a textbox)
        public string ToText(RevisedSimplexResult result)
        {
            var sb = new StringBuilder();
            var sm = result.Model;

            sb.AppendLine("=========================================");
            sb.AppendLine(" REVISED PRIMAL SIMPLEX ALGORITHM");
            sb.AppendLine(" (Product Form of the Inverse + Price Out)");
            sb.AppendLine("=========================================");
            sb.AppendLine();

            // ---- Canonical / standard form ----
            sb.AppendLine($"Objective: {sm.ObjectiveType.ToUpper()}");
            sb.Append("Variables: ");
            sb.AppendLine(string.Join(", ", sm.VariableNames));
            sb.AppendLine("Constraints:");
            for (int i = 0; i < sm.Constraints.Count; i++)
            {
                var con = sm.Constraints[i];
                var terms = new List<string>();
                for (int j = 0; j < con.Coefficients.Count; j++)
                {
                    if (con.Coefficients[j] == 0) continue;
                    terms.Add($"{Round3(con.Coefficients[j])}{sm.VariableNames[j]}");
                }
                sb.AppendLine($"  {i + 1}: {string.Join(" + ", terms)} {con.Relation} {Round3(con.RHS)}");
            }
            sb.AppendLine("  All variables >= 0.");
            sb.AppendLine();

            // ---- Iterations ----
            foreach (var rec in result.Iterations)
            {
                sb.AppendLine($"----- Iteration {rec.IterationNumber} -----");
                sb.AppendLine("Basis: " + string.Join(", ", rec.BasisVariableNames));
                sb.AppendLine("x_B (B^-1 * b): " + FmtList(rec.XB));
                sb.AppendLine("y = c_B * B^-1  (PRICE OUT step): " + FmtList(rec.Y));
                sb.AppendLine("Reduced costs:");
                for (int j = 0; j < rec.ReducedCosts.Count; j++)
                    sb.AppendLine($"    {sm.VariableNames[j],-6} : {Round3(rec.ReducedCosts[j])}");

                if (rec.EnteringVariable == null)
                {
                    sb.AppendLine("No positive reduced cost remains -> OPTIMAL.");
                }
                else
                {
                    sb.AppendLine($"Entering variable: {rec.EnteringVariable}");
                    if (rec.LeavingVariable == null)
                    {
                        sb.AppendLine("No positive entry in the ratio test -> UNBOUNDED.");
                    }
                    else
                    {
                        sb.AppendLine($"Leaving variable: {rec.LeavingVariable}");
                        sb.AppendLine("Eta matrix (Product Form update, B^-1_new = Eta * B^-1_old):");
                        sb.AppendLine(FmtMatrix(rec.Eta));
                        sb.AppendLine("Updated B^-1:");
                        sb.AppendLine(FmtMatrix(rec.BinvAfter));
                    }
                }
                sb.AppendLine();
            }

            // ---- Final result ----
            sb.AppendLine("=========================================");
            if (result.Status == "Optimal")
            {
                sb.AppendLine(" OPTIMAL SOLUTION FOUND");
                sb.AppendLine($" Z = {Round3(result.Z)}");
                foreach (var name in sm.VariableNames.Where(n => !n.StartsWith("s") && !n.StartsWith("e") && !n.StartsWith("a")))
                    sb.AppendLine($"   {name} = {Round3(result.Solution[name])}");
            }
            else if (result.Status == "Unbounded")
            {
                sb.AppendLine(" MODEL IS UNBOUNDED");
            }
            else
            {
                sb.AppendLine(" MODEL IS INFEASIBLE (an artificial variable stayed basic and positive)");
            }
            sb.AppendLine("=========================================");

            return sb.ToString();
        }
        #endregion

        #region Write straight to the output text file
        public void WriteToFile(string path, RevisedSimplexResult result)
        {
            File.WriteAllText(path, ToText(result));
        }
        #endregion

        #region Show the final iteration on the shared dgvTable, same pattern as StandardToGrid
        public void ResultToGrid(DataGridView table, RevisedSimplexResult result)
        {
            table.Columns.Clear();
            table.Rows.Clear();

            var sm = result.Model;
            var lastIter = result.Iterations.Count > 0 ? result.Iterations[result.Iterations.Count - 1] : null;

            table.RowCount = 3;
            table.ColumnCount = sm.VariableNames.Count + 1;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            table.Rows[0].Cells[0].Value = "Variable";
            table.Rows[1].Cells[0].Value = "Value";
            table.Rows[2].Cells[0].Value = "Status";

            for (int j = 0; j < sm.VariableNames.Count; j++)
            {
                table.Rows[0].Cells[j + 1].Value = sm.VariableNames[j];
                if (result.Status == "Optimal" && result.Solution.ContainsKey(sm.VariableNames[j]))
                    table.Rows[1].Cells[j + 1].Value = Round3(result.Solution[sm.VariableNames[j]]);
            }
            table.Rows[2].Cells[1].Value = result.Status + (result.Status == "Optimal" ? $" (Z = {Round3(result.Z)})" : "");
        }
        #endregion

        #region formatting helpers
        private static double Round3(double v) => Math.Round(v, 3, MidpointRounding.AwayFromZero);

        private static string FmtList(List<double> v)
        {
            return "[ " + string.Join(", ", v.Select(x => Round3(x).ToString("0.000"))) + " ]";
        }

        private static string FmtMatrix(double[,] M)
        {
            var sb = new StringBuilder();
            int rows = M.GetLength(0), cols = M.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                sb.Append("    [ ");
                for (int j = 0; j < cols; j++)
                    sb.Append(Round3(M[i, j]).ToString("0.000").PadLeft(9) + " ");
                sb.AppendLine("]");
            }
            return sb.ToString();
        }
        #endregion
    }
}
