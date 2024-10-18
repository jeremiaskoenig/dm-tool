using Wisch.FogOfWar;

class NewMapControl : Control
{
    private static event Action GlobalRefresh;
    private static void DoGlobalRefresh() => GlobalRefresh?.Invoke();

    private readonly Map map;
    public NewMapControl(Map map)
    {
        DoubleBuffered = true;
        GlobalRefresh += () => Refresh();
        this.map = map;
    }

    private Coordinate offset = (0, 0);
    private float zoom = 1.0f;

    private Rectangle GetMapImageArea(Image image)
    {
        var x = offset.X * zoom;
        var y = offset.Y * zoom;
        var width = image.Width * zoom;
        var height = image.Height * zoom;
        return new((int)x, (int)y, (int)width, (int)height);
    }

    private Rectangle GetMapGridArea()
    {
        var x = (offset.X + map.GridOffset.X) * zoom;
        var y = (offset.Y + map.GridOffset.Y) * zoom;
        var width = map.GridSize.X * map.CellSize.X * zoom;
        var height = map.GridSize.Y * map.CellSize.Y * zoom;
        return new((int)x, (int)y, (int)width, (int)height);
    }

    private Coordinate GetGridCellAt(int x, int y)
    {
        var area = GetMapGridArea();

        var cellWidth = map.CellSize.X * zoom;
        var cellHeight = map.CellSize.Y * zoom;

        var cellX = (x - area.X) / cellWidth;
        var cellY = (y - area.Y) / cellHeight;

        return ((int)cellX, (int)cellY);
    }

    public bool IsPreview { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Black);

        var mapImage = map.Image;
        var gridArea = GetMapGridArea();
        var mapArea = GetMapImageArea(mapImage);
        var fogBrush = Map.Brushes.Hidden(IsPreview);
        var cellWidth = map.CellSize.X * zoom;
        var cellHeight = map.CellSize.Y * zoom;

        // Render map and grid fog
        e.Graphics.DrawImage(mapImage, mapArea);
        for (int x = 0; x < map.GridSize.X; x++)
        {
            for (int y = 0; y < map.GridSize.Y; y++)
            {
                if (map[(x, y)].Equals(CellState.DefaultStates.Hidden))
                {
                    e.Graphics.FillRectangle(fogBrush, 
                                            gridArea.X + cellWidth * x,
                                            gridArea.Y + cellHeight * y,
                                            cellWidth,
                                            cellHeight);
                }
            }
        }

        // Render fog around grid
        var areaTop = new Rectangle(0, 0, (int)e.Graphics.ClipBounds.Width, gridArea.Y);
        var areaLeft = new Rectangle(0, gridArea.Y, gridArea.X, gridArea.Height);
        var areaRight = new Rectangle(gridArea.X + gridArea.Width, gridArea.Y, (int)e.Graphics.ClipBounds.Width - gridArea.X - gridArea.Width, gridArea.Height);
        var areaBottom = new Rectangle(0, gridArea.Y + gridArea.Height, (int)e.Graphics.ClipBounds.Width, (int)e.Graphics.ClipBounds.Height - gridArea.Y - gridArea.Height);
        e.Graphics.FillRectangle(fogBrush, areaTop);
        e.Graphics.FillRectangle(fogBrush, areaLeft);
        e.Graphics.FillRectangle(fogBrush, areaRight);
        e.Graphics.FillRectangle(fogBrush, areaBottom);

        // Render grid
        for (int x = 0; x <= map.GridSize.X; x++)
        {
            e.Graphics.DrawLine(Map.Brushes.Grid, 
                                gridArea.X + cellWidth * x,
                                gridArea.Y,
                                gridArea.X + cellWidth * x,
                                gridArea.Y + gridArea.Height);
        }
        for (int y = 0; y <= map.GridSize.Y; y++)
        {
            e.Graphics.DrawLine(Map.Brushes.Grid, 
                                gridArea.X,
                                gridArea.Y + cellHeight * y,
                                gridArea.X + gridArea.Width,
                                gridArea.Y + cellHeight * y);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        var cell = GetGridCellAt(e.X, e.Y);
        if (map.IsCellValid(cell))
            map.ToggleCell(cell.X, cell.Y);

        Refresh();
    }
}