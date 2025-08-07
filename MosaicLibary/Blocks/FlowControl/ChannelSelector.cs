using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Core.Tokens;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;


namespace iM
{
    #region ControlPanel
    /// <summary>
    /// Represents a <see cref="ControlPanel"/> for selecting active channels within a <see cref="ChannelSelector"/>.
    /// </summary>
    public partial class CpChannelSelector : ControlPanel
    {
        /// <summary>
        /// The ChannelSelector instance associated with this control panel.
        /// </summary>
        private ChannelSelector _sourceBlock;
        
        /// <summary>
        /// The current number of channels.
        /// </summary>
        private int _numberOfChannels;
        
        /// <summary>
        ///  A list indicating the activation state (true for active, false for inactive) of each channel.
        /// </summary>
        private List<bool> _channelActivations = new List<bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CpChannelSelector"/> class.
        /// </summary>
        /// <param name="sourceBlock">The <see cref="ChannelSelector"/> associated with this control panel.</param>
        public CpChannelSelector(ChannelSelector scourceBlock) : base(scourceBlock)
        {
            InitializeComponent();
            _sourceBlock = scourceBlock;
            _sourceBlock.NumberOfChannelsChanged += SourceBlock_NumberOfChannelsChanged;
        }

        /// <summary>
        /// Refreshes the control panel every Tick of the DispatcherTimer. In this case updates the <see cref="ScopeMonitor"/> int the UI./>
        /// </summary>
        /// <param name="myObject">The sender object.</param>
        /// <param name="myEventArgs">Event arguments.</param>
        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            _scope.Update(this._sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Handles changes in the number of channels, updating the UI accordingly.
        /// </summary>
        /// <param name="newChannelCount">The new number of channels.</param>
        private void SourceBlock_NumberOfChannelsChanged(int newChannelCount)
        {
            // Update the number of channels
            _numberOfChannels = newChannelCount;
            // Update the UI
            GenerateChannelCheckboxes();
        }

        /// <summary>
        /// Generates checkboxes for each channel, allowing users to activate/deactivate channels.
        /// </summary>
        private void GenerateChannelCheckboxes()
        {
            RemoveExistingCheckboxes();
            
            _channelActivations = Enumerable.Repeat(true, _numberOfChannels).ToList();

            for (int i = 0; i < _numberOfChannels; i++)
            {
                CheckBox checkBox = CreateCheckbox(i);
                checkBox.CheckedChanged += CheckBox_CheckedChanged;
                this.Controls.Add(checkBox);
            }
        }

        /// <summary>
        /// Removes all existing checkboxes from the form.
        /// </summary>
        private void RemoveExistingCheckboxes()
        {
            var checkBoxes = this.Controls.OfType<CheckBox>().ToList();
            foreach (var checkBox in checkBoxes)
            {
                this.Controls.Remove(checkBox);
                checkBox.Dispose();
            }
        }

        /// <summary>
        /// Creates a new checkbox for a given channel index.
        /// </summary>
        /// <param name="index">The index of the channel for which to create the checkbox.</param>
        /// <returns>A new checkbox configured for the specified channel.</returns>
        private CheckBox CreateCheckbox(int index)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = $"Channel {index + 1}",
                Name = $"checkBoxChannel{index + 1}",
                Location = new Point(10, 20 + (index * 25)),
                AutoSize = true,
                Checked = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            return checkBox;
        }

