using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// <see cref="ControlPanel"/> class for the <see cref="SlopeSignChanges"/> block.
    /// Allows the user to interact with and monitor SSC extraction.
    /// </summary>
    public partial class cpSlopeSignChanges : ControlPanel
    {
        private SlopeSignChanges _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpSlopeSignChanges"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The source <see cref="Filter"/> block associated with this control panel.</param>
        public cpSlopeSignChanges(SlopeSignChanges _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel display with updated Slope Sign Changes feature data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            smFilter.Update(_sourceBlock.Data as Vector); 
        }

        /// <summary>
        /// Updates the SSC threshold value when the user changes it in the UI.
        /// </summary>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _sourceBlock.Threshold = (double)numericUpDown1.Value;
        }
    }

    /// <summary>
    /// <para>
    /// The SlopeSignChanges block computes the number of slope sign changes (SSC) in each channel of a sliding window.
    /// This feature is commonly used in EMG signal processing to quantify the frequency content of a signal while reducing sensitivity to noise.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Example YAML configuration for a SlopeSignChanges block:
    /// </para>
    /// <code>
    /// sscBlock:
    /// {
    ///   Type: SlopeSignChanges,
    ///   Inputs: [windowedEMG],
    ///   Params: [0.02]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: SlopeSignChanges</c> specifies this block extracts the SSC feature from each channel in the input.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [windowedEMG]</c> provides the (windowed) data matrix for SSC extraction; typically, this is the output of a SlidingWindow block.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [0.02]</c> sets the threshold for minimum amplitude difference to count a slope sign change (to reduce noise influence).
    ///     </description>
    ///   </item>
    /// </list>
    /// In this configuration, the SSC block counts, for each channel, the number of times the slope changes sign within the input window and the difference before/after exceeds 0.02.
    /// </para>
    /// </example>
    /// <remarks>
    /// The block expects a matrix as input (samples × channels) and outputs a vector (one SSC value per channel).
    /// The SSC algorithm examines each triplet of consecutive samples per channel and counts a sign change in slope only if both differences exceed the threshold.
    /// </remarks>
    public class SlopeSignChanges : Block
    {
        private double _threshold = 0.0;

        /// <summary>
        /// Gets or sets the minimum difference required to count a slope sign change (to suppress noise).
        /// </summary>
        public double Threshold { get => _threshold; set => _threshold = value; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SlopeSignChanges"/> block.
        /// </summary>
        /// <param name="Name">Name of the block.</param>
        /// <param name="DesiredRate">Desired rate (Hz) for SSC feature extraction.</param>
        /// <param name="InputCfg">Names of input blocks (should be one).</param>
        /// <param name="Params">Parameters (threshold value as string).</param>
        /// <param name="Path">Path for data dumping.</param>
        public SlopeSignChanges(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the block's inputs and threshold parameter.
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

            cp = new cpSlopeSignChanges(this);
        }

        /// <summary>
        /// Processes new input data, computes the number of slope sign changes per channel, and outputs the feature vector.
        /// </summary>
        /// <param name="sender">The input block sending the data.</param>
        /// <param name="value">The input window as a matrix (samples × channels).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Matrix window = (value as Matrix).Clone();
            int rows = window.RowCount;  // Number of samples in the window
            int cols = window.ColumnCount;  // Number of channels

            // Create a vector to store SSC values for each channel
            Vector<double> sscVector = Vector<double>.Build.Dense(cols);

            for (int c = 0; c < cols; c++)
            {
                int slopeChanges = 0;

                for (int r = 1; r < rows - 1; r++)  // Start from index 1 to compare three consecutive points
                {
                    double prev = window[r - 1, c];
                    double curr = window[r, c];
                    double next = window[r + 1, c];

                    // Compute differences
                    double diff1 = curr - prev;
                    double diff2 = next - curr;

                    // Check if there is a slope sign change AND apply threshold filtering
                    if ((diff1 * diff2 < 0) && (Math.Abs(diff1) >= Threshold) && (Math.Abs(diff2) >= Threshold))
                    {
                        slopeChanges++;
                    }
                }

                sscVector[c] = slopeChanges;  // Store SSC count per channel
            }
            SendOutput(sscVector);
        }
    }
}
