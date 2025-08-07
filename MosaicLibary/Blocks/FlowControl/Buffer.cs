using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    #region ControlPanel
    /// <summary>
    /// Represents a partial class for the <see cref="ControlPanel"/> of the <see cref="Buffer"/> block.
    /// This control panel provides an interface to monitor and interact with the Buffer block's data.
    /// </summary>
    public partial class cpBuffer : ControlPanel
    {
        /// <summary>
        /// The Buffer block associated with this control panel.
        /// </summary>
        private Buffer _sourceBlock;

        /// <summary>
        /// Constructs a new instance of the <see cref="cpBuffer"/> class.
        /// </summary>
        /// <param name="sourceBlock">The Buffer block associated with the control panel.</param>
        public cpBuffer(Buffer sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel.
        /// Updates the labels with the current status of the Buffer block's clusters.
        /// </summary>
        /// <param name="o">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void cpRefresh(object o, EventArgs e)
        {
            lblTitle.Text = $"Contains {_sourceBlock.dB.Count} clusters:";
            lblBuffer.Text = string.Empty;

            foreach (var pair in _sourceBlock.dB)
            {
                lblBuffer.Text += $"{Utils.Print(pair.Item1)},({pair.Item2.RowCount}), ";
            }
        }

        /// <summary>
        /// Handles the click event of the Save button.
        /// Saves the data stored in the Buffer block to a file.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = $"Save data stored in {_sourceBlock.Name}",
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter dumpBufferStream = new StreamWriter(saveFileDialog.FileName))
                {
                    foreach (var pair in _sourceBlock.dB)
                    {
                        foreach (var rowIdx in Enumerable.Range(0, pair.Item2.RowCount))
                        {
                            dumpBufferStream.WriteLine($"{Utils.Print(pair.Item1)},{Utils.Print(pair.Item2.Row(rowIdx))}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event of the Clear button.
        /// Clears all data stored in the Buffer block.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            _sourceBlock.dB.Clear();
        }
    }
    #endregion

    #region Buffer
    /// <summary>
    /// Represents a Buffer block that captures clusters of data according to start/stop events
    /// and stores them in a dictionary. 
    /// Inputs include a data provider (e.g., Myo) and a trigger (e.g., commands sent sporadically).
    /// </summary>
    /// <example>
    /// >[!IMPORTANT] The second input block must have a "-" infront of its name.
    /// 
    /// <code>
    /// cluster_storage: {
    ///   Type: Buffer,
    ///   Inputs: [ [name of Triger], -[name of Data Input Block] ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       The trigger block (e.g., <see cref="Trigger"/> or <see cref="StimTrigger"/>). 
    ///       It sends start/stop signals to the <see cref="Buffer"/>—for instance, beginning a new cluster when 
    ///       a certain event occurs, and ending when that event completes.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The data provider block could be a Myo block, a function block, or 
    ///       any other block that generates or streams the signal that needs to be captured.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// When the trigger Block triggers, the Buffer subscribes to data provider Block and starts stacking 
    /// incoming vectors into a data cluster. When the trigger is null (stop event), the cluster is 
    /// finalized and stored in the Buffer’s internal database (<see cref="dB"/>).
    /// </example>
    public class Buffer : Block
    {
        /// <summary>
        /// The input trigger block used to start or stop capturing data.
        /// </summary>
        private Block _inputTrigger;

        /// <summary>
        /// The input data provider block that supplies data to the Buffer.
        /// </summary>
        private Block _inputDataProvider;

        /// <summary>
        /// The target value associated with the current data cluster.
        /// </summary>
        private Vector _dataClusterTargetValue;

        /// <summary>
        /// The matrix representing the current data cluster being captured.
        /// </summary>
        private Matrix _dataCluster = Matrix.Build.Dense(0, 0);

        /// <summary>
        /// The database: a list of data clusters, each labeled with a target value or label (Vector).
        /// </summary>
        public List<(Vector, Matrix)> dB = new List<(Vector, Matrix)>();

        /// <summary>
        /// Constructs a new instance of the <see cref="Buffer"/> class.
        /// </summary>
        /// <param name="Name">The name of the Buffer block.</param>
        /// <param name="DesiredRate">The desired rate of the Buffer block.</param>
        /// <param name="InputCfg">The input configuration of the Buffer block.</param>
        /// <param name="Params">The parameters for the Buffer block.</param>
        /// <param name="Path">The path of the Buffer block.</param>
        public Buffer(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the input settings for the Buffer block.
        /// Ensures the block has exactly two inputs: a trigger and a data provider.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            // Ensure the Buffer block has exactly two inputs.
            if (InputBlocks.Count != 2)
                throw new Exception($"Buffer {Name} must have 2 input blocks.");

            // The first input must be a Trigger or StimTrigger.
            if (!(InputBlocks[0] is Trigger) && !(InputBlocks[0] is StimTrigger))
                throw new Exception($"Buffer {Name} must have a Trigger or StimTrigger as the first input block.");

            // The Buffer block does not produce any output, so set its desired rate to zero.
            DesiredRate = 0;

            _inputTrigger = InputBlocks[0];
            _inputDataProvider = InputBlocks[1];

            cp = new cpBuffer(this);
        }

        /// <summary>
        /// Handles new input data received from connected blocks.
        /// If triggered, captures and stores data clusters.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input value received from the block.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            // Handle input from the trigger block.
            if (sender == _inputTrigger)
            {
                switch (value)
                {
                    case null:
                        // Stop capturing data, store the cluster, and reset the current cluster.
                        UnsubscribeFrom(_inputDataProvider);
                        dB.Add((_dataClusterTargetValue, _dataCluster.Clone()));
                        _dataCluster = Matrix.Build.Dense(0, 0);
                        break;
                    default:
                        // Start capturing data with the target value from the trigger.
                        SubscribeTo(_inputDataProvider);
                        _dataClusterTargetValue = (Vector)value;
                        break;
                }
            }
            // Handle input from the data provider block.
            else if (sender == _inputDataProvider)
            {
                // If this is the first datum, initialize the data cluster's dimensions.
                if (_dataCluster.RowCount == 0)
                {
                    _dataCluster = Matrix.Build.Dense(0, (value as Vector).Count);
                }

                // Add the datum to the data cluster.
                _dataCluster = _dataCluster.Stack((value as Vector).ToRowMatrix());
            }
        }
    }
    #endregion
}
