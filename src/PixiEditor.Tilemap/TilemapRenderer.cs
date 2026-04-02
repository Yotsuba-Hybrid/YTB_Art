using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.Tilemap;

/// <summary>
/// Renders a TilemapData + Tileset onto a Drawie Canvas.
/// Handles drawing tile sprites from the tileset at their grid positions,
/// and optionally rendering the tile grid overlay.
/// </summary>
public static class TilemapRenderer
{
    /// <summary>
    /// Renders the entire tilemap onto the target canvas.
    /// Each non-empty cell draws the corresponding tile from the tileset.
    /// </summary>
    public static void Render(Canvas canvas, TilemapData tilemapData, Tileset tileset)
    {
        if (tileset.SourceTexture == null)
            return;

        using var paint = new Paint();
        paint.IsAntiAliased = false;

        var sourceTexture = tileset.SourceTexture;

        for (int gy = 0; gy < tilemapData.GridHeight; gy++)
        {
            for (int gx = 0; gx < tilemapData.GridWidth; gx++)
            {
                int tileId = tilemapData.GetTileAt(gx, gy);
                if (tileId == 0) continue; // empty cell

                var entry = tileset.GetTile(tileId);
                if (entry == null) continue;

                // Source rectangle in the tileset image
                var srcRect = new RectD(
                    entry.SourcePosition.X,
                    entry.SourcePosition.Y,
                    tileset.TileWidth,
                    tileset.TileHeight);

                // Destination rectangle on the canvas
                var dstRect = new RectD(
                    gx * tilemapData.TileWidth,
                    gy * tilemapData.TileHeight,
                    tilemapData.TileWidth,
                    tilemapData.TileHeight);

                // Draw the tile sprite from the tileset
                using var snapshot = sourceTexture.DrawingSurface.Snapshot(
                    new RectI(
                        (int)srcRect.X, (int)srcRect.Y,
                        (int)srcRect.Width, (int)srcRect.Height));

                if (snapshot != null)
                {
                    canvas.DrawImage(snapshot,
                        (float)dstRect.X, (float)dstRect.Y,
                        SamplingOptions.Default, paint);
                }
            }
        }
    }

    /// <summary>
    /// Renders a grid overlay showing the tile cell boundaries.
    /// </summary>
    public static void RenderGrid(Canvas canvas, TilemapData tilemapData, Color gridColor, float lineWidth = 1f)
    {
        using var paint = new Paint();
        paint.Color = gridColor;
        paint.Style = PaintStyle.Stroke;
        paint.StrokeWidth = lineWidth;
        paint.IsAntiAliased = false;

        int totalWidth = tilemapData.PixelWidth;
        int totalHeight = tilemapData.PixelHeight;

        // Vertical lines
        for (int x = 0; x <= tilemapData.GridWidth; x++)
        {
            float px = x * tilemapData.TileWidth;
            canvas.DrawLine(new VecD(px, 0), new VecD(px, totalHeight), paint);
        }

        // Horizontal lines
        for (int y = 0; y <= tilemapData.GridHeight; y++)
        {
            float py = y * tilemapData.TileHeight;
            canvas.DrawLine(new VecD(0, py), new VecD(totalWidth, py), paint);
        }
    }

    /// <summary>
    /// Renders a highlight on a specific grid cell (e.g., cursor hover).
    /// </summary>
    public static void RenderCellHighlight(Canvas canvas, TilemapData tilemapData,
        int gridX, int gridY, Color highlightColor)
    {
        if (!tilemapData.IsInBounds(gridX, gridY))
            return;

        using var paint = new Paint();
        paint.Color = highlightColor;
        paint.Style = PaintStyle.Fill;

        float px = gridX * tilemapData.TileWidth;
        float py = gridY * tilemapData.TileHeight;

        canvas.DrawRect(px, py, tilemapData.TileWidth, tilemapData.TileHeight, paint);
    }

    /// <summary>
    /// Renders a single tile preview at a specific position (for cursor preview while painting).
    /// </summary>
    public static void RenderTilePreview(Canvas canvas, Tileset tileset,
        int tileId, float destX, float destY, float opacity = 0.6f)
    {
        if (tileset.SourceTexture == null || tileId <= 0)
            return;

        var entry = tileset.GetTile(tileId);
        if (entry == null) return;

        using var paint = new Paint();
        paint.Color = new Color(255, 255, 255, (byte)(opacity * 255));
        paint.IsAntiAliased = false;

        using var snapshot = tileset.SourceTexture.DrawingSurface.Snapshot(
            new RectI(
                entry.SourcePosition.X, entry.SourcePosition.Y,
                tileset.TileWidth, tileset.TileHeight));

        if (snapshot != null)
        {
            canvas.DrawImage(snapshot, destX, destY, SamplingOptions.Default, paint);
        }
    }
}
