using MathNet.Numerics.LinearAlgebra;
using Python.Runtime;
using System;
using System.Collections.Generic;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;


// ----------------------------------------------------------------------------------------------------
// PythonNetLearning:
// This block is an example on how to utilizes a custom Python script containing various classes
// and objects leveragingthe scikit-learn library to apply online models (PCA, LDA, ICA) to input data.
// ----------------------------------------------------------------------------------------------------


namespace MosaicLibary
{
    /// <summary>
    /// Enum representing different types of models available in the Python code.
    /// </summary>
    enum Model
    {
        PCA, 
        ICA,
        LDA_transform,
        LDA_predict
    }

    public partial class cpPythonNetPCA : ControlPanel
    {
    
        PythonNetLearning _SourceBlock;
    
        public cpPythonNetPCA(PythonNetLearning _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._SourceBlock = _SourceBlock;
       
        }

        /// <summary>
        /// Fits the selected model with the specified number of components by updating the input data.
        /// </summary>
        private void Fit_Model(object sender, EventArgs e)
        {

            int N_Components = 0;
            int.TryParse(inputField_Learning.Text, out N_Components);
            if (N_Components != 0)
            {
                switch (Model_ch.SelectedItem.ToString())
                {
                    case "PCA":
                        _SourceBlock.currentModel = Model.PCA;
                        break;
                    case "LDA Predict":
                        _SourceBlock.currentModel = Model.LDA_predict;
                        break;
                    case "LDA Transform":
                        _SourceBlock.currentModel = Model.LDA_transform;
                        break;
                    case "ICA":
                        _SourceBlock.currentModel = Model.ICA;
                        break;
                    default:
                        return;
                }

                _SourceBlock.UpdateInputs(N_Components);
            }
        }

