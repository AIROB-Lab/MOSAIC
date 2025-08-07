/// Fabio Andre Egle, Chair of Medical Robotics, AIBE FAU
/// Simple BLE Connection example inspired by: https://github.com/cornelhuman/QuickBluetoothLE


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

using System.Linq;

namespace MosaicLibary
{
    /// <summary>
    /// Manages Bluetooth Low Energy (BLE) operations, including device scanning, connection, and data streaming.
    /// Allows for BLE device discovery, connection setup, command sending, notification subscription, and data sample collection.
    /// Use this Block to connecte to Bluethooth Devices lie the Myo.
    /// </summary>
    public class BluetoothLEManager
    {

        #region static properties

        /// <summary>
        /// Serves as a repository for discovered BLE devices, identified by their MAC addresses.
        /// Thread safety is ensured through synchronization.
        /// </summary>
        private static Dictionary<string, DeviceInformation> _foundDevices = new Dictionary<string, DeviceInformation>(); // needs lock X

        /// <summary>
        /// Object used for locking to synchronize access to the _foundDevices dictionary, ensuring thread safety.
        /// </summary>
        private static object _lockDevices = new object();

        /// <summary>
        /// Represents the BLE device watcher used for scanning and discovering devices.
        /// </summary>
        private static DeviceWatcher _deviceWatcher = null; // needs lock X

        /// <summary>
        /// Object used for locking to synchronize access to the _deviceWatcher, ensuring thread safety during BLE scanning operations.
        /// </summary>>
        private static object _lockWatcher = new object();

        /// <summary>
        /// Tracks the general states of BLE operations (e.g., whether devices have been found or if scanning is in progress)
        /// to manage UI button states. Access is synchronized for thread safety.
        /// </summary>
        private static Dictionary<GENERAL_BLE_STATES, bool> _generalBLEstates = new Dictionary<GENERAL_BLE_STATES, bool>() // needs lock X
        {
            {GENERAL_BLE_STATES.FOUND_DEVICES, false},
            {GENERAL_BLE_STATES.SCANNING, false},
        };
        private static object _lockGeneralStates = new object();

        /// <summary>
        /// Temporary queue to hold BluetoothLEManager instances, illustrating a bridge between static and instance contexts.
        /// Consider reviewing for a more integrated approach between static and non-static members. 
        /// </summary>
        private static Queue<BluetoothLEManager> _bleManagerQ = new Queue<BluetoothLEManager>();
        #endregion

        #region public properties

        /// <summary>
        /// Buffer for storing streamed original byte samples from the connected BLE device.
        /// </summary>
        public List<byte[]> SampleBuffer = new List<byte[]>(); // needs lock X
        /// <summary>
        /// Lock object for synchronizing access to SampleBuffer.
        /// </summary>
        private static object Lock_Buffer = new object();

        #endregion

        #region private properties
        private static Dictionary<SPECIFIC_BLE_STATES, bool> specificBLEstates = new Dictionary<SPECIFIC_BLE_STATES, bool>() // needs lock X
        {
            {SPECIFIC_BLE_STATES.CONNECTED, false},
            {SPECIFIC_BLE_STATES.STREAMING, false},
        };
        private static object Lock_specificStates = new object();

        private Dictionary<string, (string, string)> InitialNotifications;
        private Dictionary<string, (string, string, byte[])> InitialCmds, EndCommands;

        private DeviceInformation wantedDevice = null;
        private BluetoothLEDevice bleDevice = null;

        private Queue<GattCharacteristic> NotificationCharacteristics;
        private Queue<(GattCharacteristic, byte[])> InitialCmdCharacteristics;
        private Queue<(GattCharacteristic, byte[])> EndCmdCharacteristics;

        private string WANTED_DEVICE_MAC;
        private int SAMPLE_LENGTH;

        // characteristics from a service can only be obtained once => so dump them here
        private Queue<GattCharacteristic> ObtainedCs = new Queue<GattCharacteristic>();

