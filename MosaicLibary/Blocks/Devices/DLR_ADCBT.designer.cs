namespace MosaicLibary
{
    partial class cpDLR_ADCBT
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false..</param>
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
            this.cbConnectDisconnect = new System.Windows.Forms.CheckBox();
            this.smDaq = new MosaicLibary.ScopeMonitor();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // cbConnectDisconnect
            // 
            this.cbConnectDisconnect.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbConnectDisconnect.AutoSize = true;
            this.cbConnectDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbConnectDisconnect.Location = new System.Drawing.Point(18, 25);
            this.cbConnectDisconnect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbConnectDisconnect.Name = "cbConnectDisconnect";
            this.cbConnectDisconnect.Size = new System.Drawing.Size(146, 47);
            this.cbConnectDisconnect.TabIndex = 5;
            this.cbConnectDisconnect.Text = "Connect";
            this.cbConnectDisconnect.UseVisualStyleBackColor = true;
            this.cbConnectDisconnect.CheckedChanged += new System.EventHandler(this.cbConnectDisconnect_CheckedChanged);
            // 
            // smDaq
            // 
            this.smDaq.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smDaq.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smDaq.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smDaq.Location = new System.Drawing.Point(18, 83);
            this.smDaq.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smDaq.Name = "smDaq";
            this.smDaq.Size = new System.Drawing.Size(1099, 410);
            this.smDaq.TabIndex = 6;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDown1.Location = new System.Drawing.Point(298, 29);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(108, 44);
            this.numericUpDown1.TabIndex = 7;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(199, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 37);
            this.label1.TabIndex = 8;
            this.label1.Text = "COM";
            // 
            // cpDLR_ADCBT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 514);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.smDaq);
            this.Controls.Add(this.cbConnectDisconnect);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "cpDLR_ADCBT";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbConnectDisconnect;
        private MosaicLibary.ScopeMonitor smDaq;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
    }
}