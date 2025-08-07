using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // Adaptive filter.
    //
    // This filter is specifically designed for heteroscedastic signals, i.e. signals whose noise
    // variance increases with magnitude. 
    // Put simply, it is a time-discrete IIR low-pass filter which automatically adapts its own 
    // cutoff frequency based on the current magnitude of the signal, as well as the magnitude
    // of its filtered derivative. 
    // The cutoff frequency is given by _cutoffFrequency = exp(Offset + DC * |X_dot_filt| + MC * |X|)
    // Offset, DC and MC are coefficients that can be set in the control panel. 
    // Additionally, one can set the filtering coefficient alpha used to filter the first-time 
    // derivative, which is filtered according to X_dot_filt(k) = X_dot(k) * alpha + X_dot_filt(k-1) * (1 - alpha)
    // -----------------------------------------------------------------------------------------

    #region ControlPanel
    /// <summary>
    /// Represents the control panel for the <see cref="AdaptiveFilter"/>, allowing dynamic adjustment of filter parameters.
    /// </summary>
    public partial class cpAdaptiveFilter : ControlPanel
    {
        /// <summary>
        /// The associated <see cref="AdaptiveFilter"/> block this control panel interacts with.
        /// </summary>
        private AdaptiveFilter _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the control panel for an AdaptiveFilter block.
        /// </summary>
        /// <param name="sourceBlock">The AdaptiveFilter block this panel is associated with.</param>
        public cpAdaptiveFilter(AdaptiveFilter _SourceBlock) : base(_SourceBlock) 
        {
            InitializeComponent();
            this._sourceBlock = _SourceBlock;
            trackBar1.Value = (int)(_SourceBlock.Alpha * trackBar1.Maximum);
            label1.Text = "Derivative Memory Coefficient: " + ((double)trackBar1.Value / trackBar1.Maximum).ToString();
            trackBar2.Value = (int)(_SourceBlock.Offset);
            label2.Text = "Offset: " + trackBar2.Value.ToString();
            trackBar3.Value = (int)(_SourceBlock.MagnitudeContribution);
            label3.Text = "Maginitude Contribution: " + trackBar3.Value.ToString();
            trackBar4.Value = (int)(_SourceBlock.DerivativeContribution);
            label4.Text = "Derivative Contribution: " + trackBar4.Value.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e) 
        { 
            double newAlpha = (double)trackBar1.Value / trackBar1.Maximum;
            _sourceBlock.Alpha = newAlpha;
            label1.Text = "Derivative Memory Coefficient: " + newAlpha.ToString(); 
        }
        private void trackBar2_Scroll(object sender, EventArgs e) 
        { 
            _sourceBlock.Offset = trackBar2.Value; 
            label2.Text = "Offset: " + trackBar2.Value.ToString(); 
        }
        private void trackBar3_Scroll(object sender, EventArgs e) 
        { 
            _sourceBlock.MagnitudeContribution = trackBar3.Value; 
            label3.Text = "Magnitude Contribution: " + trackBar3.Value.ToString(); 
        }
        private void trackBar4_Scroll(object sender, EventArgs e) 
        { 
            _sourceBlock.DerivativeContribution = trackBar4.Value; 
            label4.Text = "Derivative Contribution: " + trackBar4.Value.ToString(); 
        }

        override protected void cpRefresh(object o, EventArgs e) 
        { 
            label5.Text = "CutOff: " + _sourceBlock.CutoffFrequency.ToString("0.###") + "Hz"; 
        }

    }
    #endregion

    #region AdaptiveFilter
    /// <summary>
    /// Adaptive filter for heteroscedastic signals, where noise variance increases with signal magnitude.
    /// </summary>
    /// <remarks>
    /// This time-discrete IIR low-pass filter dynamically adjusts its cutoff frequency based on the 
    /// current signal magnitude and the magnitude of its filtered derivative. The primary purpose is to 
    /// provide robust filtering for signals with varying noise characteristics.
    /// For more information, refer to the [publication](https://ieeexplore.ieee.org/document/10304772).
    /// </remarks>
    /// <param name="Offset">
    /// The base logarithmic offset used in the cutoff frequency calculation.
    /// </param>
    /// <param name="DC">
    /// Coefficient for scaling the contribution of the magnitude of the filtered derivative, |X_dot_filt|.
    /// </param>
    /// <param name="MC">
    /// Coefficient for scaling the contribution of the signal magnitude, |X|.
    /// </param>
    /// <param name="alpha">
    /// Filtering coefficient for the first-time derivative, used to smooth its variations.
    /// </param>
    /// <remarks>
    /// The cutoff frequency is determined as:
    ///
    /// $$F_c = e^{\text{Offset} + \text{DC} \cdot |X_\text{dot\_filt}| + \text{MC} \cdot |X|}$$
    ///
    /// The filtered first derivative is calculated as:
    ///
    /// $$X_\text{dot\_filt}(k) = \alpha \cdot X_\text{dot}(k) + (1 - \alpha) \cdot X_\text{dot\_filt}(k-1)$$
    /// </remarks>
    /// <example>
    /// <code>
    ///  af2: { Type: AdaptiveFilter, DesiredRate: 200, Inputs: [ af1 ],  Params: [ '0.7', '-1', '-7', '8'] }
    /// </code>
    /// </example>

    public partial class AdaptiveFilter : Block
    {
        // Private fields for adaptive filter parameters
        private double _derivativeFilter;
        private double _offset;
        private double _magnitudeContribution;
        private double _derivativeContribution;

        // Sampling rate and cutoff frequency
        private double _samplingRate;
        private double _cutoffFrequency;

        // buffers for the past values of x and x_filtered. they really are matrices, one sample per row.
        // the last row contains the "latest" samples
        private Vector _currentFilteredValue;
        private Vector _filteredDerivativeSignal;
        private Vector _previousDerivative;
        private Vector _previousInput; 
        private Vector _previousOutput;

        // buffers for the past values of x and x_filtered. they really are matrices, one sample per row.
        // the last row contains the "latest" samples
        private Vector xf, xaf, dx_k_1, x_k_1, y_k_1;

        /// <summary>
        /// Gets or sets the offset parameter used in the adaptive filter's cutoff frequency calculation.
        /// </summary>
        public double Offset
        {
            get => _offset;
            set => _offset = value;
        }

        /// <summary>
        /// Gets or sets the magnitude contribution parameter, which scales the influence of the signal magnitude.
        /// </summary>
        public double MagnitudeContribution
        {
            get => _magnitudeContribution;
            set => _magnitudeContribution = value;
        }

        /// <summary>
        /// Gets or sets the derivative contribution parameter, which scales the influence of the filtered derivative.
        /// </summary>
        public double DerivativeContribution
        {
            get => _derivativeContribution;
            set => _derivativeContribution = value;
        }

        /// <summary>
        /// Gets or sets the alpha parameter, which is used to filter the first derivative.
        /// When set, this value recalculates the internal derivative filter coefficient.
        /// </summary>
        /// <remarks>
        /// The alpha value is related to the derivative filter coefficient as:
        ///
        /// $$\text{Alpha} = \frac{\text{DerivativeFilter} \cdot \text{SamplingRate}}{(1 - \text{DerivativeFilter}) \cdot 2\pi}$$
        /// </remarks>
        public double Alpha
        {
            get => _derivativeFilter * _samplingRate / (1 - _derivativeFilter) / (2 * Math.PI);
            set => _derivativeFilter = value * 2 * Math.PI / _samplingRate / (1 + 2 * Math.PI * value / _samplingRate);
        }

        /// <summary>
        /// Gets the current cutoff frequency of the adaptive filter.
        /// </summary>
        /// <remarks>
        /// The cutoff frequency is dynamically adjusted based on the following formula:
        ///
        /// $$F_c = e^{\text{Offset} + \text{DerivativeContribution} \cdot |X_\text{dot\_filt}| + \text{MagnitudeContribution} \cdot |X|}$$
        /// </remarks>
        public double CutoffFrequency => _cutoffFrequency;

        public AdaptiveFilter(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // one input only
            if (InputBlocks.Count != 1) throw new Exception($"Function {Name} must have one input block only.");

            // DesiredRate is the same as the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            // has 4 Params
            if (Params.Count != 4) throw new Exception($"Adaptive filter {Name} must have exactly 4 parameters.");

            // the filter's sampling rate is the desired rate of the Block
            _samplingRate = DesiredRate;

            // params: alpha, offest, magnitude, derivative
            _derivativeFilter = Convert.ToDouble(Params[0]) * 2 * Math.PI / _samplingRate / (1 + 2 * Math.PI * Convert.ToDouble(Params[0]) / _samplingRate);
            Offset = Convert.ToDouble(Params[1]);
            MagnitudeContribution = Convert.ToDouble(Params[2]);
            DerivativeContribution = Convert.ToDouble(Params[3]);

            cp = new cpAdaptiveFilter(this);
        }

        /// <summary>
        /// Processes incoming input signals, applying the adaptive filter, and outputs the filtered signal.
        /// </summary>
        /// <param name="sender">The block sending the input.</param>
        /// <param name="value">The input signal to be filtered.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Vector x = (Vector)value;

            // the first time the filter is called, or whenever the dimension of the input changes, reset and build the buffers
            if (xf == null || x.Count != xf?.Count)
            {
                xf = Vector.Build.Dense(x.Count);
                xaf = Vector.Build.Dense(x.Count);
                dx_k_1 = Vector.Build.Dense(x.Count);
                y_k_1 = Vector.Build.Dense(x.Count);
                x_k_1 = Vector.Build.Dense(x.Count);
            }


            // now enforce the equation for each dimension of the samples
            foreach (int sampleComponent in Enumerable.Range(0, x.Count))
            {
                xf[sampleComponent] = x[sampleComponent];
                double x_k = xf[sampleComponent];

                //Compute derivative and filter it
                double dx = (x_k - x_k_1[sampleComponent]) * _samplingRate;
                double dx_filtered = _derivativeFilter * dx + (1 - _derivativeFilter) * dx_k_1[sampleComponent];
                dx_k_1[sampleComponent] = dx_filtered;

                // Compute alpha coefficient through the cutoff frequency of the adaptive filter 
                _cutoffFrequency = Math.Exp(Offset + DerivativeContribution * Math.Abs(dx_filtered) + MagnitudeContribution * Math.Abs(x_k));
                double alpha = 2 * Math.PI * _cutoffFrequency / _samplingRate / (1 + 2 * Math.PI * _cutoffFrequency / _samplingRate);
                xaf[sampleComponent] = (alpha) * x_k + (1 - alpha) * y_k_1[sampleComponent];

                y_k_1[sampleComponent] = xaf[sampleComponent];
                x_k_1[sampleComponent] = xf[sampleComponent];
            }

            SendOutput(xaf);
        }
    }
    #endregion
}
