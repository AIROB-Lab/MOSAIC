using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;



namespace MosaicLibary
{
    /// <summary>
    /// <c>ControlAlgorithm</c> - Receives prediction vectors and maps them to actuation values based on the selected control algorithm strategy.
    /// This block supports multiple control strategies, allowing for different methods of interpreting EMG-based predictions.
    /// Currently implemented are DirectControl and StepwiseControl.
    /// </summary>
    /// <example>
    /// <para>
    /// A typical YAML configuration might look like this:
    /// </para>
    /// <code>
    /// controlAlgorithm:
    /// {
    ///   Type: ControlAlgorithm,
    ///   Inputs: [ NameOfPredictor ],
    ///   Params: [ "algorithm:NameOfControl" ]
    /// }
    /// </code>
    /// <para>
    /// Explanation:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Type: ControlAlgorithm</c> specifies that this block processes prediction data and transforms it into actuation commands.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Inputs: [ NameOfPredictor ]</c> defines the input block that provides real-time prediction vectors. 
    ///       This input can be from a machine learning model such as <see cref="IncrementalPredictor"/> or <see cref="BatchPredictor"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>Params: [ "algorithm:NameOfControl" ]</c> determines which control strategy to use. 
    ///       Available algorithms are <c>DirectControl</c> (see <see cref="DirectControlStrategy"/>) and <c>StepwiseControl</c> (see <see cref="StepwiseControlStrategy"/>). 
    ///       New strategies can be added without modifying this class due to its dependency injection structure.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// In this setup, the block named <c>incr_learn_prediction</c> provides continuous prediction data, 
    /// which is then processed by <c>controlAlgorithm</c> to generate actuation values for the system.
    /// </para>
    /// </example>
    /// <remarks>
    /// Steps to add a new Control Algorithm Strategy to the system.
    /// This guide ensures the <see cref="ControlAlgorithm"/> class remains modular and extensible.
    /// <para>
    /// Follow these steps to implement a new control strategy within the system:
    /// </para>
    ///
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       <strong>Create a new class</strong> that implements the <see cref="IControlAlgorithmStrategy"/> interface.
    ///       This class will define the new algorithm's logic.
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Implement the required methods:</strong>
    ///       <list type="bullet">
    ///         <item>
    ///           <description>Initialize the control dictionary for all required <see cref="DOAs"/>.</description>
    ///         </item>
    ///         <item>
    ///           <description>Define the logic for processing prediction data inside <c>ProcessPrediction(Vector prediction)</c>.</description>
    ///         </item>
    ///         <item>
    ///           <description>Ensure <c>GetControlDict()</c> correctly returns the updated actuation values.</description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Update the <see cref="ControlAlgorithm"/> constructor</strong> to recognize the new strategy.
    ///       Modify the switch statement to include a case for the new algorithm type, assigning it to <c>_controlAlgorithm</c>.
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Add the new algorithm type</strong> to the <see cref="Algorithms"/> enumeration.
    ///       This ensures the new strategy can be referenced correctly in the code.
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Update the YAML configuration</strong> to support the new algorithm type.
    ///       Modify the control block definition to include the new algorithm name as a valid parameter.
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Test the implementation:</strong>
    ///       <list type="bullet">
    ///         <item>
    ///           <description>Ensure the system correctly selects and executes the new algorithm.</description>
    ///         </item>
    ///         <item>
    ///           <description>Verify that the expected actuation values are generated from input predictions.</description>
    ///         </item>
    ///         <item>
    ///           <description>Check that switching between algorithms works as intended.</description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///
    ///   <item>
    ///     <description>
    ///       <strong>Update documentation</strong> to reflect the newly added control algorithm strategy.
    ///       Ensure that system users can configure and utilize the new algorithm in their YAML setup.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public class ControlAlgorithm : Block
    {
        /// <summary>
        /// The selected control algorithm strategy instance.
        /// </summary>
        private readonly IControlAlgorithmStrategy _controlAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlAlgorithm"/> class.
        /// </summary>
        /// <param name="Name">The name of the control block.</param>
        /// <param name="DesiredRate">The desired processing rate in Hertz (Hz).</param>
        /// <param name="InputCfg">List of input configuration parameters.</param>
        /// <param name="Params">List of parameters defining the control strategy.</param>
        /// <param name="Path">The file path for configuration or data storage.</param>
        /// <exception cref="Exception">
        /// Thrown if the provided algorithm is not recognized or not specified in <paramref name="Params"/>.
        /// </exception>
        public ControlAlgorithm(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, new List<string>(), Path)
        {
            //base.ConfigureInputs();

            foreach (var param in Params)
            {
                string algoName = param.Split(':')[1];

                // Convert the string to an enum
                if (!Enum.TryParse(algoName, out Algorithms algorithm))
                    throw new Exception($"Algorithm {algoName} is not defined in Enum Algorithms");

                // Traditional switch-case statement
                switch (algorithm)
                {
                    case Algorithms.DirectControl:
                        _controlAlgorithm = new DirectControlStrategy();
                        break;

                    case Algorithms.StepwiseControl:
                        _controlAlgorithm = new StepwiseControlStrategy();
                        break;

                    default:
                        throw new Exception($"Unsupported algorithm: {algorithm}");
                }

                break; // Exit loop after setting the algorithm
            }
        }

        /// <summary>
        /// Handles new input data by processing the predictions through the selected control strategy.
        /// </summary>
        /// <param name="sender">The source block providing the input data.</param>
        /// <param name="value">The input data, expected to be a prediction vector.</param>
        /// <exception cref="InvalidCastException">
        /// Thrown if the provided <paramref name="value"/> is not of type <see cref="Vector"/>.
        /// </exception>
        override protected void OnNewInput(Block sender, object value)
        {
            Vector prediction = value as Vector;
            _controlAlgorithm.ProcessPrediction(prediction);
            SendOutput(_controlAlgorithm.GetControlDict());
        }
    }
}
