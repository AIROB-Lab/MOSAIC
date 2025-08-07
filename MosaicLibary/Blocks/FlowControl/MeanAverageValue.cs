using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// <see cref="ControlPanel"/> class for the <see cref="MeanAverageValue"/> block.
    /// Allows monitoring of Mean Average Value feature extraction.
    /// </summary>
    public partial class cpMeanAverageValue : ControlPanel
    {
        private MeanAverageValue _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpMeanAverageValue"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The <see cref="MeanAverageValue"/> block associated with this control panel.</param>
        public cpMeanAverageValue(MeanAverageValue _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel display with updated Mean Average Value data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            smFilter.Update(_sourceBlock.Data as Vector); 
        }
    }

    /// <summary>
    /// <para>
    /// The <see cref="MeanAverageValue"/> block computes the mean average value (MAV) of each channel in a windowed input signal.
    /// MAV is a classic time-domain EMG feature, representing the average of the absolute values in a window for each channel.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Example YAML configuration for a MeanAverageValue block:
    /// </para>
    /// <code>
    /// mavBlock:
    /// {
    ///   Type: MeanAverageValue,
    ///   Inputs: [windowedEMG]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: MeanAverageValue</c> specifies this block extracts the mean average value feature from each channel in the input.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [windowedEMG]</c> provides the (windowed) data matrix for MAV extraction; typically, this is the output of a SlidingWindow block.
    ///     </description>
    ///   </item>
    /// </list>
    /// In this configuration, the MAV block computes, for each channel, the average of the absolute value of all samples within the input window.
    /// </para>
    /// </example>
    /// <remarks>
    /// The input to this block should be a matrix (windowed data, samples × channels).
    /// The output is a vector with the mean average value for each channel.
    /// </remarks>
    public class MeanAverageValue : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeanAverageValue"/> block.
        /// </summary>
        /// <param name="Name">Block name.</param>
        /// <param name="DesiredRate">Processing/output rate (Hz).</param>
        /// <param name="InputCfg">List of input block names (should be one).</param>
        /// <param name="Params">Unused (no parameters needed for MAV).</param>
        /// <param name="Path">Path (unused).</param>
        public MeanAverageValue(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the block's input and sets the output rate.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the block does not have exactly one input block.
        /// </exception>>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Filter has one input only
            if (InputBlocks.Count != 1) 
                throw new Exception($"Function {Name} must have one input block only.");

            // a Filter's DesiredRate is the same as the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            cp = new cpMeanAverageValue(this);
        }

        /// <summary>
        /// Processes new input data and computes the mean average value feature for each channel.
        /// </summary>
        /// <param name="sender">The input block sending the data.</param>
        /// <param name="value">The input window as a matrix (samples × channels).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            Matrix window = (value as Matrix).Clone();
            int rows = window.RowCount;  // Number of samples in the window
            int cols = window.ColumnCount;  // Number of channels

            Vector<double> mavVector = Vector<double>.Build.Dense(cols);

            for (int c = 0; c < cols; c++)
            {
                double sumAbs = 0.0;

                for (int r = 0; r < rows; r++)
                {
                    sumAbs += Math.Abs(window[r, c]);  // Sum absolute values
                }

                mavVector[c] = sumAbs / rows;  // Compute MAV for each channel
            }
            SendOutput(mavVector);
        }
    }
}
