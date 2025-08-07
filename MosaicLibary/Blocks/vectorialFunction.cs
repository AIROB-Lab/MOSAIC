using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace MosaicLibary
{
    /// <summary>
    /// A control panel for the <see cref="VectorialFunction"/> block.
    /// </summary>
    public partial class cpVectorialFunction : ControlPanel
    {
        /// <summary>
        /// The source block associated with this control panel, which represents a vectorial function.
        /// </summary>
        private VectorialFunction _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CpVectorialFunction"/> class with the specified vectorial function as its source block.
        /// </summary>
        /// <param name="sourceBlock">The vectorial function <see cref="VectorialFunction"/> to associate with this control panel.</param>
        public cpVectorialFunction(VectorialFunction sourceBlock) : base(sourceBlock)
        {
            InitializeComponent();
            _sourceBlock = sourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel's content, updating the UI to reflect the current state of the vectorial function. 
        /// The UI includes a <see cref="ScopeMonitor"/> and a <see cref="SpiderPlotMonitor"/> that are updated.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        override protected void cpRefresh(object o, EventArgs e)
        {
            smFunction.Update(_sourceBlock.Data as Vector);
            spFunction.Update(_sourceBlock.Data as Vector);
        }

        /// <summary>
        /// Handles the click event for the button that loads a model file. 
        /// It reads a file selected by the user and updates the source block with parameters derived from the file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "iM model files|*.txt",
                Title = "Open model file",
                RestoreDirectory = true
            };
            ofd.ShowDialog();

            string[] lines = System.IO.File.ReadAllLines(ofd.FileName);

            List<Vector> ll = new List<Vector>();

            foreach (string line in lines) ll.Add(Vector.Build.Dense(Array.ConvertAll(line.Split(','), double.Parse)));

            int d = lines.Length;

            // Assuming this is coming from a linear model for now
            Matrix Ainv = Matrix.Build.DenseOfRowVectors(ll.ToArray().Take(d / 2));
            Matrix B = Matrix.Build.DenseOfRowVectors(ll.ToArray().Skip(d / 2).Take(d / 2));

            Matrix w = Ainv * B;

            _sourceBlock.SetParams(w);
        }

        /// <summary>
        /// Handles the click event for the button that resets the output of the vectorial function.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void button2_Click(object sender, EventArgs e)
        {
            _sourceBlock.ResetOutput();
        }
    }

    /// <summary>
    /// Represents a specialized <see cref="Block"/> that performs various vectorial operations on input vectors,
    /// potentially involving one or two source input blocks. This class provides functionality
    /// to configure inputs, parse operation parameters, and process new incoming Vector data
    /// to produce resultant vectors based on the specified operation.
    ///
    /// Operations supported by this class include:
    /// <list type="bullet">
    /// <item><description><c>rotation</c>: Applies a rotational transform to an input Vector using specified roll, pitch, and yaw angles.</description></item>
    /// <item><description><c>absolute_value</c>: Computes the magnitude (L2 norm) of the input Vector.</description></item>
    /// <item><description><c>wrench_projection</c>: Projects a wrench Vector using a configurable projection matrix constructed from the input vectors.</description></item>
    /// <item><description><c>addVectors</c>: Adds two input vectors element-wise.</description></item>
    /// <item><description><c>subtract</c>: Subtracts the second input Vector from the first, element-wise.</description></item>
    /// <item><description><c>crossVector</c>: Computes the cross product of two 3D input vectors.</description></item>
    /// <item><description><c>scalarProduct</c>: Computes the scalar (dot) product of two input vectors.</description></item>
    /// <item><description><c>matrix_multiply</c>: Multiplies the input Vector by a provided projection matrix.</description></item>
    /// <item><description><c>integrate</c>: Integrates the input Vector over time by accumulating its values.</description></item>
    /// </list>
    /// </summary>
    /// <example>
    /// <code>
    /// myVectorOp:
    /// {
    ///   Type: VectorialFunction,
    ///   Inputs: [ inputAName, inputBName ],
    ///   Params: [ "subtract" ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: VectorialFunction</c> indicates that this block will perform one of the available vector operations 
    ///       (e.g., <c>subtract</c>).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ inputA, inputB ]</c> shows two upstream blocks providing vectors. 
    ///       For <c>subtract</c>, the second vector is subtracted from the first.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ "subtract" ]</c> configures the specific operation to be performed. 
    ///       Other operations (e.g., <c>addVectors</c>, <c>absolute_value</c>) can be substituted as needed.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// When both <c>inputA</c> and <c>inputB</c> are received, this block subtracts the second input from the first, 
    /// element by element, and outputs the resulting vector.
    /// </example>
    public class VectorialFunction : Block
    {
        /// <summary>
        /// The operation name to be performed by this vectorial function.
        /// </summary>
        private string _opName;

        /// <summary>
        /// The first Vector value input.
        /// </summary>
        private Vector<double> _value1;

        /// <summary>
        /// The second Vector value input.
        /// </summary>
        private Vector<double> _value2;

        /// <summary>
        /// The result of the operation.
        /// </summary>
        private Vector<double> _result;

        /// <summary>
        /// The name of the first source block.
        /// </summary>
        private string _source1Name = string.Empty;

        /// <summary>
        /// The name of the second source block.
        /// </summary>
        private string _source2Name = string.Empty;

        /// <summary>
        /// Parameters for the operation.
        /// </summary>
        private string[] _opParams;

        /// <summary>
        /// The projection Matrix for Matrix operations.
        /// </summary>
        private Matrix<double> _projMat = null;

        /// <summary>
        /// An array of matrices for operations requiring multiple Matrix inputs.
        /// </summary>
        private Matrix<double>[,] _matrixArray = new Matrix<double>[2, 2];

        public int _d;
        private Vector _vectorvalue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorialFunction"/> class.
        /// </summary>
        /// <param name="Name">The name of the block.</param>
        /// <param name="DesiredRate">The desired rate of operation.</param>
        /// <param name="InputCfg">Configuration for the input blocks.</param>
        /// <param name="Params">Operation parameters.</param>
        /// <param name="Path">The path for any required external resources.</param>
        public VectorialFunction(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs and parses operation parameters.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the block has incorrect input configurations or invalid operation parameters.
        /// </exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // mini-parser. expect function to be described as [f_name:factor], e.g., [multiply:2] results in 2*v
            foreach (var operation in Params)
            {
                string[] opElements = operation.Split(':');
                _opName = opElements[0];
                try { _opParams = opElements[1].Split(';'); }
                catch { }
            }

            // a Vector function should have the correct number of inputs
            if (!(_opName.Equals("rotation") || _opName.Equals("absolute_value") || _opName.Equals("matrix_multiply") || _opName.Equals("integrate")))
            {
                if (InputBlocks.Count != 2) throw new Exception($"Vector Function {Name} must have two input blocks.");
                _source1Name = InputBlocks[0].Name;
                _source2Name = InputBlocks[1].Name;
                _result = Vector.Build.Dense(3);

                if (_opName.Equals("wrench_projection"))
                {
                    _projMat = Matrix.Build.Dense(6, 6);

                    _matrixArray[0, 0] = Matrix.Build.DenseDiagonal(3, 1);
                    _matrixArray[0, 1] = Matrix.Build.Dense(3, 3);
                    _matrixArray[1, 1] = Matrix.Build.DenseDiagonal(3, 1);


                }
            }
            else
            {
                if (InputBlocks.Count != 1) throw new Exception($"Vector Function {Name} must have only one input block.");
                _source1Name = InputBlocks[0].Name;
                if (_opName.Equals("rotation"))
                {
                    double roll = Convert.ToDouble(_opParams[0]);
                    double pitch = Convert.ToDouble(_opParams[1]);
                    double yaw = Convert.ToDouble(_opParams[2]);

                    _result = Vector.Build.Dense(3);
                }
                else _result = Vector.Build.Dense(1);
            }




            // a Function's DesiredRate is the same as the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            // a Function has >=1 Params
            //if (Params.Count < 1) throw new Exception($"Function {Name} must have at least 1 parameter.");


            cp = new cpVectorialFunction(this);
        }

        /// <summary>
        /// Processes new input data and performs the configured operation.
        /// </summary>
        /// <param name="sender">The source block sending the input.</param>
        /// <param name="value">The input Vector.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            if (sender.Name.Equals(_source1Name)) 
                _value1 = value as Vector;
            if (sender.Name.Equals(_source2Name)) 
                _value2 = value as Vector;

            if (sender.Name.Equals(_source1Name) && (_value1 != null && _value2 != null && _opName != "rotation" || _opName == "rotation" || (_opName == "matrix_multiply" && _projMat != null) || _opName == "integrate" || _opName == "absolute_value"))
            {
                switch (_opName)
                {
                    case "rotation":
                        Vector vector = value as Vector;

                        break;
                    case "absolute_value":
                        _vectorvalue = value as Vector;
                        _result[0] = _vectorvalue.L2Norm();
                        break;
                    case "wrench_projection":
                        _matrixArray[1, 0] = SkewSymmetric(_value2);
                        _projMat = Matrix.Build.DenseOfMatrixArray(_matrixArray);
                        _result = _projMat * _value1;
                        break;
                    case "addVectors":
                        _result = _value1 + _value2;
                        break;
                    case "subtract":
                        _result = _value1 + _value2 * (-1);
                        break;
                    case "crossVector":
                        _result = SkewSymmetric(_value1) * _value2;
                        break;
                    case "scalarProduct":
                        _result[0] = _value1.DotProduct(_value2);
                        break;
                    case "matrix_multiply":
                        if (_projMat != null) _result = _projMat * _value1;
                        break;
                    case "integrate":
                        if (_result.Count != _value1.Count) _result = Vector.Build.Dense(_value1.Count);
                        _result = _result + _value1;
                        break;

                }
                SendOutput(_result as Vector);
            }

        }

        /// <summary>
        /// Computes the skew-symmetric matrix of a 3D Vector.
        /// </summary>
        /// <param name="vector">The input 3D Vector.</param>
        /// <returns>The skew-symmetric matrix.</returns>
        /// <exception cref="Exception">Thrown if the input Vector is not 3D.</exception>
        private Matrix SkewSymmetric(Vector vector)
        {
            if (vector.Count != 3) throw new Exception($"Vectorial function {Name} attempted an operation defined on 3D vectors on a non-3D _vector. ");

            Matrix result = Matrix.Build.Dense(3, 3);
            result[0, 1] = vector[2]; result[0, 2] = -vector[1];
            result[1, 0] = -vector[2]; result[1, 2] = vector[0];
            result[2, 0] = vector[1]; result[2, 1] = -vector[0];

            return result;
        }

        /// <summary>
        /// Updates the parameters for the operation matrix mulitplication.
        /// </summary>
        /// <param name="m">The projection matrix.</param>
        public void SetParams(Matrix m)
        {
            if (_opName == "matrix_multiply")
            {
                _projMat = m;
            }
        }

        /// <summary>
        /// Resets the output Vector to its initial state.
        /// </summary>
        public void ResetOutput()
        {
            _result = Vector.Build.Dense(_result.Count);
        }
    }
}
