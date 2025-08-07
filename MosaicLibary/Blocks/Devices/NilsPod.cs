using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Windows.Devices.Enumeration;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    public partial class cpNilsPod : ControlPanel
    {
        NilsPod _SourceBlock;

        public cpNilsPod(NilsPod _SourceBlock) : base(_SourceBlock) { InitializeComponent(); this._SourceBlock = _SourceBlock;  }

        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            smNilsPod.Update(_SourceBlock.Data as Vector);

            // only gets infos event fired beforehand => to save resources
            if (!updateUI) return;

            var generalBLEInfo = BluetoothLEManager.GetGeneralBLECopy();

            if (generalBLEInfo[GENERAL_BLE_STATES.SCANNING])
            {
                StartScan.Enabled = false;
                StopScan.Enabled = true;

                // disable connect
                cbConnectDisconnect.Enabled = false;

                // disable combobox
                DeviceDropDown.Enabled = false;
            }
            else
            {
                // if not scanning
                StartScan.Enabled = true;
                StopScan.Enabled = false;
                if (!predefinedMAC) DeviceDropDown.Enabled = true;

                if (generalBLEInfo[GENERAL_BLE_STATES.FOUND_DEVICES])
                    // enable connection
                    cbConnectDisconnect.Enabled = true;
            }

            if (DeviceDropDown.SelectedItem == null) cbConnectDisconnect.Enabled = false;


            // ignore dropdown events if mac is predefined
            if (predefinedMAC)
            {
                // get name and mac as tuple
                Tuple<string, string> name_and_mac = _SourceBlock.bleManager.GetNameAndMac();
                string name = name_and_mac.Item1, mac = name_and_mac.Item2;
                if (name != null)
                {
                    // update device name
                    DeviceDropDown.Items.Clear();

                    string newItem = $"{name} | {mac}";

                    DeviceDropDown.Items.Add(newItem);
                    DeviceDropDown.SelectedItem = newItem;
                }
            }
            else
            {

                lastDevicesCopy = BluetoothLEManager.GetDevicesCopy();
                Queue<string> deviceStringQ = new Queue<string>();
                foreach (var item in lastDevicesCopy)
                {
                    if (item.Value.Name != null && item.Value.Name != "")
                    {
                        string newItem = $"{item.Value.Name} | {item.Key}";
                        deviceStringQ.Enqueue(newItem);
                    }
                }
                string[] deviceStrings = deviceStringQ.ToArray();

                string lastSelection = DeviceDropDown.Text;

                DeviceDropDown.Items.Clear();
                DeviceDropDown.Items.AddRange(deviceStrings);

                try
                {
                    DeviceDropDown.SelectedItem = lastSelection;
                }
                catch (Exception) { }
            }
            updateUI = false;
        }

        Dictionary<string, DeviceInformation> lastDevicesCopy;
        DeviceInformation selectedDevice;

        // for UI updates
        private bool updateUI = false;

        // is MAC predefined in YAML file?
        private bool predefinedMAC = true;

        private void BLEUiUpdateEvent(object sender, EventArgs e) { updateUI = true; }

        private void cbConnectDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cbConnectDisconnect.Checked)
            {
                _SourceBlock.Connect();
                cbConnectDisconnect.Text = "Disconnect";
            }
            else
            {
                _SourceBlock.Disconnect();
                cbConnectDisconnect.Text = "Connect";
            }
        }

        private void StartScan_Click(object sender, EventArgs e) { _SourceBlock.StartScan(); }

        private void StopScan_Click(object sender, EventArgs e) { _SourceBlock.StopScan(); }

        private void DropDownClickEvent(object sender, EventArgs e) { }

        private void DropDown_SelectedChanged(object sender, EventArgs e)
        {
            // ignore dropdown events if mac is predefined
            if (predefinedMAC) return;

            // copy of box
            ComboBox _cmBx = (ComboBox)sender;

            int macIdx = _cmBx.Text.IndexOf('|');
            string macAddress = _cmBx.Text.Substring(macIdx + 2);
            selectedDevice = lastDevicesCopy[macAddress];

            // tell bleManager that this is wanted device
            _SourceBlock.bleManager.SetWantedDevice(selectedDevice);

            cbConnectDisconnect.Enabled = true;
        }
    }

    // ----------------------------------------------------------------------------------
    // NilsPod - Receives data from a ble device connected through BLE and fires out the EMG samples
    // ----------------------------------------------------------------------------------
    public class NilsPod : Block
    {
        const int SAMPLE_LENGTH = 14; // ! in Bytes

        public static double ACC_FACTOR = 1 / (Math.Pow(2, 16) / 16 / 2);
        public static double GYRO_FACTOR = 1 / (Math.Pow(2, 16) / 2000 / 2);

        public string mac_address;

        // object of bleManager to connect, command, subscribe and notify etc...
        internal BluetoothLEManager bleManager;

        public NilsPod(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Myo has one input only, and it must be a timer
            if (InputBlocks.Count != 1) throw new Exception($"Myo {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"Myo {Name}'s input must be a Timer.");

            // a Myo's DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;

            // if Params is unspecified, look for a Myo on the BLE stack; otherwise
            // assume Params[0] is the MAC address of the required myo
            if (Params == null) mac_address = string.Empty;
            else mac_address = Params[0];

            // Device BLE protocol
            Dictionary<string, (string service, string characteristic)> InitialNotifications = new Dictionary<string, (string service, string characteristic)>()
            {
                {"EMG0", (service:"6e400001-b5a3-f393-e0a9-e50e24dcca9e",characteristic:"6e400003-b5a3-f393-e0a9-e50e24dcca9e")},

            };
            Dictionary<string, (string service, string characteristic, byte[] cmd)> InitialCmds = new Dictionary<string, (string service, string characteristic, byte[] cmd)>()
            {
                {"NoSleep", (service: "6e400001-b5a3-f393-e0a9-e50e24dcca9e", characteristic: "6e400002-b5a3-f393-e0a9-e50e24dcca9e", new byte[]{ 0xC2} ) },

            };
            Dictionary<string, (string service, string characteristic, byte[] cmd)> EndCommands = new Dictionary<string, (string service, string characteristic, byte[] cmd)>()
            {
                {"NoFiltEMG_NoIMU_NoClass", (service: "6e400001-b5a3-f393-e0a9-e50e24dcca9e", characteristic: "6e400002-b5a3-f393-e0a9-e50e24dcca9e", new byte[]{ 0xC1 })},
            };
            bleManager = new BluetoothLEManager(mac_address, SAMPLE_LENGTH, InitialNotifications, InitialCmds, EndCommands);

            cp = new cpNilsPod(this);
        }

        public void Connect() { bleManager.ConnectionRoutineAsync(); }
        public void Disconnect() { bleManager.DisconnectAsync(); }

        double[] sample = new double[SAMPLE_LENGTH];
        int MAX_DELAY = 15; // in samples

        // PERFORMANCE TESTS only...
        double outputedSamples = 0;
        double outputedOld = 0;
        double jumpedSamples = 0;
        int printIndex = 0;

        override protected void OnNewInput(Block sender, object value)
        {
            bool noNewSample = false;

            // get device specific states
            bool connected = bleManager.GetSpecificBLECopy()[SPECIFIC_BLE_STATES.CONNECTED];
            bool streaming = bleManager.GetSpecificBLECopy()[SPECIFIC_BLE_STATES.STREAMING];

            // if not connected anymore set sample to 0s so it does not stream the last value
            if (!connected || !streaming) sample = new double[SAMPLE_LENGTH];

            // if necessary shorten buffer
            if (connected && streaming) jumpedSamples += bleManager.ShortenBuffer(MAX_DELAY);

            // get last sample
            byte[] new_byte_sample = bleManager.DequeueBuffer();

            // if queue is empty it returns null. In this case last sample is outputted again
            if (new_byte_sample == null) noNewSample = true;
            else
            {
                // convert depending on your device specs
                var gyroX = BitConverter.ToInt16(new_byte_sample, 0) * GYRO_FACTOR;
                var gyroY = BitConverter.ToInt16(new_byte_sample, 2) * GYRO_FACTOR;
                var gyroZ = BitConverter.ToInt16(new_byte_sample, 4) * GYRO_FACTOR;

                var accX = BitConverter.ToInt16(new_byte_sample, 6) * ACC_FACTOR * 9.81;
                var accY = BitConverter.ToInt16(new_byte_sample, 8) * ACC_FACTOR * 9.81;
                var accZ = BitConverter.ToInt16(new_byte_sample, 10) * ACC_FACTOR * 9.81;

                var counter = BitConverter.ToInt16(new_byte_sample, 12);

                // fill new sample:
                sample = new double[] { accX, accY, accZ, gyroX, gyroY, gyroZ };
            }

            // send it off as a vector of 8 doubles
            SendOutput(Vector.Build.DenseOfArray(sample));

            //< -------PERFORMANCE TESTS------->
            if (connected && streaming) outputedSamples++;
            if (noNewSample && connected && streaming) outputedOld++;
            // to not print every line...
            printIndex++;
            if (printIndex >= 100)
            {
                //Console.WriteLine($"{bleManager.GetDeviceNameElseMac()}: In BLE Buffer {bleManager.SampleBuffer.Count}");

                //double outputedOldRatio = Math.Round(outputedOld / outputedSamples, 6);
                //Console.WriteLine($"{bleManager.GetDeviceNameElseMac()}: Current percentage of Old-Arrays = {outputedOldRatio}");

                //double jumpedSamplesRatio = Math.Round(jumpedSamples / outputedSamples, 6);
                //Console.WriteLine($"{bleManager.GetDeviceNameElseMac()}: Current percentage of jumped Samples = {jumpedSamplesRatio}");
                //printIndex = 0;
            }
            //<-------END PERFORMANCE TESTS------->
        }

        internal void StartScan()
        {
            // start new watcher if null
            if (BluetoothLEManager.CheckWatcherNull())
            {
                BluetoothLEManager.StartNewDeviceWatcher();
            }
            // start new watcher if it is not started yet
            else if (BluetoothLEManager.GetWatcherStatus() != DeviceWatcherStatus.Started)
            {
                // Start new device watcher => Scanning
                BluetoothLEManager.StartNewDeviceWatcher();
            }
        }
        internal void StopScan()
        {
            if (!BluetoothLEManager.CheckWatcherNull() && BluetoothLEManager.GetWatcherStatus() == DeviceWatcherStatus.Started)
            {
                // Start new device watcher => Scanning
                BluetoothLEManager.StopDeviceWatcher();
            }
        }
    }
}
