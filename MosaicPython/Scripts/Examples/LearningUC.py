from sklearn.decomposition import PCA
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.decomposition import FastICA
import numpy as np

class OnlinePCA:
    def __init__(self):
        # Initialize the number of components and the PCA model
        self.n_components = None
        self.pca_model = None

    def fit(self, X, n_components):
        # Set the number of components
        self.n_components = n_components
        # Create a new PCA model with the specified number of components
        self.pca_model = PCA(n_components=n_components)
        
        try:
            # Fit the PCA model to the input data
            self.pca_model.fit(np.array(X))
            print('fit ok')
        except Exception as e:
            print("Fit error:", e)
            return None


    def transform(self, X):
        try:
             # Reshape the input data to the required format for transformation
            X_reshaped = np.array(X).reshape(1, -1)
            result = self.pca_model.transform(X_reshaped)
            result = np.array(result)
            #print("Output shape after transform:", result.shape)
            return result
        except Exception as e:
            print("Fit error:", e)

class OnlineLDA:
    def __init__(self):
        # Initialize number of components and LDA model
        self.n_components = None
        self.lda_model = None

    def fit(self, X, y, n_components):
        # Set the number of components
        self.n_components = n_components
        y = np.array(y).ravel()
        # Create a new LDA model with the number of components
        self.lda_model = LinearDiscriminantAnalysis(n_components=n_components)

        try:
            self.lda_model.fit(np.array(X), np.array(y))
            print('fit ok')
        except Exception as e:
            print("Fit error:", e)
            return None

    def transform(self, X):
        try:
            # Reshape the input data to the required format for transformation
            X_reshaped = np.array(X).reshape(1, -1)
            result = self.lda_model.transform(X_reshaped)
            result = np.array(result)
            #print("Output shape after transform:", result.shape)
            #print("Output :", result)
            return result
        except Exception as e:
            print("Transform error:", e)

    def predict(self, X):
        # Predict the class labels for the input data
        try:
            # Reshape the input data to the required format for transformation
            X_reshaped = np.array(X).reshape(1, -1)
            prediction = self.lda_model.predict(X_reshaped)
            ##print('label:', prediction)
            return  np.array(prediction)
        except Exception as e:
            print("Prediction error:", e)


class OnlineICA:
    def __init__(self):
        self.n_components = None
        self.ica_model = None

    def fit(self, X, n_components):
        # Set the number of components
        self.n_components = n_components
        self.ica_model = FastICA(n_components=n_components, whiten='unit-variance')


        try:
            self.ica_model.fit(np.array(X))
            print('fit ok')
        except Exception as e:
            print("Fit error:", e)
            return None

    def transform(self, X):
        # Transform the data to the specified number of components
        try:
            # Reshape the input data to the required format for transformation
            X_reshaped = np.array(X).reshape(1, -1)
            result = self.ica_model.transform(X_reshaped)
            #print("Output shape after transform:", result.shape)
            #print("Output :", result)
            return result
        except Exception as e:
            print("Transform error:", e)


# Create instances of OnlinePCA, OnlineLDA, and OnlineICA classes
# online_pca = OnlinePCA()  # Online Principal Component Analysis
# online_lda = OnlineLDA()  # Online Linear Discriminant Analysis
# online_ica = OnlineICA()  # Online Independent Component Analysis

