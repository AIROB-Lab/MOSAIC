using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// Joins multiple input streams into a single output stream. Requires a designated block that acts as a timer to trigger output.
    /// </summary>
    /// <example>
    /// <code>
    /// myJoiner: 
    /// {
    ///   Type: Joiner,
    ///   Inputs: [ BlockAName, sensorBlockBName, timerBlockName ],
    ///   Params: [ "timerBlockName:timerBlockName" ]
    /// }
    /// </code>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Type: Joiner</c> indicates that this block combines data from multiple inputs into a single output.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>Inputs: [ BlockAName, BlockBName, timerBlock ]</c> shows three different input blocks:
    ///       <list type="bullet">
    ///         <item><description><strong>BlockA</strong> and <strong>BlockB</strong> might provide numeric vectors.</description></item>
    ///         <item><description><strong>timerBlock</strong> is the designated block whose arrival of data triggers the Joiner to produce output.</description></item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description><c>Params: [ "timerBlockName:timerBlockName" ]</c> tells the Joiner which input block to treat as the timer. 
    ///     When data arrives from this block, the Joiner concatenates all currently stored vectors and outputs the combined row.</description>
    ///   </item>
    /// </list>
    /// 
    /// This configuration allows the Joiner to collect data from multiple streams at different times, 
    /// and then send a single, merged vector whenever the timer block triggers.
    /// </example>
    public class Joiner : Block
    {
        /// <summary>
        /// Stores vectors from each input stream, keyed by block name.
        /// </summary>
        private Dictionary<string, Vector> _inputVectorsDict = new Dictionary<string, Vector>();

        /// <summary>
        /// Name of the timer block used to trigger output.
        /// </summary>
        private string _timerBlockName = "";

        /// <summary>
        /// Initializes a new instance of the Joiner block.
        /// </summary>
        /// <param name="Name">Name of the block.</param>
        /// <param name="DesiredRate">Desired processing rate.</param>
        /// <param name="InputCfg">Input configuration.</param>
        /// <param name="Params">Parameters including the name of the timer block.</param>
        /// <param name="Path">Resource path, if necessary.</param>
        public Joiner(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path) 
        {
            // When this sender is received data is send out
            if (Params != null)
            {
                foreach (var param in Params)
                {
                    if (param.Contains("timerBlockName:"))
                    {
                        _timerBlockName = Params[0].Split(':')[1];
                    }
                }
            }
            else
            {
                throw new Exception($"No TimerBlockName {_timerBlockName} defined in YAML.");
            }
        }

        /// <summary>
        /// Configures the Joiner block, setting up input streams and identifying the timer block.
        /// </summary>
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // a Joiner has two inputs.
            if (InputBlocks.Count < 2) throw new Exception($"Joiner {Name} must have at least two input blocks.");

            foreach (var block in InputBlocks)
            {
                _inputVectorsDict.Add(block.Name, null);
            }

            if(!_inputVectorsDict.ContainsKey(_timerBlockName))
            {
                throw new Exception($"Defined TimerBlockName {_timerBlockName} does not exist.");
            }
        }

        /// <summary>
        /// Processes new input from any of the streams. When input is received from the timer block, it triggers output.
        /// </summary>
        /// <param name="sender">The block sending input.</param>
        /// <param name="value">The input value, expected to be a Vector.</param>
        override protected void OnNewInput(Block sender, object value)
        {
            // write vector to according place in 
            _inputVectorsDict[sender.Name] = value as Vector;

            // Defined in Yaml, which block acts as timer
            if (sender.Name == _timerBlockName)
            {
                // matrix for concating
                Matrix<double> m_cat = null;
                // iterate over dictionary and get values
                foreach (var InputVector in _inputVectorsDict)
                {
                    if (InputVector.Value != null)
                    {
                        if (m_cat == null)  
                            m_cat = InputVector.Value.ToRowMatrix();

                        else 
                            m_cat = m_cat.Append(InputVector.Value.ToRowMatrix());
                    }
                }
                // send out m_cat (which only consists of 1 row)
                SendOutput(m_cat.Row(0));
            }
        }
    }
}
