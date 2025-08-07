using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

using Windows.Devices.Enumeration;

namespace MosaicLibary
{
    /// <summary>
    /// <see cref="ControlPanel"/> instance of the <see cref="Myo"/> class for UI interaction.
    /// Including searching for MYO devices, connecting and disconnecting to one MYO device, and displaying EMG data.
    /// </summary>
    public partial class cpMyo : ControlPanel
    {
        /// <summary>
        /// The source block associated with this control panel.
        /// </summary>
        private Myo _sourceBlock;

        /// <summary>
        /// Copy of the last discovered BLE devices.
        /// </summary>
        private Dictionary<string, DeviceInformation> _lastDevicesCopy;

        /// <summary>
        /// The selected device from the device dropdown list.
        /// </summary>
        private DeviceInformation _selectedDevice;

        /// <summary>
        /// Indicates whether the UI needs to be updated.
        /// </summary>
        private bool _updateUI = false;

        /// <summary>
        /// Indicates whether the MAC address is predefined in a YAML file.
        /// </summary>
        private bool _predefinedMAC = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpMyo"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The Myo block associated with this control panel.</param>
        public cpMyo(Myo _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = _SourceBlock;

            if (string.IsNullOrEmpty(_SourceBlock.MyoMacAddress))
                _predefinedMAC = false;

            // Set the connect button to false if selecting from dropdown is allowed.
            if (_predefinedMAC)
            {
                // Display predefined MAC in the dropdown.
                DeviceDropDown.Items.Add(_SourceBlock.MyoMacAddress);
                DeviceDropDown.SelectedItem = _SourceBlock.MyoMacAddress;
                DeviceDropDown.Enabled = false;
            }

            // Initialize UI states - disable the connect button at the beginning.
            cbConnectDisconnect.Enabled = false;

            // Register for BLE UI update events.
            BluetoothLEManager.UiUpdateEvent += BLEUiUpdateEvent;
        }

        /// <summary>
        /// Refreshes the control panel with updated data and UI states.
        /// </summary>
        /// <param name="myObject">The sender of the refresh event.</param>
        /// <param name="myEventArgs">The event arguments.</param>
        protected override void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            smMyo.Update(_sourceBlock.Data as Vector);

            // Skip updates if no UI refresh is needed.
            if (!_updateUI) return;

            var generalBLEInfo = BluetoothLEManager.GetGeneralBLECopy();

            // Update UI based on scanning state.
            if (generalBLEInfo[GENERAL_BLE_STATES.SCANNING])
            {
                StartScan.Enabled = false;
                StopScan.Enabled = true;
                cbConnectDisconnect.Enabled = false;
                DeviceDropDown.Enabled = false;
            }
            else
            {
                StartScan.Enabled = true;
                StopScan.Enabled = false;

                if (!_predefinedMAC) DeviceDropDown.Enabled = true;

                if (generalBLEInfo[GENERAL_BLE_STATES.FOUND_DEVICES])
                    cbConnectDisconnect.Enabled = true;
            }

            if (DeviceDropDown.SelectedItem == null)
                cbConnectDisconnect.Enabled = false;

            // Update the dropdown list if the MAC is not predefined.
            if (_predefinedMAC)
            {
                var nameAndMac = _sourceBlock._bleManager.GetNameAndMac();
                string name = nameAndMac.Item1, mac = nameAndMac.Item2;

                if (name != null)
                {
                    DeviceDropDown.Items.Clear();
                    string newItem = $"{name} | {mac}";
                    DeviceDropDown.Items.Add(newItem);
                    DeviceDropDown.SelectedItem = newItem;
                }
            }
            else
            {
                _lastDevicesCopy = BluetoothLEManager.GetDevicesCopy();
                Queue<string> deviceStringQueue = new Queue<string>();

                foreach (var item in _lastDevicesCopy)
                {
                    if (!string.IsNullOrEmpty(item.Value.Name))
                    {
                        string newItem = $"{item.Value.Name} | {item.Key}";
                        deviceStringQueue.Enqueue(newItem);
                    }
                }

                string[] deviceStrings = deviceStringQueue.ToArray();
                string lastSelection = DeviceDropDown.Text;

                DeviceDropDown.Items.Clear();
                DeviceDropDown.Items.AddRange(deviceStrings);

                try
                {
                    DeviceDropDown.SelectedItem = lastSelection;
                }
                catch (Exception)
                {
                    // Ignore selection errors.
                }
            }

            _updateUI = false;
        }

        /// <summary>
        /// Event handler for BLE UI update events.
        /// </summary>
        private void BLEUiUpdateEvent(object sender, EventArgs e)
        {
            _updateUI = true;
        }

