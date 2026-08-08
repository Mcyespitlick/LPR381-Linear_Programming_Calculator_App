using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LP_Algorythms_App.Business_Layer
{
    
    internal class DataHandler
    {
        #region Data loading Method
        //======================================================
        /*This method only read the file and validates the structure partially*/
        public bool LoadData(string FilePath, out String[][] data)
        {

            data = null;
            string[] Lines = File.ReadAllLines(FilePath);
            string[][] TempData = new string[Lines.Length][];

            for (int i = 0; i < Lines.Length; i++) {
                TempData[i] = Lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            }

            int ArraySize = TempData.Length;
            int ExpectedRowLength = TempData[0].Length;


            // Check constraints are one longer than first line.
            // The first line contains X ammount of variables, then a min/max entry. 
            // constraints have X ammount of variables, a <= or >= and a extra entry at the end.
            // thus if the first line is a length of 7, the constraints should have a lenght of 8.
            for (int i = 1; i < ArraySize - 1; i++)
            {
                if (TempData[i].Length != (ExpectedRowLength + 1))
                {
                    Console.WriteLine("constraint lengths are wrong");
                    return false;
                }
            }

            // Check last line is one shorter than first line
            // this is the same logic as the check above, except the last line has no symbol or min/max, only X ammount of entries (eg 6)
            if (TempData[ArraySize - 1].Length != (ExpectedRowLength - 1))
            {
                Console.WriteLine("last line length is wrong");
                return false;
            }

            data = TempData;
            Console.WriteLine("lengths are fine in test");
            return true;

        }
    #endregion

        #region Raw Data To Grid Method
        
        public void RawToGrid(DataGridView table,String[][] model)
        {
            table.Columns.Clear();
            table.Rows.Clear();

            table.RowCount = model.Length+1;
            table.ColumnCount = model[1].Length+2;
            int firstrow = model[0].Length;


            //reads in the headers
            for (int i = 1; i < firstrow; i++)
            {
                table.Rows[0].Cells[i].Value = ("x"+i);
            }

            //reads the objective function into the table
            for (int i = 0; i < firstrow; i++)
            {
                table.Rows[1].Cells[i].Value = model[0][i];
            }

            //reads constraints into table
            for (int i = 1; i < model.Length-1; i++) {
                for (int j = 0; j < firstrow+1; j++)
                {
                    Console.WriteLine(model[i][j]);
                    table.Rows[i+1].Cells[j+1].Value = model[i][j];
                }
            }

            //reads in sign restrictions
            for(int i=0; i<firstrow-1; i++)
            {
                Console.WriteLine(model[table.RowCount - 2][i]);
                table.Rows[table.RowCount-1].Cells[i + 1].Value = model[table.RowCount-2][i];
            }
        }

        #endregion

        public void StandardToGrid(DataGridView table, StandardModel standardModel, ParsedModel parsedModel)
        {
            table.Columns.Clear();
            table.Rows.Clear();

            table.RowCount = standardModel.Constraints.Count + 2;
            table.ColumnCount = standardModel.Constraints[0].Coefficients.Length + 3;
            int firstrow = standardModel.ObjectiveCoefficients.Length;

            int ursCounter = 0;

            //reads in the headers
            for (int i = 0; i < firstrow; i++)
            {
                table.Rows[0].Cells[i].Value = standardModel.VariableNames[i];
            }


            firstrow = standardModel.ObjectiveCoefficients.Length;
            ursCounter = 0;
            //reads the objective function into the table
            for (int i = 0; i < firstrow; i++)
            {
                table.Rows[1].Cells[i ].Value = standardModel.ObjectiveCoefficients[i];


            }


            firstrow = standardModel.ObjectiveCoefficients.Length;
            ursCounter = 0;
            //reads constraints into table
            for (int i = 0; i < standardModel.Constraints.Count; i++)
            {
                for (int j = 0; j < firstrow + 2; j++)
                {
                    if (j<firstrow)
                    {
                        table.Rows[i + 2].Cells[j].Value = standardModel.Constraints[i].Coefficients[j];

                    }else if (j == firstrow)
                    {
                        table.Rows[i + 2].Cells[j].Value = standardModel.Constraints[i].Relation;
                    }
                    else
                    {
                        table.Rows[i + 2].Cells[j].Value = standardModel.Constraints[i].RHS;
                    }

                }
            }
        }
    }
}
