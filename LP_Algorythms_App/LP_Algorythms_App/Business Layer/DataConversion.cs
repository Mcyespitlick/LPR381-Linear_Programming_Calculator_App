using System;
using System.Collections.Generic;
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
        public bool ParseModel(String[][] RawData,out ParsedModel model) //fills the model that is given as a variable.
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

        //public bool ToCanonical() { }
    }
}
