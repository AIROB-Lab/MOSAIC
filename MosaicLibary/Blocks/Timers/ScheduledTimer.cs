using System;
using System.Collections.Generic;

namespace MosaicLibary
{
    
    #region ControlPanel

    /// <summary>
    /// Represents the control panel for a <see cref="ScheduledTimer"/>, providing a user interface to start and stop the timer.
    /// </summary>
    public partial class cpScheduledTimer : ControlPanel
    {
        /// <summary>
        /// The source <see cref="ScheduledTimer"/> block associated with this control panel.
        /// </summary>
        private ScheduledTimer _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpScheduledTimer"/> control panel.
        /// </summary>
        /// <param name="sourceBlock">The ScheduledTimer to be controlled by this panel.</param>
        public cpScheduledTimer(ScheduledTimer sourceBlock) : base(sourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = sourceBlock; 
        }

        /// <summary>
        /// Refreshes the control panel, updating the start/stop button text based on the timer's running state.
        /// </summary>
        /// <param name="o">The sender of the refresh request.</param>
        /// <param name="e">The event arguments.</param>
        override protected void cpRefresh(object o, EventArgs e) 
        { 
            cbStartStop.Text = _sourceBlock.IsRunning ? "Stop" : "Start"; 
        }

        /// <summary>
        /// Handles the change event of the start/stop checkbox, starting or stopping the ScheduledTimer as appropriate.
        /// </summary>
        /// <param name="sender">The checkbox sender.</param>
        /// <param name="e">The event arguments.</param>
        void cbStartStop_CheckedChanged(object sender, EventArgs e) 
        { 
            if (!_sourceBlock.IsRunning) 
                _sourceBlock.Start(); 
            else 
                _sourceBlock.Stop(); 
        }
    }
    #endregion

    #region ScheduledTimer 
    /// <summary>
    /// A extensions of the <see cref="Timer"/> block that fires events periodically with high accuracy, using the <see cref="Scheduler"/> for precise timing.
    /// This block is designed to trigger actions in other blocks at fixed intervals.
    /// </summary>
    /// <example>
    /// <code>
    /// timerFast: 
    /// { 
    ///     Type: ScheduledTimer,  
    ///     DesiredRate: 200 
    /// }
    /// </code>
    /// </example>
    public class ScheduledTimer : Timer
    {
        /// <summary>
        /// Initializes a new instance of the ScheduledTimer class.
        /// </summary>
        /// <param name="Name">The name of the timer.</param>
        /// <param name="DesiredRate">The desired rate (in Hz) at which the timer should fire events.</param>
        /// <param name="InputCfg">Input configuration, not used for this timer.</param>
        /// <param name="Params">Additional parameters, not used for this timer.</param>
        /// <param name="Path">Path for additional configuration or resources, not used for this timer.</param>
        public ScheduledTimer(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the ScheduledTimer, ensuring it has no input blocks and a valid desired rate.
        /// </summary>
        /// <exception cref="Exception">Thrown if input blocks are declared or if no desired rate is defined.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a scheduled timer must have no inputs
            if (InputBlocks.Count != 0) 
                throw new Exception($"Input blocks declared for Timer {Name}.");

            // a scheduled timer must have a rate
            if (DesiredRate <= 0)
                throw new Exception($"No DesiredRate defined for Timer {Name}.");

            cp = new cpScheduledTimer(this);
        }

        /// <summary>
        /// Starts the timer, scheduling its firing events according to the desired rate, by adding them to the <see cref="Scheduler"/>.
        /// </summary>
        override public void Start() 
        { 
            base.Start(); 
            Scheduler.Instance.Add(Fire, DesiredRate); 
        }

        /// <summary>
        /// Stops the timer, removing its firing events from the <see cref="Scheduler"/>.
        /// </summary>
        override public void Stop() 
        { 
            base.Stop(); Scheduler.Instance.Remove(Fire); 
        }

        /// <summary>
        /// ScheduledTimer does not process input events. Throws an exception if an input event is received.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="value">The value of the input event.</param>
        /// <exception cref="Exception">Always thrown because this timer should not receive input events.</exception>
        override protected void OnNewInput(Block sender, object value) 
        { 
            throw new Exception($"ScheduledTimer {Name} received an input event."); 
        }
    }
    #endregion
}
