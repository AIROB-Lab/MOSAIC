namespace MosaicLibary
{
    partial class cpHannesReceiver
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
            this.smHannesReceiver = new MosaicLibary.ScopeMonitor();
            this.cbConnectDisconnect = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // smHannesReceiver
            // 
            this.smHannesReceiver.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smHannesReceiver.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smHannesReceiver.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smHannesReceiver.Location = new System.Drawing.Point(16, 66);
            this.smHannesReceiver.Margin = new System.Windows.Forms.Padding(4);
            this.smHannesReceiver.Name = "smHannesReceiver";
            this.smHannesReceiver.Size = new System.Drawing.Size(977, 329);
            this.smHannesReceiver.TabIndex = 6;
            // 
            // cbConnectDisconnect
            // 
            this.cbConnectDisconnect.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbConnectDisconnect.AutoSize = true;
            this.cbConnectDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbConnectDisconnect.Location = new System.Drawing.Point(16, 20);
            this.cbConnectDisconnect.Margin = new System.Windows.Forms.Padding(4);
            this.cbConnectDisconnect.Name = "cbConnectDisconnect";
            this.cbConnectDisconnect.Size = new System.Drawing.Size(126, 41);
            this.cbConnectDisconnect.TabIndex = 5;
            this.cbConnectDisconnect.Text = "Connect";
            this.cbConnectDisconnect.UseVisualStyleBackColor = true;
            this.cbConnectDisconnect.CheckedChanged += new System.EventHandler(this.cbConnectDisconnect_CheckedChanged);
            // 
            // cpHannesReceiver
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 411);
            this.Controls.Add(this.smHannesReceiver);
            this.Controls.Add(this.cbConnectDisconnect);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "cpHannesReceiver";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MosaicLibary.ScopeMonitor smHannesReceiver;
        private System.Windows.Forms.CheckBox cbConnectDisconnect;
    }
}