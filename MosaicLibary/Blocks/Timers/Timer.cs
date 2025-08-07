using System.Collections.Generic;

namespace MosaicLibary
{
    /// <summary>
    /// Serves as the base class for all timer types, providing common functionality to periodically trigger events.
    /// This abstract class defines the basic interface and behavior for timers, like <see cref="ScheduledTimer"/>, within the system.
    /// </summary>
    abstract public class Timer : Block
    {
        /// <summary>
        /// Indicates whether the timer is currently running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the Timer class.
        /// </summary>
        /// <param name="Name">The name of the timer.</param>
        /// <param name="DesiredRate">The desired firing rate of the timer in Hz.</param>
        /// <param name="InputCfg">Configuration settings for input blocks, not used in base Timer.</param>
        /// <param name="Params">Additional parameters for the timer, not used in base Timer.</param>
        /// <param name="Path">Path to additional resources or configuration files, not used in base Timer.</param>
        public Timer(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
        : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the Timer. This method is called during initialization to set up the timer's inputs.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();
        }

        /// <summary>
        /// Starts the timer, causing it to begin firing events at its configured rate.
        /// </summary>
        virtual public void Start()
        {
            IsRunning = true;
        }

        /// <summary>
        /// Stops the timer, preventing it from firing any further events.
        /// </summary>
        virtual public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Toggles the running state of the timer when a right mouse click event is detected.
        /// </summary>
        override public void MouseRightClick()
        {
            if (!IsRunning)
                Start();
            else
                Stop();
        }

        /// <summary>
        /// Fires an event periodically according to the timer's desired rate. This method is typically called by the scheduler.
        /// </summary>
        /// <param name="o">An optional object parameter that can be used to pass data with the event, not used in base Timer.</param>
        protected void Fire(object o)
        {
            SendOutput(null);
        }
    }
}
