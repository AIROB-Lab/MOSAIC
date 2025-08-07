using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Sockets;


using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    // a Resampler's control panel
    public partial class cpMuovi : ControlPanel
    {
        Muovi _SourceBlock;

        public bool IsStreaming = false;

        public cpMuovi(Muovi _SourceBlock) : base(_SourceBlock) { InitializeComponent(); this._SourceBlock = _SourceBlock; }

        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            spiderPlot1.Update(_SourceBlock.Data as Vector);
            tB_Monitor1.Update(_SourceBlock.Data as Vector);
            scopeMonitor1.Update(_SourceBlock.Data as Vector);

            if (_SourceBlock.DataLoss) { label1.BackColor = Color.Red; _SourceBlock.DataLoss = false; }
            else label1.BackColor = Color.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsStreaming = !IsStreaming;
            if (!IsStreaming) { button1.Text = "Start"; _SourceBlock.ShutdownMuovi(); }
            else { button1.Text = "Stop"; _SourceBlock.InitialiseMuovi(); }
        }

    }

    /// <summary>
    /// Represents a specialized <see cref="Block"/> that handles data streaming from a Muovi device over a TCP connection.
    /// </summary>
    /// <remarks>
    /// This class connects to a Muovi Syncstation device, configures its data acquisition settings, and streams incoming data. 
    /// It relies on a <see cref="ScheduledTimer"/> Block to drive its updates (e.g., how frequently data is processed).
    /// </remarks>
    /// <example>
    /// <para>
    /// A minimal YAML configuration might look like this:
    /// </para>
    /// <code>
    /// muovi:
    /// {
    ///   Type: Muovi,
    ///   Inputs: [ myTimerBlock ],
    ///   Params: [ 0, 1, 2 ]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: Muovi</c> indicates this block communicates with the Muovi Syncstation.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ myTimerBlock ]</c> ensures it has exactly one <see cref="Timer"/> (or <see cref="ScheduledTimer"/>) 
    ///     dictating how frequently data frames are parsed and output.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Params: [ 0, 1, 2 ]</c> lists the **probe indices** (subtracting 1 from the actual probe number). 
    ///     For example, if the hardware label on the back of the probe is 3, you would add <c>2</c> to this list. Double check with the LEDs on the Syncstation. Probes tend to rename themselves to Probe 1.</description>
    ///   </item>
    /// </list>
    /// </para>
    /// </example>
    public class Muovi : Block
    {

        

        /// <summary>
        /// The TCP client used to connect to the Muovi device.
        /// </summary>
        public TcpClient client;

        /// <summary>
        /// The network stream to read/write Muovi data.
        /// </summary>
        public NetworkStream stream;

        /// <summary>
        /// Manages continuous data streaming for the Muovi device.
        /// </summary>
        public StreamManager streamGSR;

        /// <summary>
        /// Indicates whether the Muovi device is currently streaming data.
        /// </summary>
        public bool IsStreaming = false;




        /// <summary>
        /// Internal vector to store EMG data.
        /// </summary>
        private Vector emg;

        /// <summary>
        /// Holds the single input block, which should be a <see cref="Timer"/> (or a <see cref="ScheduledTimer"/>).
        /// </summary>
        private Block _inputTimer;

        /// <summary>
        /// Represents the most recent processed values to be sent as output.
        /// </summary>
        private Vector Value = Vector.Build.Dense(44);

        /// <summary>
        /// Tracks a ramp value from the Muovi data stream.
        /// </summary>
        private Int16 ramp = 0;

        /// <summary>
        /// Buffer for incoming data (raw bytes read from the <see cref="stream"/>).
        /// </summary>
        private String bufferRX = String.Empty;

        /// <summary>
        /// Buffer for parsing incoming data before conversion.
        /// </summary>
        private String bufferParser = String.Empty;

        /// <summary>
        /// Temporary array to hold short integer data from incoming frames.
        /// </summary>
        private Int16[] data_short;

        /// <summary>
        /// The sampling frequency, typically 2000 Hz for Muovi EMG data.
        /// </summary>
        private int sampFreq = 2000;

        /// <summary>
        /// A pointer or index used during parsing of the data buffer.
        /// </summary>
        private int ptr = 0;

        /// <summary>
        /// Lists the indices of the probes that are being used/activated.
        /// </summary>
        private List<int> used_probes = new List<int>();

        /// <summary>
        /// The total number of channels in the incoming Muovi data.
        /// </summary>
        private int totNumChan;

        /// <summary>
        /// The subsampling rate used for streaming data.
        /// </summary>
        private const int SubSamplingRate = 18;

        /// <summary>
        /// Indicates how many samples are averaged together before output.
        /// </summary>
        private int averageOver = 1;

        /// <summary>
        /// Indicates if there has been data loss during streaming.
        /// </summary>
        public bool DataLoss;

        /// <summary>
        /// Initializes a new instance of the <see cref="Muovi"/> class with the specified parameters.
        /// </summary>
        /// <param name="Name">The name of this Muovi block.</param>
        /// <param name="DesiredRate">The desired data processing rate (Hz).</param>
        /// <param name="InputCfg">The names of input blocks (should only contain one timer block).</param>
        /// <param name="Params">Parameters indicating which probes to enable.</param>
        /// <param name="Path">A file path for configuration or other purposes.</param>
        public Muovi(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the input blocks for the Muovi device, ensuring that exactly one <see cref="Timer"/> is used.
        /// </summary>
        /// <exception cref="Exception">Thrown if there is not exactly one input block or if that block is not a <see cref="Timer"/>.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // one input only, and it must be a timer
            if (InputBlocks.Count != 1) throw new Exception($"Myo {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"Myo {Name}'s input must be a Timer.");

            // DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;

            // The number of samples over which we want to draw an average is given by the nominal Muovi rate (2kHz) divided by the desired rate as given by the timer used as input for the block. 
            averageOver = 2000 / (int)DesiredRate;

            for (int i = 0; i < Params.Count; i++) 
            { 
                used_probes.Add(Convert.ToInt16(Params[i])); 
            }

            // initialisation of variables
            int sizeComm = 0;
            int configStrLen = 2;
            double[] configStr = new double[18];

            cp = new cpMuovi(this);
        }

        /// <summary>
        /// Initializes the Muovi device by configuring it and starting up the TCP connection.
        /// </summary>
        /// <exception cref="SocketException">Thrown if the TCP client fails to connect to the Muovi device.</exception>
        public void InitialiseMuovi()
        {
            totNumChan = 0;
            client = new TcpClient("192.168.76.1", 54320);
            stream = client.GetStream();

            // initialisation of variables
            int sizeComm = 0;
            int configStrLen = 2;
            double[] configStr = new double[18];

            int[] deviceEN = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] emgEN = { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] mode = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] numChan = { 38, 38, 38, 38, 70, 70, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };

            for (int i = 0; i < used_probes.Count; i++)
            {
                deviceEN[used_probes[i]] = 1;// tick 1 if in use
                emgEN[used_probes[i]] = 1; // tick 1 if in use for EMG
                mode[used_probes[i]] = 0; // 0=32Ch Monop, 1=16Ch Monp, 2=32Ch ImpCk, 3=32Ch Test
            }



            for (int i = 0; i < 16; i++) { sizeComm = sizeComm + deviceEN[i]; }

            configStr[0] = sizeComm * 2 + 1;

            for (int i = 0; i <= deviceEN.Length - 1; i++)
            {
                if (deviceEN[i] == 1)
                {
                    configStr[configStrLen - 1] = i * 16 + emgEN[i] * 8 + mode[i] * 2 + 1;

                    totNumChan = totNumChan + numChan[i];
                    //num_probes ++;

                    if (emgEN[i] == 1) { sampFreq = 2000; }

                    configStrLen++;
                }
            }

            emg = Vector.Build.Dense(used_probes.Count * 32);

            int[] syncStatChan = { totNumChan + 1, totNumChan + 2, totNumChan + 3, totNumChan + 4, totNumChan + 5, totNumChan + 6 };
            totNumChan = totNumChan + 6;

            // check configStr byte array
            byte[] byteTemp = new byte[configStr.Length];
            for (int i = 0; i <= configStr.Length - 1; i++) { byteTemp[i] = (byte)configStr[i]; }
            configStr[configStrLen - 1] = crc8(byteTemp, configStrLen - 2);

            // send configuration to SyncStation
            byte[] packet = new byte[configStr.Length];
            for (int i = 0; i <= configStr.Length - 1; i++)
            {
                packet[i] = (byte)configStr[i];
            }
            stream.Write(packet, 0, configStrLen);

            //cp = new cpMuovi(this);
            streamGSR = new StreamManager(stream, totNumChan * 2 * SubSamplingRate, 1);
            streamGSR.DataReceived += StreamGSR_DataReceived;
            Value = Vector.Build.Dense(used_probes.Count * 32);
            StartStreaming();
        }


        /// <summary>
        /// Begins streaming data from the Muovi device.
        /// </summary>
        /// <exception cref="Exception">Thrown if the Muovi device is already streaming.</exception>
        public void StartStreaming()
        {
            if (IsStreaming) throw new Exception($"GSR {Name} already streaming.");
            IsStreaming = true;
            streamGSR.StartStreaming();
        }

        /// <summary>
        /// Shuts down the Muovi device by stopping streaming, and closing the stream and TCP client.
        /// </summary>
        public void ShutdownMuovi()
        {
            // shut down Muovi station //

            double[] configStr = new double[2];
            configStr[0] = 0;

            // check configStr byte array
            byte[] byteTemp = new byte[configStr.Length];
            for (int i = 0; i <= configStr.Length - 1; i++) { byteTemp[i] = (byte)configStr[i]; }
            configStr[1] = crc8(byteTemp, 1);

            // send configuration to SyncStation
            byte[] packet = new byte[configStr.Length];
            for (int i = 0; i <= configStr.Length - 1; i++)
            {
                packet[i] = (byte)configStr[i];
            }
            //stream.Write(packet, 0, packet.Length);

            StopStreaming();

            // close stream and TCP client
            stream.Close();
            client.Close();
            // stop reading input
        }
        public void StopStreaming()
        {
            if (!IsStreaming) throw new Exception($"GSR {Name} already stopped streaming.");
            IsStreaming = false;
            streamGSR.StopStreaming();
        }

        /// <summary>
        /// Handles incoming data bytes from the <see cref="StreamManager"/>.
        /// Accumulates the data in <see cref="bufferRX"/> or detects overflow.
        /// </summary>
        /// <param name="receivedBytes">An array of bytes received from the stream.</param>
        private void StreamGSR_DataReceived(byte[] receivedBytes)
        {
            lock (bufferRX) { 
                if (bufferRX.Length <= totNumChan * 2 * 18 * 10 && ! DataLoss)
                {
                    string addition = System.Text.Encoding.GetEncoding(1252).GetString(receivedBytes);
                    bufferRX += addition;
                    //Console.WriteLine(bufferRX.Length);
                    //ptr = 0;
                }
                else
                {
                    Console.WriteLine("Overflow");
                    string addition = System.Text.Encoding.GetEncoding(1252).GetString(receivedBytes);
                    if (addition.Length == totNumChan * 2 * 18) bufferRX = System.Text.Encoding.GetEncoding(1252).GetString(receivedBytes);
                    else bufferRX += addition;
                }
        }
        }

        /// <summary>
        /// Called whenever there is new input from the timer block.
        /// Parses the incoming Muovi data, updates the <see cref="Value"/>, and sends output.
        /// </summary>
        /// <param name="sender">The block that triggered this method call (usually a <see cref="Timer"/>).</param>
        /// <param name="value">The value passed from the input block.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            bufferParser = bufferRX;
            
            {
                // if my timer ticks, parse and send out the current value           
                if (bufferParser.Length >= totNumChan * 2 * averageOver)
                {
                    byte[] frame = System.Text.Encoding.GetEncoding(1252).GetBytes(bufferParser);

                    for (int j = 0; j < Value.Count; j++)
                    {
                        Value[j] = 0;
                    }
                    for (int z = 0; z < averageOver; z++) { 
                        for (int i = 0; i < used_probes.Count; i++)
                        {
                            for (int j = 0; j < 32; j++)
                            {
                                byte[] val = { frame[2 * (j + i * 38) + 1 + z * totNumChan * 2], frame[2 * (j + i * 38) + z * totNumChan * 2] };
                                Value[i * 32 + j] += ((double)(BitConverter.ToInt16(val, 0))) / 32768 / averageOver;
                            }
                        }
                    }
                    SendOutput(Value);
                    byte[] ramp_val = { frame[2 * (totNumChan - 1) + 1], frame[2 * (totNumChan - 1)] };
                    if (BitConverter.ToInt16(ramp_val, 0) - ramp != averageOver && BitConverter.ToInt16(ramp_val, 0) - ramp != Int16.MaxValue)
                    {
                        DataLoss = true;
                    }
                    ramp = BitConverter.ToInt16(ramp_val, 0);
                    lock (bufferRX)
                    {
                        bufferRX = bufferRX.Remove(0, totNumChan * 2 * averageOver);
                    }
                }
            }
        }

        #region static methods
        /// <summary>
        /// Computes an 8-bit CRC for the given data array.
        /// </summary>
        /// <param name="data">The data for which the CRC will be calculated.</param>
        /// <param name="len">The number of bytes to process from the data array.</param>
        /// <returns>An 8-bit CRC value.</returns>
        public static int crc8(byte[] data, int len)
        {
            int sum;
            int extract;
            int crc = 0;

            // compute checksum
            for (int i = 0; i <= len; i++)
            {
                extract = data[i];

                for (int j = 8; j > 0; j--)
                {
                    sum = (crc ^ extract) & 1;
                    crc = crc >> 1;
                    if (sum == 1)
                    {
                        crc = crc ^ 140;
                    }
                    extract = extract >> 1;
                }
            }

            return crc;
        }
        #endregion
    }
}
