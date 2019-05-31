using System.Drawing;
using System.Windows.Forms;

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

}