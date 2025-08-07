namespace MosaicLibary
{
    partial class cpBatchPredictor
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
            this.btnBuildModel = new System.Windows.Forms.Button();
            this.btnResetModel = new System.Windows.Forms.Button();
            this.lblConfidence = new System.Windows.Forms.Label();
            this.pbConfidence = new MosaicLibary.SimpleProgressBar();
            this.SuspendLayout();
            // 
            // smILM
            // 
            this.smILM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smILM.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smILM.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smILM.Location = new System.Drawing.Point(12, 83);
            this.smILM.Name = "smILM";
            this.smILM.Size = new System.Drawing.Size(455, 264);
            this.smILM.TabIndex = 0;
            // 
            // btnBuildModel
            // 
            this.btnBuildModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuildModel.Location = new System.Drawing.Point(12, 12);
            this.btnBuildModel.Name = "btnBuildModel";
            this.btnBuildModel.Size = new System.Drawing.Size(174, 40);
            this.btnBuildModel.TabIndex = 1;
            this.btnBuildModel.Text = "Build model";
            this.btnBuildModel.UseVisualStyleBackColor = true;
            this.btnBuildModel.Click += new System.EventHandler(this.btnBuildModel_Click);
            // 
            // btnResetModel
            // 
            this.btnResetModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetModel.Location = new System.Drawing.Point(192, 12);
            this.btnResetModel.Name = "btnResetModel";
            this.btnResetModel.Size = new System.Drawing.Size(174, 40);
            this.btnResetModel.TabIndex = 2;
            this.btnResetModel.Text = "Reset model";
            this.btnResetModel.UseVisualStyleBackColor = true;
            this.btnResetModel.Click += new System.EventHandler(this.btnResetModel_Click);
            // 
            // lblConfidence
            // 
            this.lblConfidence.AutoSize = true;
            this.lblConfidence.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfidence.Location = new System.Drawing.Point(7, 55);
            this.lblConfidence.Name = "lblConfidence";
            this.lblConfidence.Size = new System.Drawing.Size(127, 25);
            this.lblConfidence.TabIndex = 3;
            this.lblConfidence.Text = "Confidence:";
            this.lblConfidence.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbConfidence
            // 
            this.pbConfidence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbConfidence.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbConfidence.ForeColor = System.Drawing.Color.Red;
            this.pbConfidence.Location = new System.Drawing.Point(131, 57);
            this.pbConfidence.Name = "pbConfidence";
            this.pbConfidence.Size = new System.Drawing.Size(336, 23);
            this.pbConfidence.TabIndex = 9;
            this.pbConfidence.Value = 0D;
            this.pbConfidence.Vertical = false;
            // 
            // cpBatchPredictor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 370);
            this.Controls.Add(this.pbConfidence);
            this.Controls.Add(this.lblConfidence);
            this.Controls.Add(this.btnResetModel);
            this.Controls.Add(this.btnBuildModel);
            this.Controls.Add(this.smILM);
            this.Name = "cpBatchPredictor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MosaicLibary.ScopeMonitor smILM;
        private System.Windows.Forms.Button btnBuildModel;
        private System.Windows.Forms.Button btnResetModel;
        private System.Windows.Forms.Label lblConfidence;
        private SimpleProgressBar pbConfidence;
    }
}