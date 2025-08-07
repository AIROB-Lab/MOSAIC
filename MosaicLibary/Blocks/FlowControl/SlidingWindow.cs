using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Windows.Storage.Streams;
using GraphX.Common;
using System.Drawing;

namespace MosaicLibary
{
    #region ControlPanel
    /// <summary>
    /// Represents a partial class for the <see cref="ControlPanel"/> of the <see cref="SlidingWindow"/> block.
    /// This control panel provides an interface to monitor and interact with the SlidingWindow block's data.
    /// </summary>>
    public partial class cpSlidingWindow : ControlPanel
    {
        /// <summary>
        /// The SlidingWindow block associated with this control panel.
        /// </summary>
        private SlidingWindow _sourceBlock;

        /// <summary>
        /// Constructs a new instance of the <see cref="cpSlidingWindow"/> class.
        /// </summary>
        /// <param name="sourceBlock">The SlidingWindow block associated with the control panel.</param>
        public cpSlidingWindow(SlidingWindow sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
            dropDownTypes.Items.AddRange(new string[] { "Rectangular", "Hamming", "Hann" });
            dropDownTypes.SelectedIndex = (int)_sourceBlock.Window;
        }

        /// <summary>
        /// Refreshes the control panel and updates the labels with the current status of the SlidingWindow block.
        /// </summary>
        /// <param name="o">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void cpRefresh(object o, EventArgs e)
        {
            scopeMonitor1.Update(_sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Handles the selection change in the dropdown list for window type selection.
        /// </summary>
        private void dropDownTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sourceBlock.Window = (WindowType)dropDownTypes.SelectedIndex;
        }
    }
    #endregion

    #region Sliding Window
    /// <summary>
    /// SlidingWindow - Applies a window function over streaming input data using a sliding window of fixed length and stride.
    /// Supports various window functions (Rectangular, Hamming, Hann).
    /// Useful for feature extraction, signal smoothing, or framing for further processing.
    /// </summary>
    /// <example>
    /// <para>
    /// A typical YAML configuration for a sliding window block may look like this:
    /// </para>
    /// <code>
    /// slidingWindowBlock:
    /// {
    ///   Type: SlidingWindow,
    ///   Inputs: [ myo ],
    ///   Params: [ "128", "32", "Hamming" ]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: SLIDINGWINDOW</c> specifies that this block performs a sliding window operation.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ emgBlock ]</c> specifies the input data source for the sliding window, e.g., an EMG data block.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ "128", "32", "Hamming" ]</c> defines the parameters:
    ///       <list type="number">
    ///         <item><description>
    ///           <c>128</c> - Window size (number of samples per window).
    ///         </description></item>
    ///         <item><description>
    ///           <c>32</c> - Stride (step size between windows).
    ///         </description></item>
    ///         <item><description>
    ///           <c>Hamming</c> - Window function to apply (other options: Rectangular, Hann).
    ///         </description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// In this configuration, the block processes data from <c>emgBlock</c>, grouping incoming samples into windows of 128 samples,
    /// sliding forward by 32 samples for each output, and applying a Hamming window to each segment before outputting the result.
    /// </para>
    /// </example>
    public class SlidingWindow : Block
    {
        /// <summary>
        /// The number of samples in one window.
        /// </summary>
        private int bufferSize;

        /// <summary>
        /// The step size between successive windows.
        /// </summary>
        private int stride;

        /// <summary>
        /// A function that generates a windowing matrix of the given size.
        /// </summary>
        private Func<int, Matrix> windowFunction;

        /// <summary>
        /// A dictionary mapping window types to their respective functions.
        /// </summary>
        private static readonly Dictionary<WindowType, Func<int, Matrix>> windowFunctions = new Dictionary<WindowType, Func<int, Matrix>>();

        private WindowType _window;

        /// <summary>
        /// Stores the buffered data as a matrix.
        /// </summary>
        public Matrix Buffer;

        /// <summary>
        /// Gets or sets the buffer size, which defines the number of samples in a window.
        /// </summary>
        public int BufferSize
        {
            get { return bufferSize; }
            set
            {
                if (value < 1) throw new ArgumentException("Buffer size must be greater than 0.");
                bufferSize = value;
                AdjustBufferSize();
            }
        }

