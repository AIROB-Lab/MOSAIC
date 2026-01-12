
# Using Python, Poetry, and Python.NET in Projects

## 1. Requirements

### Install Python
- **Python 3.13 is not supported in Python.NET.** Use **Python 3.12** instead.
- Download and install from the [official Python website](https://www.python.org/downloads/).
- Default installation path for the Python DLL:
  ```
  C:\Users\<username>\AppData\Local\Programs\Python\Python312\python312.dll
  ```
  Replace `<username>` with your actual username.

### Install Poetry
- Open a terminal or command prompt.
- Execute the following command to install Poetry:
  ```
  pipx install poetry 
  ```
  or
    ```
  curl -sSL https://install.python-poetry.org | python -
  ```
- Verify the installation by checking the version:
  ```
  poetry --version
  ```
- Check [official Python Poetry](https://python-poetry.org/).
---

## 2. How to Install Dependencies Using Poetry

- Open a terminal and navigate to where your project is located.
- Ensure you are in the directory containing the `pyproject.toml` file, go to:
  ```
  \MosaicPython\Dependencies\
  ```
- Poetry can stores dependencies in a global cache directory to optimize for reuse across projects. However, you can configure it to install dependencies in a location specific to your project, as explained in [Poetry Configuration Documentation](https://python-poetry.org/docs/configuration/). To store dependencies in the project directory alongside the Poetry file for the current repo, execute:
  ```
  poetry config --local virtualenvs.in-project true
  ```
- Then install the predefined libraries by executing the command:
  ```
  poetry install
  ```
- This command installs all Python dependencies listed in the `pyproject.toml` into the local `.venv` folder.
- This command **only installs Python libraries**.
- In the Python.NET example code, this dependencies path is automatically used as:
  ```
  ..\..\..\..\Lib\site-packages
  ```
- if you get an error about your python version being wrong (multiple python versions) try: poetry env use C:\path\to\python.exe

- Lastly, add your python dll path to the configuration file `config.json` inside the iM-Python folder (also change the packages path if you want to use another then the pre-set environment)

- If Poetry automatically installs packages to the current project path, the script has automatic access to the `.venv` folder and its dependencies. Now you just have to set the PythonDLL path in YAML file to start the example Python.NET blocks. 
- If your default Poetry setting creates the environment dependencies elsewhere, you can specify the path to the dependencies folder as the last parameter in the YAML file.

---

## 3. How to create a new Poetry Management for Your Project

### Steps:
1. Navigate to the directory where you want to create the Poetry environment using the terminal.
2. Run the following command to initialize a Poetry project:
   ```
   poetry init
   ```
3. Follow the prompts:
   - Specify the package name, version, description, author, and compatible Python versions.
   - Add any dependencies interactively (e.g., `numpy`).
4. After completing the setup, a `pyproject.toml` file will be automatically created.

### Important Notes:
- Always ensure you specify the correct Python DLL path in your code or the configuration file `config.json` in the iM-Python folder.
- Verify that the `.venv\Lib\site-packages` directory is referenced correctly in your C# project.
- In Python.NET, append this path in your code to the Python GIL setup (not necessary if you use the existing poetry setup):
  ```csharp
  sys.path.append(poetrySitePackagesPath);
  ```
