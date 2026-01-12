using Python.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Threading.Tasks;

namespace MosaicLibary
{
    // -----------------------------------------------------------------------------------------
    // Example iNMF.
    //
    // This block is a PythonNet example for a tight and more complex integration between a
    // custom python algorithm (iNMF) and MOSAIC. The integration is based on the publications
    // mentioned under remarks. 
    // -----------------------------------------------------------------------------------------
    public partial class cpiNMF : ControlPanel
    {
        iNMF _SourceBlock;

        public List<SpiderPlotMonitor> spSynergies;
        private delegate void update_smSynergies_delegate();
        private update_smSynergies_delegate update_smSynergies;
        public cpiNMF(iNMF _SourceBlock) : base(_SourceBlock)
        {
            InitializeComponent();
            this._SourceBlock = _SourceBlock;
            spSynergies = new List<SpiderPlotMonitor> { spS1, spS2, spS3, spS4, spS5, spS6 };
            update_smSynergies = update_smSynergies_not_normalized;
        }

        override protected void cpRefresh(object myObject, EventArgs myEventArgs)
        {
            if (tcScopeMonitors.SelectedIndex == 0)
            {
                smSynergyActivity.Update(_SourceBlock.Prediction);
                spInputSignal.Update(_SourceBlock.InputEMG);
                update_smSynergies();
            }
            else if (tcScopeMonitors.SelectedIndex == 1)
            {
                smInputEMG.Update(_SourceBlock.InputEMG);
                smReconstrucedEMG.Update(_SourceBlock.TransformedEMG);
                pb_reconstructionErr.Value = (int)_SourceBlock.ReconError;
            }
        }

        public void SetModelParameterInUI(float forgetFactor, int n_components, int n_features, int batch_size, float synReg, float encodingReg, float epsilon, int max_iter_fit, int max_iter_transform)
        {
            this.nudForgetFactor.Value = (decimal)forgetFactor;
            this.nudNComponents.Value = n_components;
            this.nudNFeatures.Value = n_features;
            this.nudBatchSize.Value = batch_size;
            this.nudEncodingReg.Value = (decimal)encodingReg;
            this.nudSynergyReg.Value = (decimal)synReg;
            this.nudEpsilon.Value = (decimal)(epsilon);
            this.nudMaxIterFit.Value = max_iter_fit;
            this.nudMaxIterTransform.Value = max_iter_transform;
        }

        private void btnAddComponent_Click(object sender, EventArgs e)
        {
            _SourceBlock.AddOneComponentToTheModel();
            this.nudNComponents.Value = ((iNMF)this._SourceBlock).n_components;
        }

        private void btnCreateModel_Click(object sender, EventArgs e)
        {
            // read parameter
            float forgetFactor = float.Parse(nudForgetFactor.Text);
            int n_components = int.Parse(nudNComponents.Text);
            int n_features = int.Parse(nudNFeatures.Text);
            int batch_size = int.Parse(nudBatchSize.Text);
            float synergyReg = float.Parse(nudSynergyReg.Text);
            float encodingReg = float.Parse(nudEncodingReg.Text);
            float epsilon = float.Parse(nudEpsilon.Text);
            int max_iter_fit = int.Parse(nudMaxIterFit.Text);
            int max_iter_transform = int.Parse(nudMaxIterTransform.Text);

            // create model
            this._SourceBlock.CreateModel(forgetFactor, n_components, n_features, batch_size, synergyReg, encodingReg, epsilon, max_iter_fit, max_iter_transform);
        }

        private void btnGetSynergies_Click(object sender, EventArgs e)
        {
            var synergies = _SourceBlock.GetSynergies();

            // user selects the destination file
            SaveFileDialog _sfd = new SaveFileDialog { Title = $"Save data stored in {_SourceBlock.Name}", RestoreDirectory = true };
            StreamWriter streamWriter;

            // write file
            if (_sfd.ShowDialog() == DialogResult.OK)
            {
                streamWriter = new StreamWriter(_sfd.FileName);

                foreach (var synergy in synergies)
                {
                    foreach (var value in synergy)
                        streamWriter.Write(value + ",");
                    streamWriter.WriteLine();
                }
                streamWriter.Close();
            }
        }

        private void btnLoadModel_Click(object sender, EventArgs e)
        {
            // user selects the source file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.zip)|*.zip";

            // load file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _SourceBlock.LoadModel(openFileDialog.FileName);
            }
        }