        /// <summary>
        /// Gets or sets the stride, which defines the step size between successive windows.
        /// </summary>
        public int Stride
        {
            get { return stride; }
            set
            {
                if (value > bufferSize) throw new ArgumentException("Stride must be smaller than BufferSize.");
                stride = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected window type.
        /// </summary>
        public WindowType Window
        {
            get { return _window; }
            set
            {
                if (!windowFunctions.ContainsKey(value))
                    throw new ArgumentException("Unsupported window type: " + value);
                _window = value;
                windowFunction = windowFunctions[value];
            }
        }
                public SlidingWindow(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Static constructor to initialize the window functions.
        /// </summary>
        static SlidingWindow()
        {
            windowFunctions.Add(WindowType.Rectangular, size =>
                Matrix.Build.DenseOfColumnVectors(Vector<double>.Build.Dense(size, 1.0))
            );

            windowFunctions.Add(WindowType.Hamming, size =>
                Matrix.Build.DenseOfColumnVectors(Vector<double>.Build.Dense(size, r => 0.54 - 0.46 * Math.Cos((2 * Math.PI * r) / (size - 1))))
            );

            windowFunctions.Add(WindowType.Hann, size =>
                Matrix.Build.DenseOfColumnVectors(Vector<double>.Build.Dense(size, r => 0.5 * (1 - Math.Cos((2 * Math.PI * r) / (size - 1)))))
            );
        }

        /// <summary>
        /// Configures the input parameters of the sliding window block.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            this.Buffer = Matrix.Build.Dense(0, 0);

            if (InputBlocks.Count != 1)
                throw new Exception($"Function {Name} must have one input block only.");
            DesiredRate = InputBlocks[0].DesiredRate;

            if (Params.Count != 3)
                throw new Exception($"Slidiwng Window {Name} must have exactly 3 parameters.");

            var parsedParams = ParseParams(Params);
            this.BufferSize = parsedParams.BufferSize;
            this.Stride = parsedParams.Stride;
            this.Window = parsedParams.WindowFunction; 


            DesiredRate = (InputBlocks[0].DesiredRate / this.Stride) ;

            cp = new cpSlidingWindow(this);
        }

        /// <summary>
        /// Handles new input data and applies the sliding window process.
        /// </summary>
        protected override void OnNewInput(Block sender, object value)
        {
            Matrix newMatrix = ConvertToMatrix(value);

            if (Buffer.RowCount == 0 || Buffer.ColumnCount == 0)
            {
                Buffer = Matrix.Build.Dense(0, newMatrix.ColumnCount);
            }
            Buffer = Buffer.Stack(newMatrix);

            if (Buffer.RowCount >= BufferSize)
            {
                int exceedingValues = Buffer.RowCount - BufferSize;

                Matrix windowedMatrix = ApplyWindowFunction(Buffer.SubMatrix(0, BufferSize, 0, Buffer.ColumnCount));
                SendOutput(windowedMatrix);
                Data = windowedMatrix.Row(0);

                Buffer = Buffer.SubMatrix(Stride, Buffer.RowCount - Stride, 0, Buffer.ColumnCount);

                if (exceedingValues > 0)
                {
                    Matrix remainingMatrix = newMatrix.SubMatrix(exceedingValues, newMatrix.RowCount - exceedingValues, 0, newMatrix.ColumnCount);
                    Buffer = Buffer.Stack(remainingMatrix);
                }
            }
        }

        /// <summary>
        /// Adjusts the buffer size.
        /// </summary>
        private void AdjustBufferSize()
        {
            Buffer = Matrix.Build.Dense(0, Buffer.ColumnCount);
        }

        /// <summary>
        /// Converts input data into a matrix.
        /// </summary>
        private Matrix ConvertToMatrix(object value)
        {
            Matrix m;
            if (value is Vector)
            {
                m = Matrix.Build.DenseOfRowVectors((Vector)value);
                return m;
            }
            else if (value is Matrix)
            {
                m = (Matrix)value;
                return m;
            }
            throw new ArgumentException("Input must be a matrix or vector.");
        }

        /// <summary>
        /// Applies the selected window function to the input matrix.
        /// </summary>
        private Matrix ApplyWindowFunction(Matrix input)
        {
            int rows = input.RowCount;
            int cols = input.ColumnCount;

            // Generate the window function once (applies to all columns)
            Vector windowVector = windowFunction(rows).Column(0);

            // Create a windowed matrix where each column is multiplied individually
            Matrix windowedMatrix = Matrix.Build.Dense(rows, cols);

            for (int c = 0; c < cols; c++)
            {
                // Apply window function column-wise
                windowedMatrix.SetColumn(c, input.Column(c).PointwiseMultiply(windowVector));
            }

            return windowedMatrix;
        }

        private WindowParams ParseParams(List<string> Params)
        {
            if (Params == null || Params.Count < 3)
                throw new ArgumentException("SlidingWindow requires at least 3 parameters.");

            var parsedParams = new WindowParams();

            if (int.TryParse(Params[0], out int bufferSize))
                parsedParams.BufferSize = bufferSize;
            else
                throw new ArgumentException("Invalid buffer size parameter.");

            if (int.TryParse(Params[1], out int stride))
                parsedParams.Stride = stride;
            else
                throw new ArgumentException("Invalid stride parameter.");

            if (Enum.TryParse(Params[2], true, out WindowType windowType))
                parsedParams.WindowFunction = windowType;
            else
                throw new ArgumentException("Invalid window function parameter.");

            return parsedParams;
        }
    }

    /// <summary>
    /// Holds the parameters for a windowed processing block.
    /// </summa
    public class WindowParams
    {
        /// <summary>
        /// The window (buffer) size in samples.
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// The stride (hop) size in samples.
        /// </summary>
        public int Stride { get; set; }
        /// <summary>
        /// The type of window function to apply.
        /// </summary>
        public WindowType WindowFunction { get; set; }
    }

    #endregion
}



