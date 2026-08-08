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
                NewModel.ObjectiveCoefficients = new double[FirstLine.Length - 1];

                for (int i = 0; i < (FirstLine.Length -1); i++)
                {
                    NewModel.ObjectiveCoefficients[i] = double.Parse(FirstLine[i + 1]);
                }

                 
                // add the constraints
                NewModel.Constraints = new List<Constraint>();

                for (int i = 1; i < (RawData.Length - 1); i++)
                {
                    String[] line = RawData[i];

                    Constraint constraint = new Constraint();                   //create the array the will hold the coeficient.

                    constraint.Coefficients = new double[FirstLine.Length - 1]; //needs to be here in order for the array size to be set.

                    for (int j = 0; j < FirstLine.Length - 1; j++)
                    {
                        constraint.Coefficients[j] = double.Parse(line[j]);
                    }

                    constraint.Relation = line[FirstLine.Length - 1];

                    constraint.RHS = Double.Parse(line[FirstLine.Length]);

                    NewModel.Constraints.Add(constraint);

                }

                // add the sign restrictions
                String[] LastLine = RawData[RawData.Length-1];
                NewModel.SignRestrictions = LastLine;

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
            int NumberOfColumns = model.ObjectiveCoefficients.Length;


            try
            {
                //this checks where the sign constraints are scanned for URS and '-', as these will require further transormations in their column
                //-----------------------------
                
                List<int> negativeSign = new List<int>();
                List<int> ursSign = new List<int>();

                for (int i = 0; i < model.SignRestrictions.Length; i++)
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

                for (int i = 0; i < NumberOfConstraints; i++)
                {
                    
                    if (model.Constraints[i].Relation == "<=" || model.Constraints[i].Relation == "=")
                    {
                        NumberOfColumns++;
                    }
                    
                    if(model.Constraints[i].Relation == ">=")
                    {
                        NumberOfColumns += 2;
                    }
                }

                String[] VariableNames = new string[NumberOfColumns];

                standardModel.ObjectiveCoefficients = new double[NumberOfColumns];

                //reads in the values of the objective function (Z row), with a bunch of zeros in the new columns

                int ExtraColumn = 0;
                for (int i = 0; i < model.ObjectiveCoefficients.Length; i++)
                {
                    
                            if (negativeSign.Contains(i))
                        {
                            standardModel.ObjectiveCoefficients[i + ExtraColumn] = -(model.ObjectiveCoefficients[i]);
                            VariableNames[i + ExtraColumn] = "x" + (i + 1) + "'";
                        }
                        else if (ursSign.Contains(i))
                        {
                            standardModel.ObjectiveCoefficients[i + ExtraColumn] = (model.ObjectiveCoefficients[i]);
                            standardModel.ObjectiveCoefficients[i + ExtraColumn + 1] = -(model.ObjectiveCoefficients[i]);

                            VariableNames[i + ExtraColumn] = "x" + (i + 1);
                            VariableNames[i + ExtraColumn + 1] = "x" + (i + 1) + "'";

                            ExtraColumn++;
                        }
                            else
                        {
                            standardModel.ObjectiveCoefficients[i + ExtraColumn] = (model.ObjectiveCoefficients[i]);
                            VariableNames[i + ExtraColumn] = "x" + (i + 1);
                        }

                }


                int Zeros = 0;          // the plan is to keep track how many zeros before the 1 in the new columns
                                        // for instance, if Zeros = 1 then the ro will be s1 = 0, s2 = 1. 0 1 0 0 0
                                        // if Zeros = 3 he it will be s1=0, s2=0 and s3=1 or 0 0 1 0 0

                standardModel.Constraints = new List<Constraint>();

                // here the constraints are actually added
                for (int i = 0; i< NumberOfConstraints; i++)
                {
                    Constraint constraint = new Constraint();
                    constraint.Coefficients = new double[NumberOfColumns];
                    ExtraColumn = 0;

                    if (model.Constraints[i].Relation == "<=" || model.Constraints[i].Relation == "=")
                    {

                        for (int j = 0; j < model.Constraints[i].Coefficients.Length; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients[j+ExtraColumn] = -(model.Constraints[i].Coefficients[j]);
                            }else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients[j + ExtraColumn] = (model.Constraints[i].Coefficients[j]);
                                constraint.Coefficients[j + ExtraColumn +1] = -(model.Constraints[i].Coefficients[j]);
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients[j + ExtraColumn] = (model.Constraints[i].Coefficients[j]);
                            }

                        }
                        constraint.RHS = model.Constraints[i].RHS;
                        constraint.Relation = model.Constraints[i].Relation;

                        constraint.Coefficients[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = 1;
                        VariableNames[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = (model.Constraints[i].Relation == "<=") ? "s" + (i + 1) : "a" + (i + 1);
                        Zeros++;
                        standardModel.Constraints.Add(constraint);
                        }

                    if (model.Constraints[i].Relation == ">=")
                    {
                        for (int j = 0; j < model.Constraints[i].Coefficients.Length; j++)
                        {
                            if (negativeSign.Contains(j))
                            {
                                constraint.Coefficients[j + ExtraColumn] = -(model.Constraints[i].Coefficients[j]);
                            }
                            else if (ursSign.Contains(j))
                            {
                                constraint.Coefficients[j + ExtraColumn] = (model.Constraints[i].Coefficients[j]);
                                constraint.Coefficients[j + ExtraColumn+1] = -(model.Constraints[i].Coefficients[j]);
                                ExtraColumn++;
                            }
                            else
                            {
                                constraint.Coefficients[j + ExtraColumn] = (model.Constraints[i].Coefficients[j]);
                            }
                        }
                        constraint.RHS = model.Constraints[i].RHS;
                        constraint.Relation = model.Constraints[i].Relation;


                        //this will first add the negative e value, then the positive a value.
                        constraint.Coefficients[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = -1;
                        VariableNames[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = "e" + (i + 1);
                        Zeros++;
                        constraint.Coefficients[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = 1;
                        VariableNames[model.Constraints[i].Coefficients.Length + Zeros + ExtraColumn] = "a" + (i + 1);
                        Zeros++;

                        standardModel.Constraints.Add(constraint);
                    }
                }

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