        /// <summary>
        /// Change the text inside LDA_Label: showing the predicted Label of the data in the GUI if the selected model is LDA_predict
        /// </summary>
        public void LDA_Label_Changer(string newText)
        {

            LDA_Label.Text = newText.ToString();
        }
        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            LDA_Label_Changer(_SourceBlock.Current_LDA_pred);
        }
    }

    public class PythonNetLearning : Block
    {
        internal Model currentModel;
        internal string Current_LDA_pred = string.Empty;
        private dynamic pyModel_PCA;
        private dynamic pyModel_LDA;
        private dynamic pyModel_ICA;

        // needs a connection to the buffer 
        Buffer DataStorage;


        public PythonNetLearning(string Name, double DesiredRate, List<string> InputCfg, List<string> Params, string Path)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        {
            PythonNetManager.Instance.Init();
        }

        /// <summary>
        /// Read an compile python code
        /// Get Model objects from python script
        /// </summary>
        override public void ConfigureInputs()
        {

            cp = new cpPythonNetPCA(this);
          
            base.ConfigureInputs();
            if (Params.Count < 1) throw new Exception($"Python Net {Name} needs a specified buffer block.");

            ////
            ///Example for using your own python code inside MOISAIC. Just load python file from Scripts.
            using (Py.GIL())
            {
                dynamic learn_py = Py.Import("Examples.LearningUC");
                pyModel_PCA = learn_py.OnlinePCA();
                pyModel_LDA = learn_py.OnlineLDA();
                pyModel_ICA = learn_py.OnlineICA();
            }

            // buffer block being hand over as data storage
            DataStorage = Blocks.Instance[Params[0]] as Buffer;

            // aDesiredRate is the same as the driving Timer
            DesiredRate = InputBlocks[0].DesiredRate;
        }

        /// <summary>
        /// These false bool allow to first fit the models and then use them
        /// </summary>
        private bool initialized = false;
        private bool initialized_ICA = false;
        private bool initialized_PCA = false;
        private bool initialized_LDA = false;

        /// <summary>
        /// This function changes to the selected model and fits it with the current data in Buffer
        /// </summary>
        /// <param name="N_Components"></param>
        public void UpdateInputs(int N_Components)
        {
            using (Py.GIL())
            {

                    Matrix<double> concatenatedData = null;
                    Matrix<double> concatenatedLabels = null;

                    /// Concatenate Data and Label
                    foreach (var tuple in this.DataStorage.dB)
                    {
                        Matrix<double> Data = tuple.Item2;
                        var label = tuple.Item1;

                        if (concatenatedData == null)
                        {
                            concatenatedData = Data;
                            concatenatedLabels = Matrix<double>.Build.DenseOfColumnArrays(label.ToArray());

                        }
                        else
                        {
                            concatenatedData = concatenatedData.Stack(Data);

                        }

                        foreach (var i in System.Linq.Enumerable.Range(1, tuple.Item2.RowCount)) concatenatedLabels = concatenatedLabels.Stack(tuple.Item1.ToRowMatrix());

                    }
                    concatenatedLabels = concatenatedLabels.RemoveRow(0);


                    // Concatenated Data and Label as train data to Fit to the model
                    if (concatenatedData != null)
                    {
                        if (currentModel == MosaicLibary.Model.PCA)
                        {
                            try
                            {
                                pyModel_PCA.fit(concatenatedData.ToArray(), N_Components);
                            }
                            catch (PythonException ex)
                            {
                                Console.WriteLine("Fit failed: " + ex.Format());
                            }
                            initialized_PCA = true;

                        }

                        else if (currentModel == MosaicLibary.Model.LDA_predict || currentModel == MosaicLibary.Model.LDA_transform)
                        {
                            try
                            {
                                pyModel_LDA.fit(concatenatedData.ToArray(), concatenatedLabels.ToArray(), N_Components);
                            }
                            catch (PythonException ex)
                            {
                                Console.WriteLine("Fit failed: " + ex.Format());
                            }
                            initialized_LDA = true;

                        }

                        else if (currentModel == MosaicLibary.Model.ICA)
                        {
                            try
                            {
                                pyModel_ICA.fit(concatenatedData.ToArray(), N_Components);
                            }
                            catch (PythonException ex)
                            {
                                Console.WriteLine("Fit failed: " + ex.Format());
                            }
                            initialized_ICA = true;
                        }

                    }
                    initialized = true;
                }
        }

     
        override protected void OnNewInput(Block sender, object value)
        {
           
            double[] Output = { };

            if (!initialized)
            {
                // Do nothing and return if the model is not fitted yet
                return;
            }

            using (Py.GIL())
            {

            
                    // Use the trained models to predict new label or transform data
                    double[] DATA_for_Models = (value as Vector).AsArray();

                    if (currentModel == MosaicLibary.Model.PCA & initialized_PCA == true)
                    {
                        dynamic Model_output = pyModel_PCA.transform(DATA_for_Models);
                        Output = Model_output.reshape(-1).As<double[]>();
                    }
                    else if (currentModel == MosaicLibary.Model.LDA_transform & initialized_LDA == true)
                    {
                        dynamic Model_output = pyModel_LDA.transform(DATA_for_Models);
                        
                        Output = Model_output.reshape(-1).As<double[]>();
                    }
                    else if (currentModel == MosaicLibary.Model.ICA & initialized_ICA == true) 
                    {
                        dynamic Model_output = pyModel_ICA.transform(DATA_for_Models);
                        Output = Model_output.reshape(-1).As<double[]>();
                        
                    }
                    else if (currentModel == MosaicLibary.Model.LDA_predict & initialized_LDA == true)
                    {

                        dynamic Model_output = pyModel_LDA.predict(DATA_for_Models);
                        Output = Model_output.reshape(-1).As<double[]>();
                        string Lable_to_show = Output[0].ToString();
                        Current_LDA_pred = Lable_to_show;

                    }
            }
            SendOutput(Vector.Build.DenseOfArray(Output));
        }
    }
}
