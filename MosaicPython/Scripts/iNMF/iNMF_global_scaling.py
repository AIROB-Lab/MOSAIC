import numpy as np
from tdigest import TDigest
# import joblib
import zipfile
import json
import copy
import logging
import datetime
import os
import warnings
from copy import deepcopy
from typing import Any, Iterable, List, MutableMapping, Optional, Tuple

# import matplotlib.pyplot as plt
# import numpy as np
from scipy.optimize._numdiff import approx_derivative
from sklearn.decomposition._nmf import NMF, _initialize_nmf
from sklearn.metrics import explained_variance_score, mean_squared_error, r2_score

EPSILON = np.finfo(float).eps
# import importlib
# import warnings
# from copy import deepcopy
# from typing import Any, Iterable, List, MutableMapping, Optional, Tuple

# import matplotlib.pyplot as plt
# import numpy as np
# from scipy.optimize._numdiff import approx_derivative
# from sklearn.decomposition._nmf import NMF, _initialize_nmf
# from sklearn.metrics import explained_variance_score, mean_squared_error, r2_score

# import importlib
# import warnings
# from copy import deepcopy
# from typing import Any, Iterable, List, MutableMapping, Optional, Tuple

# import matplotlib.pyplot as plt
# import numpy as np

# from scipy.optimize._numdiff import approx_derivative
# from sklearn.decomposition._nmf import NMF, _initialize_nmf
# from sklearn.metrics import explained_variance_score, mean_squared_error, r2_score


def log_timestamps(filename):
    def decorator(func):
        def wrapper(*args, **kwargs):
            # Configure logging for the specific file
            logging.basicConfig(filename=filename, level=logging.INFO)

            # Log timestamp before method call
            timestamp = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
            logging.info(f"Method '{func.__name__}' started at {timestamp}")

            # Call the method
            result = func(*args, **kwargs)

            # Log timestamp after method call
            timestamp = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')
            logging.info(f"Method '{func.__name__}' finished at {timestamp}")

            return result

        return wrapper

    return decorator


class TransformationResult:
    def __init__(self, activation, transformedData, reconErr):
        self.activation = activation;
        self.transformedData = transformedData
        self.reconErr = reconErr


def approx_fprime(xk, f, epsilon, *args):
    """Finite-difference approximation of the gradient of a scalar function.

    Parameters
    ----------
    xk : array_like
        The coordinate vector at which to determine the gradient of `f`.
    f : callable
        The function of which to determine the gradient (partial derivatives).
        Should take `xk` as first argument, other arguments to `f` can be
        supplied in ``*args``. Should return a scalar, the value of the
        function at `xk`.
    epsilon : array_like
        Increment to `xk` to use for determining the function gradient.
        If a scalar, uses the same finite difference delta for all partial
        derivatives. If an array, should contain one value per element of
        `xk`.
    \\*args : args, optional
        Any other arguments that are to be passed to `f`.

    Returns
    -------
    grad : ndarray
        The partial derivatives of `f` to `xk`.

    See Also
    --------
    check_grad : Check correctness of gradient function against approx_fprime.

    Notes
    -----
    The function gradient is determined by the forward finite difference
    formula::

                 f(xk[i] + epsilon[i]) - f(xk[i])
        f'[i] = ---------------------------------
                            epsilon[i]

    The main use of `approx_fprime` is in scalar function optimizers like
    `fmin_bfgs`, to determine numerically the Jacobian of a function.

    Examples
    --------
    >>> from scipy import optimize
    >>> def func(x, c0, c1):
    ...     "Coordinate vector `x` should be an array of size two."
    ...     return c0 * x[0]**2 + c1*x[1]**2

    >>> x = np.ones(2)
    >>> c0, c1 = (1, 200)
    >>> eps = np.sqrt(np.finfo(float).eps)
    >>> optimize.approx_fprime(x, func, [eps, np.sqrt(200) * eps], c0, c1)
    array([   2.        ,  400.00004198])

    """
    xk = np.asarray(xk, float)

    f0 = f(xk, *args)
    if not np.isscalar(f0):
        try:
            f0 = f0.item()
        except (ValueError, AttributeError) as e:
            raise ValueError("The user-provided " "objective function must " "return a scalar value.") from e

    return approx_derivative(f, xk, method="2-point", abs_step=epsilon, args=args, f0=f0)


def check_zerolock_cols(W, threshold_norm_ratio=1e-2, threshold_norm=np.sqrt(np.finfo(float).eps)):
    """Euristically check if any of the bases (columns of W) is zero-locked.
    Returns boolean indicating if the zero-lock happened and a list of the zero-locked bases.

    One basis wi is considered to be zero-locked if
        - norm(wi) <= threshold_norm AND
        - norm(wi) / norm(wj) <= threshold_norm_ratio, where wj is any of the other bases.

    Note: ratio 0 / 0 is set to return 0
    """
    zero_lock_happened = False
    zero_locked_bases = []

    w_norms = np.linalg.norm(W, axis=0)
    norms_ratios = np.ones((len(w_norms), len(w_norms)))
    for i, wi in enumerate(w_norms):
        for j, wj in enumerate(w_norms):
            with np.errstate(all="ignore"):
                norms_ratios[i, j] = np.nan_to_num(np.divide(wi, wj))  # 0 / 0 returns 0

    idx_w_much_smaller_than_others = np.argwhere(norms_ratios <= threshold_norm_ratio)
    if len(idx_w_much_smaller_than_others) == 0:
        return zero_lock_happened, zero_locked_bases
    else:
        idx_w_much_smaller_than_others = np.unique([el[0] for el in idx_w_much_smaller_than_others])

    for idx in idx_w_much_smaller_than_others:
        if w_norms[idx] <= threshold_norm:
            zero_lock_happened = True
            zero_locked_bases.append(idx)

    return zero_lock_happened, zero_locked_bases


def custom_check_grad(func, grad, x0, *args, **kwargs):
    """Small modification to scipy.optimize.check_grad s.t. the difference between
    analytical and numerical gradient is normalized by their norms. A value of
    this normalized difference smaller than 1e-7 is ok.

    Check the correctness of a gradient function by comparing it against a
    (forward) finite-difference approximation of the gradient.

    Parameters
    ----------
    func : callable ``func(x0, *args)``
        Function whose derivative is to be checked.
    grad : callable ``grad(x0, *args)``
        Gradient of `func`.
    x0 : ndarray
        Points to check `grad` against forward difference approximation of grad
        using `func`.
    args : \\*args, optional
        Extra arguments passed to `func` and `grad`.
    epsilon : float, optional
        Step size used for the finite difference approximation. It defaults to
        ``sqrt(np.finfo(float).eps)``, which is approximately 1.49e-08.

    Returns
    -------
    err : float
        The square root of the sum of squares (i.e., the 2-norm) of the
        difference between ``grad(x0, *args)`` and the finite difference
        approximation of `grad` using func at the points `x0`.

    See Also
    --------
    approx_fprime

    Examples
    --------
    >>> def func(x):
    ...     return x[0]**2 - 0.5 * x[1]**3
    >>> def grad(x):
    ...     return [2 * x[0], -1.5 * x[1]**2]
    >>> from scipy.optimize import check_grad
    >>> check_grad(func, grad, [1.5, -1.5])
    2.9802322387695312e-08

    """
    step = np.sqrt(np.finfo("float").eps)
    if kwargs:
        raise ValueError("Unknown keyword arguments: %r" % (list(kwargs.keys()),))
    g = grad(x0, *args)
    gapprox = approx_fprime(x0, func, step, *args)
    return np.linalg.norm(g - gapprox, ord=2) / (np.linalg.norm(g, ord=2) + np.linalg.norm(gapprox, ord=2))


