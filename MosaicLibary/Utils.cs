using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Collections;

namespace MosaicLibary
{
    #region CircularBuffer
    /// <summary>
    /// Implements a circular buffer, an array of elements of type T, with a predefined and immutable length. 
    /// Elements are added sequentially, and you always know which element is the first (added most remotely in the past) and the last (just added).
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
    public class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _currentIdx;
        private bool _firstRound = true;

        /// <summary>
        /// Gets the oldest element in the buffer.
        /// </summary>
        public T First { get { return _buffer[_firstRound ? 0 : _currentIdx]; } }

        /// <summary>
        /// Gets the newest element in the buffer.
        /// </summary>
        public T Last { get { return _buffer[_currentIdx > 0 ? _currentIdx - 1 : _buffer.Length - 1]; } }

        /// <summary>
        /// If it's the first round, gets the current index (count of elements added so far). 
        /// After on circulation returns the total length of buffer.
        /// </summary>
        public int Top { get { return _firstRound ? _currentIdx : _buffer.Length; } }



        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class with the specified size.
        /// </summary>
        /// <param name="size">The size (length) of the buffer.</param>
        public CircularBuffer(int size) { _buffer = new T[size]; }

        /// <summary>
        /// Adds a value to the next place in the buffer (<c>_buffer[_currentIdx++]</c>).
        /// </summary>
        /// <param name="value">The value to be added.</param>
        public void Add(T value)
        {
            _buffer[_currentIdx++] = value;
            if (_currentIdx == _buffer.Length)
            {
                _firstRound = false;
                _currentIdx = 0;
            }
        }

