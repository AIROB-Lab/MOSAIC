namespace MosaicLibary
{
    partial class cpFilter
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
            this.smFilter = new MosaicLibary.ScopeMonitor();
            this.SuspendLayout();
            // 
            // smFilter
            // 
            this.smFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smFilter.Location = new System.Drawing.Point(12, 12);
            this.smFilter.Name = "smFilter";
            this.smFilter.Size = new System.Drawing.Size(461, 242);
            this.smFilter.TabIndex = 0;
            // 
            // cpFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 266);
            this.Controls.Add(this.smFilter);
            this.Name = "cpFilter";
            this.ResumeLayout(false);

        }

        #endregion

        private ScopeMonitor smFilter;
    }
}