        // Needed as helper to disconnect => all services which were obtained will be disposed for disconnection
        private Queue<GattDeviceService> ObtainedServices = new Queue<GattDeviceService>();

        #endregion

        /// <summary>
        /// Event for signaling UI updates, such as changes in BLE device discovery status. 
        /// This can be subscribed to by UI components or devices (e.g., Myo Device) to refresh the UI accordingly.
        /// </summary>
        public static event EventHandler UiUpdateEvent;


        #region constructor
        /// <summary>
        /// Constructs new BLE Manager => One per device!
        /// </summary>
        /// <param name="macAddress">MAC-address of wanted device either with ":" or "-" separation</param>
        /// <param name="sampleLength">Streamed sample length in BYTE(!) => ATTENTION: is higher depending on data type</param>
        /// <param name="initialNotifications">Notifications Characteristics, STRUCTURE: string arbitrary name, string service, string characteristic</param>
        /// <param name="initialCmds">Command Characteristics, STRUCTURE: string arbitrary name, string service, string characteristic, byteArray cmd</param>
        /// <param name="endCommands">Command Characteristics, STRUCTURE: string arbitrary name, string service, string characteristic, byteArray cmd</param>
        public BluetoothLEManager(string macAddress, int sampleLength,
            Dictionary<string, (string, string)> initialNotifications,
            Dictionary<string, (string, string, byte[])> initialCmds,
            Dictionary<string, (string, string, byte[])> endCommands)
        {
            //WANTED_DEVICE_NAME = deviceName;
            SAMPLE_LENGTH = sampleLength;
            InitialNotifications = initialNotifications;
            InitialCmds = initialCmds;
            EndCommands = endCommands;

            // mac address either with "-" or ":" => bleWatcher finds with ":" between numbers
            if (macAddress.Contains("-"))
            {
                macAddress = macAddress.Replace('-', ':');
            }

            WANTED_DEVICE_MAC = macAddress;

            //ONLY TEMPORARY SEE ABOVE:
            _bleManagerQ.Enqueue(this);

        }

        #endregion

        #region private BLE events

        /// <summary>
        /// Handles value changes in a GATT characteristic.
        /// </summary>
        /// <param name="sender">The GATT characteristic that triggered the event.</param>
        /// <param name="args">The event arguments containing the characteristic value.</param>
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var characteristicVal = args.CharacteristicValue;
            var reader = DataReader.FromBuffer(characteristicVal);

            // get number of values
            var count = reader.UnconsumedBufferLength;
            int numberSamples = (int)(count / SAMPLE_LENGTH);

            if (count % SAMPLE_LENGTH != 0)
            {
                throw new Exception($"your sample length {SAMPLE_LENGTH} does not match the streamed data {count} times a factor");
            }

            // read all bytes from buffer
            byte[] samplesBytes = new byte[count];
            reader.ReadBytes(samplesBytes);

            // separate into samples (always two for myo) AND add to List/Buffer
            for (int i = 0; i < numberSamples; i++)
            {
                byte[] newSample = new byte[SAMPLE_LENGTH];
                Array.Copy(samplesBytes, i * SAMPLE_LENGTH, newSample, 0, SAMPLE_LENGTH);

                lock (Lock_Buffer)
                {
                    SampleBuffer.Add(newSample);
                }
            }
        }

        /// <summary>
        /// Automatically triggered when a BLE device is found.
        /// </summary>
        /// <param name="sender">The device watcher.</param>
        /// <param name="args">The device information.</param> 
        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            lock (_lockGeneralStates)
            {
                // for UI
                if (!_generalBLEstates[GENERAL_BLE_STATES.FOUND_DEVICES])
                {
                    _generalBLEstates[GENERAL_BLE_STATES.FOUND_DEVICES] = true;
                    OnUiUpdateEvent(EventArgs.Empty);
                }
            }

            var deviceID = args.Id;
            // device id contains mac address
            string mac = GetMACfromDeviceID(deviceID);
            var name = args.Name;
            bool nameChanged = false;