        private void btnSaveModel_Click(object sender, EventArgs e)
        {
            // user selects the destination file
            SaveFileDialog saveFileDialog = new SaveFileDialog { Title = $"Save your model in a file ", RestoreDirectory = true };
            saveFileDialog.Filter = "JSON files (*.zip)|*.zip";

            // write file
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _SourceBlock.SaveModel(saveFileDialog.FileName);
            }
        }

        private void cb_normalizeSynergies_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_normalizeSynergies.Checked) update_smSynergies = update_smSynergies_normalized;
            else update_smSynergies = update_smSynergies_not_normalized;
        }

        private void bt_resetSynergy_Click(object sender, EventArgs e)
        {
            try
            {
                int synergy_index = int.Parse(cb_resetSynergyX.SelectedItem.ToString()) - 1;
                _SourceBlock.ResetSynergy(synergy_index);
                update_smSynergies();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void update_smSynergies_normalized()
        {
            for (int i = 0; i < _SourceBlock.Synergies.Count && i < this.spSynergies.Count; i++)
            {
                double max = _SourceBlock.Synergies[i].Maximum();
                if (max > (10 ^ -4)) // if the synergy is not zero, scale it to a range between 0 and 1
                    spSynergies[i].Update(_SourceBlock.Synergies[i] / max);
            }
        }
        private void update_smSynergies_not_normalized()
        {
            for (int i = 0; i < _SourceBlock.Synergies.Count && i < this.spSynergies.Count; i++)
            {
                spSynergies[i].Update(_SourceBlock.Synergies[i]);
            }
        }

        private void cb_KeepUdating_CheckedChanged(object sender, EventArgs e)
        {
            _SourceBlock.KeepUpdating = cb_KeepUdating.Checked;
        }

        private void nudEncodingReg_ValueChanged(object sender, EventArgs e)
        {
            _SourceBlock.SetEncodingReg((double)nudEncodingReg.Value);
        }

        private void nudSynergyReg_ValueChanged(object sender, EventArgs e)
        {
            _SourceBlock.SetSynReg((double)nudSynergyReg.Value);
        }

        private void nudForgetFactor_ValueChanged(object sender, EventArgs e)
        {
            _SourceBlock.SetForgetFactor((double)nudForgetFactor.Value);
        }

        private void nudMaxIterFit_ValueChanged(object sender, EventArgs e)
        {
            _SourceBlock.SetMaxIter((double)nudMaxIterFit.Value);
        }

        private void btn_resetSynergySensibility_Click(object sender, EventArgs e)
        {
            try
            {
                int synergy_index = int.Parse(cb_resetSynergySensitivty.SelectedItem.ToString()) - 1;
                _SourceBlock.ResetSynergySensitivty(synergy_index);
                update_smSynergies();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        private void nudBatchSize_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            decimal val = nud.Value;
            _SourceBlock.batchSize = (int)val;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }

    public class iNMF : Block
    {
        Block updateInput, predictionInput;

        // for visualisation purposes
        public Vector SynergyActivation;
        public Vector Prediction;
        public Vector InputEMG;
        public Vector TransformedEMG;
        public float ReconError;
        public Boolean KeepUpdating = false;
        public List<Vector> Synergies = new List<Vector>();

        //private string pythonHomePath;
        private dynamic iNMF_py;
        private dynamic pyModel_predict;
        private dynamic pyModel_update;
        private dynamic np;

        private int outputDimension;

        // model parameter
        private float forgetFactor = 0.85f;
        internal int batchSize = 100;
        public int n_components = 2;
        private int n_features = 8;
        private float epsilon = 0.0001f;
        private float synergyReg = 0f;
        private float encodingReg = 0f;
        private int max_iter_fit = 200;
        private int max_iter_transform = 200;
        private bool modelIsTrained = false;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="DesiredRate"></param>
        /// <param name="InputCfg"></param>
        /// <param name="Params"></param>
        ///     param 1 = python dll path
        ///     param 2 = output dimension (output dimension == 12 for control algorithm)
        ///     param 3 = float forget factor between 0 and 1
        ///     param 4 = int batch size
        ///     param 5 = int n_components
        ///     param 6 = pythonHomePath --> for Anaconda users only: path to the environment you wanna use
        /// <param name="Path"></param>
        public iNMF(string Name, double DesiredRate = 0, List<string> InputCfg = null, List<string> Params = null, string Path = null)
            : base(Name, DesiredRate, InputCfg, Params, Path)
        {
            if (Params.Count > 0) { outputDimension = int.Parse(Params[0]); }
            if (Params.Count > 1) { forgetFactor = float.Parse(Params[1]); }
            if (Params.Count > 2) { n_components = int.Parse(Params[2]); }
            if (Params.Count > 3) { n_features = int.Parse(Params[3]); }
            if (Params.Count > 4) { batchSize = int.Parse(Params[4]); }
            if (Params.Count > 5) { synergyReg = float.Parse(Params[5]); }
            if (Params.Count > 6) { encodingReg = float.Parse(Params[6]); }
            if (Params.Count > 7) { epsilon = float.Parse(Params[7]); }
            if (Params.Count > 8) { max_iter_fit = int.Parse(Params[8]); }
            if (Params.Count > 9) { max_iter_transform = int.Parse(Params[9]); }


            PythonNetManager.Instance.Init();
        }

        override public void ConfigureInputs()
        {
            base.ConfigureInputs();
            cp = new cpiNMF(this);
            try { updateInput = InputBlocks[0]; predictionInput = InputBlocks[1]; }
            catch { throw new Exception("The iNMF block needs two inputs. First input block provides the update batches, the second input provides the prediction samples"); }

            // import custom iNMF script
            using (Py.GIL())
            {
                iNMF_py = Py.Import("iNMF.iNMF_global_scaling");
            }

            // if all required parameters are there --> create a model automatically & set parameter in the UI
            if (Params != null)
            {
                if (Params.Count >= 10)
                {
                    CreateModel(forgetFactor, n_components, n_features, batchSize, synergyReg, encodingReg, epsilon, max_iter_fit, max_iter_transform);
                    ((cpiNMF)this.cp).SetModelParameterInUI(forgetFactor, n_components, n_features, batchSize, synergyReg, encodingReg, epsilon, max_iter_fit, max_iter_transform);
                }
            }
        }

        protected override void OnNewInput(Block sender, object value)
        {

            if (pyModel_predict != null)
            {
                if (sender.Equals(predictionInput)) // predict synergy activation 
                {
                    InputEMG = (Vector)value;
                    if (modelIsTrained)
                    {
                        ModelPredict(((Vector)value).ToArray());
                    }
                }
                else if (KeepUpdating)// update/train model
                {
                    TrainModel(((Matrix)value).ToArray());
                }
            }
        }

        public void AddOneComponentToTheModel()
        {
            pyModel_update.addComponent();
            pyModel_predict.addComponent();
            this.n_components = pyModel_predict.n_components;
        }

        public void CreateModel(float forget_factor, int n_components, int n_features, int batch_size, float synergyReg, float encodingReg, float epsilon, int max_iter_fit, int max_iter_transform)
        {
            // set class variables
            this.forgetFactor = forget_factor;
            this.n_components = n_components;
            this.n_features = n_features;
            this.batchSize = batch_size;
            this.synergyReg = synergyReg;
            this.encodingReg = encodingReg;
            this.epsilon = epsilon;
            this.max_iter_fit = max_iter_fit;
            this.max_iter_transform = max_iter_transform;

            for (int i = 0; i < this.Synergies.Count; i++)
            {
                Synergies[i] = Vector.Build.Dense(n_features);
            }

            using (Py.GIL())
            {
                try
                {
                    pyModel_update = iNMF_py.iNMF(n_components: n_components, n_features: n_features, forget_factor: forget_factor, encodingReg: encodingReg, synReg: synergyReg, max_iteration_fit: max_iter_fit, max_iteration_transform: max_iter_transform, epsilon: epsilon, batch_size: batch_size);
                    pyModel_predict = iNMF_py.iNMF(n_components: n_components, n_features: n_features, forget_factor: forget_factor, encodingReg: encodingReg, synReg: synergyReg, max_iteration_fit: max_iter_fit, max_iteration_transform: max_iter_transform, epsilon: epsilon, batch_size: batch_size);
                }
                catch (PythonException ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }


                modelIsTrained = false;

            }
        }

        public List<Vector> GetSynergies()
        {
            this.Synergies.Clear();

            using (Py.GIL())
            {
                dynamic np = Py.Import("numpy");
                dynamic components = pyModel_predict.getComponents();

                // get synergies for visualization purposes
                for (int i = 0; i < this.n_components; i++)
                {
                    float[] synergy = np.asarray(components[i]);
                    this.Synergies.Add(Vector.Build.DenseOfArray(Array.ConvertAll(synergy, x => (double)x)));
                }
            }
            return this.Synergies;
        }

        public void LoadModel(string path)
        {
            using (Py.GIL())
            {
                pyModel_update = iNMF_py.iNMF(n_components, forgetFactor, n_features, synergyReg, encodingReg, epsilon, batchSize, max_iter_fit, max_iter_transform);
                pyModel_predict = iNMF_py.iNMF(n_components, forgetFactor, n_features, synergyReg, encodingReg, epsilon, batchSize, max_iter_fit, max_iter_transform);

                // load an existing model into the object
                this.pyModel_predict = this.pyModel_predict.loadModel(path);
                this.pyModel_update = this.pyModel_update.loadModel(path);

                dynamic components = pyModel_predict.getComponents();
                for (int i = 0; i < this.n_components; i++)
                {
                    float[] synergy = np.asarray(components[i]);
                    this.Synergies.Add(Vector.Build.DenseOfArray(Array.ConvertAll(synergy, x => (double)x)));
                }
                modelIsTrained = true;
            }

            // get all model parameter
            this.forgetFactor = pyModel_predict.forget_factor;
            this.n_components = pyModel_predict.n_components;
            this.n_features = pyModel_predict.n_features;
            this.batchSize = pyModel_predict.batch_size;
            this.synergyReg = pyModel_predict.synReg; 
            this.encodingReg = pyModel_predict.encodingReg;
            this.epsilon = pyModel_predict.epsilon;
            this.max_iter_fit = pyModel_predict.max_iteration_fit;
            this.max_iter_transform = pyModel_predict.max_iteration_transform;

            // set the model parameter in the ui
            ((cpiNMF)this.cp).SetModelParameterInUI(forgetFactor, n_components, n_features, batchSize, synergyReg, encodingReg, epsilon, max_iter_fit, max_iter_transform);

        }

        public void ModelPredict(double[] input)
        {
           // Console.WriteLine("predict: " + input.Length);

            if (modelIsTrained)
            {    // preparation
                dynamic result;
                float[] prediction;
                float[] transformedData;

                // get the prediction from pyModel
                using (Py.GIL())
                {
                    dynamic np = Py.Import("numpy");
                    dynamic input_np = np.array(input);
                    result = pyModel_predict.transform(input_np, true);
                    prediction = (float[])np.asarray(result.activation);
                    transformedData = (float[])np.asarray(result.transformedData);
                }
                
                // bring the prediction output vector to the correct length
                //Vector output = Vector.Build.Dense(this.outputDimension);
                Vector output = Vector.Build.Dense(this.outputDimension + 1);

                // map predictions (very hard coded) to [th_1, th_2, i, m, r, l, fle, ext, pro, sup, ul, ra, open]
                if (prediction.Length == 3) // synergies mapped to close, open, flex
                {
                    output[0] = prediction[0];
                    output[1] = prediction[0];
                    output[2] = prediction[0];
                    output[3] = prediction[0];
                    output[4] = prediction[0];
                    output[5] = prediction[0];

                    output[12] = prediction[1]; // open

                    output[6] = prediction[2]; // flex
                }

                if(prediction.Length == 4) // synergies mapped to close, open, flex + ext
                {
                    output[0] = prediction[0];
                    output[1] = prediction[0];
                    output[2] = prediction[0];
                    output[3] = prediction[0];
                    output[4] = prediction[0];
                    output[5] = prediction[0];

                    output[12] = prediction[1]; // open

                    output[6] = prediction[2]; // flex
                    output[7] = prediction[3]; // ext
                }

                if (prediction.Length == 5) // synergies mapped to close, open, flex, ext + pro
                {
                    output[0] = prediction[0];
                    output[1] = prediction[0];
                    output[2] = prediction[0];
                    output[3] = prediction[0];
                    output[4] = prediction[0];
                    output[5] = prediction[0];

                    output[12] = prediction[1]; // open

                    output[6] = prediction[2]; // flex
                    output[7] = prediction[3]; // ext

                    output[8] = prediction[4]; // pro
                }
                if (prediction.Length == 6) // synergies mapped to close, open, flex, ext, pro + sup
                {
                    output[0] = prediction[0];
                    output[1] = prediction[0];
                    output[2] = prediction[0];
                    output[3] = prediction[0];
                    output[4] = prediction[0];
                    output[5] = prediction[0];

                    output[12] = prediction[1]; // open

                    output[6] = prediction[2]; // flex
                    output[7] = prediction[3]; // ext

                    output[8] = prediction[4]; // pro
                    output[9] = prediction[5]; // sup
                }
               
                //ALISA: 
                //for (int i = 1; i < prediction.Length + 1; i++) // start at index 1 such because DoF 0 and 1 both control the thumb and it's not that nice for visualization
                //{
                //    output[i] = prediction[i - 1];
                //}
                //for (int i = prediction.Length + 1; i >= outputDimension; i++)
                //{
                //    output[i] = 0;
                //}

                SendOutput(output);

                // set the visualization variables 
                Prediction = Vector.Build.DenseOfArray(Array.ConvertAll(prediction, x => (double)x));
                SynergyActivation = output;
                TransformedEMG = Vector.Build.DenseOfArray(Array.ConvertAll(transformedData, x => (double)x));
                ReconError = (int)result.reconErr;
            }
        }
        public void ResetSynergy(int component)
        {
            if (modelIsTrained)
            {
                Synergies.Clear();

                using (Py.GIL())
                {
                    dynamic np = Py.Import("numpy");
                    dynamic components = pyModel_predict.resetComponent(component);
                    dynamic components_update = pyModel_update.resetComponent(component);
                    for (int i = 0; i < this.n_components; i++)
                    {
                        float[] synergy = np.asarray(components[i]);
                        this.Synergies.Add(Vector.Build.DenseOfArray(Array.ConvertAll(synergy, x => (double)x)));
                    }
                }
            }
        }

        public void ResetSynergySensitivty(int component)
        {
            if (modelIsTrained)
            {
                Synergies.Clear();

                using (Py.GIL())
                {
                    dynamic components = pyModel_predict.resetComponentSensitivity(component);
                    dynamic components_update = pyModel_update.resetComponentSensitivity(component);                
                }
            }
        }
        public void SaveModel(string path)
        {
            using (Py.GIL())
            {
                pyModel_update.saveModel(path);
            }
        }

        public void SetEncodingReg(double encodReg)
        {
            pyModel_update.setEncodingReg(encodReg);
            pyModel_predict.setEncodingReg(encodReg);;
        }

        public void SetForgetFactor(double forgetFactor)
        {
            pyModel_update.setForgetFactor(forgetFactor);
        }

        public void SetSynReg(double synReg)
        {
            pyModel_update.setSynReg(synReg);
            pyModel_predict.setSynReg(synReg);
        }

        public void SetMaxIter(double maxIter)
        {
            pyModel_update.setMaxIter(maxIter);
            pyModel_predict.setMaxIter(maxIter);
        }
        private void updateModel(double[,] input)
        {
            Console.WriteLine("update: " + input.GetLength(0) + " "+ input.GetLength(1));
            Synergies.Clear();

            using (Py.GIL())
            {
                try
                {
                    // preparation
                    dynamic np = Py.Import("numpy");
                    dynamic miniBatch = np.array(input);
                    // train model
                    dynamic components = pyModel_update.partial_fit(miniBatch, this.batchSize);
                    //create folder and filepaths from datetime
                    string dateTimeFormat = "yyyyMMd_HHmmssfff";
                    string dateTime = DateTime.Now.ToString(dateTimeFormat);
                    pyModel_update.saveModel($"./../../../Models/{dateTime}_model.zip");


                    lock (pyModel_predict)
                    {
                        pyModel_predict.overwriteModel(pyModel_update);
                    }
                    // get synergies for visualization
                    for (int i = 0; i < this.n_components; i++)
                    {
                        float[] synergy = np.asarray(components[i]);
                        this.Synergies.Add(Vector.Build.DenseOfArray(Array.ConvertAll(synergy, x => (double)x)));
                    }
                }
                catch (PythonException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                
            }
            modelIsTrained = true;
        }
        public void TrainModel(double[,] input)
        {
            Task.Run(() => updateModel(input));

        }
    }
}
