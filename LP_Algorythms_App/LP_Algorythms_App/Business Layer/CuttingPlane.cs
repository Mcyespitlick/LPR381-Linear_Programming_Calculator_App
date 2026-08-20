using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    internal class CuttingPlane
    {

        public bool CuttingPlaneAlgo(ResolvedModel model, out ResolvedModel CutModel)
        {
            //creates a deep copy of the Parsed model so that it never gets modified.
            ResolvedModel modelCopy = new ResolvedModel();


            modelCopy.EndResult = model.EndResult;
            
            foreach (StandardModel instance in  model.tablues)
            {
                StandardModel table = new StandardModel();

                table.ObjectiveFunctionRHS = instance.ObjectiveFunctionRHS;
                table.VariableNames = instance.VariableNames;
                table.ObjectiveCoefficients =new List<double>(instance.ObjectiveCoefficients);
                table.VariableNames = new List<string>(instance.VariableNames);

                table.Constraints = new List<Constraint>();
                //deep copy not completed


            }





            return true;
        }
    }
}
