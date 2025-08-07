using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicLibary
{
    /// <summary>
    /// Maps the prediction values to the corresponding Degrees of Actuation (DOAs).
    /// Each DOA represents a specific movement, and the received prediction vector is normalized between [0,1].
    /// This mapping scales the values to the range [0,100] for actuation.
    /// /// The prediction vector follows the order defined in the configuration file:
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
    ///   <item><description>[12] → Open Hand </description></item>
    ///   <item><description>[13] → Elbow Felxion </description></item>
    ///   <item><description>[14] → Elbow Extension </description></item>
    /// </list>
    /// </summary>
    public class DirectControlStrategy : IControlAlgorithmStrategy
    {
        /// <summary>
        /// Stores the actuation values for each Degree of Actuation (DOA).
        /// </summary>
        private Dictionary<DOAs, double> _controlDict = new Dictionary<DOAs, double>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectControlStrategy"/> class.
        /// Sets initial values for all DOAs.
        /// </summary>
        public DirectControlStrategy()
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

        public void ProcessPrediction(Vector<double> prediction)
        {
            // the order of the internal (received) prediction vector, as defined in the cfg file, is:
            // [ ThumbFlexion, ThumbRotation, Index, Middle, Ring, Little, wr_flex, wr_ext, wr_pron, wr_sup, wr_ulnar, wr_radial, open ]
            // all values in [0,...,1], so now convert vector activation values to HannesArm/Blender values:
            // to open/close the hand, use the Index flexion (that's immaterial since a fist has the first 6 DOFs trained uniformly)
            _controlDict[DOAs.ThumbFlexion] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[0]));
            _controlDict[DOAs.ThumbRotation] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[1]));
            _controlDict[DOAs.Index] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[2]));
            _controlDict[DOAs.Middle] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[3]));
            _controlDict[DOAs.Ring] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[4]));
            _controlDict[DOAs.Little] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[5]));

            // to flex/extend the wrist, do flexion - extension and scale it
            _controlDict[DOAs.WristFlexionExtension] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * (prediction[7] - prediction[6] + 1) / 2));

            // to prono/sup the wrist, just use our pronation (easier than having pronation AND supination involved...)
            _controlDict[DOAs.WristPronationSupination] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * (prediction[8] - prediction[9] + 1) / 2));
            _controlDict[DOAs.WristUlnarRadialDevation] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * (prediction[10] - prediction[11] + 1) / 2));
            _controlDict[DOAs.ElbowFlexion] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[13]));
            _controlDict[DOAs.ElbowExtension] = (byte)Math.Max(0.0, Math.Min(100.0, 100 * prediction[14]));
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

