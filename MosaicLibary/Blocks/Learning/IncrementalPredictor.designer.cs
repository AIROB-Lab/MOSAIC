namespace MosaicLibary
{
    partial class cpIncrementalPredictor
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
            this.smILM = new MosaicLibary.ScopeMonitor();
            this.lblConfidence = new System.Windows.Forms.Label();
            this.btnResetModel = new System.Windows.Forms.Button();
            this.pbConfidence = new MosaicLibary.SimpleProgressBar();
            this.btnLoadModel = new System.Windows.Forms.Button();
            this.btnSaveModel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // smILM
            // 
            this.smILM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smILM.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smILM.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smILM.Location = new System.Drawing.Point(12, 70);
            this.smILM.Name = "smILM";
            this.smILM.Size = new System.Drawing.Size(536, 303);
            this.smILM.TabIndex = 0;
            // 
            // lblConfidence
            // 
            this.lblConfidence.AutoSize = true;
            this.lblConfidence.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfidence.Location = new System.Drawing.Point(8, 42);
            this.lblConfidence.Name = "lblConfidence";
            this.lblConfidence.Size = new System.Drawing.Size(100, 20);
            this.lblConfidence.TabIndex = 6;
            this.lblConfidence.Text = "Confidence";
            // 
            // btnResetModel
            // 
            this.btnResetModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetModel.Location = new System.Drawing.Point(68, 6);
            this.btnResetModel.Name = "btnResetModel";
            this.btnResetModel.Size = new System.Drawing.Size(93, 30);
            this.btnResetModel.TabIndex = 5;
            this.btnResetModel.Text = "Reset";
            this.btnResetModel.UseVisualStyleBackColor = true;
            this.btnResetModel.Click += new System.EventHandler(this.btnResetModel_Click);
            // 
            // pbConfidence
            // 
            this.pbConfidence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbConfidence.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbConfidence.ForeColor = System.Drawing.Color.Red;
            this.pbConfidence.Location = new System.Drawing.Point(110, 42);
            this.pbConfidence.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbConfidence.Name = "pbConfidence";
            this.pbConfidence.Size = new System.Drawing.Size(437, 23);
            this.pbConfidence.TabIndex = 8;
            this.pbConfidence.Value = 0D;
            this.pbConfidence.Vertical = false;
            // 
            // btnLoadModel
            // 
            this.btnLoadModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadModel.Location = new System.Drawing.Point(167, 6);
            this.btnLoadModel.Name = "btnLoadModel";
            this.btnLoadModel.Size = new System.Drawing.Size(101, 30);
            this.btnLoadModel.TabIndex = 5;
            this.btnLoadModel.Text = "Load";
            this.btnLoadModel.UseVisualStyleBackColor = true;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            // 
            // btnSaveModel
            // 
            this.btnSaveModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveModel.Location = new System.Drawing.Point(274, 6);
            this.btnSaveModel.Name = "btnSaveModel";
            this.btnSaveModel.Size = new System.Drawing.Size(95, 30);
            this.btnSaveModel.TabIndex = 5;
            this.btnSaveModel.Text = "Save";
            this.btnSaveModel.UseVisualStyleBackColor = true;
            this.btnSaveModel.Click += new System.EventHandler(this.btnSaveModel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "Model";
            // 
            // cpIncrementalPredictor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 385);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbConfidence);
            this.Controls.Add(this.lblConfidence);
            this.Controls.Add(this.btnSaveModel);
            this.Controls.Add(this.btnLoadModel);
            this.Controls.Add(this.btnResetModel);
            this.Controls.Add(this.smILM);
            this.Name = "cpIncrementalPredictor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MosaicLibary.ScopeMonitor smILM;
        private System.Windows.Forms.Label lblConfidence;
        private System.Windows.Forms.Button btnResetModel;
        private SimpleProgressBar pbConfidence;
        private System.Windows.Forms.Button btnLoadModel;
        private System.Windows.Forms.Button btnSaveModel;
        private System.Windows.Forms.Label label1;
    }
}