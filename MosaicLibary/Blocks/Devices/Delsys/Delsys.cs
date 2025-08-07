using Aero.PipeLine;
using DelsysAPI.Components.Simulated;
using DelsysAPI.Components.TrignoRf;
using DelsysAPI.DelsysDevices;
using DelsysAPI.Events;
using DelsysAPI.Exceptions;
using DelsysAPI.Pipelines;
using DelsysAPI.Utils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;



namespace MosaicLibary
{

    #region ControlPanel
    /// <summary>
    /// <see cref="ControlPanel"/> class for the <see cref="Delsys"/> block.
    /// Allows the user to scan for sensors, configure, start/stop streaming, and monitor Delsys Trigno system status.
    /// </summary>
    public partial class CpDelsys : ControlPanel
    {
        private Delsys _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CpDelsys"/> control panel.
        /// </summary>
        /// <param name="sourceBlock">The <see cref="Delsys"/> block associated with this control panel.</param>
        public CpDelsys(Delsys sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
            button1.Enabled = true;
            btnStream.Enabled = false;
        }

        /// <summary>
        /// Updates the control panel display with live stream status and device information.
        /// </summary>
        override protected void cpRefresh(object myObject, EventArgs e)
        {
            smDelsys.Update(_sourceBlock.Data as Vector);
            deviceName.Text = _sourceBlock.StreamInfo.DeviceName;
            pipelineStatus.Text = _sourceBlock.StreamInfo.PipelineStatus;
            sensorsConnected.Text = _sourceBlock.StreamInfo.SensorsConnected.ToString();
            totalChannels.Text = _sourceBlock.StreamInfo.TotalChannels.ToString();
            streamTime.Text = _sourceBlock.StreamInfo.StreamTime;
            packetsLost.Text = _sourceBlock.StreamInfo.PacketsLost.ToString();
            framesCollected.Text = _sourceBlock.StreamInfo.FramesCollected.ToString();
        }

        private void CpDelsys_Load(object sender, EventArgs e)
        {
            //_sourceBlock.InitalizeDataScouce();
        }

        private void buttonArmPipeline_Click(object sender, EventArgs e)
        {
            if (sensorList.Controls.Count == 0)
            {
                Console.WriteLine("Please scan for sensors first.");
                return;
            }

            int selectCount = 0;
            int? commonSampleModeIndex = null;
            bool multipleSampleModes = false;

            foreach (SensorItem sensorItem in sensorList.Controls)
            {
                if (sensorItem.IsSelected)
                {
                    selectCount++;

                    // Check if the sensor has a sample mode set
                    if (sensorItem.ModeIndex >= 0)
                    {
                        if (commonSampleModeIndex == null)
                        {
                            // Set the first selected sample mode as the common sample mode
                            commonSampleModeIndex = sensorItem.ModeIndex;
                        }
                        else if (commonSampleModeIndex != sensorItem.ModeIndex)
                        {
                            // More than one sample mode found
                            multipleSampleModes = true;
                        }
                    }
                }
            }

            if (selectCount <= 0)
            {
                Console.WriteLine("No Sensors were selected.");
                return;
            }

            // Apply sample modes
            foreach (SensorItem sensorItem in sensorList.Controls)
            {
                if (sensorItem.IsSelected)
                {
                    foreach (var comp in _sourceBlock.Pipeline.TrignoRfManager.Components)
                    {
                        _sourceBlock._emgSampleSize = comp.TrignoChannels[0].SamplesPerFrame;
                        _sourceBlock._sampleRate = comp.TrignoChannels[0].SampleRate;

                        // If multiple sample modes exist, use each sensor's mode, otherwise apply the common mode
                        int modeToApply = (multipleSampleModes || sensorItem.ModeIndex >= 0) ?
                                            sensorItem.ModeIndex :
                                            commonSampleModeIndex.Value;

                        comp.SelectSampleMode(comp.Configuration.SampleModes[modeToApply]);
                        _sourceBlock.Pipeline.TrignoRfManager.SelectComponentAsync(comp).Wait();
                    }
                }
            }

            _sourceBlock.ConfigurePipeline();
            btnStream.Enabled = true;
        }


