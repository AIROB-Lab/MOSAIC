using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// The Projector class is a specialized block that filters and forwards a subset of elements from an input <see cref="Vector"/>.
    /// It allows you to specify a list of indices, corresponding to the elements you wish to extract from the input vector.
    /// The output is a new vector consisting of only the selected elements in the order specified. 
    /// This class inherits from the <see cref="Block"/> class.
    /// </summary>
    /// <example>
    /// <code>
    /// projector: 
    /// { 
    ///     Type: Projector, 
    ///     Inputs: [InputBlock], 
    ///     Params: ['4;5;6;0;1;2'] 
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: Projector</c> identifies this block as one that filters vector elements.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ InputBlock ]</c> shows the source vector for extraction.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ '4;5;6;0;1;2' ]</c> specifies the element indices to output in order, change order for your needs
    ///     </description>
    ///   </item>
    /// </list>
    /// </example>
    public class Projector : Block
    {
        private Vector _projectedValue;
        private int[] _projectedIdxs;

        /// <summary>
        /// Constructor for the Projector class.
        /// </summary>
        /// <param name="Name">The name of the Projector block.</param>
        /// <param name="DesiredRate">The desired rate of data processing.</param>
        /// <param name="InputCfg">Configuration for the input block.</param>
        /// <param name="Params">List of indices representing the subset of elements to be projected.</param>
        /// <param name="Path">The file system path related to the Projector, if any.</param>
        public Projector(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the Projector block. 
        /// Ensures there is exactly one input block and initializes the indices for projection based on the provided parameters.
        /// The parameters for the Projector should be defined in the YAML configuration file as a semicolon-separated list of indices 
        /// (0-based notation). For example, "1;2;4;5" will select the second, third, fifth, and sixth elements of the input vector.
        /// </summary>
        /// <exception cref="Exception">Thrown when the number of input blocks is not exactly one.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // one input only
            if (InputBlocks.Count != 1) 
                throw new Exception($"Projector {Name} must have 1 input block.");

            // the DesiredRate is that of the input Block
            DesiredRate = InputBlocks[0].DesiredRate;

            // there is one parameter: a list of channels to be selected, e.g., 1;2;4;5 will produce an output vector
            // whose 4 components are the second, third, fifth and sixth of the input vector (0-based notation)
            _projectedIdxs = Array.ConvertAll(Params[0].Split(';'), int.Parse);
            _projectedValue = Vector.Build.Dense(_projectedIdxs.Length);
        }

        /// <summary>
        /// Selects the desired elements from the input vector and forwards them to the next block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        override protected void OnNewInput(Block sender, object value)
        {
            Vector _originalValue = value as Vector;
            if (_originalValue != null && _originalValue.Count != 0)
            {
                // scan original value; find relevant indexes; clone components corresponding to indexes.
                for (int i = 0; i < _projectedIdxs.Length; i++)
                    _projectedValue[i] = _originalValue[_projectedIdxs[i]];

                SendOutput(_projectedValue);
            }

        }
    }
}
