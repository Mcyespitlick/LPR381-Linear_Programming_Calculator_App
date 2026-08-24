using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    internal class Branch_Bound
    {
        public class Node
        {
            public int NodeId;
            public int ParentId;
            public int Depth;

            public StandardModel Tableau;

            public double Bound;
            public double ObjectiveValue;

            public double[] Solution;

            public bool IsInteger;
            public bool IsPruned;
            public bool IsInfeasible;

            public string BranchDescription;
            public string PruneReason;
        }

        public class Result
        {
            public bool Optimal;
            public double Z;
            public double[] X;

            public int TotalNodes;
            public int PrunedNodes;

            public List<Node> Nodes;

            public Node BestNode;

            public string Status;
        }

        private readonly Reused_Algorythms algorithms =
            new Reused_Algorythms();

        private const double TOLERANCE = 1e-8;

        // ============================================================
        // MAIN BRANCH AND BOUND METHOD
        // ============================================================

        public bool Solve(
            StandardModel initialModel,
            ResolvedModel lpRelaxation,
            out Result result)
        {
            result = new Result
            {
                Nodes = new List<Node>(),
                X = new double[GetIntegerVariableCount(initialModel)],
                Z = initialModel.ObjectiveType.ToLower() == "max"
                    ? double.NegativeInfinity
                    : double.PositiveInfinity,
                Status = "Not solved"
            };

            if (initialModel == null)
            {
                result.Status = "No model";
                return false;
            }

            if (lpRelaxation == null ||
                lpRelaxation.tablues == null ||
                lpRelaxation.tablues.Count == 0)
            {
                result.Status = "No LP relaxation";
                return false;
            }

            // --------------------------------------------------------
            // Get the optimal LP relaxation tableau
            // --------------------------------------------------------

            StandardModel rootTableau =
                CloneModel(lpRelaxation.tablues.Last());

            // --------------------------------------------------------
            // Create root node
            // --------------------------------------------------------

            Node root = new Node
            {
                NodeId = 1,
                ParentId = -1,
                Depth = 0,
                Tableau = rootTableau,
                Bound = rootTableau.ObjectiveFunctionRHS,
                ObjectiveValue = rootTableau.ObjectiveFunctionRHS,
                BranchDescription = "Root",
                IsPruned = false,
                IsInfeasible = false
            };

            result.Nodes.Add(root);

            List<Node> openNodes = new List<Node>();
            openNodes.Add(root);

            int nextNodeId = 2;

            // --------------------------------------------------------
            // Branch & Bound loop
            // --------------------------------------------------------

            while (openNodes.Count > 0)
            {
                // Best-bound selection
                Node current;

                if (initialModel.ObjectiveType.ToLower() == "max")
                {
                    current = openNodes
                        .OrderByDescending(n => n.Bound)
                        .ThenBy(n => n.Depth)
                        .First();
                }
                else
                {
                    current = openNodes
                        .OrderBy(n => n.Bound)
                        .ThenBy(n => n.Depth)
                        .First();
                }

                openNodes.Remove(current);

                // ----------------------------------------------------
                // Check if node should be pruned
                // ----------------------------------------------------

                if (ShouldPrune(current, result))
                {
                    current.IsPruned = true;
                    current.PruneReason = "Bound";
                    result.PrunedNodes++;
                    continue;
                }

                // ----------------------------------------------------
                // Get current solution
                // ----------------------------------------------------

                current.Solution =
                    ExtractDecisionVariableValues(current.Tableau);

                current.ObjectiveValue =
                    current.Tableau.ObjectiveFunctionRHS;

                current.Bound =
                    current.ObjectiveValue;

                // ----------------------------------------------------
                // Check integer solution
                // ----------------------------------------------------

                if (IsIntegerSolution(
                    current.Tableau,
                    current.Solution))
                {
                    current.IsInteger = true;

                    bool better;

                    if (initialModel.ObjectiveType.ToLower() == "max")
                    {
                        better =
                            current.ObjectiveValue >
                            result.Z + TOLERANCE;
                    }
                    else
                    {
                        better =
                            current.ObjectiveValue <
                            result.Z - TOLERANCE;
                    }

                    if (better)
                    {
                        result.Z = current.ObjectiveValue;

                        result.X =
                            (double[])current.Solution.Clone();

                        result.BestNode = current;
                    }

                    continue;
                }

                // ----------------------------------------------------
                // Find fractional variable
                // ----------------------------------------------------

                int branchVariable =
                    FindBranchVariable(
                        current.Tableau,
                        current.Solution);

                if (branchVariable == -1)
                {
                    current.IsPruned = true;
                    current.PruneReason =
                        "No fractional integer variable";

                    result.PrunedNodes++;
                    continue;
                }

                double value =
                    current.Solution[branchVariable];

                double floorValue =
                    Math.Floor(value);

                double ceilValue =
                    Math.Ceiling(value);

                // ----------------------------------------------------
                // LEFT CHILD
                // x <= floor(x)
                // ----------------------------------------------------

                StandardModel leftTableau =
                    AddBranchConstraint(
                        current.Tableau,
                        branchVariable,
                        floorValue,
                        false);

                if (leftTableau != null)
                {
                    Node leftNode = SolveChildNode(
                        leftTableau,
                        current,
                        nextNodeId++,
                        $"x{branchVariable + 1} <= {floorValue}");

                    result.Nodes.Add(leftNode);

                    if (!leftNode.IsInfeasible)
                    {
                        openNodes.Add(leftNode);
                    }
                    else
                    {
                        result.PrunedNodes++;
                    }
                }

                // ----------------------------------------------------
                // RIGHT CHILD
                // x >= ceil(x)
                // ----------------------------------------------------

                StandardModel rightTableau =
                    AddBranchConstraint(
                        current.Tableau,
                        branchVariable,
                        ceilValue,
                        true);

                if (rightTableau != null)
                {
                    Node rightNode = SolveChildNode(
                        rightTableau,
                        current,
                        nextNodeId++,
                        $"x{branchVariable + 1} >= {ceilValue}");

                    result.Nodes.Add(rightNode);

                    if (!rightNode.IsInfeasible)
                    {
                        openNodes.Add(rightNode);
                    }
                    else
                    {
                        result.PrunedNodes++;
                    }
                }
            }

            // --------------------------------------------------------
            // Final result
            // --------------------------------------------------------

            if (result.BestNode != null)
            {
                result.Optimal = true;
                result.Status = "Optimal";
            }
            else
            {
                result.Optimal = false;
                result.Status = "Infeasible";
            }

            return true;
        }

        // ============================================================
        // SOLVE CHILD NODE USING DUAL SIMPLEX
        // ============================================================

        private Node SolveChildNode(
            StandardModel childTableau,
            Node parent,
            int nodeId,
            string description)
        {
            Node node = new Node
            {
                NodeId = nodeId,
                ParentId = parent.NodeId,
                Depth = parent.Depth + 1,
                Tableau = childTableau,
                BranchDescription = description,
                IsPruned = false,
                IsInfeasible = false
            };

            ResolvedModel startingModel =
                new ResolvedModel();

            startingModel.tablues =
                new List<StandardModel>();

            startingModel.tablues.Add(childTableau);

            ResolvedModel dualResult;

            bool success =
                algorithms.DualSimplex(
                    startingModel,
                    out dualResult);

            if (!success ||
                dualResult == null ||
                dualResult.tablues == null ||
                dualResult.tablues.Count == 0)
            {
                node.IsInfeasible = true;
                node.PruneReason = "Dual Simplex failed";
                return node;
            }

            string status =
                dualResult.EndResult == null
                    ? ""
                    : dualResult.EndResult.ToLower();

            if (status.Contains("infeasible") ||
                status.Contains("error"))
            {
                node.IsInfeasible = true;
                node.PruneReason = "Infeasible";
                return node;
            }

            // Use final Dual Simplex tableau
            node.Tableau =
                CloneModel(
                    dualResult.tablues.Last());

            node.ObjectiveValue =
                node.Tableau.ObjectiveFunctionRHS;

            node.Bound =
                node.ObjectiveValue;

            node.Solution =
                ExtractDecisionVariableValues(
                    node.Tableau);

            return node;
        }

        // ============================================================
        // ADD BRANCH CONSTRAINT
        // ============================================================

        private StandardModel AddBranchConstraint(
            StandardModel parent,
            int variableIndex,
            double value,
            bool greaterThanOrEqual)
        {
            try
            {
                StandardModel child =
                    CloneModel(parent);

                int oldColumnCount =
                    child.ObjectiveCoefficients.Count;

                // ----------------------------------------------------
                // Add one new slack column
                // ----------------------------------------------------

                foreach (Constraint constraint
                    in child.Constraints)
                {
                    constraint.Coefficients.Add(0);
                }

                child.ObjectiveCoefficients.Add(0);

                child.SignRestrictions.Add("none");

                string slackName =
                    GetNextSlackName(child.VariableNames);

                child.VariableNames.Add(slackName);

                // ----------------------------------------------------
                // Create new branch constraint
                // ----------------------------------------------------

                Constraint branchConstraint =
                    new Constraint();

                branchConstraint.Coefficients =
                    new List<double>(
                        new double[oldColumnCount + 1]);

                if (!greaterThanOrEqual)
                {
                    // x <= floor(x)
                    //
                    // x + s = floor(x)

                    branchConstraint.Coefficients
                        [variableIndex] = 1;

                    branchConstraint.Coefficients
                        [oldColumnCount] = 1;

                    branchConstraint.RHS = value;
                    branchConstraint.Relation = "<=";
                }
                else
                {
                    // x >= ceil(x)
                    //
                    // -x + s = -ceil(x)

                    branchConstraint.Coefficients
                        [variableIndex] = -1;

                    branchConstraint.Coefficients
                        [oldColumnCount] = 1;

                    branchConstraint.RHS = -value;
                    branchConstraint.Relation = ">=";
                }

                child.Constraints.Add(
                    branchConstraint);

                return child;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Failed to create branch: "
                    + ex.Message);

                return null;
            }
        }

        // ============================================================
        // CHECK INTEGER SOLUTION
        // ============================================================

        private bool IsIntegerSolution(
            StandardModel tableau,
            double[] solution)
        {
            if (solution == null)
                return false;

            for (int i = 0;
                 i < tableau.SignRestrictions.Count &&
                 i < solution.Length;
                 i++)
            {
                string restriction =
                    tableau.SignRestrictions[i];

                if (restriction == null)
                    continue;

                restriction =
                    restriction.ToLower();

                if (restriction == "int" ||
                    restriction == "bin")
                {
                    if (Math.Abs(
                        solution[i] -
                        Math.Round(solution[i])) > TOLERANCE)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // ============================================================
        // FIND FRACTIONAL INTEGER VARIABLE
        // ============================================================

        private int FindBranchVariable(
            StandardModel tableau,
            double[] solution)
        {
            int bestIndex = -1;
            double largestFraction = 0;

            for (int i = 0;
                 i < tableau.SignRestrictions.Count &&
                 i < solution.Length;
                 i++)
            {
                string restriction =
                    tableau.SignRestrictions[i];

                if (restriction == null)
                    continue;

                restriction =
                    restriction.ToLower();

                if (restriction != "int" &&
                    restriction != "bin")
                {
                    continue;
                }

                double value = solution[i];

                double fraction =
                    Math.Abs(
                        value -
                        Math.Round(value));

                if (fraction > TOLERANCE &&
                    fraction > largestFraction)
                {
                    largestFraction = fraction;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        // ============================================================
        // EXTRACT x1, x2, x3... VALUES FROM TABLEAU
        // ============================================================

        private double[] ExtractDecisionVariableValues(
            StandardModel tableau)
        {
            int numberOfDecisionVariables =
                0;

            foreach (string name
                in tableau.VariableNames)
            {
                if (name.StartsWith("x"))
                {
                    int number;

                    string numberText =
                        new string(
                            name
                                .Skip(1)
                                .TakeWhile(char.IsDigit)
                                .ToArray());

                    if (int.TryParse(
                        numberText,
                        out number))
                    {
                        numberOfDecisionVariables =
                            Math.Max(
                                numberOfDecisionVariables,
                                number);
                    }
                }
            }

            double[] solution =
                new double[numberOfDecisionVariables];

            for (int i = 0;
                 i < tableau.VariableNames.Count;
                 i++)
            {
                string name =
                    tableau.VariableNames[i];

                if (!name.StartsWith("x"))
                    continue;

                int variableNumber;

                string numberText =
                    new string(
                        name
                            .Skip(1)
                            .TakeWhile(char.IsDigit)
                            .ToArray());

                if (!int.TryParse(
                    numberText,
                    out variableNumber))
                    continue;

                // Only use the original x1, x2, x3...
                // columns, not x1' generated for URS.
                if (name.Contains("'"))
                    continue;

                int row =
                    FindBasicRow(
                        tableau,
                        i);

                if (row >= 0)
                {
                    solution[variableNumber - 1] =
                        tableau
                            .Constraints[row]
                            .RHS;
                }
                else
                {
                    solution[variableNumber - 1] = 0;
                }
            }

            return solution;
        }

        // ============================================================
        // FIND BASIC VARIABLE
        // ============================================================

        private int FindBasicRow(
            StandardModel tableau,
            int column)
        {
            int basicRow = -1;

            for (int i = 0;
                 i < tableau.Constraints.Count;
                 i++)
            {
                double value =
                    tableau.Constraints[i]
                        .Coefficients[column];

                if (Math.Abs(value - 1) < TOLERANCE)
                {
                    if (basicRow != -1)
                        return -1;

                    basicRow = i;
                }
                else if (Math.Abs(value) > TOLERANCE)
                {
                    return -1;
                }
            }

            return basicRow;
        }

        // ============================================================
        // BOUND PRUNING
        // ============================================================

        private bool ShouldPrune(
            Node node,
            Result result)
        {
            if (result.BestNode == null)
                return false;

            bool isMax =
                node.Tableau.ObjectiveType
                    .ToLower() == "max";

            if (isMax)
            {
                return node.Bound <=
                    result.Z + TOLERANCE;
            }
            else
            {
                return node.Bound >=
                    result.Z - TOLERANCE;
            }
        }

        // ============================================================
        // CLONE STANDARD MODEL
        // ============================================================

        private StandardModel CloneModel(
            StandardModel source)
        {
            StandardModel copy =
                new StandardModel();

            copy.ObjectiveType =
                source.ObjectiveType;

            copy.ObjectiveFunctionRHS =
                source.ObjectiveFunctionRHS;

            copy.ObjectiveCoefficients =
                new List<double>(
                    source.ObjectiveCoefficients);

            copy.VariableNames =
                new List<string>(
                    source.VariableNames);

            copy.SignRestrictions =
                new List<string>(
                    source.SignRestrictions);

            copy.Constraints =
                new List<Constraint>();

            foreach (Constraint c
                in source.Constraints)
            {
                Constraint newConstraint =
                    new Constraint();

                newConstraint.Coefficients =
                    new List<double>(
                        c.Coefficients);

                newConstraint.Relation =
                    c.Relation;

                newConstraint.RHS =
                    c.RHS;

                copy.Constraints.Add(
                    newConstraint);
            }

            if (source.TwoPhaseObjective != null)
            {
                copy.TwoPhaseObjective =
                    new Constraint();

                copy.TwoPhaseObjective.Coefficients =
                    new List<double>(
                        source.TwoPhaseObjective
                            .Coefficients);

                copy.TwoPhaseObjective.Relation =
                    source.TwoPhaseObjective.Relation;

                copy.TwoPhaseObjective.RHS =
                    source.TwoPhaseObjective.RHS;
            }

            return copy;
        }

        // ============================================================
        // NEXT SLACK NAME
        // ============================================================

        private string GetNextSlackName(
            List<string> variableNames)
        {
            int highest = 0;

            foreach (string name
                in variableNames)
            {
                if (!name.StartsWith("s"))
                    continue;

                int number;

                if (int.TryParse(
                    new string(
                        name.Skip(1)
                            .TakeWhile(char.IsDigit)
                            .ToArray()),
                    out number))
                {
                    highest =
                        Math.Max(highest, number);
                }
            }

            return "sBB" + (highest + 1);
        }

        private int GetIntegerVariableCount(
            StandardModel model)
        {
            int count = 0;

            foreach (string restriction
                in model.SignRestrictions)
            {
                if (restriction == "int" ||
                    restriction == "bin")
                {
                    count++;
                }
            }

            return count;
        }
    }

}
