namespace iM
{
    partial class CpChannelSelector
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
            this._scope = new iM.ScopeMonitor();
            this.radioButtonSignalZero = new System.Windows.Forms.RadioButton();
            this.radioButtonSignalDelete = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // _scope
            // 
            this._scope.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._scope.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._scope.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._scope.Location = new System.Drawing.Point(210, 99);
            this._scope.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._scope.Name = "_scope";
            this._scope.Size = new System.Drawing.Size(1385, 629);
            this._scope.TabIndex = 17;
            // 
            // radioButtonSignalZero
            // 
            this.radioButtonSignalZero.Checked = true;
            this.radioButtonSignalZero.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSignalZero.Location = new System.Drawing.Point(254, 22);
            this.radioButtonSignalZero.Name = "radioButtonSignalZero";
            this.radioButtonSignalZero.Size = new System.Drawing.Size(392, 54);
            this.radioButtonSignalZero.TabIndex = 18;
            this.radioButtonSignalZero.TabStop = true;
            this.radioButtonSignalZero.Text = "Set Channel to zero";
            this.radioButtonSignalZero.UseVisualStyleBackColor = true;
            this.radioButtonSignalZero.CheckedChanged += new System.EventHandler(this.radioButtonSignalZero_CheckedChanged);
            // 
            // radioButtonSignalDelete
            // 
            this.radioButtonSignalDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonSignalDelete.Location = new System.Drawing.Point(688, 22);
            this.radioButtonSignalDelete.Name = "radioButtonSignalDelete";
            this.radioButtonSignalDelete.Size = new System.Drawing.Size(360, 54);
            this.radioButtonSignalDelete.TabIndex = 19;
            this.radioButtonSignalDelete.Text = "Turn off Channel";
            this.radioButtonSignalDelete.UseVisualStyleBackColor = true;
            this.radioButtonSignalDelete.CheckedChanged += new System.EventHandler(this.radioButtonSignalDelete_CheckedChanged);
            // 
            // CpChannelSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1626, 760);
            this.Controls.Add(this.radioButtonSignalDelete);
            this.Controls.Add(this.radioButtonSignalZero);
            this.Controls.Add(this._scope);
            this.Name = "CpChannelSelector";
            this.Text = "ChannelSelector";
            this.ResumeLayout(false);

        }

        #endregion

        private ScopeMonitor _scope;
        private System.Windows.Forms.RadioButton radioButtonSignalZero;
        private System.Windows.Forms.RadioButton radioButtonSignalDelete;
    }
}