def cols_sparsity_hoyer(X, round_down_small_elements=None, multioutput="raw", treat_allzeros_as=None):
    """Compute average sparseness of the columns of X according to the Hoyer's metric.

    Hoyer's sparseness:
        * proposed in hoyer2004nonnegative
        * sparseness(x) = (sqrt(n) - l1(x) / l2(x)) / (sqrt(n) - 1) with x column of X and n = dim(x)
        * x is a vector of dim n
        * depends on ratio between L1 and L2 norm
        * equals 1 only if x contains a single non-zero components
        * equals 0 if all the components are equal

    By default the values of X are not rounded up to zero. For numerical reasons,
    however, it may be wise to round up elements when they get very small
    (it makes little sense working with elements in the order of e-100). If
    round_down_small_elements is not None, elements that are smaller than
    round_down_small_elements will be rounded down to 0.

    If multioutput=="raw" the sparsity of each column is returned; if
    multioutput=="mean" the average sparsity of all columnn is returned.

    The default sparsity for a column of zeros is 1. This can be modified by
    specifying a different value for the parameter treat_allzeros_as. If
    treat_allzeros_as is None and multioutput=="mean", then the nan columns
    will be ignored by the mean operation.
    """
    # X should be at least a column matrix
    X = np.array(X)
    if len(X.shape) == 1:
        X = X[:, None]

    # clip elements of X if required
    if round_down_small_elements is not None:
        X[X <= round_down_small_elements] = 0

    # check that treat_allzeros_as is None or a number
    assert (treat_allzeros_as is None) or (
            isinstance(treat_allzeros_as, (int, float, complex)) and not isinstance(treat_allzeros_as, bool)
    ), "The parameter treat_allzeros_as should be None or a number."

    # compute hoyer's sparsity
    n = X.shape[0]
    l1_cols = np.linalg.norm(X, ord=1, axis=0)
    l2_cols = np.linalg.norm(X, ord=2, axis=0)

    # note: in l1_l2_ratio, make sure that 0/0 --> 1
    with np.errstate(invalid="ignore"):
        l1_l2_ratio = np.nan_to_num(l1_cols / l2_cols, nan=1)

    # compute sparsity.
    sparsity = (np.sqrt(n) - l1_l2_ratio) / (np.sqrt(n) - 1)
    # Avoid round-off errors (l1_l2_ratio must be <= sqrt(n)).
    sparsity[np.abs(sparsity) < 1e-5] = 0

    # if a special treat_allzeros_as parameter has been specified, override the default value
    if treat_allzeros_as is not None:
        sparsity[[i for i, c in enumerate(X.T) if not np.any(c)]] = treat_allzeros_as

    # average over columns
    if multioutput == "mean":
        sparsity = np.nanmean(sparsity)

    return sparsity


def angle_between_cols(B_old, B_new, multioutput="mean", round_small_bases=False):
    # bases are cols of B. Returns mean_k(abs(angle)) between the k bases of
    # B_old and B_new.
    #
    # Bases with norm=0 will be considered to be 0 degrees away from each other.
    #
    # Multioutput can be "mean", which returns the mean of the absolute value
    # of each angle, or "raw" which returns each angle.
    #
    # round_small_bases can be used to avoid rounding errors. Bases that are
    # considerably (more than sqrt(eps) times) smaller than others will be set
    # to zero.

    # TODO: handle properly the case when the n_components new differs from the n_components init

    if round_small_bases:
        norm_B_old = np.linalg.norm(B_old, axis=0)
        norm_B_new = np.linalg.norm(B_new, axis=0)
        for i, (norm_b_old, norm_b_new) in enumerate(zip(norm_B_old, norm_B_old)):
            ratio = norm_b_new / norm_b_old
            if ratio < np.sqrt(np.finfo(float).eps):
                B_old[:, i] = 0
            elif ratio > 1 / np.sqrt(np.finfo(float).eps):
                B_new[:, i] = 0

        B_old[B_old < EPSILON] = 0
        B_old[B_old < EPSILON] = 0

    d = []
    for co, cn in zip(B_old.T, B_new.T):
        if (np.linalg.norm(co, 2) == 0) or (np.linalg.norm(cn, 2) == 0):
            d.append(0)
        else:
            d.append(np.arccos(np.clip(np.dot(co, cn) / (np.linalg.norm(co) * np.linalg.norm(cn)), -1, 1)))

    # d = [np.arccos(np.clip(np.dot(co, cn) / (np.linalg.norm(co) * np.linalg.norm(cn) + np.finfo(float).eps), -1, 1))
    #      for co, cn in zip(B_old.T, B_new.T)]

    d = np.abs(np.nan_to_num(d, np.pi))

    if multioutput == "mean":
        d = np.nanmean(d)

    return d


def cosine_similarity_between_cols(B_old, B_new, multioutput="mean", round_small_bases=False):
    if round_small_bases:
        norm_B_old = np.linalg.norm(B_old, axis=0)
        norm_B_new = np.linalg.norm(B_new, axis=0)
        for i, (norm_b_old, norm_b_new) in enumerate(zip(norm_B_old, norm_B_new)):
            if norm_b_new / norm_b_old < np.sqrt(np.finfo(float).eps):
                B_new[:, i] = 0

    s = [
        np.clip(np.dot(co, cn) / (np.linalg.norm(co) * np.linalg.norm(cn) + np.finfo(float).eps), -1, 1)
        for co, cn in zip(B_old.T, B_new.T)
    ]
    s = np.nan_to_num(s, 0)

    if multioutput == "mean":
        s = np.nanmean(s)

    return s


def distance_between_cols(B_old, B_new, multioutput="mean"):
    # Computes the euclidean distance between i-th column of B_old and i-th column of B_new for all i.

    d = [np.linalg.norm(co - cn) for co, cn in zip(B_old.T, B_new.T)]

    if multioutput == "mean":
        d = np.nanmean(d)

    return d


