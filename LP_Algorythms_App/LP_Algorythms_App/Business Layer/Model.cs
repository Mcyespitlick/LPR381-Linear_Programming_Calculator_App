using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{


    //This is the canonical form, but it just takes the input given and spits it up into usable sections.
    public class ParsedModel 
    {
        public string ObjectiveType;                // WHich type of objective, such as "max" or "min"
        public double[] ObjectiveCoefficients;      // the values for the objective funtion, such as [2, 3, 3, 5, 2, 4]
        public List<Constraint> Constraints;        // Will hold the constraints (makes use of the class below)
        public string[] SignRestrictions;           // contains the last line. bin = binary, [+] = [x>=0], [-] = [x<=0], [urs] = can be anything (positive, negative, zero)
    }
    public class StandardModel
    {
        public string ObjectiveType;                // WHich type of objective, such as "max" or "min"
        public double[] ObjectiveCoefficients;      // the values for the objective funtion, such as [2, 3, 3, 5, 2, 4]
        public List<Constraint> Constraints;        // Will hold the constraints (makes use of the class below)
        public string[] VariableNames;              // contains the variable names like x1, s2 and e3.
    }

    //this is only used to make  list in the Model class (above)
    public class Constraint
    {
        public double[] Coefficients;   // the values such as [11, 8, 6, 14, 10, 10]
        public string Relation;         // the sign such as "<=", ">=", or "="
        public double RHS;              // THe RHS value (will only have 1 value)
    }


}