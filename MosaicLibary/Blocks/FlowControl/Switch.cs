using System;
using System.Collections.Generic;

namespace MosaicLibary
{
    /// <summary>
    /// Represents the control panel for the <see cref="Switch"/> block.
    /// Provides a user interface to interact with the Switch block and toggle between inputs.
    /// </summary>
    public partial class cpSwitch : ControlPanel
    {
        /// <summary>
        /// The <see cref="Switch"/> block associated with this control panel.
        /// </summary>
        private Switch _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpSwitch"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The Switch block associated with this control panel.</param>
        public cpSwitch(Switch _SourceBlock) : base(_SourceBlock)
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }

        /// <summary>
        /// Handles the click event of the switch button.
        /// Toggles the active input of the associated Switch block.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnSwitch_Click(object sender, EventArgs e) 
        { 
            _sourceBlock.Switch01(); 
        }
    }

    /// <summary>
    /// Represents a Switch block that toggles between two input blocks.
    /// This block allows dynamic switching of its input source and forwards the selected input to its output.
    /// </summary>
    /// <example>
    /// >  The second Input Block must have a "-" in front of the name. The first input block is the selected Block at Start.
    /// 
    /// The <see cref="Switch"/> Block need to Input Blocks. 
    /// switch: 
    /// { 
    ///     Type: Switch, 
    ///     Inputs: [ name of Block 1, -name of Block 2 ] }
    /// </example>
    public class Switch : Block
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Switch"/> class.
        /// </summary>
        /// <param name="Name">The name of the Switch block.</param>
        /// <param name="DesiredRate">The desired rate of the Switch block.</param>
        /// <param name="InputCfg">The input configuration for the Switch block.</param>
        /// <param name="Params">The parameters for the Switch block.</param>
        /// <param name="Path">The path related to the Switch block.</param>
        public Switch(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the Switch block.
        /// Ensures that exactly two input blocks are provided and sets the initial subscription to one of the input blocks.
        /// </summary>
        /// <exception cref="Exception">Thrown if the number of input blocks is not exactly two.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Switch has two inputs
            if (InputBlocks.Count != 2) throw new Exception($"Switch {Name} must have 2 input blocks.");

            // a Switch's DesiredRate is that of the Block which is let go right now. initially, the zero'th.
            DesiredRate = InputBlocks[0].DesiredRate;

            cp = new cpSwitch(this);
        }

        /// <summary>
        /// Handles the right mouse click event.
        /// Toggles the active input of the Switch block.
        /// </summary>
        override public void MouseRightClick() 
        { 
            Switch01(); 
        }

        /// <summary>
        /// Toggles the subscription between the two input blocks.
        /// Updates the desired rate to match the newly subscribed input block.
        /// </summary>
        public void Switch01()
        {
            // and this is it: switch the subscription to the blocks 
            if (IsSubscribedTo(InputBlocks[0])) 
            { 
                UnsubscribeFrom(InputBlocks[0]); 
                SubscribeTo(InputBlocks[1]); DesiredRate = InputBlocks[1].DesiredRate; 
            }
            else 
            { 
                UnsubscribeFrom(InputBlocks[1]); 
                SubscribeTo(InputBlocks[0]); DesiredRate = InputBlocks[0].DesiredRate; 
            }
        }

        /// <summary>
        /// Forwards any received input value from the currently subscriped to input block to the output without modification.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input value to be forwarded.</param>
        override protected void OnNewInput(Block sender, object value) 
        { 
            SendOutput(value); 
        }
    }
}
