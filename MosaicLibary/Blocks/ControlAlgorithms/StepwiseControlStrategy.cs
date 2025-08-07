using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace MosaicLibary
{
    /// <summary>
    /// Implements the Stepwise Control strategy, which updates actuation values 
    /// incrementally based on thresholded activation levels from the prediction vector.
    /// Used in <see cref="ControlAlgorithm"/>.
    /// </summary>
    /// <remarks>
    /// The Stepwise Control strategy ensures that movements are adjusted gradually 
    /// rather than mapped directly from the prediction vector. It uses a threshold (_deadBand)
    /// to prevent unintended minor activations and scales actuation values incrementally 
    /// with <see cref="_dsMax"/> (step size).
    /// </remarks>
    public class StepwiseControlStrategy : IControlAlgorithmStrategy
    {
        /// <summary>
        /// Stores the actuation values for each Degree of Actuation (DOA) (implemented DOAs see <see cref="DOAs"/>).
        /// </summary>
        private Dictionary<DOAs, double> _controlDict = new Dictionary<DOAs, double>();
        /// <summary>
        /// Defines the step size for incremental adjustments in actuation.
        /// </summary>
        private readonly double _dsMax = 100 / 25.0;
        /// <summary>
        /// Defines the deadband threshold to prevent small activations from triggering changes.
        /// </summary>
        private readonly double _deadBand = 0.3;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepwiseControlStrategy"/> class.
        /// Sets initial values for all DOAs.
        /// The prediction vector follows the structure defined in the configuration file:
        /// <list type="bullet">
        ///   <item><description>[0] → Thumb Flexion</description></item>
        ///   <item><description>[1] → Thumb Rotation</description></item>
        ///   <item><description>[2] → Index Finger Flexion</description></item>
        ///   <item><description>[3] → Middle Finger Flexion</description></item>
        ///   <item><description>[4] → Ring Finger Flexion</description></item>
        ///   <item><description>[5] → Little Finger Flexion</description></item>
        ///   <item><description>[6] → Wrist Flexion</description></item>
        ///   <item><description>[7] → Wrist Extension</description></item>
        ///   <item><description>[8] → Wrist Pronation</description></item>
        ///   <item><description>[9] → Wrist Supination</description></item>
        ///   <item><description>[10] → Wrist Ulnar Deviation</description></item>
        ///   <item><description>[11] → Wrist Radial Deviation</description></item>
        ///   <item><description>[12] → Elbow Flexion</description></item>
        ///   <item><description>[13] → Elbow Extension</description></item>
        ///   <item><description>[14] → Global Open Hand Activation</description></item>
        /// </list>
        /// </summary>
        public StepwiseControlStrategy()
        {
            _controlDict[DOAs.ThumbFlexion] = 0;
            _controlDict[DOAs.ThumbRotation] = 0;
            _controlDict[DOAs.Index] = 0;
            _controlDict[DOAs.Middle] = 0;
            _controlDict[DOAs.Ring] = 0;
            _controlDict[DOAs.Little] = 0;
            _controlDict[DOAs.WristFlexionExtension] = 50;
            _controlDict[DOAs.WristUlnarRadialDevation] = 50;
            _controlDict[DOAs.WristPronationSupination] = 50;
            _controlDict[DOAs.ElbowFlexion] = 0;
            _controlDict[DOAs.ElbowExtension] = 0;
        }

        public void ProcessPrediction(Vector prediction)
        {


            if (prediction[0] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.ThumbFlexion] += Math.Max(0.0, Math.Min(1.0, prediction[0] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ThumbFlexion] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ThumbFlexion] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.ThumbFlexion]));
            }
            // ThumbFlexion/Open
            if (prediction[1] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.ThumbRotation] += Math.Max(0.0, Math.Min(1.0, prediction[1] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ThumbRotation] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ThumbRotation] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.ThumbRotation]));
            }
            // Index/Open
            if (prediction[2] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.Index] += Math.Max(0.0, Math.Min(1.0, prediction[2] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Index] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Index] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.Index]));
            }
            // Middle/Open
            if (prediction[3] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.Middle] += Math.Max(0.0, Math.Min(1.0, prediction[3] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Middle] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Middle] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.Middle]));
            }

            // Ring/Open
            if (prediction[4] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.Ring] += Math.Max(0.0, Math.Min(1.0, prediction[4] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Ring] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Ring] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.Ring]));
            }

            // Little/Open
            if (prediction[5] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.Little] += Math.Max(0.0, Math.Min(1.0, prediction[5] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Little] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.Little] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.Little]));
            }

            // Ext/Flex
            if (prediction[7] > _deadBand || prediction[6] > _deadBand)
            {
                _controlDict[DOAs.WristFlexionExtension] += Math.Max(0.0, Math.Min(1.0, prediction[7] - _deadBand)) * _dsMax;
                _controlDict[DOAs.WristFlexionExtension] -= Math.Max(0.0, Math.Min(1.0, prediction[6] - _deadBand)) * _dsMax;
                _controlDict[DOAs.WristFlexionExtension] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.WristFlexionExtension]));
            }

            // Pro/Sup
            if (prediction[9] > _deadBand || prediction[8] > _deadBand)
            {
                _controlDict[DOAs.WristPronationSupination] += Math.Max(0.0, Math.Min(1.0, prediction[9] - _deadBand)) * _dsMax;
                _controlDict[DOAs.WristPronationSupination] -= Math.Max(0.0, Math.Min(1.0, prediction[8] - _deadBand)) * _dsMax;
                _controlDict[DOAs.WristPronationSupination] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.WristPronationSupination]));
            }

            if (prediction[13] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.ElbowFlexion] += Math.Max(0.0, Math.Min(1.0, prediction[13] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ElbowFlexion] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ElbowFlexion] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.ElbowFlexion]));

            }

            if (prediction[14] > _deadBand || prediction[12] > _deadBand)
            {
                _controlDict[DOAs.ElbowFlexion] += Math.Max(0.0, Math.Min(1.0, prediction[14] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ElbowFlexion] -= Math.Max(0.0, Math.Min(1.0, prediction[12] - _deadBand)) * _dsMax;
                _controlDict[DOAs.ElbowFlexion] = Math.Max(0.0, Math.Min(100.0, _controlDict[DOAs.ElbowFlexion]));

            }
        }

        /// <summary>
        /// Retrieves the current control dictionary, which contains actuation values for all DOAs.
        /// </summary>
        /// <returns>A dictionary mapping each <see cref="DOAs"/> to its current actuation value.</returns>
        public Dictionary<DOAs, double> GetControlDict()
        {
            return _controlDict;
        }
    }
}

