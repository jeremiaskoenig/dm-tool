using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Wisch.FogOfWar;

public partial class FormOverlay : Form
{
    private Map map;

    public Map Map
    {
        get => map;
        set
        {
            map = value;
            mapControl.Map = map;
        }
    }

    private readonly MapControl mapControl;

    public FormOverlay()
    {
        mapControl = new();
        Controls.Add(mapControl);
        mapControl.Dock = DockStyle.Fill;
        Text = "Player";
    }

    protected override void OnLoad(EventArgs e)
    {
        WindowState = FormWindowState.Maximized;
    }
}
