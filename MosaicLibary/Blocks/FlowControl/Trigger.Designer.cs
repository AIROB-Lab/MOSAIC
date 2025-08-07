namespace MosaicLibary
{
    partial class cpTrigger
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
            this.lblStimulus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStimulus
            // 
            this.lblStimulus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStimulus.AutoSize = true;
            this.lblStimulus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStimulus.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStimulus.Location = new System.Drawing.Point(13, 13);
            this.lblStimulus.Name = "lblStimulus";
            this.lblStimulus.Size = new System.Drawing.Size(67, 33);
            this.lblStimulus.TabIndex = 0;
            this.lblStimulus.Text = "cmd";
            this.lblStimulus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cpTrigger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 70);
            this.Controls.Add(this.lblStimulus);
            this.Name = "cpTrigger";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cpTriggerKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cpTriggerKeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStimulus;
    }
}