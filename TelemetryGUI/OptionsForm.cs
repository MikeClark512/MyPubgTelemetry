using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyPubgTelemetry.GUI
{
    public partial class OptionsForm : Form
    {
        public TelemetryApp App { get; set; }

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            App = new TelemetryApp();
            textBox1.Text = App.ApiKey;
            MinimumSize = Size;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            File.WriteAllText(App.DefaultApiKeyFile, textBox1.Text);
            Close();
        }
    }
}
