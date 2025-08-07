using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicLibary
{
    /// <summary>
    /// Holds real-time status and metadata about the Delsys data streaming session.
    /// Used by the control panel to display device and stream status.
    /// </summary>
    public class StreamInfo
    {
        /// <summary>
        /// Gets or sets the name or identifier of the connected Delsys device.
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// Gets or sets the current pipeline status (e.g., Streaming, Stopped, Armed).
        /// </summary>
        public string PipelineStatus { get; set; }
        /// <summary>
        /// Gets or sets the number of sensors currently connected.
        /// </summary>
        public int SensorsConnected { get; set; }
        /// <summary>
        /// Gets or sets the total number of data channels in use.
        /// </summary>
        public int TotalChannels { get; set; }
        /// <summary>
        /// Gets or sets the elapsed streaming time as a formatted string.
        /// </summary>
        public string StreamTime { get; set; }
        /// <summary>
        /// Gets or sets the number of data packets lost during streaming.
        /// </summary>
        public int PacketsLost { get; set; }
        /// <summary>
        /// Gets or sets the total number of frames collected during the session.
        /// </summary>
        public int FramesCollected { get; set; }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
