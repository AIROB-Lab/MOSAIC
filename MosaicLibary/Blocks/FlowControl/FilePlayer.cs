using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using static MosaicLibary.FileReader;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;



namespace MosaicLibary
{
    #region Control Panel
    /// <summary>
    /// Represents a <see cref="ControlPanel"/> for playing back a file within a <see cref="FilePlayer"/>.
    /// </summary>
    public partial class CpFilePlayer : ControlPanel
    {
        /// <summary>
        /// The FilePlayer instance associated with this control panel.
        /// </summary>
        private FilePlayer _sourceBlock;

        public CpFilePlayer(FilePlayer scourceBlock) : base(scourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = scourceBlock;

            //subscribe to FileReader events
            scourceBlock.FileReader.FileReadFinished += FileReader_FileReadFinished;
        }

        /// <summary>
        /// Refreshes the control panel every Tick of the DispatcherTimer. In this case updates the <see cref="ScopeMonitor"/> int the UI./>
        /// </summary>
        /// <param name="myObject">The sender object.</param>
        /// <param name="myEventArgs">Event arguments.</param>
        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            scope.Update(this._sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Event handler for the "Select File" button click. Opens a dialog to select a file, then reads the file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSelectFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
            _sourceBlock.FileReader.Path = openFileDialog1.FileName;
            textBoxSelectedFile.Text = openFileDialog1.FileName;
            textBoxSelectedFile.Focus();
            _sourceBlock.FileReader.ReadFile();
        }

        private void FileReader_FileReadFinished(object sender, MatrixEventArgs e)
        {
            pictureBox1.Visible = true;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            _sourceBlock.FileReader.FileReload();
        }
    }
    #endregion


    #region FileReader
    enum FileType
    {
        Unknown,
        Matlab,
        Text, 
        Csv
        // Add more file formats if necesseary
    }

    public class FileReader
    {
        /// <summary>
        /// Full Path to temporal data file that should be read in.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Frequency at wich the temporal data should be sampled.
        /// </summary>
        public double Frequency { get; set; }
        /// <summary>
        /// Holds the data, which got read in from file.
        /// </summary>
        public Matrix<double> Matrix { get; set; }

        /// <summary>
        /// Event for notifiyng that the File was sucessfuly read.
        /// </summary>
        public event EventHandler<MatrixEventArgs> FileReadFinished;
        public class MatrixEventArgs : EventArgs
        {
            public Matrix<double> Matrix { get; set; }
        }

        private int _currentRowIndex = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public FileReader()
        {
        }

        /// <summary>
        /// Reads in File: checks for file ending, reads in file and formats matrix.
        /// </summary>
        public void ReadFile()
        {
            if (Path == String.Empty || !File.Exists(Path))
            {
                Console.WriteLine("Path is not set or file is not exisiting.");
                return;
            }

            FileType fileType = GetFileType();

            switch (fileType)
            {
                case FileType.Text:
                    Matrix = ReadTextFile();
                    break;
                case FileType.Matlab:
                    Matrix = ReadMatlabFile();
                    break;
                case FileType.Csv:
                    Matrix = ReadCsvFile();
                    break;
                case FileType.Unknown:
                    Console.WriteLine("Filetype unkown!");
                    break;

                    //Add more cases when adding new types of filetypes

            }

        }

        /// <summary>
        /// Identifies the Type of the file to read by the file ending. 
        /// </summary>
        /// <returns><see cref="FileType"/> of the input file.</returns>
        private FileType GetFileType()
        {
            string fileExtension = System.IO.Path.GetExtension(Path);

            if (string.IsNullOrEmpty(fileExtension))
            {
                return FileType.Unknown;
            }

            switch (fileExtension.ToLower())
            {
                case ".mat":
                    return FileType.Matlab;

                case ".txt":
                    return FileType.Text;

                case ".csv":
                    return FileType.Csv;
                // Add more cases for additional file formats

                default:
                    return FileType.Unknown;
            }
        }

