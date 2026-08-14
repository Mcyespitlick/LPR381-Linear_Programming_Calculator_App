using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LP_Algorythms_App.Business_Layer
{
    internal class DataConversion
    {
        #region Method to split up the table into usable data.
        //======================================================
        public bool ToCanonical(String[][] RawData,out ParsedModel model) //fills the model that is given as a variable.
        {
            model = null;

            try
            {
                ParsedModel NewModel = new ParsedModel();

                //add the objective function
                String[] FirstLine = RawData[0];                //Loads objective function
                NewModel.ObjectiveType = FirstLine[0];         //check the first entry for if its min or max

                int ObjectiveFunctionLength = FirstLine[0].Length - 1;
                NewModel.ObjectiveCoefficients = new List<Double>();

                for (int i = 0; i < (FirstLine.Length -1); i++)
                {
                    NewModel.ObjectiveCoefficients.Add(double.Parse(FirstLine[i + 1]));
                }

                 
                // add the constraints
                NewModel.Constraints = new List<Constraint>();

                for (int i = 1; i < (RawData.Length - 1); i++)
                {
                    String[] line = RawData[i];

                    Constraint constraint = new Constraint();                   //create the array the will hold the coeficient.

                    constraint.Coefficients = new List<double>();

                    for (int j = 0; j < FirstLine.Length - 1; j++)
                    {
                        constraint.Coefficients.Add(double.Parse(line[j]));
                    }

                    constraint.Relation = line[FirstLine.Length - 1];

                    constraint.RHS = Double.Parse(line[FirstLine.Length]);

                    NewModel.Constraints.Add(constraint);

                }

                // add the sign restrictions
                String[] LastLine = RawData[RawData.Length-1];
                NewModel.SignRestrictions = new List<String>();

                for (int i = 0; i < (LastLine.Length); i++)
                {
                    NewModel.SignRestrictions.Add(LastLine[i]);
                }

                model = NewModel;
                return true;
            }
            catch (Exception e)
            {
                {
                    Console.WriteLine("Failed to parse:" + e.Message);
                    return false;
                }
            }
        }
        #endregion





        #region Method to convert the canonical (parsed) data into standard form
        public bool ToStandard(ParsedModel model, out StandardModel NewModel) 
        {
            NewModel = null;
            int NumberOfConstraints = model.Constraints.Count;
            int NumberOfColumns = model.ObjectiveCoefficients.Count;
            
            try
            {

                //this checks where the sign constraints are scanned for URS and '-', as these will require further transormations in their column
                //-----------------------------
                
                List<int> negativeSign = new List<int>();
                List<int> ursSign = new List<int>();
                List<String> SignRestrictions = new List<String>();



                for (int i = 0; i < model.SignRestrictions.Count; i++)
                {
                    if (model.SignRestrictions[i] == "-")
                    {
                        negativeSign.Add(i);
                    }
                    else if (model.SignRestrictions[i] == "urs")
                    {
                        ursSign.Add(i);
                    }
                }
                NumberOfColumns += ursSign.Count; //URS signs add 1 extra column


                //----------------------------

                StandardModel standardModel = new StandardModel();

                standardModel.ObjectiveType = model.ObjectiveType; //just reads in the Objective type

                //Checks the constraint's signs and RHS, flips all values and sign if RHS is negative.
                //then reads the signs to see how many extra columns will be required due to the slacks, excesses and artificial values.
                for (int i = 0; i < NumberOfConstraints; i++)
                {
                    if (model.Constraints[i].RHS < 0)
                    {
                        for (int j = 0; j < model.Constraints[i].Coefficients.Count; j++)
                        {
                            model.Constraints[i].Coefficients[j] = -model.Constraints[i].Coefficients[j];
                        }
                        model.Constraints[i].RHS = -model.Constraints[i].RHS;

                        if (model.Constraints[i].Relation == ">=")
                        {
                            model.Constraints[i].Relation = "<=";
                        } else 
                        if (model.Constraints[i].Relation == "<=")
                        {
                            model.Constraints[i].Relation = ">=";
                        }
                    }

                    if (model.Constraints[i].Relation == "<=" || model.Constraints[i].Relation == "=")
                    {
                        NumberOfColumns++;
                    }
                    
                    if(model.Constraints[i].Relation == ">=")
                    {
                        NumberOfColumns += 2;
                    }
                }

                List<String> VariableNames = new List<String>();

                standardModel.ObjectiveCoefficients = new List<Double>();

                //reads in the values of the objective function (Z row), with a bunch of zeros in the new columns


                standardModel.SignRestrictions = new List<String>();

                int ExtraColumn = 0;
                for (int i = 0; i < model.ObjectiveCoefficients.Count; i++)
                {
                    if (negativeSign.Contains(i))
                    {
                        standardModel.ObjectiveCoefficients.Add(model.ObjectiveCoefficients[i]);
                        VariableNames.Add("x" + (i + 1) + "'");
                        standardModel.SignRestrictions.Add("none");
                    }
                    else if (ursSign.Contains(i))
                    {
                        standardModel.ObjectiveCoefficients.Add(-(model.ObjectiveCoefficients[i]));
                        standardModel.ObjectiveCoefficients.Add(model.ObjectiveCoefficients[i]);

                        VariableNames.Add("x" + (i + 1));
                        VariableNames.Add("x" + (i + 1) + "'");

                        standardModel.SignRestrictions.Add("none");
                        standardModel.SignRestrictions.Add("none");

                        ExtraColumn++;
                    }
                    else
                    {
                        standardModel.ObjectiveCoefficients.Add(-(model.ObjectiveCoefficients[i]));
                        VariableNames.Add("x" + (i + 1));

                        if (model.SignRestrictions[i] == "bin")
                            standardModel.SignRestrictions.Add("bin");
                        else if (model.SignRestrictions[i] == "int")
                            standardModel.SignRestrictions.Add("int");
                        else
                            standardModel.SignRestrictions.Add("invalid");
                    }
                }

                //objective fucntion will never have values in the slack, excess and artificial values. so itt just fills it with zeros.
                while (standardModel.ObjectiveCoefficients.Count < NumberOfColumns)
                {
                    standardModel.ObjectiveCoefficients.Add(0);
                }

                //this just fills the slack, excess and artificial columns. these values are ALWAYS >=0 , so they will always turn to "None".
                while (standardModel.SignRestrictions.Count < NumberOfColumns)
                {
                    standardModel.SignRestrictions.Add("none");
                }


                int Zeros = 0;          // the plan is to keep track how many zeros before the 1 in the new columns
                                        // for instance, if Zeros = 1 then the ro will be s1 = 0, s2 = 1. 0 1 0 0 0
                                        // if Zeros = 3 he it will be s1=0, s2=0 and s3=1 or 0 0 1 0 0

                standardModel.Constraints = new List<Constraint>();

                #region Adding of constraints
                // here the constraints are actually added
                for (int i = 0; i< NumberOfConstraints; i++)
                {
                    Constraint constraint = new Constraint();
                    constraint.Coefficients = new List<Double>();
                    ExtraColumn = 0;



                    if (model.Constraints[i].Relation == "<=" || model.Constraints[i].Relation == "=")
                    {
                          
                        for (int j = 0; j < model.Constraints[i].Coefficients.Count; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients.Add(-(model.Constraints[i].Coefficients[j]));
                            }
                            else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients.Add((model.Constraints[i].Coefficients[j]));
                                constraint.Coefficients.Add(-(model.Constraints[i].Coefficients[j]));
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients.Add(model.Constraints[i].Coefficients[j]);
                                
                            }

                        }
                        for(int k = 0; k<Zeros; k++)
                        {
                            constraint.Coefficients.Add(0);
                        }

                        constraint.RHS = model.Constraints[i].RHS;
                        constraint.Relation = model.Constraints[i].Relation;

                        for (int k = 0; k < standardModel.Constraints.Count; k++)
                        {
                            standardModel.Constraints[k].Coefficients.Add(0);
                        }

                        constraint.Coefficients.Add(1);
                        VariableNames.Add((model.Constraints[i].Relation == "<=") ? "s" + (i + 1) : "a" + (i + 1));
                        Zeros++;
                        standardModel.Constraints.Add(constraint);
                    }

                    if (model.Constraints[i].Relation == ">=")
                    {
                        for (int j = 0; j < model.Constraints[i].Coefficients.Count; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients.Add(-(model.Constraints[i].Coefficients[j]));
                            }
                            else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients.Add(model.Constraints[i].Coefficients[j]);
                                constraint.Coefficients.Add(-(model.Constraints[i].Coefficients[j]));
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients.Add(model.Constraints[i].Coefficients[j]);
                            }
                        }

                        for (int k = 0; k < Zeros; k++)
                        {
                            constraint.Coefficients.Add(0);
                        }

                        constraint.RHS = model.Constraints[i].RHS;
                        constraint.Relation = model.Constraints[i].Relation;

                        for (int k = 0; k < standardModel.Constraints.Count; k++)
                        {
                            standardModel.Constraints[k].Coefficients.Add(0);
                        }

                        constraint.Coefficients.Add(-1);
                        VariableNames.Add("e" + (i + 1));
                        Zeros++;

                        for (int k = 0; k < standardModel.Constraints.Count; k++)
                        {
                            standardModel.Constraints[k].Coefficients.Add(0);
                        }

                        constraint.Coefficients.Add(1);
                        VariableNames.Add("a" + (i + 1));
                        Zeros++;

                        standardModel.Constraints.Add(constraint);


                    }
                }
                #endregion

                #region creation of new objective (W row) if required)
                //this will check if there are any artifcial values. if so, will add an extra objective row.



                Constraint NewOBJ = new Constraint();
                NewOBJ.Coefficients = new List<double>();
                bool YesObjective = false;

                for (int k = 0; k < VariableNames.Count; k++)
                {
                    NewOBJ.Coefficients.Add(0);
                }

                for (int i=0; i< VariableNames.Count; i++)
                {
                    if (VariableNames[i].StartsWith("a"))
                    {
                        string numberPart = VariableNames[i].Substring(1); // should delete the "a" and put the rest in a string.
                        int constraintIndex = int.Parse(numberPart);       // "11" -> 11 as an int

                        for(int j = 0; j < VariableNames.Count; j++)
                        {
                            if (!VariableNames[j].StartsWith("a"))
                            {
                                NewOBJ.Coefficients[j] += standardModel.Constraints[constraintIndex - 1].Coefficients[j];
                                
                            }
                        }
                        NewOBJ.RHS += standardModel.Constraints[constraintIndex - 1].RHS;
                        YesObjective = true;
                    }
                }

                if (YesObjective)
                {
                    standardModel.TwoPhaseObjective = NewOBJ;
                }
                #endregion



                standardModel.VariableNames = VariableNames;
                NewModel = standardModel;
                return true;
            }
            catch (Exception e) 
            {
                Console.WriteLine("Could not transform to Standard Form: "+ e.Message);
                return false;
            }
        }
        #endregion
    }
}
