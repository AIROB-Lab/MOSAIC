using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

using Windows.Devices.Enumeration;

using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace MosaicLibary
{
    // ---------------------------
    // HannesReceiver - via UDP
    // ---------------------------
    public partial class cpHannesReceiver : ControlPanel
    {
        HannesReceiver _SourceBlock;

        public cpHannesReceiver(HannesReceiver _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent(); this._SourceBlock = _SourceBlock;

        }

        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            smHannesReceiver.Update(_SourceBlock.Data as Vector);
        }

        private void cbConnectDisconnect_CheckedChanged(object sender, EventArgs e)
        {
            if (cbConnectDisconnect.Checked)
            {
                // start UDP listener
                _SourceBlock.StartUDPlistener();
                cbConnectDisconnect.Text = "Disconnect";
                //cbConnectDisconnect.Enabled = false;
                _SourceBlock.CONNECTED = true;
            }
            else
            {
                // stop UDP listener
                _SourceBlock.StopUDPlistener();
                cbConnectDisconnect.Text = "Connect";
            }
        }
    }

    public class HannesReceiver : Block
    {
        //Client uses as receive udp client
        UdpClient Client; //= new UdpClient(11000);
        internal bool CONNECTED = false;
        internal bool toDISCONNECT = false;
        
        // new buffer for incoming samples
        public List<double[]> SampleBuffer = new List<double[]>(); 
        private static object Lock_Buffer = new object();// needs lock

        // num of values coming in
        int num_emgs;

        public HannesReceiver(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) 
        {
            // Params: Port, numEMGs
            Client = new UdpClient(int.Parse(Params[0]));
            num_emgs = int.Parse(Params[1]);
        }


        /// <summary>
        /// starts udp listener to specified port
        /// </summary>
        internal void StartUDPlistener()
        {
            try
            {
                // this starts recursive(!) async loop
                Client.BeginReceive(new AsyncCallback(recv), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        /// <summary>
        /// gives signal to stop udp listener by setting the toDISCONNECT flag to false
        /// </summary>
        internal void StopUDPlistener()
        {
            // breaks recursive callback
            toDISCONNECT = true;
        }

        //Recursive CallBack!
        void recv(IAsyncResult res)
        {
            // flag breaks recursive callback
            if (toDISCONNECT == true) { toDISCONNECT = false; CONNECTED = false; return; }

            // receives from any ip and any port, which send to this port 11000 (as this is only receiving, no need to specify sender for now)
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);

            // watch out for next sample
            Client.BeginReceive(new AsyncCallback(recv), null);

            // ---- START PROCESSING OF SAMPLES (currently hannes specific, so I could test with sample and send it to localhost) ---
            byte cmd = received[1];
            byte[] messsage = new byte[12];
            Array.Copy(received, 2, messsage, 0, 12);

            double[] emg_signals = new double[6];

            for (int i = 0; i < emg_signals.Length; i++)
            {
                emg_signals[i] = BitConverter.ToUInt16(messsage, i * sizeof(UInt16)) / Math.Pow(2.0, 16);
            }

            // add to buffer, which then can be read by the timed output again
            lock (Lock_Buffer)
            {
                SampleBuffer.Add(emg_signals);
            }
        }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a HannesReceiver has one input only, and it must be a timer
            if (InputBlocks.Count != 1) throw new Exception($"HannesReceiver {Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer)) throw new Exception($"HannesReceiver {Name}'s input must be a Timer.");

            // a HannesReceiver's DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;

            cp = new cpHannesReceiver(this);

        }

        // defined MAX delay for Hannes UDP connection
        int MAX_DELAY = 10;

        //stats
        int i = 0;
        int i_stats = 1000;
        int nullSamples = 0;
        int bufferCount = 0;
        int jumpedSamples = 0;
       
        override protected void OnNewInput(Block sender, object value)
        {
            double[] last_sample = new double[num_emgs];
            
            // get last sample
            lock (SampleBuffer)
            {
                if(CONNECTED)
                {
                    if (SampleBuffer.Count > MAX_DELAY)
                    {
                        jumpedSamples += ShortenBuffer(MAX_DELAY);
                    }
                    else if (SampleBuffer.Count > 0)
                    {
                        last_sample = SampleBuffer[0];
                        SampleBuffer.RemoveAt(0);
                    }
                    else
                    {
                        //stats
                        nullSamples++;
                    }

                    //stats
                    bufferCount = SampleBuffer.Count;
                }
            }
            // only happend once...
            if (last_sample == null)
            {
                Console.WriteLine("Null Sample was given from buffer");
                last_sample = new double[num_emgs];
            }

            //Console.WriteLine("[{0}]", string.Join(", ", last_sample));
            
            // send it off as a vector of 8 doubles
            SendOutput(Vector.Build.DenseOfArray(last_sample));

            //stats
            if(i >= i_stats)
            {
                Console.WriteLine("Total number of nullsamples: " + nullSamples);
                Console.WriteLine("Buffer length: " + SampleBuffer.Count);
                Console.WriteLine("Total jumped Samples: " + jumpedSamples);
                Console.WriteLine();

                i = 0;
            }

            i++;
        }

        /// <summary>
        /// Function that shortens the buffer if > MAX_DELAY (in samples)
        /// </summary>
        /// <param name="MAX_DELAY">This is the maximal delay which is introduced by the buffering</param>
        /// <returns></returns>
        public int ShortenBuffer(int MAX_DELAY)
        {
            lock (Lock_Buffer)
            {
                int numOfRemoval = 0;
                if (SampleBuffer.Count > MAX_DELAY)
                {
                    // how many need to be removed?
                    numOfRemoval = SampleBuffer.Count - MAX_DELAY; // in samples

                    for (int i = 0; i < numOfRemoval; i++)
                    {
                        SampleBuffer.RemoveAt(0);
                    }
                    //Console.WriteLine($"Buffer was cut short: {numOfRemoval} samples");
                }
                return numOfRemoval;
            }
        }

       
    }
}
