using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MyPubgTelemetry.GUI
{
    public class SplitContainerEx : SplitContainer
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder3D(e.Graphics, SplitterRectangle, Border3DStyle.Raised, Border3DSide.Top | Border3DSide.Bottom);
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripLabelTextBox : MyToolStripControlHost
    {
        public Label Label { get; private set; }
        public ComboBox ComboBox { get; private set; }

        public string LabelText
        {
            get => Label.Text;
            set => Label.Text = value;
        }

        public ToolStripLabelTextBox() : this("")
        {
        }

        public ToolStripLabelTextBox(string labelText) : base(new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            //AutoSize = true,
            //AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            //BorderStyle = BorderStyle.Fixed3D
        })
        {
            Label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Top,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                //                Height = 10
                //Dock = DockStyle.Left
            };
            ComboBox = new ComboBox()
            {
                AutoSize = true,
                //Dock = DockStyle.Right
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            ComboBox.Items.Add("hi");
            ComboBox.SelectedIndex = 0;
            ComboBox.FlatStyle = FlatStyle.Flat;
            FlowLayoutPanel panel = (FlowLayoutPanel)Control;
            panel.Controls.Add(Label);
            panel.Controls.Add(ComboBox);
        }
    }

    public class MyToolStripControlHost : ToolStripControlHost
    {
        public MyToolStripControlHost()
            : base(new Control())
        {
        }
        public MyToolStripControlHost(Control c)
            : base(c)
        {
        }
    }
}