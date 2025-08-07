using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{

    #region ControlPanel
    /// <summary>
    /// Control Panel to handle and visualize the communication with the Hannes hand.
    /// </summary>
    public partial class cpHannes : ControlPanel
    {
        private HannesHand _sourceBlock;

        /// <summary>
        /// Setup the Hannes Block GUI.
        /// </summary>
        /// <param name="sourceBlock">Hannes source block.</param>
        public cpHannes(HannesHand sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = sourceBlock;
            ConnectHannesDevice.Enabled = false;
            disconnectHannesDevice.Enabled = true;
            ScanDevices.Enabled = false;

            COMPortUpDown.Value = Convert.ToInt32((sourceBlock.PortName.Split('M')).Last());

        }

        /// <summary>
        /// Method to refresh the scope monitors showing the reference values for each joint.
        /// </summary>
        /// <param name="o">Object</param>
        /// <param name="e">Events</param>
        protected override void cpRefresh(object o, EventArgs e)
        {
            Vector hand_oc_vec = Vector.Build.DenseOfArray(new double[] { _sourceBlock.Data[0] });
            smHandOC.Update(hand_oc_vec);

            Vector wrist_fe_vec = Vector.Build.DenseOfArray(new double[] { _sourceBlock.Data[1] });
            smWristFE.Update(wrist_fe_vec);

            Vector wrist_ps_vec = Vector.Build.DenseOfArray(new double[] { _sourceBlock.Data[2] });
            smWristPS.Update(wrist_ps_vec);

            Vector thumb_rot_vec = Vector.Build.DenseOfArray(new double[] { _sourceBlock.Data[3] });
            smThumb.Update(thumb_rot_vec);

        }

        private void COMPortUpDown_Changed(object sender, EventArgs e)
        {
            _sourceBlock.PortName = "COM" + COMPortUpDown.Value.ToString();
        }

        private void dongleConnect_Click(object sender, EventArgs e)
        {
            _sourceBlock.ConnectRehabDongle();
            dongleConnect.Enabled = false;
            dongleDisconnect.Enabled = true;
            ScanDevices.Enabled = true;
        }

        private void dongleDisconnect_Click(object sender, EventArgs e)
        {
            dongleConnect.Enabled = true;
            dongleDisconnect.Enabled = false;
        }
        private void scanDevices_Click(object sender, EventArgs e)
        {
            List<string> found_devices = _sourceBlock.ScanHannesDevices();

            if (found_devices.Count != 0)
            {
                // TODO: add check in case the device was already added 
                foreach (string device in found_devices) { DeviceDropDown.Items.Add(device); }
            }
        }

        private void connectHannesDevice_Click(object sender, EventArgs e)
        {
            if (DeviceDropDown.Text != "")
            {
                _sourceBlock.ConnectHannesDevice(DeviceDropDown.Text);
                disconnectHannesDevice.Enabled = true;
                ConnectHannesDevice.Enabled = false;
            }
            else
            {
                Console.WriteLine("No device has been selected");
            }
        }

        private void DropDownHannesDevice_Changed(object sender, EventArgs e)
        {

            ConnectHannesDevice.Enabled = true;
        }

        private void disconnectHannesDevice_Click(object sender, EventArgs e)
        {
            _sourceBlock.DisconnectHannesDevice();

            disconnectHannesDevice.Enabled = false;
            ConnectHannesDevice.Enabled = true;
        }


        private void checkBoxWristFE_CheckedChanged(object sender, EventArgs e)
        {
            _sourceBlock.SwitchActivationDOF(HannesHand.RefTable.REF_WRIST_FE);
        }

        private void checkBoxWristPS_CheckedChanged(object sender, EventArgs e)
        {
            _sourceBlock.SwitchActivationDOF(HannesHand.RefTable.REF_WRIST_PS);
        }

        private void checkBoxHandOC_CheckedChanged(object sender, EventArgs e)
        {
            _sourceBlock.SwitchActivationDOF(HannesHand.RefTable.REF_HAND);
        }

        private void checkBoxThumbRot_CheckedChanged(object sender, EventArgs e)
        {
            _sourceBlock.SwitchActivationDOF(HannesHand.RefTable.REF_THUMB);
        }
    }

    #endregion


    /// <summary>
    /// Communicates with the physical Hannes Prosthetic Hand via serial communication over Bluetooth. 
    /// This Block aims to implement the Communication Protocol specified in "SW_HannesBTCommProtocol.docx" file to send joint reference commands. 
    /// </summary>
    /// <remarks>
    /// <para><strong>How to use it:</strong></para>
    /// <list type="number">
    ///   <item>
    ///     Connect the Rehab Dongle to the PC. (Optionally set the COM port to <c>COM50</c> in the 
    ///     Windows Device Manager, or specify a different COM port in the block’s <c>Params</c>.)
    ///   </item>
    ///   <item>Turn on the Hannes hand.</item>
    ///   <item>Open the COM port (e.g., <c>COM50</c>).</item>
    ///   <item>Scan for Hannes devices.</item>
    ///   <item>
    ///     Select the device (if found) from the drop-down menu and connect to it. 
    ///     If the ping procedure is successful, the Hannes hand is ready to receive commands.
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// myHannesHand: 
    /// {
    ///   Type: HannesHand,
    ///   Inputs: [ myControlBlock ],
    ///   Params: [ "COM50" ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: HannesHand</c> identifies this block for controlling 
    ///     the Hannes prosthetic hand over serial/Bluetooth.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ myControlBlock ]</c> indicates a single input block 
    ///     that provides a <c>Dictionary&lt;OldDOAs, double&gt;</c> of DOF (degree-of-freedom) activation values.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Params: [ "COM50" ]</c> sets the COM port name 
    ///     (in this case, <c>COM50</c>) for communication with the Rehab Dongle. This parameter is optional, default is COM50.</description>
    ///   </item>
    /// </list>
    /// 
    /// In this configuration, the HannesHand class uses serial commands to transmit the DOF values 
    /// coming from <c>myControlBlock</c> directly to the Hannes prosthetic hand.
    /// </example>
    public class HannesHand : Block

    #region Fields
    {
        /// <summary>
        /// Vector with the current reference value of the 4 controllable joints. The
        /// </summary>
        public Vector Data = Vector.Build.DenseOfArray(new double[4]);

        // List of Serial messages
        private static byte startByte = 0x25;        //%
        private static byte endByte = 0x26;          //&
        private static byte LFByte = 0x0A;           //LF
        private static byte CRByte = 0x0D;           //CR
        private static byte[] tailByte = { endByte, LFByte, CRByte };
        private static string LFString = "\n";       //LF
        private static string CRString = "\r";       //CR
        private static byte all_hand_joints_bitmask = 0x27; // Bitmask for the combination that activate the 4 joints that are usable. 
        // ACK (acknowledge) and NACK (NOT acknowledged) reply from EMGEM device that tell if a command was succesfully implemented or not
        private static string ack_reply = (char)startByte + Convert.ToString((char)CommandParsing.ACK) + (char)endByte + CRString;
        private static string nack_reply = (char)startByte + Convert.ToString((char)CommandParsing.NACK) + (char)endByte + CRString;
        //
        // List of BT command
        private static string CommandMode = "$$$";
        private static string ScanDevice = "scan";
        private static string Disconnect = "dct";
        private static string Connect = "con ";
        private static string ConfPinCon = "gfu 0 str_active_n\r";
        private static string SconfPinCon = "gfu 0 none\r";
        private static string successReply = "Success\r";
        private static string failReply = "Command failed\r";
        //
        // COM Port config
        /// <summary>
        /// COM Port name where the REhab Dongle is connected.
        /// </summary>
        public string PortName = "COM50";
        private static SerialPort serialPort = new SerialPort();
        private int BaudRate = 115200;
        private int ReadTimeout = 100;
        private int WriteTimeout = 200;
        //
        // Hannes Devices
        private static List<string> nameDevices = new List<string>();
        private static List<int> conDevices = new List<int>();
        private static List<int> rssiDevices = new List<int>();
        private static List<string> addrDevices = new List<string>();
        //
        // Device scanning process
        private static int max_scans = 3;
        private static int countHannesDevices = 0;
        //
        // Device connection process
        private static bool connected_hannes_device = false;
        private static bool streaming = false;

        /// <summary>
        /// List of Commands and Parsing. The commands are organized in the way that their Index is the correct command
        /// header. Explanations of the commands are in the Communication Protocol Documentation.
        /// 
        /// Not all of the commands are used for now.
        /// </summary>
        public enum CommandParsing
        {
            EnterCMDMode,
            ExitCMDMode,
            ACK,
            NACK,
            EraseEEPROM,
            ReadEEPROM,
            WriteEEPROM,
            ReplyEEPROM,
            EraseFLASH,
            ReadFLASH,
            WriteFLASH,
            ReplyFLASH,
            EnterClassification,
            EnterTraining,
            RefControl,
            ReplyRef,
            ReplyEMG,
            ReplyQuaternions,
            ReplySkin,
            ReplyMeasurements,
            ReplyGestures,
            CommandControl,
            ReadEEPROMboard,
            WriteEEPROMboard,
            ReplyEEPROMboard,
            VibroControl,
            ReplyVibro,
            ReadCounter,
            ReplyCounter,
            ReplyGravity,
            SimultRefControl,
            ReplyFingermap,
            ReplyEMGFiltered,
            btProtocol_END
        }

        /// <summary>
        /// Contains the indices of the use options of the RefControl Command.
        /// </summary>
        public enum RefTable
        {
            /// <summary>
            /// To set the joints to be activated.
            /// </summary>
            REF_JOINT_SET,
            /// <summary>
            /// Option to set the value of the Hand Open/Close DOF.
            /// </summary>
            REF_HAND,
            /// <summary>
            /// Option to set the value of the Wrist Pronation/Supination DOF.
            /// </summary>
            REF_WRIST_PS,
            /// <summary>
            /// Option to set the value of the Wrist Flexion/Extension DOF.
            /// </summary>
            REF_WRIST_FE,
            /// <summary>
            /// Option to set the value of the Elbow DOF (Does not apply for now).
            /// </summary>
            REF_ELBOW,
            /// <summary>
            /// Option to set the control mode (see ControlModality).
            /// </summary>
            REF_CONTROL_MODE,
            /// <summary>
            /// Option to set the value of the Thumb Rotation DOF.
            /// </summary>
            REF_THUMB,
            /// <summary>
            /// Option to set the 3D Digit (Does not apply for now).
            /// </summary>
            REF_3DDIGIT
        }

        /// <summary>
        /// Commands for the different Control Modes to give to RefControl -> REF_CONTROL_MODE
        /// </summary>
        public enum ControlModality
        {
            EMG_CONTROL,
            HMI_CONTROL,
            EMC_TEST_CONTROL,
            /// <summary>
            /// Unity App Control. SO far, this is the only mode in which the commands for RefControl 
            /// described in the Communication Protocol work. 
            /// </summary>
            UNITY_CONTROL
        }

        /// <summary>
        ///  Possible modes of the EMGEM device. Certain commands are only possible in one of them. To "setup the device", e.g. 
        ///  set mode or activate joints, the Command mode has to be ON. For streaming joint position/speed commands the Stream
        ///  Mode has to be activated.
        /// </summary>
        public enum EMGMDeviceMode
        {
            EMGM_COMMAND_MODE,
            EMGM_STREAM_MODE
        }

        /// <summary>
        /// Bitmask for the joints activation. Each bit corresponds to a joint (see JointsActivationIndex) and when used in RefControl -> REF_JOINT_SET
        /// the bit array is converted to a hex-byte
        /// </summary>
        public BitArray JointsBitmask = new BitArray(8);

        /// <summary>
        /// Indices of the joints in the activation bitmask (given to RefControl -> REF_JOINT_SET). 
        /// </summary>
        public enum JointsActivationIndex
        {
            JOINT_RSVD,
            JOINT_3DIGIT,
            JOINT_THUMB,
            JOINT_SHOULDER,
            JOINT_ELBOW,
            JOINT_WRIST_FE,
            JOINT_WRIST_PS,
            JOINT_HAND
        }

        /// <summary>
        /// Current EMGEM device mode. 
        /// </summary>
        public EMGMDeviceMode current_EMGM_mode = EMGMDeviceMode.EMGM_STREAM_MODE;
        #endregion

        #region Constructors 
        /// <summary>
        /// The standard Block constructor, instantiates a Hannes object.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="DesiredRate"></param>
        /// <param name="InputCfg"></param>
        /// <param name="Params"></param>
        /// <param name="Path"></param>
        public HannesHand(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Initial configuration of the object: set desired rate, setup the serial port, init the control panel.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            // DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;

            // Setup the serial Port specs
            serialPort.BaudRate = BaudRate;
            serialPort.ReadTimeout = ReadTimeout;
            serialPort.WriteTimeout = WriteTimeout;

            // If there is a COM Port given as param
            if (Params != null) { PortName = Params[0]; };

            cp = new cpHannes(this);

        }

        #endregion

        #region Methods

        #region Communication with Rehab Dongle

        /// <summary>
        /// Opens specified COM Port to the Rehab Dongle.
        /// </summary>
        private void OpenCOMPort()
        {
            if (PortName != null)
            {
                serialPort.PortName = PortName;
                try { serialPort.Open(); Console.WriteLine($"Opened COM Port {PortName}"); }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        /// <summary>
        /// Connects with the Rehab Dongle by opnening the specified COM Port and setting the device in Command mode.
        /// </summary>
        public void ConnectRehabDongle()
        {

            OpenCOMPort();
            Thread.Sleep(500);
            CommandModeRehabDongle();
            Thread.Sleep(500);
        }

        /// <summary>
        /// Send command to set Rehab Dongle in command mode. This is necessary to scan and connect to Hannes devices
        /// and later communicate with them. 
        /// </summary>
        private void CommandModeRehabDongle()
        {
            if (serialPort.IsOpen)
            {
                try { serialPort.Write(CommandMode); }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }


        /// <summary>
        /// Disconnect from Rehab Dongle i.e. closes the COM Port. 
        /// </summary>
        public void DisconnectRehabDongle()
        {
            Console.WriteLine($"Closing {PortName}");
            serialPort.Close();
        }

        /// <summary>
        /// Write commands to the Rehab Dongle and its own command interface (Dongle Command Mode). 
        /// </summary>
        /// <param name="command"></param>
        public void WriteCommandToRehabDongle(string command)
        {
            serialPort.WriteLine(command);
        }

        /// <summary>
        /// Reads the serial port buffer. 
        /// </summary>
        /// <returns></returns>
        public static string[] ReadAllRehabDongle()
        {
            List<string> data = new List<string>();
            while (true)
            {
                try
                {
                    data.Add(serialPort.ReadLine());
                }
                catch (TimeoutException)
                {
                    return data.ToArray();
                }
            }
        }

        /// <summary>
        /// Scans Hannes devices nearby.
        /// </summary>
        /// <returns></returns>
        public List<string> ScanHannesDevices()
        {
            // TODO: fix, why does this not work when the app is restarted and the dongle was not disconnected and connected ?

            // Init storage for scan command replies
            string[] devices_info = new string[] { };

            // Flags to limit the amount of attempts, usually 2 should be enough
            bool try_to_scan = true;
            int scan_attempt = 1;

            while ((try_to_scan) && (scan_attempt <= max_scans))
            {
                // Make sure older meesages and commands are discarded
                serialPort.ReadExisting();
                // Scan Hannes devices and wait for answer
                WriteCommandToRehabDongle(ScanDevice + CRString);
                Thread.Sleep(1500);
                //string[] scan_result = 
                devices_info = ReadAllRehabDongle(); //devices_info.Concat(scan_result).ToArray();

                if (devices_info.Contains("Success\r")) { try_to_scan = false; }
                scan_attempt++;
            }

            if (try_to_scan) { Console.WriteLine("Scanning unsuccessful. Check connection with Dongle. Eventually try to Disconnect and Connect. "); }


            int device_idx = 0;
            // Get the device info for each found device
            foreach (string line in devices_info)
            {
                //Console.WriteLine(line);
                int device_con, device_rssi;
                string device_address, device_name;
                string[] device_strings = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Check that it is a line from the table, but not the header of the table
                if ((device_strings.Length >= 5) && (!device_strings.Contains("RSSI")))
                {
                    try
                    {
                        if (device_strings[1].Contains("#"))
                        {

                            device_con = int.Parse(device_strings[2]);
                            device_rssi = int.Parse(device_strings[3]);
                            device_address = device_strings[4];
                            device_name = device_strings[5];
                            //countHannesDevices++;
                        }
                        else
                        {
                            device_con = int.Parse(device_strings[1]);
                            device_rssi = int.Parse(device_strings[2]);
                            device_address = device_strings[3];
                            device_name = device_strings[4];
                            //countHannesDevices++;
                        }

                        if (!addrDevices.Contains(device_address))
                        {
                            conDevices.Add(device_con);
                            rssiDevices.Add(device_rssi);
                            addrDevices.Add(device_address);
                            nameDevices.Add(device_name);

                            Console.WriteLine($"Found device {device_name}");
                            countHannesDevices++;
                        }

                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }


            }
            if (countHannesDevices == 0) { Console.WriteLine("No devices were found during scan. Make sure the Hannes hand is ON."); }
            return (nameDevices);
        }

        /// <summary>
        /// Connects to a Hannes device given the name of the device. It automatically tries to set it to Command Mode 
        /// and checks the correct parsing of the command (ping procedure).
        /// </summary>
        /// <param name="device_name"></param>
        public void ConnectHannesDevice(string device_name)
        {
            int device_list_idx = nameDevices.IndexOf(device_name);
            string device_con = Convert.ToString(conDevices[device_list_idx]);
            try
            {
                // Send connecting command and wait for the replies (usually 3: "con 1", "Success" and "STREAM_MODE")
                Console.WriteLine($"Attempting to connect to {device_name}");
                WriteCommandToRehabDongle(Connect + device_con + CRString);
                Thread.Sleep(500);
                connected_hannes_device = true;

                // Ping procedure and verify it is actually connected
                connected_hannes_device = pingCommandModeHannes();
                if (connected_hannes_device)
                {
                    Console.WriteLine($"Correctly connected {device_name}");
                    current_EMGM_mode = EMGMDeviceMode.EMGM_COMMAND_MODE;
                    // Setup to activate all joints and correct control mode
                    configureHannesForRefControl();
                }
                else { Console.WriteLine($" Could not connect {device_name}"); }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        /// <summary>
        /// Disconnect from Hannes device. For safety it first sets the Hand to a neutral position. 
        /// </summary>
        public void DisconnectHannesDevice()
        {
            if (connected_hannes_device)
            {
                streaming = false; // To avoid that OnNewInput continues to send commands to the Hand

                // Short pause to avoid eventual abrupt position change
                Thread.Sleep(500);

                // Set hand in "Neutral position" before disconnecting. Important especially to avoid that the 
                // pronation/supination wrist joint keeps rotating
                Dictionary<DOAs, double> actuationDict = new Dictionary<DOAs, double> {
                    {DOAs.Middle, 0 },
                    {DOAs.WristFlexionExtension, 80},
                    {DOAs.ThumbRotation, 50 },
                    {DOAs.WristPronationSupination, 50 }

                };
                sendControlValues(actuationDict);

                try
                {
                    string[] reply_strings = ReadAllRehabDongle();

                    // Not necessarily required, but will enhance the possiblity that the success/failed reply is caught 
                    // sine the 1Hz-ACK-MESSAGES will be stopped: 
                    if (current_EMGM_mode == EMGMDeviceMode.EMGM_STREAM_MODE) { SwitchEMGMDeviceMode(); }



                    // Send the command mode command of the REHAB DONGLE (not the EMGEM HANNES device)
                    CommandModeRehabDongle();
                    Thread.Sleep(2000);

                    // Send the command to disconnect
                    WriteCommandToRehabDongle(Disconnect + CRString);
                    Thread.Sleep(2000);

                    // Check it was successfull
                    reply_strings = ReadAllRehabDongle();

                    // Check if answer tell about succcess of commands (unfortunately the wait is sometimes not long enough)
                    if (reply_strings.Contains(successReply)) { Console.WriteLine("Successfully disconnected from Hannes device"); }
                    else if (reply_strings.Contains(failReply)) { Console.WriteLine("Disconnecting failed"); }


                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
        #endregion
        #region Communication with Hannes device

        /// <summary>
        /// Write command packets to the Hannes EMGEM device according ot the specified format, which includes
        /// a Start Byte (% =  0x25), a Header Byte, the Payload, and the End Sequence (&\r\n = [0x26 0x0D 0x0A]). 
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        static private void WriteCommandPacketToHannesEMGEM(byte[] message)
        {
            if (serialPort.IsOpen && connected_hannes_device)
            {
                List<byte> msg = new List<byte>();
                msg.Insert(0, startByte);
                foreach (byte msg_i in message)
                {
                    msg.Add(msg_i);
                }
                msg.Add(endByte);
                msg.Add(CRByte);
                msg.Add(LFByte);
                message = msg.ToArray();

                serialPort.Write(message, 0, message.Length);
            }
        }


        /// <summary>
        /// Tries to set Hannes device to command mode and checks if the reply is an ACK message, that confirms 
        /// the command was correcly parse and executed.
        /// </summary>
        /// <returns></returns>
        private bool pingCommandModeHannes()
        {
            string[] reply_strings = ReadAllRehabDongle();
            // Send command to set Hannes EMGEM device to CMD mode
            WriteCommandPacketToHannesEMGEM(new byte[] { (byte)CommandParsing.EnterCMDMode });
            Thread.Sleep(1000);

            // Check if answer is ACK, which means a command was correctly parsed
            reply_strings = ReadAllRehabDongle();
            //foreach (string reply_string in reply_strings) { Console.WriteLine(reply_string); }

            foreach (string reply_string in reply_strings)
            {
                if (reply_string == ack_reply) { return true; }
                // Give also feedback if a message was sent but not properly or it was not properly parsed. 
                else if (reply_string == nack_reply) { Console.WriteLine("NACK"); }
            }
            return false;
        }

        /// <summary>
        /// Method to switch the EMGEM device between Command and Stream Mode.
        /// </summary>
        private void SwitchEMGMDeviceMode()
        {
            if (current_EMGM_mode == EMGMDeviceMode.EMGM_STREAM_MODE)
            {
                WriteCommandPacketToHannesEMGEM(new byte[] { (byte)CommandParsing.EnterCMDMode });
                current_EMGM_mode = EMGMDeviceMode.EMGM_COMMAND_MODE;
                Console.WriteLine("Hannes in Command Mode");

            }
            else if (current_EMGM_mode == EMGMDeviceMode.EMGM_COMMAND_MODE)
            {
                WriteCommandPacketToHannesEMGEM(new byte[] { (byte)CommandParsing.ExitCMDMode });
                current_EMGM_mode = EMGMDeviceMode.EMGM_STREAM_MODE;
                Console.WriteLine("Hannes in Stream Mode");
            }
        }

        /// <summary>
        /// Fills the bit array with the default joints activation combination. 
        /// (Activate the four relevamt joints).
        /// </summary>
        private void SetDefaultJointmask()
        {
            JointsBitmask[(int)JointsActivationIndex.JOINT_RSVD] = false;
            JointsBitmask[(int)JointsActivationIndex.JOINT_3DIGIT] = false;
            JointsBitmask[(int)JointsActivationIndex.JOINT_THUMB] = true;
            JointsBitmask[(int)JointsActivationIndex.JOINT_SHOULDER] = false;
            JointsBitmask[(int)JointsActivationIndex.JOINT_ELBOW] = false;
            JointsBitmask[(int)JointsActivationIndex.JOINT_WRIST_FE] = true;
            JointsBitmask[(int)JointsActivationIndex.JOINT_WRIST_PS] = true;
        }

        /// <summary>
        /// Sends the commands to the EMGEM Hannes device required to be able to control it with 
        /// the standard communication protocol. This includes to acivate relevant joints and put 
        /// the device in UNITY_MODE control, as well as to let it in STREAM_MODE.
        /// </summary>
        private void configureHannesForRefControl()
        {
            // if not yet, set the device to command mode
            if (current_EMGM_mode == EMGMDeviceMode.EMGM_STREAM_MODE) { SwitchEMGMDeviceMode(); }

            // Activate Wrist (P/S & (F/E)), Hand and Thumb joints
            SetDefaultJointmask();
            byte[] activate_all_hand_joints_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_JOINT_SET, BitArrayToByte(JointsBitmask) };
            WriteCommandPacketToHannesEMGEM(activate_all_hand_joints_cmd);

            // Set the control mode to Unity GUI Mode (seems to be the one it respons to)
            byte[] activate_unity_gui_control_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_CONTROL_MODE, (byte)ControlModality.UNITY_CONTROL };
            WriteCommandPacketToHannesEMGEM(activate_unity_gui_control_cmd);

            // Set in streaming mode to be able to control with RefControl
            SwitchEMGMDeviceMode();

            Thread.Sleep(1000);

            streaming = true;

            Console.WriteLine("Ready to receive control commands.");


        }

        /// <summary>
        /// Sends to the EMGEM device the reference values for all joints simultaneously.
        /// </summary>
        /// <param name="actuationDict"></param>
        private void sendControlValues(Dictionary<DOAs, double> actuationDict)
        {
            //// Send fingers ref
            //byte[] hand_ref_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_HAND, (byte)_actuationDict[OldDOAs.Middle]};
            //WriteCommandPacketToHannesEMGEM(hand_ref_cmd);
            ////Console.WriteLine(serialPort.ReadLine());
            //// Send wrist flexion extension ref
            //byte[] wrist_fe_ref_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_WRIST_FE, (byte)_actuationDict[OldDOAs.WristFlexionExtension] };
            //WriteCommandPacketToHannesEMGEM(wrist_fe_ref_cmd);
            //// Send wrist pronation supination ref 
            //byte[] wrist_ps_ref_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_WRIST_PS, (byte)rescaled_ps_value };
            //WriteCommandPacketToHannesEMGEM(wrist_ps_ref_cmd);
            //// Send thumb flexion extension ref
            //byte[] thumb_ref_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_THUMB, (byte)_actuationDict[OldDOAs.ThumbRotation] };
            //WriteCommandPacketToHannesEMGEM(thumb_ref_cmd);

            // Since these DOFs go from -100 to 100, the value has to be re-scaled from the 0 to 100 scale
            double rescaled_ps_value = -(actuationDict[DOAs.WristPronationSupination] - 50) * 2; // The control algorithm is capped at 0 -100
            double rescaled_thumb_value = - (actuationDict[DOAs.ThumbRotation]  - 50) * 2;

            byte[] simultaneous_ref_cmd = new byte[] { (byte)CommandParsing.SimultRefControl,  //B1:Command for simultaneous ref control
                                                       0x00,                                   //B2: has to be 0x00 according to documentation 
                                                       (byte)actuationDict[DOAs.Middle],       //B3: Ref Hand
                                                       (byte)rescaled_ps_value,                //B4: Ref Wrist P/S 
                                                       (byte)actuationDict[DOAs.WristFlexionExtension],  //B5: Ref Wrist F/E
                                                       0x00,                                   //B6: Ref Elbow, for now 0 since no elbow
                                                       0x03,                                   //B7: has to be 0x00 according to documentation
                                                       (byte)rescaled_thumb_value,             //B8: Ref Thumb
                                                       0x00,                                   //B9: Ref 3D Digit, fow now 0 since not implemented
                                                       0x00,                                   //B10:Ref Shoulder, for now 0 since no shoulder
                                                       0x00 };                                 //B11:has to be 0x00 according to documentation

            WriteCommandPacketToHannesEMGEM(simultaneous_ref_cmd);

            // For visualization 
            Data =  Vector.Build.DenseOfArray(new double[] { actuationDict[DOAs.Middle], actuationDict[DOAs.WristFlexionExtension], rescaled_ps_value, rescaled_thumb_value });

        }

        /// <summary>
        /// Activate or activate the given joint. Currently it does not work.  
        /// </summary>
        /// <param name="joint"></param>
        public void SwitchActivationDOF(RefTable joint)
        {
            Console.WriteLine("Changing joints activation configuration. ");
            streaming = false;
            // if not yet, set the device to command mode
            if (current_EMGM_mode != EMGMDeviceMode.EMGM_COMMAND_MODE) { SwitchEMGMDeviceMode(); }

            // change the joints activation
            JointsBitmask[(int)joint] = !JointsBitmask[(int)joint];
            byte[] joint_activation_cmd = new byte[] { (byte)CommandParsing.RefControl, (byte)RefTable.REF_JOINT_SET, BitArrayToByte(JointsBitmask) };
            WriteCommandPacketToHannesEMGEM(joint_activation_cmd);

            // TODO: check the command was processed succesfully

            // go back to streaming
            SwitchEMGMDeviceMode();
            streaming = true;
        }

        /// <summary>
        /// Helper method to convert an 8-bit array into a byte. Needed for the joints 
        /// activation method.
        /// </summary>
        /// <param name="bitarray"></param>
        /// <returns></returns>
        private byte BitArrayToByte(BitArray bitarray)
        {
            byte[] byteValue = new byte[1];
            bitarray.CopyTo(byteValue, 0);
            return byteValue[0];
        }

        #endregion

        /// <summary>
        /// If streaming (previous successfull connection and config of a Hannes device), 
        /// calls the method to send reference control commands 
        /// to the Hannes device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        override protected void OnNewInput(Block sender, object value)
        {
            if (streaming)
            {
                sendControlValues(value as Dictionary<DOAs, double>);
            }
        }
        #endregion
    }
}
