using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.IO;
using Python.Runtime;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

using Windows.Devices.Enumeration;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace MosaicLibary
{
    public partial class cpPythonNetFiltUC : ControlPanel
    {
        PythonNetFiltUC _SourceBlock;
        public string SelectedFilter { get; set; }

        public cpPythonNetFiltUC(PythonNetFiltUC _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._SourceBlock = _SourceBlock;
        }

        private void buttonUpdateFilter(object sender, EventArgs e)

        {
            float fn, fc, fs;
            int filterType;
            if (float.TryParse(filtorder.Text, out fn) &&
                float.TryParse(filtFs.Text, out fs) &&
                float.TryParse(filtFc.Text, out fc))
            {
                // type = 0 Means the user chose Butterworth Filter
                // type = 1 Means the user chose Notch Filter // tbd add more
                filterType = filter.SelectedItem.ToString() == "Butterworth LP Filter" ? 0 : 1;

                _SourceBlock.UpdateInputs(fn, fc, fs, filterType);
            }
        }
    }

    public class PythonNetFiltUC : Block
    {
        private dynamic scipy_signal;
        private dynamic np;

        private dynamic a;
        private dynamic b;
        private dynamic[] zi_s;

        public PythonNetFiltUC(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        {
            PythonNetManager.Instance.Init();
        }

        override public void ConfigureInputs()
        {
            cp = new cpPythonNetFiltUC(this);
            base.ConfigureInputs();
            if (Params.Count < 5) throw new Exception($"Python Net {Name} must have at least six input blocks.");

            using (Py.GIL())
            {
                scipy_signal = Py.Import("scipy.signal");
                np = Py.Import("numpy");

                string Path1 = Params[3];
                int filterType = Convert.ToInt32(Params[0]);
                int fn = Convert.ToInt32(Params[1]);
                double fc = Convert.ToInt32(Params[2]);
                double fs = Convert.ToInt32(Params[4]);

                UpdateInputs(fn, fc, fs, filterType);
            }
            // aDesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;
        }

        /// <summary>
        // filt_type = 0 Means the user chose Butterworth LP Filter
        // filter_n is The order of the filter
        // num2 is the critical frequency or frequencies for the notch filter (2 * critical frequencies / fs)
        // fs is sampling frequency

        // filt_type = 1 Means the user chose Notch Filter
        // filter_n is Quality factor
        // num2 is the frequency to remove from a signal (must satisfy 0 < w0 < 1)
        // fs is sampling frequency
        /// </summary>
        /// <param name="f_n"></param>
        /// <param name="f_c"></param>
        /// <param name="newfs"></param>
        /// <param name="filt_type"></param>
        public void UpdateInputs(double fn, double fc, double fs, int filt_type)
        {
            ////
            /// This is an example on how to use an external python script (which is not part of your python folder), set variables, read it, compile it and execute it and get your variables back
            using (Py.GIL())
            {
                dynamic res = null;
                if(filt_type == 0)
                {
                    res = scipy_signal.butter(fn, 2 * fc / fs, "low");
                }
                else if (filt_type == 1)
                {
                    res = scipy_signal.iirnotch(fc, fn, fs);

                }
                a = res[1];
                b = res[0];
                zi_s = null;
            }
        }
        override protected void OnNewInput(Block sender, object value)
        {
            double[] outSig = new double[(value as Vector).Count];

            ////
            ///This is an example on how to directly use a python library "scipy.signal" which was imported earlier and access its lfilter function
            using (Py.GIL())
            {

                // directly after update b, a, initialize zi
                if (zi_s == null)
                {
                    zi_s = new dynamic[(value as Vector).Count];
                    for (int j = 0; j < zi_s.Length ; j++)
                    {
                        zi_s[j] = scipy_signal.lfilter_zi(b, a);
                    }
                }
                double[] signal = (value as Vector).AsArray();

                // go through signal and apply on new sample (example works for vector only, but would be easily changeable to matrix)
                for (int j = 0; j < outSig.Length; j++)
                {
                    dynamic res = scipy_signal.lfilter(b, a, np.array(new double[] { signal[j] }), zi:zi_s[j]);
                    outSig[j] = res[0][0];
                    zi_s[j] = res[1];
                }

            }
            SendOutput(Vector.Build.DenseOfArray(outSig));
        }
    }
}
