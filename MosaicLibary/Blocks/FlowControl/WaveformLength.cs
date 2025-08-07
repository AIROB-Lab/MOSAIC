using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// <see cref="ControlPanel"/> class for the <see cref="WaveformLength"/> block.
    /// Allows monitoring of Waveform Length feature extraction.
    /// </summary>
    public partial class cpWaveformLength : ControlPanel
    {
        private WaveformLength _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpWaveformLength"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The <see cref="WaveformLength"/> block associated with this control panel.</param>
        public cpWaveformLength(WaveformLength _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel display with updated Waveform Length data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            smFilter.Update(_sourceBlock.Data as Vector); 
        }
    }

    /// <summary>
    /// <para>
    /// The <see cref="WaveformLength"/> block computes the waveform length (WL) of each channel in a windowed input signal.
    /// Waveform Length is a widely used feature in EMG signal analysis, reflecting the cumulative absolute changes in the signal amplitude.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Example YAML configuration for a WaveformLength block:
    /// </para>
    /// <code>
    /// wlBlock:
    /// {
    ///   Type: WaveformLength,
    ///   Inputs: [windowedEMG]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: WaveformLength</c> specifies that this block extracts the waveform length feature from each input channel.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [windowedEMG]</c> provides the matrix of windowed EMG data (samples × channels); typically from a SlidingWindow block.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// In this configuration, the block computes, for each channel, the sum of the absolute differences between consecutive samples in the window.
    /// </para>
    /// </example>
    /// <remarks>
    /// The input to this block should be a matrix (windowed data, samples × channels).  
    /// The output is a vector with the waveform length for each channel.
    /// </remarks>
    public class WaveformLength : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformLength"/> block.
        /// </summary>
        /// <param name="Name">Block name.</param>
        /// <param name="DesiredRate">Processing/output rate (Hz).</param>
        /// <param name="InputCfg">List of input block names (should be one).</param>
        /// <param name="Params">Unused (no parameters needed for WL).</param>
        /// <param name="Path">Path for data dumping.</param>
        public WaveformLength(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the block's input and sets the output rate.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the block does not have exactly one input block.
        /// </exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Filter has one input only
            if (InputBlocks.Count != 1) 
                throw new Exception($"Function {Name} must have one input block only.");

            // a Filter's DesiredRate is the same as the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            cp = new cpWaveformLength(this);
        }

        /// <summary>
        /// Processes new input data and computes the waveform length feature for each channel.
        /// </summary>
        /// <param name="sender">The input block sending the data.</param>
        /// <param name="value">The input window as a matrix (samples × channels).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Matrix window = (value as Matrix).Clone();
            int rows = window.RowCount;  // Number of samples in the window
            int cols = window.ColumnCount;  // Number of channels

            // Create a vector to store WL values for each channel
            Vector<double> wlVector = Vector<double>.Build.Dense(cols);

            for (int c = 0; c < cols; c++)
            {
                double wlSum = 0.0;

                for (int r = 1; r < rows; r++)  // Start from index 1 to compute differences
                {
                    wlSum += Math.Abs(window[r, c] - window[r - 1, c]);  // Compute waveform length
                }

                wlVector[c] = wlSum;  // Store WL for each channel
            }
            SendOutput(wlVector);
        }
    }
}