        private void buttonScanSensors_Click(object sender, EventArgs e)
        {
            SensorPopulation();
            button1.Enabled = true;
        }

        private async Task SensorPopulation()
        {
            await _sourceBlock.Scan();
            sensorList.Controls.Clear();

            foreach (var sensor in _sourceBlock.Pipeline.TrignoRfManager.Components)
            {
                SensorItem sensorItem = new SensorItem(sensor);
                sensorList.Controls.Add(sensorItem);
            }
        }

        private void btnStream_CheckedChanged(object sender, EventArgs e)
        {
            if (btnStream.Checked)
            {
                buttonScanSensors.Enabled = false;
                Console.WriteLine("Start streaming...");
                _sourceBlock.Pipeline.Start();

                btnStream.Text = "Stop";
                button1.Enabled = false;
                buttonScanSensors.Enabled = false;

            }
            else
            {
                Console.WriteLine("Stop streaming...");
                _sourceBlock.StopStreamAsync().Wait();
                Thread.Sleep(100);
                btnStream.Text = "Stream";
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            buttonReset.Enabled = false;
            _sourceBlock.ResetPipeline().Wait();
            buttonReset.Enabled = true;
            buttonScanSensors.Enabled = true;
            btnStream.Enabled = false;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            _sourceBlock.ExportData();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(!sensorList.HasChildren)
            {
                checkBox1.Checked = false;
                return;
            }

            
            foreach(SensorItem sensor in sensorList.Controls)
            {
                if (checkBox1.Checked)
                    sensor.sensorTypeCheckBox.Checked = true;
                else
                    sensor.sensorTypeCheckBox.Checked = false;
            }
        }
    }
    #endregion


    #region Block

    /// <summary>
    /// Provides an interface for acquiring data from Delsys Trigno hardware using the Delsys SPI.
    /// <para>
    /// <b>Disclaimer:</b> This block requires a valid Delsys API license and key for use with Delsys hardware.
    /// You must provide your unique key (<see cref="_key"/>) and license (<see cref="_license"/>) in the source code before using any Delsys device functionality.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// Example YAML configuration for a Delsys block:
    /// </para>
    /// <code>
    /// delsysBlock:
    /// {
    ///   Type: Delsys,
    ///   Inputs: [myTimer]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: Delsys</c> specifies that this block will stream EMG data from a Delsys Trigno system.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [myTimer]</c> specifies a timer block that triggers data acquisition.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </example>
    /// <remarks>
    /// The block will initialize and manage a Delsys Trigno base, allow scanning and selection of sensors,
    /// and provide live EMG data streaming as output vectors.
    /// <para>
    /// <b>Important:</b> You must enter your Delsys SPI key and license in the fields <c>_key</c> and <c>_license</c> in the source code.
    /// </para>
    /// </remarks>
    public class Delsys : Block
    {
        /// <summary>
        /// Your Delsys API license key (must be provided).
        /// </summary>
        private string _key = "your key goes here";
        
        /// <summary>
        /// Your Delsys API license string (must be provided).
        /// </summary>
        private string _license = "your licens goes here";

        /// <summary>
        /// Reference to the Delsys Base Station device.
        /// </summary>
        private IDelsysDevice _deviceSource;

        // Holds collection data
        private List<List<double>> _data;

        // Metadata fields
        private int _totalFrames;
        private int _totalLostPackets;
        private int _frameThroughput;
        private double _packetInterval;
        private double _streamTime = 0.0;

        private bool _isStreaming = false;

        bool _streaming = false;
        List<List<double>> sensorData; // to be exported
        Vector vector;
        double interpolationFactor; // DesiredRate / SampleRate
        Vector[] buffer = { Vector.Build.Dense(0, 0) };
        int bufferSize = -1;
        int bufferCounter = 0;

