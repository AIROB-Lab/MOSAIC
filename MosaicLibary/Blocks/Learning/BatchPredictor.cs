using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace MosaicLibary
{
    #region Control Panel
    /// <summary>
    /// Control Panel for the <see cref="BatchPredictor"/> block, providing UI elements to interact with the <see cref="BatchPredictor"/>.
    /// Allows users to build or reset the predictive model and displays the model's confidence.
    /// </summary>
    public partial class cpBatchPredictor : ControlPanel
    {
        /// <summary>
        /// The <see cref="BatchPredictor"/> block associated with this control panel.
        /// </summary>
        private BatchPredictor _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the cpBatchPredictor control panel.
        /// </summary>
        /// <param name="sourceBlock">The BatchPredictor block this panel controls.</param>
        public cpBatchPredictor(BatchPredictor sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel, updating the confidence progress bar and the model's data visualization.
        /// </summary>
        /// <param name="o">The sender of the refresh event.</param>
        /// <param name="e">The event arguments.</param>
        override protected void cpRefresh(object o, EventArgs e)
        {
            double logC = Math.Log(1 / _sourceBlock.Confidence + 1);
            pbConfidence.Value = Math.Min(100, (int)(logC == double.PositiveInfinity ? 0 : 100 * logC));
            smILM.Update(_sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Handles the click event on the "Build Model" button, initiating the model building process in the BatchPredictor.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnBuildModel_Click(object sender, EventArgs e)
        {
            _sourceBlock.BuildModel();
        }

        /// <summary>
        /// Handles the click event on the "Reset Model" button, resetting the predictive model in the BatchPredictor.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnResetModel_Click(object sender, EventArgs e)
        {
            _sourceBlock.ResetModel();
        }
    }
    #endregion

    #region BatchPredictor Block
    /// <summary>
    /// A block that encapsulates a batch learning machine for predictive modeling. It gathers data samples from a specified provider
    /// and uses a buffer to build a predictive model. Ideal for scenarios where model construction or update is based on a batch of data rather than streaming data.
    /// </summary>
    /// <example>
    /// <code>
    /// batch_learn: 
    /// { 
    ///     Type: BatchPredictor,
    ///     Inputs: [ name of Data Block ],
    ///     Params: [ name of Buffer, name of LearningMachine, 8, 12, ...other hyperparameters... ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: BatchPredictor</c> indicates that the block constructs or updates 
    ///       a predictive model using batches of data, rather than incremental samples.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ Data BLock ]</c> needs that the block receives data from an EMG or signal-providing.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ name_of_buffer, NameOfLearningMachine, 8, 12, ... ]</c> includes:
    ///       <list type="number">
    ///         <item><description>The name of the <c>Buffer</c> storing data for model building.</description></item>
    ///         <item><description>The class name of the machine learning model (e.g., <c>NameOfLearningMachine</c>). Current LearningMachines are <see cref="RidgeRegression"/> and <see cref="RR_RFF"/></description></item>
    ///         <item><description>The input dimension (<c>8</c>) and output dimension (<c>12</c>), followed by optional hyperparameters.</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// </example>
    /// <remarks>
    /// Batch prediction involves creating or updating a predictive model using a batch (or collection) of data points at once, rather than 
    /// updating the model incrementally with each new data point. This approach is often used in scenarios where the model benefits from being 
    /// trained or retrained on a comprehensive dataset to improve accuracy or when real-time prediction is not required. 
    /// The <see cref="BatchPredictor"/> class wraps around a machine learning model, facilitating the collection of data, model training, and prediction in a 
    /// batch processing manner.
    /// </remarks>
    public class BatchPredictor : Block
    {
        /// <summary>
        /// The provider of sample data for model building.
        /// </summary>
        private Block _sampleProvider;

        /// <summary>
        /// Storage for the data used to build the model.
        /// </summary>
        private Buffer _dataStorage;

        /// <summary>
        /// The machine learning model for batch prediction (<see cref="BatchLearningMachine"/>).
        /// </summary>
        private BatchLearningMachine _learningMachine;

        /// <summary>
        /// The dimensionality of the input space (d).
        /// </summary>
        private int _inputDimension;

        /// <summary>
        /// The dimensionality of the output space (M).
        /// </summary>
        private int _outputDimension;

        /// <summary>
        /// The current confidence of the predictive model.
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>
        /// Initializes a new instance of the BatchPredictor class.
        /// </summary>
        /// <param name="Name">The name of the predictor block.</param>
        /// <param name="DesiredRate">The desired rate of processing, not directly used in batch prediction.</param>
        /// <param name="InputCfg">Configuration for input blocks, only one is expected for sample provision.</param>
        /// <param name="Params">Parameters for configuring the predictive model, including the buffer, model type, and dimensions.</param>
        /// <param name="Path">Path for additional resources, not used in this block.</param>

        public BatchPredictor(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs and parameters for the BatchPredictor. Validates the setup and initializes the learning machine.
        /// A BatchPredictor has 4 Params or more, that is
        /// - the Buffer storing the data we build the model out of,
        /// - the type of LearningMachine,
        /// - the input space dimension d,
        /// - the output space dimension m
        /// - all other (hyper)parameters (lambda, sigma, D, ...)
        /// </summary>
        /// <exception cref="Exception">Thrown if the configuration does not meet the required criteria.</exception>

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a BatchPredictor has 1 input only
            if (InputBlocks.Count != 1)
                throw new Exception($"BatchPredictor {Name} must have 1 input block.");

            _sampleProvider = InputBlocks[0];
            DesiredRate = _sampleProvider.DesiredRate;

            // a BatchPredictor has 4 Params or more, that is
            // - the Buffer storing the data we build the model out of,
            // - the type of LearningMachine,
            // - the input space dimension d,
            // - the output space dimension m
            // - all other (hyper)parameters (lambda, sigma, D, ...)
            if (Params.Count < 4)
                throw new Exception($"BatchPredictor {Name} must have at least 4 parameters.");
            if (!(Blocks.Instance[Params[0]] is Buffer))
                throw new Exception($"BatchPredictor {Name} must have a Buffer as its first parameter.");
            _dataStorage = Blocks.Instance[Params[0]] as Buffer;
            string LearningMachineTypeName = Params[1];
            _inputDimension = Convert.ToInt32(Params[2]);
            _outputDimension = Convert.ToInt32(Params[3]);
            string[] HyperParams = Params.Count == 4 ? Array.Empty<string>() : Params.GetRange(4, Params.Count - 4).ToArray();

            // another example of C# reflection (see also the class CfgParser): decide at runtime what type of learning machine to use!
            // first define the type, using Params[0]
            Type thisLearningMachineType = typeof(Block).Assembly.GetType("iM." + LearningMachineTypeName);
            // then create the instance - each LearningMachine must have a constructor (int,int,string[]) besides its standard one.
            _learningMachine = (BatchLearningMachine)Activator.CreateInstance(thisLearningMachineType, _inputDimension, _outputDimension, HyperParams);

            cp = new cpBatchPredictor(this);
        }

        /// <summary>
        /// Builds the predictive model using the accumulated data in the buffer.
        /// </summary>
        public void BuildModel()
        {
            Matrix X = Matrix.Build.Dense(0, _inputDimension);
            Matrix Y = Matrix.Build.Dense(0, _outputDimension);

            // now consider the database in the _dataStorage:
            foreach (var pair in _dataStorage.dB)
            {
                // for each cluster in it, accumulate the samples onto X
                X = X.Stack(pair.Item2);
                // and accumulate a matrix into Y which is N copies of the target values - one copy per row in X.
                foreach (var i in System.Linq.Enumerable.Range(1, pair.Item2.RowCount))
                    Y = Y.Stack(pair.Item1.ToRowMatrix());
            }

            // then build my model!
            _learningMachine.BuildModel(X, Y);
        }

        /// <summary>
        /// Resets the predictive model to its initial state.
        /// </summary>
        public void ResetModel()
        {
            _learningMachine.ResetModel();
        }

        /// <summary>
        /// Processes new input data for prediction, evaluates model confidence, and sends the predicted output.
        /// </summary>
        /// <param name="sender">The block sending the input.</param>
        /// <param name="value">The input value for prediction.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            // evaluate and store confidence, predict y_hat and send it off!
            Confidence = _learningMachine.Confidence(value as Vector);
            SendOutput(_learningMachine.Predict(value as Vector));
        }
    }
    #endregion
}
