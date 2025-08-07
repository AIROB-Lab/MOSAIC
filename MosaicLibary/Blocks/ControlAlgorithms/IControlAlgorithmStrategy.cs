using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicLibary
{
    /// <summary>
    /// Defines the interface for all control algorithm strategies.
    /// </summary>
    public interface IControlAlgorithmStrategy
    {
        /// <summary>
        /// Processes the prediction vector and updates actuation values accordingly.
        /// </summary>
        /// <param name="prediction">
        /// A vector containing predicted activation values for each Degree of Actuation (DOA).
        /// The values are typically in the range [0,1] and need to be mapped to [0,100] for actuation.
        /// </param>
        /// <remarks>
        /// The interpretation of the prediction values depends on the specific control strategy.
        /// Implementing classes must define how the mapping is performed.
        /// </remarks>
        void ProcessPrediction(Vector<double> prediction);


        /// <summary>
        /// Retrieves the current control dictionary containing actuation values for all DOAs.
        /// </summary>
        /// <returns>
        /// A dictionary where each <see cref="DOAs"/> key is mapped to its corresponding actuation value.
        /// </returns>
        Dictionary<DOAs, double> GetControlDict();
    }
}
