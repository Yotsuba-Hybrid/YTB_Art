using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.Tilemap;

/// <summary>
/// Represents a tileset: a source image subdivided into a grid of tiles.
/// The tileset owns the source texture and provides methods to access individual tiles.
/// </summary>
public class Tileset : IDisposable
{
    private readonly List<TilesetEntry> _entries = new();
    private bool _disposed;

    /// <summary>
    /// The source image containing all tiles arranged in a grid.
    /// </summary>
    public Texture? SourceTexture { get; private set; }

    /// <summary>
    /// Width of each tile in pixels.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Height of each tile in pixels.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Number of tile columns in the source image.
    /// </summary>
    public int Columns { get; private set; }

    /// <summary>
    /// Number of tile rows in the source image.
    /// </summary>
    public int Rows { get; private set; }

    /// <summary>
    /// All tile entries in this tileset.
    /// </summary>
    public IReadOnlyList<TilesetEntry> Entries => _entries;

    /// <summary>
    /// Total number of tiles available.
    /// </summary>
    public int TileCount => _entries.Count;

    /// <summary>
    /// Tile size as a vector.
    /// </summary>
    public VecI TileSize => new(TileWidth, TileHeight);

    public Tileset(int tileWidth, int tileHeight)
    {
        if (tileWidth <= 0) throw new ArgumentException("Tile width must be positive", nameof(tileWidth));
        if (tileHeight <= 0) throw new ArgumentException("Tile height must be positive", nameof(tileHeight));

        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }

    /// <summary>
    /// Loads a source texture and auto-generates tile entries from the grid subdivision.
    /// </summary>
    public void LoadFromTexture(Texture texture)
    {
        SourceTexture?.Dispose();
        SourceTexture = texture;
        _entries.Clear();

        Columns = texture.Size.X / TileWidth;
        Rows = texture.Size.Y / TileHeight;

        int id = 1; // 0 is reserved for "empty"
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var position = new VecI(col * TileWidth, row * TileHeight);
                _entries.Add(new TilesetEntry(id, position, $"Tile_{id}"));
                id++;
            }
        }
    }

    /// <summary>
    /// Creates a simple colored tileset for testing (no external image needed).
    /// Generates a grid of colored tiles programmatically.
    /// </summary>
    public void LoadTestTileset(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;

        var textureSize = new VecI(columns * TileWidth, rows * TileHeight);
        SourceTexture?.Dispose();
        SourceTexture = Texture.ForProcessing(textureSize, ColorSpace.CreateSrgb());

        var canvas = SourceTexture.DrawingSurface.Canvas;
        canvas.Clear();

        _entries.Clear();

        int id = 1;
        var random = new Random(42); // Fixed seed for reproducibility

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                var position = new VecI(col * TileWidth, row * TileHeight);

                // Generate a distinct color for each tile
                byte r = (byte)(50 + random.Next(180));
                byte g = (byte)(50 + random.Next(180));
                byte b = (byte)(50 + random.Next(180));

                using var paint = new Drawie.Backend.Core.Surfaces.PaintImpl.Paint();
                paint.Color = new Drawie.Backend.Core.ColorsImpl.Color(r, g, b);

                var rect = new RectD(position.X + 1, position.Y + 1, TileWidth - 2, TileHeight - 2);
                canvas.DrawRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, paint);

                // Draw a border
                paint.Color = new Drawie.Backend.Core.ColorsImpl.Color(40, 40, 40);
                paint.IsAntiAliased = false;
                paint.Style = Drawie.Backend.Core.Surfaces.PaintImpl.PaintStyle.Stroke;
                paint.StrokeWidth = 1;
                canvas.DrawRect((float)(position.X + 0.5), (float)(position.Y + 0.5),
                    TileWidth - 1, TileHeight - 1, paint);

                _entries.Add(new TilesetEntry(id, position, $"Tile_{id}"));
                id++;
            }
        }
    }

    /// <summary>
    /// Gets a tile entry by its ID. Returns null if not found.
    /// </summary>
    public TilesetEntry? GetTile(int tileId)
    {
        if (tileId <= 0 || tileId > _entries.Count)
            return null;

        return _entries[tileId - 1]; // IDs are 1-based
    }

    /// <summary>
    /// Gets the source rectangle for a given tile ID in the source texture.
    /// </summary>
    public RectI GetTileSourceRect(int tileId)
    {
        var entry = GetTile(tileId);
        if (entry == null)
            return RectI.Empty;

        return new RectI(entry.SourcePosition.X, entry.SourcePosition.Y, TileWidth, TileHeight);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        SourceTexture?.Dispose();
        SourceTexture = null;
    }
}
