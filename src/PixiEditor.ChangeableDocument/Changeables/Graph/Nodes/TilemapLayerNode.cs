using System;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Tilemap;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("TilemapLayer")]
public class TilemapLayerNode : LayerNode
{
    public TilemapData TilemapData { get; private set; }
    public Tileset Tileset { get; private set; }
    
    // Position/Size para el renderer
    public override VecD GetScenePosition(KeyFrameTime time) => 
        new VecD(TilemapData.PixelWidth / 2.0, TilemapData.PixelHeight / 2.0);
        
    public override VecD GetSceneSize(KeyFrameTime time) => 
        new VecD(TilemapData.PixelWidth, TilemapData.PixelHeight);

    public TilemapLayerNode()
    {
        // 1. Inicializar datos de prueba
        TilemapData = new TilemapData(20, 15, 32, 32); 
        Tileset = new Tileset(32, 32);
        
        // 2. Generar un tileset procedural con 5x5 tiles
        Tileset.LoadTestTileset(5, 5);
        
        // 3. Pintar algunos tiles de forma aleatoria para demostrar que funciona
        var random = new Random();
        for(int x = 0; x < TilemapData.GridWidth; x++)
        {
            for(int y = 0; y < TilemapData.GridHeight; y++)
            {
                if (random.Next(10) > 6) // 30% de probabilidad de generar un tile
                {
                    TilemapData.SetTileAt(x, y, random.Next(1, Tileset.TileCount));
                }
            }
        }
        
        MemberName = "Tilemap (Prototipo)";
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        int saved = workingSurface.Save();
        
        // El renderer base de LayerNode hace un Translate al centro. Tenemos que deshacerlo 
        // para dibujar desde la esquina superior izquierda (0,0) (PixiEditor es medio raro con el centro).
        var sceneSize = GetSceneSize(ctx.FrameTime);
        workingSurface.Translate((float)(-sceneSize.X / 2.0), (float)(-sceneSize.Y / 2.0));
        
        // Renderizar tiles
        TilemapRenderer.Render(workingSurface, TilemapData, Tileset);
        
        // Renderizar la grilla por encima
        TilemapRenderer.RenderGrid(workingSurface, TilemapData, new Color(255, 255, 255, 100));

        workingSurface.RestoreToCount(saved);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        // Simplificado para el prototipo
        DrawWithoutFilters(ctx, workingSurface, paint);
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return new RectD(0, 0, TilemapData.PixelWidth, TilemapData.PixelHeight);
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime) => GetTightBounds(frameTime);

    public override Node CreateCopy()
    {
        var copy = new TilemapLayerNode();
        copy.TilemapData = TilemapData.Clone(); // Deberías clonar esto
        return copy;
    }

    public void AddRandomTile()
    {
        // Función rápida para testear desde afuera
        var rand = new Random();
        TilemapData.SetTileAt(rand.Next(TilemapData.GridWidth), rand.Next(TilemapData.GridHeight), rand.Next(1, Tileset.TileCount));
    }
}
