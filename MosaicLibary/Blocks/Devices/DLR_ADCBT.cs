using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows.Devices.Enumeration;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    #region Control Panel
    /// <summary>
    /// Represents the control panel for the DLR ADC/BT device (<see cref="DLR_ADCBT"/>), handling UI interactions for connecting, disconnecting, and displaying data from the device.
    /// </summary>
    public partial class cpDLR_ADCBT : ControlPanel
    {
        private DLR_ADCBT _sourceBlock;
        private Dictionary<string, DeviceInformation> _lastDevicesCopy;
        private DeviceInformation _selectedDevice;

        // For UI updates
        private bool _updateUI = false;

        // Is MAC predefined in YAML file?
        private bool _predefinedMAC = true;

        /// <summary>
        /// Event handler for updating BLE UI.
        /// </summary>
        private void BLEUiUpdateEvent(object sender, EventArgs e) { _updateUI = true; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CpDLR_ADCBT"/> class.
        /// </summary>
        /// <param name="sourceBlock">The DLR_ADCBT source block associated with this control panel.</param>
        public cpDLR_ADCBT(DLR_ADCBT sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;

            // Initial UI states - disable in beginning
            cbConnectDisconnect.Enabled = false;

            numericUpDown1.Value = Convert.ToInt32((sourceBlock.PortNumber.Split('M')).Last());
        }

        /// <summary>
        /// Refreshes the control panel with the latest data from the DLR_ADCBT device.
        /// </summary>
        /// <param name="myObject">The sender object of the refresh event.</param>
        /// <param name="myEventArgs">The event arguments.</param>
        protected override void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            smDaq.Update(_sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Sets the COM port value and enables the connect/disconnect checkbox.
        /// </summary>
        /// <param name="value">The COM port value.</param>
        public void SetComValue(int value)
        {
            cbConnectDisconnect.Enabled = true;
            numericUpDown1.Value = value;
        }

        /// <summary>
        /// Event handler for the Connect/Disconnect checkbox state change.
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
        /// Event handler for numericUpDown1 value change.
        /// </summary>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _sourceBlock.PortNumber = "COM" + numericUpDown1.Value.ToString();
            cbConnectDisconnect.Enabled = true;
        }

        private void smDaq_Load(object sender, EventArgs e) { }
    }
    #endregion

    #region DLR ADC/BT
    /// <summary>
    /// Represents a communication block for interfacing with a DLR ADC/BT device.
    /// This class handles connection, configuration, and data acquisition from the device.
    /// </summary>
    public class DLR_ADCBT : Block
    {
        private string _portNumber;
        private int _bytesPerFrame = 0;
        private string _tmpReadBuffer = string.Empty;
        private string _bufferParser = string.Empty;
        private string _frame = string.Empty;

        private SerialPort _serialPort;
        private StreamManager _streamManager;
        private double[] _sample;

        private readonly byte[] _cmd_numOfChannels = { 0xAA, 0x7A, 0x03, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8F, 0xFF, 0xEF };
        private readonly byte[] _cmd_startStreaming = { 0xAA, 0x7A, 0x01, 0x6A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8F, 0xFF, 0xEF };
        private readonly byte[] _cmd_stopStreaming = { 0xAA, 0x7A, 0x01, 0x6B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x8F, 0xFF, 0xEF };

        private readonly string _frameStart = string.Format("{0}{1}", (char)0xAA, (char)0x7A);
        private readonly char _frameEnding1 = (char)0xFF;
        private readonly char _frameEnding2 = (char)0xFF;

        /// <summary>
        /// Gets or sets the serial port number used for communication with the DLR device.
        /// </summary>
        public string PortNumber
        {
            get { return _portNumber; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _portNumber = value;
                    if (_serialPort != null)
                    {
                        _serialPort.PortName = _portNumber;
                    }
                }
            }
        }

        /// <summary>
        /// The number of channels to be read from the DLR device. Default is 10.
        /// </summary>
        public int NumOfChannels { get; set; } = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLR_ADCBT"/> class.
        /// </summary>
        /// <param name="Name">The name of the block.</param>
        /// <param name="DesiredRate">The desired rate of data acquisition.</param>
        /// <param name="InputCfg">Configuration for input blocks.</param>
        /// <param name="Params">Additional parameters for device configuration.</param>
        /// <param name="Path">The path used for device communication.</param>
        public DLR_ADCBT(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the input settings for the DLR_ADCBT block.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            if (InputBlocks.Count != 1) throw new Exception($"DLR_ADCBT {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"DLR_ADCBT {Name}'s input must be a Timer.");

            DesiredRate = InputBlocks[0].DesiredRate;

            if (Params == null) PortNumber = string.Empty;
            else
            {
                PortNumber = Params[0];
                NumOfChannels = Convert.ToInt32(Params[1]);
            }
            _sample = new double[NumOfChannels];

            cp = new cpDLR_ADCBT(this);
        }

        /// <summary>
        /// Establishes a connection to the DLR device and starts the data stream.
        /// </summary>
        public void Connect()
        {
            _cmd_numOfChannels[4] = (byte)NumOfChannels;
            _bytesPerFrame = 4 + 2 * NumOfChannels + 4;

            _serialPort = new SerialPort(PortNumber, 115200, Parity.None, 8, StopBits.One)
            {
                Encoding = Encoding.GetEncoding(1252),
                ReceivedBytesThreshold = _bytesPerFrame,
                ReadBufferSize = 100 * _bytesPerFrame,
                ReadTimeout = -1,
                WriteBufferSize = 2048,
                WriteTimeout = -1
            };

            _serialPort.Open();
            StartStream();
        }

        /// <summary>
        /// Disconnects from the DLR device and stops the data stream.
        /// </summary>
        public void Disconnect()
        {
            StopStream();
            _serialPort.Close();
        }

        /// <summary>
        /// Starts streaming data from the device.
        /// </summary>
        public void StartStream()
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Write(_cmd_numOfChannels, 0, _cmd_numOfChannels.Length);
                _streamManager = new StreamManager(_serialPort.BaseStream, _bytesPerFrame, 0);
                _streamManager.StartStreaming();
                _streamManager.DataReceived += AsyncReadBody;

                _serialPort.Write(_cmd_startStreaming, 0, _cmd_startStreaming.Length);
            }
        }

        /// <summary>
        /// Stops streaming data from the device.
        /// </summary>
        public void StopStream()
        {
            if (_serialPort?.IsOpen == true)
            {
                _streamManager.StopStreaming();
                _streamManager.DataReceived -= AsyncReadBody;
            }
        }

        /// <summary>
        /// Reads data asynchronously from the device.
        /// </summary>
        /// <param name="received_bytes">The received bytes.</param>
        private void AsyncReadBody(byte[] received_bytes)
        {
            lock (_bufferParser)
            {
                if (_bufferParser.Length <= NumOfChannels * 2 * 18 * 10)
                {
                    _bufferParser += Encoding.GetEncoding(1252).GetString(received_bytes);
                }
                else
                {
                    Console.WriteLine("Overflow");
                    string addition = Encoding.GetEncoding(1252).GetString(received_bytes);
                    _bufferParser = addition.Length == NumOfChannels * 2 * 18 ? _serialPort.ReadExisting() : _bufferParser + addition;
                }
            }
        }

        /// <summary>
        /// Processes new input data from the connected block. 
        /// It identifies and extracts valid frames of data from the buffer, validates frame integrity,
        /// parses the sensor values, and sends the parsed data as a vector of doubles.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input value received from the block.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            if (_bufferParser.Length >= _bytesPerFrame)
            {
                lock (_bufferParser) _tmpReadBuffer = _bufferParser;

                int startIndex = _tmpReadBuffer.IndexOf(_frameStart);
                if (startIndex == -1 || _tmpReadBuffer.Length < startIndex + _bytesPerFrame) 
                    return;

                _frame = _tmpReadBuffer.Substring(startIndex, _tmpReadBuffer[startIndex + 2]);

                if (_frame[2] != _bytesPerFrame)
                {
                    Console.WriteLine("length mismatch.");
                    lock (_bufferParser) _bufferParser = _bufferParser.Remove(0, startIndex + _frame[2]);
                    return;
                }

                if (_frame[_frame.Length - 2] != _frameEnding1 || _frame[_frame.Length - 1] != _frameEnding2)
                {
                    Console.WriteLine("frame ending error.");
                    lock (_bufferParser) _bufferParser = _bufferParser.Remove(0, startIndex + _frame[2]);
                    return;
                }


                byte[] frameBytes = Encoding.Default.GetBytes(_frame);
                for (int frameIdx = 4, channelIdx = 0; frameIdx < _frame[2] - 4; frameIdx += 2)
                    _sample[channelIdx++] = 5.0 * (256 * frameBytes[frameIdx] + frameBytes[frameIdx + 1]) / 4000.0;

                lock (_bufferParser) 
                    _bufferParser = _bufferParser.Remove(0, startIndex + _bytesPerFrame);
            }

            SendOutput(Vector.Build.DenseOfArray(_sample));
        }
    }
    #endregion
}