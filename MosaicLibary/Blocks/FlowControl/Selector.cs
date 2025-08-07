using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// The Selector class receives a tuple as input (e.g., <c>(string, Vector)</c>) and forwards 
    /// only the second element of that tuple as output.
    /// </summary>
    /// <example>
    /// <code>
    /// selector: 
    /// {
    ///   Type: Selector,
    ///   Inputs: [ myTupleBlock ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: Selector</c> indicates this block will take a tuple from its input 
    ///     and output only the tuple’s second element (e.g., a <see cref="Vector"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ myTupleBlock ]</c> shows that the Selector expects a single 
    ///     input block named <c>myTupleBlock</c>, which produces a tuple <c>(string, Vector)</c>.</description>
    ///   </item>
    /// </list>
    /// 
    /// In this configuration, whenever the <c>myTupleBlock</c> sends a tuple like <c>("identifier", VectorOfValues)</c>, 
    /// the Selector extracts <c>VectorOfValues</c> and forwards it as output to any downstream blocks.
    /// </example>
    public class Selector : Block
    {
        /// <summary>
        /// Constructor for the Selector class.
        /// </summary>
        /// <param Name="Name">The Name of the Selector block.</param>
        /// <param Name="DesiredRate">The desired rate of data processing.</param>
        /// <param Name="InputCfg">Configuration for the input.</param>
        /// <param Name="Params">Additional parameters for the Selector.</param>
        /// <param Name="Path">The file system path related to the Selector, if any.</param>
        public Selector(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) { }

        /// <summary>
        /// Configures the inputs for the Selector. Verifies that there is exactly one input block and sets the desired rate.
        /// </summary>
        /// <exception cref="Exception">Thrown when the number of input blocks is not exactly one.</exception>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Selector has one input
            if (InputBlocks.Count != 1) 
                throw new Exception($"Selector {Name} must have 1 input block.");

            // a Switch's DesiredRate is that of the Block which is let go right now. initially, the zero'th.
            DesiredRate = InputBlocks[0].DesiredRate;
        }

        /// <summary>
        /// Processes new input received from the connected block.
        /// Specifically, it extracts and forwards the second element of a tuple.
        /// </summary>
        /// <param Name="sender">The block that sent the input.</param>
        /// <param Name="value">The input value, expected to be a tuple.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            var (a_string, a_target_value) = ((string, Vector))value;

            SendOutput(a_target_value);
        }
    }
}
