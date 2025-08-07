using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    #region Control Panel
    /// <summary>
    /// Provides a user interface for interacting with the <see cref="TAC"/> block, displaying the current status,
    /// distance to target, and time in target to help monitor task accomplishment conditions.
    /// </summary>
    public partial class cpTAC : ControlPanel
    {
        /// <summary>
        /// The TAC block associated with this control panel.
        /// </summary>
        private TAC _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the CpTAC control panel.
        /// </summary>
        /// <param name="sourceBlock">The TAC block this panel is associated with.</param>
        public cpTAC(TAC sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel, updating the status label to reflect the current distance to target,
        /// time in target, and whether the TAC block is actively trying to accomplish the task.
        /// </summary>
        /// <param name="sender">The sender of the refresh event.</param>
        /// <param name="e">Event arguments.</param>
        override protected void cpRefresh(object o, EventArgs e)
        {
            if (_sourceBlock.Trying)
            {
                if (_sourceBlock.TimeInTarget > 0.01) lblStatus.BackColor = System.Drawing.Color.LightGreen;
                else lblStatus.BackColor = System.Drawing.Color.PaleVioletRed;
                lblStatus.Text = _sourceBlock.DistanceToTarget.ToString("0.00") + ";" + _sourceBlock.TimeInTarget.ToString("0.00");
            }
            else
            {
                lblStatus.BackColor = System.Drawing.Color.LightGray;
                lblStatus.Text = "not running";
            }
        }
    }
    #endregion

    #region TAC
    /// <summary>
    /// Represents a block designed to implement a Target Achievement Control (TAC) test.
    /// </summary>
    /// <remarks>
    /// The Target Achievement Control (TAC) Test is an assessment tool used to evaluate the performance 
    /// of myoelectric prosthetic control. It measures a user's proficiency in controlling a virtual 
    /// prosthetic limb to achieve and maintain certain target positions for a specific duration. This 
    /// test presents real-world challenges such as unintentional movements correction and proportional 
    /// control, providing a comprehensive measure of a user's ability to recognize myoelectric patterns. 
    /// It serves as a more realistic and challenging benchmark for evaluating control algorithms and 
    /// user adaptability in a virtual environment. 
    /// <para>[Simon et al.](https://www.rehab.research.va.gov/jour/11/486/pdf/page619.pdf)</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// # YAML Example Configuration
    /// 
    /// # 1) Configure a Stimulus block that provides changing target values
    /// TAC_stim: 
    /// {
    ///   Type: Stimulus,
    ///   Inputs: [ -timer_subs ],
    ///   Params: [ 
    ///     20.0,                # capture state duration (seconds)
    ///     0.8;0.8;0.8;0.8;0.8;0.8;0;0;0;0;0;0,  # various hand/wrist positions
    ///     0;0;0;0;0;0;0.8;0;0;0;0;0,
    ///     0;0;0;0;0;0;0;0.8;0;0;0;0,
    ///     0.5;0.5;0.5;0.5;0.5;0.5;0;0;0;0;0;0,
    ///     0;0;0;0;0;0;0.5;0;0;0;0;0,
    ///     0;0;0;0;0;0;0;0.5;0;0;0;0,
    ///     0.7;0.7;0.7;0.7;0.7;0.7;0.5;0;0;0;0;0,
    ///     0.7;0.7;0.7;0.7;0.7;0.7;0;0.5;0;0;0;0,
    ///     0.5;0.5;0.5;0.5;0.5;0.5;0.3;0;0;0;0;0,
    ///     0.5;0.5;0.5;0.5;0.5;0.5;0;0.3;0;0;0;0
    ///   ]
    /// }
    ///
    /// # 2) A Selector block that extracts the target values from TAC_stim
    /// TAC_stim_tv: 
    /// {
    ///   Type: Selector,
    ///   Inputs: [ TAC_stim ]
    /// }
    ///
    /// # 3) (Optional) A BlenderArm block for visual feedback
    /// blender_white_arm_ctrl:
    /// {
    ///   Type: BlenderArm,
    ///   Inputs: [ TAC_stim_tv ],
    ///   Params: [ localhost, 3333 ]
    /// }
    ///
    /// # 4) The TAC block itself compares the stimulus target vs. 
    /// #    an incoming vector from incremental learning or a predictor
    /// increm_learning_TAC: 
    /// {
    ///   Type: TAC,
    ///   Inputs: [ TAC_stim_tv, incr_learn_prediction ],
    ///   Params: [ TAC_stim, 2.0, 0.2 ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>TAC_stim</c> is a <c>Stimulus</c> block that generates target values over time. 
    ///     Its Params define the capture duration (e.g., 20.0 seconds) and a sequence of hand/wrist positions 
    ///     for the user to match.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>TAC_stim_tv</c> is a <c>Selector</c> block used to extract or transform the data 
    ///     from <c>TAC_stim</c> before sending it to other blocks (e.g., a <c>BlenderArm</c> for visualization). 
    ///     The exact function depends on how <c>Stimulus</c> outputs its data.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>incr_learn_prediction</c> could be an incremental learning or predictive block 
    ///     that outputs the user's current hand/wrist posture estimate.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>increm_learning_TAC</c> (the TAC block) takes the target values 
    ///     (<c>TAC_stim_tv</c>) and the predicted posture (<c>incr_learn_prediction</c>), 
    ///     checking how well the user is matching the stimulus. The Params array includes:
    ///       <list type="number">
    ///         <item><description><c>TAC_stim</c>: Reference to the Stimulus block for state checks.</description></item>
    ///         <item><description><c>2.0</c>: The success timeout duration (seconds) the user must maintain the target.</description></item>
    ///         <item><description><c>0.2</c>: The success threshold for how close the user needs to be to the target.</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// In this setup, the TAC block continuously compares the user's posture (from 
    /// <c>incr_learn_prediction</c>) against the target posture (from <c>TAC_stim_tv</c>). If the user remains 
    /// within <c>0.2</c> L2 distance of the target for <c>2.0</c> seconds (while <c>TAC_stim</c> is in "capture" mode), 
    /// the test automatically triggers a beep and transitions the stimulus to the next state.
    /// </example>
    public class TAC : Block
    {
        /// <summary>
        /// Timeout duration for success condition.
        /// </summary>
        private double _successTimeOut;

        /// <summary>
        /// Threshold for determining if targets are sufficiently close.
        /// </summary>
        private double _successThreshold;

        /// <summary>
        /// Timestamp when targets last entered the success condition threshold.
        /// </summary>
        private double _enteredTarget;

        /// <summary>
        /// The stimulus provider used to trigger task start and stop conditions.
        /// </summary>
        private Stimulus _stimulusProvider;

        /// <summary>
        /// Input blocks providing the target values for comparison.
        /// </summary>
        private Block _inputA, _inputB;

        /// <summary>
        /// Vector a for distance Evaluation
        /// </summary>
        private Vector _a = Vector.Build.Dense(12);

        /// <summary>
        /// Vector b for distance Evaluation
        /// </summary>
        private Vector _b = Vector.Build.Dense(12);

        /// <summary>
        /// Indicates whether the TAC block is currently attempting to meet the success condition.
        /// </summary>
        public bool Trying { get; private set; }

        /// <summary>
        /// The current distance between the two target values.
        /// </summary>
        public double DistanceToTarget { get; private set; }

        /// <summary>
        /// Amount of time targets have been within the success condition threshold.
        /// </summary>
        public double TimeInTarget { get; private set; }


        /// <summary>
        /// Initializes a new instance of the TAC block.
        /// </summary>
        /// <param name="Name">The name of the block.</param>
        /// <param name="DesiredRate">The desired operation rate, not directly used by TAC.</param>
        /// <param name="InputCfg">Configuration for input blocks, TAC expects exactly two inputs.</param>
        /// <param name="Params">Parameters defining the stimulus provider, success timeout, and success threshold.</param>
        /// <param name="Path">Path for additional resources or configuration, not used by TAC.</param>
        public TAC(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs and parameters for the TAC block.
        /// Validates the presence of exactly two input blocks and three parameters,
        /// including a Stimulus block, success timeout, and success threshold.
        /// </summary>
        /// <exception cref="Exception">Thrown when the configuration does not meet the expected criteria.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            if (InputBlocks.Count != 2)
                throw new Exception($"TAC {Name} must have two input blocks.");
            _inputA = InputBlocks[0];
            _inputB = InputBlocks[1];

            if (Params.Count != 3)
                throw new Exception($"TAC {Name} must have three parameters.");
            if (!(Blocks.Instance[Params[0]] is Stimulus))
                throw new Exception($"TAC {Name}'s first parameter must be a Stimulus block.");

            _stimulusProvider = Blocks.Instance[Params[0]] as Stimulus;
            _successTimeOut = Convert.ToDouble(Params[1]);
            _successThreshold = Convert.ToDouble(Params[2]);

            cp = new cpTAC(this);
        }


        /// <summary>
        /// Processes new input from either of the two input blocks.
        /// Calculates the distance between the vectors provided by the inputs
        /// and evaluates whether the success condition has been met.
        /// </summary>
        /// <param name="sender">The block sending the input.</param>
        /// <param name="value">The input value, expected to be a Vector.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            if (sender == _inputA) _a = value as Vector;
            else if (sender == _inputB) _b = value as Vector;

            DistanceToTarget = (_a - _b).L2Norm();

            Trying = _stimulusProvider.IsRunning;
            EvaluateSuccessCondition();
        }

        /// <summary>
        /// Evaluates the success condition based on the current distance to the target and the stimulus provider's state.
        /// Triggers a beep and changes the stimulus provider's state if the success condition is met.
        /// </summary>
        private void EvaluateSuccessCondition()
        {
            if (_stimulusProvider.State == "capture")
            {
                if (DistanceToTarget > _successThreshold) _enteredTarget = Utils.Now;
                TimeInTarget = Utils.Now - _enteredTarget;
                if (TimeInTarget > _successTimeOut)
                {
                    Task.Run(() => Console.Beep(1000, 500));
                    _stimulusProvider.MoveTo("fall");
                }
            }
            else
            {
                TimeInTarget = 0;
                _enteredTarget = Utils.Now;
            }
        }
    }
    #endregion
}
