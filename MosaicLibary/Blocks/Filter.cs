using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // Filters.
    //
    // an extremely simple FIR filtering class, implementing the well-known equation
    // x_filtered(t) = numerator*x(t,t-1,...,t-N) - denominator*x_filtered(t-1,t-2,...,t-M)
    //
    // use MATLAB to get the coefficients in the numerator and denominator, for Dict
    // >> [_numerator,_denominator] = butter(3,5/(200/2))
    // for a Butterworth 3rd order lowpass filter with cutoff at 5Hz and assuming a sampling rate of 200S/s, or (simpler example)
    // >> _numerator = ones(1,N)/N, _denominator = zeros(1,N)
    // for a moving average where N is the number of samples in the chosen time window.
    //
    // this class works on multi-dimensional signals, taking as input the current sample x and returning
    // the filtered sample at the current time.
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// A control panel class for the <see cref="Filter"/> block.
    /// </summary>
    public partial class cpFilter : ControlPanel
    {
        private Filter _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpFilter"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The source <see cref="Filter"/> block associated with this control panel.</param>
        public cpFilter(Filter _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel display with updated filter data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            smFilter.Update(_sourceBlock.Data as Vector); 
        }
    }

    /// <summary>
    /// A simple FIR/IIR filtering block that applies a filter equation to multi-dimensional signals.
    /// </summary>
    /// <example>
    /// The <see cref="Filter"/> Block needs a data providing Block as input and the filter coefficients as parameters.
    /// <code>
    /// LowPassFilter: 
    /// {
    ///     Type: Filter,
    ///     Inputs: [DataBlock],
    ///     Params: [0.00094469;0.0018894;0.00094469, 1;-1.9112;0.91498 ]
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// Implements the equation:
    /// 
    /// $$x_\text{filtered}(t) = \text{numerator} \cdot x(t,t-1,\ldots,t-N)\;-\;\text{denominator} \cdot x_\text{filtered}(t-1,t-2,\ldots,t-M)$$
    /// 
    /// Use tools like MATLAB to generate the numerator and denominator coefficients. For example:
    /// <list type="bullet">
    /// <item>
    /// <description>Butterworth filter: <c>[num, den] = butter(3, 5/(200/2))</c></description>
    /// </item>
    /// <item>
    /// <description>Moving average: <c>num = ones(1, N)/N, den = zeros(1, N)</c></description>
    /// </item>
    /// </list>
    /// 
    /// This block operates on multi-dimensional signals, processing the current sample and returning the filtered sample.
    /// </remarks>
public class Filter : Block
    {
        // the numerator and denominator
        private Vector _numerator;
        private Vector _denominator;

        // buffers for the past values of x and x_filtered. they really are matrices, one sample per row.
        // the last row contains the "latest" samples
        private Matrix _xBuffer, _xfBuffer;
        private Vector _xf;

        public Filter(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the input settings and initializes the filter coefficients.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the filter does not have exactly one input block or if the parameter count is invalid.
        /// </exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Filter has one input only
            if (InputBlocks.Count != 1) 
                throw new Exception($"Function {Name} must have one input block only.");

            // a Filter's DesiredRate is the same as the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            // a Filter has 2 Params
            if (Params.Count != 2) 
                throw new Exception($"Filter {Name} must have exactly 2 parameters.");

            // so now build the numerator and denominators out of the strings in the cfg file
            double[] numArray = Array.ConvertAll(Params[0].Split(';'), double.Parse);
            double[] denArray = Array.ConvertAll(Params[1].Split(';'), double.Parse);

            _numerator = Vector.Build.DenseOfEnumerable(numArray.Reverse());
            _denominator = Vector.Build.DenseOfEnumerable(denArray.Reverse());

            cp = new cpFilter(this);
        }

        /// <summary>
        /// Processes new input data and applies the filter equation to produce the filtered output.
        /// </summary>
        /// <param name="sender">The input block sending the data.</param>
        /// <param name="value">The input sample as a vector.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Vector x = (Vector)value;

            // the first time the filter is called, or whenever the dimension of the input changes, reset and build the buffers
            if (_xBuffer == null || x.Count != _xBuffer?.ColumnCount)
            {
                _xBuffer = Matrix.Build.Dense(_numerator.Count, x.Count);
                _xfBuffer = Matrix.Build.Dense(_numerator.Count, x.Count);
                _xf = Vector.Build.Dense(x.Count);
            }

            // "shift" the buffers one up: first remove the first row,
            _xBuffer = _xBuffer.RemoveRow(0); _xfBuffer = _xfBuffer.RemoveRow(0);
            // then stack the new sample at the bottom of the x buffer,
            _xBuffer = _xBuffer.Stack(x.ToRowMatrix());
            // and a row of zeroes at the bottom of the x_filtered buffer (to be filled at the end)
            _xfBuffer = _xfBuffer.Stack(Vector.Build.Dense(x.Count, 0).ToRowMatrix());

            // now enforce the equation for each dimension of the samples
            foreach (int sampleComponent in Enumerable.Range(0, x.Count))
                _xf[sampleComponent] =
                    _numerator.DotProduct(_xBuffer.Column(sampleComponent)) -
                    _denominator.DotProduct(_xfBuffer.Column(sampleComponent));

            // and put the result in the last row of the x_filtered buffer
            _xfBuffer.SetRow(_xfBuffer.RowCount - 1, _xf);

            SendOutput(_xf);
        }
    }
}
