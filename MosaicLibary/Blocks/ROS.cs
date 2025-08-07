using System;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using System.Linq;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------

    // -----------------------------------------------------------------------------------------

    ///// <summary>
    ///// ROS - Robot Operating System (ROS) Integration
    ///// This class establishes a connection to a ROS bridge server, subscribes to or publishes messages on specified ROS topics, and processes incoming data.
    ///// 
    ///// Before running the code, make sure the ROS bridge is running either on WSL or a docker with exposed port 9090 (Websocket port).
    ///// 1. source your bash file e.g. ROS humble: source /opt/ros/humble/setup.bash
    ///// 2. Start the ROS bridge
    /////  ROS 1:
    /////  roslaunch rosbridge_server rosbridge_websocket.launch
    /////  ROS2:
    /////  ros2 launch rosbridge_server rosbridge_websocket_launch.xml
    ///// The final published data can be monitored on the topic "example_topic2":
    ///// rostopic echo /example_topic2
    ///// Params: [ "IP", "publish/subscribe", "Topic_name", "Message_Type" ]
    ///// Explanation of each parameter:
    ///// 1. "IP": WebSocket server IP.
    ///// 2. "publish" / "subscribe":  Use "publish" to PUBLISH message on the ROS bridge. "subscribe" receive message from the ROS bridge.
    ///// 3. "Topic_name": The name of the topic to which the data will be published or subscribed.
    ///// 4. "Message_Type": The message type used for publishing/Subscribing. Here, it specifies that the message is of type "std_msgs/Float64MultiArray".
    ///// Other types can be found here: http://docs.ros.org/en/melodic/api/std_msgs/html/index-msg.html
    ///// </summary>
    ///<summary>
    /// Provides integration with the Robot Operating System (ROS) by connecting to a ROS bridge server 
    /// and either subscribing to or publishing messages on specified ROS topics.
    ///</summary>
    ///<remarks>
    ///Before running the code, make sure the ROS bridge is running either on WSL or a docker with exposed port 9090 (Websocket port).
    /// <para>
    /// <strong>Setup Instructions</strong>  
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Source your ROS environment. For example (ROS2 Humble):
    ///       <code>
    ///       source /opt/ros/humble/setup.bash
    ///       </code>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Start the ROS bridge (exposing WebSocket port 9090). For example:
    ///       <list type="bullet">
    ///         <item>
    ///           <description><strong>ROS1</strong>:
    ///             <code>
    ///             roslaunch rosbridge_server rosbridge_websocket.launch
    ///             </code>
    ///           </description>
    ///         </item>
    ///         <item>
    ///           <description><strong>ROS2</strong>:
    ///             <code>
    ///             ros2 launch rosbridge_server rosbridge_websocket_launch.xml
    ///             </code>
    ///           </description>
    ///         </item>
    ///       </list>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Once the bridge is running, you can observe published messages on a topic using:
    ///       <code>
    ///       rostopic echo /example_topic2
    ///       </code>
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <strong>Parameters (Params)</strong>  
    /// <list type="number">
    ///   <item>
    ///     <description><c>"IP"</c> – The IP address of the WebSocket server.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>"publish" or "subscribe"</c> – Specify <c>"publish"</c> to send messages, 
    ///       or <c>"subscribe"</c> to receive messages.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>"Topic_name"</c> – The name of the ROS topic for publishing/subscribing.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>"Message_Type"</c> – The ROS message type (e.g., <c>std_msgs/Float64MultiArray</c>). 
    ///       Other options are documented at 
    ///       <see href="http://docs.ros.org/en/melodic/api/std_msgs/html/index-msg.html"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// 
    ///</remarks>
    public class ROS : Block
    {
        private WebSocket ws;
        private string ipAddress;
        private string action;
        private string topic;
        private string type;
        private string numChannelsInt;

        private JObject subscribeMessage;
        private JObject publishMessage;
        private JObject advertiseMessage;
        private JObject receivedMessage;

        public ROS(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        { }

        // ConfigureInputs: Validates and configures input parameters for ROS connection.
        override public void ConfigureInputs()
        {
            base.ConfigureInputs();

            // Ensure that the necessary parameters are provided.
            if (Params.Count < 4)
            {
                throw new Exception("Insufficient parameters provided.");
            }

            ipAddress = Params[0]; // "IP": WebSocket server IP.
            action = Params[1]; // "publish" / "subscribe"
            topic = Params[2]; // "Topic_name": The name of the topic to which the data will be published or subscribed.
            type = Params[3]; //  The message type used for publishing/Subscribing  http://docs.ros.org/en/melodic/api/std_msgs/html/index-msg.html
            numChannelsInt = Params[4];
            int numChannels = int.Parse(numChannelsInt);
            // Connecting to the ROS Bridge
            Console.WriteLine("Connecting to ROS bridge...");
            string webSocketUrl = $"ws://{ipAddress}:9090"; // Rosbridge WebSocket server started at ws://0.0.0.0:9090
            ws = new WebSocket(webSocketUrl);

            // Prepare the subscribe or publish message based on the action.
            if (action.ToLower() == "subscribe")
            {
                subscribeMessage = JObject.FromObject(new
                {
                    op = action,
                    topic = $"/{topic}",
                    type = $"/{type}"
                });
            }
            else if (action.ToLower() == "publish")
            {
                advertiseMessage = JObject.FromObject(new
                {
                    op = "advertise",
                    topic = $"/{topic}",
                    type = $"/{type}"
                });
            }

            // Define function 
            ws.OnMessage += (msgSender, msgE) =>
            {
                if (action.ToLower() == "subscribe")
                {
                    receivedMessage = JObject.Parse(msgE.Data);
                    if (receivedMessage["op"]?.ToString() == "publish" && receivedMessage["topic"]?.ToString() == $"/{topic}")
                    {
                        // Convert received data to Array
                        var dataArray = ((JArray)receivedMessage["msg"]["data"]).ToObject<double[]>();

                        int totalLength = dataArray.Length;

                        // Calculate the number of samples
                        int numSamples = totalLength / numChannels;

                        // Adjust totalLength to fit into the matrix dimensions
                        int adjustedLength = numChannels * numSamples;
                        var reshapedMatrix = new double[numChannels, numSamples];

                        // Efficient reshaping without intermediate collections
                        for (int i = 0; i < totalLength; i++)
                        {
                            int row = i % numChannels;
                            int col = i / numChannels;
                            reshapedMatrix[row, col] = dataArray[i];
                        }

                        // Convert the matrix back to a vector (if needed)
                        var reshapedArray = reshapedMatrix.Cast<double>().ToArray();
                        //var reshapedArray = reshapedMatrix.SelectMany(x => x).ToArray();
                        var data = Vector<double>.Build.DenseOfArray(reshapedArray);

                        // Send the reshaped data to the output
                        SendOutput(data);
                    }
                }
            };


            // Event handler for WebSocket connection open event.
            ws.OnOpen += (wsSender, wsE) =>
            {
                Console.WriteLine("ROS bridge Connected");

                if (action.ToLower() == "subscribe")
                {
                    ws.Send(subscribeMessage.ToString());
                }
                if (action.ToLower() == "publish")
                {
                    ws.Send(advertiseMessage.ToString());
                }
            };

            ws.Connect(); // Establish the WebSocket connection.
        }

        // OnNewInput: Handles new input data and publishes/subscribes it to the ROS topic.
        override protected void OnNewInput(Block inputSender, object value)
        {
            if (ws == null || !ws.IsAlive)
            {
                Console.WriteLine("WebSocket is not connected.");
                return;
            }

            if (action.ToLower() == "publish")
            {
                publishMessage = JObject.FromObject(new
                {
                    op = "publish",
                    topic = $"/{topic}",
                    msg = new { data = (value as Vector).ToArray() },
                    type = $"/{type}"
                });
                ws.Send(publishMessage.ToString()); // Send the publish message.
            }

            if (action.ToLower() == "subscribe")
            {
                Console.WriteLine("SENT: " + receivedMessage["msg"]["data"]);
            }
        }
    }
}