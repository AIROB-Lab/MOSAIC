namespace MosaicLibary
{
    partial class cpHannes
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
            this.DeviceDropDown = new System.Windows.Forms.ComboBox();
            this.ScanDevices = new System.Windows.Forms.Button();
            this.ConnectHannesDevice = new System.Windows.Forms.Button();
            this.disconnectHannesDevice = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.COMPortUpDown = new System.Windows.Forms.NumericUpDown();
            this.dongleConnect = new System.Windows.Forms.Button();
            this.dongleDisconnect = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.smWristPS = new MosaicLibary.ScopeMonitor();
            this.smWristFE = new MosaicLibary.ScopeMonitor();
            this.smThumb = new MosaicLibary.ScopeMonitor();
            this.smHandOC = new MosaicLibary.ScopeMonitor();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.checkBoxWristFE = new System.Windows.Forms.CheckBox();
            this.checkBoxWristPS = new System.Windows.Forms.CheckBox();
            this.checkBoxHandOC = new System.Windows.Forms.CheckBox();
            this.checkBoxThumbRot = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.COMPortUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // DeviceDropDown
            // 
            this.DeviceDropDown.Location = new System.Drawing.Point(325, 117);
            this.DeviceDropDown.Name = "DeviceDropDown";
            this.DeviceDropDown.Size = new System.Drawing.Size(480, 28);
            this.DeviceDropDown.TabIndex = 0;
            this.DeviceDropDown.SelectedValueChanged += new System.EventHandler(this.DropDownHannesDevice_Changed);
            // 
            // ScanDevices
            // 
            this.ScanDevices.Location = new System.Drawing.Point(176, 110);
            this.ScanDevices.Name = "ScanDevices";
            this.ScanDevices.Size = new System.Drawing.Size(124, 40);
            this.ScanDevices.TabIndex = 7;
            this.ScanDevices.Text = "Scan Devices";
            this.ScanDevices.UseVisualStyleBackColor = true;
            this.ScanDevices.Click += new System.EventHandler(this.scanDevices_Click);
            // 
            // ConnectHannesDevice
            // 
            this.ConnectHannesDevice.Location = new System.Drawing.Point(826, 101);
            this.ConnectHannesDevice.Name = "ConnectHannesDevice";
            this.ConnectHannesDevice.Size = new System.Drawing.Size(124, 59);
            this.ConnectHannesDevice.TabIndex = 8;
            this.ConnectHannesDevice.Text = "Connect Device";
            this.ConnectHannesDevice.UseVisualStyleBackColor = true;
            this.ConnectHannesDevice.Click += new System.EventHandler(this.connectHannesDevice_Click);
            // 
            // disconnectHannesDevice
            // 
            this.disconnectHannesDevice.Location = new System.Drawing.Point(967, 101);
            this.disconnectHannesDevice.Name = "disconnectHannesDevice";
            this.disconnectHannesDevice.Size = new System.Drawing.Size(124, 59);
            this.disconnectHannesDevice.TabIndex = 10;
            this.disconnectHannesDevice.Text = "Disconnect Device";
            this.disconnectHannesDevice.UseVisualStyleBackColor = true;
            this.disconnectHannesDevice.Click += new System.EventHandler(this.disconnectHannesDevice_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "Hannes devices";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "Rehab Dongle";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(274, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "COM";
            // 
            // COMPortUpDown
            // 
            this.COMPortUpDown.Location = new System.Drawing.Point(325, 60);
            this.COMPortUpDown.Name = "COMPortUpDown";
            this.COMPortUpDown.Size = new System.Drawing.Size(60, 26);
            this.COMPortUpDown.TabIndex = 14;
            this.COMPortUpDown.ValueChanged += new System.EventHandler(this.COMPortUpDown_Changed);
            // 
            // dongleConnect
            // 
            this.dongleConnect.Location = new System.Drawing.Point(447, 43);
            this.dongleConnect.Name = "dongleConnect";
            this.dongleConnect.Size = new System.Drawing.Size(124, 59);
            this.dongleConnect.TabIndex = 15;
            this.dongleConnect.Text = "Connect Dongle";
            this.dongleConnect.UseVisualStyleBackColor = true;
            this.dongleConnect.Click += new System.EventHandler(this.dongleConnect_Click);
            // 
            // dongleDisconnect
            // 
            this.dongleDisconnect.Location = new System.Drawing.Point(600, 43);
            this.dongleDisconnect.Name = "dongleDisconnect";
            this.dongleDisconnect.Size = new System.Drawing.Size(124, 59);
            this.dongleDisconnect.TabIndex = 16;
            this.dongleDisconnect.Text = "Disconnect Dongle";
            this.dongleDisconnect.UseVisualStyleBackColor = true;
            this.dongleDisconnect.Click += new System.EventHandler(this.dongleDisconnect_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 926);
            this.splitter1.TabIndex = 27;
            this.splitter1.TabStop = false;
            // 
            // smWristPS
            // 
            this.smWristPS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smWristPS.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smWristPS.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smWristPS.Location = new System.Drawing.Point(38, 664);
            this.smWristPS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smWristPS.Name = "smWristPS";
            this.smWristPS.Size = new System.Drawing.Size(523, 258);
            this.smWristPS.TabIndex = 25;
            // 
            // smWristFE
            // 
            this.smWristFE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smWristFE.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smWristFE.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smWristFE.Location = new System.Drawing.Point(38, 347);
            this.smWristFE.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smWristFE.Name = "smWristFE";
            this.smWristFE.Size = new System.Drawing.Size(523, 256);
            this.smWristFE.TabIndex = 6;
            // 
            // smThumb
            // 
            this.smThumb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smThumb.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smThumb.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smThumb.Location = new System.Drawing.Point(630, 664);
            this.smThumb.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smThumb.Name = "smThumb";
            this.smThumb.Size = new System.Drawing.Size(529, 258);
            this.smThumb.TabIndex = 26;
            // 
            // smHandOC
            // 
            this.smHandOC.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smHandOC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smHandOC.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smHandOC.Location = new System.Drawing.Point(630, 347);
            this.smHandOC.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smHandOC.Name = "smHandOC";
            this.smHandOC.Size = new System.Drawing.Size(529, 256);
            this.smHandOC.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(626, 317);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 20);
            this.label4.TabIndex = 28;
            this.label4.Text = "Hand Open/Close";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(34, 317);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(173, 20);
            this.label5.TabIndex = 29;
            this.label5.Text = "Wrist Flexion/Extension";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(34, 634);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(197, 20);
            this.label6.TabIndex = 30;
            this.label6.Text = "Wrist Pronation/Supination";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(632, 634);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(123, 20);
            this.label7.TabIndex = 31;
            this.label7.Text = "Thumb Rotation";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(34, 272);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(110, 25);
            this.label12.TabIndex = 36;
            this.label12.Text = "Reference";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Enabled = false;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(33, 163);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(488, 38);
            this.label13.TabIndex = 37;
            this.label13.Text = "Activated Joints (Disabled, WIP)";
            // 
            // checkBoxWristFE
            // 
            this.checkBoxWristFE.AutoSize = true;
            this.checkBoxWristFE.Checked = true;
            this.checkBoxWristFE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWristFE.Enabled = false;
            this.checkBoxWristFE.Location = new System.Drawing.Point(55, 208);
            this.checkBoxWristFE.Name = "checkBoxWristFE";
            this.checkBoxWristFE.Size = new System.Drawing.Size(199, 24);
            this.checkBoxWristFE.TabIndex = 38;
            this.checkBoxWristFE.Text = "Wrist Flexion/Extension";
            this.checkBoxWristFE.UseVisualStyleBackColor = true;
            this.checkBoxWristFE.CheckedChanged += new System.EventHandler(this.checkBoxWristFE_CheckedChanged);
            // 
            // checkBoxWristPS
            // 
            this.checkBoxWristPS.AutoSize = true;
            this.checkBoxWristPS.Checked = true;
            this.checkBoxWristPS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWristPS.Enabled = false;
            this.checkBoxWristPS.Location = new System.Drawing.Point(293, 208);
            this.checkBoxWristPS.Name = "checkBoxWristPS";
            this.checkBoxWristPS.Size = new System.Drawing.Size(223, 24);
            this.checkBoxWristPS.TabIndex = 39;
            this.checkBoxWristPS.Text = "Wrist Pronation/Supination";
            this.checkBoxWristPS.UseVisualStyleBackColor = true;
            this.checkBoxWristPS.CheckedChanged += new System.EventHandler(this.checkBoxWristPS_CheckedChanged);
            // 
            // checkBoxHandOC
            // 
            this.checkBoxHandOC.AutoSize = true;
            this.checkBoxHandOC.Checked = true;
            this.checkBoxHandOC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHandOC.Enabled = false;
            this.checkBoxHandOC.Location = new System.Drawing.Point(563, 208);
            this.checkBoxHandOC.Name = "checkBoxHandOC";
            this.checkBoxHandOC.Size = new System.Drawing.Size(161, 24);
            this.checkBoxHandOC.TabIndex = 40;
            this.checkBoxHandOC.Text = "Hand Open/Close";
            this.checkBoxHandOC.UseVisualStyleBackColor = true;
            this.checkBoxHandOC.CheckedChanged += new System.EventHandler(this.checkBoxHandOC_CheckedChanged);
            // 
            // checkBoxThumbRot
            // 
            this.checkBoxThumbRot.AutoSize = true;
            this.checkBoxThumbRot.Checked = true;
            this.checkBoxThumbRot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThumbRot.Enabled = false;
            this.checkBoxThumbRot.Location = new System.Drawing.Point(801, 208);
            this.checkBoxThumbRot.Name = "checkBoxThumbRot";
            this.checkBoxThumbRot.Size = new System.Drawing.Size(149, 24);
            this.checkBoxThumbRot.TabIndex = 41;
            this.checkBoxThumbRot.Text = "Thumb Rotation";
            this.checkBoxThumbRot.UseVisualStyleBackColor = true;
            this.checkBoxThumbRot.CheckedChanged += new System.EventHandler(this.checkBoxThumbRot_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(33, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(122, 25);
            this.label8.TabIndex = 42;
            this.label8.Text = "Connection";
            // 
            // cpHannes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1310, 926);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.checkBoxThumbRot);
            this.Controls.Add(this.checkBoxHandOC);
            this.Controls.Add(this.checkBoxWristPS);
            this.Controls.Add(this.checkBoxWristFE);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.smThumb);
            this.Controls.Add(this.smWristPS);
            this.Controls.Add(this.smHandOC);
            this.Controls.Add(this.dongleDisconnect);
            this.Controls.Add(this.dongleConnect);
            this.Controls.Add(this.COMPortUpDown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.disconnectHannesDevice);
            this.Controls.Add(this.ConnectHannesDevice);
            this.Controls.Add(this.ScanDevices);
            this.Controls.Add(this.DeviceDropDown);
            this.Controls.Add(this.smWristFE);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "cpHannes";
            ((System.ComponentModel.ISupportInitialize)(this.COMPortUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox DeviceDropDown;
        private System.Windows.Forms.Button ScanDevices;
        private System.Windows.Forms.Button ConnectHannesDevice;
        private System.Windows.Forms.Button disconnectHannesDevice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown COMPortUpDown;
        private System.Windows.Forms.Button dongleConnect;
        private System.Windows.Forms.Button dongleDisconnect;
        private System.Windows.Forms.Splitter splitter1;
        private ScopeMonitor smWristPS;
        private ScopeMonitor smWristFE;
        private ScopeMonitor smThumb;
        private ScopeMonitor smHandOC;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox checkBoxWristFE;
        private System.Windows.Forms.CheckBox checkBoxWristPS;
        private System.Windows.Forms.CheckBox checkBoxHandOC;
        private System.Windows.Forms.CheckBox checkBoxThumbRot;
        private System.Windows.Forms.Label label8;
    }
}