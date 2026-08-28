using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LP_Algorythms_App.Business_Layer
{
    // Formats a KnapsackResult for display/export.
    internal class KnapsackOutput
    {
        public string ToText(KnapsackResult result)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=========================================");
            sb.AppendLine(" BRANCH & BOUND KNAPSACK ALGORITHM");
            sb.AppendLine("=========================================");
            sb.AppendLine();

            if (result.Status == "Error")
            {
                sb.AppendLine("ERROR: " + result.ErrorMessage);
                return sb.ToString();
            }

            sb.AppendLine("Model:");
            sb.AppendLine($"  Capacity (RHS): {Round3(result.Capacity)}");
            sb.AppendLine("  Items sorted by value/weight ratio (used for branching & bounding):");
            foreach (var it in result.SortedItems)
            {
                sb.AppendLine($"    {it.Name,-6} value = {Round3(it.Value),-10} weight = {Round3(it.Weight),-10} ratio = {Round3(it.Ratio)}");
            }
            sb.AppendLine();

            if (result.Status == "Infeasible")
            {
                sb.AppendLine("MODEL IS INFEASIBLE" + (string.IsNullOrEmpty(result.ErrorMessage) ? "" : (": " + result.ErrorMessage)));
                return sb.ToString();
            }

            sb.AppendLine("Sub-problems explored (depth-first, with backtracking):");
            sb.AppendLine();
            foreach (var node in result.Nodes)
            {
                sb.AppendLine($"----- Node {node.NodeId}  (Parent: {(node.ParentId == -1 ? "-" : node.ParentId.ToString())} | Branch: {node.Branch}) -----");
                sb.AppendLine("  Fixed variables: " + FormatDecisions(node.Decisions, result.SortedItems));
                sb.AppendLine($"  Value so far: {Round3(node.Value)}    Weight so far: {Round3(node.Weight)} / {Round3(result.Capacity)}");

                if (node.Feasible)
                    sb.AppendLine($"  Bound (best possible from here): {Round3(node.Bound)}");

                if (node.NodeId == result.BestNodeId)
                    sb.AppendLine("  *** BEST CANDIDATE ***");

                sb.AppendLine("  " + (node.FathomReason ?? "Branched further (both sub-problems below were created from this node)"));
                sb.AppendLine();
            }

            sb.AppendLine("=========================================");
            sb.AppendLine(" BEST CANDIDATE FOUND");
            sb.AppendLine($" Z = {Round3(result.BestZ)}");
            foreach (var kv in result.BestSolution.OrderBy(kv => kv.Key))
                sb.AppendLine($"   {kv.Key} = {kv.Value}");
            sb.AppendLine("=========================================");

            return sb.ToString();
        }

        public void WriteToFile(string path, KnapsackResult result)
        {
            File.WriteAllText(path, ToText(result));
        }

        public void ResultToGrid(DataGridView table, KnapsackResult result)
        {
            table.Columns.Clear();
            table.Rows.Clear();

            if (result.Status != "Optimal")
            {
                table.RowCount = 1;
                table.ColumnCount = 1;
                table.Rows[0].Cells[0].Value = result.Status + ": " + result.ErrorMessage;
                return;
            }

            var names = result.BestSolution.Keys.OrderBy(k => k).ToList();

            table.RowCount = 2;
            table.ColumnCount = names.Count + 1;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            table.Rows[0].Cells[0].Value = "Variable";
            table.Rows[1].Cells[0].Value = "Value";

            for (int j = 0; j < names.Count; j++)
            {
                table.Rows[0].Cells[j + 1].Value = names[j];
                table.Rows[1].Cells[j + 1].Value = result.BestSolution[names[j]];
            }
        }

        private static double Round3(double v) => Math.Round(v, 3, MidpointRounding.AwayFromZero);

        // Prints each item's fixed value in original (x1, x2, ...) order.
        private static string FormatDecisions(int[] decisionsInSortedOrder, List<KnapsackItem> sortedItems)
        {
            var byOriginal = new SortedDictionary<int, string>();
            for (int i = 0; i < sortedItems.Count; i++)
            {
                string val = decisionsInSortedOrder[i] == -1 ? "-" : decisionsInSortedOrder[i].ToString();
                byOriginal[sortedItems[i].OriginalIndex] = $"{sortedItems[i].Name}={val}";
            }
            return string.Join(", ", byOriginal.Values);
        }
    }
}
