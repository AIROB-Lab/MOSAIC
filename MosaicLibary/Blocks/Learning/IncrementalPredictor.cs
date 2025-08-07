using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.IO;

namespace MosaicLibary
{
    /// <summary>
    /// Provides a user interface for interacting with an <see cref="IncrementalPredictor"/> block. Allows users to reset the model,
    /// load a model from a file, or save the current model to a file, and displays the model's confidence.
    /// </summary>
    public partial class cpIncrementalPredictor : ControlPanel
    {
        /// <summary>
        /// The associated <see cref="IncrementalPredictor"/> block this control panel interacts with.
        /// </summary>
        private IncrementalPredictor _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the CpIncrementalPredictor control panel.
        /// </summary>
        /// <param name="sourceBlock">The <see cref="IncrementalPredictor"/> block this panel controls.</param>
        public cpIncrementalPredictor(IncrementalPredictor sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel UI, specifically the confidence progress bar and potentially other data visualizations.
        /// </summary>
        /// <param name="sender">The sender of the refresh event.</param>
        /// <param name="e">Event arguments.</param>
        override protected void cpRefresh(object o, EventArgs e)
        {
            double logC = Math.Log(1 / _sourceBlock.Confidence + 1);
            pbConfidence.Value = Math.Min(100, (int)(logC == double.PositiveInfinity ? 0 : 100 * logC));
            smILM.Update(_sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Handles user requests to reset the predictive model to its initial state.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the reset button.</param>
        /// <param name="e">Event arguments.</param>
        private void btnResetModel_Click(object sender, EventArgs e)
        {
            _sourceBlock.ResetModel();
        }

        /// <summary>
        /// Provides a mechanism for the user to load a model from a .txt file. 
        /// </summary>
        /// <param name="sender">The sender of the event, typically the load model button.</param>
        /// <param name="e">Event arguments.</param>
        private void btnLoadModel_Click(object sender, EventArgs e)
        {
            OpenFileDialog _ofd = new OpenFileDialog { Filter = "iM model files|*.txt", Title = "Open model file", RestoreDirectory = true };
            _ofd.ShowDialog();

            string[] lines = System.IO.File.ReadAllLines(_ofd.FileName);

            List<Vector> ll = new List<Vector>();

            foreach (string line in lines) 
                ll.Add(Vector.Build.Dense(Array.ConvertAll(line.Split(','), double.Parse)));

            // take the number of dimensionality for A_inv and B
            Matrix Ainv = Matrix.Build.DenseOfRowVectors(ll.ToArray().Take(_sourceBlock._inputDimension));
            Matrix B = Matrix.Build.DenseOfRowVectors(ll.ToArray().Skip(_sourceBlock._inputDimension).Take(_sourceBlock._inputDimension));

            _sourceBlock.Model = new List<object>() { Ainv, B };
        }

        /// <summary>
        /// Enables the user to save the current model to a .txt file.
        /// </summary>
        /// <param name="sender">The sender of the event, typically the save model button.</param>
        /// <param name="e">Event arguments.</param>
        private void btnSaveModel_Click(object sender, EventArgs e)
        {
            // manual save through button click
            _sourceBlock.SaveModel();
        }
    }

    #region IncrementalPredictor
    /// <summary>
    /// Implements a block containing an incremental learning machine, suitable for environments where the model
    /// needs to be updated dynamically with new data. It receives input samples from a data <see cref="Block"/>
    /// and target values from a <see cref="Trigger"/>, updating the model with each new target value received.
    /// </summary>
    /// <example>
    /// <code>
    /// incremental_learn:
    /// {
    ///   Type: IncrementalPredictor,
    ///   Inputs: [ DataBlockName, TriggerBlockName ],
    ///   Params: [ IncrementalLearningMachineName, 8, 12, ...hyperparameters... ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: IncrementalPredictor</c> designates that this block updates its model 
    ///     incrementally (on a per-sample or per-target basis), as opposed to batch updates.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ DataBlockName, TriggerBlockName ]</c> shows that the first input must be a data containing Block, which
    ///       provides new data samples, while the second input must be a <see cref="Trigger"/> Block, that supplies target values 
    ///       for model updates.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params</c> includes:
    ///       <list type="number">
    ///         <item><description>The name of the incremental learning machine class 
    ///         (e.g., <c>IncrementalLearningMachineName</c>).</description></item>
    ///         <item><description>The input dimension (<c>8</c>) and output dimension (<c>12</c>) for the model.</description></item>
    ///         <item><description>Optional hyperparameters (e.g., <c>lambda</c>, <c>sigma</c>, etc.).</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// When <c>triggerBlock</c> sends a target vector, the incremental model is updated. 
    /// When it sends <c>null</c>, the block saves the current model state to disk (if configured).
    /// </example>
    public class IncrementalPredictor : Block
    {
        /// <summary>
        /// The block that provides the input samples for the incremental learning process.
        /// These samples are used by the learning machine for making predictions or updates.
        /// </summary>
        private Block _sampleProvider;

        /// <summary>
        /// The trigger that provides the target values for the learning process.
        /// Target values are used to update the learning machine incrementally.
        /// </summary>
        private Trigger _targetProvider;

        /// <summary>
        /// The core of the incremental predictor, an instance of an <see cref="IncrementalLearningMachine"/>.
        /// This machine is updated with new data samples and target values as they are received.
        /// </summary>
        private IncrementalLearningMachine _incrementalLearningMachine;

        /// <summary>
        /// The dimensionality of the input space (d) for the learning machine, indicating how many features each input sample has.
        /// This is used to configure the learning machine and validate input data.
        /// </summary>
        internal int _inputDimension;

        /// <summary>
        /// The dimensionality of the output space (M) for the learning machine, indicating how many target variables each prediction includes.
        /// This helps define the shape of the output produced by the learning machine.
        /// </summary>
        internal int _outputDimension;

        /// <summary>
        /// The most recently received target Vector from the target provider.
        /// This Vector is used to update the learning machine with the latest known outcomes.
        /// </summary>
        private Vector _target;

        /// <summary>
        /// Gets or sets the model of the incremental learning machine. The getter returns the current model, 
        /// and the setter applies a new model to the machine.
        /// </summary>
        public List<object> Model
        {
            get => _incrementalLearningMachine.GetModel();
            set => _incrementalLearningMachine.SetModel(value);
        }

        /// <summary>
        /// The confidence level of the current model, reflecting its accuracy or reliability.
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>
        /// Initializes a new instance of the IncrementalPredictor with specified configuration.
        /// </summary>
        /// <param name="Name">Name of the predictor.</param>
        /// <param name="DesiredRate">Desired rate of operation, influences how often the model may be updated.</param>
        /// <param name="InputCfg">Configuration settings for inputs.</param>
        /// <param name="Params">Parameters for model configuration, including learning machine type and dimensions.</param>
        /// <param name="Path">Path for additional configuration or resources.</param>
        public IncrementalPredictor(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures inputs and initializes the incremental learning machine based on provided parameters.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // an IncrementalPredictor has 2 inputs, the second of which must be a Trigger
            if (InputBlocks.Count != 2)
                throw new Exception($"IncrementalPredictor {Name} must have 2 input blocks.");
            if (!(InputBlocks[1] is Trigger))
                throw new Exception($"IncrementalPredictor {Name} must have a Trigger as second input block.");

            _sampleProvider = InputBlocks[0]; _targetProvider = InputBlocks[1] as Trigger;
            DesiredRate = _sampleProvider.DesiredRate;

            // an IncrementalPredictor has 3 Params or more (like a BatchPredictor), that is
            // - the type of LearningMachine,
            // - the input space dimension _inputDimension,
            // - the output space dimension m
            // - all other (hyper)parameters (lambda, sigma, D, ...)
            if (Params.Count < 3)
                throw new Exception($"IncrementalPredictor {Name} must have at least 3 parameters.");

            string IncrementalLearningMachineTypeName = Params[0];
            _inputDimension = Convert.ToInt32(Params[1]);
            _outputDimension = Convert.ToInt32(Params[2]);
            string[] HyperParams = Params.Count == 3 ? Array.Empty<string>() : Params.GetRange(3, Params.Count - 3).ToArray();

            // like in BatchPredictors: use the C# Reflection machinery to decide at runtime what incremental learning machine to use
            Type thisIncrementalLearningMachineType = typeof(Block).Assembly.GetType("MosaicLibary." + IncrementalLearningMachineTypeName);
            _incrementalLearningMachine = (IncrementalLearningMachine)Activator.CreateInstance(thisIncrementalLearningMachineType, _inputDimension, _outputDimension, HyperParams);

            cp = new cpIncrementalPredictor(this);

            // create folder structure for automatic model saving, in case it already exists => nothing happens
            Directory.CreateDirectory(@"..\Data\SavedModels\");
        }

        /// <summary>
        /// Resets the learning model to its initial state, discarding any learned information.
        /// </summary>
        public void ResetModel()
        {
            _incrementalLearningMachine.ResetModel();
        }

        /// <summary>
        /// Processes incoming data samples and _target values, updating the model as appropriate and predicting output values.
        /// </summary>
        /// <param name="sender">The block sending the input, either the sample provider or the _target provider.</param>
        /// <param name="value">The value of the input, either a new sample or a _target value for model update.</param>
        override protected void OnNewInput(Block sender, object value)
        {

            // if trying to update
            if (sender == _targetProvider)
            {

                // just store the _target value (could be null!)
                _target = value as Vector;

                // last value which is send is null => KeyUp
                if (_target == null)
                {
                    // Save Model each time after an acquisition and training was completed
                    AutomaticSave();
                }
            }


            // otherwise, if data received from the sample provider,
            else if (sender == _sampleProvider)
            {
                // if the _target is not null then update
                if (_target != null) _incrementalLearningMachine.UpdateModel(value as Vector, _target as Vector);

                // anyway, store confidence then predict and send prediction off
                Confidence = _incrementalLearningMachine.Confidence(value as Vector);
                SendOutput(_incrementalLearningMachine.Predict(value as Vector));
            }
        }

        /// <summary>
        /// Function that saves a model with a current date and time in a bin/Data/SavedModels
        /// </summary>
        internal void AutomaticSave()
        {
            // defines path with current date and time
            string path = @"..\Data\SavedModels\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Name.ToString() + ".txt";
            SaveModel(path);
        }

        /// <summary>
        /// Function that saves model under specified path. If no path is given a <see cref="FileDialog"/> opens up to select path.
        /// </summary>
        /// <param name="path">optional: set a path, where to save the model</param>
        internal void SaveModel(string path = null)
        {
            if (path == null)
            {
                SaveFileDialog _sfd = new SaveFileDialog { Filter = "iM model files|*.txt", Title = "Save model file", RestoreDirectory = true };
                _sfd.ShowDialog();
                path = _sfd.FileName;
            }
            string lines = string.Empty;
            foreach (object o in Model)
            {
                lines += Utils.Print(o);
                //lines += Environment.NewLine;
            }
            if (path != null) // in case cancel was pressed
            {
                File.WriteAllText(path, lines);
            }
        }
    }
}
    #endregion