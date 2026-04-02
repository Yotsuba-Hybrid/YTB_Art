using Drawie.Numerics;

namespace PixiEditor.Tilemap;

/// <summary>
/// Represents the tile data of a tilemap: a 2D grid where each cell contains a tile ID.
/// Tile ID 0 = empty (no tile placed). Tile IDs >= 1 reference entries in the associated Tileset.
/// </summary>
public class TilemapData
{
    private int[,] _grid;

    /// <summary>
    /// Number of columns (horizontal cells) in the grid.
    /// </summary>
    public int GridWidth { get; private set; }

    /// <summary>
    /// Number of rows (vertical cells) in the grid.
    /// </summary>
    public int GridHeight { get; private set; }

    /// <summary>
    /// Width of each tile in pixels.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Height of each tile in pixels.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Total pixel width of the tilemap (GridWidth * TileWidth).
    /// </summary>
    public int PixelWidth => GridWidth * TileWidth;

    /// <summary>
    /// Total pixel height of the tilemap (GridHeight * TileHeight).
    /// </summary>
    public int PixelHeight => GridHeight * TileHeight;

    public TilemapData(int gridWidth, int gridHeight, int tileWidth, int tileHeight)
    {
        if (gridWidth <= 0) throw new ArgumentException("Must be positive", nameof(gridWidth));
        if (gridHeight <= 0) throw new ArgumentException("Must be positive", nameof(gridHeight));
        if (tileWidth <= 0) throw new ArgumentException("Must be positive", nameof(tileWidth));
        if (tileHeight <= 0) throw new ArgumentException("Must be positive", nameof(tileHeight));

        GridWidth = gridWidth;
        GridHeight = gridHeight;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        _grid = new int[gridWidth, gridHeight];
    }

    /// <summary>
    /// Gets the tile ID at the given grid position. Returns 0 (empty) if out of bounds.
    /// </summary>
    public int GetTileAt(int gridX, int gridY)
    {
        if (!IsInBounds(gridX, gridY))
            return 0;

        return _grid[gridX, gridY];
    }

    /// <summary>
    /// Sets the tile ID at the given grid position. Returns false if out of bounds.
    /// </summary>
    public bool SetTileAt(int gridX, int gridY, int tileId)
    {
        if (!IsInBounds(gridX, gridY))
            return false;

        _grid[gridX, gridY] = tileId;
        return true;
    }

    /// <summary>
    /// Erases (sets to 0) the tile at the given grid position.
    /// </summary>
    public bool EraseTileAt(int gridX, int gridY)
    {
        return SetTileAt(gridX, gridY, 0);
    }

    /// <summary>
    /// Fills a rectangular region of the grid with the given tile ID.
    /// </summary>
    public void FillRect(int startX, int startY, int width, int height, int tileId)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                SetTileAt(x, y, tileId);
            }
        }
    }

    /// <summary>
    /// Clears the entire grid (all cells become empty).
    /// </summary>
    public void Clear()
    {
        Array.Clear(_grid);
    }

    /// <summary>
    /// Converts a pixel position to a grid cell coordinate.
    /// </summary>
    public VecI PixelToGrid(VecD pixelPosition)
    {
        int gridX = (int)Math.Floor(pixelPosition.X / TileWidth);
        int gridY = (int)Math.Floor(pixelPosition.Y / TileHeight);
        return new VecI(gridX, gridY);
    }

    /// <summary>
    /// Converts a grid cell coordinate to the pixel position of its top-left corner.
    /// </summary>
    public VecI GridToPixel(int gridX, int gridY)
    {
        return new VecI(gridX * TileWidth, gridY * TileHeight);
    }

    /// <summary>
    /// Checks if a grid coordinate is within bounds.
    /// </summary>
    public bool IsInBounds(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < GridWidth &&
               gridY >= 0 && gridY < GridHeight;
    }

    /// <summary>
    /// Resizes the grid, preserving existing tile data where possible.
    /// </summary>
    public void Resize(int newWidth, int newHeight)
    {
        if (newWidth <= 0 || newHeight <= 0)
            throw new ArgumentException("Grid dimensions must be positive");

        var newGrid = new int[newWidth, newHeight];

        int copyWidth = Math.Min(GridWidth, newWidth);
        int copyHeight = Math.Min(GridHeight, newHeight);

        for (int x = 0; x < copyWidth; x++)
        {
            for (int y = 0; y < copyHeight; y++)
            {
                newGrid[x, y] = _grid[x, y];
            }
        }

        _grid = newGrid;
        GridWidth = newWidth;
        GridHeight = newHeight;
    }

    /// <summary>
    /// Creates a deep copy of the tilemap data.
    /// </summary>
    public TilemapData Clone()
    {
        var clone = new TilemapData(GridWidth, GridHeight, TileWidth, TileHeight);
        Array.Copy(_grid, clone._grid, _grid.Length);
        return clone;
    }

    /// <summary>
    /// Returns a snapshot of the raw grid data (for serialization/undo).
    /// </summary>
    public int[,] GetGridSnapshot()
    {
        var snapshot = new int[GridWidth, GridHeight];
        Array.Copy(_grid, snapshot, _grid.Length);
        return snapshot;
    }

    /// <summary>
    /// Restores grid data from a snapshot (for undo).
    /// </summary>
    public void RestoreFromSnapshot(int[,] snapshot)
    {
        if (snapshot.GetLength(0) != GridWidth || snapshot.GetLength(1) != GridHeight)
            throw new ArgumentException("Snapshot dimensions don't match grid dimensions");

        Array.Copy(snapshot, _grid, _grid.Length);
    }
}
