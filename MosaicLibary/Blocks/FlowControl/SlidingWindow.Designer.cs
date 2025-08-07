namespace MosaicLibary
{
    partial class cpSlidingWindow
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.scopeMonitor1 = new MosaicLibary.ScopeMonitor();
            this.dropDownTypes = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.Black;
            this.lblTitle.Location = new System.Drawing.Point(18, 14);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(167, 29);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Window Type:";
            // 
            // scopeMonitor1
            // 
            this.scopeMonitor1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scopeMonitor1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scopeMonitor1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scopeMonitor1.Location = new System.Drawing.Point(18, 78);
            this.scopeMonitor1.Name = "scopeMonitor1";
            this.scopeMonitor1.Size = new System.Drawing.Size(924, 388);
            this.scopeMonitor1.TabIndex = 2;
            // 
            // dropDownTypes
            // 
            this.dropDownTypes.BackColor = System.Drawing.Color.LightGray;
            this.dropDownTypes.FormattingEnabled = true;
            this.dropDownTypes.Location = new System.Drawing.Point(192, 18);
            this.dropDownTypes.Name = "dropDownTypes";
            this.dropDownTypes.Size = new System.Drawing.Size(363, 28);
            this.dropDownTypes.TabIndex = 3;
            this.dropDownTypes.SelectedIndexChanged += new System.EventHandler(this.dropDownTypes_SelectedIndexChanged);
            // 
            // cpSlidingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 489);
            this.Controls.Add(this.dropDownTypes);
            this.Controls.Add(this.scopeMonitor1);
            this.Controls.Add(this.lblTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "cpSlidingWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblTitle;
        private ScopeMonitor scopeMonitor1;
        private System.Windows.Forms.ComboBox dropDownTypes;
    }
}