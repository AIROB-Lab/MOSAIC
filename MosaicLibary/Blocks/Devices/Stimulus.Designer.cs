namespace MosaicLibary
{
    partial class cpStimulus
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
            this.btnStart = new System.Windows.Forms.Button();
            this.lblTargetValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStimulus
            // 
            this.lblStimulus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStimulus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStimulus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStimulus.Location = new System.Drawing.Point(91, 9);
            this.lblStimulus.Name = "lblStimulus";
            this.lblStimulus.Size = new System.Drawing.Size(170, 36);
            this.lblStimulus.TabIndex = 0;
            this.lblStimulus.Text = "Stimulus";
            this.lblStimulus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(4, 9);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(81, 37);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblTargetValue
            // 
            this.lblTargetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTargetValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTargetValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetValue.Location = new System.Drawing.Point(4, 49);
            this.lblTargetValue.Name = "lblTargetValue";
            this.lblTargetValue.Size = new System.Drawing.Size(257, 46);
            this.lblTargetValue.TabIndex = 2;
            this.lblTargetValue.Text = "Stimulus";
            this.lblTargetValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cpStimulus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 104);
            this.Controls.Add(this.lblTargetValue);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblStimulus);
            this.Name = "cpStimulus";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblStimulus;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblTargetValue;
    }
}