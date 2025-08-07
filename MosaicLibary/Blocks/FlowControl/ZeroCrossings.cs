using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// <see cref="ControlPanel"/> class for the <see cref="ZeroCrossings"/> block.
    /// Allows the user to monitor Zero Crossing feature extraction.
    /// </summary>
    public partial class cpZeroCrossings : ControlPanel
    {
        private ZeroCrossings _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpZeroCrossings"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The source <see cref="Filter"/> block associated with this control panel.</param>
        public cpZeroCrossings(ZeroCrossings _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel display with updated Zero Crossing feature data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            smFilter.Update(_sourceBlock.Data as Vector); 
        }

        /// <summary>
        /// Updates the ZC threshold value when the user changes it in the UI.
        /// </summary>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _sourceBlock.Threshold = (double)numericUpDown1.Value;
        }
    }

    /// <summary>
    /// <para>
    /// The <see cref="ZeroCrossings"/> block computes the number of zero crossings in each channel of a windowed input signal.
    /// This feature is widely used in EMG analysis to represent signal frequency content or activity.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Example YAML configuration for a ZeroCrossings block:
    /// </para>
    /// <code>
    /// zcBlock:
    /// {
    ///   Type: ZeroCrossings,
    ///   Inputs: [windowedEMG],
    ///   Params: [0.02]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: ZeroCrossings</c> specifies this block extracts the zero crossing feature from each channel in the input.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [windowedEMG]</c> provides the (windowed) data matrix for ZC extraction; typically, this is the output of a SlidingWindow block.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [0.02]</c> sets the minimum amplitude difference required to count a zero crossing (to reduce noise influence).
    ///     </description>
    ///   </item>
    /// </list>
    /// In this configuration, the ZC block counts, for each channel, the number of times the signal crosses zero (changes sign) within the input window and the difference between samples exceeds 0.02.
    /// </para>
    /// </example>
    /// <remarks>
    /// The input to this block should be a matrix (windowed data, samples × channels).
    /// The output is a vector with the zero crossing count for each channel.
    /// A zero crossing is counted only if the sign of the signal changes between consecutive samples, and the amplitude change exceeds the set threshold.
    /// </remarks>
    public class ZeroCrossings : Block
    {
        private double _threshold = 0.0;
        /// <summary>
        /// Gets or sets the minimum amplitude difference required to count a zero crossing (reduces noise effects).
        /// </summary>
        public double Threshold { get => _threshold; set => _threshold = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroCrossings"/> block.
        /// </summary>
        /// <param name="Name">Block name.</param>
        /// <param name="DesiredRate">Processing/output rate (Hz).</param>
        /// <param name="InputCfg">List of input block names (should be one).</param>
        /// <param name="Params">Parameters (threshold value as string).</param>
        /// <param name="Path">Path for data dumping.</param>
        public ZeroCrossings(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the block's input and threshold parameter.
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

            if(Params != null)
                this.Threshold = double.Parse(Params[0]);

            cp = new cpZeroCrossings(this);
        }

        /// <summary>
        /// Processes new input data, computes the number of zero crossings per channel, and outputs the feature vector.
        /// </summary>
        /// <param name="sender">The input block sending the data.</param>
        /// <param name="value">The input window as a matrix (samples × channels).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Matrix window = (value as Matrix).Clone();
            int rows = window.RowCount;  // Number of samples in the window
            int cols = window.ColumnCount;  // Number of channels

            // Create a vector to store ZC values for each channel
            Vector<double> zcVector = Vector<double>.Build.Dense(cols);

            for (int c = 0; c < cols; c++)
            {
                int zeroCrossings = 0;

                for (int r = 0; r < rows - 1; r++) // Go up to N-1
                {
                    double current = window[r, c];
                    double next = window[r + 1, c];

                    // Check if a zero crossing occurs with the threshold condition
                    if ((current > 0 && next < 0) || (current < 0 && next > 0))
                    {
                        if (Math.Abs(current - next) >=Threshold)
                        {
                            zeroCrossings++;
                        }
                    }
                }

                zcVector[c] = zeroCrossings;  // Store ZC count per channel
            }
            SendOutput(zcVector);
        }
    }
}