        /// <summary>
        /// Resets the Buffer.
        /// </summary>
        public void Reset()
        {
            _currentIdx = 0;
            _firstRound = true;
        }
    }
    #endregion

    #region DBWriter
    /// <summary>
    /// Implements a double-buffered data writer. Uses two alternating buffers to store data as fast as possible.
    /// Once buffer 1 is full, switches storing to buffer 2 and dumps buffer 1 to the disk.
    /// If the dump file already exists, appends happily so that no data ever gets lost.
    /// </summary>
    public class DBWriter
    {
        /// <summary>
        /// Path to which file to dump.
        /// </summary>
        private readonly string _filename;

        /// <summary>
        /// Outputstream to file.
        /// </summary>
        //private StreamWriter _outputStream;

        /// <summary>
        /// Size (length) of each buffer.
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// Buffers implemented as lists of strings.
        /// </summary>
        private List<string> _buffer1, _buffer2;

        /// <summary>
        /// Indicates which buffer is active. True if buffer 1 is active, false if buffer 2 is active.
        /// </summary>
        private bool _buffer1IsActive;

        /// <summary>
        /// True if buffer is flushing.
        /// </summary>
        private bool _flushing;

        readonly object dumpingBufferLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DBWriter"/> class.
        /// </summary>
        /// <param name="completeFilename">The file path where data will be written to.</param>
        /// <param name="bufferSize">Length of the Buffer.</param>
        public DBWriter(string completeFilename, int bufferSize = 500)
        {
            _filename = completeFilename;
            //_outputStream = new StreamWriter(_filename, true, Encoding.ASCII);
            _bufferSize = bufferSize;
            _buffer1 = new List<string>(_bufferSize);
            _buffer2 = new List<string>(_bufferSize);
            // buffer 1 goes first
            _buffer1IsActive = true;
        }

        /// <summary>
        /// Updates the buffer size based on the desired rate to ensure the buffer capacity aligns with the rate at which data is expected to be processed. 
        /// This adjustment is necessary because the desired rate of blocks may be determined by the blocks to which they are subscribed, which can occur after initial buffer setup.
        /// This method recalculates the buffer size to maintain data for a fixed duration, typically 10 seconds, based on the newly set desired rate.
        /// </summary>
        /// <param name="bufferSize">The new buffer size, calculated to hold 10 seconds' worth of data at the current desired rate. 
        /// This size is used to adjust the buffer's capacity to ensure 
        /// it can accommodate the expected volume of incoming data without overflow or excessive memory usage.</param>
        public void UpdateBufferSize(int bufferSize = 500)
        {
            _bufferSize = bufferSize;
            _buffer1 = new List<string>(_bufferSize);
            _buffer2 = new List<string>(_bufferSize);
            // buffer 1 goes first
            _buffer1IsActive = true;
        }

        /// <summary>
        /// Adds data to the buffer for writing to the file.
        /// </summary>
        /// <param name="data">The data to be added to the buffer.</param>
        public void Add(string data)
        {
            // If _flushing, cannot add anything more
            if (_flushing)
                return;

            // Choose which buffer is "active"
            List<string> activeBuffer = _buffer1IsActive ? _buffer1 : _buffer2;

            // Add the datum to the active buffer
            activeBuffer.Add(data);

            // Check if the active buffer is full
            if (activeBuffer.Count == _bufferSize)
            {
                // If full, start a new task to dump the buffer asynchronously
                Task.Run(() => DumpBuffer(activeBuffer));

                // Switch to the other buffer
                _buffer1IsActive = !_buffer1IsActive;
            }
        }

        /// <summary>
        /// Dumps the buffer to the disk.
        /// </summary>
        /// <param name="buffer">The buffer to be dumped.</param>
        private void DumpBuffer(List<string> buffer)
        {
            if (System.Threading.Monitor.TryEnter(dumpingBufferLock))
            {
                //using statments for automatic disposing of the Streamwriter
                using (StreamWriter outputStream = new StreamWriter(_filename, true, Encoding.ASCII))
                {
                    foreach (string str in buffer)
                        outputStream.WriteLine(str);
                }
                buffer.Clear();
                System.Threading.Monitor.Exit(dumpingBufferLock);
            }
            else
                Console.WriteLine($"WARNING: DBWriter {_filename} cannot keep the pace. Increase size _buffer.");
        }

        /// <summary>
        /// Flushes buffer after shutting down <see cref="DBWriter"/> and dispose them.
        /// </summary>
        /// <returns>A task representing the asynchronous flush operation.</returns>
        public Task Flush()
        {
            _flushing = true;
            //wait till both buffers are empty
            if (_buffer1.Count > 0) DumpBuffer(_buffer1);
            if (_buffer2.Count > 0) DumpBuffer(_buffer2);
            //dispose of outpustream
            //_outputStream.Close(); 
            //_outputStream.Dispose();
            return Task.CompletedTask;
        }
    }
    #endregion

    #region StreamManager
    /// <summary>
    /// StreamManager - Takes a Stream (off a file, a string, a serial port, a TCP socket,
    ///     or whatever else is based upon a Stream) and
    ///         1. enforces a Write()/WriteAsync() method to write to the stream;
    ///         2. fires a DataReceived event whenever new data is available.
    ///     Originally born because the SerialPort.DataReceived event seems to be unreliable
    ///     (see https://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport).
    ///     Uses StartReading() to invoke Stream.BeginRead and asynchronously read stuff from the stream;
    ///     upon finishing reading, FinishedReading() is called upon, which is where the DataReceived event is fired.
    ///     After firing the event, FinishedReading() calls StartReading() again, unless StopReading is set to true.
    ///     Initialise the class with a Stream, the maximum length of the buffer for the received data,
    ///     and a delay in msecs, which the class will wait for if no data was available. This is essential,
    ///     otherwise the BeginRead()/FinishedReading() cycle would spin as fast as possible.
    /// </summary>
    public class StreamManager
    {
        /// <summary>
        /// Stream used for reading data.
        /// </summary>
        private Stream _myStream;

        /// <summary>
        /// Buffer used for storing received data.
        /// </summary>
        private byte[] _myBuffer;

        /// <summary>
        /// Delay in milliseconds for waiting if no data is available.
        /// </summary>
        private int _delayInMilliseconds;

        /// <summary>
        /// Flag indicating whether to stop streaming.
        /// </summary>
        private bool _stopStreaming;

        /// <summary>
        /// Maximum length of the buffer for received data.
        /// </summary>
        private int _bufferLength;

        /// <summary>
        /// Number of bytes received in the last read operation.
        /// </summary>
        private int _numberOfBytesReceived;


        /// <summary>
        /// Constructs a new StreamManager instance.
        /// </summary>
        /// <param name="myStream">The input stream.</param>
        /// <param name="bufferLength">The maximum length of the buffer for received data.</param>
        /// <param name="delayInMilliseconds">The delay in milliseconds for waiting if no data is available.</param>
        public StreamManager(Stream myStream, int bufferLength = 1024, int delayInMilliseconds = 10)
        {
            //throws an exeption if myStream is null
            _myStream = myStream ?? throw new ArgumentNullException(nameof(myStream));
            _bufferLength = bufferLength;
            _myBuffer = new byte[_bufferLength];
            _delayInMilliseconds = delayInMilliseconds;
        }

        /// <summary>
        /// Writes the specified message to the stream.
        /// </summary>
        /// <param name="msg">The message to write.</param>
        public void Write(byte[] msg) { _myStream.Write(msg, 0, msg.Length); }

        /// <summary>
        /// Asynchronously writes the specified message to the stream.
        /// </summary>
        /// <param name="msg">The message to write.</param>
        public void WriteAsync(byte[] msg) { _myStream.WriteAsync(msg, 0, msg.Length); }

        /// <summary>
        /// Start the Stream.
        /// </summary>
        public void StartStreaming() { _stopStreaming = false; _myStream.BeginRead(_myBuffer, 0, _bufferLength, FinishedReading, null); }

        /// <summary>
        /// Stops the stream by setting the stopStreaming flag to true.
        /// </summary>
        public void StopStreaming() { _stopStreaming = true; }

        /// <summary>
        /// Checks  if Stream is still read. Returns is still reading. If anything is received from Stream, asynchronously fires event with the right amount of data. If nothing is received,
        /// waits a while. Countious reading, or stops reading is _stopStreaming is <c>true</c>.
        /// </summary>
        /// <param name="ar"></param>
        void FinishedReading(IAsyncResult ar)
        {
            // Check if we are finished reading.
            try { _numberOfBytesReceived = _myStream.EndRead(ar); }
            // If not, bail out.
            catch { return; }

            // if so, if we received anything, asynchronously fire my event with the right amount of data
            if (_numberOfBytesReceived != 0) { DataReceived?.Invoke((byte[])_myBuffer.Take(_numberOfBytesReceived).ToArray().Clone()); }
            // otherwise, wait a while
            else { Thread.Sleep(_delayInMilliseconds); }

            // then, by all means go back to reading, unless we want to stop reading
            if (!_stopStreaming) _myStream.BeginRead(_myBuffer, 0, _bufferLength, FinishedReading, null);
        }

        /// <summary>
        /// Event handler delegate for when data is received.
        /// </summary>
        /// <param name="receivedBytes">The received bytes.</param>
        public delegate void DataReceivedHandler(byte[] Receivedbytes);

        /// <summary>
        /// Event that is fired when data is received.
        /// </summary>
        public event DataReceivedHandler DataReceived;

    }
    #endregion

    #region Utils
    /// <summary>
    /// Utils - A class with static members only, containing all "helper" functions, not precisely related to any
    /// specific class. Call them by prepending Utils (e.g. "Utils.Now") or via "using MosaicLibary.Utils".
    /// </summary>
    public class Utils
    {
        static double t0;
        static Stopwatch sw = null;

        /// <summary>
        /// Now - Use the Windows Stopwatch class to obtain precise timestamps
        /// </summary>
        public static double Now
        {

            get
            {
                if (sw == null) { sw = Stopwatch.StartNew(); t0 = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; }
                return t0 + (double)sw.ElapsedTicks / (double)Stopwatch.Frequency;
            }
        }


        /// <summary>
        /// global, slow, windows-based timer for refreshing purposes (block table, control panels, etc.)
        /// we use a DispatcherTimer because it is used to refresch the GUI. Interval: 50msec (20 frames/s)
        /// </summary>
        public static System.Windows.Threading.DispatcherTimer dtmRefresh =
            new System.Windows.Threading.DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 50), IsEnabled = true };

        /// <summary>
        /// Pretty-printing function used to properly dump vectors in SendOutput
        /// </summary>
        /// <param name="o"><c>object</c> to be print 
        /// <list type="bullet">
        /// <item>
        /// <term>o == null</term>
        /// <description>NaN</description>
        /// </item>
        /// <item>
        /// <term>o == Vector</term>
        /// <description>print in matlab-compatible format
        /// </description>
        /// </item>
        /// <item>
        /// <term>o == matrix</term>
        /// <description>print it in matlab-compatible format (one row per line)
        /// </description>
        /// </item>
        /// <item>
        /// <term>anything else</term>
        /// <description>print the standard way <c>$"{o}"</c>
        /// </description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="timeStamp"></param>
        /// <returns>Returns input <c>object o</c> as <c>string</c>. Formatting criteria see above. </returns>
        public static string Print(object o, string timeStamp = "")
        {
            System.Windows.Forms.Application.CurrentCulture = CultureInfo.InvariantCulture;

            switch (o)
            {
                case null:
                    return $"{timeStamp},{string.Join(",", "NaN")}";
                case Vector vector:
                    return Print(vector, timeStamp);
                case Matrix matrix:
                    return Print(matrix, timeStamp);
                case byte[] byteArray:
                    return PrintByteArray(byteArray, timeStamp);
                default:
                    return $"{timeStamp},{string.Join(",", "{o}")}";

            }
        }

        private static string Print(Vector vector, string timeStamp = "")
        {
            return string.IsNullOrEmpty(timeStamp)
                ? string.Join(",", vector) 
                : $"{timeStamp},{string.Join(",", vector)}";
        }

        private static string Print(Matrix matrix, string timeStamp = "")
        {
            string result = string.Empty;
            for (int rowIdx = 0; rowIdx < matrix.RowCount; ++rowIdx)
            {
                result += Print(matrix.Row(rowIdx), timeStamp) + '\n';
            }
            return result; // Remove the trailing newline
        }

        private static string PrintByteArray(byte[] byteArray, string timeStamp = "")
        {
            return string.IsNullOrEmpty(timeStamp)
                ? string.Join(",", byteArray)
                : $"{timeStamp},{string.Join(",", byteArray)}";
        }
    }
    #endregion
}