class ISNMF:
    """ Implementation of incremental sparse nonnegative matrix factorization.

    Implementation of [1]_. Decomposes a nonnegative matrix X into the product
    of two nonnegative matrices B and C. It does so incrementally and imposing
    sparsity constraints on the C matrix. See notes for complete loss function.


    UNANBIGUOUS NOTATION FOR MATRICES NAMES AND SHAPES.

    A different notation w.r.t. literature and sklearn.decomposition.NMF
    has been adopted. The aim is to eliminate ambiguity about the meaning and
    dimension of the matrices. This ambiguity arises from sklearn's notation,
    which uses different matrix names organizes samples-as-rows; conversely, in
    ISNMF literature typically uses samples-as-columns.
    Custom notation:
        X = B * C
        with
        X: input data. Shape (dim_input, n_samples). I-th column is i-th data sample.
        B: bases. Shape (dim_input, dim_latent). Columns are bases.
        C: mixing cofficients. Shape (dim_latent, n_samples). I-th column are coeffs for i-th sample.

    Comparison to notation in Dang [1]_:
        X -> V_dang, B -> W_dang, C -> H_dang.

    Comparison to notation in sklearn.decomposition.NMF:
        X_skl = W_skl * H_skl = mixing coefficients * bases
        with
        X_skl: input data. Shape (n_samples, dim_input). I-th row is i-th data sample.
        W_skl: mixing cofficients. Shape (n_samples, dim_latent). I-th row are coeffs for i-th sample.
        H_skl: bases. Shape (dim_latent, dim_input). Rows are bases.
        X -> X_skl.T, C -> W_skl.T, B -> H_skl.T

    See examples to compare decompositions obtained by ISNMF and sklearn.decomposition.NMF.

    Attributes
    ----------
    n_components : int
        Number of ISNMF components, i.e., dimension of the latent space to be
        learned by ISNMF.
    regularization : float, default=0.0
        Regularization strength.
    l1_ratio : float, default=0.5
        Ranges in [0, 1] and regulates the amount of L1 regularization on C
        and L2 regularization on B. See Notes in class' docstring for its
        detailed contribution to the loss function.
    mem : float, default=1.0
        Ranges in [0, 1] and regulates the amount of memory retained by the
        incremental updates. Values lower than one imply that older samples
        are exponentially forgotten. Typical values range in [0.95, 1.0].
    influence_new : float, default=1.0
        Ranges in [0, 1] and regulates the influence of new samples (or
        minibatches) on the incremental updates.
    init : string, default:"random"
        Initialization mode for B and C. Possible modalities are "random",
        "nndsvda", "nndsvdar". Their implementation correspond to that in
        sklearn.decomposition.NMF.
    random_state : int or None, default=None
        Random state seed, for repeatability.
    tol : float, default=0.0001
        Used in termination condition for the iterative model update.
    max_iter : int, default=200
        Used in termination condition for the iterative model update.
    clip_zerolock : float, default=0.0
        Nonnegative constant used to clip the multiplicative update increments
        of B and C, s.t. their components will not converge and get stuck to 0.
    norm_C : string or None, default=None
        Algorithm used to normalized the coefficients returned by the
        transform method (no normalization performed during model update).
        "max" and "robustmax" normalize the whole C respectively with the
        maximum or the 99th (default) percentile of C. "maxrows" and "robustmaxrows"
        normalize each row of C independently using, respectively, the
        maximum or the 99th (default) percentile of the row. Maximum and percentile
        are updated online every time the method transform() is called.
    percentile : int, default=99
        Integer between 0 and 100. Specifies which percentile is used by the
        "robustmax" and "robustmaxrows" normalization methods. Not used if the
         norm_C is None or uses max.
    verbose : bool, default=False
        If true, prints realtime stats during model update.

    Methods
    -------

    Examples
    --------
    Comparing decompositions obtained by ISNMF and sklearn.decomposition.NMF:
        if samples are the columns of X
        then X = B * C = (W_skl * H_shl).T

    Notes
    -----
    Mathematical formulation of the sparse ISNMF problem:
    .. math::
        min_{B, C} 1 / 2 * {\left\| X - B * C \right\|}_F^2 + l1_ratio * regularization / 2 * {\left\| B \right\|}_F^2 + (1 - l1_ratio) * regularization * {\left\| C \right\|}_1 \\

    References
    ----------
    .. [1] Dang, Sihang, et al. "SAR target recognition via incremental
    nonnegative matrix factorization." Remote Sensing 10.3 (2018): 374.
    """

    def __init__(
            self,
            n_components: int,
            reg_C: str = "l1",  # "l1" or "l05"
            regularization: float = 0.0,
            l1_ratio: float = 1.0,
            gamma: Optional[float] = None,
            beta: Optional[float] = None,
            scale_reg_B: bool = True,
            mem: Optional[float] = None,
            influence_new: Optional[float] = None,
            init: str = "random",
            init_history: str = "zeros",
            bnew_scaling: str = "bold",  # (for pismf) "bold" or "v"
            random_state: Optional[int] = None,
            tol: float = 0.0001,
            max_iter: int = 200,
            clip_zerolock: Optional[float] = None,
            norm_C: Optional[str] = None,
            percentile: int = 99,
            predict_before_init: bool = False,
            verbose: bool = False,
            debug: bool = False,
            debug_gradients: bool = False,
    ) -> None:
        """Class constructor.

        Parameters
        ----------
        n_components : int
        regularization : float, default=0.0
        l1_ratio : float, default=0.5
        mem : float, default=1.0
        influence_new : double, default=1.0
        init : string, default:"random"
        init_history : string, default:"zeros"
        random_state : int or None, default=None
        tol : float, default=0.0001
        max_iter : int, default=200
        clip_zerolock : nonnegative float or None, default=None
        norm_C : string or None, default=None
        percentile : int, default=99
        verbose : bool, default=False
        debug : bool, default=False
        """
        # simulation independent variables
        self._n_components = n_components
        self._reg_C = reg_C
        self._l1_ratio = l1_ratio
        self._regularization = regularization
        self._gamma: float = gamma if (gamma is not None) and (beta is not None) else regularization * l1_ratio
        self._beta: float = beta if (gamma is not None) and (beta is not None) else regularization * l1_ratio
        self._scale_reg_B = scale_reg_B
        self._mem: float = mem if mem is not None else 1
        self._influence_new: float = influence_new if influence_new is not None else 1
        self.n_old_samples: Optional[int] = None
        self._init = init
        self._init_history = init_history if init_history in ["zeros", "random"] else "zeros"
        self._tol = tol
        self._max_iter = max_iter
        self._random_state: int = random_state if random_state is not None else np.random.randint(0, 100)
        self._norm_C = norm_C if norm_C in [None, "max", "robustmax", "maxrows", "robustmaxrows"] else None
        self._prc = percentile
        self._clip_zerolock = clip_zerolock
        self._updating: bool = False
        self._predict_before_init = predict_before_init  # whether to predict random things before the model is initialized (vs. not predicting anything)
        self._random_B_before_init = None
        self._verbose = verbose
        self._debug = debug  # whether to store the debug information
        self._debug_gradients = debug_gradients
        self._debug_data: MutableMapping[str, Any] = {
            "|CCt| / |hist_CCt|": [],
            "|XCt| / |hist_XCt|": [],
            "mean_update_C": [],
            "mean_update_B": [],
            "mag_B": [],
            "evol_B": [],
            "iteration": [],
            "mean_C": [],
            "mean_update_B_num1": [],
            "mean_update_B_num2": [],
            "mean_update_B_den1": [],
            "mean_update_B_den2": [],
            "mean_update_B_den3": [],
            "mean_update_C_num": [],
            "mean_update_C_den1": [],
            "mean_update_C_den2": [],
            "update_no": [],
            "all_C": [],
            "all_X": [],
        }

        # define simulation-dependent variables
        self._initialized: bool = False
        self._bnew_scaling = bnew_scaling
        # TODO: OBSOLETE, DELETE
        self._B: Optional[np.ndarray] = None
        self._B_init: Optional[np.ndarray] = None
        self._B_init_unscaled: Optional[np.ndarray] = None
        self._B_to_reinit: Iterable = []  # list of bases to reinitialize to their initial value  # probably useless!
        self._C_init: Optional[np.ndarray] = None
        self._h_XCt: Optional[np.ndarray] = None
        self._h_CCt: Optional[np.ndarray] = None

        self._max_c_rows: Optional[np.ndarray] = None
        self._robustmax_c_rows: Optional[np.ndarray] = None
        self._max_c: float = 0
        self._robustmax_c: float = 0
        self._livepercentile: Optional[Any] = None
        if self._norm_C is not None:
            if self._norm_C == "maxrows":
                self._max_c_rows = np.zeros(self._n_components)
            elif self._norm_C == "robustmaxrows":
                self._robustmax_c_rows = np.zeros(self._n_components)
            elif self._norm_C == "max":
                self._max_c = 0
            elif self._norm_C == "robustmax":
                self._robustmax_c = 0
        self._update_no: int = 0
        self._historical_data_mean: float = 0

    def reset_model(self):
        self.__init__(
            n_components=self._n_components,
            reg_C=self._reg_C,
            regularization=self._regularization,
            l1_ratio=self._l1_ratio,
            gamma=self._gamma,
            beta=self._beta,
            scale_reg_B=self._scale_reg_B,
            mem=self._mem,
            influence_new=self._influence_new,
            init=self._init,
            init_history=self._init_history,
            bnew_scaling=self._bnew_scaling,
            random_state=self._random_state,
            tol=self._tol,
            max_iter=self._max_iter,
            clip_zerolock=self._clip_zerolock,
            norm_C=self._norm_C,
            percentile=self._prc,
            predict_before_init=False,
            verbose=self._verbose,
            debug=self._debug,
            debug_gradients=self._debug_gradients,
        )

    @property
    def B(self):
        return deepcopy(self._B)

    @B.setter
    def B(self, B):
        self._B = B

    @property
    def B_init(self):
        return deepcopy(self._B_init)

    @B_init.setter
    def B_init(self, B_init):
        self._B_init = B_init

    @property
    def B_to_reinit(self):
        return deepcopy(self._B_to_reinit)

    @B_to_reinit.setter
    def B_to_reinit(self, B_to_reinit):
        self._B_to_reinit = B_to_reinit

    @property
    def B_norm(self):
        """Computes and returns normalized version of current bases matrix.

        Returns
        -------
        B_norm : ndarray of shape (dim_input, dim_latent)

        """
        return self._B

    @property
    def beta(self):
        return deepcopy(self._beta)

    @beta.setter
    def beta(self, beta):
        self._beta = beta

    @property
    def debug_data(self):
        return self._debug_data

    @property
    def gamma(self):
        return deepcopy(self._gamma)

    @gamma.setter
    def gamma(self, gamma):
        self._gamma = gamma

    @property
    def n_components(self):
        return self._n_components

    @n_components.setter
    def n_components(self, n_components):
        self._n_components = n_components

    @property
    def regularization(self):
        return deepcopy(self._regularization)

    @regularization.setter
    def regularization(self, regularization):
        self._regularization = regularization
        self._gamma = regularization * self._l1_ratio
        self._beta = regularization * (1 - self._l1_ratio)

    @property
    def mem(self):
        return deepcopy(self._mem)

    @mem.setter
    def mem(self, mem):
        self._mem = mem

    @property
    def influence_new(self):
        return deepcopy(self._influence_new)

    @influence_new.setter
    def influence_new(self, influence_new):
        self._influence_new = influence_new

    @property
    def percentile(self):
        return deepcopy(self._prc)

    @percentile.setter
    def percentile(self, percentile):
        self._prc = percentile

    def change_n_components(self, n_components_desired: int) -> Tuple[bool, int]:
        """Adds a component to ISNMF in realtime.

        TODO
        """
        if True:
            success = False

            # if the model has not been initialized yet, just change self._n_components
            if self._B is None:
                self._n_components = n_components_desired
                success = True
                return success, self._n_components

            # do not change n_components during a model update. Not using the lock
            # because the multiplicative update does not use the lock (to allow prediction during the model update).
            if self._updating:
                print("New components cannot be added during a model update.")
                return success, self.n_components

            if n_components_desired < 1:
                print(f"Impossible to have less than 1.")
                return success, self.n_components
            # NOTE: ISNMF does not know the input dimensionality a priori, therefore the n_components is virtually not
            # upper-bounded.

            # either add bases
            if n_components_desired > self._n_components:
                # retrieve new bases from self._B_init_for_later_use (they have
                # been initialized all at once during the first model update,
                # to guarantee consistency between B_init of ISNMF and
                # progressive ISNMF).
                assert self._B_init_unscaled is not None
                b_new = self._B_init_unscaled[:, self._n_components: n_components_desired]
                # rescale the retrived bases...
                if self._bnew_scaling == "bold":
                    # ... to match the average magnitude of the current bases
                    scaling = np.mean(np.linalg.norm(self._B, axis=0))
                    b_new = b_new / np.linalg.norm(b_new, axis=0)
                    b_new = b_new * scaling
                    # print(f"B: {np.linalg.norm(self._B, axis=0)}")
                    # print(f"B_INIT: {np.linalg.norm(self._B_init, axis=0)}")
                else:  # self._bnew_scaling == "v"
                    # ... or to match the historical average of the data observed so far.
                    # This scaling is the same used in the standard initialization (compute_B_init() with self._init == "random"))
                    scaling = np.sqrt(self._historical_data_mean / n_components_desired)
                    b_new = b_new * scaling  # replicates the scaling used in compute_B_init ("random")
                    # TODO: OBSOLETE, DELETE
                    # # ... or to match the average value of the data (divided by the n_components) in the next available minibatch
                    # # the scaling is postponed to isnmf.update(), when new data is available
                    # # self._b_new_not_initialized_halfassed = np.arange(self._n_components, n_components_desired)  # to postpone the scaling in the update function (bad implementation)
                b_new[b_new < EPSILON] = EPSILON
                assert self._B_init is not None
                self._B_init = np.hstack((self._B_init, b_new))
                self._B = np.hstack((self._B, b_new))

                # initialize history matrices
                if self._init_history == "zeros":
                    self._h_XCt = np.pad(
                        self._h_XCt, ((0, 0), (0, n_components_desired - self._n_components)), mode="constant"
                    )
                    self._h_CCt = np.pad(
                        self._h_CCt,
                        (
                        (0, n_components_desired - self._n_components), (0, n_components_desired - self._n_components)),
                        mode="constant",
                    )
                elif self._init_history == "random":
                    try:
                        desired_mean_h_XCt = (self._beta * np.mean(self._B)) / (1 - self._mem)
                        std_normal_h_XCt = desired_mean_h_XCt * np.sqrt(np.pi / 2)
                        desired_mean_h_CCt = self._beta / ((1 - self._mem) * self._n_components)
                        std_normal_h_CCt = desired_mean_h_CCt * np.sqrt(np.pi / 2)
                    except:
                        std_normal_h_XCt = 0
                        std_normal_h_CCt = 0
                        print(
                            "Problems initializing history matrices (probably invalid division). History matrices initialized to zero."
                        )
                    assert self._h_XCt is not None
                    assert self._h_CCt is not None
                    self._h_XCt = np.hstack(
                        (self._h_XCt, np.abs(np.random.standard_normal((self._h_XCt.shape[0], 1)) * std_normal_h_XCt))
                    )
                    self._h_CCt = np.hstack(
                        (self._h_CCt, np.abs(np.random.standard_normal((self._h_CCt.shape[0], 1)) * std_normal_h_CCt))
                    )
                    self._h_CCt = np.vstack(
                        (self._h_CCt, np.abs(np.random.standard_normal((1, self._h_CCt.shape[1])) * std_normal_h_CCt))
                    )

            # or remove bases
            else:
                # remove the last self._n_components - n_components_desired
                self._B = self._B[:, :n_components_desired]
                # TODO: OBSOLETE, DELETE
                # if self._bnew_scaling == "v":
                #     self._b_new_not_initialized_halfassed = []

                # remove last column/row to history matrices
                assert self._h_XCt is not None
                assert self._h_CCt is not None
                self._h_XCt = self._h_XCt[:, :n_components_desired]
                self._h_CCt = self._h_CCt[:n_components_desired, :n_components_desired]

            self._n_components = n_components_desired
            success = True

            return success, self._n_components

    def compute_B_init(self, X: np.ndarray) -> np.ndarray:
        """Initializes the matrix of bases.

        Same implementation as sklearn.decomposition._nmf._initialize_nmf(init=init)
        with init="random"/"nndsva"/"nndsvar"

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Matrix of input data.

        Returns
        -------
        B_init : ndarray of shape (dim_input, dim_latent)
            Init values for the matrix of bases.
        """
        n_features, n_samples = X.shape
        if self._init == "random":
            avg = np.sqrt(
                X.mean() / n_features
            )  # sklearn divides by n_components. Here I divide by n_features so to have uniform behavior between isnmf and pisnf.
            if avg == 0:
                avg = 1
            rng = np.random.RandomState(self._random_state)
            self._B_init_unscaled = np.abs(rng.standard_normal((n_features, n_features)).astype(X.dtype, copy=False).T)
            # self._B_init_unscaled = self._B_init_unscaled / np.linalg.norm(self._B_init_unscaled, axis=0)  # TODO: I BELIEVE THIS MAKES SENSE, BUT MUST BE TESTED PROPERLY
            B_init = avg + self._B_init_unscaled[:, : self._n_components]  # new version
            # B_init = avg * self._B_init_unscaled[:, : self._n_components] old verion
        else:
            _, self._B_init_unscaled = _initialize_nmf(
                X.T, n_components=n_features, init=self._init, random_state=self._random_state
            )
            assert self._B_init_unscaled is not None
            self._B_init_unscaled = self._B_init_unscaled.T
            B_init = self._B_init_unscaled[:, : self._n_components]

        return B_init

    def compute_C_init(self, X: np.ndarray) -> np.ndarray:
        """Initializes the matrix of mixing coefficients.

        Same implementation as sklearn.decomposition._nmf._initialize_nmf(init=init)
        with init="random"/"nndsva"/"nndsvar"

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Matrix of input data.

        Returns
        -------
        C_init : ndarray of shape (dim_latent, n_samples)
            Init values for the matrix of mixing coefficients.
        """
        # TODO: this should use self._historical_data_mean instead of the mean of the last data
        n_features, n_samples = X.shape
        if self._init == "random":
            avg = np.sqrt(
                X.mean() / n_features
            )  # sklearn divides by n_components. Here I divide by n_features so to have uniform behavior between isnmf and pisnf.
            if avg == 0:
                avg = 1
            # (re)set the state rng always in the same way
            rng = np.random.RandomState(self._random_state + 1)
            C_init = np.abs(rng.standard_normal((n_samples, self._n_components)).astype(X.dtype, copy=False).T)
            # C_init = C_init / np.linalg.norm(C_init, axis=1)[:, None]  # TODO: I BELIEVE THIS MAKES SENSE, BUT MUST BE TESTED PROPERLY
            # C_init = avg * C_init # old version
            C_init = avg + C_init  # new version
            # avoid errors due to numerical imprecisions
            C_init[C_init < EPSILON] = EPSILON
        else:
            C_init, _ = _initialize_nmf(X.T, n_components=self._n_components, init=self._init,
                                        random_state=self._random_state)
            C_init = C_init.T

        # TODO: try initializing C to abs(dot(B.T, X))

        return C_init

    def initialize_matrices(self, X: np.ndarray) -> None:
        """Initializes B, C and history matrices based on some initial training data.

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Matrix of input data.
        init_history : string
            If "zeros", initialize the history matrices to zeros. If
            "random" it initializes them so that the average init value
            of h_CCt is comparable to the regularization term at the denominator
            of MU_W, while the average init value of h_XCt is comparable to the
            init value of h_CCt.
        """
        if True:
            # init bases
            B = self.compute_B_init(X)
            self._B_init = B
            self._B = B

            # initialize history matrices
            if self._init_history == "zeros":
                self._h_XCt = np.zeros((X.shape[0], self._n_components))
                self._h_CCt = np.zeros((self._n_components, self._n_components))
            elif self._init_history == "random":
                try:
                    desired_mean_h_XCt = (self._beta * np.mean(B)) / (1 - self._mem)
                    std_normal_h_XCt = desired_mean_h_XCt * np.sqrt(np.pi / 2)
                    desired_mean_h_CCt = self._beta / ((1 - self._mem) * self._n_components)
                    std_normal_h_CCt = desired_mean_h_CCt * np.sqrt(np.pi / 2)
                except:
                    std_normal_h_XCt = 0
                    std_normal_h_CCt = 0
                    print(
                        "Problems initializing history matrices (probably invalid division). History matrices initialized to zero."
                    )
                self._h_XCt = np.abs(np.random.standard_normal((X.shape[0], self._n_components)) * std_normal_h_XCt)
                self._h_CCt = np.abs(
                    np.random.standard_normal((self._n_components, self._n_components)) * std_normal_h_CCt)

            self._initialized = True

    def update(self, X: np.ndarray, n_samples_init: Optional[int] = None) -> Optional[np.ndarray]:
        """Updates the ISNMF model with new data.

        Updates the matrices B and C based on new data and history matrices.
        Prior to the very first update the model is initialized with a
        pseudo-random strategy based on all the provided data or a portion of
        it, depending on n_samples_init.

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Input data.
        n_samples_init : int or None, default=None
            Number of samples in X to use to initialize the model. Must be less
            or equal than n_samples and preferably greater than 2 * dim_latent
            (2 * self.n_coefficients). Default=None corresponds to initializing
            matrices (and batch updating) on all the provided samples.

        Returns
        -------
        B : ndarray of shape (dim_input, dim_latent)
            Updated bases.
        """
        # check that X is nonnegative
        if np.any(X < 0):
            print("ISNMF model cannot be updated with negative data X.")
            return None

        # initialize matrices the first time
        if not self._initialized:
            # by default, initialize on all data batch X
            if n_samples_init is None:
                n_samples_init = X.shape[1]
                # warn if initializing on too few samples (less than 2 * n_components)
                if X.shape[1] < 2 * self._n_components:
                    self._init = "random"
                    warnings.warn(
                        f"WARNING: ISNMF should be initialized on at least 2 * n_components samples, "
                        f"but only {X.shape[1]} samples were provided. Matrices initialized with 'random' criterion."
                    )
            else:
                # alternatively, initialize on custom n_samples_init provided
                # that they are at least 2 * n_components
                if n_samples_init < 2 * self._n_components:
                    self._init = "random"
                    warnings.warn(
                        f"WARNING: ISNMF should be initialized on at least 2 * n_components samples, "
                        f"but a lower desired n_samples_init was provided. Matrices initialized with 'random' criterion."
                    )

            # init B, (C), and history matrices
            self.initialize_matrices(X[:, :n_samples_init])

            # perform first update on the desired number of samples
            B, _ = self.multiplicative_update(X[:, :n_samples_init])

            # update the mean value of the data observed so far
            # TODO: move this before the multiplicative update, then use the historical mean to initialize H
            self.update_historical_data_mean(np.mean(X[:, :n_samples_init]))

            # discard the samples used for the initial training
            X = X[:, n_samples_init:]

            # if all the available samples were used for the first update, return...
            if X.size == 0:
                return B

        # TODO: OBSOLETE, DELETE
        # # if some bases have just been added mid-experiment (PISNMF), scale them so to match the magnitude of X
        # if self._bnew_scaling == "v" and len(self._b_new_not_initialized_halfassed) != 0:
        #     with self._lock:
        #         avg = np.sqrt(X.mean() / self.n_components)
        #         self._B_init[:, self._b_new_not_initialized_halfassed] = self._B_init[:, self._b_new_not_initialized_halfassed] * avg
        #         self._B[:, self._b_new_not_initialized_halfassed] = self._B_init[:, self._b_new_not_initialized_halfassed]
        #         # print(f"B_INIT: {np.linalg.norm(self._B_init, axis=0)}")
        #         # print(f"B: {np.linalg.norm(self._B, axis=0)}")
        #         self._b_new_not_initialized_halfassed = []

        # if matrices are initialized and there are (still) samples in X, perform incremental update on unused samples
        self._updating = True

        B, _ = self.multiplicative_update(X)

        # update historical data mean
        self.update_historical_data_mean(np.mean(X))

        self._updating = False

        return B

    def multiplicative_update(self, X: np.ndarray, partial_mu: bool = False) -> Tuple[np.ndarray, np.ndarray]:
        """Incremental multiplicative update for ISNMF.

        Implements MU rules to solve the optimization problem defined in the
        Notes of this class' docstring.

        Implements both batch and incremental updates.
        A batch update trains the model from scratch (ignoring possible existing
        memory) and only requires B and C to be initialized, typically at
        random.
        An incremental update updates an existing model (B, C) taking into
        account existing memory (h_CCt and h_XCt).

        This method also piggybacks the method self.transform() by implementing
        mode="partial_mu". In this case, MU are only applied to C
        (B is kept constant and unchanged).

        About the lock. Normally, the entire multiplicative update should be
        performed keeping the variables locked (with lock: at the beginning should
        enclose the whole function). However, this would prevent any other
        operation such as predictions, dumping etc during a model update. To
        enable realtime operations, the lock to multiplicative_update is only applied:
        - if not partial_mu (updating purposes): at the beginning to retrieve
        the variable B, C and at the end to store the updated variables B, C.
        - if partial_mu (prediction): only at the beginning to retrieve B, C (
        updated C is not stored).

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Input data.
        mode : string, default="incremental"
            - "batch" : trains the model from scratch using batch update rules on
                all data.
            - "incremental" : updates existing model (B, C, history)
                using incremental update rules on all data.
            - "partial_mu" : solves ISNMF problem only for C without
                updating the model. Only used by self.transform() method.

        Returns
        -------
        B : ndarray of shape (dim_input, dim_latent)
            Updated bases.
        """

        if True:
            # retrieve existing bases
            B = deepcopy(self._B)

            # init C for the given data. self.compute_C_init() always restarts from the same random state.
            C = self.compute_C_init(X)

            assert B is not None
            assert C is not None

        # init variables to monitor convergence
        iteration = 0
        error_at_init = np.linalg.norm(X - np.dot(B, C), ord="fro")
        prev_error = error_at_init
        converged = False

        # precompute constant quantities (only for partial_mu, used by self.transform())
        if partial_mu:
            num = np.dot(B.T, X)
            BtB = np.dot(B.T, B)

        if not partial_mu:
            self._update_no += 1
            #self._debug_data["all_C"].append(C)
            #self._debug_data["all_X"].append(X)

            # balance the regularization of the bases in the loss by prepending the term \sum_{j=1}^{m} \mu^{m-j} to \beta \|W^m\|_F^2
            if self._scale_reg_B:
                if self._mem != 1:
                    scaling_reg_B = self._mem * (1 - self._mem ** self._update_no) / (1 - self._mem)
                else:
                    scaling_reg_B = self._update_no
            else:
                scaling_reg_B = 1

        # MU rules
        while iteration < self._max_iter and not converged:
            # region MU B
            if self._debug:
                self._debug_data["mean_C"].append(np.mean(C))

            # gradcheck (only valid for the first update!)
            if self._debug_gradients and self._reg_C == "l05":
                def loss_l05_l2_wrtB(params, B_shape, all_X, all_C, m, mu, gamma, beta):
                    B = np.reshape(params, B_shape)
                    return sum(
                        [
                            (mu ** (m - j))
                            * (
                                    1 / 2 * np.linalg.norm(all_X[j - 1] - np.dot(B, all_C[j - 1]), ord="fro") ** 2
                                    + 2 * gamma * np.sum(np.power(all_C[j - 1], 0.5))
                                    + 1 / 2 * beta * np.linalg.norm(B, ord="fro") ** 2
                            )
                            for j in range(1, m + 1)
                        ]
                    )

                def grad_B_l05_l2(params, B_shape, all_X, all_C, m, mu, gamma, beta):
                    B = np.reshape(params, B_shape)
                    return np.ravel(
                        sum(
                            [
                                (mu ** (m - j))
                                * (
                                        -np.dot(all_X[j - 1], all_C[j - 1].T)
                                        + np.linalg.multi_dot([B, all_C[j - 1], all_C[j - 1].T])
                                        + beta * B
                                )
                                for j in range(1, m + 1)
                            ]
                        )
                    )

                print(
                    f"MU_B: Magnitude B=(mean {np.mean(B)}, min {np.min(B)}, max {np.max(B)}), magnitude C=(mean {np.mean(C)}, min {np.min(C)}, max {np.max(C)})"
                )
                print(
                    f"grad check: {custom_check_grad(loss_l05_l2_wrtB, grad_B_l05_l2, B.ravel(), B.shape, self._debug_data['all_X'], self._debug_data['all_C'], self._update_no, self._mem, self._gamma, self._beta)}"
                )

            # only update B if not "partial_mu"
            if not partial_mu:
                assert self._h_XCt is not None
                num = self._mem * self._h_XCt + self._influence_new * np.dot(X, C.T)
                den = (
                        self._mem * np.dot(B, self._h_CCt)
                        + self._influence_new * np.linalg.multi_dot([B, C, C.T])
                        + self._influence_new * self._beta * scaling_reg_B * B
                )

                # avoid invalid division
                den[den < EPSILON] = EPSILON
                # den = np.maximum(den, EPSILON)

                # compute MU term
                u = num / den

                if self._debug:
                    assert self._h_XCt is not None
                    self._debug_data["mean_update_B_num1"].append(np.mean(self._mem * self._h_XCt))
                    self._debug_data["mean_update_B_num2"].append(np.mean(self._influence_new * np.dot(X, C.T)))
                    self._debug_data["mean_update_B_den1"].append(np.mean(self._mem * np.dot(B, self._h_CCt)))
                    self._debug_data["mean_update_B_den2"].append(
                        np.mean(self._influence_new * np.linalg.multi_dot([B, C, C.T]))
                    )
                    self._debug_data["mean_update_B_den3"].append(
                        np.mean(self._influence_new * self._beta * scaling_reg_B * B)
                    )
                    self._debug_data["update_no"].append(self._update_no)
                    self._debug_data["mean_update_B"].append(np.mean(u))

                    # print(f"N {np.mean(self._mem * self._h_XCt, axis=0)}, D1 {np.mean(self._mem * np.dot(B, self._h_CCt), axis=0)}, D3 {np.mean(self._influence_new * self._beta * scaling_reg_B * B, axis=0)}")

                # apply the update to B
                B *= u

                # avoid zero-lock (gillis)
                if self._clip_zerolock is not None:
                    B[B < EPSILON] = self._clip_zerolock

                if self._debug:
                    self._debug_data["mag_B"].append(np.linalg.norm(B, axis=0))

            # endregion

            # region MU C

            if self._debug_gradients:
                # gradcheck (only valid for the first update!)
                if self._reg_C == "l05":
                    def loss_l05_l2_wrtC(params, C_last_shape, all_X, all_C, B, m, mu, gamma, beta):
                        all_C[-1] = np.reshape(params, C_last_shape)
                        return sum(
                            [
                                (mu ** (m - j))
                                * (
                                        1 / 2 * np.sum(np.power(all_X[j - 1] - np.dot(B, all_C[j - 1]), 2))
                                        + 2 * gamma * np.sum(np.power(all_C[j - 1], 0.5))
                                        + 1 / 2 * beta * np.sum(np.power(B, 2))
                                )
                                for j in range(1, m + 1)
                            ]
                        )

                    def grad_C_l05_l2(params, C_last_shape, all_X, all_C, B, m, mu, gamma, beta):
                        C_last = np.reshape(params, C_last_shape)
                        return np.ravel(
                            -np.dot(B.T, all_X[-1]) + np.linalg.multi_dot((B.T, B, C_last)) + gamma * np.power(C_last,
                                                                                                               -0.5)
                        )

                    print(
                        f"MU_C: Magnitude B=(mean {np.mean(B)}, min {np.min(B)}, max {np.max(B)}), magnitude C=(mean {np.mean(C)}, min {np.min(C)}, max {np.max(C)})"
                    )
                    print(
                        f"grad check: {custom_check_grad(loss_l05_l2_wrtC, grad_C_l05_l2, C.ravel(), C.shape, self._debug_data['all_X'], self._debug_data['all_C'], B, self._update_no, self._mem, self._gamma, self._beta)}"
                    )

            # compute num and den
            if partial_mu:
                # for "partial_incremental" mode (isnmf.transform), the MU rule
                # for C does not require recomputing num and BtB at every iteration
                if self._reg_C == "l1":
                    den = np.dot(BtB, C) + self._gamma * np.sign(C)
                else:  # "l05": regularization of C = 2 * gamma * |C|_{1/2}^{1/2} (element-wise). MU according to dang2018sar.
                    den = np.dot(BtB, C) + self._gamma * np.power(C, -0.5)
            else:
                num = np.dot(B.T, X)
                if self._reg_C == "l1":
                    # OPTION 1: regularization of C = gamma * |C|_1
                    den = np.linalg.multi_dot([B.T, B, C]) + self._gamma * np.sign(C)
                else:  # "l05": regularization of C = 2 * gamma * |C|_{1/2}^{1/2} (element-wise). MU according to dang2018sar.
                    den = np.linalg.multi_dot([B.T, B, C]) + self._gamma * np.power(C, -0.5)

            # avoid invalid division
            den[den < EPSILON] = EPSILON

            # compute MU term
            u = num / den

            if self._debug and not partial_mu:
                if self._reg_C == "l1":
                    self._debug_data["mean_update_C_num"].append(np.mean(num))
                    self._debug_data["mean_update_C_den1"].append(np.mean(np.linalg.multi_dot([B.T, B, C])))
                    self._debug_data["mean_update_C_den2"].append(np.mean(self._gamma * np.sign(C)))
                else:  # "l05"
                    self._debug_data["mean_update_C_num"].append(np.mean(num))
                    self._debug_data["mean_update_C_den1"].append(np.mean(np.linalg.multi_dot([B.T, B, C])))
                    self._debug_data["mean_update_C_den2"].append(np.mean(self._gamma * np.power(C, -0.5)))
                self._debug_data["iteration"].append(iteration)
                self._debug_data["mean_update_C"].append(np.mean(u))

            # apply the update to C
            C *= u

            if self._clip_zerolock is not None:
                C[C < EPSILON] = self._clip_zerolock
                #self._debug_data["all_C"][-1] = C

            # endregion

            # region check convergence

            # check convergence every 10 iterations
            if iteration % 10 == 0:
                error = np.linalg.norm(X - np.dot(B, C), ord="fro")
                # convergence criterion based on sklearn's nmf
                if np.abs(prev_error - error) / error_at_init < self._tol:
                    converged = True
                prev_error = error
            iteration = iteration + 1

            # endregion

        # print stats (not for partial_mu)
        if self._verbose and not partial_mu:
            if not converged:
                print(
                    "max_iter was reached before convergence for at least one sample. " "Consider increasing max_iter or tol."
                )
            else:
                print(f"Convergence reached after {iteration} iterations.")

        if True:
            # region update history matrices (not for partial_mu)
            if not partial_mu:
                assert self._h_XCt is not None
                assert self._h_CCt is not None
                self._h_XCt = self._mem * self._h_XCt + self._influence_new * np.dot(X, C.T)
                self._h_CCt = self._mem * self._h_CCt + self._influence_new * np.dot(C, C.T)

                if self._debug:
                    self._debug_data["|XCt| / |hist_XCt|"].append(
                        np.linalg.norm(np.dot(X, C.T)) / np.linalg.norm(self._h_XCt))
                    self._debug_data["|CCt| / |hist_CCt|"].append(
                        np.linalg.norm(np.dot(C, C.T)) / np.linalg.norm(self._h_CCt))

            # endregion

            # set new bases (not for partial_mu)
            if not partial_mu:  # no bases update after "partial_incremental". Used by self.transform().
                self._B = B
                if self._debug:
                    self._debug_data["evol_B"].append(B)

            return B, C

    def reconstruction_loss(self, X: np.ndarray, metric: str = "nfrobe") -> Optional[float]:
        """Returns the reconstruction loss of is-nmf for the given input data.

        Specifically, X is transformed (to C) and back-transformed (to X_rec);
        then, the normalized Frobenius norm is used to compute the reconstruction
        error nfrobe(X-X_rec).

        Alternatively, it is also possible to compute the reconstruction loss
        using the normalized rmse.

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Input data matrix.
        metric : string, default="nfrobe"
            Whether to compute the reconstruction loss with normalized frobenius
            norm "nfrobe", with normalized rms error "nrmse", or with simple rms
            error "rmse"

        Returns
        -------
        rec_loss : float
            Reconstruction loss corresponding to nfrobe(X-X_rec),
            nrmse(X, X_rec), or nrmse(X, X_rec).

        """
        assert self._initialized, "Impossible to compute the reconstruction_loss before initializing the model."

        C = self.transform(X)
        if C is not None:
            X_rec = self.inverse_transform(C)

            # if metric == "nrmse":
            # return nrmse_robust(X.T, X_rec.T)

            if metric == "rmse":
                return np.sqrt(mean_squared_error(X.T, X_rec.T))

            elif metric == "frobe":
                return np.linalg.norm(X - X_rec, ord="fro")

            elif metric == "expl_var":
                return explained_variance_score(X.T, X_rec.T)

            elif metric == "r2":
                return r2_score(X.T, X_rec.T)

            # else:   "nfrobe"
            # return nfrobe(X, X_rec)

        else:
            return None

    def reinit_bases(self, bases_to_reinit: Optional[List] = None) -> bool:
        """Reinitializes the bases specified in bases_to_reinit to their initial value.

        Also reinitialize the history matrices as it is done when new bases are introduced
        in PISNMF (set the i-th column of hist_XCt to zero, set the i-th row and column of hist_CCt to zero).

        The argument bases_to_reinit is a list of 0-based indices representing the columns of self._B.
        """
        if True:
            success = False

            # check that the bases have already been initialized
            if self._B is None:
                print("reinit_bases() can only be called after the bases have been initialized.")
                return success

            # more checks...
            if bases_to_reinit is not None:
                # check that bases_to_reinit is not empty
                if len(bases_to_reinit) == 0:
                    print("bases_to_reinit is empty...")
                    return success

                # check that bases_to_reinit only contains valid indices
                if any([el < 0 for el in bases_to_reinit]) or np.max(bases_to_reinit) >= self._B.shape[1]:
                    print("invalid bases_to_reinit...")
                    return success

            # by default, reinitialize all the bases
            if bases_to_reinit is None:
                bases_to_reinit = np.arange(self._B.shape[1], dtype=int)

            # define which bases will not be reinitialized
            bases_to_keep = [el for el in np.arange(self._B.shape[1]) if el not in bases_to_reinit]

            # reinitialize desired bases
            print(self._B)  # debug
            # retrieve unscaled init bases
            assert self._B_init_unscaled is not None
            # OPTION 1: reuse B_init_unscaled
            b_new = self._B_init_unscaled[:, bases_to_reinit]
            # # OPTION 2: reinitialize randomly everytime
            # b_new = np.abs(np.random.standard_normal(b_new.shape))
            # scale retrieved init bases
            if self._bnew_scaling == "bold":
                # ... to match the average magnitude of the non-reinitialized bases
                scaling = np.mean(np.linalg.norm(self._B[:, bases_to_keep], axis=0))
                b_new = b_new / np.linalg.norm(
                    b_new, axis=0
                )  # TODO: THIS WOULDN'T BE NECESSARY IF THE UNSCALED BASES WERE ACTUALLY NORMALIZED AFTER THEIR CREATION
                b_new = b_new * scaling
            else:  # self._bnew_scaling == "v"
                # ... or to match the historical average of the data observed so far.
                # This scaling is the same used in the standard initialization (compute_B_init() with self._init == "random"))
                scaling = np.sqrt(self._historical_data_mean / self._n_components)
                b_new = (
                        b_new / np.linalg.norm(b_new, axis=0) * scaling
                )  # replicates the scaling used in compute_B_init ("random")
            b_new[b_new < EPSILON] = EPSILON
            # reinit basis
            assert self._B_init is not None
            assert self._B is not None
            self._B_init[:, bases_to_reinit] = b_new
            self._B[:, bases_to_reinit] = b_new
            print(self._B)  # debug

            # reinitialize corresponding history matrices
            assert self._h_XCt is not None
            assert self._h_CCt is not None
            self._h_XCt[:, bases_to_reinit] = 0
            self._h_CCt[:, bases_to_reinit] = 0
            self._h_CCt[bases_to_reinit, :] = 0

            success = True
            return success

    def transform(self, X: np.ndarray, mode: str = "partial_mu") -> Optional[np.ndarray]:
        """Maps input data into the ISNMF's latent representation.

        Obtains coefficient matrix C corresponding to new input data. This can
        be performed via clipped pseudoinverse or partial mu.

        Parameters
        ----------
        X : ndarray of shape (dim_input, n_samples)
            Input data.
        mode : string, default="partial_mu"
            Whether to solve X = B * C for C using nonnegative pinv ("clipped_pinv")
            or multiplicative updates only on C ("partial_mu").
        normalize_C : bool, default=False
            Used to avoid performing any normalization specified in self.norm_C.
            If False, no normalization is performed regardless of self.norm_C.
            If True, the normalization specified in self.norm_C is performed (
            potentially no normalization if self.norm_C is None).

        Returns
        -------
        C : ndarray of shape (dim_latent, n_samples)
        """
        assert mode in ["clipped_pinv", "partial_mu"]

        update_max = True

        B = self._B

        if B is None:
            if not self._predict_before_init:
                print("ISNMF.transform() can be only used after the model has been trained.")
                return None
            else:
                # create temporary random B and use it for random predictions (until the model is initialized)
                if self._random_B_before_init is None:
                    # create using the same strategy that will be used in the first update (not to mess up the incremental scaling of C)
                    data_dim, _ = X.shape
                    avg = np.sqrt(X.mean() / self._n_components)
                    if avg == 0:
                        avg = 1
                    # upscale avg s.t. the bases become bigger and the coeffs C become smaller.
                    # This should reduce the chances that high (random) Cs will influence the
                    # incremental normalization of future (actual Cs).
                    rng = np.random.RandomState(self._random_state)
                    self._random_B_before_init = (
                            avg
                            * np.abs(rng.standard_normal((data_dim, data_dim)).astype(X.dtype, copy=False).T)[
                              :, : self._n_components
                              ]
                    )
                B = self._random_B_before_init
                mode = "clipped_pinv"
                # avoid that (random) Cs will influence the incremental normalization of future (actual) Cs.
                update_max = False
        assert B is not None

        if len(X.shape) == 1:
            X = X[:, None]

        if mode == "clipped_pinv":
            B_pinv_pos = np.clip(np.linalg.pinv(B), a_min=0, a_max=None)
            C = np.dot(B_pinv_pos, X)  # with SVD-based MP-pseudoinverse
            # C = np.dot(np.dot(np.linalg.inv(np.dot(B.T, B)), B.T), X)  # with algebraic left MP-pseudoinverse (faster, but requires full-rank vertical matrix B)
        else:
            # mode == "partial_mu"
            _, C = self.multiplicative_update(X, partial_mu=True)

        return C

    def remove_unserializable(self) -> None:
        self.B = self.B.tolist()
        if self._B_init_unscaled is not None:
            self._B_init_unscaled = self._B_init_unscaled.tolist()
        self.debug_data.clear()
        self.B_init = self.B_init.tolist()
        self._h_CCt = self._h_CCt.tolist()
        self._h_XCt = self._h_XCt.tolist()

    def restore_unserializable(self) -> Any:
        self.B = np.array(self.B)
        self.B_init = np.array(self.B_init)
        self._h_CCt = np.array(self._h_CCt)
        self._h_XCt = np.array(self._h_XCt)
        if self._B_init_unscaled is not None:
            self._B_init_unscaled = np.array(self._B_init_unscaled)        


    def inverse_transform(self, C: np.ndarray, B: Optional[np.ndarray] = None) -> np.ndarray:
        """Maps an ISNMF's latent representation back into the original input space.

        Reconstructs input data from the corresponding latent coefficients C and
        the bases of the latent space self.B. X_rec = self.B * C.
        If B is explicitely passed, it is used instead of self.B.

        Parameters
        ----------
        C : ndarray of shape (dim_latent, n_samples)
            Mixing coefficients, i.e., latent representation of the data.
        B : ndarray of shape (dim_input, dim_latent) or None, default=None
            Optional bases matrix to be used for the inverse transform.
            If None (default), then self.B is used.

        Returns
        -------
        X : ndarray of shape (dim_input, n_samples)
            Reconstructed data in the input space.
        """
        assert C.shape[0] == self._n_components, "The mixing coefficients C should be of size (dim_latent, n_samples)."
        if B is not None:
            assert B.shape[1] == self._n_components, "The bases B should be of size (dim_input, dim_latent)."

        if B is None:
            return np.dot(self.B, C)
        else:
            return np.dot(B, C)

    def update_historical_data_mean(self, mean_last_minibatch: float) -> None:
        """Updates the historical mean with new mean.

        Formula from https://math.stackexchange.com/questions/106700/incremental-averaging .
        """
        self._historical_data_mean = (
                self._historical_data_mean + (mean_last_minibatch - self._historical_data_mean) / self._update_no
        )
        # print(f"historical data mean: {self._historical_data_mean}")


