

namespace Wisch.FogOfWar;

public class FormMapPreperation : Form
{
    public static Map ShowPreperationDialog(string imagePath)
    {
        using var dialog = new FormMapPreperation(Image.FromFile(imagePath));

        dialog.ShowDialog();

        return new Map(imagePath, 
                       dialog.gridColumns, dialog.gridRows,
                       dialog.gridX, dialog.gridY,
                       dialog.gridWidth / dialog.gridColumns, dialog.gridHeight / dialog.gridRows);
    }

    private int gridColumns = 10;
    private int gridRows = 10;
    private int gridWidth = 100;
    private int gridHeight = 100;
    private int gridX = 10;
    private int gridY = 10;
    private readonly Image image;

    private Rectangle StartHandle => new(gridX - 4, gridY - 4, 7, 7);
    private Rectangle EndHandle => new(gridX + gridWidth - 4, gridY + gridHeight - 4, 7, 7);

    public FormMapPreperation(Image image)
    {
        this.Text = "Karte vorbereiten";
        this.DoubleBuffered = true;
        this.image = image;

        Panel panel = new()
        {
            Width = 200,
            Height = 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Left = Width - 200,
            Top = 0,
        };
        Controls.Add(panel);

        Label labelColumns = new()
        {
            Text = "Spalten",
            Top = 10,
            Left = 10,
            AutoSize = true,
        };
        panel.Controls.Add(labelColumns);

        NumericUpDown numColumns = new()
        {
            Top = 10,
            Left = 70,
            Width = 80,
            Increment = 1,
            Minimum = 1,
            Maximum = 500,
            Value = gridColumns,
        };
        numColumns.ValueChanged += (s, e) => 
        {
            gridColumns = (int)numColumns.Value;
            Refresh();
        };
        panel.Controls.Add(numColumns);

        Label labelRows = new()
        {
            Text = "Zeilen",
            Top = 35,
            Left = 10,
            AutoSize = true,
        };
        panel.Controls.Add(labelRows);

        NumericUpDown numRows = new()
        {
            Top = 35,
            Left = 70,
            Width = 80,
            Increment = 1,
            Minimum = 1,
            Maximum = 500,
            Value = gridRows,
        };
        numRows.ValueChanged += (s, e) => 
        {
            gridRows = (int)numRows.Value;
            Refresh();
        };
        panel.Controls.Add(numRows);
    }

    private bool isDragging = false;
    private bool isDraggingStartHandle = false;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (StartHandle.Contains(e.X, e.X))
        {
            isDragging = true;
            isDraggingStartHandle = true;
        }
        if (EndHandle.Contains(e.X, e.X))
        {
            isDragging = true;
            isDraggingStartHandle = false;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (isDragging)
        {
            if (isDraggingStartHandle)
            {
                gridX = e.X;
                gridY = e.Y;
            }
            else
            {
                gridWidth = e.X - gridX;
                gridHeight = e.Y - gridY;
            }
            Refresh();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        isDragging = false;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Black);
        e.Graphics.DrawImage(image, 0, 0);
        DrawGrid(e.Graphics);
    }

    private void DrawGrid(Graphics g)
    {
        var cellWidth = gridWidth / gridColumns;
        var cellHeight = gridHeight / gridRows;

        for (int x = 0; x <= gridColumns; x++)
        {
            g.DrawLine(Pens.Red, gridX + (x * cellWidth), gridY, gridX + (x * cellWidth), gridY + gridHeight);
        }
        for (int y = 0; y <= gridRows; y++)
        {
            g.DrawLine(Pens.Red, gridX, gridY + (y * cellHeight), gridX + gridWidth, gridY + (y * cellHeight));
        }
        g.FillRectangle(Brushes.Red, StartHandle);
        g.FillRectangle(Brushes.Red, EndHandle);
    }
}
