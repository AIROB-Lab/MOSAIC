using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    public partial class Monitor : UserControl
    {
        protected Vector Signal;
        public Monitor() { InitializeComponent(); }
        virtual public void Update(Vector Signal) { this.Signal = Signal; }
    }
}
