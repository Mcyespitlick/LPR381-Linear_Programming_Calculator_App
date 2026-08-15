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
            this.dgvTable = new System.Windows.Forms.DataGridView();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnStandard = new System.Windows.Forms.Button();
            this.btnCanonical = new System.Windows.Forms.Button();
            this.TwoPhase = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvTable
            // 
            this.dgvTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTable.Location = new System.Drawing.Point(244, 59);
            this.dgvTable.Name = "dgvTable";
            this.dgvTable.Size = new System.Drawing.Size(1121, 358);
            this.dgvTable.TabIndex = 0;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(13, 393);
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
            this.btnStandard.Location = new System.Drawing.Point(13, 141);
            this.btnStandard.Name = "btnStandard";
            this.btnStandard.Size = new System.Drawing.Size(186, 23);
            this.btnStandard.TabIndex = 3;
            this.btnStandard.Text = "Standard Form";
            this.btnStandard.UseVisualStyleBackColor = true;
            this.btnStandard.Click += new System.EventHandler(this.btnCanonical_Click);
            // 
            // btnCanonical
            // 
            this.btnCanonical.Location = new System.Drawing.Point(13, 89);
            this.btnCanonical.Name = "btnCanonical";
            this.btnCanonical.Size = new System.Drawing.Size(186, 23);
            this.btnCanonical.TabIndex = 4;
            this.btnCanonical.Text = "To canonical";
            this.btnCanonical.UseVisualStyleBackColor = true;
            this.btnCanonical.Click += new System.EventHandler(this.btnCanonical_Click_1);
            // 
            // TwoPhase
            // 
            this.TwoPhase.Location = new System.Drawing.Point(12, 170);
            this.TwoPhase.Name = "TwoPhase";
            this.TwoPhase.Size = new System.Drawing.Size(187, 23);
            this.TwoPhase.TabIndex = 5;
            this.TwoPhase.Text = "Two-Phase";
            this.TwoPhase.UseVisualStyleBackColor = true;
            this.TwoPhase.Click += new System.EventHandler(this.TwoPhase_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1397, 450);
            this.Controls.Add(this.TwoPhase);
            this.Controls.Add(this.btnCanonical);
            this.Controls.Add(this.btnStandard);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.dgvTable);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTable;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnLoadData;
        private System.Windows.Forms.Button btnStandard;
        private System.Windows.Forms.Button btnCanonical;
        private System.Windows.Forms.Button TwoPhase;
    }
}

