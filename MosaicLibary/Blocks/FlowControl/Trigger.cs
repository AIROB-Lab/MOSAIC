using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using System.Threading.Tasks;

namespace MosaicLibary
{
    /// <summary>
    /// Represents the control panel for the <see cref="Trigger"/> block.
    /// Provides a user interface to select and send actions or commands from the Trigger block.
    /// </summary>
    public partial class cpTrigger : ControlPanel
    {
        /// <summary>
        /// The <see cref="Trigger"/> block associated with this control panel.
        /// </summary>
        private Trigger _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpTrigger"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The Trigger block associated with this control panel.</param>
        public cpTrigger(Trigger _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = _SourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel with the current action and capturing status.
        /// </summary>
        /// <param name="o">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void cpRefresh(object o, EventArgs e)
        {
            lblStimulus.Text = _sourceBlock.Actions.ElementAt(_sourceBlock._currentIdx).Key;
            lblStimulus.BackColor = _sourceBlock._capturing ? Color.Red : Color.Transparent;
        }

        /// <summary>
        /// Handles the KeyDown event for the Trigger control panel.
        /// Sends the current action value when the down arrow key is pressed.
        /// </summary>
        private void cpTriggerKeyDown(object sender, KeyEventArgs e)
        {
            // Avoid sending a value while a key is being pressed.
            if (_sourceBlock._capturing) 
                return;

            // Handle key presses.
            switch (e.KeyCode)
            {
                case Keys.Down:
                    _sourceBlock.Send(_sourceBlock.Actions.ElementAt(_sourceBlock._currentIdx).Value);
                    _sourceBlock._capturing = true;
                    break;
            }
        }

        /// <summary>
        /// Handles the KeyUp event for the Trigger control panel.
        /// Toggles between actions or sends a null value when keys are released.
        /// </summary>
        private void cpTriggerKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right: // Cycle to the next action.
                    if (++_sourceBlock._currentIdx == _sourceBlock.Actions.Count) 
                        _sourceBlock._currentIdx = 0;
                    break;
                case Keys.Left: // Cycle to the previous action.
                    if (--_sourceBlock._currentIdx < 0) 
                        _sourceBlock._currentIdx = _sourceBlock.Actions.Count - 1;
                    break;
                case Keys.Down: // Send a null value to stop the action.
                    _sourceBlock.Send(null);
                    _sourceBlock._capturing = false;
                    break;
            }

            // Handle number keys to acquire data for a specific duration.
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                int keySec = e.KeyValue - (int)Keys.D0;
                _sourceBlock._capturing = true;
                Console.WriteLine("Starting acquisition " + DateTime.Now.ToString("ss,fff"));
                _sourceBlock.Send(_sourceBlock.Actions.ElementAt(_sourceBlock._currentIdx).Value);
                _sourceBlock.SendNullInXSeconds(keySec);
            }
        }
    }

    /// <summary>
    /// Represents a Trigger block that sends predefined actions or commands to downstream blocks.
    /// Useful for starting/stopping data capture or providing target values for a learning machine.
    /// Manually used through the control panel to select and send actions.
    /// Change action by roatating through them via the left and right arrow keys.
    /// Start recording by pressing the down arrow key and stop by releasing it.
    /// Or press a number key between 1-9 to start recording for a specific duration.
    /// </summary>
    /// <example>
    /// Actions are similar defiend as in <see cref="Stimulus"/>. 
    /// capture_data: 
    /// { 
    ///     Type: Trigger, 
    ///     Params: [
    ///         rest:0;0;0;0;0;0;0;0;0;0;0;0,
    ///         power:1;1;1;1;1;1;0;0;0;0;0;0,
    ///         flex:0;0;0;0;0;0;1;0;0;0;0;0,
    ///         extend:0;0;0;0;0;0;0;1;0;0;0;0,
    ///         pron:0;0;0;0;0;0;0;0;1;0;0;0,
    ///         sup:0;0;0;0;0;0;0;0;0;1;0;0
    ///         ]
    ///}
    /// </example>
public class Trigger : Block
    {
        /// <summary>
        /// Indicates whether the Trigger is currently capturing data.
        /// </summary>
        internal bool _capturing;

        /// <summary>
        /// A dictionary of actions, where the key is the action name and the value is a vector of activation values.
        /// </summary>
        public Dictionary<string, Vector> Actions { get; private set; } = new Dictionary<string, Vector>();

        /// <summary>
        /// The index of the current action in the <see cref="Actions"/> dictionary.
        /// </summary>
        public int _currentIdx;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trigger"/> class.
        /// </summary>
        /// <param name="Name">The name of the Trigger block.</param>
        /// <param name="DesiredRate">The desired rate of the Trigger block.</param>
        /// <param name="InputCfg">The input configuration for the Trigger block (should be empty).</param>
        /// <param name="Params">The parameters defining the actions for the Trigger block.</param>
        /// <param name="Path">The file system path related to the Trigger block.</param>
        public Trigger(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the Trigger block.
        /// Ensures the block has no input blocks and initializes the actions from the parameters.
        /// </summary>
        /// <exception cref="Exception">Thrown if the block has inputs or insufficient parameters.</exception>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            // Ensure the Trigger block has no input blocks.
            if (InputBlocks.Count != 0)
                throw new Exception($"Trigger {Name} must have no input blocks.");

            // Set the desired rate to zero since the output is sporadic.
            DesiredRate = 0;

            // Ensure the Trigger block has at least one parameter.
            if (Params.Count < 1)
                throw new Exception($"Trigger {Name} must have at least 1 parameter.");

            // Parse actions from the parameters.
            foreach (var action in Params)
            {
                string[] actionElements = action.Split(':');
                string actionName = actionElements[0];
                double[] actionTargetValues = Array.ConvertAll(actionElements[1].Split(';'), double.Parse);
                Actions.Add(actionName, Vector.Build.Dense(actionTargetValues));
            }

            // Initialize the control panel.
            cp = new cpTrigger(this);
        }

        /// <summary>
        /// Sends the specified object as output.
        /// </summary>
        /// <param name="o">The object to send (e.g., an action vector or null).</param>
        public void Send(object o)
        {
            SendOutput(o);
        }

        /// <summary>
        /// Handles unexpected input events, which are invalid for the Trigger block.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input value.</param>
        /// <exception cref="Exception">Always thrown to indicate invalid input.</exception>
        protected override void OnNewInput(Block sender, object value)
        {
            throw new Exception($"Trigger {Name} received an input event.");
        }

        /// <summary>
        /// Sends a null value after a specified duration to stop the current action.
        /// </summary>
        /// <param name="seconds">The duration in seconds to wait before sending null.</param>
        internal async void SendNullInXSeconds(float seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            Send(null);
            Console.WriteLine("Stopping acquisition: " + DateTime.Now.ToString("ss,fff"));
            _capturing = false;
        }
    }
}
