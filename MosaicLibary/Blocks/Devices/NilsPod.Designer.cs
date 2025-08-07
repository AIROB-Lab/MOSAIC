namespace MosaicLibary
{
    partial class cpNilsPod
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
            this.smNilsPod = new MosaicLibary.ScopeMonitor();
            this.StartScan = new System.Windows.Forms.Button();
            this.StopScan = new System.Windows.Forms.Button();
            this.DeviceDropDown = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbConnectDisconnect
            // 
            this.cbConnectDisconnect.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbConnectDisconnect.AutoSize = true;
            this.cbConnectDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbConnectDisconnect.Location = new System.Drawing.Point(12, 12);
            this.cbConnectDisconnect.Name = "cbConnectDisconnect";
            this.cbConnectDisconnect.Size = new System.Drawing.Size(102, 35);
            this.cbConnectDisconnect.TabIndex = 5;
            this.cbConnectDisconnect.Text = "Connect";
            this.cbConnectDisconnect.UseVisualStyleBackColor = true;
            this.cbConnectDisconnect.CheckedChanged += new System.EventHandler(this.cbConnectDisconnect_CheckedChanged);
            // 
            // smNilsPod
            // 
            this.smNilsPod.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smNilsPod.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smNilsPod.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smNilsPod.Location = new System.Drawing.Point(12, 54);
            this.smNilsPod.Name = "smNilsPod";
            this.smNilsPod.Size = new System.Drawing.Size(734, 268);
            this.smNilsPod.TabIndex = 6;
            // 
            // StartScan
            // 
            this.StartScan.AutoSize = true;
            this.StartScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.StartScan.Location = new System.Drawing.Point(448, 16);
            this.StartScan.Margin = new System.Windows.Forms.Padding(2);
            this.StartScan.Name = "StartScan";
            this.StartScan.Size = new System.Drawing.Size(146, 35);
            this.StartScan.TabIndex = 7;
            this.StartScan.Text = "1. Start Scan";
            this.StartScan.UseVisualStyleBackColor = true;
            this.StartScan.Click += new System.EventHandler(this.StartScan_Click);
            // 
            // StopScan
            // 
            this.StopScan.AutoSize = true;
            this.StopScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.StopScan.Location = new System.Drawing.Point(605, 18);
            this.StopScan.Margin = new System.Windows.Forms.Padding(2);
            this.StopScan.Name = "StopScan";
            this.StopScan.Size = new System.Drawing.Size(145, 35);
            this.StopScan.TabIndex = 8;
            this.StopScan.Text = "2. Stop Scan";
            this.StopScan.UseVisualStyleBackColor = true;
            this.StopScan.Click += new System.EventHandler(this.StopScan_Click);
            // 
            // DeviceDropDown
            // 
            this.DeviceDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeviceDropDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeviceDropDown.FormattingEnabled = true;
            this.DeviceDropDown.Location = new System.Drawing.Point(146, 16);
            this.DeviceDropDown.Margin = new System.Windows.Forms.Padding(2);
            this.DeviceDropDown.Name = "DeviceDropDown";
            this.DeviceDropDown.Size = new System.Drawing.Size(234, 25);
            this.DeviceDropDown.TabIndex = 9;
            this.DeviceDropDown.SelectedIndexChanged += new System.EventHandler(this.DropDown_SelectedChanged);
            this.DeviceDropDown.Click += new System.EventHandler(this.DropDownClickEvent);
            // 
            // cpNilsPod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 334);
            this.Controls.Add(this.DeviceDropDown);
            this.Controls.Add(this.StopScan);
            this.Controls.Add(this.StartScan);
            this.Controls.Add(this.smNilsPod);
            this.Controls.Add(this.cbConnectDisconnect);
            this.Name = "cpNilsPod";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbConnectDisconnect;
        private MosaicLibary.ScopeMonitor smNilsPod;
        private System.Windows.Forms.Button StartScan;
        private System.Windows.Forms.Button StopScan;
        private System.Windows.Forms.ComboBox DeviceDropDown;
    }
}