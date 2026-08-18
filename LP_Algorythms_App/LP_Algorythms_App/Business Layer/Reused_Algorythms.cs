using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    internal class Reused_Algorythms
    {
        #region Algorythm to do the Primal Simplex
        public bool PrimalSimplex(StandardModel initialTable, ResolvedModel TwoPhaseResult, out ResolvedModel PrimalResult)
        {
            PrimalResult = new ResolvedModel();
            PrimalResult.tablues = new List<StandardModel>();

            try
            {
                if (TwoPhaseResult != null)
                {
                    int TablueCount = TwoPhaseResult.tablues.Count - 1;
                    StandardModel DroppedAColumns = new StandardModel();
                    DroppedAColumns.VariableNames = new List<string>();
                    DroppedAColumns.ObjectiveCoefficients = new List<double>();
                    DroppedAColumns.Constraints = new List<Constraint>();
                    DroppedAColumns.SignRestrictions = new List<string>();
                    DroppedAColumns.ObjectiveType = TwoPhaseResult.tablues[TablueCount].ObjectiveType;
                    DroppedAColumns.ObjectiveFunctionRHS = TwoPhaseResult.tablues[TablueCount].ObjectiveFunctionRHS;



                    for (int i = 0; i < TwoPhaseResult.tablues[TablueCount].Constraints.Count; i++) //for each row
                    {

                        DroppedAColumns.Constraints.Add(new Constraint { Coefficients = new List<double>() });
                        DroppedAColumns.Constraints[i].Relation = TwoPhaseResult.tablues[TablueCount].Constraints[i].Relation;
                        DroppedAColumns.Constraints[i].RHS = TwoPhaseResult.tablues[TablueCount].Constraints[i].RHS;

                        for (int j = 0; j < TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients.Count; j++)//foreach Column
                        {
                            if (!TwoPhaseResult.tablues[TablueCount].VariableNames[j].StartsWith("a")) 
                            {
                                DroppedAColumns.Constraints[i].Coefficients.Add(TwoPhaseResult.tablues[TablueCount].Constraints[i].Coefficients[j]);
                            }

                        }

                    }

                    for (int j = 0; j < TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients.Count; j++)//foreach Column
                    {
                        if (!TwoPhaseResult.tablues[TablueCount].VariableNames[j].StartsWith("a"))
                        {
                            DroppedAColumns.ObjectiveCoefficients.Add(TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients[j]);
                            DroppedAColumns.SignRestrictions.Add(TwoPhaseResult.tablues[TablueCount].SignRestrictions[j]);
                        }
                    }

                    PrimalResult.tablues.Add(DroppedAColumns);
                }
                else
                {
                    PrimalResult.tablues.Add(initialTable);
                }



                bool optimal = false;
                int itterationNumber = 0;

                if (PrimalResult.tablues[0].ObjectiveType.ToLower() == "min")
                {
                    double OptimalityCheck = PrimalResult.tablues[0].ObjectiveCoefficients.Max();
                    if (OptimalityCheck <= 0) //if the largest number is 0, and everything else is smaller, than its optimal for being a min problem
                    {
                        optimal = true;
                    }
                }
                else
                {
                    double OptimalityCheck = PrimalResult.tablues[0].ObjectiveCoefficients.Min();
                    if (OptimalityCheck >= 0) //if the smallest number is 0, and everything else is larger, than its optimal for being a max problem
                    {
                        optimal = true;
                    }
                }



                while (optimal == false)
                {

                    StandardModel itteration = new StandardModel();


                    var coefficients = PrimalResult.tablues[itterationNumber].ObjectiveCoefficients;

                    var constraints = PrimalResult.tablues[itterationNumber].Constraints;

                    //Pivot column is initialised outside the if/else so when its called later it doesnt complain.
                    int PivotColumn = -1;
                    if (PrimalResult.tablues[0].ObjectiveType.ToLower() == "min")
                    {
                        double max = coefficients.Max();
                        PivotColumn = coefficients.IndexOf(max);
                    }
                    else
                    {
                        double min = coefficients.Min();
                        PivotColumn = coefficients.IndexOf(min);
                    }

                    List<double> ratios = new List<double>();
                    for (int i = 0; i < PrimalResult.tablues[itterationNumber].Constraints.Count; i++)
                    {
                        ratios.Add((PrimalResult.tablues[itterationNumber].Constraints[i].RHS) / (PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]));
                    }


                    double SmallestRatio = Double.MaxValue;
                    int PivotRow = -1;
                    for (int i = 0; i < ratios.Count; i++)
                    {
                        if (constraints[i].RHS > 0)
                        {
                            if (ratios[i] > 0)
                            {
                                if (ratios[i] < SmallestRatio)
                                {
                                    SmallestRatio = ratios[i];
                                    PivotRow = i;
                                }
                            }
                        }
                    }

                    if (PivotRow == -1)
                    {
                        PrimalResult.EndResult = "unbounded";
                        break;
                    }

                    itteration.ObjectiveCoefficients = new List<double>();
                    itteration.Constraints = new List<Constraint>();

                    //creates enough Constraints so that it can simply be filled later
                    for (int i = 0; i < PrimalResult.tablues[itterationNumber].Constraints.Count; i++)
                    {
                        itteration.Constraints.Add(new Constraint { Coefficients = new List<double>() });
                    }

                    //Calculates new values for the Objective function (Z row)

                    itteration.ObjectiveCoefficients = new List<Double>();
                    for (int i = 0; i < PrimalResult.tablues[itterationNumber].ObjectiveCoefficients.Count; i++)
                    {
                        itteration.ObjectiveCoefficients.Add
                            ((PrimalResult.tablues[itterationNumber].ObjectiveCoefficients[i])
                            -
                            (PrimalResult.tablues[itterationNumber].ObjectiveCoefficients[PivotColumn]
                            *
                            ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[i])
                            /
                            (PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));
                    }
                    itteration.ObjectiveFunctionRHS =
                        ((PrimalResult.tablues[itterationNumber].ObjectiveFunctionRHS)
                        -
                        (PrimalResult.tablues[itterationNumber].ObjectiveCoefficients[PivotColumn]
                        *
                        ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].RHS)
                        /
                        (PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));

                    //Pivots constraints
                    for (int i = 0; i < PrimalResult.tablues[itterationNumber].Constraints.Count; i++)
                    {
                        if (i != PivotRow)
                        {
                            for (int j = 0; j < PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add
                                    (
                                    (PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[j])
                                    -
                                    (PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]
                                    *
                                    ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[j])
                                    /
                                    ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))
                                    )));


                            }
                            //Pivots RHS
                            itteration.Constraints[i].RHS =
                                ((PrimalResult.tablues[itterationNumber].Constraints[i].RHS)
                                -
                                (PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn])
                                *
                                ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].RHS)
                                /
                                ((PrimalResult.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn])
                                )));

                        }
                        else
                        {
                            for (int j = 0; j < PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add
                                    (
                                    PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[j]
                                    /
                                    PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]
                                    );
                            }
                            //Pivots RHS
                            itteration.Constraints[i].RHS =
                                (
                                (PrimalResult.tablues[itterationNumber].Constraints[i].RHS)
                                /
                                (PrimalResult.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn])
                                );

                        }
                    }
                    itteration.SignRestrictions = initialTable.SignRestrictions;
                    itteration.ObjectiveType = initialTable.ObjectiveType;
                    itteration.VariableNames = initialTable.VariableNames;
                    itteration.VariableNames.RemoveAll(name => name.StartsWith("a"));



                    if (PrimalResult.tablues[0].ObjectiveType.ToLower() == "min")
                    {
                        double OptimalityCheck = itteration.ObjectiveCoefficients.Max();
                        if (OptimalityCheck <= 0) //if the largest number is 0, and everything else is smaller, than its optimal for being a min problem
                        {
                            optimal = true;
                        }
                    }
                    else
                    {
                        double OptimalityCheck = itteration.ObjectiveCoefficients.Min();
                        if (OptimalityCheck >= 0) //if the smallest number is 0, and everything else is larger, than its optimal for being a max problem
                        {
                            optimal = true;
                        }
                    }



                    PrimalResult.tablues.Add(itteration);
                    itterationNumber++;


                    if (itterationNumber >= 10000)
                    {
                        PrimalResult.EndResult = "Exceeded 10000 itterations";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Two-phase simplex failed");
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
        #endregion

        /*  first attempt at dual simplex
#region Dual simplex first attempt
public bool DualSimplex(StandardModel initialTable, ResolvedModel TwoPhaseResult, out ResolvedModel DualResult)
{
    DualResult = new ResolvedModel();
    DualResult.tablues = new List<StandardModel>();

    try
    {
        if (TwoPhaseResult != null)
        {
            int TablueCount = TwoPhaseResult.tablues.Count - 1;
            StandardModel DroppedAColumns = new StandardModel();
            DroppedAColumns.VariableNames = new List<string>();
            DroppedAColumns.ObjectiveCoefficients = new List<double>();
            DroppedAColumns.Constraints = new List<Constraint>();
            DroppedAColumns.SignRestrictions = new List<string>();
            DroppedAColumns.ObjectiveType = TwoPhaseResult.tablues[TablueCount].ObjectiveType;
            DroppedAColumns.ObjectiveFunctionRHS = TwoPhaseResult.tablues[TablueCount].ObjectiveFunctionRHS;


            for (int i = 0; i < TwoPhaseResult.tablues[TablueCount].Constraints.Count; i++) //for each row
            {

                DroppedAColumns.Constraints.Add(new Constraint { Coefficients = new List<double>() });
                DroppedAColumns.Constraints[i].Relation = TwoPhaseResult.tablues[TablueCount].Constraints[i].Relation;
                DroppedAColumns.Constraints[i].RHS = TwoPhaseResult.tablues[TablueCount].Constraints[i].RHS;

                for (int j = 0; j < TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients.Count; j++)//foreach Column
                {
                    if (!TwoPhaseResult.tablues[TablueCount].VariableNames[j].StartsWith("a"))
                    {
                        DroppedAColumns.Constraints[i].Coefficients.Add(TwoPhaseResult.tablues[TablueCount].Constraints[i].Coefficients[j]);
                    }

                }

            }

            for (int j = 0; j < TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients.Count; j++)//foreach Column
            {
                if (!TwoPhaseResult.tablues[TablueCount].VariableNames[j].StartsWith("a"))
                {
                    DroppedAColumns.ObjectiveCoefficients.Add(TwoPhaseResult.tablues[TablueCount].ObjectiveCoefficients[j]);
                    DroppedAColumns.SignRestrictions.Add(TwoPhaseResult.tablues[TablueCount].SignRestrictions[j]);
                }
            }

            DualResult.tablues.Add(DroppedAColumns);
        }
        else
        {
            DualResult.tablues.Add(initialTable);
        }


        int itterationNumber = 0;

        while (true)
        {
            StandardModel current = DualResult.tablues[itterationNumber];

            // check if all RHS are non-negative -> done
            bool allNonNegative = true;
            for (int i = 0; i < current.Constraints.Count; i++)
            {
                if (current.Constraints[i].RHS < 0)
                {
                    allNonNegative = false;
                    break;
                }
            }

            if (allNonNegative)
            {
                // check optimality of objective row similar to primal
                bool optimal = false;
                if (current.ObjectiveType.ToLower() == "min")
                {
                    double OptimalityCheck = current.ObjectiveCoefficients.Max();
                    if (OptimalityCheck <= 0)
                    {
                        optimal = true;
                    }
                }
                else
                {
                    double OptimalityCheck = current.ObjectiveCoefficients.Min();
                    if (OptimalityCheck >= 0)
                    {
                        optimal = true;
                    }
                }

                DualResult.EndResult = optimal ? "optimal" : "feasible";
                break;
            }

            // choose pivot row: most negative RHS
            int PivotRow = -1;
            double mostNegative = Double.MaxValue;
            for (int i = 0; i < current.Constraints.Count; i++)
            {
                if (current.Constraints[i].RHS < mostNegative)
                {
                    mostNegative = current.Constraints[i].RHS;
                    PivotRow = i;
                }
            }

            if (PivotRow == -1)
            {
                DualResult.EndResult = "infeasible";
                break;
            }

            // choose pivot column: for a_rj < 0, pick j with minimal ratio c_j / a_rj (leftmost tie-break)
            int PivotColumn = -1;
            double bestRatio = Double.MaxValue;
            for (int j = 0; j < current.ObjectiveCoefficients.Count; j++)
            {
                double a_rj = current.Constraints[PivotRow].Coefficients[j];
                if (a_rj < 0)
                {
                    double ratio = current.ObjectiveCoefficients[j] / a_rj;
                    if (ratio < bestRatio)
                    {
                        bestRatio = ratio;
                        PivotColumn = j;
                    }
                }
            }

            if (PivotColumn == -1)
            {
                DualResult.EndResult = "infeasible";
                break;
            }

            StandardModel itteration = new StandardModel();
            itteration.ObjectiveCoefficients = new List<double>();
            itteration.Constraints = new List<Constraint>();

            for (int i = 0; i < current.Constraints.Count; i++)
            {
                itteration.Constraints.Add(new Constraint { Coefficients = new List<double>() });
            }

            // update objective coefficients
            for (int j = 0; j < current.ObjectiveCoefficients.Count; j++)
            {
                itteration.ObjectiveCoefficients.Add(
                    (current.ObjectiveCoefficients[j])
                    -
                    (current.ObjectiveCoefficients[PivotColumn]
                    *
                    ((current.Constraints[PivotRow].Coefficients[j])
                    /
                    (current.Constraints[PivotRow].Coefficients[PivotColumn]))));
            }

            itteration.ObjectiveFunctionRHS =
                ((current.ObjectiveFunctionRHS)
                -
                (current.ObjectiveCoefficients[PivotColumn]
                *
                ((current.Constraints[PivotRow].RHS)
                /
                (current.Constraints[PivotRow].Coefficients[PivotColumn]))));

            // pivot constraints
            for (int i = 0; i < current.Constraints.Count; i++)
            {
                if (i != PivotRow)
                {
                    for (int j = 0; j < current.Constraints[i].Coefficients.Count; j++)
                    {
                        itteration.Constraints[i].Coefficients.Add(
                            (current.Constraints[i].Coefficients[j])
                            -
                            (current.Constraints[i].Coefficients[PivotColumn]
                            *
                            ((current.Constraints[PivotRow].Coefficients[j])
                            /
                            ((current.Constraints[PivotRow].Coefficients[PivotColumn])))));
                    }

                    itteration.Constraints[i].RHS =
                        ((current.Constraints[i].RHS)
                        -
                        (current.Constraints[i].Coefficients[PivotColumn])
                        *
                        ((current.Constraints[PivotRow].RHS)
                        /
                        ((current.Constraints[PivotRow].Coefficients[PivotColumn]))));
                }
                else
                {
                    for (int j = 0; j < current.Constraints[i].Coefficients.Count; j++)
                    {
                        itteration.Constraints[i].Coefficients.Add(
                            current.Constraints[i].Coefficients[j]
                            /
                            current.Constraints[i].Coefficients[PivotColumn]);
                    }

                    itteration.Constraints[i].RHS =
                        (
                        (current.Constraints[i].RHS)
                        /
                        (current.Constraints[i].Coefficients[PivotColumn])
                        );
                }
            }

            itteration.SignRestrictions = initialTable.SignRestrictions;
            itteration.ObjectiveType = initialTable.ObjectiveType;
            itteration.VariableNames = initialTable.VariableNames;
            itteration.VariableNames.RemoveAll(name => name.StartsWith("a"));

            DualResult.tablues.Add(itteration);
            itterationNumber++;

            if (itterationNumber == 10000)
            {
                DualResult.EndResult = "Exceeded 10000 itterations";
                break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Dual simplex failed");
        Console.WriteLine(ex.ToString());
        return false;
    }

    return true;
}
#endregion

*/



        #region Method to do Two-Phase
        public bool TwoPhase(StandardModel initialTable, out ResolvedModel TwoPhasedResolved)
        {
            TwoPhasedResolved = new ResolvedModel();
            TwoPhasedResolved.tablues = new List<StandardModel>();
            TwoPhasedResolved.tablues.Add(initialTable);

            try
            {

                int itterationNumber = 0;
                bool optimal = false;

                while(optimal == false)
                {
                    StandardModel itteration = new StandardModel();
                    var coefficients = TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients;

                    var constraints = TwoPhasedResolved.tablues[itterationNumber].Constraints;

                    double max = coefficients.Max();
                    int PivotColumn = coefficients.IndexOf(max);    //keep in mind, this is arrays/lists.
                                                                    //if the pivot Column is techncally column 5,
                                                                    //for the method it will be column 4 as it starts at 0

                    List<double> ratios = new List<double>();
                    for(int i = 0; i < TwoPhasedResolved.tablues[itterationNumber].Constraints.Count; i++)
                    {
                        ratios.Add((TwoPhasedResolved.tablues[itterationNumber].Constraints[i].RHS) / (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]));
                    }

                    double SmallestRatio = Double.MaxValue;
                    int PivotRow = -1;
                    for(int i = 0; i < ratios.Count; i++)
                    {
                    if (constraints[i].RHS > 0)
                    {
                        if (ratios[i] > 0) { 
                            if (ratios[i] < SmallestRatio)
                            {
                                SmallestRatio = ratios[i];
                                PivotRow = i;
                            }   
                            }
                        }
                    }
                    itteration.TwoPhaseObjective = new Constraint();
                    itteration.TwoPhaseObjective.Coefficients = new List<Double>();

                    itteration.Constraints = new List<Constraint>();

                    //creates enough Constraints so that it can simply be filled later
                    for (int i = 0; i< TwoPhasedResolved.tablues[itterationNumber].Constraints.Count; i++)
                    {
                    itteration.Constraints.Add(new Constraint { Coefficients = new List<double>()});
                    }

                    //calculates the new values of the new objective row W
                    for (int i = 0; i < TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients.Count; i++)
                    {
                        itteration.TwoPhaseObjective.Coefficients.Add
                            ((TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients[i])
                            -
                            (TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients[PivotColumn]
                            *
                            ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[i])
                            /
                            (TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));
                    }
                        itteration.TwoPhaseObjective.RHS=
                            ((TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.RHS)
                            -
                            (TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients[PivotColumn]
                            *
                            ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].RHS)
                            /
                            (TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));


                //Calculates new values for the Objective function (Z row)

                itteration.ObjectiveCoefficients = new List<Double>();
                    for (int i = 0; i < TwoPhasedResolved.tablues[itterationNumber].TwoPhaseObjective.Coefficients.Count; i++)
                    {
                        itteration.ObjectiveCoefficients.Add
                            ((TwoPhasedResolved.tablues[itterationNumber].ObjectiveCoefficients[i])
                            -
                            (TwoPhasedResolved.tablues[itterationNumber].ObjectiveCoefficients[PivotColumn]
                            *
                            ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[i])
                            /
                            (TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));
                    }
                    itteration.ObjectiveFunctionRHS =
                        ((TwoPhasedResolved.tablues[itterationNumber].ObjectiveFunctionRHS)
                        -
                        (TwoPhasedResolved.tablues[itterationNumber].ObjectiveCoefficients[PivotColumn]
                        *
                        ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].RHS)
                        /
                        (TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))));


                //Pivots constraints
                for (int i = 0;i < TwoPhasedResolved.tablues[itterationNumber].Constraints.Count;i++)
                    {
                        if (i != PivotRow)
                        {
                            for (int j = 0; j < TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add
                                    (
                                    (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[j])
                                    -
                                    (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]
                                    *
                                    ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[j])
                                    /
                                    ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn]))
                                    )));


                            }
                            //Pivots RHS
                            itteration.Constraints[i].RHS =
                                ((TwoPhasedResolved.tablues[itterationNumber].Constraints[i].RHS)
                                -
                                (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn])
                                *
                                ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].RHS)
                                /
                                ((TwoPhasedResolved.tablues[itterationNumber].Constraints[PivotRow].Coefficients[PivotColumn])
                                )));

                        } else
                        {
                            for (int j = 0; j < TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add
                                    (
                                    TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[j]
                                    /
                                    TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn]
                                    );
                            }
                            //Pivots RHS
                            itteration.Constraints[i].RHS =
                                (
                                (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].RHS)
                                /
                                (TwoPhasedResolved.tablues[itterationNumber].Constraints[i].Coefficients[PivotColumn])
                                );

                        }
                    }
                    itteration.SignRestrictions = initialTable.SignRestrictions;
                    itteration.ObjectiveType = initialTable.ObjectiveType;
                    itteration.VariableNames = initialTable.VariableNames;



                    TwoPhasedResolved.tablues.Add(itteration);
                    itterationNumber++;

                    double OptimalityCheck = itteration.TwoPhaseObjective.Coefficients.Max();
                    if (OptimalityCheck <= 0)
                    {
                        optimal = true;
                        TwoPhasedResolved.EndResult = "optimal";
                    }

                    if(itterationNumber >= 10000)
                    {
                        TwoPhasedResolved.EndResult = "Exceeded 10000 itterations";
                        break;
                    }
                }

                }

                catch (Exception ex)
                {
                    Console.WriteLine("Two-phase simplex failed");
                    Console.WriteLine(ex.ToString());
                    return false;
                }



            return true;
        }
        #endregion



        #region Dual simplex method
        public bool DualSimplex(ResolvedModel InitialTablues, out ResolvedModel DualResult)
        {

            int initialLength = InitialTablues.tablues.Count -1; //does the minus one to get the index of the last tablue/list.
            DualResult = new ResolvedModel();
            DualResult.tablues = new List<StandardModel>();
            DualResult.tablues.Add(InitialTablues.tablues[initialLength]);


            try
            {

                int itterationNumber = 0;
                bool optimal = false;

                while (optimal == false)
                {
                    StandardModel current = DualResult.tablues[itterationNumber];
                    var coefficients = DualResult.tablues[itterationNumber].ObjectiveCoefficients;
                    var constraints = DualResult.tablues[itterationNumber].Constraints;



                    // check if all RHS are non-negative -> done
                    bool allNonNegative = true;
                    for (int i = 0; i < current.Constraints.Count; i++)
                    {
                        if (current.Constraints[i].RHS < -1e-9)
                        {
                            allNonNegative = false;
                            break;
                        }
                    }
                    if (allNonNegative)
                    {
                        DualResult.EndResult = "Optimal";
                        break;
                    }

                    //checks the pivot row
                    int PivotRow = -1;
                    double mostNegative = -1e-9; // also to prevent errors via double shenanigans.
                    for (int i = 0; i < current.Constraints.Count; i++)
                    {
                        if (current.Constraints[i].RHS < mostNegative)
                        {
                            mostNegative = current.Constraints[i].RHS;
                            PivotRow = i;
                        }
                    }

                    // at this stage, if pivot row is -1 that means all RHS values are 0, meaning its degenerate.
                    if (PivotRow == -1)
                    {
                        DualResult.EndResult = "Degenerate";
                        break;
                    }


                    int PivotColumn = -1;
                    double bestRatio = double.MaxValue;

                    for (int i = 0; i < current.ObjectiveCoefficients.Count; i++)
                    {
                        double a = current.Constraints[PivotRow].Coefficients[i];
                        if (a < -1e-9)                              // this is to prevent the weird quirk with doubles, where it can randomlt generate tiny numbers like -1e-27 (-0.0000000000000000000000000001)
                        {
                            double ratio = Math.Abs(current.ObjectiveCoefficients[i] / a);
                            if (ratio < bestRatio)
                            {
                                bestRatio = ratio;
                                PivotColumn = i;
                            }
                        }
                    }

                    if (PivotColumn == -1)
                    {
                        DualResult.EndResult = "infeasible";
                        break;
                    }

                    StandardModel itteration = new StandardModel();
                    itteration.ObjectiveCoefficients = new List<double>();
                    itteration.Constraints = new List<Constraint>();


                    for (int i = 0; i < current.Constraints.Count; i++)
                    {
                        itteration.Constraints.Add(new Constraint { Coefficients = new List<double>() });
                    }


                    // update objective coefficients
                    for (int j = 0; j < current.ObjectiveCoefficients.Count; j++)
                    {
                        itteration.ObjectiveCoefficients.Add(
                            (current.ObjectiveCoefficients[j])
                            -
                            (current.ObjectiveCoefficients[PivotColumn]
                            *
                            ((current.Constraints[PivotRow].Coefficients[j])
                            /
                            (current.Constraints[PivotRow].Coefficients[PivotColumn]))));
                    }

                    itteration.ObjectiveFunctionRHS =
                        ((current.ObjectiveFunctionRHS)
                        -
                        (current.ObjectiveCoefficients[PivotColumn]
                        *
                        ((current.Constraints[PivotRow].RHS)
                        /
                        (current.Constraints[PivotRow].Coefficients[PivotColumn]))));

                    // pivot constraints
                    for (int i = 0; i < current.Constraints.Count; i++)
                    {
                        if (i != PivotRow)
                        {
                            for (int j = 0; j < current.Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add(
                                    (current.Constraints[i].Coefficients[j])
                                    -
                                    (current.Constraints[i].Coefficients[PivotColumn]
                                    *
                                    ((current.Constraints[PivotRow].Coefficients[j])
                                    /
                                    ((current.Constraints[PivotRow].Coefficients[PivotColumn])))));
                            }

                            itteration.Constraints[i].RHS =
                                ((current.Constraints[i].RHS)
                                -
                                (current.Constraints[i].Coefficients[PivotColumn])
                                *
                                ((current.Constraints[PivotRow].RHS)
                                /
                                ((current.Constraints[PivotRow].Coefficients[PivotColumn]))));
                        }
                        else
                        {
                            for (int j = 0; j < current.Constraints[i].Coefficients.Count; j++)
                            {
                                itteration.Constraints[i].Coefficients.Add(
                                    current.Constraints[i].Coefficients[j]
                                    /
                                    current.Constraints[i].Coefficients[PivotColumn]);
                            }

                            itteration.Constraints[i].RHS =
                                (
                                (current.Constraints[i].RHS)
                                /
                                (current.Constraints[i].Coefficients[PivotColumn])
                                );
                        }
                    }

                    itteration.SignRestrictions = current.SignRestrictions;
                    itteration.ObjectiveType = current.ObjectiveType;
                    itteration.VariableNames = current.VariableNames;

                    DualResult.tablues.Add(itteration);
                    itterationNumber++;

                    if (itterationNumber >= 10000)
                    {
                        DualResult.EndResult = "Exceeded 10000 itterations";
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dual simplex failed");
                Console.WriteLine("Error in Dual simplex calculation");

                DualResult.EndResult = "error";
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
        #endregion
    }
}




