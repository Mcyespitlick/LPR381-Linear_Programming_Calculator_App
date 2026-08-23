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

        CuttingPlane cuttingPlane = new CuttingPlane();


        String[][] data = null;
        ParsedModel parsedModel = null;
        ParsedModel CanonicalModel = null;
        StandardModel standardModel = null; //this is the very first tablue, just after converting it.

        ResolvedModel TwoPhasedResolved = null; //will have a set of tablues, if two-phase was done
        ResolvedModel PrimalResolved = null;    //will have a set of tablues if primal simplex was done
                                                //you will use the very last tablue if you coninue with two-phase.

        ResolvedModel CutPlaneOutput = null;

        ResolvedModel DualResolved = null;

        RevisedSimplex revisedSimplex = new RevisedSimplex();
        RevisedSimplexOutput revisedOutput = new RevisedSimplexOutput();
        RevisedSimplexResult revisedResult = null; // holds Binv/basis too, needed later for Sensitivity Analysis
        SensitivityAnalysis sensitivityAnalysis = new SensitivityAnalysis();

        Knapsack knapsack = new Knapsack();
        KnapsackOutput knapsackOutput = new KnapsackOutput();
        KnapsackResult knapsackResult = null;


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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void PrimalSimplex_Click(object sender, EventArgs e)
        {
            //PrimalSimplex(StandardModel initialTable, ResolvedModel TwoPhaseResult, out ResolvedModel PrimalResult)

            if (reused_Algorythms.PrimalSimplex(standardModel, TwoPhasedResolved, out PrimalResolved))
            {
                Console.WriteLine("Primal Simplex successfully done");
            }
            else
            {
                Console.WriteLine("Primal Simplex failed");
            }
        }

        private void btnDualSimplex_Click(object sender, EventArgs e)
        {
            ResolvedModel DualResolved;

            if (standardModel == null)
            {
                Console.WriteLine("No standard model available. Click 'To Standard' first.");
                return;
            }

            /*   this was for testing
            StandardModel t = new StandardModel();
            t.ObjectiveType = "max";
            t.ObjectiveCoefficients = new List<double> { 2, 3, 0, 0 };
            t.ObjectiveFunctionRHS = 0;
            t.VariableNames = new List<string> { "x1", "x2", "s1", "s2" };
            t.SignRestrictions = new List<string> { "none", "none", "none", "none" };
            t.Constraints = new List<Business_Layer.Constraint>
{
            new Business_Layer.Constraint { Coefficients = new List<double> { -1, -1, 1, 0 }, Relation = "<=", RHS = -2 },
            new Business_Layer.Constraint { Coefficients = new List<double> { -1, -2, 0, 1 }, Relation = "<=", RHS = -3 }
};
            ResolvedModel testinput = new ResolvedModel { tablues = new List<StandardModel> { t } };

            bool ok = reused_Algorythms.DualSimplex2(testinput, out DualResolved);
            */



            bool ok = reused_Algorythms.DualSimplex(PrimalResolved, out DualResolved);
            if (ok)
            {
                Console.WriteLine("Dual Simplex completed: " + (DualResolved.EndResult ?? "result unknown"));
                if (DualResolved.tablues != null && DualResolved.tablues.Count > 0)
                {
                    // reuse existing DataHandler display method (same used elsewhere)
                    handler.StandardToGrid(dgvTable, DualResolved.tablues.Last(), parsedModel);
                }
            }
            else
            {
                Console.WriteLine("Dual Simplex failed.");
            }
        }

        private void btnRevisedSimplex_Click(object sender, EventArgs e)
        {
            if (standardModel == null)
            {
                Console.WriteLine("No standard model to solve - load data and convert to standard form first.");
                return;
            }

            if (revisedSimplex.Solve(standardModel, out revisedResult))
            {
                Console.WriteLine("Revised Primal Simplex finished: " + revisedResult.Status);
                revisedOutput.ResultToGrid(dgvTable, revisedResult);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Revised Simplex Output";
                saveFileDialog.FileName = "output.txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    revisedOutput.WriteToFile(saveFileDialog.FileName, revisedResult);
                    Console.WriteLine("Output written to " + saveFileDialog.FileName);
                }
            }
            else
            {
                Console.WriteLine("Revised Primal Simplex failed.");
            }
        }

        private void btnSensitivity_Click(object sender, EventArgs e)
        {
            if (revisedResult == null)
            {
                Console.WriteLine("Run Revised Simplex first to generate sensitivity information.");
                return;
            }

            try
            {
                string report = sensitivityAnalysis.CreateReport(revisedResult);
                Console.WriteLine(report);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Sensitivity Analysis";
                saveFileDialog.FileName = "sensitivity-analysis.txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    System.IO.File.WriteAllText(saveFileDialog.FileName, report);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sensitivity analysis failed: " + ex.Message);
            }
        }
        //================================================================================================//
        private void btnKnapsack_Click(object sender, EventArgs e)
        {
            if (data == null || !data.Any())
            {
                Console.WriteLine("No data loaded. Click 'Load data from file' first.");
                return;
            }

            // Re-parse straight from the raw input so this button works standalone,
            // without depending on the LP simplex buttons having been clicked first.
            if (!conversion.ToCanonical(data, out parsedModel))
            {
                Console.WriteLine("Failed to parse the input file.");
                return;
            }

            if (knapsack.Solve(parsedModel, out knapsackResult))
            {
                Console.WriteLine("Branch & Bound Knapsack completed: " + knapsackResult.Status);

                knapsackOutput.ResultToGrid(dgvTable, knapsackResult);

                if (knapsackResult.Status == "Optimal")
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Save Knapsack Output";
                    saveFileDialog.FileName = "output.txt";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        knapsackOutput.WriteToFile(saveFileDialog.FileName, knapsackResult);
                        Console.WriteLine("Output written to " + saveFileDialog.FileName);
                    }
                }
            }
            else
            {
                Console.WriteLine("Branch & Bound Knapsack failed: " + knapsackResult.ErrorMessage);
            }
        }

        private void btnCuttingPlane_Click(object sender, EventArgs e)
        {
            
            bool ok = cuttingPlane.CuttingPlaneAlgo(PrimalResolved, out CutPlaneOutput);
            if (ok)
            {
                Console.WriteLine("CuttingPlane completed: " + (CutPlaneOutput.EndResult ?? "result unknown"));

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Cutting Plane Output";
                saveFileDialog.FileName = "CuttingPlaneOutput.txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    knapsackOutput.WriteToFile(saveFileDialog.FileName, knapsackResult);
                    Console.WriteLine("Output written to " + saveFileDialog.FileName);
                }


                if (CutPlaneOutput.tablues != null && CutPlaneOutput.tablues.Count > 0)
                {
                    handler.WriteResolvedModel(CutPlaneOutput, "Cutting Plane", saveFileDialog.FileName);
                }
            }
            else
            {
                Console.WriteLine("Cutting Plane failed.");
            }
        }
    }
    //================================================================================================//
}
