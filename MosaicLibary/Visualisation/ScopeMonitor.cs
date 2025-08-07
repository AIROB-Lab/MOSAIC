using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // ScopeMonitor - Display vectors as classical line-style plot.
    // -----------------------------------------------------------------------------------------


    public partial class ScopeMonitor : Monitor
    {
        public ScopeMonitor() : base() { InitializeComponent(); }

        override public void Update(Vector Signal)
        {
            if (Signal == null) return;
            base.Update(Signal);

            // if first call, define the drawable area
            if (plotter == null || !new Size(drawableArea.Size.Width + yScale.Width, drawableArea.Size.Height + xScale.Height).Equals(ClientRectangle.Size))
            {
                drawableArea = ClientRectangle;
                drawableArea.Height -= xScale.Height;
                drawableArea.Width -= yScale.Width;
                plotter = CreateGraphics();
                plotter.SmoothingMode = SmoothingMode.AntiAlias;
                plotter.FillRectangle(new SolidBrush(BackColor), drawableArea);
            }

            currentX += (double)sizeX / 20;
            if ((int)currentX > drawableArea.Width)
            {
                currentX = 0;
                curvePoints = new List<List<Point>>();
                for (int i = 0; i < this.Signal.Count; i++) curvePoints.Add(new List<Point>());
            }

            yScale_ValueChanged(this, null);
            Invalidate();
        }

        Graphics plotter;
        Rectangle drawableArea;

        // graphics bookmarking
        double currentX = 0;
        int xZoomFactor, yZoomFactor;
        List<List<Point>> curvePoints = new List<List<Point>>();
        const double maxSignalValue = 5.0;

        Pen grayDashedPen = new Pen(Color.Gray, 1);
        int centerX, centerY, sizeX, sizeY;
        Dictionary<string, double[]> queuedFixedPoints = new Dictionary<string, double[]>();

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // draw axes
            g.DrawLine(grayDashedPen, yScale.Width + 0, centerY, yScale.Width + drawableArea.Width, centerY);
            g.DrawLine(grayDashedPen, yScale.Width + centerX, 0, yScale.Width + centerX, drawableArea.Height);
            foreach (int tick in Enumerable.Range(1, (int)maxSignalValue))
            {
                g.DrawLine(grayDashedPen, yScale.Width + 0, centerY + tick * sizeY, yScale.Width + drawableArea.Width, centerY + tick * sizeY);
                g.DrawLine(grayDashedPen, yScale.Width + 0, centerY - tick * sizeY, yScale.Width + drawableArea.Width, centerY - tick * sizeY);
                g.DrawLine(grayDashedPen, yScale.Width + centerX - tick * sizeX, 0, yScale.Width + centerX - tick * sizeX, drawableArea.Height);
                g.DrawLine(grayDashedPen, yScale.Width + centerX + tick * sizeX, 0, yScale.Width + centerX + tick * sizeX, drawableArea.Height);
            }

            // draw current signal
            if (Signal != null)
            {
                if (Signal.Count != curvePoints.Count || curvePoints.Count == 0)
                    for (int i = 0; i < Signal.Count; i++) curvePoints.Add(new List<Point>());

                for (int i = 0; i < Signal.Count; i++)
                {
                    curvePoints[i].Add(new Point(yScale.Width + (int)currentX, Convert.ToInt32(drawableArea.Height / 2 - 0.5 * Signal.Count * yZoomFactor + i * 1 * yZoomFactor +
                                -1 * Signal[i] * yZoomFactor / 20 * (drawableArea.Height / (2 * maxSignalValue)))));
                    if (curvePoints[i].Count > 1)
                        g.DrawLines(new Pen(Color.FromArgb(i * 255 / Signal.Count, 0, 255 - i * 255 / Signal.Count), 1), curvePoints[i].ToArray());
                }

                if (false && queuedFixedPoints.Count > 0)
                {
                    for (int pntIdx = 0; pntIdx < queuedFixedPoints.Count; pntIdx++)
                    {
                        double[] pnt = convertSpiderToXY(Vector.Build.DenseOfArray(queuedFixedPoints.ElementAt(pntIdx).Value));
                        float x = Convert.ToSingle(drawableArea.Width / 2 + pnt[0] * xZoomFactor / 20 * (drawableArea.Width / (2 * maxSignalValue)));
                        float y = Convert.ToSingle(drawableArea.Height / 2 + pnt[1] * yZoomFactor / 20 * (drawableArea.Height / (2 * maxSignalValue)));
                        g.DrawEllipse(new Pen(Color.Purple, 1), x, y, 5, 5);
                        g.DrawString(queuedFixedPoints.ElementAt(pntIdx).Key.ToUpper(), Font,
                            new SolidBrush(Color.Purple), new Point(Convert.ToInt32(x) + 3, Convert.ToInt32(y)));
                    }
                }
            }

            base.OnPaint(e);
        }

        double[] convertSpiderToXY(Vector sig)
        {
            int size = sig.Count;
            double deltaA = 2 * Math.PI / size, xC = 0, yC = 0;

            for (int i = 0; i < size; i++)
            {
                xC += sig[i] * Math.Sin(deltaA * i);
                yC += sig[i] * Math.Cos(deltaA * i);
            }
            xC /= size; yC /= size;

            return new double[2] { xC, yC };
        }

        void yScale_ValueChanged(object sender, EventArgs e) { yScale.Invoke(new MethodInvoker(delegate { yZoomFactor = yScale.Value; })); drawAxes(); }
        void xScale_ValueChanged(object sender, EventArgs e) { xScale.Invoke(new MethodInvoker(delegate { xZoomFactor = xScale.Value; })); drawAxes(); }
        void drawAxes()
        {
            // draw axes in grey dotted style
            //Pen grayDashedPen = new Pen(Color.Gray, 1);
            grayDashedPen.DashPattern = new float[] { 3, 3 };

            centerX = drawableArea.Width / 2;
            centerY = drawableArea.Height / 2;
            sizeX = (int)((double)xScale.Value / 20 * (drawableArea.Width / maxSignalValue));
            sizeY = (int)((double)yScale.Value / 20 * (drawableArea.Height / maxSignalValue));
        }
    }
}
