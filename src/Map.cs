using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Wisch.FogOfWar;

public class Map
{
    public Map(string imageFileName, int width, int height, int offsetX, int offsetY, int cellWidth, int cellHeight)
    {
        this.gridSize = (width, height);
        this.cellSize = (cellWidth, cellHeight);
        this.gridOffset = (offsetX, offsetY);
        this.image = Image.FromFile(imageFileName);
        this.renderImage = new Bitmap(Image.FromFile(imageFileName));

        this.cells = new CellState[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = CellState.DefaultStates.Hidden;
            }
        }
    }

    // Map information
    private readonly Coordinate gridSize;
    public Coordinate GridSize => gridSize;

    private readonly Coordinate cellSize;
    public Coordinate CellSize => cellSize;

    private readonly Coordinate gridOffset;
    public Coordinate GridOffset => gridOffset;
    // Map state
    private readonly CellState[,] cells;
    public CellState this[Coordinate coord] => cells[coord.X, coord.Y];

    public void UpdateCell(int x, int y, CellState state)
    {
        cells[x, y] = state;
    }

    public void ToggleCell(int x, int y)
    {
        if (cells[x, y] == CellState.DefaultStates.Hidden)
            cells[x, y] = CellState.DefaultStates.Revealed;
        else
            cells[x, y] = CellState.DefaultStates.Hidden;
    }

    public bool IsCellValid(Coordinate c)
    {
        return c.X >= 0
            && c.Y >= 0
            && c.X < gridSize.X
            && c.Y < gridSize.Y;
    }

    // Render information
    public Image Image => image;

    private readonly Image image; 
    private readonly Image renderImage;

    public static class Brushes
    {
        private static readonly Brush hiddenPreview = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
        private static readonly Brush hiddenLive = new SolidBrush(Color.Black);
        public static Brush Hidden(bool isPreview) => isPreview ? hiddenPreview : hiddenLive;

        public static readonly Pen Grid = new(Color.Red);
    }

    public Coordinate CellFromPixel(int x, int y)
    {
        (int width, int height) = ((int, int))gridSize;
        float cellWidth = image.Width / (float)width;
        float cellHeight = image.Height / (float)height;

        var indexX = (int)(x / cellWidth);
        var indexY = (int)(y / cellHeight);
        if (indexX < 0 || indexX >= gridSize.X || indexY < 0 || indexY >= gridSize.Y)
            return Coordinate.Invalid;
        else
            return (indexX, indexY);
    }

    public Image Render(bool isPreview)
    {
        using var g = Graphics.FromImage(renderImage);
        g.Clear(Color.Black);
        g.DrawImage(image, 0, 0, image.Width, image.Height);

        (int width, int height) = ((int, int))gridSize;
        float cellWidth = image.Width / (float)width;
        float cellHeight = image.Height / (float)height;

        // TODO: - Draw solid color around the grid
        //       - Respect the new grid offset / size
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y] == CellState.DefaultStates.Hidden)
                    g.FillRectangle(Brushes.Hidden(isPreview), gridOffset.X + x * cellWidth, gridOffset.Y + y * cellHeight, cellWidth, cellHeight);
            }
        }

        // TODO: Move the grid render code to the map control
        //if (isPreview)
        //{
        //    for (int x = 0; x <= width; x++)
        //    {
        //        g.DrawLine(Brushes.Grid, x * cellWidth, 0, x * cellWidth, image.Height);
        //    }
        //    for (int y = 0; y <= height; y++)
        //    {
        //        g.DrawLine(Brushes.Grid, 0, y * cellHeight, image.Width, y * cellHeight);
        //    }
        //}

        return renderImage;
    }
}

public readonly struct CellState
{
    public string Value { get; }

    public CellState(string value)
    {
        Value = value;
    }

    public static implicit operator CellState(string s)
    {
        return new(s);
    }

    public static implicit operator string(CellState s)
    {
        return s.Value;
    }

    public static List<CellState> States { get; } = new();
    static CellState()
    {
        States.Add(DefaultStates.Hidden);
        States.Add(DefaultStates.Revealed);
        States.Add(DefaultStates.Players);
        States.Add(DefaultStates.Exit);
    }

    public static class DefaultStates
    {
        public const string Hidden = "Hidden";
        public const string Revealed = "Revealed";
        public const string Players = "Players";
        public const string Exit = "Exit";
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is string s)
            return base.Equals((CellState)s);
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        // default implementation since the equals only checks for the implicit string cast
        return base.GetHashCode();
    }
}

public readonly struct Room
{
    public bool Contains(Coordinate coordinate)
    {
        return IncludedCells.Contains(coordinate);
    }

    public bool Contains(int x, int y)
    {
        return IncludedCells.Contains((x, y));
    }

    Coordinate[] IncludedCells { get; }

    private Room(Coordinate[] includedCells)
    {
        IncludedCells = includedCells;
    }

    public static Room From(params Coordinate[] includedCells)
    {
        return new(includedCells);
    } 
}

public readonly struct Coordinate
{
    public bool Valid => !Equals(Invalid);
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Coordinate Invalid { get; } = (-1, -1);

    public static implicit operator Coordinate((int x, int y) c)
    {
        return new(c.x, c.y);
    }

    public static implicit operator (int x, int y)(Coordinate c)
    {
        return (c.X, c.Y);
    }

    public override string ToString() => Valid ? $"{X}|{Y}" : "Invalid";
}