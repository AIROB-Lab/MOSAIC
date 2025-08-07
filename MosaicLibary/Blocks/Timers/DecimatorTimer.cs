using System;
using System.Collections.Generic;

namespace MosaicLibary
{

    /// <summary>
    /// Provides functionality to decimate (thin out) the output of a scheduled Timer based on a specified decimation factor.
    /// This class acts as a Timer that triggers less frequently than its input timer by a defined factor <see cref="_decimationFactor"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// decimatorTimer:
    /// {
    ///   Type: DecimatorTimer,
    ///   Inputs: [ name of Timer to be decimateted ],
    ///   DesiredRate: 25
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Inputs: [ name of Timer ]</c> points to the input <see cref="ScheduledTimer"/> whose events are decimated.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>DesiredRate: 25</c> indicates how many times per second (Hz) the <c>DecimatorTimer</c> 
    ///     should fire, which must be slower than the Input timer's rate.</description>
    ///   </item>
    /// </list>
    /// </example>
    public class DecimatorTimer : Timer
    {
        /// <summary>
        /// The input timer whose output is to be decimated.
        /// </summary>
        private Timer _inputTimer;

        /// <summary>
        /// Counter used to determine when to trigger based on the decimation factor.
        /// </summary>
        private int _decimationCounter = 0;


        /// <summary>
        /// The factor by which the input timer's output frequency is reduced.
        /// </summary>
        private int _decimationFactor = 1;


        /// <summary>
        /// Initializes a new instance of the DecimatorTimer class.
        /// </summary>
        /// <param name="Name">The name of the timer.</param>
        /// <param name="DesiredRate">The desired rate at which this timer should trigger relative to the input timer.</param>
        /// <param name="InputCfg">Input configuration for the timer.</param>
        /// <param name="Params">Parameters for the timer's configuration.</param>
        /// <param name="Path">Path for additional configuration or resources.</param>
        public DecimatorTimer(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }


        /// <summary>
        /// Configures the inputs for the DecimatorTimer. Validates that there is exactly one input block, and it is a <see cref="Timer"/>.
        /// The desired rate of the DecimatorTimer must be slower than that of the input Timer.
        /// </summary>
        /// <exception cref="Exception">Thrown when the input block count is not one, the input block is not a Timer, or the desired rate is invalid.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a DecimatorTimer has one input only, and it must be a timer
            if (InputBlocks.Count != 1) 
                throw new Exception($"DecimatorTimer {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) 
                throw new Exception($"DecimatorTimer {Name}'s input must be a Timer.");
            
            _inputTimer = InputBlocks[0] as Timer;

            // the DesiredRate must be slower than that of the input timer
            if (DesiredRate <= 0 || DesiredRate > _inputTimer.DesiredRate) 
                throw new Exception($"DecimatorTimer {Name}'s DesiredRate cannot be <=0 or faster than the input timer's.");
           
            _decimationFactor = (int)(_inputTimer.DesiredRate / DesiredRate);

            // are we subscribed to the input timer or not? set IsRunning accordingly
            IsRunning = IsSubscribedTo(_inputTimer);
        }

        /// <summary>
        /// Starts the DecimatorTimer by subscribing to the input timer.
        /// </summary>
        override public void Start()
        {
            base.Start();
            SubscribeTo(_inputTimer);
        }

        /// <summary>
        /// Stops the DecimatorTimer by unsubscribing from the input timer.
        /// </summary>
        override public void Stop()
        {
            base.Stop();
            UnsubscribeFrom(_inputTimer);
        }

        /// <summary>
        /// Processes new input from the subscribed input timer. Triggers the DecimatorTimer based on the decimation factor.
        /// </summary>
        /// <param name="sender">The block sending the input, which should be the input timer.</param>
        /// <param name="value">The input value from the timer, not used in this context.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            if (_decimationCounter++ % _decimationFactor == 0)
                Fire(null);
        }
    }
}
