

namespace Wisch.FogOfWar;

class FormLogger : Form
{
    private static event Action<string> LogAppended;
    public static void Log(string line)
    {
        LogAppended?.Invoke(line);
    }

    private static event Action LogCleared;
    public static void Clear()
    {
        LogCleared?.Invoke();
    }

    Label label;
    public FormLogger()
    {
        DoubleBuffered = true;
        label = new();
        Controls.Add(label);
        label.Dock = DockStyle.Fill;

        LogAppended += line => label.Text += line + Environment.NewLine;
        LogCleared += () => label.Text = "";
    }
}

public class FormMain : Form
{
    const int GridWidth = 23;
    const int GridHeight = 16;

    FormOverlay overlay;
    Map map;
    private readonly MapControl mapControl;

    public FormMain()
    {
        Text = "DM";
        mapControl = new()
        {
            IsPreview = true
        };
        Controls.Add(mapControl);
        mapControl.Dock = DockStyle.Fill;

        var menu = new MenuStrip();
        Controls.Add(menu);

        var menuFile = new ToolStripMenuItem("&Datei");
        menu.Items.Add(menuFile);

        var itemOpen = menuFile.DropDownItems.Add("Ö&ffnen");
        itemOpen.Click += (o, e) =>
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Bild-Dateien (*.png, *.bmp, *.jpg, *.jpeg, *.gif)|*.png;*.bmp;*.jpg;*.jpeg;*.gif|Alle Dateien|*.*";
                dialog.Title = "Karte auswählen";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        map = FormMapPreperation.ShowPreperationDialog(dialog.FileName);
                        //map = new Map(dialog.FileName, GridWidth, GridHeight);
                    }
                    catch { return; }
                }
                else return;
            }
            mapControl.Map = map;
            overlay?.Close();
            overlay?.Dispose();
            overlay = new FormOverlay()
            {
                Width = 500,
                Height = 500,
                Map = map,
            };
            overlay.Show();

            var form = new Form();
            form.Controls.Add(new NewMapControl(map)
            {
                IsPreview = true,
                Dock = DockStyle.Fill
            });
            form.Show();
        };

        var itemClose = menuFile.DropDownItems.Add("&Beenden");
        itemClose.Click += (s, e) =>
        {
            Application.Exit();
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        new FormLogger().Show();
        WindowState = FormWindowState.Maximized;
    }
}
