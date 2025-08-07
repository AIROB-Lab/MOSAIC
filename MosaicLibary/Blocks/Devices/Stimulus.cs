    using System;
using System.Linq;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    #region ControlPanel
    /// <summary>
    /// Represents the control panel for the <see cref="Stimulus"/> block.
    /// This class provides a user interface for displaying the current state and target value of the Stimulus,
    /// and allows the user to start the stimulus process.
    /// </summary>
    public partial class cpStimulus : ControlPanel
    {
        /// <summary>
        /// The <see cref="Stimulus"/> block associated with this control panel.
        /// </summary>
        private Stimulus _sourceBlock;


        /// <summary>
        /// Initializes a new instance of the CpStimulus class, setting up the UI components and associating the control panel with a Stimulus block.
        /// </summary>
        /// <param name="sourceBlock">The Stimulus block to be associated with this control panel.</param>
        public cpStimulus(Stimulus sourceBlock) : base(sourceBlock) { InitializeComponent(); this._sourceBlock = sourceBlock; }

        /// <summary>
        /// Refreshes the control panel, updating the UI with the latest data from the Stimulus block.
        /// This method updates the stimulus state and target value display, and enables or disables the start button based on the running state.
        /// </summary>
        /// <param name="sender">The sender of the refresh event (unused).</param>
        /// <param name="e">Event arguments associated with the refresh event (unused).</param>
        override protected void cpRefresh(object sender, EventArgs e)
        {
            lblStimulus.Text = _sourceBlock.State;
            lblTargetValue.Text = Utils.Print(_sourceBlock.TargetValue);
            btnStart.Enabled = !_sourceBlock.IsRunning;
        }

        /// <summary>
        /// Handles the Start button click event to initiate the stimulus process.
        /// </summary>
        /// <param name="sender">The sender of the click event.</param>
        /// <param name="e">Event arguments associated with the click event.</param>
        private void btnStart_Click(object sender, EventArgs e)
        { 
            _sourceBlock.Start(); 
        }
    }
    #endregion

    #region Stimulus
    /// <summary>
    /// Represents a stimulus generator that implements a Finite-State Machine (FSM) to output structured flows of target values.
    /// It can simulate a sequence of tasks, such as showing a series of targets in a chronological order, useful for testing in the absence of real sensors.
    /// Configuration parameters should specify the duration of states and tasks as: [captureDuration, task1, task2, ...].
    /// 
    /// </summary>
    /// <example>
    /// >  The Input Block must have a "-" infront of it's name in the YAML file. This insures the Stimuls runs on the correct Desired Rate but is not triggered by the Input Block.
    /// 
    /// <code>
    /// visual_stim: { 
    ///   Type: Stimulus, 
    ///   Inputs: [ -timer ], 
    ///   Params: [ 4.0,
    ///       0;0;0;0;0;0;0;0;0;0;0;0,   # rest 
    ///       1;1;1;1;1;1;0;0;0;0;0;0,   # hand close
    ///       0;0;0;0;0;0;1;0;0;0;0;0,   # wr_flex
    ///       0;0;0;0;0;0;0;1;0;0;0;0,   # wr_ext
    ///       0;0;0;0;0;0;0;0;1;0;0;0,   # wr_pron
    ///       0;0;0;0;0;0;0;0;0;1;0;0,   # wr_sup
    ///       0;0;0;0;0;0;0;0;0;0;1;0,   # wr_ulnar
    ///       0;0;0;0;0;0;0;0;0;0;0;1,   # wr_radial
    ///       1;0;0;0;0;0;0;0;0;0;0;0,   # throt
    ///       0;1;0;0;0;0;0;0;0;0;0;0,   # thflex
    ///       0;0;1;0;0;0;0;0;0;0;0;0,   # indflex
    ///       0;0;0;1;0;0;0;0;0;0;0;0,   # midflex
    ///       0;0;0;0;1;0;0;0;0;0;0;0,   # ringflex
    ///       0;0;0;0;0;1;0;0;0;0;0;0    # litflex
    ///   ]
    /// }
    /// </code>
    /// 
    /// Explanation:
    /// <list type="bullet">
    ///   <item><description><c>Type: Stimulus</c> specifies the Block type to instantiate.</description></item>
    ///   <item><description><c>Inputs: [ -timer ]</c> indicates the Stimulus uses one input, a <see cref="ScheduledTimer"/> block named "timer".</description></item>
    ///   <item><description><c>Params: [ 4.0, ... ]</c>:
    ///     <list type="number">
    ///       <item><description>The first value (<c>4.0</c>) sets the duration (in seconds) for the "capture" state.</description></item>
    ///       <item><description>Each subsequent entry is a semicolon-delimited string of 12 numbers, describing a "task" vector of length 12 
    ///       (e.g., <c>1;1;1;1;1;1;0;0;0;0;0;0</c> for hand close). These tasks cycle through "rest", "rise", "hold", "capture", and "fall" states in a finite-state machine.</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>The Stimulus FSM automatically transitions through states (e.g., <c>rest</c> → <c>rise</c> → <c>hold</c> → <c>capture</c> → <c>fall</c> → next task), 
    ///   ending with <c>end</c> once all tasks are done.</description></item>
    /// </list>
    /// 
    /// This example shows how you can define a sequence of movements or targets (rest, power grip, wrist flex, etc.) and 
    /// capture data for a specified duration in each cycle, all without requiring real sensor input.
    /// </example>
    public class Stimulus : Block
    {
        /// <summary>
        /// The instance of a <see cref="Timer"/> Block that defines the Desired Rate of the Stimulus.
        /// </summary>
        private Block _inputTimer;


        /// <summary>
        /// List of Task, defined in the cfg YAMl file.
        /// </summary>
        private List<Vector> _tasks = new List<Vector>();


        private double _deltaT;
        private double _transT;

        /// <summary>
        /// A dictionary representing the states of the FSM and their durations.
        /// </summary>
        private Dictionary<string, double> _states = new Dictionary<string, double>()
        {
            { "rest", 2.0 }, { "rise", 1.0 }, { "hold", 2.0 }, { "capture", 5.0 }, { "fall", 1.0 }, { "end", 0 }
        };

        /// <summary>
        /// The current state of the FSM.
        /// </summary>
        public string State { get; private set; }


        /// <summary>
        /// The target value output by the current state. This value is intended for external observation
        /// and interaction, such as rendering in a user interface or further processing by another system component.
        /// </summary>
        public Vector TargetValue { get; private set; }

        /// <summary>
        /// Represents a null target value to be used as the output in states where no specific action is required.
        /// </summary>
        private Vector _nullTargetValue = Vector.Build.Dense(12, 0);

        /// <summary>
        /// Index of the current task being executed.
        /// </summary>
        private int _currentTaskIdx;

        /// <summary>
        /// Indicates whether the Stimulus FSM is actively running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Stimulus class.
        /// </summary>
        /// <param name="name">The name of the stimulus instance.</param>
        /// <param name="desiredRate">The desired rate of stimulus generation, inherited from the driving Timer.</param>
        /// <param name="inputCfg">Input configuration (unused in Stimulus).</param>
        /// <param name="params">Parameters for stimulus generation: [captureDuration, task1, task2, ...].</param>
        /// <param name="path">The path for any required external resources (unused in Stimulus).</param>

        public Stimulus(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }


        /// <summary>
        /// Configures the inputs and initializes the generator based on provided parameters.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Stimulus has one input only, and it must be a timer
            if (InputBlocks.Count != 1)
                throw new Exception($"Stimulus {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer))
                throw new Exception($"Stimulus {Name}'s input must be a Timer.");

            _inputTimer = InputBlocks[0];

            // a Stimulus's DesiredRate is the same as the driving Timer
            DesiredRate = _inputTimer.DesiredRate;

            // a SinGenerator's has at least two parameters (the duration of the capture state plus one task)
            if (Params.Count < 1)
                throw new Exception($"Stimulus {Name} must have at least 2 parameters (capture duration and one task).");

            // gather duration of the capture state
            _states["capture"] = Convert.ToDouble(Params[0]);

            // gather the tasks in Params[1:end]
            foreach (var task in Params.Skip(1).Take(Params.Count - 1).ToArray())
                _tasks.Add(Vector.Build.Dense(Array.ConvertAll(task.Split(';'), double.Parse)));

            // the initial state is the "rest", i.e., the first in the dictionary
            State = _states.ElementAt(0).Key;

            cp = new cpStimulus(this);
        }

        /// <summary>
        /// Starts the stimulus generation process by subscribing to the input timer and initializing the FSM.
        /// </summary>
        public void Start()
        {
            SubscribeTo(_inputTimer);
            _transT = Utils.Now;
            State = _states.ElementAt(0).Key; IsRunning = true;
        }


        /// <summary>
        /// Responds to new input signals by adjusting the state of the FSM and generating the appropriate target values.
        /// This method embodies the core logic of the Stimulus FSM, dictating the flow and transitions between states
        /// based on elapsed time and the sequence of tasks.
        /// </summary>
        /// <param name="sender">The source of the input signal, typically a timer.</param>
        /// <param name="value">The input value, not used in this implementation as the state transitions are time-based.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            _deltaT = Utils.Now - _transT;

            switch (State)
            {
                case "rest":
                    // REST: keep on sending a rest value, wait till your time is up
                    TargetValue = _nullTargetValue;
                    WaitThenMoveTo("rise");
                    break;
                case "rise":
                    // RISE: show a smooth transition from rest to the next Task
                    TargetValue = _tasks[_currentTaskIdx] * Math.Pow(Math.Sin(_deltaT * (Math.PI / 2) / _states[State]), 2);
                    WaitThenMoveTo("hold");
                    break;
                case "hold":
                    // HOLD: stay like this for a while
                    TargetValue = _tasks[_currentTaskIdx];
                    WaitThenMoveTo("capture");
                    break;
                case "capture":
                    // CAPTURE: stay like this, but longer - in this state something should happen
                    //   (capture data or check whether a target is being reached)
                    TargetValue = _tasks[_currentTaskIdx];
                    WaitThenMoveTo("fall");
                    break;
                case "fall":
                    // FALL: show a smooth transition from the current Task to rest
                    TargetValue = _tasks[_currentTaskIdx] * Math.Pow(Math.Cos(_deltaT * (Math.PI / 2) / _states[State]), 2);
                    if (_deltaT > _states[State])
                        if (++_currentTaskIdx == _tasks.Count) MoveTo("end");
                        else MoveTo("rest");
                    break;
                case "end":
                    // END: finish it off.
                    UnsubscribeFrom(_inputTimer);
                    _currentTaskIdx = 0;
                    IsRunning = false;
                    break;
            }

            SendOutput((State, TargetValue));
        }

        private void WaitThenMoveTo(string NextState)
        {
            if (_deltaT > _states[State])
                MoveTo(NextState);
        }
        /// <summary>
        /// Moves the FSM to the specified state and updates the transition time.
        /// </summary>
        /// <param name="NextState"></param>
        public void MoveTo(string NextState)
        {
            State = NextState;
            _transT = Utils.Now;
        }
    }
    #endregion
}