            lock (_lockDevices)
            {
                //device already there?
                if (_foundDevices.ContainsKey(mac))
                    // at least new name?
                    if (_foundDevices[mac].Name == name) return; // else old one will be overwritten

                _foundDevices[mac] = args;
                if (name == "") name = "[no name]";
                if (nameChanged) Console.WriteLine($"BLE: {mac} changed name to: {name}");
                else Console.WriteLine($"BLE: New device: {mac} with name {name}");


                // ONLY TEMPORARY
                foreach (var ble in _bleManagerQ)
                {
                    ble.CheckDeviceMatching(mac, args);
                }
            }
        }

        /// <summary>
        /// Retrieves the MAC address from a device ID.
        /// </summary>
        /// <param name="DeviceID">The device ID.</param>
        /// <returns>The MAC address.</returns>
        private static string GetMACfromDeviceID(string DeviceID)
        {
            // need to find "-" as after is MAC address
            int idx = DeviceID.IndexOf('-');
            string mac = DeviceID.Substring(idx + 1);

            return mac;
        }

        /// <summary>
        /// Checks if the discovered device matches the desired device.
        /// </summary>
        /// <param name="mac">The MAC address of the device.</param>
        /// <param name="args">The device information.</param>
        private void CheckDeviceMatching(string mac, DeviceInformation args)
        {
            // is the correct one found
            if (mac.ToLower().Contains(WANTED_DEVICE_MAC.ToLower()))
            {
                wantedDevice = args;

                lock (Lock_specificStates)
                {
                    specificBLEstates[SPECIFIC_BLE_STATES.FOUND] = true;
                }
                OnUiUpdateEvent(EventArgs.Empty);

                Console.WriteLine("BLE: wantedDevice was found!");
                Console.WriteLine($"USER: press connect-button to connect with {wantedDevice.Name}");
            }
        }

        /// <summary>
        /// Handles the completion of BLE device enumeration.
        /// </summary>
        /// <param name="sender">The device watcher.</param>
        /// <param name="args">The event arguments.</param>
        private static void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            Console.WriteLine("Enumeration of BLE devices complete");
        }

        /// <summary>
        /// Handles the removal of a BLE device.
        /// </summary>
        /// <param name="sender">The device watcher.</param>
        /// <param name="args">The device information update.</param>
        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            lock (_lockDevices)
            {
                // get mac address to compare to dict
                string mac = GetMACfromDeviceID(args.Id);
                _foundDevices.Remove(mac);
            }
        }

        /// <summary>
        /// Handles updates to a BLE device's information.
        /// </summary>
        /// <param name="sender">The device watcher.</param>
        /// <param name="args">The device information update.</param>
        private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            lock (_lockDevices)
            {
                // get mac address to compare to dict
                string mac = GetMACfromDeviceID(args.Id);
                //device already there?
                if (_foundDevices.ContainsKey(mac))
                {
                    _foundDevices[mac].Update(args);
                }
            }
        }

        /// <summary>
        /// Handles the stopping of the device watcher.
        /// </summary>
        /// <param name="sender">The device watcher.</param>
        /// <param name="args">The event arguments.</param>
        private static void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            lock (_lockGeneralStates)
            {
                _generalBLEstates[GENERAL_BLE_STATES.SCANNING] = false;
                OnUiUpdateEvent(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles changes in the connection status of a BLE device.
        /// </summary>
        /// <param name="sender">The BLE device.</param>
        /// <param name="args">The event arguments.</param>
        private void DeviceConnectionChanged(BluetoothLEDevice sender, object args)
        {
            // update connection status
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                lock (Lock_specificStates)
                {
                    specificBLEstates[SPECIFIC_BLE_STATES.CONNECTED] = true;
                }
            }
            else if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                lock (Lock_specificStates)
                {
                    specificBLEstates[SPECIFIC_BLE_STATES.CONNECTED] = false;
                }
            }

            OnUiUpdateEvent(EventArgs.Empty);
        }

        #endregion

        #region BLE functions

        /// <summary>
        /// Connection Routine for BLE Device (but kept general)
        /// </summary>
        public async void ConnectionRoutineAsync()
        {
            if (wantedDevice == null)
            {
                Console.WriteLine($"BLE: Device {WANTED_DEVICE_MAC} was not yet found");
                return;
            }
            Console.WriteLine($"BLE: Attempting to pair with device {wantedDevice.Name}...");
            // Connect to device
            bleDevice = await GetBLEdevice(wantedDevice);

            // subscribe to change in connection status
            bleDevice.ConnectionStatusChanged += DeviceConnectionChanged;

            // for the GattCharacteristics
            NotificationCharacteristics = new Queue<GattCharacteristic>();
            InitialCmdCharacteristics = new Queue<(GattCharacteristic, byte[])>();
            EndCmdCharacteristics = new Queue<(GattCharacteristic, byte[])>();

            // Find Notifications => Change the finding to function at some point
            foreach (KeyValuePair<string, (string service, string characteristic)> notification in InitialNotifications)
            {
                GattCharacteristic newCharacteristic = null;
                try
                {
                    newCharacteristic = await GetCharacteristic(bleDevice, notification.Value.characteristic, notification.Value.service);
                }
                catch (Exception)
                {
                    HandleException();
                    return;
                }

                //handle null
                if (HandleNull(newCharacteristic))
                    return;

                NotificationCharacteristics.Enqueue(newCharacteristic);
            }

            // Find Commands
            foreach (KeyValuePair<string, (string service, string characteristic, byte[] cmd)> cmd_item in InitialCmds)
            {
                GattCharacteristic newCharacteristic = null;

                try
                {
                    newCharacteristic = await GetCharacteristic(bleDevice, cmd_item.Value.characteristic, cmd_item.Value.service);
                }
                catch (Exception)
                {
                    HandleException();
                }

                //handle null
                if (HandleNull(newCharacteristic)) return;

                var newCmdCharacteristic = (newCharacteristic, cmd_item.Value.cmd);
                InitialCmdCharacteristics.Enqueue(newCmdCharacteristic);
            }
            foreach (KeyValuePair<string, (string service, string characteristic, byte[] cmd)> cmd_item in EndCommands)
            {
                GattCharacteristic newCharacteristic = null;

                try
                {
                    newCharacteristic = await GetCharacteristic(bleDevice, cmd_item.Value.characteristic, cmd_item.Value.service);
                }
                catch (Exception)
                {
                    HandleException();
                }

                //handle null
                if (HandleNull(newCharacteristic)) return;

                var newCmdCharacteristic = (newCharacteristic, cmd_item.Value.cmd);
                EndCmdCharacteristics.Enqueue(newCmdCharacteristic);
            }

            // Activate Notifications and write commands
            if (NotificationCharacteristics.Count != 0 && InitialCmdCharacteristics.Count != 0)
            {
                foreach (var not_item in NotificationCharacteristics)
                    await StartNotification(not_item);

                foreach ((GattCharacteristic, byte[]) cmd_item in InitialCmdCharacteristics)
                    await SendCommand(cmd_item.Item1, cmd_item.Item2);

                lock (Lock_specificStates)
                {
                    specificBLEstates[SPECIFIC_BLE_STATES.STREAMING] = true;
                }
                OnUiUpdateEvent(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Stops Notifications. Disconnection from current bleDevice. 
        /// </summary>
        public async void DisconnectAsync()
        {
            if (bleDevice != null)
            {
                // Send stop cmds
                foreach (var cmd_item in EndCmdCharacteristics)
                {
                    await SendCommand(cmd_item.Item1, cmd_item.Item2);
                }

                lock (Lock_specificStates)
                {
                    specificBLEstates[SPECIFIC_BLE_STATES.STREAMING] = false;
                }
                OnUiUpdateEvent(EventArgs.Empty);

                // Stop Notifications
                if (NotificationCharacteristics.Count != 0)
                {
                    foreach (var not_item in NotificationCharacteristics)
                    {
                        if (not_item != null)
                        {
                            GattCommunicationStatus notify_status = await not_item.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                            if (notify_status == GattCommunicationStatus.Success)
                            {
                                not_item.ValueChanged -= Characteristic_ValueChanged;

                                // Server has been informed of clients interest.
                                Console.WriteLine($"Notification {not_item.Uuid} stopped");
                            }

                        }
                    }
                }
                else Console.WriteLine("Notification deactivation failed");

                // Disconnect
                bleDevice.Dispose();

                // Set BLE device to null and dispose of all services as well
                // From (https://stackoverflow.com/questions/39599252/windows-ble-uwp-disconnect) => it disconnects completely
                bleDevice = null;

                foreach (var service in ObtainedServices)
                {
                    service.Dispose();
                }

                // Reset Q etc..
                ObtainedServices = new Queue<GattDeviceService>();
                ObtainedCs = new Queue<GattCharacteristic>();

                Console.WriteLine($"BLE: Device {wantedDevice.Name} was disconnected");
            }
            else
            {
                Console.WriteLine($"BLE: Device {WANTED_DEVICE_MAC} was not connected in the first place");
            }
        }

        /// <summary>
        /// Async function to start new Device Watcher => Starts Scanning for BLE
        /// </summary>
        public static void StartNewDeviceWatcher()
        {

            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
            lock (_lockGeneralStates)
            {
                // for UI
                _generalBLEstates[GENERAL_BLE_STATES.SCANNING] = true;
                OnUiUpdateEvent(EventArgs.Empty);
            }

            lock (_lockWatcher)
            {
                // if first time => Else Device watcher already there
                if (_deviceWatcher == null)
                {
                    _deviceWatcher =
                             DeviceInformation.CreateWatcher(
                                     BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                     requestedProperties,
                                     DeviceInformationKind.AssociationEndpoint);

                    // Register event handlers before starting the watcher.
                    // Added, Updated and Removed are required to get all nearby devices
                    _deviceWatcher.Added += DeviceWatcher_Added;
                    _deviceWatcher.Updated += DeviceWatcher_Updated;
                    _deviceWatcher.Removed += DeviceWatcher_Removed;

                    // EnumerationCompleted and Stopped are optional to implement.
                    _deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
                    _deviceWatcher.Stopped += DeviceWatcher_Stopped;

                    // Start the watcher.
                    _deviceWatcher.Start();

                }
                if (_deviceWatcher.Status == DeviceWatcherStatus.Stopped)
                    _deviceWatcher.Start();
            }
        }

        /// <summary>
        /// Stops Scanning
        /// </summary>
        public static void StopDeviceWatcher()
        {
            lock (_lockWatcher)
            {
                _deviceWatcher.Stop();
            }
        }

        private static async Task<BluetoothLEDevice> GetBLEdevice(DeviceInformation bleDeviceInfo)
        {
            BluetoothLEDevice _bleDevice = await BluetoothLEDevice.FromIdAsync(bleDeviceInfo.Id);
            return _bleDevice;
        }

        private async Task<GattCharacteristic> GetCharacteristic(BluetoothLEDevice bleDevice, string wantedCharacteristicUUID, string ServiceUUID)
        {
            //check if already in obtained characteristics:
            foreach (var characteristic in ObtainedCs)
            {
                if (characteristic.Uuid.ToString() == wantedCharacteristicUUID)
                {
                    Console.WriteLine("BLE: characteristic" + wantedCharacteristicUUID + "was already obtained earlier");
                    return characteristic;
                }
            }

            GattDeviceService gattDeviceService = await GetService(bleDevice, ServiceUUID);
            if (gattDeviceService == null)
                return null;

            GattCharacteristicsResult charactiristicResult = await gattDeviceService.GetCharacteristicsAsync();

            if (charactiristicResult.Status == GattCommunicationStatus.Success)
            {
                var characteristics = charactiristicResult.Characteristics;

                // IMPORTANT: add to Queue for future use, as every characteristic can only be obtained once => afterwards access denied!
                foreach (var characteristic in characteristics)
                {
                    ObtainedCs.Enqueue(characteristic);
                }

                foreach (var characteristic in characteristics)
                {


                    Console.WriteLine("---------------");
                    Console.WriteLine(characteristic.Uuid.ToString());
                    GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                    if (characteristic.Uuid.ToString().Equals(wantedCharacteristicUUID))
                    {
                        Console.WriteLine("BLE: characteristic" + wantedCharacteristicUUID + "was returned from BLE device");
                        return characteristic;
                    }


                }
            }
            Console.WriteLine("BLE: characteristic" + wantedCharacteristicUUID + "could not be returned from BLE device");
            return null;
        }

        private async Task<GattDeviceService> GetService(BluetoothLEDevice bleDevice, string wantedServiceUUID)
        {
            GattDeviceServicesResult result = await bleDevice.GetGattServicesAsync();

            if (result.Status == GattCommunicationStatus.Success)
            {
                //Console.WriteLine($"BLE: Getting services succeeded");
                var services = result.Services;

                foreach (var service in services)
                {
                    // IMPORTANT: add to Queue for future use, as Services needed to be disposed to disconnect => Not sure if only the services with used characteristics count...
                    ObtainedServices.Enqueue(service);

                    var serviceId = service.Uuid.ToString().ToLower();
                    Console.WriteLine("service: " + serviceId);

                    //get notify characteristic
                    if (serviceId.Equals(wantedServiceUUID.ToLower()))
                    {
                        Console.WriteLine($"BLE: service {wantedServiceUUID} was returned from BLE device");
                        return service;
                    }
                }
            }

            Console.WriteLine($"BLE: Getting service {wantedServiceUUID} => No success");
            return null;
        }

        private async Task<bool> StartNotification(GattCharacteristic not_item)
        {
            Console.WriteLine($"BLE: Activate notification {not_item.Uuid}...");
            GattCommunicationStatus notify_status = await not_item.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (notify_status == GattCommunicationStatus.Success)
            {
                // Server has been informed of clients interest.
                not_item.ValueChanged += Characteristic_ValueChanged;
                Console.WriteLine($"BLE: Notification {not_item.Uuid} activated");
                return true;
            }
            Console.WriteLine("Notification {gattCharacteristic.Uuid} activation => No success");
            return false;
        }

        private async Task<bool> SendCommand(GattCharacteristic gattCharacteristic, byte[] cmd)
        {
            var writer = new DataWriter();

            writer.WriteBytes(cmd);

            GattCommunicationStatus cmd_status = await gattCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (cmd_status == GattCommunicationStatus.Success)
            {
                // Successfully wrote to device
                Console.WriteLine($"BLE: Command {string.Join(" ", cmd)} successfully written to {gattCharacteristic.Uuid}");
                return true;
            }

            Console.WriteLine($"BLE: Command {string.Join(" ", cmd)} to {gattCharacteristic.Uuid} => No success");
            return false;
        }

        #endregion

        #region public functions
        /// <summary>
        /// Evaluates current device buffer. If it is empty it returns null
        /// </summary>
        /// <returns>original streamed byte or null if empty</returns>
        public byte[] DequeueBuffer()
        {
            lock (Lock_Buffer)
            {
                if (SampleBuffer.Count > 0)
                {
                    byte[] new_byte_sample = SampleBuffer[0];

                    // remove sample from buffer
                    SampleBuffer.RemoveAt(0);

                    return new_byte_sample;
                }
                else return null;
            }
        }

        /// <summary>
        /// If for whatever reason Sample Buffer gets bigger => Delay gets bigger, is cut to maximum delay (in samples)
        /// </summary>
        /// <param name="MAX_DELAY">Max Delay in Samples</param>
        /// <returns>returns the number of jumped samples</returns>
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

                    Console.WriteLine($"Buffer was cut short: {numOfRemoval} samples");
                }
                return numOfRemoval;
            }
        }


        /// <summary>
        /// getter function for device name, in case it is null. It returns the device mac address as backup.
        /// </summary>
        /// <returns>Device name or mac address as backup</returns>
        public string GetDeviceNameElseMac()
        {
            if (wantedDevice != null && wantedDevice.Name != null) return wantedDevice.Name;
            return WANTED_DEVICE_MAC;
        }

        /// <summary>
        /// Returns a copy of the Gerneral BLE States Instance
        /// </summary>
        /// <returns></returns>
        public static Dictionary<GENERAL_BLE_STATES, bool> GetGeneralBLECopy()
        {
            return _generalBLEstates.ToDictionary(entry => entry.Key,
                                          entry => entry.Value);
        }

        /// <summary>
        /// Returns a copy of the current _foundDevices Dictionary
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, DeviceInformation> GetDevicesCopy()
        {
            lock (_lockDevices) return _foundDevices.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// Checks if the Device Watcher is NULL
        /// </summary>
        /// <returns>Returns TRUE if watcher is NULL</returns>
        public static bool CheckWatcherNull()
        {
            lock (_lockWatcher)
                if (_deviceWatcher == null) return true;
                else return false;
        }

        /// <summary>
        /// Gets the current watcher status.
        /// </summary>
        /// <returns>Returns device watcher status (Started, Stopped, etc).</returns>
        public static DeviceWatcherStatus GetWatcherStatus()
        {
            lock (_lockWatcher)
            {
                return _deviceWatcher.Status;
            }
        }

        /// <summary>
        /// Gets device specific BLE state information.
        /// </summary>
        /// <returns>Copy of device specific BLE states dictionary.</returns>
        public Dictionary<SPECIFIC_BLE_STATES, bool> GetSpecificBLECopy()
        {
            lock (Lock_specificStates)
                return specificBLEstates.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// Gets the name and MAC address of the device.
        /// </summary>
        /// <returns>A tuple containing the device name and MAC address.</returns>
        public Tuple<string, string> GetNameAndMac()
        {
            if (wantedDevice != null) return new Tuple<string, string>(wantedDevice.Name, GetMACfromDeviceID(wantedDevice.Id));
            else return new Tuple<string, string>(null, null);
        }

        /// <summary>
        /// Set Wanted Device from outside => for Dropdown list
        /// </summary>
        /// <param name="wantedDevice">he desired device information.</param>
        public void SetWantedDevice(DeviceInformation wantedDevice)
        {
            this.wantedDevice = wantedDevice;
            CheckDeviceMatching(GetMACfromDeviceID(wantedDevice.Id), wantedDevice);
        }
        #endregion

        #region Helper Funcs

        /// <summary>
        /// Handles null objects.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the object is null; otherwise false.</returns>
        private bool HandleNull(object obj)
        {
            if (obj != null) return false;

            HandleException();

            return true;
        }

        /// <summary>
        /// Handles BLE communication exceptions.
        /// </summary>
        private async void HandleException()
        {
            Console.WriteLine("BLE: There was an error in BLE communication");

            DisconnectAsync();

            Console.WriteLine("Disconnecting and Starting again...");

            //StartNewDeviceWatcher();
        }

        /// <summary>
        /// Fire event UI needs update
        /// </summary>
        /// <param name="e"></param>
        static protected void OnUiUpdateEvent(EventArgs e)
        {
            UiUpdateEvent?.Invoke(new object(), e);
        }

        #endregion
    }

    /// <summary>
    /// Enum for general BLE states => for UI purpose
    /// </summary>
    public enum GENERAL_BLE_STATES
    {
        FOUND_DEVICES,
        SCANNING,
    }
    /// <summary>
    /// Enum for device specific BLE states => for UI purpose
    /// </summary>
    public enum SPECIFIC_BLE_STATES
    {
        FOUND,
        CONNECTED,
        STREAMING
    }
}