        /// <summary>
        /// Handles the state change of the connect/disconnect checkbox.
        /// </summary>
        private void cbConnectDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cbConnectDisconnect.Checked)
            {
                _sourceBlock.Connect();
                cbConnectDisconnect.Text = "Disconnect";
            }
            else
            {
                _sourceBlock.Disconnect();
                cbConnectDisconnect.Text = "Connect";
            }
        }

        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        private void StartScan_Click(object sender, EventArgs e)
        {
            _sourceBlock.StartScan();
        }

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        private void StopScan_Click(object sender, EventArgs e)
        {
            _sourceBlock.StopScan();
        }

        /// <summary>
        /// Handles dropdown click events.
        /// </summary>
        private void DropDownClickEvent(object sender, EventArgs e)
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Handles changes in the selected item of the dropdown list.
        /// </summary>
        private void DropDown_SelectedChanged(object sender, EventArgs e)
        {
            // Ignore dropdown events if the MAC address is predefined.
            if (_predefinedMAC) return;

            // Extract the MAC address from the selected item.
            ComboBox comboBox = (ComboBox)sender;
            int macIndex = comboBox.Text.IndexOf('|');
            string macAddress = comboBox.Text.Substring(macIndex + 2);
            _selectedDevice = _lastDevicesCopy[macAddress];

            // Notify BLE manager about the selected device.
            _sourceBlock._bleManager.SetWantedDevice(_selectedDevice);

            cbConnectDisconnect.Enabled = true;
        }
    }

    #region Myo
    /// <summary>
    /// Myo - Receives data from one Thalmic Lab Myo device connected through BLE and outputs EMG samples.
    /// If you want to connect to multiple Myo devices, you need to create multiple Myo blocks in your YAML file.
    /// </summary>
    /// <example>
    /// <para>
    /// A typical YAML configuration might look like this:
    /// </para>
    /// <code>
    /// myoBlock:
    /// {
    ///   Type: MYO,
    ///   Inputs: [ myTimer ],
    ///   Params: [ "00:00:00:00:00:00" ]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: MYO</c> specifies that this block reads EMG data from a Myo device.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ myTimer ]</c> indicates the Myo block depends on a timer block 
    ///       (could be <see cref="Timer"/> or <see cref="ScheduledTimer"/>) to schedule data acquisition ticks.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ "00:00:00:00:00:00" ]</c> optionally provides the MAC address of the Myo to connect to. 
    ///       If left blank or omitted, you can specify it later in the UI or let the system search for available devices.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// In this setup, the block named <c>myTimer</c> triggers the Myo block at the desired rate, and the <c>myoBlock</c> 
    /// establishes a BLE connection (using the specified MAC address) to stream 8-channel EMG data.
    /// </para>
    /// </example>
    public class Myo : Block
    {
        #region Fields
 

        private const int _numOfEMGChannels = 8;
        private const double _myoHwEmgScale = 128.0;

        /// <summary>
        /// Manages the BLE connection, commands, subscriptions, and notifications for the Myo device.
        /// </summary>
        internal BluetoothLEManager _bleManager;

        private double[] _emgSample = new double[_numOfEMGChannels];
        private int _maxDelay = 15; // Maximum delay in samples

        // PERFORMANCE TESTING VARIABLES
        private double _outputtedMyo = 0;
        private double _outputtedOld = 0;
        private double _jumpedSamples = 0;
        private int _printIndex = 0;

        /// <summary>
        /// The Myo device is uniquely identified by its MAC address.
        /// It provides 8 EMG channels as 8-bit signed integers in the range [-128..128].
        /// </summary>
        public string MyoMacAddress;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Myo"/> class.
        /// </summary>
        /// <param name="Name">The name of the block.</param>
        /// <param name="DesiredRate">The desired rate for data acquisition.</param>
        /// <param name="InputCfg">The input configuration for the block.</param>
        /// <param name="Params">The parameters for configuring the block, including the Myo MAC address.</param>
        /// <param name="Path">The path used for device communication.</param>
        public Myo(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }
        #endregion

        #region Methods

        /// <summary>
        /// Configures the input settings for the Myo block.
        /// Validates the input block and initializes BLE communication with the Myo device.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            // Ensure the Myo block has one input and it is a Timer.
            if (InputBlocks.Count != 1) throw new Exception($"Myo {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"Myo {Name}'s input must be a Timer.");

            // Set the desired rate to match the driving Timer.
            DesiredRate = InputBlocks[0].DesiredRate;

            // Set the MAC address for the Myo device, or search for it if unspecified.
            MyoMacAddress = Params == null ? string.Empty : Params[0];

            // Initialize BLE communication protocol with the Myo device.
            var initialNotifications = new Dictionary<string, (string service, string characteristic)>()
            {
                {"EMG0", (service:"d5060005-a904-deb9-4748-2c7f4a124842",characteristic:"d5060105-a904-deb9-4748-2c7f4a124842")},
                {"EMG1", (service:"d5060005-a904-deb9-4748-2c7f4a124842",characteristic:"d5060205-a904-deb9-4748-2c7f4a124842")},
                {"EMG2", (service:"d5060005-a904-deb9-4748-2c7f4a124842",characteristic:"d5060305-a904-deb9-4748-2c7f4a124842")},
                {"EMG3", (service:"d5060005-a904-deb9-4748-2c7f4a124842",characteristic:"d5060405-a904-deb9-4748-2c7f4a124842")}
            };
            Dictionary<string, (string service, string characteristic, byte[] cmd)> initialCommands = new Dictionary<string, (string service, string characteristic, byte[] cmd)>()
            {
                {"NoSleep", (service: "D5060001-A904-DEB9-4748-2C7F4A124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{0x09, 0x01, 0x01} ) },
                {"FiltEMG_NoIMU_NoClass", (service: "D5060001-A904-DEB9-4748-2C7F4A124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x01, 0x03, 0x02, 0x00, 0x00 })},
                {"VibrateLong", (service: "d5060001-a904-deb9-4748-2c7f4a124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x03, 0x01, 0x03}) },
                {"VibrateLong2", (service: "d5060001-a904-deb9-4748-2c7f4a124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x03, 0x01, 0x03}) }
            };
            Dictionary<string, (string service, string characteristic, byte[] cmd)> endCommands = new Dictionary<string, (string service, string characteristic, byte[] cmd)>()
            {
                {"NoFiltEMG_NoIMU_NoClass", (service: "D5060001-A904-DEB9-4748-2C7F4A124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x01, 0x03, 0x00, 0x00, 0x00 })},
                {"NormalSleep", (service: "D5060001-A904-DEB9-4748-2C7F4A124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{0x09, 0x01, 0x00} ) },
                {"VibrateShort", (service: "d5060001-a904-deb9-4748-2c7f4a124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x03, 0x01, 0x01}) },
                {"VibrateShort2", (service: "d5060001-a904-deb9-4748-2c7f4a124842", characteristic: "d5060401-a904-deb9-4748-2c7f4a124842", new byte[]{ 0x03, 0x01, 0x01}) },
            };

            _bleManager = new BluetoothLEManager(MyoMacAddress, _numOfEMGChannels, initialNotifications, initialCommands, endCommands);
            cp = new cpMyo(this);
        }

        /// <summary>
        /// Initiates a connection to the Myo device using the BLE manager.
        /// </summary>
        public void Connect()
        {
            _bleManager.ConnectionRoutineAsync();
        }

        /// <summary>
        /// Disconnects from the Myo device using the BLE manager.
        /// </summary>
        public void Disconnect()
        {
            _bleManager.DisconnectAsync();
        }

        /// <summary>
        /// Processes new input from the connected block, handles streaming status, and outputs EMG samples.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input value received from the block.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            bool noNewSample = false;

            // Get BLE connection states.
            bool connected = _bleManager.GetSpecificBLECopy()[SPECIFIC_BLE_STATES.CONNECTED];
            bool streaming = _bleManager.GetSpecificBLECopy()[SPECIFIC_BLE_STATES.STREAMING];

            // Reset EMG samples if not connected or streaming.
            if (!connected || !streaming)
                _emgSample = new double[_numOfEMGChannels];

            // Shorten buffer if necessary.
            if (connected && streaming)
                _jumpedSamples += _bleManager.ShortenBuffer(_maxDelay);

            // Retrieve the latest sample.
            byte[] newByteSample = _bleManager.DequeueBuffer();
            if (newByteSample == null)
            {
                noNewSample = true;
            }
            else
            {
                sbyte[] newEmgSample = new sbyte[newByteSample.Length];
                for (int i = 0; i < newByteSample.Length; i++)
                    newEmgSample[i] = (sbyte)newByteSample[i];

                for (int i = 0; i < _numOfEMGChannels; i++)
                    _emgSample[i] = newEmgSample[i] / _myoHwEmgScale;
            }

            // Output the normalized EMG sample as a vector.
            SendOutput(Vector.Build.DenseOfArray(_emgSample));

            // PERFORMANCE TESTS (Optional: Uncomment for debugging or analytics).
            if (connected && streaming)
                _outputtedMyo++;
            if (noNewSample && connected && streaming)
                _outputtedOld++;
        }

        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        internal void StartScan()
        {
            if (BluetoothLEManager.CheckWatcherNull() || BluetoothLEManager.GetWatcherStatus() != DeviceWatcherStatus.Started)
                BluetoothLEManager.StartNewDeviceWatcher();
        }

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        internal void StopScan()
        {
            if (!BluetoothLEManager.CheckWatcherNull() && BluetoothLEManager.GetWatcherStatus() == DeviceWatcherStatus.Started)
                BluetoothLEManager.StopDeviceWatcher();
        }

        #endregion
    }
    #endregion

}
