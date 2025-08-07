using System.Drawing;
using System.Windows.Forms;

namespace MosaicLibary
{
    // SimpleProgressBar - extremely simple implementation of a coloured bar which can be either vertical or horizontal.
    //   Just set the Vertical property to true or false in the Properties panel.
    public partial class SimpleProgressBar : UserControl
    {
        double percent;
        public double Value
        {
            get { return percent; }
            set
            {
                // Maintain the Value between 0 and 100
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                percent = value;
                // Redraw the Pbar every time the value changes
                Invalidate();
            }
        }
        public bool Vertical { get; set; }

        public SimpleProgressBar() { InitializeComponent(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Vertical)
            {
                Brush b = new SolidBrush(ForeColor);
                int height = (int)(percent / 100.0 * Height);
                e.Graphics.FillRectangle(b, 0, Height - height, Width, Height);
                b.Dispose();
            }
            else
            {
                Brush b = new SolidBrush(ForeColor);
                int width = (int)(percent / 100.0 * Width);
                e.Graphics.FillRectangle(b, 0, 0, width, Height);
                b.Dispose();
            }
        }
    }
}
