namespace MosaicLibary
{
    public partial class ScopeMonitor
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.xScale = new System.Windows.Forms.TrackBar();
            this.yScale = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.xScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScale)).BeginInit();
            this.SuspendLayout();
            // 
            // xScale
            // 
            this.xScale.AutoSize = false;
            this.xScale.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.xScale.Location = new System.Drawing.Point(0, 238);
            this.xScale.Maximum = 100;
            this.xScale.Minimum = 5;
            this.xScale.Name = "xScale";
            this.xScale.Size = new System.Drawing.Size(311, 30);
            this.xScale.TabIndex = 1;
            this.xScale.TickFrequency = 10;
            this.xScale.Value = 20;
            this.xScale.ValueChanged += new System.EventHandler(this.xScale_ValueChanged);
            // 
            // yScale
            // 
            this.yScale.Dock = System.Windows.Forms.DockStyle.Left;
            this.yScale.Location = new System.Drawing.Point(0, 0);
            this.yScale.Maximum = 300;
            this.yScale.Minimum = 1;
            this.yScale.Name = "yScale";
            this.yScale.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.yScale.Size = new System.Drawing.Size(45, 238);
            this.yScale.TabIndex = 1;
            this.yScale.Value = 20;
            this.yScale.ValueChanged += new System.EventHandler(this.yScale_ValueChanged);
            // 
            // ScopeMonitor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.yScale);
            this.Controls.Add(this.xScale);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ScopeMonitor";
            this.Size = new System.Drawing.Size(311, 268);
            ((System.ComponentModel.ISupportInitialize)(this.xScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar xScale;
        private System.Windows.Forms.TrackBar yScale;
    }
}
