namespace MosaicLibary
{
    partial class cpResampler
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
            this.smResampler = new MosaicLibary.ScopeMonitor();
            this.spResampler = new MosaicLibary.SpiderPlotMonitor();
            this.SuspendLayout();
            // 
            // smResampler
            // 
            this.smResampler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.smResampler.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smResampler.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smResampler.Location = new System.Drawing.Point(18, 18);
            this.smResampler.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smResampler.Name = "smResampler";
            this.smResampler.Size = new System.Drawing.Size(690, 370);
            this.smResampler.TabIndex = 0;
            // 
            // spResampler
            // 
            this.spResampler.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spResampler.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spResampler.Location = new System.Drawing.Point(716, 18);
            this.spResampler.Name = "spResampler";
            this.spResampler.Size = new System.Drawing.Size(414, 370);
            this.spResampler.TabIndex = 1;
            // 
            // cpResampler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 409);
            this.Controls.Add(this.spResampler);
            this.Controls.Add(this.smResampler);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "cpResampler";
            this.ResumeLayout(false);

        }

        #endregion

        private ScopeMonitor smResampler;
        private SpiderPlotMonitor spResampler;
    }
}