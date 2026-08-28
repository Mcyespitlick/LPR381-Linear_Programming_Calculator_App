using System;
using System.Collections.Generic;

namespace LP_Algorythms_App.Business_Layer
{
    // One decision variable of the knapsack problem.
    public class KnapsackItem
    {
        public string Name;
        public int OriginalIndex;
        public double Value;
        public double Weight;

        // Bang-per-buck ratio used for sorting/bounding.
        public double Ratio => Weight > 0 ? Value / Weight : double.MaxValue;
    }

    // One sub-problem generated while branching.
    public class KnapsackNode
    {
        public int NodeId;
        public int ParentId;            // -1 for root
        public string Branch;           // "Root", "Include x3", "Exclude x3"
        public int Level;               // items fixed so far, in sorted order
        public int[] Decisions;         // -1 free, 0 excluded, 1 included (sorted order)
        public double Value;
        public double Weight;
        public double Bound;            // LP-relaxation upper bound
        public bool Feasible;
        public bool IsIntegerSolution;
        public string FathomReason;     // null while node is still being explored
    }

    // Final outcome of a Branch & Bound Knapsack run.
    public class KnapsackResult
    {
        public string Status;           // "Optimal", "Infeasible", "Error"
        public string ErrorMessage;
        public double Capacity;
        public List<KnapsackItem> SortedItems;
        public List<KnapsackNode> Nodes;
        public int BestNodeId = -1;
        public double BestZ;
        public Dictionary<string, int> BestSolution;
    }
}
