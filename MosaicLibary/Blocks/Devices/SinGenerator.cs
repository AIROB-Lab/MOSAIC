using System;
using System.Linq;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    #region ControlPanel
    /// <summary>
    /// Represents the control panel for the <see cref="SinGenerator"/> block.
    /// This class provides a user interface for displaying the generate sinus curves in real-time.
    /// </summary>
    public partial class cpSinGenerator : ControlPanel
    {
        /// <summary>
        /// The SinGenerator block that this control panel is associated with.
        /// This field holds a reference to the source block whose data is displayed
        /// and controlled by this panel.
        /// </summary>
        private SinGenerator _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the CpSinGenerator class, setting up the UI
        /// components and associating the control panel with a SinGenerator block.
        /// </summary>
        /// <param name="sourceBlock">The SinGenerator block to be associated with this control panel.</param>
        public cpSinGenerator(SinGenerator sourceBlock) : base(sourceBlock) { InitializeComponent(); this._sourceBlock = sourceBlock; }

        /// <summary>
        /// Refreshes the control panel, updating the UI with the latest data from the SinGenerator block.
        /// This method is typically called in response to a refresh event, ensuring that the
        /// display reflects the current state of the SinGenerator's output. Updates the <see cref="ScopeMonitor"/> in the UI to show the generated sinus curves.
        /// </summary>
        /// <param name="o">The sender object of the refresh event (not used).</param>
        /// <param name="e">Event arguments associated with the refresh event (not used).</param>

        override protected void cpRefresh(object o, EventArgs e) { smSinGenerator.Update(_sourceBlock.Data as Vector); }
    }
    #endregion

    #region SinGenerator
    /// <summary>
    /// SinGenerator - Generates a signal with d sinusoidal components (<see cref="_componentCount"/>) with different phases:
    ///   $$ s_i = G*sin(2  π  nu t + (phi+10*i) ), i=1,...,d $$
    ///  <list type="bullet">
    ///  <item>d: Componenten Count</item>
    ///  <item>G: Amplitude</item>
    ///  <item>nu: Frequency</item>
    ///  <item>phi: phase</item>
    /// </list>
    ///    Useful if you want to test your stuff but have no real sensors attached to the system.
    ///    Specify the parameters in the cfg file as: [d G nu phi].
    /// </summary>
    /// <example>
    /// Example YAML configuration for the <see cref="SinGenerator"/> Block:
    /// <code>
    /// sinGenerator:
    ///   {
    ///     Type: SinGenerator,
    ///     Inputs: [ MyTimer ],
    ///     Params: [ 3, 2.5, 0.25, 1.57 ]
    ///   }
    /// </code>
    /// Explanation:
    /// - <c>Type: SinGenerator</c> indicates the block type.
    /// - <c>Inputs: [ MyTimer ]</c> shows the name of the one required input block, which must be a Timer.
    /// - <c>Params: [ 3, 2.5, 0.25, 1.57 ]</c> corresponds to:
    ///   - 3 = <c>d</c> (number of sinusoidal components)
    ///   - 2.5 = <c>G</c> (amplitude)
    ///   - 0.25 = <c>nu</c> (frequency, in Hz)
    ///   - 1.57 = <c>phi</c> (phase, in radians)
    ///   
    /// Or 
    /// 
    /// /// <code>
    /// sinGenerator:
    ///   {
    ///     Type: SinGenerator,
    ///     Inputs: [ MyTimer ],
    ///     // Only specifying the first parameter (d = 1):
    ///     Params: [ 1 ]
    ///   }
    /// </code>
    /// In this minimal configuration:
    /// - The number of sinusoidal components = 1
    /// - The default amplitude (G) is 1
    /// - The default frequency (nu) is 0.2
    /// - The default phase (phi) is 0
    ///
    /// The resulting output is a single sinusoidal component with the defaults for amplitude, frequency, and phase.
    /// </example>
    public class SinGenerator : Block
    {

        private Vector _signal;

        /// <summary>
        /// Count of sinusoidal components to be generated: d.
        /// </summary>
        private int _componentCount;

        /// <summary>
        /// Amplitude G
        /// </summary>
        private double _amplitude = 1;

        /// <summary>
        /// Frequency nu
        /// </summary>
        private double _freqeuncy = 0.2;

        /// <summary>
        /// Phase phi
        /// </summary>
        private double _phase = 0;

        /// <summary>
        /// Initializes a new instance of the SinGenerator class.
        /// </summary>
        /// <param name="name">The name of the SinGenerator instance.</param>
        /// <param name="desiredRate">The desired rate of signal generation.</param>
        /// <param name="inputCfg">Input configuration (unused in SinGenerator).</param>
        /// <param name="params">Parameters for signal generation: [componentCount (d), amplitude (G), frequency (nu), phase (phi)].</param>
        /// <param name="path">The path for any required external resources (unused in SinGenerator).</param>
        public SinGenerator(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }
        #endregion

        /// <summary>
        /// Configures the inputs and initializes the generator based on provided parameters.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a SinGenerator has one input only, and it must be a timer
            if (InputBlocks.Count != 1) throw new Exception($"SinGenerator {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"SinGenerator {Name}'s input must be a Timer.");

            // a SinGenerator's DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;

            // a SinGenerator's has 1-4 Params
            if (Params.Count < 1 || Params.Count > 4) throw new Exception($"SinGenerator {Name} must have 1 to 4 parameters.");

            // use params to define the sinusoidal signals
            double[] dParam = Array.ConvertAll(Params.ToArray(), double.Parse);
            _componentCount = (int)dParam[0];
            if (dParam.Count() >= 2) _amplitude = dParam[1];
            if (dParam.Count() >= 3) _freqeuncy = dParam[2];
            if (dParam.Count() >= 4) _phase = dParam[3];

            // build the _signal we send out
            _signal = Vector.Build.Dense(_componentCount);



            cp = new cpSinGenerator(this);
        }

        /// <summary>
        /// Generates and sends out the sinusoidal signal upon receiving a trigger from the timer.
        /// </summary>
        /// <param name="sender">The sender of the trigger (the Timer).</param>
        /// <param name="value">The value sent by the sender (unused).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            // so, whenever you get a nudge from a timer, generate the _signal and send it off
            foreach (var i in Enumerable.Range(0, _componentCount)) 
                _signal[i] = _amplitude * Math.Sin(2 * Math.PI * _freqeuncy * Utils.Now + (_phase + 10 * i));
            SendOutput(_signal);
        }
    }
}