        int NTotalChannels = 0;

        private string[] _selectedSensorModes; // getter: GetSelectedSensorModes
        public int _emgSampleSize; // this is to track the sample size of the EMG channel which should be the same for all sensors
        public double _sampleRate; // this is to track the sample rate of the sensor
        private double _desiredRate; // this is to track the desired rate of the block

        private int _lowestFrequency = 3000; // Hz

        /// <summary>
        /// Reference to the active Delsys pipeline.
        /// </summary>
        public Pipeline Pipeline;

        /// <summary>
        /// List of currently selected Trigno sensors.
        /// </summary>
        public List<SensorTrignoRf> SelectedSensors;

        /// <summary>
        /// Provides live stream information for the control panel.
        /// </summary>
        public StreamInfo StreamInfo;

        /// <summary>
        /// Dictionary mapping sensor IDs to data.
        /// </summary>
        public Dictionary<string, List<double>> SensorIDMapping = new Dictionary<string, List<double>>();
        
        /// <summary>
        /// Dictionary mapping sensor IDs to channel indices.
        /// </summary>
        public Dictionary<string, int> SensorIDMapping2 = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Delsys"/> block.
        /// </summary>
        /// <param name="Name">Block name.</param>
        /// <param name="DesiredRate">Desired output rate (Hz).</param>
        /// <param name="InputCfg">Input configuration (should be one Timer).</param>
        /// <param name="Params">Unused.</param>
        /// <param name="Path">Path to dump raw Delsys data.</param>
        public Delsys(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
           : base(Name, DesiredRate, InputCfg, Params, Path)
        {
        }

        /// <summary>
        /// Configures the block input and initializes the Delsys system.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();
            if (InputBlocks.Count != 1)
                throw new Exception($"{Name} must have one input block only.");
            if (!(InputBlocks[0] is Timer))
                throw new Exception($"{Name}'s input must be a Timer.");

            // a CpDelsys's DesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;
            //_desiredRate = DesiredRate;


            // Here we configure the input sources for this Delsys instance.
            StreamInfo = new StreamInfo();
            InitalizeDataScouce();
            LoadDataSource();
            

            this.cp = new CpDelsys(this);
        }

        /// <summary>
        /// Called when new input data is received (from timer). Outputs latest data vector if streaming.
        /// </summary>
        /// <param name="sender">Input block.</param>
        /// <param name="value">Not used.</param>
        protected override void OnNewInput(Block sender, object value)
        {
            if (_isStreaming)
            {
                SendOutput(buffer[bufferCounter++]);
                if (bufferCounter == bufferSize)
                    bufferCounter = 0;
            }
            else
                SendOutput(Vector.Build.Dense(0, 0));
        }

        /// <summary>
        /// Initializes the data source for the Delsys instance.
        /// </summary>
        public void InitalizeDataScouce()
        {
            // The API uses a factory method to create the data source of your application.
            // This creates the factory method, which will then give the data source for your platform.
            // In this case, the platform is RF.
            var deviceSourceCreator = new DeviceSourcePortable(_key, _license);
            deviceSourceCreator.SetDebugOutputStream((str, args) => Trace.WriteLine(string.Format(str, args)));

            // Here is where we tell the factory method what type of data source we want to receive,
            // which we then set a reference to for future use.
            _deviceSource = deviceSourceCreator.GetDataSource(SourceType.TRIGNO_RF);

            // Here we use the key and license we previously loaded.
            _deviceSource.Key = _key;
            _deviceSource.License = _license;
        }

