using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // Resampler - receive signal at sampling rate S, output it at sampling rate S'.
    //   The implementation is outreageously simple: if the input comes from the data provider, just
    //   store whatever you just got; otherwise, send it off. It works like a "sample-and-hold" interpolator.
    // -----------------------------------------------------------------------------------------

    #region Control Panel
    /// <summary>
    /// Control Panel for the <see cref="Resampler"/> class, providing a UI for visualizing the resampling process.
    /// </summary>
    public partial class cpResampler : ControlPanel
    {
        /// <summary>
        /// The <see cref="Resampler"/> block associated with this control panel.
        /// </summary>
        private Resampler _sourceBlock;

        public cpResampler(Resampler sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel to update the display with the latest data from the Resampler block.
        /// UI includes a <see cref="SpiderPlotMonitor"/> and <see cref="ScopeMonitor"/> that get updated.
        /// </summary>
        /// <param name="o">The event sender.</param>
        /// <param name="e">Event arguments.</param>
        override protected void cpRefresh(object o, EventArgs e)
        {
            spResampler.Update(_sourceBlock.Data as Vector);
            smResampler.Update(_sourceBlock.Data as Vector);
        }
    }
    #endregion


    #region Resampler 
    /// <summary>
    /// Resamples an input signal from a source sampling rate to a target sampling rate using a simple sample-and-hold method.
    /// The first input provides the signal to be resampled, and the second input must be a <see cref="DecimatorTimer"/> 
    /// that dictates when the resampled value is output.
    /// </summary>
    /// <example>
    /// <code>
    /// resampler:
    /// {
    ///   Type: Resampler,
    ///   Inputs: [ SignalBlockName, DecimatorTimerName ],
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: Resampler</c> indicates this block will output a “sample-and-held” version 
    ///     of the incoming data according to a timer.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ SignalBlockName, DecimatorTimerName ]</c> shows that:
    ///       <list type="bullet">
    ///         <item><c>SignalBlockName</c> is the data source (e.g., EMG after low-pass filtering).</item>
    ///         <item><c>DecimatorTimerName</c> is a <see cref="DecimatorTimer"/> that triggers 
    ///         how often the Resampler outputs the last sampled value.</item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// The Resampler holds the latest input sample internally until the timer block ticks, upon which 
    /// it sends the stored sample as output, effectively matching the <c>DecimatorTimerName</c> rate.
    /// </example>
    public class Resampler : Block
    {
        /// <summary>
        /// Represents the timer block used to determine when to output the resampled value.
        /// </summary>
        private Block _inputTimer;
        /// <summary>
        /// Holds the latest value received from the input. This value is output when the timer block triggers.
        /// </summary>
        private Vector _value = Vector.Build.Dense(1);

        /// <summary>
        /// Indicates whether the first value has been received from the input. This ensures that output begins
        /// only after the first value is received.
        /// </summary>
        private bool _firstValueReceived = false;


        /// <summary>
        /// Initializes a new instance of the Resampler class, setting up the block for resampling operations.
        /// </summary>
        /// <param name="Name">The unique name of the block.</param>
        /// <param name="DesiredRate">The target sampling rate for the output signal.</param>
        /// <param name="InputCfg">Configuration for inputs to the Resampler block.</param>
        /// <param name="Params">Additional parameters specifying the behavior of the Resampler, such as the source timer block name.</param>
        /// <param name="Path">Path for any additional resources needed by the block.</param>
        public Resampler(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the Resampler inputs, including identifying the timer input for resampling.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Resampler has two inputs, the second of which must be a Timer
            if (InputBlocks.Count != 2)
                throw new Exception($"Resampler {Name} must have two input blocks.");
            if (!(InputBlocks[1] is Timer))
                throw new Exception($"Resampler {Name}'s second input block must be a Timer.");
            _inputTimer = InputBlocks[1] as Timer;

            // a Function's DesiredRate is the same as the input timer's
            DesiredRate = _inputTimer.DesiredRate;

            cp = new cpResampler(this);
        }

        /// <summary>
        /// Processes new input by either storing it for later output or outputting the last stored value on timer tick.
        /// </summary>
        /// <param name="sender">The block sending the input.</param>
        /// <param name="value">The input value, expected to be a Vector.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            // if my timer ticks, send out the current value
            if (sender == _inputTimer)
            {
                if (_firstValueReceived)
                    SendOutput(_value);
            }
            // otherwise, just store it. that's all, folks.
            else
            {
                if (value is Vector)
                {
                    _value = (value as Vector).Clone();
                    _firstValueReceived = true;
                }
                else if (value is Matrix)
                {
                    _value = (value as Matrix).Row(0).Clone();
                    _firstValueReceived = true;
                }
            }
        }
    }
    #endregion
}

