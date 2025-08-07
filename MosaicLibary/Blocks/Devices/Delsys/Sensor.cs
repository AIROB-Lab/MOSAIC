using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DelsysAPI.Components.TrignoRf;

namespace MosaicLibary
{
    /// <summary>
    /// Represents a user control for selecting and configuring a single Delsys Trigno sensor.
    /// Used within the <see cref="CpDelsys"/> for sensor selection and mode assignment.
    /// </summary>
    public partial class SensorItem : UserControl
    {
        /// <summary>
        /// Selecet Mode Index 
        /// </summary>
        public int ModeIndex = 0;

        /// <summary>
        /// Gets whether this sensor is selected for use (checked in the UI).
        /// </summary>
        public bool IsSelected
        {
            get { return sensorTypeCheckBox.Checked; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorItem"/> user control for a given sensor.
        /// </summary>
        /// <param name="sensor">The <see cref="SensorTrignoRf"/> representing the Delsys sensor to display and configure.</param>
        public SensorItem(SensorTrignoRf sensor)
        {
            InitializeComponent();

            sensorTypeCheckBox.Text = sensor.FriendlyName;
            sensorId.Text = sensor.Properties.Sid.ToString();
            sensorNumber.Text = sensor.PairNumber.ToString();
            int modeIndex = 0;
            foreach (var mode in sensor.Configuration.SampleModes)
            {
                modeIndex++;
                cbSensorModes.Items.Add(mode.ToString());
            } 
        }

        /// <summary>
        /// Event handler for when the sample mode selection changes.
        /// Updates <see cref="ModeIndex"/> accordingly.
        /// </summary>
        private void cbSensorModes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ModeIndex = cbSensorModes.SelectedIndex;
        }
    }
}
