using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    internal class CuttingPlane
    {
        Reused_Algorythms reused_Algorythms = new Reused_Algorythms();

        public bool CuttingPlaneAlgo(ResolvedModel model, out ResolvedModel CutModel)
        {
            int ItterationNumber = 0;
            double tolerance = 1e-9;
            CutModel = null;
            //creates a deep copy of the Parsed model so that it never gets modified.
            ResolvedModel modelCopy = new ResolvedModel();
            CutModel = null;

            modelCopy.tablues = new List<StandardModel>();
            modelCopy.EndResult = model.EndResult;
            
            foreach (StandardModel instance in  model.tablues)
            {
                StandardModel table = new StandardModel();

                table.ObjectiveFunctionRHS = instance.ObjectiveFunctionRHS;
                table.ObjectiveCoefficients =new List<double>(instance.ObjectiveCoefficients);
                table.VariableNames = new List<string>(instance.VariableNames);
                table.ObjectiveType = instance.ObjectiveType;
                table.SignRestrictions = new List<string>(instance.SignRestrictions);

                table.Constraints = new List<Constraint>();

                foreach (Constraint c in instance.Constraints)
                {
                    Constraint newConstraint = new Constraint();
                    newConstraint.Coefficients = new List<double>(c.Coefficients);
                    newConstraint.Relation = c.Relation;
                    newConstraint.RHS = c.RHS;
                    table.Constraints.Add(newConstraint);
                }

                if (instance.TwoPhaseObjective != null)
                {
                    Constraint newObjective = new Constraint();
                    newObjective.Coefficients = new List<double>(instance.TwoPhaseObjective.Coefficients);
                    newObjective.Relation = instance.TwoPhaseObjective.Relation;
                    newObjective.RHS = instance.TwoPhaseObjective.RHS;
                    table.TwoPhaseObjective = newObjective;
                }
                modelCopy.tablues.Add(table);
            }

            CutModel = modelCopy;

            int tables = model.tablues.Count;

            StandardModel LastTablue = new StandardModel();
            LastTablue = model.tablues[tables - 1];


            bool optimal = false;

            while (optimal == false)
            {

                List<int> indexes = new List<int>();
                List<double> ValidRHS = new List<double>();

                for (int i = 0; i< CutModel.tablues[tables-1].Constraints.Count; i++)
                {
                    double value = CutModel.tablues[tables - 1].Constraints[i].RHS;
                    if (Math.Abs(value - Math.Round(value)) > tolerance)
                    {
                        indexes.Add(i);
                        ValidRHS.Add(value);
                    }
                }

                int bestindex = -1;
                double testRHS = -1;
                double bestRHS = -1;

                if (indexes.Count == 0)
                {
                    CutModel.EndResult = "Optimal";
                    break;
                }


                //finds best RHS. due to being > and nit >= it will always take the topmost constraint
                for (int i = 0; i < indexes.Count; i++)
                {
                    testRHS = Math.Abs(ValidRHS[i] - Math.Round(ValidRHS[i]));
                    if (testRHS > bestRHS)
                    {
                        bestindex = indexes[i];
                        bestRHS = testRHS;
                    }
                }



                /*
                StandardModel instance = new StandardModel();
                instance.Constraints = new List<Constraint>();

                instance.ObjectiveFunctionRHS = CutModel.tablues[tables - 1].ObjectiveFunctionRHS;
                instance.ObjectiveCoefficients = new List<double>(CutModel.tablues[tables - 1].ObjectiveCoefficients);
                instance.VariableNames = new List<string>(CutModel.tablues[tables - 1].VariableNames);
                */



                Constraint NewConstraint = new Constraint();
                NewConstraint.Coefficients = new List<double>();

                NewConstraint.RHS = -Math.Abs(CutModel.tablues[tables - 1].Constraints[bestindex].RHS-Math.Floor(CutModel.tablues[tables - 1].Constraints[bestindex].RHS));
                NewConstraint.Relation = CutModel.tablues[tables - 1].Constraints[bestindex].Relation;

                //calculates and adds the new values into the new constraint
                foreach (double d in CutModel.tablues[tables - 1].Constraints[bestindex].Coefficients)
                {
                    NewConstraint.Coefficients.Add(-(d - Math.Floor(d)));
                }

                //adds the new slack column's value
                NewConstraint.Coefficients.Add(1);


                StandardModel source = CutModel.tablues[tables - 1];
                StandardModel instance = new StandardModel();
                instance.ObjectiveType = source.ObjectiveType;
                instance.ObjectiveFunctionRHS = source.ObjectiveFunctionRHS;
                instance.ObjectiveCoefficients = new List<double>(source.ObjectiveCoefficients);
                instance.VariableNames = new List<string>(source.VariableNames);
                instance.SignRestrictions = new List<string>(source.SignRestrictions);
                instance.Constraints = new List<Constraint>();
                foreach (Constraint c in source.Constraints)
                {
                    instance.Constraints.Add(new Constraint
                    {
                        Coefficients = new List<double>(c.Coefficients),
                        Relation = c.Relation,
                        RHS = c.RHS
                    });
                }

                // widen every existing row by one column for the cut's own slack
                foreach (Constraint c in instance.Constraints)
                {
                    c.Coefficients.Add(0);
                }
                instance.ObjectiveCoefficients.Add(0);
                instance.SignRestrictions.Add("none");

                int ExtraColumns = 0;
                foreach (string name in instance.VariableNames)
                {
                    string numberPart = name.Substring(1).Replace("'", "");
                    int number;
                    if (int.TryParse(numberPart, out number) && number > ExtraColumns)
                    {
                        ExtraColumns = number;
                    }
                }
                instance.VariableNames.Add("s" + (ExtraColumns + 1));

                instance.Constraints.Add(NewConstraint);
                CutModel.tablues.Add(instance);
                tables = CutModel.tablues.Count;


                ResolvedModel UpdatedResolvedModel = new ResolvedModel();

                reused_Algorythms.DualSimplex(CutModel, out UpdatedResolvedModel);

                for (int k = 1; k < UpdatedResolvedModel.tablues.Count; k++)
                {
                    CutModel.tablues.Add(UpdatedResolvedModel.tablues[k]);
                }


                CutModel.EndResult = UpdatedResolvedModel.EndResult;

                bool optimalityCheck = true;



                for (int i = 0; i < CutModel.tablues[tables - 1].Constraints.Count; i++)
                {
                    double value = CutModel.tablues[tables - 1].Constraints[i].RHS;
                    if (Math.Abs(value - Math.Round(value)) > tolerance)
                    {
                        optimalityCheck = false;
                    }
                }

                if (optimalityCheck)
                {
                    optimal = true;
                }
                tables = CutModel.tablues.Count;

                ItterationNumber++;
                if (ItterationNumber >= 10000)
                {
                    CutModel.EndResult = "Exceeded 10000 itterations";
                    break;
                }
            }

            return true;
        }
    }
}
