using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.IO;

namespace MosaicLibary
{
    class PythonNetManager
    {
        private static readonly Lazy<PythonNetManager> instance = new Lazy<PythonNetManager>(() => new PythonNetManager());

        public string pythonDllPath { get; private set; }
        public string poetrySitePackagesPath { get; private set; }
        public string scriptsPath { get; private set; }

        // Private constructor to prevent instantiation
        private PythonNetManager()
        {
            dynamic config_json = JsonConvert.DeserializeObject(File.ReadAllText(@"..\..\..\MosaicPython\config.json"));
            this.pythonDllPath = config_json.pythonDllPath;
            this.poetrySitePackagesPath = config_json.poetrySitePackagesPath;
            this.scriptsPath = config_json.loadScriptsPath;

            Runtime.PythonDLL = pythonDllPath;

            //if (pythonHomePath != null) PythonEngine.PythonHome = pythonHomePath;
            
            PythonEngine.Initialize();
            // this is needed to allow multiple py.GILs see: https://github.com/pythonnet/pythonnet/wiki/Threading
            PythonEngine.BeginAllowThreads();

            // append scripts for direct access
            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(poetrySitePackagesPath);
                    sys.path.append(scriptsPath);
                }
            }
            Console.WriteLine($"PythonManager was started with \npython dll: {pythonDllPath} \npython venv: {poetrySitePackagesPath} \npython scripts: {scriptsPath}");
        }

        public static PythonNetManager Instance
        {
            get
            {
                return instance.Value;
            }
        }
        /// <summary>
        // Mandatory in every block using the PythonNet integration. If there are multiple blocks PythonNetManager will only be activated once.
        // Singelton is automatically created, when a function is called.
        /// </summary>
        public void Init()
        {
            Console.WriteLine("Init Pythonnet");
        }
    }
}
