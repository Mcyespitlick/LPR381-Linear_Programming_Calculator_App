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
        DataHandler handler = new DataHandler();
        DataConversion conversion = new DataConversion();
        Reused_Algorythms reused_Algorythms = new Reused_Algorythms();
        String[][] data = null;
        ParsedModel parsedModel = null;
        ParsedModel CanonicalModel = null;
        StandardModel standardModel = null;

        ResolvedModel TwoPhasedResolved = null;

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
                handler.LoadData(filePath,out data);
                Console.WriteLine(data.Length);

                handler.RawToGrid(dgvTable, data); //the raw data is in canonical form, as this is how it was input. might still need to be parsed.
            }
        }

        //================================================================================================//
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //================================================================================================//
        private void btnCanonical_Click(object sender, EventArgs e) //to Standard
        {
            if (data == null || !data.Any())
            {
                Console.WriteLine("no data to convert");
            }
            else
            {
                if (conversion.ToCanonical(data, out parsedModel))
                {
                    Console.WriteLine("Successfully parsed");

                    if (conversion.ToStandard(parsedModel, out standardModel))
                    {
                        Console.WriteLine("Successfully Converted to StandardModel");
                        handler.StandardToGrid(dgvTable, standardModel, parsedModel);
                    }
                    else
                    {
                        Console.WriteLine("Failed to convert to standardModel");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to parse");
                }
            }
        }

        //================================================================================================//
        private void btnStandard_Click(object sender, EventArgs e)
        {

        }
        //================================================================================================//
        private void btnCanonical_Click_1(object sender, EventArgs e) // to canonical
        {
            if (data == null || !data.Any())
            {
                Console.WriteLine("no data to convert");
            }
            else
            {
                if (conversion.ToCanonical(data, out parsedModel))
                {
                    Console.WriteLine("Successfully parsed");
                }
                else
                {
                    Console.WriteLine("Failed to parse");
                }
            }
        }

        private void TwoPhase_Click(object sender, EventArgs e)
        {

            if (reused_Algorythms.TwoPhase(standardModel, out TwoPhasedResolved))
            {
                Console.WriteLine("Two-Phased successfully converted");
            } else
            {
                Console.WriteLine("Two-Phase failed");
            }
        }
        //================================================================================================//
    }
}
