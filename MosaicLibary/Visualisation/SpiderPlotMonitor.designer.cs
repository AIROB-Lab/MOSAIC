namespace MosaicLibary
{
    partial class SpiderPlotMonitor
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
            this.components = new System.ComponentModel.Container();
            this.trbZoom = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trbZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // trbZoom
            // 
            this.trbZoom.AutoSize = false;
            this.trbZoom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.trbZoom.Location = new System.Drawing.Point(0, 238);
            this.trbZoom.Maximum = 100;
            this.trbZoom.Name = "trbZoom";
            this.trbZoom.Size = new System.Drawing.Size(311, 30);
            this.trbZoom.TabIndex = 1;
            this.trbZoom.TickFrequency = 10;
            this.trbZoom.Value = 20;
            this.trbZoom.ValueChanged += new System.EventHandler(this.trbZoom_ValueChanged);
            // 
            // SpiderPlot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.trbZoom);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SpiderPlot";
            this.Size = new System.Drawing.Size(311, 268);
            ((System.ComponentModel.ISupportInitialize)(this.trbZoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TrackBar trbZoom;
    }
}
