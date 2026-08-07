using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LP_Algorythms_App.Business_Layer;

namespace LP_Algorythms_App
{
    public partial class Form1 : Form
    {
        DataLoader logic = new DataLoader();
        DataConversion conversion = new DataConversion();
        String[][] data = null;
        ParsedModel parsedModel = null;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Only show text files
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Select Input File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                data = null;
                string filePath = openFileDialog.FileName;
                Console.WriteLine(filePath );
                logic.LoadData(filePath,out data);
                Console.WriteLine(data.Length);

                logic.RawToGrid(dgvTable, data);
            }
        }

        //================================================================================================//
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //================================================================================================//
        private void btnCanonical_Click(object sender, EventArgs e)
        {
            if (!data.Any() || data == null) { 
                Console.WriteLine("no data to convert"); 
            }
            else
            {
                if(conversion.ParseModel(data, out parsedModel))
                {
                    Console.WriteLine("Successfully parsed");

                    //still needs to convert then add the canonical form
                }
                else
                {
                    Console.WriteLine("Failed to parse");
                }
            }
        }
        //================================================================================================//
    }
}
