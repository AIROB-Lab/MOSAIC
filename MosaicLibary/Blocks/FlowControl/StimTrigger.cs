using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// StimTrigger converts the output event from a <see cref="Stimulus"/> into commands that a Buffer can use
    /// to start or stop gathering data. As long as the <see cref="Stimulus"/>  is in the "capture" state, it sends out
    /// the associated target value; as soon as the <see cref="Stimulus"/> exits the "capture" state, it sends off a null value, just once.
    /// </summary>
    public class StimTrigger : Block
    {
        /// <summary>
        /// Indicates whether the StimTrigger is currently in the capturing state.
        /// </summary>
        private bool _isCapturing;

        /// <summary>
        /// Initializes a new instance of the StimTrigger class.
        /// </summary>
        /// <param name="Name">The name of the StimTrigger.</param>
        /// <param name="DesiredRate">The desired rate of the StimTrigger.</param>
        /// <param name="InputCfg">The input configuration for the StimTrigger.</param>
        /// <param name="Params">The parameters for the StimTrigger.</param>
        /// <param name="Path">The path for the StimTrigger.</param>
        public StimTrigger(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the StimTrigger. Ensures there is exactly one input, and it is of type Stimulus.
        /// </summary>
        /// <exception cref="Exception">Thrown when the number of input blocks is not 1 or the input block is not a Stimulus.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a StimTrigger has one input, and it must be a Stimulus
            if (InputBlocks.Count != 1) throw new Exception($"StimTrigger {Name} must have 1 input block.");
            if (!(InputBlocks[0] is Stimulus)) throw new Exception($"StimTrigger {Name} can only have a Stimulus as input block.");

            // a StimTrigger's DesiredRate is that of my input Stimulus
            DesiredRate = InputBlocks[0].DesiredRate;
        }

        /// <summary>
        /// Handles new input from the connected Stimulus block. Outputs the second element of a tuple during "capture" state
        /// and sends a null value once when exiting the capture state.
        /// </summary>
        /// <param name="sender">The sender of the input event.</param>
        /// <param name="value">The value of the input event, expected to be a tuple of (string, Vector).</param>
        override protected void OnNewInput(Block sender, object value)
        {
            var (state, tv) = ((string, Vector))value;

            // send off the second element of a tuple only while receiving a "capture" state from the Stimulus;
            // on exiting the state, send a null value, just once.
            if (state == "capture")
            {
                SendOutput(tv);
                _isCapturing = true;
            }
            else if (state != "capture" && _isCapturing)
            {
                SendOutput(null);
                _isCapturing = false;
            }
        }
    }
}