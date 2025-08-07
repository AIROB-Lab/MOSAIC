namespace MosaicLibary
{
    partial class cpSinGenerator
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
            this.smSinGenerator = new MosaicLibary.ScopeMonitor();
            this.SuspendLayout();
            // 
            // smSinGenerator
            // 
            this.smSinGenerator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smSinGenerator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smSinGenerator.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smSinGenerator.Location = new System.Drawing.Point(12, 12);
            this.smSinGenerator.Name = "smSinGenerator";
            this.smSinGenerator.Size = new System.Drawing.Size(473, 246);
            this.smSinGenerator.TabIndex = 0;
            // 
            // cpSinGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 270);
            this.Controls.Add(this.smSinGenerator);
            this.Name = "cpSinGenerator";
            this.ResumeLayout(false);

        }

        #endregion

        private MosaicLibary.ScopeMonitor smSinGenerator;
    }
}