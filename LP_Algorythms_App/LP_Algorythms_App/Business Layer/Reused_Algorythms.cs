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
        public bool PrimalSimplex()
        {
            return true;
        }



        public bool DualSimplex()
        {
            return true;
        }

        public bool TwoPhase(StandardModel initialTable, out ResolvedModel TwoPhasedResolved)
        {
            TwoPhasedResolved = new ResolvedModel();
            TwoPhasedResolved.tablues = new List<StandardModel>();
            TwoPhasedResolved.tablues.Add(initialTable);

            //try
            //{

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

                    double SmallestRatio = 10000;
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

                    double OptimalityCheck = itteration.TwoPhaseObjective.Coefficients.Max();
                    if (OptimalityCheck < 1)
                    {
                    optimal = true;
                    }

                    TwoPhasedResolved.tablues.Add(itteration);
                    itterationNumber++;
                    if(itterationNumber == 20)
                    {
                        break;
                    }
                }
            /*
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Two-phase simplex failed");
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            */


            return true;
        }
    }
}
