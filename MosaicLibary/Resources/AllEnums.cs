using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicLibary
{
    /// <summary>
    /// Available Degrees of Actuation currently supported by the system
    /// </summary>
    public enum DOAs
    {
        ThumbFlexion,
        ThumbRotation,
        Index,
        Middle,
        Ring,
        Little,
        WristFlexionExtension,
        WristUlnarRadialDevation,
        WristPronationSupination,
        OpenHand, // not used in AIROB, just for clearity reasons
        ElbowFlexion,
        ElbowExtension
        // tbc if needed...
    }

    /// <summary>
    /// Available Control Algorithms supported by the system
    /// </summary>
    public enum Algorithms
    {
        None,
        DirectControl, // AIROB
        StepwiseControl  // IIT
    }

    /// <summary>
    /// Available enums for Window types, selectable in <see cref="SlidingWindow"/>
    /// </summary>
    public enum WindowType
    {
        Rectangular, 
        Hamming, 
        Hann
    }
}
