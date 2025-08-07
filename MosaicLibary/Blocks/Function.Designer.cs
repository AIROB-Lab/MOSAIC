namespace MosaicLibary
{
    partial class cpFunction
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
            this.smFunction = new MosaicLibary.ScopeMonitor();
            this.spFunction = new MosaicLibary.SpiderPlotMonitor();
            this.SuspendLayout();
            // 
            // smFunction
            // 
            this.smFunction.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smFunction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smFunction.Location = new System.Drawing.Point(12, 12);
            this.smFunction.Name = "smFunction";
            this.smFunction.Size = new System.Drawing.Size(406, 242);
            this.smFunction.TabIndex = 0;
            // 
            // spFunction
            // 
            this.spFunction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spFunction.Location = new System.Drawing.Point(438, 13);
            this.spFunction.Name = "spFunction";
            this.spFunction.Size = new System.Drawing.Size(284, 240);
            this.spFunction.TabIndex = 1;
            // 
            // cpFunction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 266);
            this.Controls.Add(this.spFunction);
            this.Controls.Add(this.smFunction);
            this.Name = "cpFunction";
            this.ResumeLayout(false);

        }

        #endregion

        private ScopeMonitor smFunction;
        private SpiderPlotMonitor spFunction;
    }
}