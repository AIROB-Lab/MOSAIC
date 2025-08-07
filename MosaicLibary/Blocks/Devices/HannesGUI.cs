using System;
using System.Collections.Generic;
using System.Net.Sockets;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// The HannesGUI class controls the virtual Hannes prosthetic arm (HannesGUI by IIT) by sending activation values over a UDP port. 
    /// It can also send raw EMG values and ARVs (average rectified values) for logging. 
    /// The data is formatted into distinct byte arrays for each type of information:
    /// <list type="bullet">
    ///   <item><description>(7 bytes) Activation values: <c>%</c>, <c>15</c>, <c>#s</c>, <c>#a</c>, <c>&amp;</c>, <c>\r</c>, <c>\n</c></description></item>
    ///   <item><description>(N+5 bytes) Raw EMG values: <c>%</c>, <c>16</c>, <c>raw EMG bytes</c>, <c>&amp;</c>, <c>\r</c>, <c>\n</c></description></item>
    ///   <item><description>(M+5 bytes) ARVs (8-byte doubles each): <c>%</c>, <c>32</c>, <c>ARV bytes</c>, <c>&amp;</c>, <c>\r</c>, <c>\n</c></description></item>
    /// </list>
    /// </summary>
    /// <example>
    /// <code>
    /// myHannesArm:
    /// {
    ///   Type: HannesArm,
    ///   Inputs: [ myPredictions, myRawEMG, myARVs ],
    ///   Params: [ "192.168.0.10", 8889, 8, 64 ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: HannesArm</c> indicates this block interfaces with the Hannes prosthesis.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ myPredictions, myRawEMG, myARVs ]</c> define the 
    ///     blocks that provide control signals, raw EMGs, and ARVs, respectively.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Params: [ "192.168.0.10", 8889, 8, 64 ]</c> are the host IP, the UDP port,
    ///       the number of raw EMG bytes, and the number of ARV bytes. These determine the byte array sizes 
    ///       for sending data to the Hannes device.</description>
    ///   </item>
    /// </list>
    /// </example>
    public class HannesGUI : Block
    {
        /// <summary>
        /// The UDP client used to transmit data to the prosthetic arm.
        /// </summary>
        private UdpClient _udpClient = new UdpClient();

        /// <summary>
        /// The block that provides the control signals (activation values).
        /// </summary>
        private Block _controlProvider;

        /// <summary>
        /// The block that provides raw EMG signals.
        /// </summary>
        private Block _rawEMGProvider;

        /// <summary>
        /// The block that provides ARV (average rectified value) signals.
        /// </summary>
        private Block _arvProvider;

        /// <summary>
        /// The byte array used for sending activation values to the prosthetic arm.
        /// </summary>
        private byte[] _actuationAsByteArray = new byte[7];

        /// <summary>
        /// Raw EMG values are serialized into this byte array before transmission.
        /// Its length is <c>num_rawEMGs</c> plus a small header/trailer.
        /// </summary>
        private byte[] _rawEMGValues;

        /// <summary>
        /// ARVs (as doubles) are serialized into this byte array before transmission.
        /// Its length is <c>num_ARVs</c> times 8 bytes, plus a small header/trailer.
        /// </summary>
        private byte[] _arvs;

        /// <summary>
        /// An offset to account for header, footer, and other fixed overhead bytes in the arrays.
        /// </summary>
        private int _numAddBytes = 5;

        private Dictionary<DOAs, double> _actuationDict;


        /// <summary>
        /// Initializes a new instance of the <see cref="HannesGUI"/> class, 
        /// providing a connection to the Hannes prosthetic device via UDP.
        /// </summary>
        /// <param name="Name">The name of this block instance.</param>
        /// <param name="DesiredRate">The rate of operation, set to zero for event-driven updates.</param>
        /// <param name="InputCfg">The names of the input blocks (control, raw EMG, ARVs).</param>
        /// <param name="Params">[host IP, UDP port, num_rawEMGs, num_ARVs].</param>
        /// <param name="Path">Any resource path if needed (unused here).</param> 
        public HannesGUI(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) 
        {
        }

        /// <summary>
        /// Configures the HannesGUI inputs and sets up the UDP connection, byte arrays, and block references.
        /// </summary>
        /// <exception cref="Exception">Thrown if the number of input blocks or parameters is incorrect.</exception>   
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            if (InputBlocks.Count != 3) 
                throw new Exception($"Arm {Name} must have three input blocks.");
            
            DesiredRate = 0;
            _controlProvider = InputBlocks[0];
            _rawEMGProvider = InputBlocks[1]; 
            _arvProvider = InputBlocks[2];

            // Params: host, port, num_rawEMGs, num_ARVs => if not specified => not streamed
            if (Params.Count != 4)
            {
                throw new Exception($"Hannes {Name} must have 4 parameters [hostIP, Port, No. Bytes EMG, No. Bytes ARVs]");
            }
            // define the size of arrays
            int num_EMGs = int.Parse(Params[2]);
            int num_ARVs = int.Parse(Params[3]);

            // set size of arrays
            _rawEMGValues = new byte[num_EMGs + _numAddBytes];
            _arvs = new byte[num_ARVs + _numAddBytes];

            // configure byte arrays to be sent off: the raw EMG values,
            _rawEMGValues[0] = Convert.ToByte('%'); _rawEMGValues[1] = 16;
            _rawEMGValues[_rawEMGValues.Length - 3] = Convert.ToByte('&'); 
            _rawEMGValues[_rawEMGValues.Length - 2] = Convert.ToByte('\r'); 
            _rawEMGValues[_rawEMGValues.Length-1] = Convert.ToByte('\n'); // 1 myo

            // configure byte arrays to be sent off: the ARV values
            _arvs[0] = Convert.ToByte('%'); _arvs[1] = 32;
            _arvs[_arvs.Length - 3] = Convert.ToByte('&'); 
            _arvs[_arvs.Length-2] = Convert.ToByte('\r'); 
            _arvs[_arvs.Length - 1] = Convert.ToByte('\n');


            // configure byte arrays to be sent off: the activation prediction,
            _actuationAsByteArray[0] = Convert.ToByte('%');
            _actuationAsByteArray[1] = 15;
            _actuationAsByteArray[4] = Convert.ToByte('&'); 
            _actuationAsByteArray[5] = Convert.ToByte('\r'); 
            _actuationAsByteArray[6] = Convert.ToByte('\n');

            // open UDP stream client!
            _udpClient.Connect(Params[0], Convert.ToInt32(Params[1]));
        }


        /// <summary>
        /// Handles new input from any of the three sources (control signals, raw EMG, ARVs) 
        /// and sends the corresponding data out via UDP to the Hannes device.
        /// </summary>
        /// <param name="sender">The block sending input.</param>
        /// <param name="value">The data, which may be a dictionary of DOF activations or a <see cref="Vector"/> of EMGs/ARVs.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            if (sender == _controlProvider)
            {
                _actuationDict = value as Dictionary<DOAs, double>;

                const byte _HandId = 0, _WristPSId = 1, _WristFEId = 2;
                _actuationAsByteArray[2] = _HandId; 
                _actuationAsByteArray[3] = (byte)_actuationDict[DOAs.ThumbFlexion]; 
                _udpClient.SendAsync(_actuationAsByteArray, _actuationAsByteArray.Length);
                _actuationAsByteArray[2] = _WristPSId;
                _actuationAsByteArray[3] = (byte)_actuationDict[DOAs.WristPronationSupination]; 
                _udpClient.SendAsync(_actuationAsByteArray, _actuationAsByteArray.Length);
                _actuationAsByteArray[2] = _WristFEId; 
                _actuationAsByteArray[3] = (byte)_actuationDict[DOAs.WristFlexionExtension]; 
                _udpClient.SendAsync(_actuationAsByteArray, _actuationAsByteArray.Length);
            }
            else if (sender == _rawEMGProvider)
            {
                // pack and send the 8 raw values (as bytes)
                Vector RawEMGValues = value as Vector;
                for (int i = 0; i < RawEMGValues.Count; i++) _rawEMGValues[i + 2] = (byte)(128 * RawEMGValues[i]);
                _udpClient.SendAsync(_rawEMGValues, _rawEMGValues.Length);
            }
            else if (sender == _arvProvider)
            {
                // pack and send the 8 ARVs (as doubles!)
                Vector ARVs = value as Vector;
                for (int i = 0; i < ARVs.Count; i++)
                {
                    byte[] doubleToByte = BitConverter.GetBytes(ARVs[i]);
                    Array.Copy(doubleToByte, 0, _arvs, 2 + i * 8, 8);
                }
                _udpClient.SendAsync(_arvs, _arvs.Length);
            }
        }
    }
}
