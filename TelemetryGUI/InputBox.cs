using System;
using System.Windows.Forms;

namespace MyPubgTelemetry.GUI
{
    public partial class InputBox : Form
    {
        public string InputText
        {
            get => textBox1.Text;
            set => textBox1.Text = value;
        }

        public InputBox()
        {
            InitializeComponent();
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void InputBox_VisibleChanged(object sender, EventArgs e)
        {
            textBox1.Focus();
            textBox1.SelectAll();
        }
    }
}