        /// <summary>
        /// Sets up Connection to the Tirgno RF base, as well as the Pipeline and Collection events.
        /// </summary>
        private void LoadDataSource()
        {
            // Attempts to load device
            try
            {
                // Create a Pipeline based on the datasource.
                Console.WriteLine("Creating pipeline...");
                PipelineController.Instance.AddPipeline(_deviceSource);
                Console.WriteLine("Pipeline created. Connection to Trignio Base established.");
                StreamInfo.DeviceName = _deviceSource.PipelineIdentifier;
            }
            // Catches exception if no base is detected
            catch (BaseDetectionFailedException e)
            {
                Console.WriteLine("No Base detected. Please retry.");
                return;
            }

            // Create a reference to this Pipeline
            Pipeline = PipelineController.Instance.PipelineIds[0];

            // Define the time (in seconds) we want to spend scanning for paired sensors.
            Pipeline.TrignoRfManager.InformationScanTime = 1;

            // Register handlers for API component (sensor specific) events
            Pipeline.TrignoRfManager.ComponentAdded += ComponentAdded;
            Pipeline.TrignoRfManager.ComponentLost += ComponentLost;
            Pipeline.TrignoRfManager.ComponentRemoved += ComponentRemoved;
            Pipeline.TrignoRfManager.ComponentScanComplete += ComponentScanComplete;

            // Register handlers for API collection events
            Pipeline.CollectionStarted += CollectionStarted;
            //Pipeline.CollectionDataReady += CollectionDataReady;
            Pipeline.CollectionDataReady += CollectionDataReady_emg_only;
            Pipeline.CollectionComplete += CollectionComplete;
        }

        /// <summary>
        /// Configures the pipeline and sensor selection.
        /// </summary>
        public void ConfigurePipeline()
        {
            DataLine dataLine = new DataLine(Pipeline);
            dataLine.ConfigurePipeline();

            PipelineController.Instance.SetFrameThroughput(1);

            // Get the frame throughput (the number of Trigno frames passed from the API at a time)
            _frameThroughput = PipelineController.Instance.GetFrameThroughput();

            int totalChannels = 0;
            foreach (var comp in Pipeline.TrignoRfManager.Components)
            {
                totalChannels += comp.TrignoChannels.Count();
            }
            Console.WriteLine("Total Channels: " + totalChannels);

            StreamInfo.PipelineStatus = Pipeline.CurrentState.ToString();
            StreamInfo.SensorsConnected = Pipeline.TrignoRfManager.Components.Count();
            StreamInfo.TotalChannels = totalChannels;

            for(int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
            {
                SensorIDMapping2.Add(Pipeline.TrignoRfManager.Components[i].Id.ToString(), i);
                Console.WriteLine("Sensor ID: " + Pipeline.TrignoRfManager.Components[i].Id.ToString() + " with PairNumber: " + Pipeline.TrignoRfManager.Components[i].PairNumber + " Mapped to index: " + i);
            }  
            
            Console.WriteLine("Pipeline configured.");

            return;
        }

        /// <summary>
        /// Scans for paired sensors on the Trigno base station.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task Scan()
        {
            Console.WriteLine("# of components before scan: " + Pipeline.TrignoRfManager.Components.Count);
            // Remove any previously connected sensors from pipeline
            foreach (var comp in Pipeline.TrignoRfManager.Components)
            {
                await Pipeline.TrignoRfManager.DeselectComponentAsync(comp);
                Pipeline.TrignoRfManager.RemoveTrignoComponent(comp);
            }

            // Remove all sensors from the name list
            //SensorNames.Clear();

            // Scan
            Console.WriteLine("Scanning for paired Sensors 5s...");
            await Pipeline.Scan();

            Console.WriteLine("# of components after scan: " + Pipeline.TrignoRfManager.Components.Count);
            //UpdateSensorsConnected();
            //UpdatePipelineStatus();
        }

        /// <summary>
        /// Stops the active pipeline data stream.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task StopStreamAsync()
        {
            await Pipeline.Stop();
        }

        /// <summary>
        /// Resets the pipeline and removes all components.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task ResetPipeline()
        {
            // Start reset by disarming pipeline
            await Pipeline.DisarmPipeline();

            _totalFrames = 0;
            _totalLostPackets = 0;
            _streamTime = 0.0;

            // Removes components from pipeline
            for (int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
            {
                Debug.WriteLine("Removing component...");
                Pipeline.TrignoRfManager.RemoveTrignoComponent(Pipeline.TrignoRfManager.Components[i]);
            }

            ResetUI();

        }

        /// <summary>
        /// Exports the data collected from the sensors to a CSV file.
        /// </summary>
        public void ExportData()
        {
            List<string> lines = new List<string>();

            int nSensors = Pipeline.TrignoRfManager.Components.Count;
            int nChannels = _data.Count;

            string labelRow = "";

            foreach (var sensor in Pipeline.TrignoRfManager.Components.Where(x => x.State == SelectionState.Allocated))
            {

                foreach (var channel in sensor.TrignoChannels)
                {
                    Console.WriteLine("Channel Name: " + channel.Name);
                    
                    labelRow += sensor.Properties.Sid + " " + channel.Name + ",";
                }
            }

            int largestChannel = 0;
            for (int i = 1; i < nChannels; i++)
            {
                if (_data[i].Count > _data[largestChannel].Count)
                {
                    largestChannel = i;
                }
            }

            for (int i = 0; i < _data[largestChannel].Count; i++)
            {
                string dataRow = "";

                if (i == 0)
                {
                    dataRow += labelRow;
                    dataRow += "\n";
                }

                for (int j = 0; j < nChannels; j++)
                {
                    if (i < _data[j].Count)
                    {
                        dataRow += _data[j].ElementAt(i).ToString() + ",";
                    }
                    else
                    {
                        dataRow += ",";
                    }
                }

                lines.Add(dataRow);
            }

            string dataDir = "./sensor_data";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            string fileName = DateTime.Now.ToString("yyy-dd-MM--HH-mm-ss");
            string path = dataDir + "/" + fileName + ".csv";
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                foreach (string line in lines)
                {
                    outputFile.WriteLine(line);
                }
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "sensor_data",
                UseShellExecute = true,
                Verb = "open"
            });
        }

