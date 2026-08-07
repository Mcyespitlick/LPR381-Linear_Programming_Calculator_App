using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LP_Algorythms_App.Business_Layer
{
    //this will effective only hold the objective function row (line 1 of input) and sign restrictions (last line of input)
    //it will be used to calculate the Objective type (Min or Max), objective function, 
    public class ParsedModel 
    {
        public string ObjectiveType;                // WHich type of objective, such as "max" or "min"
        public double[] ObjectiveCoefficients;      // the values for the objective funtion, such as [2, 3, 3, 5, 2, 4]
        public List<Constraint> Constraints;        // Will hold the constraints (makes use of the class below)
        public string[] SignRestrictions;           // contains the last line. bin = binary, [+] = [x>=0], [-] = [x<=0], [urs] = can be anything (positive, negative, zero)
    }

    //this is only used to make  list in the Model class (above)
    public class Constraint
    {
        public double[] Coefficients;   // the values such as [11, 8, 6, 14, 10, 10]
        public string Relation;         // the sign such as "<=", ">=", or "="
        public double RHS;              // THe RHS value (will only have 1 value)
    }
}