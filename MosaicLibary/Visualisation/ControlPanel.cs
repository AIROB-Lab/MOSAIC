using System;
using System.Windows.Forms;

namespace MosaicLibary
{
    /// <summary>
    /// Represents a generic control panel form for managing a specific device or module within the application.
    /// This class provides a foundational UI component that can be extended for specific device or module control panels.
    /// </summary>
    public partial class ControlPanel : Form
    {
        /// <summary>
        /// The source block associated with this control panel. 
        /// This is the device or module that this panel is intended to control or display information for.
        /// </summary>
        private Block _sourceBlock;

        /// <summary>
        /// Initializes a new instance of the ControlPanel class. 
        /// This constructor is used by the designer and should not contain code.
        /// </summary>
        public ControlPanel() { }

        /// <summary>
        /// Initializes a new instance of the ControlPanel class with a specific source block.
        /// </summary>
        /// <param name="sourceBlock">The block (device or module) that this control panel will manage or display information for.</param>
        public ControlPanel(Block sourceBlock)
        {
            InitializeComponent();
            VisibleChanged += (o, e) => 
            { 
                if (Visible) Utils.dtmRefresh.Tick += cpRefresh; 
                else Utils.dtmRefresh.Tick -= cpRefresh; 
            };
            this._sourceBlock = sourceBlock;
            Text = $"{this._sourceBlock.Name}";
        }

        /// <summary>
        /// A virtual method to refresh the control panel's content. 
        /// This method should be overridden in derived classes to update the UI based on the current state of the source block.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        virtual protected void cpRefresh(object o, EventArgs e) { }

        /// <summary>
        /// Handles the KeyDown event to toggle the visibility of the control panel when the Escape _key is pressed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        private void ControlPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                _sourceBlock.ShowHideCp();
        }
    }
}
