using Drawie.Numerics;

namespace PixiEditor.Tilemap;

/// <summary>
/// Provides brush-like operations for painting tiles onto a TilemapData.
/// Supports single-tile placement, line drawing, and rectangle fill.
/// </summary>
public class TilemapBrush
{
    /// <summary>
    /// The currently selected tile ID to paint with.
    /// </summary>
    public int SelectedTileId { get; set; } = 1;

    /// <summary>
    /// The brush mode (how tiles are applied).
    /// </summary>
    public TilemapBrushMode Mode { get; set; } = TilemapBrushMode.Single;

    /// <summary>
    /// Places a single tile at the given grid position.
    /// Returns the previous tile ID (for undo).
    /// </summary>
    public int PlaceTile(TilemapData tilemap, int gridX, int gridY)
    {
        int previousTileId = tilemap.GetTileAt(gridX, gridY);
        tilemap.SetTileAt(gridX, gridY, SelectedTileId);
        return previousTileId;
    }

    /// <summary>
    /// Erases the tile at the given grid position.
    /// Returns the previous tile ID (for undo).
    /// </summary>
    public int EraseTile(TilemapData tilemap, int gridX, int gridY)
    {
        int previousTileId = tilemap.GetTileAt(gridX, gridY);
        tilemap.EraseTileAt(gridX, gridY);
        return previousTileId;
    }

    /// <summary>
    /// Draws a line of tiles from one grid position to another (Bresenham).
    /// Returns a list of (position, previousTileId) for undo.
    /// </summary>
    public List<(VecI Position, int PreviousTileId)> PlaceLine(
        TilemapData tilemap, VecI from, VecI to)
    {
        var changes = new List<(VecI, int)>();

        int x0 = from.X, y0 = from.Y;
        int x1 = to.X, y1 = to.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (tilemap.IsInBounds(x0, y0))
            {
                int prev = tilemap.GetTileAt(x0, y0);
                tilemap.SetTileAt(x0, y0, SelectedTileId);
                changes.Add((new VecI(x0, y0), prev));
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }

        return changes;
    }

    /// <summary>
    /// Fills a rectangular region with the selected tile.
    /// Returns a snapshot of the previous state (for undo).
    /// </summary>
    public int[,] FillRect(TilemapData tilemap, int startX, int startY, int width, int height)
    {
        // Save previous state for undo
        var snapshot = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                snapshot[x, y] = tilemap.GetTileAt(startX + x, startY + y);
            }
        }

        tilemap.FillRect(startX, startY, width, height, SelectedTileId);
        return snapshot;
    }

    /// <summary>
    /// Bucket fill: flood-fills from a starting position, replacing all connected
    /// tiles of the same type with the selected tile.
    /// Returns a list of changed positions (for undo).
    /// </summary>
    public List<(VecI Position, int PreviousTileId)> BucketFill(
        TilemapData tilemap, int startX, int startY)
    {
        var changes = new List<(VecI, int)>();

        if (!tilemap.IsInBounds(startX, startY))
            return changes;

        int targetTileId = tilemap.GetTileAt(startX, startY);
        if (targetTileId == SelectedTileId)
            return changes; // Already the same tile, nothing to do

        var visited = new bool[tilemap.GridWidth, tilemap.GridHeight];
        var queue = new Queue<VecI>();
        queue.Enqueue(new VecI(startX, startY));

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();

            if (!tilemap.IsInBounds(pos.X, pos.Y) || visited[pos.X, pos.Y])
                continue;

            if (tilemap.GetTileAt(pos.X, pos.Y) != targetTileId)
                continue;

            visited[pos.X, pos.Y] = true;
            changes.Add((pos, targetTileId));
            tilemap.SetTileAt(pos.X, pos.Y, SelectedTileId);

            // Enqueue 4-connected neighbors
            queue.Enqueue(new VecI(pos.X + 1, pos.Y));
            queue.Enqueue(new VecI(pos.X - 1, pos.Y));
            queue.Enqueue(new VecI(pos.X, pos.Y + 1));
            queue.Enqueue(new VecI(pos.X, pos.Y - 1));
        }

        return changes;
    }
}

/// <summary>
/// The mode for the tilemap brush tool.
/// </summary>
public enum TilemapBrushMode
{
    /// <summary>
    /// Place one tile at a time (or drag to paint a line).
    /// </summary>
    Single,

    /// <summary>
    /// Draw a line between two points.
    /// </summary>
    Line,

    /// <summary>
    /// Fill a rectangular region.
    /// </summary>
    RectFill,

    /// <summary>
    /// Flood fill (bucket tool).
    /// </summary>
    BucketFill,

    /// <summary>
    /// Erase tiles.
    /// </summary>
    Eraser
}
