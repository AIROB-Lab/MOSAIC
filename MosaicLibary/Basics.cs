using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace MosaicLibary
{
    #region Scheduler 

    /// <summary>
    /// Scheduler class to implement accurate timers.
    /// </summary>
    public sealed class Scheduler
    {
        /// <summary>
        /// After Singelton design pattern: only one scheduler is allowed.
        /// </summary>
        private static readonly Lazy<Scheduler> lazyInstance = new Lazy<Scheduler>(() => new Scheduler());
        public static Scheduler Instance => lazyInstance.Value;

        // constructor: just start the looping body
        readonly Task _task;
        Scheduler() { _task = Task.Factory.StartNew(MainLoop); }
        // destructor: tell the looping body to stop, wait for the associated task to finish, bail out
        volatile bool _shutDown;
        ~Scheduler() { _shutDown = true; _task.Wait(); }

        // an entry of the scheduler: a callback, when to launch it (T), how often to launch it (DeltaT)
        class Entry
        {
            public double T, DeltaT;
            public TimerCallback Callback;
            public Entry(TimerCallback Cb, double Rate) { T = Utils.Now; DeltaT = 1 / Rate; Callback = Cb; }
        }

        // the list of entries
        readonly LinkedList<Entry> _EntryList = new LinkedList<Entry>();

        // add a new callback to the list of callbacks, with a specific rate
        readonly object _EntryListLock = new object();
        public void Add(TimerCallback Cb, double Rate)
        {
            lock (_EntryListLock)
            {
                // add this entry only if the callback is not yet in. trying to add a callback twice throws an exception.
                if (_EntryList.Any(e => e.Callback == Cb)) throw new Exception($"{((Block)Cb.Target).Name} tried to add itself twice to the Scheduler.");
                else _EntryList.AddLast(new Entry(Cb, Rate));
            }
        }
        // remove a callback from the list
        public void Remove(TimerCallback Cb)
        {
            lock (_EntryListLock)
                // find and remove the only entry in EntryList which matches the callback.
                try
                {
                    _EntryList.Remove(_EntryList.Single(entry => entry.Callback == Cb));
                }
                catch (InvalidOperationException)
                {
                    // there must be precisely one such entry otherwise something is wrong.
                    throw new Exception($"{((Block)Cb.Target).Name} tried to remove itself from the Scheduler without currently being in it.");
                }
        }

        // main loop of the Scheduler (runs as fast as it can via spinning)
        // - sweep the entry list to find the next entry to be fired
        // - asynchronously launch its callback
        // - update its firing time T to T+DeltaT
        Entry _launchMe;
        void MainLoop()
        {
            //Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (!_shutDown)
                lock (_EntryListLock)
                {
                    // if no callbacks scheduled, skip
                    if (_EntryList.Count == 0) continue;

                    // find the next callback to be launched: that's the node in the callback list with the smallest T
                    _launchMe = _EntryList.First.Value;
                    foreach (Entry se in _EntryList) if (se.T <= _launchMe.T) _launchMe = se;

                    // spin till its time comes
                    while (Utils.Now < _launchMe.T) ;

                    // then launch it (asynchronously!)
                    _launchMe.Callback.BeginInvoke(null, null, null);

                    // lastly, schedule this callback's next occurrence
                    _launchMe.T += _launchMe.DeltaT;
                }
        }
    }

    #endregion

    #region Block

    /// The Block class has too many responsibilities. It would be better to refactor it into several classes, see Single Responsibility Principle (SRP).
    /// SendOutput invokes eventHandler synchronously, so it waits for each subscriber to be finished, and then moves on to the next one. ould be asynchronous calls to improve performance if a Block has several subscribers.

    /// <summary>
    /// Abstract base class <see cref="Block"/> for an object receiving inputs and producing outputs - in the ideal world,
    /// everything is a block (Simulink-style). Values calculated within the block are sent to other blocks
    /// by firing an Output event; inputs are received by subscribing to the Output eventhandler of each input block.
    /// Input blocks to this one are dynamically decided using SubscribeTo() and UnsubscribeFrom().
    /// It's the Chain of Life.
    /// </summary>
    public abstract class Block : IDisposable
    {

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the desired rate of the block.
        /// </summary>
        public double DesiredRate { get; set; }

        /// <summary>
        /// Rate defiened as (#ticks-1 / _deltaT), where 
        /// - #ticks is the length of the timestamp buffer so far
        /// - _deltaT is the difference between the last and first timestamp in the buffer 
        /// A new Timestamp is added to <c>Ticks</c> from each call off <c>SendOutput</c>.
        /// Total Numer of Elements in <c>Ticks</c> divied by the diffrence between the last and first timestamp in <c>Ticks</c>.
        /// </summary>
        public double Rate { get { return (Ticks.Top - 1) / (Ticks.Last - Ticks.First); } }

        /// <summary>
        /// A <c>Block </c>is defiened as lagging, when its <c>Rate</c> is lower than 95% of the <c>DesiredRate</c>.
        /// </summary>
        public bool IsLagging { get { return Rate < 0 ? false : (Rate <= 0.95 * DesiredRate); } }

        /// <summary>
        /// A <c>Block</c> id defiened as idle, when it has not ticked after 10*(1/<c>DesiredRate</c>) seconds.
        /// </summary>
        public bool IsIdle { get { return Utils.Now > Ticks.Last + 10 / DesiredRate; } }

        /// <summary>
        /// A <c>Block</c> is defiend as stumbling, when previous <c>SendOutput</c> is still executing while a new instance of it is being called. 
        /// </summary>
        public bool IsStumbling;

        protected List<string> InputCfg { get; set; }
        protected List<string> Params;
        /// <summary>
        /// The list of blocks the inputs get taken from, to be populated by ConfigureInputs
        /// </summary>
        protected List<Block> InputBlocks = new List<Block>();


        /// <summary>
        /// Circualar Buffer <c>Ticks</c> for evaluating the rate. Buffer consists of 100 timestamps calles <c>Ticks</c>. Each time an output is produced, i.e., whenever <c>SendOutput(value)</c> is 
        /// called, the current timestamp is added to <c>Ticks</c>; whenever <c>Rate</c> is queried, the total number of elements currently in <c>Ticks</c> is divided by the difference between the 
        /// last and first timestamps in it.
        /// >[!NOTE]
        /// >Why not simply evaluating the rate since the birth of the block, instead of taking the burden of memorising the past 100 timestamps? Because a block can 
        /// >be started and stopped arbitrarily – actually, whether it is running depends on the blocks it is connected to – so at some point evaluating the rate over the whole 
        /// >course of time could become very inaccurate. Picture, e.g., the case in which a block has been running at 2kHz for one minute, then it has stopped running for two minutes, 
        /// >then it has run again at 2kHz for one minute: the rate since the start in this case is 1kHz, which is deadly wrong in any possible sense; using the timestamps instead, the rate 
        /// >is evaluated only over the “recent past”. 
        /// </summary>
        readonly CircularBuffer<double> Ticks = new CircularBuffer<double>(100);
        /// <summary>
        /// Double-buffered data writer <see cref="DBWriter"/> for storing data.
        /// </summary>
        private readonly DBWriter _dump;


        private readonly object _stumblingLock = new object();
        private readonly object _onNewInputLock = new object();

        public object Data = null;
        /// <summary>
        /// Form <see cref="MosaicLibary.ControlPanel"/>
        /// </summary>
        public ControlPanel cp = null;



        /// <summary>
        /// Initializes a new instance of the <see cref="Block"/> class. Parameter <see cref="Name"/> is required.
        /// </summary>
        /// <param name="Name">The name of the block.</param>
        /// <param name="DesiredRate">The desired rate of the block.</param>
        /// <param name="InputCfg">List of inputs.</param>
        /// <param name="Params">List of parameters.</param>
        /// <param name="Path">Path for DBWriter.</param>
        public Block(string Name, double DesiredRate = 0, List<string> InputCfg = null, List<string> Params = null, string Path = null)
        {
            // store my own name, list of inputs, output type and desired rate
            this.Name = Name;
            this.DesiredRate = DesiredRate;
            this.InputCfg = InputCfg;
            this.Params = Params;
            // if required, create double-buffered writer to dump my data. store 10 seconds of data in each buffer.
            // caution the DesiredRate is often zero here, hence the buffer grows indefetenly and the writes to file, on closing
            // fixed with BufferUpgrade, when Block is subscripted to another Block, hence inhertice the Desired Rate from the previous Block
            if (Path != null)
                _dump = new DBWriter($"{Path}\\{Name}.txt", (int)(10 * this.DesiredRate));
        }

        /// <summary>
        /// Cleans up resources used by the block.
        /// </summary>
        public void Dispose()
        {
            // unsubscribe from all blocks I am currently subscribed to
            foreach (Block b in InputBlocks)
            {
                if (IsSubscribedTo(b))
                {
                    UnsubscribeFrom(b);
                }
            }
            // remove myself from the global dictionary of Blocks
            Blocks.Instance.Remove(Name);
            // flush dump, if it was active
            if (_dump != null)
            {
                _dump.Flush();
            }
        }


        /// <summary>
        /// Subscribes to the specified block. Updates Buffer size.
        /// </summary>
        /// <param name="b">The block to subscribe to.</param>
        /// <exception cref="Exception">Thrown if there is an attempt to subscribe to a block that is not among its inputs or if trying to subscribe twice.</exception>
        protected void SubscribeTo(Block b)
        {
            // only let me subscribe to blocks in my input list
            if (!HasInput(b))
            {
                throw new Exception($"{Name} tried to subscribe to {b.Name} which is not among its inputs.");
            }
            // cannot subscribe twice to the same block...
            if (IsSubscribedTo(b))
            {
                throw new Exception($"{Name} tried to subscribe to {b.Name} twice.");
            }
            // otherwise, subscribe!
            b.Output += new OutputHandler(OnNewInputLocked);

            // Updates Buffersize
            _dump?.UpdateBufferSize((int)(10 * b.DesiredRate));

        }

        /// <summary>
        /// Unsubscribes from the specified block.
        /// </summary>
        /// <param name="b">The block to unsubscribe from.</param>
        /// <exception cref="Exception">Thrown if there is an attempt to unsubscribe from a block to which it is not currently subscribed.</exception>
        protected void UnsubscribeFrom(Block b)
        {
            // are we currently subscribed to b? only if so, unsubscribe from b's output event handler
            if (IsSubscribedTo(b))
            {
                b.Output -= OnNewInputLocked;
            }
            else
            {
                throw new Exception($"{Name} tried to unsubscribe from {b.Name} to which it is currently not subscribed.");
            }
        }

        /// <summary>
        /// Configures input blocks based on the specified input configuration.
        /// This method is responsible for adding input blocks to the list of input blocks and subscribing to some of them, as specified in the input configuration.
        /// </summary>
        virtual public void ConfigureInputs()
        {
            if (InputCfg != null)
                foreach (var cfg in InputCfg)
                {
                    string ibName = cfg[0] == '-' ? cfg.Substring(1) : cfg;
                    bool ibAlsoSubscribe = cfg[0] == '-' ? false : true;

                    InputBlocks.Add(Blocks.Instance[ibName]);
                    if (ibAlsoSubscribe) SubscribeTo(Blocks.Instance[ibName]);
                }
        }

        /// <summary>
        /// Checks if <see cref="Block"/> <c>b</c> is on of the inputs.
        /// </summary>
        /// <param name="b">The block to check.</param>
        /// <returns>True if the block is one of the inputs, false otherwise.</returns>
        public bool HasInput(Block b)
        {
            return InputBlocks.Contains(b);
        }

        /// <summary>
        /// Checks if <see cref="Block"/> <c>b</c> is subscribed to.
        /// </summary>
        /// <param name="b">The block to check.</param>
        /// <returns>True if the block is subscribed to the specified block, false otherwise</returns>
        public bool IsSubscribedTo(Block b)
        {
            return b.Output != null && b.Output.GetInvocationList().Contains((OutputHandler)OnNewInputLocked);
        }



        // ------------ sending out stuff
        // a way to provide the Block's data to the outside world, if required/desired
        // the output event handler and event, fired from within a function called SendOutput

        /// <summary>
        /// Mehode <c>SendOutput</c> to sned output to the next Block if required/desired.
        /// > [!IMPORTANT]
        /// >One block should send out only one type of value or, in the most contrived case, a tuple of values of different kinds, e.g., an observation together with a target value
        /// </summary>
        /// <param name="value">The value to send as output.</param>
        /// <param name="syncSendOutput">Specifies whether to send the output synchronously or asynchronously.</param>
        protected void SendOutput(object value, bool syncSendOutput = false)
        {
            // if no previous instance of SendOutput is running, then send the output
            if (System.Threading.Monitor.TryEnter(_stumblingLock))
            {
                IsStumbling = false;
                // update the Rate's buffer
                Ticks.Add(Utils.Now);
                // make my output readable by the world
                Data = value;
                // now send off my output to all subscribers. the "sender" is always... me :-)
                if (Output != null)
                {
                    // if syncSendOutput is set, then fire the event synchronously, that is in the standard way:
                    // send to the first subscriber, wait for control to come back, send to second subscriber, wait, ...
                    if (syncSendOutput)
                    {
                        Output(this, value);
                    }
                    // otherwise, send asynchronously to each subscriber, without waiting for each subscriber to give back control
                    else
                    {
                        foreach (OutputHandler oh in Output.GetInvocationList())
                        {
                            oh.BeginInvoke(this, value, null, null);
                        }
                    }
                }
                // if required, dump my output
                if (_dump != null)
                {
                    Application.CurrentCulture = new CultureInfo("en-us");
                    _dump.Add($"{Utils.Print(value, $"{Utils.Now:0.000000}")}");
                }
                System.Threading.Monitor.Exit(_stumblingLock);
            }
            else
                // otherwise we are stumbling: calculating the output takes too much time wr
                // t the rate.
                IsStumbling = true;
        }

        public delegate void OutputHandler(Block sender, object value);
        public event OutputHandler Output;


        // ------------ receiving stuff
        // each time one of my input blocks produces a new datum, this function is called.
        // "sender" is the input block which is sending; "value" is the datum.
        // who is the sender? both the type of block (using "is") and a specific block (using ==) can be checked, e.g.
        // if (sender == EndOfTheWorldTrigger) { checks that the sender is exactly "EndOfTheWorldTrigger" }
        // if (sender is Trigger) { checks that the sender is any Trigger }
        // (need to protect the callback with a lock, otherwise mayhem happens.)



        void OnNewInputLocked(Block sender, object value) { lock (_onNewInputLock) OnNewInput(sender, value); }

        /// <summary>
        /// Methode <c>OnNewInput</c> is a call-back methode. Get's called each time on of the <c>Blocks</c> input blocks (sender) produces a new value. Handles all received inputs.
        /// </summary>
        /// <param name="sender">Input Block, which sends the value.</param>
        /// <param name="value">Value that is sent by the input block</param>
        abstract protected void OnNewInput(Block sender, object value);

        // ------------ managing the Control Panel

        /// <summary>
        /// Shows or hides the control panel associated with the block.
        /// </summary>
        public void ShowHideCp()
        {
            if (cp != null)
                if (!cp.Visible)
                    cp.Show();
                else
                    cp.Hide();
        }

        /// <summary>
        /// Handles the left-click event on the block in the BlockTable.
        /// </summary>
        public virtual void MouseLeftClick()
        {
            ShowHideCp();
        }

        /// <summary>
        /// Handles the right-click event on the block in the BlockTable.
        /// </summary>
        public virtual void MouseRightClick() { }
    }
    #endregion

    #region Blocks
    /// <summary>
    /// Blocks - A global dictionary of all blocks. After Singelton Design Pattern: there is only one list of blocks. After Singetlon Design Pattern, only one global <c>Blocks</c>.
    /// </summary>
    public sealed class Blocks : Dictionary<string, Block>
    {

        private static readonly Lazy<Blocks> lazyInstance = new Lazy<Blocks>(() => new Blocks());

        /// <summary>
        /// Gets the singleton instance of Blocks.
        /// </summary>
        public static Blocks Instance => lazyInstance.Value;

        /// <summary>
        /// Private constructor to enforce singleton pattern and initialize the dictionary. Dictionary  is case insenstive. 
        /// </summary>
        private Blocks() : base(0) { }
    }
    #endregion

    #region CfgParser
    /// <summary>
    /// CfgParser: parse the YAML configuration file and create each block. based upon YAMLDotNet.
    /// </summary>
    public class CfgParser
    {
        /// <summary>
        /// A CfgBlock is a Block as specified in the configuration - basically, a syntactic proxy of a real Block.
        /// </summary>
        public struct CfgBlock
        {
            /// <summary>
            /// The type of the block as specified in the YAML configuration.
            /// Examples include <see cref="ScheduledTimer"/>, <see cref="Function"/>, etc.
            /// This determines the functional behavior of the block.
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// The desired rate or frequency for the block's operation, if applicable.
            /// For example, a <see cref="ScheduledTimer"/> block may use this to define its trigger rate (in Hz).
            /// Blocks that process data streams, such as filters or resamplers, may also rely on this value.
            /// Often Blocks like <see cref="Function"/> or <see cref="Trigger"/> inherited the <see cref="DesiredRate"/> from the their Input Block.
            /// It is used to claculated if the Block is stumbeling, lagging or idle.
            /// </summary>
            public double DesiredRate { get; set; }
            /// <summary>
            /// A list of input block names or identifiers that this block depends on.
            /// These inputs define the data sources for the block's operation, as parsed from the YAML file.
            /// </summary>
            public List<string> Inputs { get; set; }
            /// <summary>
            /// A list of parameters associated with the block. These parameters define
            /// additional configuration details, such as coefficients for filters or scaling factors
            /// for mathematical operations. The specific meaning depends on the block's <see cref="Type"/>.
            /// </summary>
            public List<string> Params { get; set; }
            /// <summary>
            /// The file path where the block periodically writes its output vector.
            /// This is typically used for logging or external storage of the block's results.
            /// If this field is null or empty, the block does not write to an external file.
            /// </summary>
            public string Path { get; set; }
        }

        private Dictionary<string, CfgBlock> _cfgBlockDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="CfgParser"/> class. Reads in YAML cfg file. First uses Yaml.Net to parse ("Deserialize") the YAML cfg into a dictionary of cfgBlocks (structures). 
        /// Then, for each such entry, instantiates a corresponding Block.
        /// </summary>
        /// <param name="filename">Path to YAML file.</param>
        public CfgParser(string filename)
        {
            // first use Yaml.Net to parse ("Deserialize") the YAML cfg into a dictionary of cfgBlocks (structures)
            // using directive makes sure stream is properly disposed of
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    _cfgBlockDict = new DeserializerBuilder().Build().Deserialize<Dictionary<string, CfgBlock>>(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or parsing the YAML file: {ex.Message}");
                return; // Exit the constructor if there's an error
            }


            // then, for each such entry, instantiate a corresponding Block
            foreach (KeyValuePair<string, CfgBlock> cfgbEntry in _cfgBlockDict)
            {
                try
                {
                    Type thisBlockType = typeof(Block).Assembly.GetType("MosaicLibary" +
                        "." + cfgbEntry.Value.Type);
                    // then create the object using Activator.CreateInstance:
                    // a Block's constructor's signature must be (double,string,list_of_strings,list_of_strings,string,string)
                    // corresponding to (desired_rate, name, inputs, parameters, output_type, path)
                    object thisBlockInstance = Activator.CreateInstance(thisBlockType, new object[] {
                    cfgbEntry.Key,
                    cfgbEntry.Value.DesiredRate,
                    cfgbEntry.Value.Inputs,
                    cfgbEntry.Value.Params,
                    cfgbEntry.Value.Path
                    });

                    // now add this Block to the global dictionary of blocks
                    Blocks.Instance.Add(cfgbEntry.Key, thisBlockInstance as Block);
                }
                catch
                {
                    Console.WriteLine($"Error: No BlockType {cfgbEntry.Value.Type} available. Check your YAML file for typos.");
                    return;
                }

            }
            // now try and build the dependencies among blocks (input and subcription).
            // Need to do this after Blocks.Instance has been completely populated because of cycles.
            foreach (var b in Blocks.Instance.Values) b.ConfigureInputs();
        }
    }
    #endregion
}
