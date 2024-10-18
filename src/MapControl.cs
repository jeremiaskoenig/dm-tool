

using System.Text;

namespace Wisch.FogOfWar;

public class MapControl : Control
{
    public MapControl()
    {
        DoubleBuffered = true;
        GlobalRefresh += instance =>
        {
            if (instance != this)
                Refresh();
        };
        ContextMenuStrip = new ContextMenuStrip();
        ContextMenuStrip.Items.Add("Spielermarker hierhin bewegen");
        ContextMenuStrip.Items.Add("Ausgang platzieren");
        ContextMenuStrip.Opening += (s, e) =>
        {
            e.Cancel = !canShowContextMenu || dragEndLocation != null;
        };
    }

    private Map map;
    private static readonly Dictionary<Map, float> zooms = new();
    private static readonly Dictionary<Map, Coordinate> offsets = new();

    private static event Action<MapControl> GlobalRefresh;

    public Map Map
    {
        get => map;
        set 
        {
            map = value;
            Offset = ((Width - map.Image.Width) / 2, (Height - map.Image.Height) / 2);
            Zoom = 1;
            Refresh();
        }
    }

    public bool IsPreview { get; set; }

    public float Zoom
    {
        get => zooms[map];
        set
        {
            zooms[map] = value;
        }
    }
    public Coordinate Offset
    {
        get => offsets[map];
        set
        {
            offsets[map] = value;
        }
    }

    private bool isDragLeftClick = false;
    private Nullable<Point> dragStartLocation = null;
    private Nullable<Point> dragEndLocation = null;

    private bool canShowContextMenu = true;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (map == null)
            return;

        isDragLeftClick = e.Button == MouseButtons.Left;
        dragStartLocation = e.Location;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (map == null)
            return;