class iNMF:

    def __init__(self, n_components=2, n_features=8, forget_factor=0.7, encodingReg=0.001, synReg=0.1,
                 max_iteration_fit=200,
                 max_iteration_transform=200, epsilon=10 ** (-5), batch_size=100):
        self.forget_factor = forget_factor 
        self.n_components = n_components
        self.n_features = n_features
        self.batch_size = batch_size
        self.synReg = synReg
        self.encodingReg = encodingReg
        self.epsilon = epsilon
        self.max_iteration_fit = max_iteration_fit
        self.max_iteration_transform = max_iteration_transform

        reg_C = "l05"
        scale_reg_B = True
        influence_new = 1  # or 1 - mem
        init = "random"  # "random", "nndsvda", "nndsvdar"
        init_history = "zeros"
        isnmf_bnew_scaling = "bold"  # "v" or "bold"
        tol = 1e-5
        # max_iter =200
        random_state = 0
        norm_C = "robustmax"  # "max", "robustmax", "maxrows", "robustmaxrows"
        percentile = 95
        clip_zerolock = np.finfo(float).eps
        verbose = False
        debug = False
        debug_gradients = False

        # for sklearn's NMF: specify regularization and l1_ratio instead of beta and gamma
        regularization = 0.05
        l1_ratio = 0.5

        # endregion

        # region generate data matrix M

        n_samples = 1000
        dim_input = 8
        np.random.seed(random_state)
        t = np.linspace(0, 10, n_samples)
        # X has shape (dim_input, n_samples) according to custom isnmf
        X = np.abs(
            np.vstack(
                [(np.sin(np.random.rand() * t) + np.random.normal(scale=0.01, size=len(t))) for _ in range(dim_input)])
        )

        # endregion

        # region ISNMF

        self.model = ISNMF(
            n_components=n_components,
            reg_C=reg_C,
            gamma=encodingReg,
            beta=synReg,
            scale_reg_B=scale_reg_B,
            mem=forget_factor,
            influence_new=influence_new,
            init=init,
            init_history=init_history,
            tol=tol,
            max_iter=max_iteration_fit,
            random_state=random_state,
            norm_C=norm_C,
            clip_zerolock=clip_zerolock,
            percentile=percentile,
            verbose=True,
            debug=debug,
            debug_gradients=debug_gradients,
        )
        # to normalize the prediction with the percentiles
        self.percentile = TDigest()
        # self.percentiles = []
        # for i in range(n_components):
        #    self.percentiles.append(TDigest())

        print("Model created")

        print("Python: Model created ")
        return

    def addComponent(self):
        self.model.change_n_components(self.model.n_components + 1)
        self.n_components += 1
        return

    def getComponents(self):
        print("Components: ")
        print(self.model.B)
        try:
            return self.model.B.T
        except:
            print("Python: the model has no components yet")
            return None

    '''
    Loads the model from a given path.
    Attention: Make sure the path ends with .zip !
    '''

    def loadModel(self, zip_path):
        print("load model")
        # Open the .zip file
        model = None
        with zipfile.ZipFile(zip_path, 'r') as zipf:
            # Extract the JSON file with all parameter
            with zipf.open('Parameter.json') as json_file:
                json_data = json.load(json_file)
                model = iNMF()
                model.__dict__ = json_data
                model.__deserializse()
                print("Loaded JSON data:", json_data)

            # Extract the json file with the ISNMF model
            with zipf.open('ISNMF.json') as json_file:
                json_data = json.load(json_file)
                nmf = ISNMF(1)
                nmf.__dict__ = json_data
                nmf.restore_unserializable()

            model.model = nmf

        return model

    def overwriteModel(self, new_model):
        #new_model.model.remove_unserializable()
        self.model = copy.deepcopy(new_model.model)
        print("restore 1")
        #new_model.model.restore_unserializable()
        print("restore 2")

        #self.model.restore_unserializable()

    @log_timestamps('train_MB_NMF.log')
    def partial_fit(self, mini_batch, batch_size):
        # preparation
        print("started update process. ID: ", os.getpid())

        mini_batch = mini_batch.reshape(batch_size, -1)
        mini_batch = np.abs(mini_batch)
        mini_batch[np.isnan(mini_batch)] = 0
        mini_batch = mini_batch.T
        print("Mini Batch: ", mini_batch.shape)
        print("W before the update:", self.model.B)
        W = self.model.update(mini_batch)
        print("W after the update:", W)
        H = self.model.transform(mini_batch)

        recon_err_t_minus_1 = np.linalg.norm(mini_batch - np.matmul(W, H))
        print("recon err:", recon_err_t_minus_1)
        print("Params: mem:", self.model.mem, " beta:", self.model.beta, " gamma: ", self.model.gamma,
              ", n_components :", self.model.n_components)
        return W.T

    def resetComponent(self, component_n):
        self.model.reinit_bases([component_n])
        return self.model.B.T

    def resetComponentSensitivity(self, component_n):
        # self.percentiles[component_n] = TDigest()
        self.percentile = TDigest()

    '''
    Saves the model at the given path.
    Attention: Make sure the path ends with .zip !
    '''

    def saveModel(self, path):
        print("Python: path: ", path)
        # zip everything up
        self.model.remove_unserializable()
        with zipfile.ZipFile(path, 'w') as zipf:
            with open("ISNMF.json", 'w') as json_file:
                # create the json file
                json.dump(self.model.__dict__, json_file)
            # pack the json file into the zip
            zipf.write("ISNMF.json", "ISNMF.json")

            # save the rest in a json file
            print("save json")
            copy = self.__serializableCopy()
            with open("Parameter.json", 'w') as json_file:
                json.dump(copy.__dict__, json_file)
            zipf.write("Parameter.json", "Parameter.json")

            print("save copy:")
        self.model.restore_unserializable()


    def setForgetFactor(self, forget_factor):
        self.model.mem = forget_factor
        self.forget_factor = forget_factor

    def setSynReg(self, synReg):
        self.model.beta = synReg
        self.synReg = synReg

    def setEncodingReg(self, encodingReg):
        self.model.gamma = encodingReg
        self.encodingReg = encodingReg

    def setMaxIter(self, max_iter):
        self.model._max_iter = max_iter
        self.max_iteration_fit = max_iter
        self.max_iteration_transform = max_iter

    @log_timestamps('transform_MB_NMF.log')
    def transform(self, input, normalize=False):
        # preparation
        input = input.reshape(1, -1)

        # transform
        if (self.model != None):
            self.activation = self.model.transform(input.T)
            # get the transformed data
            transformedData = np.matmul(self.model.B, self.activation, )
            # compute reconstruction error
            reconErr = np.linalg.norm(input - transformedData, ord='fro')

            # normalize the prediction to the range (0,1) with the 95-th percentile
            if (normalize):
                self.__normalizeTransformation()

            return TransformationResult(self.activation.T.reshape(self.n_components, ),
                                        transformedData.reshape(self.n_features, ), reconErr)

    def __deserializse(self):
        print("deserialze model")
        #self.activation = np.array(self.activation)
        self.model = None
        td = TDigest()
        td.update_from_dict(self.percentile)
        self.percentile = td

        print("deserialzed model: ", self.__dict__)

    def __normalizeTransformation(self):
        # print("coef before normalizing:", self.activation)
        for i in range(self.n_components):
            self.percentile.update(self.activation[i, 0])
            # self.percentiles[i].update(self.activation[i,0])
            max_v = self.percentile.percentile(85)
            # max_v = self.percentiles[i].percentile(85)

            self.activation[i, 0] = max(min(self.activation[i, 0] / max_v, 1), 0)
            # print("Prediction: " ,np.transpose(self.H)[0])
        self.activation[self.activation < 0.3] = 0
        # print("coef after normalizing:", self.activation)

        np.nan_to_num(self.activation, copy=False)

    def __serializableCopy(self):
        print("Create serializable copy")
        c = copy.deepcopy(self)
        #c.activation = c.activation.tolist()
        c.model = None
        c.percentile = c.percentile.to_dict()
        return c


# pyModel_update = iNMF(n_components=n_components, forget_factor=forget_factor, n_features=n_features, synReg=synergyReg,
#                       encodingReg=encodingReg, epsilon=epsilon, batch_size=batch_size, max_iteration_fit=max_iter_fit,
#                       max_iteration_transform=max_iter_transform)
# pyModel_predict = iNMF(n_components=n_components, forget_factor=forget_factor, n_features=n_features, synReg=synergyReg,
#                        encodingReg=encodingReg, epsilon=epsilon, batch_size=batch_size, max_iteration_fit=max_iter_fit,
#                        max_iteration_transform=max_iter_transform)