        /// <summary>
        /// Helper method to reset the UI after a reset button was pressed and the pipeline was reset.
        /// </summary>
        private void ResetUI()
        {
            StreamInfo.PipelineStatus = Pipeline.CurrentState.ToString();
            StreamInfo.StreamTime = "0.0 seconds";
            StreamInfo.PacketsLost = 0;
            StreamInfo.FramesCollected = 0;

            _totalFrames = 0;
            _totalLostPackets = 0;
            _streamTime = 0.0;

        }


        #region API Component Event Handlers

        public void ComponentAdded(object sender, ComponentAddedEventArgs e)
        {
            Console.WriteLine("ComponentAdded");
        }

        public void ComponentLost(object sender, ComponentLostEventArgs e)
        {
            Console.WriteLine("ComponentLost");
        }

        public void ComponentRemoved(object sender, ComponentRemovedEventArgs e)
        {
            Console.WriteLine("ComponentRemoved");
        }

        public void ComponentScanComplete(object sender, ComponentScanCompletedEventArgs e)
        {
            // Check if no sensors were detected in scan
            if (e.ComponentDictionary.Count <= 0)
            {
                // Propmt user to try again if none found
                //_scannedSensors.NoSensorsDetected();
                Console.WriteLine("No sensors detected. Please retry.");

                return;
            }

            int sensorIndex = 0;
            foreach (var comp in this.Pipeline.TrignoRfManager.Components)
            {
                // Add the component to the pipeline
                comp.SelectSampleMode(comp.Configuration.SampleModes[0]);

                // Add the component to the list of selected sensors
                //SelectedSensors.Add(comp);

                // Add the component to the list of sensor names
                //SensorNames.Add(comp.FriendlyName);

                sensorIndex++;
            }
        }

        #endregion

        #region API Data Collection Event Handlers