        /// <summary>
        /// Updates the channel activation state based on checkbox changes.
        /// </summary>
        /// <param name="sender">The checkbox whose state has changed.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                int channelIndex = GetChannelIndexFromCheckboxName(checkBox.Name);
                if (channelIndex >= 0 && channelIndex < _channelActivations.Count)
                {
                    _channelActivations[channelIndex] = checkBox.Checked;
                    _sourceBlock.UpdateChannelActivations(_channelActivations);
                }
            }
        }

        /// <summary>
        /// Extracts the channel index from a checkbox's name.
        /// </summary>
        /// <param name="checkboxName">The name of the checkbox.</param>
        /// <returns>The zero-based index of the channel associated with the checkbox.</returns>
        private int GetChannelIndexFromCheckboxName(string checkboxName)
        {
            string indexPart = checkboxName.Replace("checkBoxChannel", "");
            if (int.TryParse(indexPart, out int channelIndex))
            {
                channelIndex -= 1;
                return channelIndex;
            }
            return -1;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the radioButtonSignalDelete control.
        /// This method is called whenever the checked state of radioButtonSignalDelete changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void radioButtonSignalDelete_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                _sourceBlock.DeleteChannel = true;
                radioButtonSignalZero.Checked = false;
            }

        }

        /// <summary>
        /// Handles the CheckedChanged event of the radioButtonSignalZero control.
        /// This method is called whenever the checked state of radioButtonSignalZero changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void radioButtonSignalZero_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                _sourceBlock.DeleteChannel = false;
                radioButtonSignalDelete.Checked = false;
            }
        }
    }
    #endregion


    #region ChannelSelector Block

    /// <summary>
    /// Represents a selector for handling input channels, allowing for dynamic activation and deactivation of channels. Currently sets the Signals to zero that are deselected. 
    /// Can be easly changed to no signal in the <see cref="GenerateVector(object)"/> Function.
    /// </summary>
    public class ChannelSelector : Block
    {
        /// <summary>
        /// Holds the activation state for each channel. If a channel is active, the corresponding position in the list is true; otherwise, it's false.
        /// </summary>
        private List<bool> _channelActivations;

        /// <summary>
        /// Stores the current number of channels. This value is used to determine how many channels are being managed and may influence the generation and processing of data.
        /// </summary>
        private int _numberOfChannels;

        /// <summary>
        /// Gets or sets the number of channels. Setting this property triggers the <see cref="NumberOfChannelsChanged"/> event.
        /// </summary>
        public int NumberOfChannels
        {
            get => _numberOfChannels;
            set
            {
                if (_numberOfChannels != value)
                {
                    _numberOfChannels = value;
                    // Trigger the event
                    NumberOfChannelsChanged?.Invoke(_numberOfChannels);
                }
            }
        }

        /// <summary>
        /// Indicates whether channels should be deleted or set to zero when processed.
        /// When set to <c>true</c>, channels are deleted from the dataset. 
        /// When set to <c>false</c>, channel values are set to zero instead of being removed.
        /// This distinction affects how the dataset is modified during processing operations.
        /// Default false, changed through radiobuttons in the UI.
        /// </summary>
        public bool DeleteChannel = false;


        /// <summary>
        /// An event that is triggered when the number of channels changes.
        /// </summary>
        public event Action<int> NumberOfChannelsChanged;


        /// <inheritdoc/>
        public ChannelSelector(string Name, double DesiredRate = 0, List<string> InputCfg = null, List<string> Params = null, string Path = null) :
            base(Name, DesiredRate, InputCfg, Params, Path)
        {}

        /// <summary>
        /// Configures inputs for the channel selector. This method ensures only one input block is attached.
        /// Throws an exception if more than one input block is found.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();
            if (InputBlocks.Count != 1) throw new Exception($"Channelelector {Name} must have one input block only.");
            this.DesiredRate = InputBlocks[0].DesiredRate;
            cp = new CpChannelSelector(this);
        }

        /// <summary>
        /// Processes new input data, applying channel activations to filter or modify the input before sending it as output.
        /// </summary>
        /// <param name="sender">The source of the input data.</param>
        /// <param name="value">The input data to be processed.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            Data = GenerateVector(value);
            SendOutput(Data);
        }

        /// <summary>
        /// Updates the activation state of each channel based on the provided list.
        /// </summary>
        /// <param name="channelActivations">A list indicating the active state of each channel.</param>
        public void UpdateChannelActivations(List<bool> channelActivations)
        {
            _channelActivations = channelActivations;
        }

        /// <summary>
        /// Generates a vector from the input value from the previous <see cref="Block"/>, applying the current <see cref="_channelActivations"/> to include or exclude data points.
        /// </summary>
        /// <param name="value">The input data, expected to be a Vector.</param>
        /// <returns>A new Vector with data from the activated channels.</returns>
        private Vector GenerateVector(object value)
        {
            List<double> dataList = new List<double>();
            NumberOfChannels = (value as Vector).Count;
            if (_channelActivations == null)
            {
                _channelActivations = Enumerable.Repeat(true, NumberOfChannels).ToList();
            }
            for (int i = 0; i < NumberOfChannels; i++)
            {
                if (_channelActivations[i])
                {
                    // Add the data from the active channel to the filteredData
                    dataList.Add((value as Vector)[i]);
                }
                else
                {
                    if (!DeleteChannel)
                    {
                        dataList.Add(0);
                    }
                }
            }
            return Vector<double>.Build.Dense(dataList.ToArray());
        }
    }
    #endregion
}

