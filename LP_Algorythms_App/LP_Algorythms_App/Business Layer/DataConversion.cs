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
            //creates a deep copy of the Parsed model so that it never gets modified.
            ParsedModel modelCopy = new ParsedModel();
            modelCopy.ObjectiveType = model.ObjectiveType;
            modelCopy.ObjectiveCoefficients = new List<double>(model.ObjectiveCoefficients);
            modelCopy.SignRestrictions = new List<string>(model.SignRestrictions);
            modelCopy.Constraints = new List<Constraint>();
            foreach (var c in model.Constraints)
            {
                modelCopy.Constraints.Add(new Constraint
                {
                    Coefficients = new List<double>(c.Coefficients),
                    Relation = c.Relation,
                    RHS = c.RHS
                });
            }


            NewModel = null;

            int NumberOfColumns = model.ObjectiveCoefficients.Count;
            
            try
            {
                // adding new constraints to deal with the Binary constraints

                List<int> BinIndexes = new List<int>();

                for (int i = 0;i< modelCopy.SignRestrictions.Count; i++)
                {
                    string sign = modelCopy.SignRestrictions[i];
                    if (modelCopy.SignRestrictions[i].ToLower() == "bin")
                    {
                        BinIndexes.Add(i);
                    }
                }

                for (int i=0; i< BinIndexes.Count; i++)
                {
                    int BinColumn = BinIndexes[i];
                    Constraint BinConstraint = new Constraint();
                    BinConstraint.Coefficients = new List<double>();
                    BinConstraint.RHS = 1;
                    BinConstraint.Relation = "<=";

                    for (int j = 0; j < modelCopy.ObjectiveCoefficients.Count; j++)
                    {
                        if(j == BinColumn)
                        {
                            BinConstraint.Coefficients.Add(1);
                        }
                        else
                        {
                            BinConstraint.Coefficients.Add(0);
                        }

                    }
                    modelCopy.Constraints.Add(BinConstraint);
                }

                int NumberOfConstraints = modelCopy.Constraints.Count;

                //this checks where the sign constraints are scanned for URS and '-', as these will require further transormations in their column
                //-----------------------------

                List<int> negativeSign = new List<int>();
                List<int> ursSign = new List<int>();
                List<String> SignRestrictions = new List<String>();



                for (int i = 0; i < modelCopy.SignRestrictions.Count; i++)
                {
                    if (modelCopy.SignRestrictions[i] == "-")
                    {
                        negativeSign.Add(i);
                    }
                    else if (modelCopy.SignRestrictions[i] == "urs")
                    {
                        ursSign.Add(i);
                    }
                }
                NumberOfColumns += ursSign.Count; //URS signs add 1 extra column


                //----------------------------

                StandardModel standardModel = new StandardModel();

                standardModel.ObjectiveType = modelCopy.ObjectiveType; //just reads in the Objective type

                //Checks the constraint's signs and RHS, flips all values and sign if RHS is negative.
                //then reads the signs to see how many extra columns will be required due to the slacks, excesses and artificial values.
                for (int i = 0; i < NumberOfConstraints; i++)
                {
                    if (modelCopy.Constraints[i].RHS < 0)
                    {
                        for (int j = 0; j < modelCopy.Constraints[i].Coefficients.Count; j++)
                        {
                            modelCopy.Constraints[i].Coefficients[j] = -modelCopy.Constraints[i].Coefficients[j];
                        }
                        modelCopy.Constraints[i].RHS = -modelCopy.Constraints[i].RHS;

                        if (modelCopy.Constraints[i].Relation == ">=")
                        {
                            modelCopy.Constraints[i].Relation = "<=";
                        } else 
                        if (modelCopy.Constraints[i].Relation == "<=")
                        {
                                modelCopy.Constraints[i].Relation = ">=";
                        }
                    }

                    if (modelCopy.Constraints[i].Relation == "<=" || modelCopy.Constraints[i].Relation == "=")
                    {
                        NumberOfColumns++;
                    }
                    
                    if(modelCopy.Constraints[i].Relation == ">=")
                    {
                        NumberOfColumns += 2;
                    }
                }

                List<String> VariableNames = new List<String>();

                standardModel.ObjectiveCoefficients = new List<Double>();

                //reads in the values of the objective function (Z row), with a bunch of zeros in the new columns


                standardModel.SignRestrictions = new List<String>();

                int ExtraColumn = 0;
                for (int i = 0; i < modelCopy.ObjectiveCoefficients.Count; i++)
                {
                    if (negativeSign.Contains(i))
                    {
                        standardModel.ObjectiveCoefficients.Add(modelCopy.ObjectiveCoefficients[i]);
                        VariableNames.Add("x" + (i + 1) + "'");
                        standardModel.SignRestrictions.Add("none");
                    }
                    else if (ursSign.Contains(i))
                    {
                        standardModel.ObjectiveCoefficients.Add(-(modelCopy.ObjectiveCoefficients[i]));
                        standardModel.ObjectiveCoefficients.Add(modelCopy.ObjectiveCoefficients[i]);

                        VariableNames.Add("x" + (i + 1));
                        VariableNames.Add("x" + (i + 1) + "'");

                        standardModel.SignRestrictions.Add("none");
                        standardModel.SignRestrictions.Add("none");

                        ExtraColumn++;
                    }
                    else
                    {
                        standardModel.ObjectiveCoefficients.Add(-(modelCopy.ObjectiveCoefficients[i]));
                        VariableNames.Add("x" + (i + 1));

                        if (modelCopy.SignRestrictions[i] == "bin")
                            standardModel.SignRestrictions.Add("bin");
                        else if (modelCopy.SignRestrictions[i] == "int")
                            standardModel.SignRestrictions.Add("int");
                        else if (modelCopy.SignRestrictions[i] == "+")
                            standardModel.SignRestrictions.Add("none");
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



                    if (modelCopy.Constraints[i].Relation == "<=" || modelCopy.Constraints[i].Relation == "=")
                    {
                          
                        for (int j = 0; j < modelCopy.Constraints[i].Coefficients.Count; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients.Add(-(modelCopy.Constraints[i].Coefficients[j]));
                            }
                            else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients.Add((modelCopy.Constraints[i].Coefficients[j]));
                                constraint.Coefficients.Add(-(modelCopy.Constraints[i].Coefficients[j]));
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients.Add(modelCopy.Constraints[i].Coefficients[j]);
                                
                            }

                        }
                        for(int k = 0; k<Zeros; k++)
                        {
                            constraint.Coefficients.Add(0);
                        }

                        constraint.RHS = modelCopy.Constraints[i].RHS;
                        constraint.Relation = modelCopy.Constraints[i].Relation;

                        for (int k = 0; k < standardModel.Constraints.Count; k++)
                        {
                            standardModel.Constraints[k].Coefficients.Add(0);
                        }

                        constraint.Coefficients.Add(1);
                        VariableNames.Add((modelCopy.Constraints[i].Relation == "<=") ? "s" + (i + 1) : "a" + (i + 1));
                        Zeros++;
                        standardModel.Constraints.Add(constraint);
                    }

                    if (modelCopy.Constraints[i].Relation == ">=")
                    {
                        for (int j = 0; j < modelCopy.Constraints[i].Coefficients.Count; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients.Add(-(modelCopy.Constraints[i].Coefficients[j]));
                            }
                            else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients.Add(modelCopy.Constraints[i].Coefficients[j]);
                                constraint.Coefficients.Add(-(modelCopy.Constraints[i].Coefficients[j]));
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients.Add(modelCopy.Constraints[i].Coefficients[j]);
                            }
                        }

                        for (int k = 0; k < Zeros; k++)
                        {
                            constraint.Coefficients.Add(0);
                        }

                        constraint.RHS = modelCopy.Constraints[i].RHS;
                        constraint.Relation = modelCopy.Constraints[i].Relation;

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
                standardModel.ObjectiveFunctionRHS = 0;


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
