using MathNet.Numerics.LinearAlgebra;
using Python.Runtime;
using System;
using System.Collections.Generic;
using Windows.Devices.Printers;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;


// --------------------------------------------------------------------------------------------------------
// SimplePyTorch:
// This block is an example on how to utilizes a simple model created with pytorch. This is not a complete
// example and will need tweaking depending on your model. Alternatively, you could also train your model
// directly in MOSAIC
//
// IMPORTANT: Torch is not included in the poetry environment. If you want to run this block you need to add 
// torch to your environment dependencies or global python dependencies (with CUDA or CPU support). 
// --------------------------------------------------------------------------------------------------------
namespace MosaicLibary
{
    #region ControlPanel
    /// <summary>
    /// Represents a placeholder control panel for the <see cref="SimplePyTorch"/>.
    /// </summary>
    public partial class cpSimplePyTorch : ControlPanel
    {
        /// <summary>
        /// The associated <see cref="SimplePyTorch"/> block this control panel interacts with.
        /// </summary>
        SimplePyTorch _SourceBlock;


        /// <summary>
        /// Initializes a new instance of the control panel.
        /// </summary>
        /// <param name="sourceBlock">The AdaptiveFilter block this panel is associated with.</param>
        public cpSimplePyTorch(SimplePyTorch _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._SourceBlock = _SourceBlock;
        }

    }

    #endregion

    #region SimplePyTorch
    /// <summary>
    /// Simple PyTorch example based on the PythonNet Integration which loads a predefined model and uses it for inference. 
    /// Can easily be adjusted to load arbitrary models from the GUI etc...
    /// </summary>
    /// <remarks>
    ///  IMPORTANT: Torch is not included in the poetry environment. If you want to run this block you need to add 
    ///  torch to your environment dependencies or global python dependencies (with CUDA or CPU support) [https://pytorch.org/get-started/locally/] . 
    /// </remarks>
    /// <example>
    /// <code>
    ///  af2: { Type: SimplePyTorch, DesiredRate: 200, Inputs: [ processed_EMG_signal ]}
    /// </code>
    /// </example>
    public class SimplePyTorch : Block
    {
        // private pythonnet imports and objects
        private dynamic torch;
        private dynamic model;
        private dynamic np;

        public SimplePyTorch(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        {
            // Mandatory in every block using the PythonNet integration. If there are multiple blocks PythonNetManager will only be activated once. 
            PythonNetManager.Instance.Init();
        }

        override public void ConfigureInputs()
        {
            cp = new cpSimplePyTorch(this);
          
            base.ConfigureInputs();

            string modelPath = PythonNetManager.Instance.scriptsPath + "\\Examples\\Example_PyTorch\\model.pt";

            using (Py.GIL())
            {
                // import necessary python packages as class members
                torch = Py.Import("torch");
                np = Py.Import("numpy");

                // print if and which Cuda is available
                Console.WriteLine($"Cuda version: {torch.version.cuda}, Cuda available: {torch.cuda.is_available()} used in Block {this.Name}");

                // Load the model
                model = torch.jit.load(modelPath);
                model.eval();
            }
            // sets the previous inputs block desired rate as this blocks desired rate
            DesiredRate = InputBlocks[0].DesiredRate;
        }

     
        /// <summary>
        /// Whenever there is a new sample, it is converted to a numpy array and then torch tensor. 
        /// Afterwards, the prediction is run and the output casted back to a c# double. The output is a vector with a single element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        override protected void OnNewInput(Block sender, object value)
        {
            // input vector
            Vector inputVector = value as Vector;
            
            // get the double array inside the vector
            double[] inputData = inputVector.AsArray();
            
            // new prediction
            double[] prediction;

            using (Py.GIL())
            {
                // Convert input to numpy, then to torch tensor
                dynamic input_np = np.array(inputData, dtype: np.float32);
                dynamic input_tensor = torch.from_numpy(input_np).unsqueeze(0);  // shape: [1, N]

                // Run prediction
                dynamic output = model(input_tensor);

                // Detach, convert to numpy, then to double
                dynamic output_np = output.detach().numpy();

                // the double [0] indexing only works here because output is a single np.float inside a tensor => needs to be adapted for other shapes
                prediction = new double[] { (double)output_np[0][0] }; ;
            }

            // send output to next block
            SendOutput(Vector.Build.DenseOfArray(prediction));
        }
    }
    #endregion
}
