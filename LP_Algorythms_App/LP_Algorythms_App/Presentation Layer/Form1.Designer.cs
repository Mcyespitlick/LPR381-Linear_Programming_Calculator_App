namespace LP_Algorythms_App
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.dgvTable = new System.Windows.Forms.DataGridView();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnStandard = new System.Windows.Forms.Button();
            this.TwoPhase = new System.Windows.Forms.Button();
            this.PrimalSimplex = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDualSimplex = new System.Windows.Forms.Button();
            this.btnRevisedSimplex = new System.Windows.Forms.Button();
            this.btnSensitivity = new System.Windows.Forms.Button();
            this.btnKnapsack = new System.Windows.Forms.Button();
            this.btnCuttingPlane = new System.Windows.Forms.Button();
            this.btnPrintTwoPhase = new System.Windows.Forms.Button();
            this.btnPrintPrimal = new System.Windows.Forms.Button();
            this.btnPrintDual = new System.Windows.Forms.Button();
            this.btnPrintCuttingPlane = new System.Windows.Forms.Button();
            this.btnBranchBound = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvTable
            // 
            this.dgvTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTable.Location = new System.Drawing.Point(244, 59);
            this.dgvTable.Name = "dgvTable";
            this.dgvTable.RowHeadersWidth = 51;
            this.dgvTable.Size = new System.Drawing.Size(1219, 358);
            this.dgvTable.TabIndex = 0;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(10, 449);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(13, 59);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(186, 23);
            this.btnLoadData.TabIndex = 2;
            this.btnLoadData.Text = "Load data from file";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // btnStandard
            // 
            this.btnStandard.Location = new System.Drawing.Point(15, 88);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new System.Drawing.Size(186, 23);
            this.btnStandard.TabIndex = 3;
            this.btnStandard.Text = "Standard Form";
            this.btnStandard.UseVisualStyleBackColor = true;
            this.btnStandard.Click += new System.EventHandler(this.btnCanonical_Click);
            // 
            // TwoPhase
            // 
            this.TwoPhase.Location = new System.Drawing.Point(15, 146);
            this.TwoPhase.Name = "TwoPhase";
            this.TwoPhase.Size = new System.Drawing.Size(128, 23);
            this.TwoPhase.TabIndex = 5;
            this.TwoPhase.Text = "Two-Phase";
            this.TwoPhase.UseVisualStyleBackColor = true;
            this.TwoPhase.Click += new System.EventHandler(this.TwoPhase_Click);
            // 
            // PrimalSimplex
            // 
            this.PrimalSimplex.Location = new System.Drawing.Point(16, 176);
            this.PrimalSimplex.Name = "PrimalSimplex";
            this.PrimalSimplex.Size = new System.Drawing.Size(127, 23);
            this.PrimalSimplex.TabIndex = 6;
            this.PrimalSimplex.Text = "PrimalSimplex";
            this.PrimalSimplex.UseVisualStyleBackColor = true;
            this.PrimalSimplex.Click += new System.EventHandler(this.PrimalSimplex_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1157, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnDualSimplex
            // 
            this.btnDualSimplex.Location = new System.Drawing.Point(15, 205);
            this.btnDualSimplex.Name = "btnDualSimplex";
            this.btnDualSimplex.Size = new System.Drawing.Size(128, 23);
            this.btnDualSimplex.TabIndex = 8;
            this.btnDualSimplex.Text = "Dual Simplex";
            this.btnDualSimplex.UseVisualStyleBackColor = true;
            this.btnDualSimplex.Click += new System.EventHandler(this.btnDualSimplex_Click);
            // 
            // btnRevisedSimplex
            // 
            this.btnRevisedSimplex.Location = new System.Drawing.Point(15, 336);
            this.btnRevisedSimplex.Name = "btnRevisedSimplex";
            this.btnRevisedSimplex.Size = new System.Drawing.Size(181, 23);
            this.btnRevisedSimplex.TabIndex = 9;
            this.btnRevisedSimplex.Text = "Revised Simplex";
            this.btnRevisedSimplex.UseVisualStyleBackColor = true;
            this.btnRevisedSimplex.Click += new System.EventHandler(this.btnRevisedSimplex_Click);
            // 
            // btnSensitivity
            // 
            this.btnSensitivity.Location = new System.Drawing.Point(15, 366);
            this.btnSensitivity.Name = "btnSensitivity";
            this.btnSensitivity.Size = new System.Drawing.Size(181, 23);
            this.btnSensitivity.TabIndex = 10;
            this.btnSensitivity.Text = "Sensitivity Analysis";
            this.btnSensitivity.UseVisualStyleBackColor = true;
            this.btnSensitivity.Click += new System.EventHandler(this.btnSensitivity_Click);
            // 
            // btnKnapsack
            // 
            this.btnKnapsack.Location = new System.Drawing.Point(15, 396);
            this.btnKnapsack.Name = "btnKnapsack";
            this.btnKnapsack.Size = new System.Drawing.Size(181, 23);
            this.btnKnapsack.TabIndex = 11;
            this.btnKnapsack.Text = "Branch && Bound Knapsack";
            this.btnKnapsack.UseVisualStyleBackColor = true;
            this.btnKnapsack.Click += new System.EventHandler(this.btnKnapsack_Click);
            // 
            // btnCuttingPlane
            // 
            this.btnCuttingPlane.Location = new System.Drawing.Point(16, 234);
            this.btnCuttingPlane.Name = "btnCuttingPlane";
            this.btnCuttingPlane.Size = new System.Drawing.Size(127, 23);
            this.btnCuttingPlane.TabIndex = 12;
            this.btnCuttingPlane.Text = "CuttingPlane";
            this.btnCuttingPlane.UseVisualStyleBackColor = true;
            this.btnCuttingPlane.Click += new System.EventHandler(this.btnCuttingPlane_Click);
            // 
            // btnPrintTwoPhase
            // 
            this.btnPrintTwoPhase.Location = new System.Drawing.Point(159, 146);
            this.btnPrintTwoPhase.Name = "btnPrintTwoPhase";
            this.btnPrintTwoPhase.Size = new System.Drawing.Size(75, 23);
            this.btnPrintTwoPhase.TabIndex = 13;
            this.btnPrintTwoPhase.Text = "Print";
            this.btnPrintTwoPhase.UseVisualStyleBackColor = true;
            this.btnPrintTwoPhase.Click += new System.EventHandler(this.btnPrintTwoPhase_Click);
            // 
            // btnPrintPrimal
            // 
            this.btnPrintPrimal.Location = new System.Drawing.Point(159, 176);
            this.btnPrintPrimal.Name = "btnPrintPrimal";
            this.btnPrintPrimal.Size = new System.Drawing.Size(75, 23);
            this.btnPrintPrimal.TabIndex = 14;
            this.btnPrintPrimal.Text = "Print";
            this.btnPrintPrimal.UseVisualStyleBackColor = true;
            this.btnPrintPrimal.Click += new System.EventHandler(this.btnPrintPrimal_Click);
            // 
            // btnPrintDual
            // 
            this.btnPrintDual.Location = new System.Drawing.Point(159, 205);
            this.btnPrintDual.Name = "btnPrintDual";
            this.btnPrintDual.Size = new System.Drawing.Size(75, 23);
            this.btnPrintDual.TabIndex = 15;
            this.btnPrintDual.Text = "Print";
            this.btnPrintDual.UseVisualStyleBackColor = true;
            this.btnPrintDual.Click += new System.EventHandler(this.btnPrintDual_Click);
            // 
            // btnPrintCuttingPlane
            // 
            this.btnPrintCuttingPlane.Location = new System.Drawing.Point(159, 234);
            this.btnPrintCuttingPlane.Name = "btnPrintCuttingPlane";
            this.btnPrintCuttingPlane.Size = new System.Drawing.Size(75, 23);
            this.btnPrintCuttingPlane.TabIndex = 16;
            this.btnPrintCuttingPlane.Text = "Print";
            this.btnPrintCuttingPlane.UseVisualStyleBackColor = true;
            this.btnPrintCuttingPlane.Click += new System.EventHandler(this.btnPrintCuttingPlane_Click);
            // 
            // btnBranchBound
            // 
            this.btnBranchBound.Location = new System.Drawing.Point(15, 306);
            this.btnBranchBound.Margin = new System.Windows.Forms.Padding(2);
            this.btnBranchBound.Name = "btnBranchBound";
            this.btnBranchBound.Size = new System.Drawing.Size(181, 19);
            this.btnBranchBound.TabIndex = 17;
            this.btnBranchBound.Text = "Branch && Bound";
            this.btnBranchBound.UseVisualStyleBackColor = true;
            this.btnBranchBound.Click += new System.EventHandler(this.btnBranchBound_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1443, 510);
            this.Controls.Add(this.btnBranchBound);
            this.Controls.Add(this.btnPrintCuttingPlane);
            this.Controls.Add(this.btnPrintDual);
            this.Controls.Add(this.btnPrintPrimal);
            this.Controls.Add(this.btnPrintTwoPhase);
            this.Controls.Add(this.btnCuttingPlane);
            this.Controls.Add(this.btnKnapsack);
            this.Controls.Add(this.btnRevisedSimplex);
            this.Controls.Add(this.btnSensitivity);
            this.Controls.Add(this.btnDualSimplex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PrimalSimplex);
            this.Controls.Add(this.TwoPhase);
            this.Controls.Add(this.btnStandard);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.dgvTable);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTable;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnStandard;
        private System.Windows.Forms.Button TwoPhase;
        private System.Windows.Forms.Button PrimalSimplex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDualSimplex;
        private System.Windows.Forms.Button btnRevisedSimplex;
        private System.Windows.Forms.Button btnSensitivity;
        private System.Windows.Forms.Button btnKnapsack;
        private System.Windows.Forms.Button btnCuttingPlane;
        private System.Windows.Forms.Button btnPrintTwoPhase;
        private System.Windows.Forms.Button btnPrintPrimal;
        private System.Windows.Forms.Button btnPrintDual;
        private System.Windows.Forms.Button btnPrintCuttingPlane;
        private System.Windows.Forms.Button btnBranchBound;
    }
}