        public void CollectionStarted(object sender, CollectionStartedEvent e)
        {

            StreamInfo.PipelineStatus = Pipeline.CurrentState.ToString();

            _data = new List<List<double>>();
            _totalFrames = 0;
            _totalLostPackets = 0;

            int totalChannels = 0;

            sensorData = new List<List<double>>();
            vector = Vector.Build.Dense(Pipeline.TrignoRfManager.Components.Count);
            bufferSize = (int)(_emgSampleSize * _frameThroughput);
            buffer = new Vector<double>[bufferSize];
            for (int i = 0; i < bufferSize; i++)
            {
                buffer[i] = Vector.Build.Dense(Pipeline.TrignoRfManager.Components.Count, 0.0);
            }



            // Recreate the list of data channels for recording.
            // First, iterate across all components
            for (int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
            {
                Console.WriteLine(Pipeline.TrignoRfManager.Components[i].Properties.Sid.ToString());
                // then across all channels within each component.
                for (int j = 0; j < Pipeline.TrignoRfManager.Components[i].TrignoChannels.Count; j++)
                {
                    if (_data.Count <= totalChannels)
                    {
                        _data.Add(new List<double>());
                        sensorData.Add(new List<double>());
                    }
                    else
                    {
                        _data[totalChannels] = new List<double>();
                        sensorData[totalChannels] = new List<double>();
                    }
                    if (_packetInterval == 0)
                    {
                        _packetInterval = Pipeline.TrignoRfManager.Components[i].TrignoChannels[j].FrameInterval * _frameThroughput;
                    }
                    totalChannels++;
                    NTotalChannels++;
                }
            }

            Console.WriteLine("Collection Started ...");
            _isStreaming = true;

            bufferSize = (int)(_emgSampleSize * _frameThroughput);

        }

        public void CollectionDataReady(object sender, ComponentDataReadyEventArgs e)
        {
            // Increment timer
            _streamTime += _packetInterval;

            int lostPackets = 0;

            // Checks to see if any of the packets were lost during the stream.
            // Loops through each frame data, since the API passes the data as n packets/frames, where n is the value of frameThroughput.
            // We need to check all frames to see if any data is lost from any of them.
            for (int k = 0; k < e.Data.Count(); k++)
            {
                // Loops through each sensor.
                for (int i = 0; i < e.Data[k].SensorData.Count(); i++)
                {
                    // Checks to see if any of the data in a sensor is lost.
                    if (e.Data[k].SensorData[i].IsDroppedPacket)
                    {
                        lostPackets++;
                    }
                }
            }
            _totalLostPackets += lostPackets;
            _totalFrames += _frameThroughput * e.Data[0].SensorData.Count();

            
            // Determines the column that the data will be inserted into.
            int columnIndex = 0;
            for (int k = 0; k < e.Data.Count(); k++)
            {
                // Loops through each connected sensors.
                for (int i = 0; i < e.Data[k].SensorData.Count(); i++)
                {
                    // Loops through each channel for each sensor.
                    for (int j = 0; j < e.Data[k].SensorData[i].ChannelData.Count(); j++)
                    {
                        //var test = e.Data[k].SensorData[i].Id.ToString();
                        //SensorIDMapping[test] = (e.Data[k].SensorData[i].ChannelData[j].Data);
                        //SensorIDMapping[e.Data[k].SensorData[i].]
                        // Loops through the data of each channel.
                        foreach (var val in e.Data[k].SensorData[i].ChannelData[j].Data)
                        {
                            // Adds the data at the current column index.
                            _data[columnIndex].Add(val);
                            //SensorIDMapping[test].Add(val);
                        }
                        columnIndex++;
                    }
                }
                // Resets column index so the next data in the second Trigno Frame gets added to the right channels.
                columnIndex = 0;
            }

            StreamInfo.PacketsLost = _totalLostPackets;
            StreamInfo.StreamTime = _streamTime.ToString("#.##") + " seconds";
            StreamInfo.FramesCollected = _totalFrames;
        }

        public void CollectionComplete(object sender, CollectionCompleteEvent e)
        {

        }

        // this works only if all sensors have the same sample mode
        /// <summary>
        /// This is the main entry point for data collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public void CollectionDataReady_emg_only(object sender, ComponentDataReadyEventArgs e)
        {
            // Initialize the vector to hold data for all components
            vector = Vector.Build.Dense(Pipeline.TrignoRfManager.Components.Count);

            // List of expected sensor IDs from your mapping
            var expectedSensorIDs = SensorIDMapping2.Keys.ToList();

            // Loop through each frame data
            for (int a = 0; a < _emgSampleSize; a++)
            {
                // HashSet to track processed sensor IDs in this iteration
                var processedSensorIDs = new HashSet<string>();
                var duplicateSensorIDs = new HashSet<string>();

                // Loop through each sensor data
                for (int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
                {
                    var sensorID = e.Data[0].SensorData[i].Id.ToString();

                    // Check for duplicate sensor IDs
                    if (!processedSensorIDs.Add(sensorID))
                    {
                        // Duplicate detected
                        duplicateSensorIDs.Add(sensorID);
                    }

                    // Map sensor ID to index vID
                    if (SensorIDMapping2.TryGetValue(sensorID, out int vID))
                    {
                        // Assign data to the correct position in the vector
                        vector[vID] = e.Data[0].SensorData[i].ChannelData[0].Data[a];
                    }
                    else
                    {
                        // Log if the sensor ID is not found in the mapping
                        Console.WriteLine($"[Sample {a}] Sensor ID {sensorID} not found in mapping.");
                    }
                }

                // Check for missing sensor IDs
                var missingSensorIDs = expectedSensorIDs.Except(processedSensorIDs).ToList();
                if (missingSensorIDs.Count > 0)
                {
                    Console.WriteLine($"[Sample {a}] Missing sensor IDs: {string.Join(", ", missingSensorIDs)}");
                }

                // Log any duplicate sensor IDs
                if (duplicateSensorIDs.Count > 0)
                {
                    Console.WriteLine($"[Sample {a}] Duplicate sensor IDs detected: {string.Join(", ", duplicateSensorIDs)}");
                }

                // Store the data in the buffer
                buffer[a] = vector.Clone();
            }
        }

        public void CollectionDataReady_emg_only_old(object sender, ComponentDataReadyEventArgs e)
        {
            // Loops through each frame data, since the API passes the data as n packets/frames, where n is the value of frameThroughput.
            // We are setting FrameThroughput to 1 so its e.Data[0] only.
            // Loops through the data of EMG channel
            int vID;
            vector = Vector.Build.Dense(Pipeline.TrignoRfManager.Components.Count);
            for (int a = 0; a < _emgSampleSize; a++)
            {
                // Loops through each allocated sensor
                for (int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
                {
                    var test = e.Data[0].SensorData[i].Id.ToString();
                    //SensorIDMapping[test] = e.Data[0].SensorData[i].ChannelData[0].Data[a];
                    vector[i] = e.Data[0].SensorData[i].ChannelData[0].Data[a];
                
                    SensorIDMapping2.TryGetValue(test, out vID);
                    vector[vID] = e.Data[0].SensorData[i].ChannelData[0].Data[a];

                }
                buffer[(int)a] = vector.Clone(); // store the downsampled data (interpolated index) in buffer
            }

            //// Loops through each allocated sensor
            //for (int i = 0; i < Pipeline.TrignoRfManager.Components.Count; i++)
            //{
            //    var test = e.Data[0].SensorData[i].Id.ToString();
            //    SensorIDMapping[test] = e.Data[0].SensorData[i].ChannelData[0].Data;
            //}
            //foreach(var key in SensorIDMapping)
            //{

            //}
        }

        #endregion
    }
    #endregion
}
