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
        Branch_Bound branchBound = new Branch_Bound();
        Branch_Bound.Result branchBoundResult = null;


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
            if (standardModel == null)
            {
                Console.WriteLine("No standard model to solve.");
                return;
            }

            if (reused_Algorythms.TwoPhase(standardModel, out TwoPhasedResolved))
            {
                int tables = TwoPhasedResolved.tablues.Count;
                Console.WriteLine("Two-Phased successfully converted");
                handler.StandardToGrid(dgvTable, TwoPhasedResolved.tablues[tables-1], parsedModel);
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
                handler.StandardToGrid(dgvTable, PrimalResolved.tablues.Last(), parsedModel);
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

        #region
        private void btnSensitivity_Click(object sender, EventArgs e)
        {
            if (revisedResult == null)
            {
                MessageBox.Show("Run Revised Simplex first, then select Sensitivity Analysis.",
                    "Sensitivity Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var optionsForm = new SensitivityAnalysisForm(revisedResult, sensitivityAnalysis,
                    updatedResult => revisedResult = updatedResult))
                    optionsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sensitivity analysis failed: " + ex.Message,
                    "Sensitivity Analysis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        //================================================================================================//

        #region Button to do Knapsack
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

            if (PrimalResolved == null)
            {
                Console.WriteLine("No Primal Simplex model to cut on.");
                return;
            }

            bool ok = cuttingPlane.CuttingPlaneAlgo(PrimalResolved, out CutPlaneOutput);
            if (ok)
            {
                handler.StandardToGrid(dgvTable, CutPlaneOutput.tablues.Last(), parsedModel);
                
            }
            else
            {
                Console.WriteLine("Cutting Plane failed.");
            }
        }

        private void btnBranchBound_Click(object sender, EventArgs e)
        {


            if (data == null || !data.Any())
            {
                Console.WriteLine(
                    "No data loaded. Click 'Load data from file' first.");

                return;
            }

            // ============================================================
            // PARSE INPUT
            // ============================================================

            if (!conversion.ToCanonical(
                data,
                out parsedModel))
            {
                Console.WriteLine(
                    "Failed to parse input.");

                return;
            }

            Console.WriteLine(
                "Input successfully parsed.");

            // ============================================================
            // CONVERT TO STANDARD FORM
            // ============================================================

            if (!conversion.ToStandard(
                parsedModel,
                out standardModel))
            {
                Console.WriteLine(
                    "Failed to convert to StandardModel.");

                return;
            }

            Console.WriteLine(
                "StandardModel successfully created.");

            // ============================================================
            // TWO PHASE
            // ============================================================

            if (!reused_Algorythms.TwoPhase(
                standardModel,
                out TwoPhasedResolved))
            {
                Console.WriteLine(
                    "Two-Phase failed.");

                return;
            }

            Console.WriteLine(
                "Two-Phase result: "
                + TwoPhasedResolved.EndResult);

            // ============================================================
            // CHECK TWO-PHASE INFEASIBILITY
            // ============================================================

            if (!string.IsNullOrEmpty(
                TwoPhasedResolved.EndResult))
            {
                string status =
                    TwoPhasedResolved.EndResult.ToLower();

                if (status.Contains("infeasible"))
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "======================================");

                    Console.WriteLine(
                        "BRANCH AND BOUND");

                    Console.WriteLine(
                        "======================================");

                    Console.WriteLine(
                        "The LP relaxation is infeasible.");

                    Console.WriteLine(
                        "Branch and Bound cannot continue.");

                    Console.WriteLine(
                        "======================================");

                    return;
                }
            }

            // ============================================================
            // PRIMAL SIMPLEX
            // ============================================================

            if (!reused_Algorythms.PrimalSimplex(
                standardModel,
                TwoPhasedResolved,
                out PrimalResolved))
            {
                Console.WriteLine(
                    "Primal Simplex failed.");

                return;
            }

            Console.WriteLine(
                "Primal Simplex result: "
                + PrimalResolved.EndResult);

            // ============================================================
            // RUN BRANCH AND BOUND
            // ============================================================

            if (!branchBound.Solve(
                standardModel,
                PrimalResolved,
                out branchBoundResult))
            {
                Console.WriteLine(
                    "Branch and Bound failed.");

                return;
            }

            // ============================================================
            // DISPLAY RESULT
            // ============================================================

            Console.WriteLine();
            Console.WriteLine(
                "======================================");

            Console.WriteLine(
                "BRANCH AND BOUND RESULT");

            Console.WriteLine(
                "======================================");

            Console.WriteLine(
                "Status: "
                + branchBoundResult.Status);

            Console.WriteLine(
                "Optimal: "
                + branchBoundResult.Optimal);

            Console.WriteLine(
                "Total Nodes: "
                + branchBoundResult.TotalNodes);

            Console.WriteLine(
                "Pruned Nodes: "
                + branchBoundResult.PrunedNodes);

            if (!double.IsNaN(
                branchBoundResult.Z))
            {
                Console.WriteLine(
                    "Z = "
                    + branchBoundResult.Z);
            }

            if (branchBoundResult.X != null)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Solution:");

                for (int i = 0;
                     i < branchBoundResult.X.Length;
                     i++)
                {
                    Console.WriteLine(
                        "x" +
                        (i + 1) +
                        " = " +
                        Math.Round(
                            branchBoundResult.X[i],
                            6));
                }
            }

            Console.WriteLine(
                "======================================");

            // ============================================================
            // DISPLAY NODE INFORMATION
            // ============================================================

            if (branchBoundResult.Nodes != null)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "NODE INFORMATION");

                Console.WriteLine(
                    "--------------------------------------");

                foreach (Branch_Bound.Node node
                    in branchBoundResult.Nodes)
                {
                    Console.WriteLine(
                        "Node "
                        + node.NodeId
                        + " | Parent: "
                        + node.ParentId
                        + " | Depth: "
                        + node.Depth);

                    Console.WriteLine(
                        "Branch: "
                        + node.BranchDescription);

                    Console.WriteLine(
                        "Z/Bound: "
                        + Math.Round(
                            node.Bound,
                            6));

                    if (node.IsInteger)
                    {
                        Console.WriteLine(
                            "Result: INTEGER");
                    }
                    else if (node.IsInfeasible)
                    {
                        Console.WriteLine(
                            "Result: INFEASIBLE");
                    }
                    else if (node.IsPruned)
                    {
                        Console.WriteLine(
                            "Result: PRUNED - "
                            + node.PruneReason);
                    }

                    Console.WriteLine();
                }
            }

            // ============================================================
            // DISPLAY BEST TABLEAU
            // ============================================================

            if (branchBoundResult.BestNode != null)
            {
                handler.StandardToGrid(
                    dgvTable,
                    branchBoundResult.BestNode.Tableau,
                    parsedModel);
            }
        }

        #endregion

        private void btnPrintTwoPhase_Click(object sender, EventArgs e)
        {

            if (TwoPhasedResolved == null)
            {
                Console.WriteLine("No two-phase output to print.");
                return;
            }

            Console.WriteLine("CuttingPlane completed: " + (TwoPhasedResolved.EndResult ?? "result unknown"));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Cutting Plane Output";
            saveFileDialog.FileName = "Two-PhaseOutput.txt";


            
            if (TwoPhasedResolved.tablues != null && TwoPhasedResolved.tablues.Count > 0)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    handler.WriteResolvedModel(TwoPhasedResolved, "Two-Phase", saveFileDialog.FileName);
                    MessageBox.Show("Written to:\n" + saveFileDialog.FileName);
                }
            }
        }

        private void btnPrintCuttingPlane_Click(object sender, EventArgs e)
        {

            if (CutPlaneOutput == null)
            {
                Console.WriteLine("No Cutting plane output to print.");
                return;
            }

            Console.WriteLine("CuttingPlane completed: " + (CutPlaneOutput.EndResult ?? "result unknown"));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Cutting Plane Output";
            saveFileDialog.FileName = "CuttingPlaneOutput.txt";



            if (CutPlaneOutput.tablues != null && CutPlaneOutput.tablues.Count > 0)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    handler.WriteResolvedModel(CutPlaneOutput, "Cutting Plane", saveFileDialog.FileName);
                    MessageBox.Show("Written to:\n" + saveFileDialog.FileName);
                }
            }
        }

        private void btnPrintPrimal_Click(object sender, EventArgs e)
        {
            if (PrimalResolved == null)
            {
                Console.WriteLine("No Primal Simplex output to print.");
                return;
            }


            Console.WriteLine("PrimalSimplex completed: " + (PrimalResolved.EndResult ?? "result unknown"));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Primal Simplex Output";
            saveFileDialog.FileName = "PrimalSimplexOutput.txt";


            
            if (PrimalResolved.tablues != null && PrimalResolved.tablues.Count > 0)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    handler.WriteResolvedModel(PrimalResolved, "Primal Simplex", saveFileDialog.FileName);
                    MessageBox.Show("Written to:\n" + saveFileDialog.FileName);
                }
            }
        }

        private void btnPrintDual_Click(object sender, EventArgs e)
        {
            if (CutPlaneOutput == null)
            {
                Console.WriteLine("No Dual Simplex output to print.");
                return;
            }

            Console.WriteLine("Dual Simplex Completed completed: " + (DualResolved.EndResult ?? "result unknown"));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Dual Simplex Output";
            saveFileDialog.FileName = "DualSimplexOutput.txt";


            
            if (DualResolved.tablues != null && DualResolved.tablues.Count > 0)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    handler.WriteResolvedModel(DualResolved, "Dual Simplex", saveFileDialog.FileName);
                    MessageBox.Show("Written to:\n" + saveFileDialog.FileName);
                }
            }
        }
    }
    
    //================================================================================================//
}
