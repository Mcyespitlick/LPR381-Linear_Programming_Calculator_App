using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LP_Algorythms_App.Business_Layer;

namespace LP_Algorythms_App
{
    public partial class SensitivityAnalysisForm : Form
    {
        private readonly SensitivityAnalysis analysis;
        private RevisedSimplexResult result;
        private readonly Action<RevisedSimplexResult> resultUpdated;
        private readonly ComboBox operationBox = new ComboBox();
        private readonly ComboBox variableBox = new ComboBox();
        private readonly ComboBox constraintBox = new ComboBox();
        private readonly TextBox valueBox = new TextBox();
        private readonly TextBox secondValueBox = new TextBox();
        private readonly TextBox listBox = new TextBox();
        private readonly TextBox reportBox = new TextBox();
        private readonly Label valueLabel = new Label();
        private readonly Label secondValueLabel = new Label();
        private readonly Label listLabel = new Label();

        private const string FullReport = "Display full sensitivity report";
        private const string ObjectiveRange = "Display objective coefficient range";
        private const string RhsRange = "Display constraint RHS range";
        private const string ColumnRange = "Display non-basic column range";
        private const string ObjectiveChange = "Apply objective coefficient change";
        private const string RhsChange = "Apply constraint RHS change";
        private const string ColumnChange = "Apply non-basic column change";
        private const string AddActivity = "Add a new activity";
        private const string AddConstraint = "Add a new constraint";
        private const string Dual = "Solve and verify the dual model";

        public SensitivityAnalysisForm(RevisedSimplexResult solvedResult,
            SensitivityAnalysis sensitivityEngine, Action<RevisedSimplexResult> onResultUpdated)
        {
            result = solvedResult ?? throw new ArgumentNullException("solvedResult");
            analysis = sensitivityEngine ?? throw new ArgumentNullException("sensitivityEngine");
            resultUpdated = onResultUpdated;
            InitializeComponent();
            LoadModelOptions();
            operationBox.SelectedIndex = 0;
            ShowCurrentReport();
        }

