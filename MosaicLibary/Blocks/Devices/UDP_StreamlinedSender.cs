using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Windows.Devices.Sensors;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using MosaicLibary;

namespace MosaicLibary
{
    public struct UdpPayload
    {
        // The count of data elements (not raw bytes)
        public byte NumCount;
        // The .NET type, e.g. typeof(double) or typeof(sbyte)
        public Type ValType;
        // Category + subcategory
        public (byte, byte) DataType;
        // The actual raw bytes to send
        public byte[] Data;
    }


    /// <summary>
    /// Block for a UDP output for Unity's Streamlined Input Manager
    /// To be as generic as possible the following syntax has been implemented
    /// |0: How many numbers | 1: value type | 2: data type | 3: data subtype | 4 => 11: timestamp | 12 => -3: DATA | -2: => -1: Counter (as uInt16)|
    /// 
    /// These combinations need to be defined in json / yaml (for now in struct below)
    /// [1]: 
    /// [2,3]:
    ///       "0" EMG - "0.0" raw EMG, "0.1" ARVs ... tbd
    ///       "1" PositionControl - "1.0" ThumbFlexion, "1.1" ThumbRotation, "1.2" Index, "1.3" Middle, "1.4" Ring, "1.5" Little, "1.6" WristFlexionExtension, "1.7" WristUlnarSupination, "1.8" WirstUlnarPronation, ...
    ///       "2" "FMG" - "2.0" raw FMG
    ///       "tbd"...
    ///       "3" "PositionControl2" - hack for bimanual see above
    /// </summary>
    public class UDP_StreamlinedSender : Block
    {
        /// <summary>
        /// Udp client for the connection with e.g. Unity
        /// </summary>
        private UdpClient _UdpClient = new UdpClient();

        /// <summary>
        /// Gets set depending on the Input Block
        /// </summary>
        private SerializeFunc _serializerDelegate;


        public delegate void SerializeFunc(dynamic value);


        public UDP_StreamlinedSender(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        {

        }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            if (Params.Count < 2)
            {
                throw new Exception($"Block {Name} must have at least 2 parameters [hostIP, Port]");
            }

            
            var _sender = this.InputBlocks[0].ToString();
            switch (_sender)
            {
                case "iM.Myo":
                    _serializerDelegate = MyoSerialize;
                    break;

                case "iM.ControlAlgorithm":
                    _serializerDelegate = ControlAlgorithmSerialize;
                    break;

                default:
                    // Optionally a fallback
                    _serializerDelegate = DefaultSerialize;
                    break;
            }

            // open UDP stream client!
            _UdpClient.Connect(Params[0], Convert.ToInt32(Params[1]));
        }

        protected override void OnNewInput(Block sender, dynamic value)
        {
            // If we never assigned a delegate, do nothing or log
            if (_serializerDelegate == null)
                return;

            // 1) Convert the incoming data => UdpPayload
            
            _serializerDelegate(value);
            
        }


        // Arrays of supported data types
        Type[] floatingTypes = { typeof(float), typeof(double) };
        Type[] signedTypes = { typeof(sbyte), typeof(short), typeof(int), typeof(long) };
        Type[] unsignedTypes = { typeof(ushort), typeof(uint), typeof(ulong), typeof(byte) };

        /// <summary>
        /// Function to get the specific command for UDP Protocol to hand over info on data type
        /// </summary>
        /// <param name="type">is a data type e.g. float </param>
        /// <returns></returns>
        private byte GetByteForType(Type type)
        {
            if (!floatingTypes.Contains(type) && !signedTypes.Contains(type) && !unsignedTypes.Contains(type))
            {
                throw new Exception("This data type is not support yet by the UDP protocol: " + type.Name);
            }

            byte typeInfo = 0b_0000_0000;

            // decimal?
            if (floatingTypes.Contains(type))
            {
                typeInfo += 0b_0010_0000;
            }
            // if not check if signed or unsigned (only positive)
            else if (signedTypes.Contains(type))
            {
                typeInfo += 0b_0001_0000;
            }
            // num of bytes?
            typeInfo += (byte)Marshal.SizeOf(type);

            return typeInfo;
        }
        /// <summary>
        /// This is the general UDP counter of sent messages over all types
        /// </summary>
        UInt16 UDP_counter = 0;

