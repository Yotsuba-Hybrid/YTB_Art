using Drawie.Numerics;

namespace PixiEditor.Tilemap;

/// <summary>
/// Represents a single entry in a tileset - one tile with its position in the source image.
/// </summary>
public class TilesetEntry
{
    /// <summary>
    /// Unique ID for this tile within the tileset. 0 = empty (no tile).
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Position of this tile in the tileset source image (in pixels, top-left corner).
    /// </summary>
    public VecI SourcePosition { get; }

    /// <summary>
    /// Optional display name for the tile (e.g., "Grass", "Wall_Top").
    /// </summary>
    public string Name { get; set; }

    public TilesetEntry(int id, VecI sourcePosition, string name = "")
    {
        Id = id;
        SourcePosition = sourcePosition;
        Name = name;
    }

    public override string ToString() => $"Tile#{Id} '{Name}' at {SourcePosition}";
}