        if (dragStartLocation != null)
        {
            dragEndLocation = e.Location;
            if (isDragLeftClick)
            {
                // Draw Selection Box, nothing necessary here
            }
            else
            {
                // Set Offset
                var deltaX = dragEndLocation.Value.X - dragStartLocation.Value.X;
                var deltaY = dragEndLocation.Value.Y - dragStartLocation.Value.Y;
                Offset = (Offset.X + deltaX, Offset.Y + deltaY);

                //TODO: Change this and save the initial offset to handle left+rightclick stuff
                dragStartLocation = dragEndLocation;
            }
            Refresh();
            GlobalRefresh?.Invoke(this);
        }
        else
        {
            canShowContextMenu = IsPreview && !map.CellFromPixel(e.Location.X, e.Location.Y).Valid;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (map == null)
            return;

        if (dragStartLocation != null && dragEndLocation != null)
        {
            if (isDragLeftClick)
            {
                (var coordStart, var coordEnd) = GetGridSelection();
                if (coordStart.Valid && coordEnd.Valid)
                {
                    bool setHidden = SetSelectionHidden(coordStart, coordEnd);
                    for (int x = coordStart.X; x <= coordEnd.X; x++)
                    {
                        for (int y = coordStart.Y; y <= coordEnd.Y; y++)
                        {
                            if (setHidden)
                                map.UpdateCell(x, y, CellState.DefaultStates.Hidden);
                            else if (map[(x, y)].Equals(CellState.DefaultStates.Hidden))
                                map.UpdateCell(x, y, CellState.DefaultStates.Revealed);
                            //map.ToggleCell(x, y);   
                        }
                    }
                    Refresh();
                    GlobalRefresh?.Invoke(this);
                }
            }
        }
        dragStartLocation = null;
        dragEndLocation = null;
        ContextMenuStrip.Tag = map.CellFromPixel(e.Location.X, e.Location.Y);
        Refresh();
        GlobalRefresh?.Invoke(this);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (map == null)
            return;

        if (!IsPreview || e.Button != MouseButtons.Left)
            return;
        
        
        var cell = map.CellFromPixel((int)((e.Location.X - Offset.X) / Zoom), (int)((e.Location.Y - Offset.Y) / Zoom));
        if (cell.Valid)
        {
            map.ToggleCell(cell.X, cell.Y);
            Refresh();
            GlobalRefresh?.Invoke(this);
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (map == null)
            return;

        float imageX = (e.X - Offset.X) / Zoom;
        float imageY = (e.Y - Offset.Y) / Zoom;

        if (e.Delta > 0)
        {
            Zoom *= 1.5f;
        }
        else
        {
            Zoom /= 1.5f;
        }
        
        Offset = ((int)(e.X - imageX * Zoom), (int)(e.Y - imageY * Zoom));

        Refresh();
        GlobalRefresh?.Invoke(this);
    }

    private Coordinate GridCellAt(int x , int y)
    {
        var xStart = (map.GridOffset.X + Offset.X) * Zoom;
        var yStart = (map.GridOffset.Y + Offset.Y) * Zoom;
        var xEnd = (map.GridOffset.X + Offset.X + map.GridSize.X) * Zoom;
        var yEnd = (map.GridOffset.Y + Offset.Y + map.GridSize.Y) * Zoom;
        
        var xStep = xEnd - xStart;
        var yStep = yEnd - yStart;

        return ((int)(x / xStep), (int)(y / yStep));
    }

    private (Coordinate start, Coordinate end) GetGridSelection(Point start, Point end)
    {
        int dsX = start.X;
        int dsY = start.Y;
        int deX = end.X;
        int deY = end.Y;

        var startX = (int)((Math.Min(dsX, deX) - Offset.X) / Zoom);
        var startY = (int)((Math.Min(dsY, deY) - Offset.Y) / Zoom);
        var endX = (int)((Math.Max(dsX, deX) - Offset.X) / Zoom);
        var endY = (int)((Math.Max(dsY, deY) - Offset.Y) / Zoom);

        FormLogger.Clear();
        FormLogger.Log("corrected");
        FormLogger.Log($"start: {startX}|{startY}");
        FormLogger.Log($"end: {endX}|{endY}");

        return (map.CellFromPixel(startX, startY), map.CellFromPixel(endX, endY));
    }

    private (Coordinate start, Coordinate end) GetGridSelection()
    {
        if (dragStartLocation == null && dragEndLocation == null)
            return (Coordinate.Invalid, Coordinate.Invalid);
        
        return GetGridSelection(dragStartLocation.Value, dragEndLocation.Value);
    }

    private bool SetSelectionHidden(Coordinate coordStart, Coordinate coordEnd)
    {
        FormLogger.Log("");
        FormLogger.Log("grid coords");
        FormLogger.Log($"start: {coordStart}");
        FormLogger.Log($"end: {coordEnd}");
        if (coordStart.Valid && coordEnd.Valid)
        {
            int total = (coordEnd.X - coordStart.X + 1) * (coordEnd.Y - coordStart.Y + 1);
            int hidden = 0;

            for (int x = coordStart.X; x <= coordEnd.X; x++)
            {
                for (int y = coordStart.Y; y <= coordEnd.Y; y++)
                {
                    if (map[(x, y)].Equals(CellState.DefaultStates.Hidden))
                        hidden++;
                }
            }

            return (total - hidden) > hidden;
        }
        return false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (map == null)
            return;
        
        e.Graphics.Clear(Color.Black);
        var mapImage = map.Render(IsPreview);

        int newWidth = (int)(mapImage.Width * Zoom);
        int newHeight = (int)(mapImage.Height * Zoom);

        Rectangle destRect = new(Offset.X, Offset.Y, newWidth, newHeight);

        e.Graphics.DrawImage(mapImage, destRect);

        if (dragStartLocation != null && dragEndLocation != null)
        {
            int dsX = dragStartLocation.Value.X;
            int dsY = dragStartLocation.Value.Y;
            int deX = dragEndLocation.Value.X;
            int deY = dragEndLocation.Value.Y;

            var startX = Math.Min(dsX, deX);
            var startY = Math.Min(dsY, deY);
            var endX = Math.Max(dsX, deX);
            var endY = Math.Max(dsY, deY);

            (var coordStart, var coordEnd) = GetGridSelection();
            var hideSelection = SetSelectionHidden(coordStart, coordEnd);
            var boxBrush = hideSelection ? ControlBrushes.DarkField : ControlBrushes.LightField;
            e.Graphics.FillRectangle(boxBrush, startX, startY, endX - startX, endY - startY);
            e.Graphics.DrawRectangle(Pens.Lime, startX, startY, endX - startX, endY - startY);
        }
    }

    private static class ControlBrushes
    {
        public static readonly Brush LightField = new SolidBrush(Color.FromArgb(64, 255, 255, 255));
        public static readonly Brush DarkField = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
    }
}