        private void InitializeComponent()
        {
            Text = "Sensitivity Analysis Options";
            Width = 980;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new System.Drawing.Size(760, 520);

            var controls = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 175,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(10)
            };
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245));
            controls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
            operationBox.Items.AddRange(new object[] { FullReport, ObjectiveRange, RhsRange, ColumnRange,
                ObjectiveChange, RhsChange, ColumnChange, AddActivity, AddConstraint, Dual });
            operationBox.SelectedIndexChanged += operationBox_SelectedIndexChanged;
            AddRow(controls, 0, "Operation", operationBox);
            AddRow(controls, 1, "Variable", variableBox);
            AddRow(controls, 2, "Constraint", constraintBox);
            valueLabel.Text = "New value";
            AddRow(controls, 3, valueLabel, valueBox);
            secondValueLabel.Text = "Second value";
            AddRow(controls, 4, secondValueLabel, secondValueBox);
            listLabel.Text = "Coefficients (comma separated)";
            AddRow(controls, 5, listLabel, listBox);

            var executeButton = new Button { Text = "Run", AutoSize = true };
            executeButton.Click += executeButton_Click;
            var saveButton = new Button { Text = "Save report", AutoSize = true };
            saveButton.Click += saveButton_Click;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            buttons.Controls.Add(executeButton);
            buttons.Controls.Add(saveButton);
            AddRow(controls, 6, "", buttons);

            reportBox.Multiline = true;
            reportBox.ReadOnly = true;
            reportBox.ScrollBars = ScrollBars.Both;
            reportBox.Dock = DockStyle.Fill;
            reportBox.Font = new System.Drawing.Font("Consolas", 10);

            Controls.Add(controls);
            Controls.Add(reportBox);
            reportBox.BringToFront();
        }

        private static void AddRow(TableLayoutPanel panel, int row, string label, Control control)
        {
            AddRow(panel, row, new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, control);
        }

        private static void AddRow(TableLayoutPanel panel, int row, Label label, Control control)
        {
            panel.Controls.Add(label, 0, row);
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, 1, row);
        }

        private void LoadModelOptions()
        {
            variableBox.Items.Clear();
            variableBox.Items.AddRange(result.Model.VariableNames.ToArray());
            if (variableBox.Items.Count > 0)
                variableBox.SelectedIndex = 0;

            constraintBox.Items.Clear();
            for (int index = 0; index < result.Model.Constraints.Count; index++)
                constraintBox.Items.Add("Constraint " + (index + 1));
            if (constraintBox.Items.Count > 0)
                constraintBox.SelectedIndex = 0;
        }

        private void operationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string operation = operationBox.SelectedItem as string;
            bool needsVariable = operation != FullReport && operation != RhsRange && operation != AddConstraint && operation != Dual;
            bool needsConstraint = operation == RhsRange || operation == ColumnRange || operation == RhsChange || operation == ColumnChange;
            bool needsValue = operation == ObjectiveChange || operation == RhsChange || operation == ColumnChange || operation == AddActivity || operation == AddConstraint;
            bool needsSecondValue = operation == ColumnChange || operation == AddActivity || operation == AddConstraint;
            bool needsList = operation == AddActivity || operation == AddConstraint;
            variableBox.Enabled = needsVariable;
            constraintBox.Enabled = needsConstraint;
            valueBox.Enabled = needsValue;
            secondValueBox.Enabled = needsSecondValue;
            listBox.Enabled = needsList;
            valueLabel.Text = operation == AddActivity ? "Objective coefficient" :
                operation == AddConstraint ? "Relation (=, <=, >=)" : "New value";
            secondValueLabel.Text = operation == AddActivity ? "Activity name" :
                operation == AddConstraint ? "Right-hand side" : "New coefficient";
            listLabel.Text = operation == AddActivity ? "Constraint coefficients (comma separated)" :
                "Variable coefficients (comma separated)";

            if (operation == FullReport)
                ShowCurrentReport();
        }

        private void ShowCurrentReport()
        {
            try
            {
                if (result == null || result.Model == null)
                {
                    reportBox.Text = "No optimal simplex result is available yet.";
                    return;
                }

                if (result.Status == "Optimal")
                    reportBox.Text = analysis.CreateReport(result);
                else
                    reportBox.Text = "Sensitivity analysis requires an optimal revised simplex result.\nCurrent status: " + result.Status;
            }
            catch (Exception ex)
            {
                reportBox.Text = ex.Message;
            }
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            try
            {
                string operation = operationBox.SelectedItem as string;
                int variableIndex = variableBox.SelectedIndex;
                int constraintIndex = constraintBox.SelectedIndex;
                string report;

                if (operation == FullReport)
                    reportBox.Text = analysis.CreateReport(result);
                else if (operation == ObjectiveRange)
                    reportBox.Text = RangeText(analysis.GetObjectiveCoefficientRange(result, variableIndex));
                else if (operation == RhsRange)
                    reportBox.Text = RangeText(analysis.GetConstraintRange(result, constraintIndex));
                else if (operation == ColumnRange)
                    reportBox.Text = RangeText(analysis.GetNonBasicColumnRange(result, variableIndex, constraintIndex));
                else if (operation == ObjectiveChange)
                    UpdateResult(analysis.ApplyObjectiveCoefficientChange(result, variableIndex, Number(valueBox.Text), out report), report);
                else if (operation == RhsChange)
                    UpdateResult(analysis.ApplyConstraintRhsChange(result, constraintIndex, Number(valueBox.Text), out report), report);
                else if (operation == ColumnChange)
                    UpdateResult(analysis.ApplyNonBasicColumnChange(result, variableIndex, constraintIndex,
                        Number(secondValueBox.Text), out report), report);
                else if (operation == AddActivity)
                    UpdateResult(analysis.AddActivity(result, secondValueBox.Text.Trim(), Number(valueBox.Text),
                        Values(listBox.Text, result.Model.Constraints.Count), out report), report);
                else if (operation == AddConstraint)
                    UpdateResult(analysis.AddConstraint(result, valueBox.Text.Trim(), Number(secondValueBox.Text),
                        Values(listBox.Text, result.Model.VariableNames.Count), out report), report);
                else if (operation == Dual)
                    SolveDual();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sensitivity Analysis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SolveDual()
        {
            string report;
            RevisedSimplexResult dualResult = analysis.SolveDual(result, out report);
            string status = "Primal status: " + result.Status + Environment.NewLine +
                "Dual status: " + dualResult.Status + Environment.NewLine;
            if (result.Status == "Optimal" && dualResult.Status == "Optimal")
                status += "Duality: " + (Math.Abs(result.Z - dualResult.Z) < 0.001 ? "Strong" : "Weak") +
                    Environment.NewLine + "Primal Z: " + result.Z.ToString("0.###") +
                    Environment.NewLine + "Dual Z: " + dualResult.Z.ToString("0.###") + Environment.NewLine + Environment.NewLine;
            reportBox.Text = status + report;
        }

        private void UpdateResult(RevisedSimplexResult updatedResult, string report)
        {
            result = updatedResult;
            resultUpdated?.Invoke(updatedResult);
            LoadModelOptions();
            reportBox.Text = report;
        }

        private static double Number(string text)
        {
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                throw new ArgumentException("Enter a valid number.");
            return value;
        }

        private static IList<double> Values(string text, int expectedCount)
        {
            List<double> values = text.Split(',').Select(value => Number(value.Trim())).ToList();
            if (values.Count != expectedCount)
                throw new ArgumentException("Enter exactly " + expectedCount + " comma-separated coefficients.");
            return values;
        }

        private static string RangeText(SensitivityRange range)
        {
            return range.Name + Environment.NewLine +
                "Current value: " + range.CurrentValue.ToString("0.###") + Environment.NewLine +
                "Allowable decrease: " + Format(range.AllowableDecrease) + Environment.NewLine +
                "Allowable increase: " + Format(range.AllowableIncrease) + Environment.NewLine +
                (range.IsBasic ? "Variable is basic." : "Variable is non-basic.");
        }

        private static string Format(double value)
        {
            return double.IsPositiveInfinity(value) ? "infinity" :
                double.IsNegativeInfinity(value) ? "-infinity" : value.ToString("0.###");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(reportBox.Text))
            {
                MessageBox.Show("Run an analysis first.", "Sensitivity Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*", FileName = "sensitivity-analysis.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    File.WriteAllText(dialog.FileName, reportBox.Text);
            }
        }
    }
}
