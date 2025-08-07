namespace MosaicLibary
{
    partial class CpDelsys
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
            this.button1 = new System.Windows.Forms.Button();
            this.buttonScanSensors = new System.Windows.Forms.Button();
            this.sensorList = new System.Windows.Forms.FlowLayoutPanel();
            this.smDelsys = new MosaicLibary.ScopeMonitor();
            this.btnStream = new System.Windows.Forms.CheckBox();
            this.DeviceInfo = new System.Windows.Forms.GroupBox();
            this.framesCollected = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.packetsLost = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.streamTime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.totalChannels = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.sensorsConnected = new System.Windows.Forms.Label();
            this.pipelineStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.deviceName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.DeviceInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(181, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 67);
            this.button1.TabIndex = 0;
            this.button1.Text = "Arm";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonArmPipeline_Click);
            // 
            // buttonScanSensors
            // 
            this.buttonScanSensors.Location = new System.Drawing.Point(12, 12);
            this.buttonScanSensors.Name = "buttonScanSensors";
            this.buttonScanSensors.Size = new System.Drawing.Size(150, 65);
            this.buttonScanSensors.TabIndex = 1;
            this.buttonScanSensors.Text = "Scan";
            this.buttonScanSensors.UseVisualStyleBackColor = true;
            this.buttonScanSensors.Click += new System.EventHandler(this.buttonScanSensors_Click);
            // 
            // sensorList
            // 
            this.sensorList.AutoScroll = true;
            this.sensorList.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.sensorList.Location = new System.Drawing.Point(603, 39);
            this.sensorList.Name = "sensorList";
            this.sensorList.Size = new System.Drawing.Size(468, 873);
            this.sensorList.TabIndex = 2;
            // 
            // smDelsys
            // 
            this.smDelsys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smDelsys.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smDelsys.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smDelsys.Location = new System.Drawing.Point(1087, 39);
            this.smDelsys.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smDelsys.Name = "smDelsys";
            this.smDelsys.Size = new System.Drawing.Size(808, 873);
            this.smDelsys.TabIndex = 3;
            // 
            // btnStream
            // 
            this.btnStream.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnStream.AutoSize = true;
            this.btnStream.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnStream.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnStream.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnStream.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStream.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStream.Location = new System.Drawing.Point(181, 759);
            this.btnStream.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStream.Name = "btnStream";
            this.btnStream.Size = new System.Drawing.Size(130, 47);
            this.btnStream.TabIndex = 14;
            this.btnStream.Text = "Stream";
            this.btnStream.UseVisualStyleBackColor = false;
            this.btnStream.CheckedChanged += new System.EventHandler(this.btnStream_CheckedChanged);
            // 
            // DeviceInfo
            // 
            this.DeviceInfo.Controls.Add(this.framesCollected);
            this.DeviceInfo.Controls.Add(this.label6);
            this.DeviceInfo.Controls.Add(this.packetsLost);
            this.DeviceInfo.Controls.Add(this.label7);
            this.DeviceInfo.Controls.Add(this.streamTime);
            this.DeviceInfo.Controls.Add(this.label5);
            this.DeviceInfo.Controls.Add(this.totalChannels);
            this.DeviceInfo.Controls.Add(this.label3);
            this.DeviceInfo.Controls.Add(this.label4);
            this.DeviceInfo.Controls.Add(this.sensorsConnected);
            this.DeviceInfo.Controls.Add(this.pipelineStatus);
            this.DeviceInfo.Controls.Add(this.label2);
            this.DeviceInfo.Controls.Add(this.deviceName);
            this.DeviceInfo.Controls.Add(this.label1);
            this.DeviceInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.DeviceInfo.Location = new System.Drawing.Point(13, 276);
            this.DeviceInfo.Name = "DeviceInfo";
            this.DeviceInfo.Size = new System.Drawing.Size(570, 379);
            this.DeviceInfo.TabIndex = 15;
            this.DeviceInfo.TabStop = false;
            this.DeviceInfo.Text = "Device Information";
            // 
            // framesCollected
            // 
            this.framesCollected.AutoSize = true;
            this.framesCollected.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.framesCollected.Location = new System.Drawing.Point(344, 308);
            this.framesCollected.Name = "framesCollected";
            this.framesCollected.Size = new System.Drawing.Size(26, 29);
            this.framesCollected.TabIndex = 13;
            this.framesCollected.Text = "0";
            this.framesCollected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label6.Location = new System.Drawing.Point(22, 308);
            this.label6.MaximumSize = new System.Drawing.Size(240, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(210, 29);
            this.label6.TabIndex = 12;
            this.label6.Text = "Frames Collected:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // packetsLost
            // 
            this.packetsLost.AutoSize = true;
            this.packetsLost.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.packetsLost.Location = new System.Drawing.Point(344, 266);
            this.packetsLost.Name = "packetsLost";
            this.packetsLost.Size = new System.Drawing.Size(26, 29);
            this.packetsLost.TabIndex = 11;
            this.packetsLost.Text = "0";
            this.packetsLost.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label7.Location = new System.Drawing.Point(22, 266);
            this.label7.MaximumSize = new System.Drawing.Size(175, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(155, 29);
            this.label7.TabIndex = 10;
            this.label7.Text = "Packets Lost:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // streamTime
            // 
            this.streamTime.AutoSize = true;
            this.streamTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.streamTime.Location = new System.Drawing.Point(344, 224);
            this.streamTime.Name = "streamTime";
            this.streamTime.Size = new System.Drawing.Size(63, 29);
            this.streamTime.TabIndex = 9;
            this.streamTime.Text = "0.0 s";
            this.streamTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label5.Location = new System.Drawing.Point(22, 224);
            this.label5.MaximumSize = new System.Drawing.Size(160, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(158, 29);
            this.label5.TabIndex = 8;
            this.label5.Text = "Stream Time:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // totalChannels
            // 
            this.totalChannels.AutoSize = true;
            this.totalChannels.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.totalChannels.Location = new System.Drawing.Point(344, 183);
            this.totalChannels.Name = "totalChannels";
            this.totalChannels.Size = new System.Drawing.Size(26, 29);
            this.totalChannels.TabIndex = 7;
            this.totalChannels.Text = "0";
            this.totalChannels.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(22, 183);
            this.label3.MaximumSize = new System.Drawing.Size(200, 0);
            this.label3.MinimumSize = new System.Drawing.Size(120, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 29);
            this.label3.TabIndex = 6;
            this.label3.Text = "Total Channels:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label4.Location = new System.Drawing.Point(22, 140);
            this.label4.MaximumSize = new System.Drawing.Size(240, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(231, 29);
            this.label4.TabIndex = 5;
            this.label4.Text = "Sensors Connected:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sensorsConnected
            // 
            this.sensorsConnected.AutoSize = true;
            this.sensorsConnected.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.sensorsConnected.Location = new System.Drawing.Point(344, 140);
            this.sensorsConnected.Name = "sensorsConnected";
            this.sensorsConnected.Size = new System.Drawing.Size(26, 29);
            this.sensorsConnected.TabIndex = 4;
            this.sensorsConnected.Text = "0";
            this.sensorsConnected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pipelineStatus
            // 
            this.pipelineStatus.AutoSize = true;
            this.pipelineStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.pipelineStatus.Location = new System.Drawing.Point(344, 97);
            this.pipelineStatus.Name = "pipelineStatus";
            this.pipelineStatus.Size = new System.Drawing.Size(53, 29);
            this.pipelineStatus.TabIndex = 3;
            this.pipelineStatus.Text = "N/A";
            this.pipelineStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.Location = new System.Drawing.Point(22, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 29);
            this.label2.TabIndex = 2;
            this.label2.Text = "Pipeline Status:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // deviceName
            // 
            this.deviceName.AutoSize = true;
            this.deviceName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.deviceName.Location = new System.Drawing.Point(344, 59);
            this.deviceName.Name = "deviceName";
            this.deviceName.Size = new System.Drawing.Size(53, 29);
            this.deviceName.TabIndex = 1;
            this.deviceName.Text = "N/A";
            this.deviceName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(22, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device Name:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(12, 94);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(150, 67);
            this.buttonReset.TabIndex = 16;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(181, 94);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(150, 67);
            this.buttonExport.TabIndex = 17;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(603, 9);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(115, 24);
            this.checkBox1.TabIndex = 18;
            this.checkBox1.Text = "All Sensors";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // CpDelsys
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 940);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.DeviceInfo);
            this.Controls.Add(this.btnStream);
            this.Controls.Add(this.smDelsys);
            this.Controls.Add(this.sensorList);
            this.Controls.Add(this.buttonScanSensors);
            this.Controls.Add(this.button1);
            this.Name = "CpDelsys";
            this.Text = "Delsys";
            this.DeviceInfo.ResumeLayout(false);
            this.DeviceInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonScanSensors;
        private System.Windows.Forms.FlowLayoutPanel sensorList;
        private ScopeMonitor smDelsys;
        private System.Windows.Forms.CheckBox btnStream;
        private System.Windows.Forms.GroupBox DeviceInfo;
        private System.Windows.Forms.Label deviceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label sensorsConnected;
        private System.Windows.Forms.Label pipelineStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label packetsLost;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label streamTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label totalChannels;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label framesCollected;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}