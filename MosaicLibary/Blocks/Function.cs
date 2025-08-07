using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// A control panel class for the <see cref="Function"/> block.
    /// </summary>
    public partial class cpFunction : ControlPanel
    {
        private readonly Function _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="cpFunction"/> class.
        /// </summary>
        /// <param name="_SourceBlock">The source function block associated with this control panel.</param>
        public cpFunction(Function _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._sourceBlock = _SourceBlock;
        }

        /// <summary>
        /// Refreshes the control panel with the latest function data.
        /// </summary>
        override protected void cpRefresh(object o, EventArgs e)
        {
            smFunction.Update(_sourceBlock.Data as Vector);
            spFunction.Update(_sourceBlock.Data as Vector);
        }
    }

    /// <summary>
    /// Represents a block that applies a specified mathematical operation to each element of an input Vector, 
    /// producing a transformed output Vector. This block supports a variety of operations, enabling flexible 
    /// element-wise manipulation of the input data.
    /// Function Block required the parameter defention in the yaml file to define the function in the format <c>[f_name:factor]</c>.
    ///
    /// The currently implemented operations include:
    /// <list type="bullet">
    ///   <item><description><c>add</c> - Adds a specified scalar value to each element.</description></item>
    ///   <item><description><c>multiply</c> - Multiplies each element by a specified scalar value.</description></item>
    ///   <item><description><c>abs</c> - Converts each element to its absolute value.</description></item>
    ///   <item><description><c>power</c> - Raises each element to a specified power.</description></item>
    ///   <item><description><c>clip</c> - Restricts each element within a specified minimum and maximum range.</description></item>
    /// </list>
    /// </summary>
    /// <example>
    /// 
    /// Configuration in YAML file: \
    /// Multiplication by 5: 
    /// <code>
    /// amplified_raw_EMG: 
    /// { 
    ///     Type: Function, 
    ///     Inputs: [ raw_EMG ], 
    ///     Params: [ multiply:5 ] 
    /// }</code> \
    /// Absolute value: 
    /// <code>
    /// rectified_raw_EMG: 
    /// { 
    ///     Type: Function, 
    ///     Inputs: [amplified_raw_EMG], 
    ///     Params: [abs:0 ] 
    /// }</code> 
    /// </example>
    public class Function : Block
    {
        /// <summary>
        /// The function to be applied to each element of the input Vector.
        /// </summary>
        private Func<double, double> _function;

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="Name">The name of the function block.</param>
        /// <param name="DesiredRate">The desired sampling rate in Hz.</param>
        /// <param name="InputCfg">The configuration of the input block(s).</param>
        /// <param name="Params">
        /// Parameters describing the function to apply. Each parameter is expected to be in the format:
        /// <c>[f_name:factor]</c>.
        /// For example:
        /// <list type="bullet">
        /// <item><description><c>add:2</c> (adds 2 to each element)</description></item>
        /// <item><description><c>multiply:3</c> (multiplies each element by 3)</description></item>
        /// </list>
        /// </param>
        /// <param name="Path">The file path for the block.</param>
        public Function(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the input and initializes the function to be applied to the Vector.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if the block does not have exactly one input block or if the parameters are invalid.
        /// </exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // Ensure exactly one input block
            if (InputBlocks.Count != 1)
                throw new Exception($"Function {Name} must have one input block only.");

            // Set the desired rate to match the input block
            DesiredRate = InputBlocks[0].DesiredRate;

            // Ensure at least one parameter is provided
            if (Params.Count < 1)
                throw new Exception($"Function {Name} must have at least 1 parameter.");

            // Parse and initialize the function based on the provided parameters
            foreach (var operation in Params)
            {
                string[] opElements = operation.Split(':');
                string opName = opElements[0];
                double opFactor = Convert.ToDouble(opElements[1]);
                switch (opName)
                {
                    case "add": _function = v => opFactor + v; break;
                    case "multiply": _function = v => opFactor * v; break;
                    case "abs": _function = v => Math.Abs(v); break;
                    case "power": _function = v => Math.Pow(v, opFactor); break;
                    case "clip": _function = v => Math.Max(0, Math.Min(opFactor, v)); break;
                }
            }

            // Initialize the control panel
            cp = new cpFunction(this);
        }

        /// <summary>
        /// Processes the input Vector by applying the configured function to each element.
        /// </summary>
        /// <param name="sender">The block that sent the input.</param>
        /// <param name="value">The input Vector.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            // Apply the function to all elements of the input Vector and send the result as output
            SendOutput((value as Vector).Map(_function));
        }
    }
}