        /// <summary>
        /// Reads in Matlab .mab file.
        /// </summary>
        /// <returns>Returns <see cref="MathNet.Numerics.LinearAlgebra.Matrix{T}"/> or null if reading the file fails.</returns>
        private Matrix<double> ReadMatlabFile()
        {
            try
            {
                Matrix<double> matrix = MatlabReader.Read<double>(Path);
                matrix = FormatMatrix(matrix);
                OnFileReadFinished(matrix);
                DebugPrintToConsole(matrix, FileType.Matlab);
                return matrix;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading MATLAB file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reads in .txt file and Parses it to <see cref="MathNet.Numerics.LinearAlgebra.Matrix{T}"/>
        /// </summary>
        private Matrix<double> ReadTextFile()
        {
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(Path))
                {
                    var filedata = sr.ReadToEnd();
                    List<List<double>> dataLists = ParseTextData(filedata);
                    Matrix<double> matrix = CreateMatrix(dataLists);
                    matrix = FormatMatrix(matrix);
                    OnFileReadFinished(matrix);

                    // Now, 'matrix' contains the data from the text file 
                    DebugPrintToConsole(matrix, FileType.Text);

                    return matrix;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Reads in .csv file and Parses it to <see cref="MathNet.Numerics.LinearAlgebra.Matrix{T}"/>
        /// </summary>
        private Matrix<double> ReadCsvFile()
        {
            try
            {
                // Open the CSV file using a stream reader.
                using (var sr = new StreamReader(Path))
                {
                    var csvData = new List<List<double>>();
                    string line;

                    // Read each line in the CSV file
                    while ((line = sr.ReadLine()) != null)
                    {
                        var values = line.Split(',')
                                         .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
                                         .ToList();
                        csvData.Add(values);
                    }

                    // Convert the CSV data into a matrix
                    Matrix<double> matrix = CreateMatrix(csvData);
                    matrix = FormatMatrix(matrix);
                    OnFileReadFinished(matrix);

                    // Now, 'matrix' contains the data from the CSV file 
                    DebugPrintToConsole(matrix, FileType.Csv);

                    return matrix;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
            catch (FormatException e)
            {
                Console.WriteLine("Data format error in CSV file:");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Parses test data in lists ouf doubles format. 
        /// </summary>
        /// <param name="filedata">Text data read from stream.</param>
        /// <returns>Lists generated out ouf the text data."</returns>
        private List<List<double>> ParseTextData(string filedata)
        {
            List<List<double>> dataLists = new List<List<double>>();

            string[] rows = filedata.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string row in rows)
            {
                string[] columns = row.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<double> dataList = new List<double>();

                foreach (string column in columns)
                {
                    double value;
                    if (double.TryParse(column, out value))
                    {
                        dataList.Add(value);
                    }
                    else
                    {
                        // Handle invalid data or provide appropriate error messages
                        Console.WriteLine($"Invalid data: {column}");
                    }
                }

                dataLists.Add(dataList);
            }
            return dataLists;
        }

        /// <summary>
        /// Creats Matrix out of Lists.
        /// </summary>
        /// <param name="dataLists"></param>
        /// <returns></returns>
        private Matrix<double> CreateMatrix(List<List<double>> dataLists)
        {
            int rowCount = dataLists.Count;
            int maxColumnCount = dataLists.Max(list => list.Count);

            double[,] dataArray = new double[rowCount, maxColumnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < dataLists[i].Count; j++)
                {
                    dataArray[i, j] = dataLists[i][j];
                }
            }

            return Matrix<double>.Build.DenseOfArray(dataArray);
        }

        /// <summary>
        /// Checks if in the mxn Matrix matrix is bigger or equal then n. If n is bigger transpose Matrix.
        /// </summary>
        /// <param name="matrix">Matrix to be checked.</param>
        /// <returns>Retruns mxn Matrix, where matrix > n.</returns>
        private Matrix<double> FormatMatrix(Matrix<double> matrix)
        {
            if (matrix.ColumnCount >= matrix.RowCount)
            {
                matrix = matrix.Transpose();
            }
            return matrix;
        }

        /// <summary>
        /// Reloads the file from the beginning. This method resets the internal state for a new file read operation.
        /// </summary>
        public void FileReload()
        {
            _currentRowIndex = 0;
        }


        /// <summary>
        /// Sends the next available vector of data from the matrix. This method is intended to be called repeatedly to simulate real-time data playback.
        /// </summary>
        /// <returns>A vector representing the next timestep's data, or null if the matrix is not loaded.</returns>
        public Vector SendData()
        {
            if (Matrix != null)
            {

                Vector<double> newVector = Matrix.Row(_currentRowIndex);

                Vector currentRow = newVector.SubVector(1, newVector.Count - 1);

                _currentRowIndex++;
                return currentRow;
            }
            else 
            { 
                return null; 
            }
        }

        #region Events

        /// <summary>
        /// Triggers the FileReadFinished event with the loaded matrix data. This method is called after successfully reading a file.
        /// </summary>
        /// <param name="matrix">The matrix that was read from the file.</param>
        protected virtual void OnFileReadFinished(Matrix<double> matrix)
        {
            // Trigger the event with relevant information
            FileReadFinished?.Invoke(this, new MatrixEventArgs { Matrix = matrix });
        }

        public class ErrorEventArgs : EventArgs
        {
            public string ErrorMessage { get; }

            public ErrorEventArgs(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }

        #endregion

        #region Debug
        [Conditional("DEBUG")]
        private void DebugPrintToConsole(Matrix<double> matrix, FileType fileType)
        {
            Console.WriteLine(matrix.ToMatrixString());
            Console.WriteLine(fileType.ToString());
            Console.WriteLine(Frequency.ToString());
        }

        [Conditional("DEBUG")]
        private void DebugFrequency(object o)
        {
            Console.WriteLine("Frequency currently at: ");
            Console.WriteLine(o.ToString());
        }
        #endregion
    }
    #endregion

    #region FilePlayer Block

    /// <summary>
    /// Represents a FilePlayer for reading and playing back data from files. Data will be played 
    /// at the <c>DesiredRate</c> defined by the <see cref="ScheduledTimer"/> input block.
    /// </summary>
    /// <example>
    /// <code>
    /// myFilePlayer:
    /// {
    ///   Type: FilePlayer,
    ///   Inputs: [ myTimer ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: FilePlayer</c> indicates that this block reads data from a file and outputs 
    ///       it one sample (or vector) per timer tick.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ myTimer ]</c> means the block uses <c>myTimer</c> to drive its playback rate. 
    ///       Each timer tick triggers the reading of the next sample from the file.
    ///     </description>
    ///   </item>
    /// </list>
    /// </example>
    public class FilePlayer : Block
    {
        /// <summary>
        /// The FileReader component used for reading and processing file data.
        /// </summary>
        public FileReader FileReader { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePlayer"/> class with specified configuration parameters.
        /// </summary>
        /// <param name="name">The name of the FilePlayer block.</param>
        /// <param name="desiredRate">The desired rate at which the file data should be processed and played back.</param>
        /// <param name="inputCfg">A list of configuration settings for input processing.</param>
        /// <param name="parameter">Additional parameters for fine-tuning the playback and processing behavior.</param>
        /// <param name="path">The file path from which data will be read.</param>
        public FilePlayer(string name, double desiredRate, List<string> inputCfg, List<string> parameter, string path)
            : base(name, desiredRate, inputCfg, parameter, path) { }

        /// <summary>
        /// Configures the inputs for the FilePlayer, ensuring there is only one input block and setting up the FileReader.
        /// </summary>
        public override void ConfigureInputs()
        {
            base.ConfigureInputs();

            if (InputBlocks.Count != 1) 
                throw new Exception($"FilePlayer {Name} must have one input block only.");
            DesiredRate = InputBlocks[0].DesiredRate;
            FileReader = new FileReader();
            cp = new CpFilePlayer(this);
        }

        /// <summary>
        /// When <see cref="FileReader"/> recieves an Input Tick from a Timer, one Vector at the current Timestep is sent to the next Block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        protected override void OnNewInput(Block sender, object value)
        {
            Data = FileReader.SendData();
            SendOutput(Data);
        }
    }
    #endregion
}
