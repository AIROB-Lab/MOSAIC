using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // SpiderPlotMonitor - Display vectors in a "cobweb" fashion, that is, as a polar graph.
    //     Best suited for vectors which only have positive values.
    // -----------------------------------------------------------------------------------------
    public partial class SpiderPlotMonitor : Monitor
    {
        public SpiderPlotMonitor() : base() { InitializeComponent(); }

        override public void Update(Vector Signal)
        {
            if (Signal == null) return;
            base.Update(Signal);

            // if first call, define the drawable area
            if (plotter == null)
            {
                drawableArea = ClientRectangle;
                drawableArea.Height -= trbZoom.Height;

                plotter = CreateGraphics();
                plotter.SmoothingMode = SmoothingMode.AntiAlias;
                plotter.FillRectangle(new SolidBrush(BackColor), drawableArea);
            }

            trbZoom_ValueChanged(this, null);
            Invalidate();
        }

        Graphics plotter;
        Rectangle drawableArea;

        // graphics bookmarking
        int zoomFactor, centerX, centerY, sizeX, sizeY;
        Pen grayDashedPen = new Pen(Color.Gray, 1);
        Point[] points;
        Point[] evaluateCartesianCoordinates(Vector v)
        {
            points = new Point[v.Count];

            foreach (int componentIndex in Enumerable.Range(0, points.Length))
            {
                try
                {
                    double multFactor = 2 * Math.PI * componentIndex / v.Count;
                    points[componentIndex].X = Convert.ToInt32(
                        drawableArea.Width / 2 +
                        v[componentIndex] * Math.Sin(multFactor) * (double)zoomFactor / 20 * (drawableArea.Width / (2 * maxSignalValue))
                    );
                    points[componentIndex].Y = Convert.ToInt32(
                        drawableArea.Height / 2 +
                        v[componentIndex] * Math.Cos(multFactor) * (double)zoomFactor / 20 * (drawableArea.Height / (2 * maxSignalValue))
                    );
                }
                catch { }
            }

            return points;
        }

        Color color = Color.Red;
        const double maxSignalValue = 5.0;

        void drawAxes()
        {
            grayDashedPen.DashPattern = new float[] { 3, 3 };
            centerX = drawableArea.Width / 2;
            centerY = drawableArea.Height / 2;
            sizeX = (int)((double)trbZoom.Value / 20 * (drawableArea.Width / maxSignalValue));
            sizeY = (int)((double)trbZoom.Value / 20 * (drawableArea.Height / maxSignalValue));
        }

        void trbZoom_ValueChanged(object sender, EventArgs e)
        {
            if (!IsHandleCreated) return;
            trbZoom.Invoke(new MethodInvoker(delegate { zoomFactor = trbZoom.Value; }));
            drawAxes();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // draw axes
            foreach (int tick in Enumerable.Range(1, (int)maxSignalValue))
                g.DrawEllipse(grayDashedPen, new Rectangle(
                    centerX - tick * sizeX / 2, centerY - tick * sizeY / 2,
                    tick * sizeX,
                    tick * sizeY
                ));
            if (Signal != null)
                foreach (var pointIndex in Enumerable.Range(1, Signal.Count))
                {
                    double multFactor = 2 * Math.PI * pointIndex / Signal.Count;
                    g.DrawLine(grayDashedPen, centerX, centerY,
                        (int)(centerX + maxSignalValue * Math.Sin(multFactor) * (double)trbZoom.Value / 20 * (drawableArea.Width / (2 * maxSignalValue))),
                        (int)(centerY + maxSignalValue * Math.Cos(multFactor) * (double)trbZoom.Value / 20 * (drawableArea.Height / (2 * maxSignalValue)))
                    );
                }

            // draw current signal
            if (Signal != null)
                if (Signal.Count > 1)
                    g.DrawPolygon(new Pen(color, 2), evaluateCartesianCoordinates(Signal));

            base.OnPaint(e);
        }
    }
}