        /// <summary>
        /// This function let's you send out standardized UDP messages
        /// </summary>
        /// <param name="numCount">how many numbers (not how many bytes)</param>
        /// <param name="valType">what kind of value type (double, float, int32 etc)</param>
        /// <param name="dataType">What kind of data of struct Data_Type are you sending</param>
        /// <param name="data">Actual data as byte array</param>
        private void SendUDPmessage(byte numCount, Type valType, (byte,byte) dataType, byte[] data)
        {
            // add components
            List<byte> udpMessage = new List<byte>
            {
                numCount,
                GetByteForType(valType),
                dataType.Item1,
                dataType.Item2,
            };
            udpMessage.AddRange(BitConverter.GetBytes(Utils.Now));
            udpMessage.AddRange(data);
            udpMessage.AddRange(BitConverter.GetBytes(UDP_counter));

            UDP_counter++;

            // Debug Print Message
            //byte[] array = udpMessage.ToArray();
            //Console.WriteLine(
            //    "UDP Message (dec): " + string.Join(", ", array)
            //);

            _UdpClient.SendAsync(udpMessage.ToArray(), udpMessage.Count);
        }

        #region Delegate Functions

        private void MyoSerialize(dynamic val)
        {
            // We expect 'val' to be a Vector<double> from MathNet
            var vec = val as MathNet.Numerics.LinearAlgebra.Vector<double>;
            if (vec == null)
            {
                // Return an empty or default payload if it's invalid
                return;
            }

            // Convert each double in [-1..1] to a single byte
            List<byte> dataList = new List<byte>(vec.Count);
            for (int i = 0; i < vec.Count; i++)
            {
                dataList.Add((byte)(128 * vec[i]));
            }

            var message =  new UdpPayload
            {
                NumCount = (byte)vec.Count,       // We treat each as an sbyte
                ValType = typeof(sbyte),
                DataType = ((byte)1, (byte)1),              // Category=0, SubCategory=0 for Myo
                Data = dataList.ToArray()
            };

            SendUDPmessage(message.NumCount, message.ValType, message.DataType, message.Data);
        }

        private void ControlAlgorithmSerialize(dynamic val)
        {
            // We expect a Dictionary<OldDOAs, double>
            var dict = val as Dictionary<DOAs, double>;
            if (dict == null || dict.Count == 0)
                return;

            foreach (var kv in dict)
            {
                byte[] dataBytes = BitConverter.GetBytes(kv.Value);

                // category=1, subcategory=(byte)kv.Key
                (byte, byte) dataType = (1, (byte)kv.Key);

                var message = (new UdpPayload
                {
                    NumCount = 1,
                    ValType = typeof(double),
                    DataType = dataType,
                    Data = dataBytes
                });

                SendUDPmessage(message.NumCount, message.ValType, message.DataType, message.Data);
            }
        }

        private void DefaultSerialize(dynamic val)
        {
            // Possibly handle AdditionalSenders logic here or do nothing
            // For example, if you have a Vector<double> from a block named "joiner"
            // Return default if you can't handle it
            return;
        }



        #endregion
    }

    /// <summary>
    /// Intermediate solution for defining the types of data => ToDo: Change to config file, alternatively set everything in YAML
    /// </summary>
    internal struct DataType
    {
        // Datatypes (type, subtype)

        // EMG
        public static readonly byte[] EMG_RAW = { 0, 0 };
        public static readonly byte[] EMG_ARV = { 0, 1 };
        // ...

        // CTRl (not really needed in current implementation)
        public static readonly byte[] CTRL_th_flex = { 1, (byte)DOAs.ThumbFlexion };
        public static readonly byte[] CTRL_th_rot = { 1, (byte)DOAs.ThumbRotation };
        public static readonly byte[] CTRL_index = { 1, (byte)DOAs.Index };
        public static readonly byte[] CTRL_middle = { 1, (byte)DOAs.Middle };
        public static readonly byte[] CTRL_ring = { 1, (byte)DOAs.Ring };
        public static readonly byte[] CTRL_little = { 1, (byte)DOAs.Little };
        public static readonly byte[] CTRL_wr_flex_ext = { 1, (byte)DOAs.WristFlexionExtension };
        public static readonly byte[] CTRL_wr_uln_rad = { 1, (byte)DOAs.WristUlnarRadialDevation };
        public static readonly byte[] CTRL_wr_sup_pron = { 1, (byte)DOAs.WristPronationSupination };
        public static readonly byte[] CTRL_elbow_flex = { 1, (byte)DOAs.ElbowFlexion };
        public static readonly byte[] CTRL_elbow_ext = { 1, (byte)DOAs.ElbowExtension };
        // ...

        // FMG
        public static readonly byte[] FMG_RAW = { 2, 0 };
        public static readonly byte[] FMG_ARV = { 2, 1 };
        // ...

        // e.g. BodyRig {3, Joint)

    }

}
