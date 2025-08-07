using System;
using System.Collections.Generic;

using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace MosaicLibary
{
    #region PredictingMachine
    /// <summary>
    /// Represents an abstract base class for a predicting machine. 
    /// This class provides the structure for building models that can make predictions
    /// and evaluate confidence in those predictions.
    /// </summary>
    public abstract class PredictingMachine
    {
        /// <summary>
        /// An object used to enforce thread safety for operations on the model.
        /// This lock ensures that predictions cannot be made while the model is being updated or rebuilt.
        /// </summary>
        private protected object _pmLock = new object();

        /// <summary>
        /// The dimensionality of the input space (R^_d).
        /// This specifies the number of features or variables in the input data.
        /// </summary>
        private protected int _d;

        /// <summary>
        /// The dimensionality of the output space (R^_M).
        /// This specifies the number of outputs or predictions the machine produces.
        /// </summary>
        private protected int _M;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredictingMachine"/> class.
        /// </summary>
        /// <param name="d">The number of dimensions in the input space.</param>
        /// <param name="M">The number of dimensions in the output space.</param>
        public PredictingMachine(int d, int M) 
        { 
            this._d = d; 
            this._M = M; 
        }

        /// <summary>
        /// Resets the internal model of the predicting machine.
        /// This method should clear any learned parameters or state, preparing the machine
        /// for new data or a fresh start.
        /// </summary>
        abstract public void ResetModel();

        /// <summary>
        /// Predicts an output based on the given input vector.
        /// </summary>
        /// <param name="x">The input vector, which must match the dimensionality of the input space (R^_d).</param>
        /// <returns>A vector representing the predicted output in the output space (R^_M).</returns>
        abstract public Vector Predict(Vector x);
        
        /// <summary>
        /// Evaluates the confidence of a prediction for a given input vector.
        /// </summary>
        /// <param name="x">The input vector, which must match the dimensionality of the input space (R^_d).</param>
        /// <returns>A double value representing the confidence in the prediction,
        /// typically normalized between 0 and 1 or represented as a probability.</returns>
        abstract public double Confidence(Vector x);
    }
    #endregion

    #region BatchLearningMachine
    /// <summary>
    /// Represents an abstract base class for a batch learning machine.
    /// This class extends the capabilities of the <see cref="PredictingMachine"/> 
    /// by allowing batch processing of multiple samples and a one-shot model building process.
    /// </summary>
    public abstract class BatchLearningMachine : PredictingMachine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchLearningMachine"/> class.
        /// </summary>
        /// <param name="d">The number of dimensions in the input space (R^d).</param>
        /// <param name="M">The number of dimensions in the output space (R^M).</param>
        public BatchLearningMachine(int d, int M) : base(d, M) { }

        /// <summary>
        /// Predicts outputs for a batch of input samples.
        /// </summary>
        /// <param name="X">A matrix where each row represents an input sample, and each column corresponds to a feature.
        /// The number of columns must match the input dimensionality (d).</param>
        /// <returns>A matrix where each row represents the predicted output for the corresponding input sample,
        /// and each column corresponds to an output dimension (M).</returns>
        abstract public Matrix Predict(Matrix X);
        /// <summary>
        /// Builds the predictive model using a batch of input-output samples.
        /// This method constructs the model in a one-shot process, which means the model is trained 
        /// with the provided data and cannot be incrementally updated afterward.
        /// </summary>
        /// <param name="X">A matrix where each row represents an input sample, and each column corresponds to a feature.
        /// The number of columns must match the input dimensionality (d).</param>
        /// <param name="Y">A matrix where each row represents the corresponding output for the input sample in <paramref name="X"/>.
        /// The number of columns must match the output dimensionality (M).</param>
        abstract public void BuildModel(Matrix X, Matrix Y);
    }
    #endregion

    #region IncrementalLearningMachine
    /// <summary>
    /// Represents an abstract base class for an incremental learning machine.
    /// This class extends the <see cref="PredictingMachine"/> to allow models that can be updated or downgraded incrementally
    /// as new data points are provided or removed.
    /// </summary>
    public abstract class IncrementalLearningMachine : PredictingMachine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalLearningMachine"/> class.
        /// </summary>
        /// <param name="d">The number of dimensions in the input space (R^d).</param>
        /// <param name="M">The number of dimensions in the output space (R^M).</param>
        public IncrementalLearningMachine(int d, int M) : base(d, M) { }

        /// <summary>
        /// Updates the model with a new input-output pair.
        /// This allows the model to adapt incrementally as new data becomes available.
        /// </summary>
        /// <param name="x">The input vector representing a single data point in the input space (R^d).</param>
        /// <param name="y">The output vector representing the corresponding target in the output space (R^M).</param>
        abstract public void UpdateModel(Vector x, Vector y);

        /// <summary>
        /// Downgrades the model by removing the influence of a specific input-output pair.
        /// This is useful for scenarios where a previously learned data point is no longer valid or relevant.
        /// </summary>
        /// <param name="x">The input vector representing the data point to be removed from the model.</param>
        /// <param name="y">The output vector representing the target associated with the input vector.</param>
        abstract public void DowndateModel(Vector x, Vector y);

        /// <summary>
        /// Retrieves the internal state of the model as a list of objects.
        /// This is useful for saving the model state or for external inspection and analysis.
        /// </summary>
        /// <returns>A list of objects representing the internal state of the model.</returns>
        abstract public List<object> GetModel();

        /// <summary>
        /// Sets the internal state of the model using a list of objects.
        /// This allows for loading a previously saved model or restoring its state.
        /// </summary>
        /// <param name="value">A list of objects representing the internal state of the model to be restored.</param>
        abstract public void SetModel(List<object> value);
    }
    #endregion

    #region RidgeRegression

    /// <summary>
    /// Implements Ridge Regression, a batch learning algorithm based on Tikhonov-regularized Least Squares.
    /// This approach is particularly effective for reducing overfitting by introducing a regularization term.
    /// Setting the regularisation coefficient lambda at zero falls back onto standard (unregularised) Least-Squares Regression.
    /// </summary>
    public class RidgeRegression : BatchLearningMachine
    {
        /// <summary>
        /// The weight matrix, which represents the learned model.
        /// For predictions:
        /// <list type="bullet">
        /// <item><description>For a single sample x: ŷ = W^T x</description></item>
        /// <item><description>For a batch of samples X: Ŷ = X W</description></item>
        /// </list>
        /// </summary>
        Matrix W;
        /// <summary>
        /// The identity matrix of size d (input space dimensionality).
        /// Used for regularization.
        /// </summary>
        Matrix I_d;
        /// <summary>
        /// The Moore-Penrose pseudoinverse of X, precomputed for evaluating prediction confidence.
        /// </summary>
        Matrix Ainv;
        /// <summary>
        /// The regularization coefficient. Higher values impose stronger regularization.
        /// Default is 1.0, and setting it to 0 reverts to unregularized Least Squares Regression.
        /// </summary>
        double lambda;

        /// <summary>
        /// Constructs a Ridge Regression model with specified dimensions and regularization coefficient.
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="lambda">The regularization coefficient.</param>
        public RidgeRegression(int d, int M, double lambda) : base(d, M) 
        { 
            Initialise(d, M, lambda); 
        }

        /// <summary>
        /// Constructs a Ridge Regression model from a YAML configuration file's hyperparameters.
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="HyperParams">An array of strings containing hyperparameters. Expects a single parameter for lambda.</param>
        public RidgeRegression(int d, int M, string[] HyperParams) : base(d, M)
        {
            if (HyperParams == Array.Empty<string>())
                Initialise(d, M);
            else
            {
                // expect HyperParams to contain [lambda]
                lambda = Convert.ToDouble(HyperParams[0]);
                Initialise(d, M, lambda);
            }
        }


        /// <summary>
        /// Initializes the Ridge Regression model, including setting the regularization coefficient and creating the identity matrix.
        /// </summary>
        /// <param name="d">The input space dimensionality.</param>
        /// <param name="M">The output space dimensionality.</param>
        /// <param name="lambda">The regularization coefficient. Defaults to 1.0.</param>
        public void Initialise(int d, int M, double lambda = 1.0)
        {
            this.lambda = lambda;

            I_d = Matrix.Build.DiagonalIdentity(d);

            ResetModel();
        }

        /// <summary>
        /// Builds the model by computing the weight matrix W using the Ridge Regression formula:
        /// W = (XᵀX + λI)⁻¹XᵀY
        /// </summary>
        /// <param name="X">The input matrix (features).</param>
        /// <param name="Y">The output matrix (targets).</param>
        override public void BuildModel(Matrix X, Matrix Y)
        {
            lock (_pmLock)
            {
                // build W out of X and Y. standard equation of Ridge Regression.
                W = (X.Transpose() * X + lambda * I_d).Inverse() * (X.Transpose() * Y);
                // also evaluate Ainv once and for all (is then used often to evaluate the confidence)
                Ainv = (X.Transpose() * X + lambda * I_d).Inverse();
            }
        }

        /// <summary>
        /// Resets the model by clearing the weight matrix <see cref="W"/> and the pseudoinverse <see cref="Ainv"/>.
        /// </summary>
        public override void ResetModel()
        {
            lock (_pmLock)
            {
                // reset W and Ainv
                W = Matrix.Build.Dense(_d, _M);
                Ainv = Matrix.Build.Dense(_d, _d);
            }
        }

        /// <summary>
        /// Predicts the output for a single input vector using the learned model.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The predicted output vector.</returns>
        override public Vector Predict(Vector x) 
        { 
            lock (_pmLock) 
                return W.Transpose() * x; 
        }

        /// <summary>
        /// Predicts the output for a batch of input samples using the learned model.
        /// </summary>
        /// <param name="X">The input matrix (each row represents a sample).</param>
        /// <returns>The output matrix (each row corresponds to a predicted output).</returns>
        override public Matrix Predict(Matrix X) 
        { 
            lock (_pmLock) 
                return X * W; 
        }

        /// <summary>
        /// Evaluates the confidence of a prediction for a single input vector.
        /// The confidence is computed using the formula: C = xᵀAinvx
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The confidence value for the prediction.</returns>
        override public double Confidence(Vector x) 
        { 
            lock (_pmLock) 
                return (x.ToRowMatrix() * Ainv * x.ToColumnMatrix()).At(0, 0); 
        }
    }
    #endregion

    #region RR_RFF

    /// <summary>
    /// Ridge Regression with Random Fourier Features (RR_RFF).
    /// This approach maps input samples into a high-dimensional feature space via phi(x),
    /// where phi(x) uses Random Fourier Features (RFF) to approximate a Radial Basis Function (RBF) kernel.
    /// As the number of features (D) increases, this method approximates Least-Squares Support Vector Machines (LS-SVM) with an RBF kernel.
    /// </summary>
    public class RR_RFF : BatchLearningMachine
    {
        /// <summary>
        /// The matrix of random Fourier feature weights (Ω), drawn from a normal distribution with mean 0 and standard deviation sigma.
        /// </summary>
        private Matrix Omega;

        /// <summary>
        /// The vector of random bias terms (β), drawn from a uniform distribution between -π and π.
        /// </summary>
        private Vector beta;

        /// <summary>
        /// The Ridge Regression instance used to operate on the transformed feature space (RFF space).
        /// </summary>
        private RidgeRegression RR;

        /// <summary>
        /// Constructs the RR_RFF model with explicit hyperparameters.
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="lambda">The regularization coefficient for Ridge Regression.</param>
        /// <param name="sigma">The standard deviation of the RBF kernel (used for generating random features).</param>
        /// <param name="D">The number of random Fourier features (higher D results in better approximation).</param>
        public RR_RFF(int d, int M, double lambda = 1.0, double sigma = 1.0, int D = 300) : base(d, M) 
        { 
            Initialise(d, M, lambda, sigma, D); 
        }

        /// <summary>
        /// Constructs the RR_RFF model from a YAML configuration file.
        /// Expects hyperparameters [lambda, sigma, D] in the configuration file.
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="HyperParams">An array of strings representing hyperparameters: [lambda, sigma, D].</param>
        public RR_RFF(int d, int M, string[] HyperParams) : base(d, M)
        {
            if (HyperParams == Array.Empty<string>())
                Initialise(d, M);
            else
            {
                // expect HyperParams to contain [lambda,sigma,D]
                double lambda = Convert.ToDouble(HyperParams[0]);
                double sigma = Convert.ToDouble(HyperParams[1]);
                int D = Convert.ToInt32(HyperParams[2]);
                Initialise(d, M, lambda, sigma, D);
            }
        }

        /// <summary>
        /// Initializes the RR_RFF model, including the Ridge Regression instance and the random Fourier features.
        /// </summary>
        /// <param name="d">The input space dimensionality.</param>
        /// <param name="M">The output space dimensionality.</param>
        /// <param name="lambda">The regularization coefficient for Ridge Regression.</param>
        /// <param name="sigma">The standard deviation of the RBF kernel (used for generating random features).</param>
        /// <param name="D">The number of random Fourier features.</param>
        public void Initialise(int d, int M, double lambda = 1.0, double sigma = 1.0, int D = 300)
        {
            // initialise my Ridge Regression machine - it works in a D-dimensional space, of course, not in _d dimensions.
            RR = new RidgeRegression(D, M, lambda);

            // RR_RFF have two hyperparamters:
            // - sigma, corresponding to the stdv of the RBF kernel to be approximated and
            // - D, the desired number of RFF (the larger D, the better the RBF approximation)
            // the Omega are drawn from a normal distribution N(0,sigma)
            Omega = Matrix.Build.Random(D, d, new Normal(0.0, sigma));
            // while beta is drawn from a uniform distribution U(-pi,pi)
            beta = Vector.Build.Random(D, new ContinuousUniform(-Math.PI, Math.PI));
        }

        /// <summary>
        /// Builds the model by mapping input samples into the RFF space and applying Ridge Regression.
        /// </summary>
        /// <param name="X">The input matrix (features).</param>
        /// <param name="Y">The output matrix (targets).</param>
        override public void BuildModel(Matrix X, Matrix Y) 
        { 
            RR.BuildModel(phi(X), Y); 
        }

        /// <summary>
        /// Resets the Ridge Regression model.
        /// </summary>
        public override void ResetModel() 
        { 
            RR.ResetModel(); 
        }

        /// <summary>
        /// Predicts the output for a single input vector by first mapping it to the RFF space.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The predicted output vector.</returns>
        override public Vector Predict(Vector x) 
        { 
            return RR.Predict(phi(x)); 
        }

        /// <summary>
        /// Predicts the output for a batch of input samples by first mapping them to the RFF space.
        /// </summary>
        /// <param name="X">The input matrix (each row represents a sample).</param>
        /// <returns>The output matrix (each row corresponds to a predicted output).</returns>
        override public Matrix Predict(Matrix X) 
        { 
            return RR.Predict(phi(X)); 
        }

        /// <summary>
        /// Evaluates the confidence of a prediction for a single input vector.
        /// Confidence is computed in the RFF space.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The confidence value for the prediction.</returns>
        override public double Confidence(Vector x) 
        { 
            return RR.Confidence(phi(x)); 
        }

        /// <summary>
        /// Maps a single input vector to the RFF space using the transformation:
        /// phi(x) = cos(Omega * x + beta)
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The transformed vector in the RFF space.</returns>
        private Vector phi(Vector x)
        {
            // phi for a single vector, slightly simpler than the overload for matrices.
            // could use the matrix overload here too, but this is more elegant :-)

            // return cos(Omega*x + beta)
            return (Omega * x + beta).PointwiseCos();
        }

        /// <summary>
        /// Maps a batch of input samples to the RFF space using the transformation:
        /// phi(X) = cos(Omega * Xᵀ + Beta), where Beta consists of stacked copies of β.
        /// </summary>
        /// <param name="X">The input matrix (features).</param>
        /// <returns>The transformed matrix in the RFF space.</returns>
        private Matrix phi(Matrix X)
        {
            // phi for matrices of samples - need some easy matrix algebra first
            int N = X.RowCount;

            // (the matrix) Beta consists of N stacked copies of (the vector) beta
            Matrix Beta = Matrix.Build.DenseOfRowVectors(System.Linq.Enumerable.Repeat(beta, N));

            // return cos(Omega*X + Beta)
            return (X * Omega.Transpose() + Beta).PointwiseCos();
        }
    }
    #endregion

    #region IncrementalRidgeRegression

    // -----------------------------------------------------------------------------------------
    // Incremental Ridge Regression. Uses the Sherman-Morrison formula as a rank-1-update method for Ridge Regression.
    // y_hat = W x, where W =def Ainv B
    // 
    //   t=0:
    //           Ainv = (1/lambda) I
    //           B = 0
    // t=t+1:
    //           Ainv = Ainv - (Ainv x x' Ainv) / (1 + x' Ainv x)
    //           B = B + x'y
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Implements Incremental Ridge Regression (IRR), an online version of Ridge Regression.
    /// </summary>
    /// <remarks>
    /// This algorithm uses the Sherman-Morrison formula to efficiently perform rank-1 updates to the model, 
    /// allowing for incremental learning without rebuilding the model from scratch for each new data point.
    /// The regression model is represented as:
    /// <code>
    /// W = Ainv * B
    /// </code>
    /// Where:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>Ainv</c> is the pseudo-inverse of the feature matrix (updated incrementally).</description>
    ///   </item>
    ///   <item>
    ///     <description><c>B</c> accumulates the outer product of inputs and targets.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public class IncrementalRidgeRegression : IncrementalLearningMachine
    {
        /// <summary>
        /// The weight matrix (W), used for predictions.
        /// </summary>
        private Matrix W;

        /// <summary>
        /// The pseudo-inverse of the feature matrix (Ainv), updated incrementally.
        /// </summary>
        private Matrix Ainv;

        /// <summary>
        /// The matrix B, which accumulates the outer product of inputs and targets.
        /// </summary>
        private Matrix B;

        /// <summary>
        /// The regularization parameter, which controls the Ridge Regression penalty.
        /// </summary>
        private double lambda;

        /// <summary>
        /// Constructor for IRR, specifying the dimensionalities and regularization parameter directly.
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="lambda">The regularization parameter (default is 1.0).</param>
        public IncrementalRidgeRegression(int d, int M, double lambda = 1.0) : base(d, M) 
        { 
            Initialise(d, M, lambda); 
        }

        /// <summary>
        /// Constructor for IRR, parsing hyperparameters from a YAML configuration file.
        /// Expects hyperparameters [lambda].
        /// </summary>
        /// <param name="d">The input space dimensionality (number of features).</param>
        /// <param name="M">The output space dimensionality (number of targets).</param>
        /// <param name="HyperParams">An array of strings representing hyperparameters [lambda].</param>
        public IncrementalRidgeRegression(int d, int M, string[] HyperParams) : base(d, M)
        {
            if (HyperParams == Array.Empty<string>())
                Initialise(d, M);
            else
            {
                // expect HyperParams to contain [lambda]
                lambda = Convert.ToDouble(HyperParams[0]);
                Initialise(d, M, lambda);
            }
        }

        /// <summary>
        /// Initializes the <see cref="IncrementalRidgeRegression"/> model by setting the regularization parameter and resetting the model.
        /// </summary>
        /// <param name="d">The input space dimensionality.</param>
        /// <param name="M">The output space dimensionality.</param>
        /// <param name="lambda">The regularization parameter (default is 1.0).</param>
        public void Initialise(int d, int M, double lambda = 1.0) 
        { 
            this.lambda = lambda; 
            ResetModel(); 
        }

        /// <summary>
        /// Incrementally updates the model with a new data point using the Sherman-Morrison formula.
        /// </summary>
        /// <param name="x">The input vector for the new data point.</param>
        /// <param name="y">The target vector for the new data point.</param>
        override public void UpdateModel(Vector x, Vector y)
        {
            Matrix _x = x.ToColumnMatrix();
            Matrix _y = y.ToColumnMatrix();

            // pure equations of the Sherman/Morrison formula. pure delight.
            lock (_pmLock)
            {
                Ainv = Ainv - 1 / (1 + (_x.Transpose() * Ainv * _x).At(0, 0)) * (Ainv * _x * _x.Transpose() * Ainv);
                B = B + _x * _y.Transpose();
                W = Ainv * B;
            }
        }

        /// <summary>
        /// Throws an exception, as downdating (removing data points from the model) is not yet implemented.
        /// </summary>
        /// <param name="x">The input vector for the data point to remove.</param>
        /// <param name="y">The target vector for the data point to remove.</param>
        override public void DowndateModel(Vector x, Vector y) 
        { 
            throw new Exception("Incremental Ridge Regression: downdating not yet implemented."); 
        }

        /// <summary>
        /// Resets the <see cref="IncrementalRidgeRegression"/> model by initializing <see cref="Ainv"/> and <see cref="B"/>.
        /// </summary>
        public override void ResetModel()
        {
            SetModel(new List<object>() {
                1 / lambda * Matrix.Build.DenseIdentity(_d),
                Matrix.Build.Dense(_d, _M, 0.0)
            });
        }

        /// <summary>
        /// Retrieves the current model as a list of matrices.
        /// </summary>
        /// <returns>A list containing Ainv and B.</returns>
        override public List<object> GetModel() 
        { 
            return new List<object> { Ainv, B }; 
        }

        /// <summary>
        /// Sets the model using a list of matrices.
        /// </summary>
        /// <param name="ModelObjects">A list containing Ainv and B matrices.</param>
        override public void SetModel(List<object> ModelObjects)
        {
            lock (_pmLock)
            {
                Ainv = ModelObjects[0] as Matrix;
                B = ModelObjects[1] as Matrix;
                W = Ainv * B;
            }
        }

        /// <summary>
        /// Predicts the output for a given input vector using the current model.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The predicted output vector.</returns>
        override public Vector Predict(Vector x) 
        { 
            lock (_pmLock) 
                return W.Transpose() * x; 
        }

        /// <summary>
        /// Computes the confidence of a prediction for a given input vector.
        /// The confidence is computed as xᵀ * Ainv * x.
        /// </summary>
        /// <param name="x">The input vector.</param>
        /// <returns>The confidence value of the prediction.</returns>
        override public double Confidence(Vector x) 
        { 
            lock (_pmLock) 
                return (x.ToRowMatrix() * Ainv * x.ToColumnMatrix()).At(0, 0); 
        }
    }
    #endregion

    #region IncrementalRFF
    // -----------------------------------------------------------------------------------------
    // Incremental Ridge Regression + Random Fourier Features. Same as an incremental RR, but employs phi
    // to map x onto a higher-dimensional "feature" space, just like in batch RFF. check out those classes
    // for the meaning of the coefficients.
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// Implements Incremental Ridge Regression with Random Fourier Features (IncrementalRFF).
    /// </summary>
    /// <remarks>
    /// IncrementalRFF maps input data onto a higher-dimensional "feature" space using Random Fourier Features (RFF).
    /// This approach extends Incremental Ridge Regression (IRR) by approximating a radial basis function (RBF) kernel,
    /// enabling efficient incremental learning in a transformed feature space.
    /// </remarks>
    public class IncrementalRFF : IncrementalLearningMachine
    {
        /// <summary>
        /// Random Fourier Features transformation matrix (Omega).
        /// </summary>
        private Matrix Omega;


        private Vector beta;

        /// <summary>
        /// <see cref="IncrementalRidgeRegression"/> model used after feature transformation.
        /// </summary>
        private IncrementalRidgeRegression IRR;

        /// <summary>
        /// Initializes a new instance of IncrementalRFF with specified hyperparameters.
        /// </summary>
        /// <param name="d">Input space dimension.</param>
        /// <param name="M">Output space dimension.</param>
        /// <param name="lambda">Regularization coefficient for Ridge Regression.</param>
        /// <param name="sigma">Standard deviation of the RBF kernel approximation.</param>
        /// <param name="D">Number of Random Fourier Features (controls the quality of RBF approximation).</param>

        public IncrementalRFF(int d, int M, double lambda = 1.0, double sigma = 1.0, int D = 300) : base(d, M) 
        { 
            Initialise(d, M, lambda, sigma, D); 
        }


        /// <summary>
        /// Initializes a new instance of IncrementalRFF from YAML configuration.
        /// </summary>
        /// <param name="d">Input space dimension.</param>
        /// <param name="M">Output space dimension.</param>
        /// <param name="HyperParams">Hyperparameters as an array of strings, expected in the order: [lambda, sigma, D].</param>
        public IncrementalRFF(int d, int M, string[] HyperParams) : base(d, M)
        {
            if (HyperParams == Array.Empty<string>())
                Initialise(d, M);
            else
            {
                // expect HyperParams to contain [lambda,sigma,D]
                double lambda = Convert.ToDouble(HyperParams[0]);
                double sigma = Convert.ToDouble(HyperParams[1]);
                int D = Convert.ToInt32(HyperParams[2]);
                Initialise(d, M, lambda, sigma, D);
            }
        }

        /// <summary>
        /// Initializes the IncrementalRFF model with specified parameters.
        /// </summary>
        /// <param name="d">Input space dimension.</param>
        /// <param name="M">Output space dimension.</param>
        /// <param name="lambda">Regularization coefficient for Ridge Regression.</param>
        /// <param name="sigma">Standard deviation of the RBF kernel approximation.</param>
        /// <param name="D">Number of Random Fourier Features (controls the quality of RBF approximation).</param>
        public void Initialise(int d, int M, double lambda = 1.0, double sigma = 1.0, int D = 300)
        {
            // initialise my Ridge Regression machine
            IRR = new IncrementalRidgeRegression(D, M, lambda);

            // RR_RFF have two hyperparamters:
            // - sigma, corresponding to the stdv of the RBF kernel to be approximated and
            // - D, the desired number of RFF (the larger D, the better the RBF approximation)
            // the Omega are drawn from a normal distribution N(0,sigma)
            Omega = Matrix.Build.Random(D, d, new Normal(0.0, sigma));
            // while beta is drawn from a uniform distribution U(-pi,pi)
            beta = Vector.Build.Random(D, new ContinuousUniform(-Math.PI, Math.PI));
        }

        /// <inheritdoc/>
        public override void UpdateModel(Vector x, Vector y) 
        { 
            IRR.UpdateModel(phi(x), y); 
        }
        /// <inheritdoc/>
        override public void DowndateModel(Vector x, Vector y) 
        { 
            throw new Exception("Incremental RFF: downdating not yet implemented."); 
        }
        /// <inheritdoc/>
        public override void ResetModel() 
        { 
            IRR.ResetModel(); 
        }
        /// <inheritdoc/>
        public override Vector Predict(Vector x) 
        { 
            return IRR.Predict(phi(x)); 
        }
        /// <inheritdoc/>
        override public double Confidence(Vector x) 
        { 
            lock (_pmLock) 
                return IRR.Confidence(phi(x)); 
        }
        /// <inheritdoc/>
        override public List<object> GetModel() 
        { 
            return new List<object> { Omega, beta }; 
        }
        /// <inheritdoc/>
        override public void SetModel(List<object> ModelObjects)
        {
            lock (_pmLock)
            {
                IRR.SetModel(ModelObjects);
                Omega = ModelObjects[2] as Matrix;
                beta = ModelObjects[3] as Vector;
                
            }
        }

        /// <summary>
        /// Maps an input vector to the higher-dimensional feature space using Random Fourier Features.
        /// </summary>
        /// <param name="x">Input vector in the original space.</param>
        /// <returns>Transformed vector in the feature space.</returns>
        Vector phi(Vector x) 
        { 
            return (Omega * x + beta).PointwiseCos(); 
        }
    }
    #endregion

    #region LDA

    // <summary>
    /// Implements an Incremental Linear Discriminant Analysis (LDA) model.
    /// This class updates class statistics without full retraining.
    /// </summary>
    public class IncrementalLDA : IncrementalLearningMachine
    {
        private Dictionary<int, int> classCounts; // Track sample count per class
        private Dictionary<int, Vector> classMeans; // Store running means
        private Matrix<double> withinClassScatter; // Scatter matrix
        private int totalSamples; // Track total samples

        /// <summary>
        /// Initializes a new instance of the Incremental LDA model.
        /// </summary>
        /// <param name="d">Feature dimension.</param>
        /// <param name="M">Number of classes.</param>
        public IncrementalLDA(int d, int M, string[] HyperParams = null) : base(d, M)
        {
            classCounts = new Dictionary<int, int>();
            classMeans = new Dictionary<int, Vector>();
            withinClassScatter = Matrix.Build.Dense(d, d, 0);
            totalSamples = 0;
        }

        /// <summary>
        /// Incrementally updates LDA with a new training sample.
        /// </summary>
        /// <param name="x">Feature vector.</param>
        /// <param name="y">Class label (one-hot encoded).</param>
        public override void UpdateModel(Vector<double> x, Vector<double> y)
        {
            Console.WriteLine($"Received Input: {x}");
            Console.WriteLine($"Received Velocity Label: {y}");

            if (!classMeans.ContainsKey(0))
            {
                classMeans[0] = x;
                classCounts[0] = 1;
                Console.WriteLine("First training sample stored.");
                return;
            }

            int classCount = ++classCounts[0];
            Vector<double> prevMean = classMeans[0];

            Vector<double> newMean = prevMean + (x - prevMean) / classCount;
            classMeans[0] = newMean;

            Vector<double> diff = x - prevMean;
            double diffNorm = diff.L2Norm();
            Console.WriteLine($"Feature Difference Norm: {diffNorm}");

            if (diffNorm > 1e-6) 
            {
                withinClassScatter += diff.ToColumnMatrix() * diff.ToRowMatrix();
                Console.WriteLine($"Updated Scatter Norm: {withinClassScatter.FrobeniusNorm()}");
            }
            else
            {
                Console.WriteLine("Skipping scatter update - No feature difference detected.");
            }
        }




        /// <summary>
        /// Removes a sample's influence from the model.
        /// </summary>
        public override void DowndateModel(Vector x, Vector y)
        {
            int classLabel = y.MaximumIndex();

            if (!classCounts.ContainsKey(classLabel) || classCounts[classLabel] <= 1)
                return;

            int classCount = --classCounts[classLabel];

            // Compute new mean
            Vector prevMean = classMeans[classLabel];
            Vector newMean = (prevMean * (classCount + 1) - x) / classCount;
            classMeans[classLabel] = newMean;

            // Update within-class scatter
            Vector diff = x - prevMean;
            withinClassScatter -= diff.ToColumnMatrix() * diff.ToRowMatrix();

            totalSamples--;
        }

        /// <summary>
        /// Retrieves the model state.
        /// </summary>
        public override List<object> GetModel()
        {
            return new List<object> { classCounts, classMeans, withinClassScatter, totalSamples };
        }

        /// <summary>
        /// Restores the model state.
        /// </summary>
        public override void SetModel(List<object> value)
        {
            classCounts = (Dictionary<int, int>)value[0];
            classMeans = (Dictionary<int, Vector>)value[1];
            withinClassScatter = (Matrix)value[2];
            totalSamples = (int)value[3];
        }

        /// <summary>
        /// Performs classification on new data.
        /// </summary>
        /// <param name="x">Feature vector to classify.</param>
        /// <returns>Predicted class label.</returns>
        public override Vector<double> Predict(Vector<double> x)
        {
            if (!classMeans.ContainsKey(0))
            {
                Console.WriteLine("No trained model yet - Returning zero velocity.");
                return Vector<double>.Build.Dense(x.Count, 0); // Return zero vector if model is untrained
            }

            Vector<double> projectedX = x - classMeans[0];

            if (withinClassScatter.FrobeniusNorm() < 1e-6)  // ✅ Avoid division by zero
            {
                Console.WriteLine("Scatter matrix is not updated - Returning zero velocity.");
                return Vector<double>.Build.Dense(x.Count, 0);
            }

            var scatterPseudoInv = withinClassScatter.PseudoInverse();
            Vector<double> velocityPrediction = scatterPseudoInv * projectedX;

            Vector<double> maskedPrediction = Vector<double>.Build.Dense(x.Count, 0);
            for (int i = 0; i < x.Count; i++)
            {
                if (classCounts[0] > 1) 
                {
                    maskedPrediction[i] = velocityPrediction[i];
                }
            }

            Console.WriteLine($"Predicted Velocity (Masked): {maskedPrediction}");

            return maskedPrediction;
        }



        public override double Confidence(Vector x)
        {
            lock (_pmLock)
            {
                int bestClass = -1;
                double bestScore = double.NegativeInfinity;
                double sumExp = 0;

                foreach (var kvp in classMeans)
                {
                    int classLabel = kvp.Key;
                    double score = x.DotProduct(kvp.Value); // Compute LDA projection score

                    sumExp += Math.Exp(score);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestClass = classLabel;
                    }
                }

                return Math.Exp(bestScore) / sumExp;  // Softmax-like confidence
            }
        }

        /// <summary>
        /// Resets the IncrementalLDA model by clearing class statistics and sample count.
        /// </summary>
        public override void ResetModel()
        {
            classCounts.Clear();  // Reset class sample counts
            classMeans.Clear();   // Reset class means
            withinClassScatter = Matrix<double>.Build.Dense(this._d, this._d, 0);  // Reset scatter matrix
            totalSamples = 0;     // Reset total sample counter
        }

    }

    #endregion
}
