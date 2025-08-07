namespace MosaicLibary
{
    partial class SensorItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sensorTypeCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.sensorNumber = new System.Windows.Forms.Label();
            this.sensorId = new System.Windows.Forms.Label();
            this.cbSensorModes = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // sensorTypeCheckBox
            // 
            this.sensorTypeCheckBox.AutoSize = true;
            this.sensorTypeCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.sensorTypeCheckBox.Location = new System.Drawing.Point(31, 23);
            this.sensorTypeCheckBox.Name = "sensorTypeCheckBox";
            this.sensorTypeCheckBox.Size = new System.Drawing.Size(145, 30);
            this.sensorTypeCheckBox.TabIndex = 0;
            this.sensorTypeCheckBox.Text = "checkBox1";
            this.sensorTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label1.Location = new System.Drawing.Point(27, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "Mode:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label2.Location = new System.Drawing.Point(27, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(171, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "Sensor Number:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label3.Location = new System.Drawing.Point(27, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 26);
            this.label3.TabIndex = 3;
            this.label3.Text = "Sensor ID:";
            // 
            // sensorNumber
            // 
            this.sensorNumber.AutoSize = true;
            this.sensorNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.sensorNumber.Location = new System.Drawing.Point(295, 105);
            this.sensorNumber.Name = "sensorNumber";
            this.sensorNumber.Size = new System.Drawing.Size(70, 26);
            this.sensorNumber.TabIndex = 4;
            this.sensorNumber.Text = "label4";
            // 
            // sensorId
            // 
            this.sensorId.AutoSize = true;
            this.sensorId.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.sensorId.Location = new System.Drawing.Point(295, 147);
            this.sensorId.Name = "sensorId";
            this.sensorId.Size = new System.Drawing.Size(70, 26);
            this.sensorId.TabIndex = 5;
            this.sensorId.Text = "label5";
            // 
            // cbSensorModes
            // 
            this.cbSensorModes.AllowDrop = true;
            this.cbSensorModes.BackColor = System.Drawing.Color.AliceBlue;
            this.cbSensorModes.DropDownWidth = 700;
            this.cbSensorModes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cbSensorModes.FormattingEnabled = true;
            this.cbSensorModes.ItemHeight = 25;
            this.cbSensorModes.Location = new System.Drawing.Point(114, 64);
            this.cbSensorModes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbSensorModes.Name = "cbSensorModes";
            this.cbSensorModes.Size = new System.Drawing.Size(279, 33);
            this.cbSensorModes.TabIndex = 6;
            this.cbSensorModes.Text = "Select Mode";
            this.cbSensorModes.SelectedIndexChanged += new System.EventHandler(this.cbSensorModes_SelectedIndexChanged);
            // 
            // SensorItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.cbSensorModes);
            this.Controls.Add(this.sensorId);
            this.Controls.Add(this.sensorNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sensorTypeCheckBox);
            this.Name = "SensorItem";
            this.Size = new System.Drawing.Size(446, 188);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox sensorTypeCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label sensorNumber;
        private System.Windows.Forms.Label sensorId;
        private System.Windows.Forms.ComboBox cbSensorModes;
    }
}
