namespace MosaicLibary
{
    partial class cpScheduledTimer
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
            this.cbStartStop = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbStartStop
            // 
            this.cbStartStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStartStop.AutoSize = true;
            this.cbStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbStartStop.Location = new System.Drawing.Point(102, 11);
            this.cbStartStop.Name = "cbStartStop";
            this.cbStartStop.Size = new System.Drawing.Size(67, 35);
            this.cbStartStop.TabIndex = 0;
            this.cbStartStop.Text = "Start";
            this.cbStartStop.UseVisualStyleBackColor = true;
            this.cbStartStop.CheckedChanged += new System.EventHandler(this.cbStartStop_CheckedChanged);
            // 
            // cpScheduledTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 58);
            this.Controls.Add(this.cbStartStop);
            this.Name = "cpScheduledTimer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbStartStop;
    }
}