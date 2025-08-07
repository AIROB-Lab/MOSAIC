using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Sockets;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;


namespace MosaicLibary
{
    // a Resampler's control panel
    public partial class cpMuoviSingleProbe : ControlPanel
    {
        private MuoviSingleProbe _sourceBlock;

        public bool IsStreaming = false;

        public cpMuoviSingleProbe(MuoviSingleProbe _SourceBlock) : base(_SourceBlock) 
        { 
            InitializeComponent(); 
            this._sourceBlock = _SourceBlock; 
        }
        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            spiderPlot1.Update(_sourceBlock.LatestSample as Vector);
            tB_Monitor1.Update(_sourceBlock.LatestSample as Vector);
            scopeMonitor1.Update(_sourceBlock.LatestSample as Vector);

            if (_sourceBlock.DataLoss)
            {
                label1.BackColor = Color.Red;
                _sourceBlock.DataLoss = false;
            }
            else
                label1.BackColor = Color.Empty;
        }
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            IsStreaming = !IsStreaming;
            if (!IsStreaming)
            {
                button1.Text = "Start";
                _sourceBlock.ShutdownMuovi();
            }
            else
            {
                button1.Text = "Stop";
                _sourceBlock.InitialiseMuovi();
            }
        }
    }


    /// <summary>
    /// The <see cref="MuoviSingleProbe"/> class acquires EMG data from a single Muovi probe over a TCP connection. 
    /// It expects exactly one <see cref="Timer"/> input block, which triggers data parsing at a specified sampling rate.
    /// </summary>
    /// <remarks>
    /// This version is specialized for a single probe and does not rely on the <c>Params</c> array to specify probe indices.
    /// You must be connected to the single probe’s Wi-Fi network. Press the On/Off Button for &gt;5 seconds on the probe 
    /// and connect to the probe’s Wi-Fi network before running this block.
    /// </remarks>
    /// <example>
    /// <code>
    /// muovi_singleprobe:
    /// {
    ///   Type: MuoviSingleProbe,
    ///   Inputs: [ myTimerBlock ]
    ///   # Note: No Params array is provided here, 
    ///   #       as this block uses a single default probe internally.
    /// }
    /// </code>
    /// <para>Explanation:</para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: MuoviSingleProbe</c> indicates that this block connects to the Muovi SyncStation, 
    ///       awaiting a TCP connection and acquiring data from one selected probe.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ myTimerBlock ]</c> shows that it must have exactly one <see cref="Timer"/> 
    ///       (or a derived class like <see cref="ScheduledTimer"/>) to drive data parsing at the chosen rate.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// After the block establishes a TCP connection with the Muovi SyncStation, it begins streaming EMG data 
    /// at a nominal 2 kHz rate and downsamples/batches the samples according to the timer’s <c>DesiredRate</c>.
    /// </para>
    /// </example>
    public class MuoviSingleProbe : Block
    {
        /// <summary>
        /// TCP client for communication with the Muovi SyncStation.
        /// </summary>
        public TcpClient client;
        /// <summary>
        /// Network stream for communication with the Muovi SyncStation.
        /// </summary>
        public NetworkStream stream;
        public StreamManager streamGSR;
        public Vector LatestSample;
        public bool IsStreaming = false;
        public bool DataLoss;


        private Socket sqSocket;
        private Vector emg;
        private Vector Value = Vector.Build.Dense(44);
        private Int16 ramp = 0;
        private String bufferRX = String.Empty;
        private String bufferParser = String.Empty;

        private int sampFreq = 2000;

        private int ptr = 0;

        private List<int> used_probes = new List<int>();

        /// <summary>
        /// Tracks the total Number of channels used.
        /// </summary>
        private int totNumChan;
        private const int SubSamplingRate = 18;
        private int MatrixLength;

        private delegate void ProcessNewInput(byte[] frame);
        private double _conversionFactor = 0.000286;
        private ProcessNewInput processNewInput;
        private Socket clientSocket;
        private int countReceived;
        private double lastTimestamp = 0;
        private int lastCountReceived;

        public MuoviSingleProbe(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // one input only, and it must be a timer
            if (InputBlocks.Count != 1)
                throw new Exception($"Muovi {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer))
                throw new Exception($"Muovi {Name}'s input must be a Timer.");

            // The number of samples over which we want to draw an average is given by the nominal Muovi rate (2kHz) divided by the desired rate as given by the timer used as input for the block. 
            MatrixLength = 2000 / (int)DesiredRate;

            if (MatrixLength == 1)
                processNewInput = ProcessNewInputVector;
            else
                processNewInput = ProcessNewInputMatrix;

            for (int i = 0; i < Params.Count; i++) { used_probes.Add(Convert.ToInt16(Params[i])); }

            // initialisation of variables
            int sizeComm = 0;
            int configStrLen = 2;
            double[] configStr = new double[18];

            cp = new cpMuoviSingleProbe(this);
        }

        public void InitialiseMuovi()
        {
            //totNumChan = 0;
            //client = new TcpClient("192.168.76.1", 54320);
            //stream = client.GetStream();
            SetUpConnection();

            byte[] config = BitConverter.GetBytes(9);
            byte[] config2 = { config[0] };
            stream = new NetworkStream(clientSocket);
            stream.Write(config2, 0, config2.Length);

            streamGSR = new StreamManager(stream, 10000, 1);
            streamGSR.DataReceived += StreamGSR_DataReceived;
            Value = Vector.Build.Dense(used_probes.Count * 32);
            StartStreaming();
        }

        /// <summary>
        /// Setting up the connection to the Muovi SyncStation. Creating a TCPClient on the IP address of the SyncStation 192.168.76.1 and the port 54320.
        /// Opens up Networkstream for communication.
        /// </summary>
        private void SetUpConnection()
        {
            sqSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sqSocket.ReceiveBufferSize = 10000;
            sqSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sqSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            IPEndPoint localEndPoint = new IPEndPoint(System.Net.IPAddress.Parse("0.0.0.0"), 54321);
            sqSocket.Bind(localEndPoint);
            // Listen for incoming connections
            sqSocket.Listen(1);
            Console.WriteLine("Waiting for connection...");
            clientSocket = sqSocket.Accept();
            Console.WriteLine("Client connected from: " + clientSocket.RemoteEndPoint);
        }


        /// <summary>
        /// This function is performing an n 8-bit Cyclic Redundancy Check (CRC-8) checksum.  
        /// CRC is used in error-checking protocols, where a small, fixed-size checksum (like 8 bits in this case) is generated from data to detect errors that may occur during 
        /// transmission or storage.
        /// TODO: check the for loop, it seems to be off by one.
        /// </summary>
        /// <param name="data">array of bytes received from Muovi</param>
        /// <param name="len">length of the</param>
        /// <returns></returns>
        public static int CRC8(byte[] data, int len)
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

        /// <summary>
        /// Starts the streaming process of the Muovi SyncStation.
        /// </summary>
        /// <exception cref="Exception">Thrown if SyncStation is already streaming.</exception>
        public void StartStreaming()
        {
            if (IsStreaming)
                throw new Exception($"GSR {Name} already streaming.");
            IsStreaming = true;
            streamGSR.StartStreaming();
        }

        public void ShutdownMuovi()
        {
            // shut down Muovi station //
            double[] configStr = new double[2];
            configStr[0] = 0;

            // check configStr byte array
            byte[] byteTemp = new byte[configStr.Length];
            for (int i = 0; i <= configStr.Length - 1; i++) { byteTemp[i] = (byte)configStr[i]; }
            configStr[1] = CRC8(byteTemp, 1);

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
            if (!IsStreaming)
                throw new Exception($"GSR {Name} already stopped streaming.");
            IsStreaming = false;
            streamGSR.StopStreaming();
        }

        void StreamGSR_DataReceived(byte[] receivedBytes)
        {
           
            string addition = System.Text.Encoding.GetEncoding(1252).GetString(receivedBytes);
            //  Console.WriteLine("receivded data");
            lock (bufferRX)
            {
                if (bufferRX.Length <= totNumChan * 2 * 18 * 20 && !DataLoss) //--> marek
                                                                              //if (bufferRX.Length <= totNumChan * 2 * 18 * 20) // --> alisa
                {
                    bufferRX += addition;
                    //Console.WriteLine(bufferRX.Length);
                    //ptr = 0;
                }
                else
                {
                    Console.WriteLine("Overflow:" + bufferRX.Length);
                    bufferRX = addition;
                }
            }
        }


        override protected void OnNewInput(Block sender, object value)
        {

            lock (bufferRX) 
            { 
                bufferParser = String.Copy(bufferRX); 
            }

            {
                // if my timer ticks, parse and send out the current value           
                if (bufferParser.Length >= totNumChan * 2 * MatrixLength && totNumChan != 0)
                {
                    byte[] frame = System.Text.Encoding.GetEncoding(1252).GetBytes(bufferParser.ToCharArray(), 0, 2 * totNumChan * MatrixLength); // get the first frames from the Muovi
                    processNewInput(frame);
                }
            }
        }

        private void CheckForDataLoss(byte[] frame, int offset = 0)
        {
            byte[] ramp_val = { frame[2 * (totNumChan - 1) + 1 + offset], frame[2 * (totNumChan - 1) + offset] };
            short ramp_new = (BitConverter.ToInt16(ramp_val, 0));
            //Console.WriteLine( "new sample counter: " +ramp_new);
            if (ramp_new - ramp != 1 && ramp_new - ramp != Int16.MaxValue)
            {
                DataLoss = true;
                Console.WriteLine("Last counter: " + ramp + " New counter: " + ramp_new);
            }
            ramp = ramp_new;
        }

        private void DeleteFirstNSamplesFromBuffer(int n = 1)
        {
            // delete first frame from the general buffer
            lock (bufferRX)
            {
                try
                {
                    bufferRX = bufferRX.Remove(0, totNumChan * 2 * n);
                }
                catch
                {
                    Console.WriteLine("Error while removing frame");
                }

            }
        }

        private void ProcessNewInputMatrix(byte[] frame)
        {
            // preparation: create a row matrix to store the values
            Matrix valueArr = Matrix.Build.Dense(MatrixLength, used_probes.Count * 32);

            // read samples and fill the row matrix
            for (int i = 0; i < MatrixLength; i++)
            {
                int offset = i * totNumChan * 2;
                Value = ReadAndDecodeSample(frame, offset);
                CheckForDataLoss(frame, offset);
                valueArr.SetRow(i, Value); // set the corresponding row in the value matrix
            }
            LatestSample = Value;
            //Console.WriteLine("Read value length: "+ Value.Count);
            SendOutput(valueArr);
            DeleteFirstNSamplesFromBuffer(MatrixLength);
        }

        private void ProcessNewInputVector(byte[] frame)
        {
            Value = ReadAndDecodeSample(frame);
            CheckForDataLoss(frame);
            LatestSample = Value;
            SendOutput(Value);
            DeleteFirstNSamplesFromBuffer();
        }

        private Vector ReadAndDecodeSample(byte[] frame, int offset = 0)
        {
            // zero Vector as base 
            Vector value = Vector.Build.Dense(used_probes.Count * 32);

            // read first value in the frame
            for (int i = 0; i < used_probes.Count; i++) // iterate over the probes
            {
                for (int j = 0; j < 32; j++) // iterate over the channels
                {
                    byte[] val = { frame[2 * (j + i * 38) + 1 + offset], frame[2 * (j + i * 38) + offset] };
                    value[i * 32 + j] += ((double)(BitConverter.ToInt16(val, 0))) * _conversionFactor;
                }
            }
            return value;
        }
    }
}
