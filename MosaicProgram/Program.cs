using System;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using MosaicLibary;


namespace MosaicProgram
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            // globally use the dot for decimals, e.g., 3.1415 and not 3,1415
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MosaicMain());
        }
    }

    public partial class MosaicMain : Form
    {
        public MosaicMain()
        {
            // in this example the Form serves no purpose, so hide it
            WindowState = FormWindowState.Minimized; Hide();

            // if available, use the Intel Mathnet Kernel Library provided by Mathnet-Numerics, which is
            // +1.5 OOMs faster than the all-purpose code. This should be automatically achieved by
            // NuGet'ting the package called MathNet.Numerics.MKL.Win.
            if (MathNet.Numerics.Control.TryUseNativeMKL())
                Console.WriteLine("Found Intel.MKL.Mathnet DLL - using it from now on.");
            else
                Console.WriteLine("Could not find Intel.MKL.Mathnet DLL - math operations will be slow.");

            // so, open a configuration file and parse it
            OpenFileDialog _ofd = new OpenFileDialog { Filter = "iM configuration files|*.yaml", Title = "Open config file", RestoreDirectory = true };
            _ofd.ShowDialog();
            // out of the cfg file, the CfgParser populates Blocks, a global Dictionary<string,Block> containing all blocks (see Basics.cs)
            CfgParser parser = new CfgParser(_ofd.FileName);

            // build and start the block table. the BlockTable constructor use Blocks, built at the previous step, to create the GUI
            BlockTable bTable = new BlockTable();

            // set the exiting method
            Disposed += Template_project_mainForm_Disposed;
        }

        private void Template_project_mainForm_Disposed(object sender, EventArgs e) 
        { 
            foreach (Block b in Blocks.Instance.Values.ToList()) 
                b.Dispose(); 
        }
    }
}
