using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{

    // Branch & Bound Knapsack Algorithm for a 0/1 single-constraint IP:
    // max c.x  s.t.  a.x <= b,  x binary.
    internal class Knapsack
    {
        private double capacity;
        private List<KnapsackItem> sortedItems;
        private int nodeCounter;
        private double bestValue;
        private int[] bestDecisions;
        private int bestNodeId;
 
        #region Public entry point
        public bool Solve(ParsedModel model, out KnapsackResult result)
        {
            result = new KnapsackResult { Nodes = new List<KnapsackNode>() };
 
            try
            {
                // Validate this is a 0/1 single-constraint knapsack model
                if (model == null || model.ObjectiveCoefficients == null || model.ObjectiveCoefficients.Count == 0)
                {
                    result.Status = "Error";
                    result.ErrorMessage = "No model to solve. Load and parse an input file first.";
                    return false;
                }
 
                if (model.ObjectiveType == null || model.ObjectiveType.ToLower() != "max")
                {
                    result.Status = "Error";
                    result.ErrorMessage = "The Branch & Bound Knapsack algorithm only supports maximisation models.";
                    return false;
                }
 
                if (model.Constraints == null || model.Constraints.Count != 1)
                {
                    result.Status = "Error";
                    result.ErrorMessage = "The Knapsack algorithm expects exactly one constraint (the knapsack capacity).";
                    return false;
                }
 
                if (model.Constraints[0].Relation != "<=")
                {
                    result.Status = "Error";
                    result.ErrorMessage = "The knapsack constraint must be a \"<=\" (capacity) constraint.";
                    return false;
                }
 
                if (model.SignRestrictions == null || model.SignRestrictions.Any(s => s.ToLower() != "bin"))
                {
                    result.Status = "Error";
                    result.ErrorMessage = "Every decision variable must be restricted to \"bin\" for the Knapsack algorithm.";
                    return false;
                }
 
                int n = model.ObjectiveCoefficients.Count;
                capacity = model.Constraints[0].RHS;
 
                if (capacity < 0)
                {
                    result.Status = "Infeasible";
                    result.ErrorMessage = "Negative capacity - no selection can be feasible.";
                    return true;
                }
 
                // Build and sort items by ratio (value / weight), descending
                var items = new List<KnapsackItem>();
                for (int i = 0; i < n; i++)
                {
                    items.Add(new KnapsackItem
                    {
                        Name = "x" + (i + 1),
                        OriginalIndex = i,
                        Value = model.ObjectiveCoefficients[i],
                        Weight = model.Constraints[0].Coefficients[i]
                    });
                }
                sortedItems = items.OrderByDescending(it => it.Ratio).ToList();
 
                nodeCounter = 0;
                bestValue = -1;
                bestDecisions = null;
                bestNodeId = -1;
 
                int[] rootDecisions = new int[n];
                for (int i = 0; i < n; i++) rootDecisions[i] = -1;
 
                Branch(0, rootDecisions, 0, 0, -1, "Root", result.Nodes);
 
                if (bestDecisions == null)
                {
                    result.Status = "Infeasible";
                    return true;
                }
 
                // Map best candidate back into original x1..xn order
                result.BestSolution = new Dictionary<string, int>();
                for (int i = 0; i < n; i++)
                {
                    int sortedPos = sortedItems.FindIndex(it => it.OriginalIndex == i);
                    result.BestSolution["x" + (i + 1)] = bestDecisions[sortedPos];
                }
 
                result.Status = "Optimal";
                result.Capacity = capacity;
                result.SortedItems = sortedItems;
                result.BestZ = bestValue;
                result.BestNodeId = bestNodeId;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Knapsack Branch & Bound failed: " + ex.Message);
                result.Status = "Error";
                result.ErrorMessage = ex.Message;
                return false;
            }
        }
        #endregion
 
        #region Recursive branch-and-bound with backtracking
        // Explores one sub-problem. Both children of an internal node are always
        // created; returning from the call is the backtracking step to the parent.
        private void Branch(int level, int[] decisions, double curValue, double curWeight, int parentId, string branchLabel, List<KnapsackNode> nodeLog)
        {
            int myId = nodeCounter++;
            var node = new KnapsackNode
            {
                NodeId = myId,
                ParentId = parentId,
                Branch = branchLabel,
                Level = level,
                Decisions = (int[])decisions.Clone(),
                Value = curValue,
                Weight = curWeight
            };
            nodeLog.Add(node);
 
            // Fathom: infeasible
            if (curWeight > capacity + 1e-9)
            {
                node.Feasible = false;
                node.FathomReason = "Fathomed - Infeasible (capacity exceeded)";
                return;
            }
            node.Feasible = true;
 
            // Fathom: bounded
            node.Bound = ComputeBound(level, curValue, curWeight);
            if (node.Bound <= bestValue + 1e-9)
            {
                node.FathomReason = $"Fathomed - Bounded (bound {Round3(node.Bound)} cannot beat best {Round3(bestValue)})";
                return;
            }
 
            // Fathom: candidate (complete solution)
            if (level == sortedItems.Count)
            {
                node.IsIntegerSolution = true;
                if (curValue > bestValue + 1e-9)
                {
                    bestValue = curValue;
                    bestDecisions = (int[])decisions.Clone();
                    bestNodeId = myId;
                    node.FathomReason = "Fathomed - Candidate solution (NEW BEST)";
                }
                else
                {
                    node.FathomReason = "Fathomed - Candidate solution (not better than current best)";
                }
                return;
            }
 
            // Branch further: create both sub-problems
            node.FathomReason = null;
            var next = sortedItems[level];
 
            decisions[level] = 1;
            Branch(level + 1, decisions, curValue + next.Value, curWeight + next.Weight, myId, $"Include {next.Name}", nodeLog);
            decisions[level] = -1;
 
            decisions[level] = 0;
            Branch(level + 1, decisions, curValue, curWeight, myId, $"Exclude {next.Name}", nodeLog);
            decisions[level] = -1;
        }
 
        // Upper bound = value locked in + greedy fractional fill of remaining capacity.
        private double ComputeBound(int level, double curValue, double curWeight)
        {
            double value = curValue;
            double weight = curWeight;
            int j = level;
 
            while (j < sortedItems.Count && weight + sortedItems[j].Weight <= capacity)
            {
                weight += sortedItems[j].Weight;
                value += sortedItems[j].Value;
                j++;
            }
 
            if (j < sortedItems.Count && sortedItems[j].Weight > 0)
            {
                double remaining = capacity - weight;
                value += remaining * sortedItems[j].Ratio;
            }
 
            return value;
        }
        #endregion
 
        private static double Round3(double v) => Math.Round(v, 3, MidpointRounding.AwayFromZero);
    }
}
