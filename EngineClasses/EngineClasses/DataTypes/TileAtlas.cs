// ============================================================
//  TileAtlas.cs  (Engine library)
//  Slices a single large spritesheet into a grid of smaller
//  Texture2D sub-textures, then registers them with the
//  content system so they can be referenced by path string
//  exactly like individually loaded textures.
//
//  Two usage patterns:
//
//  A) Index-based (quick):
//       var atlas = new TileAtlas("2D\\Tilesets\\Terrain", 32, 32);
//       Texture2D grassTex = atlas.GetByIndex(0, 0);   // col 0, row 0
//
//  B) Named (recommended — integrates with TileStateDef):
//       atlas.Register(0, 0, "2D\\Grid\\Tiles\\GrassTile1");
//       atlas.Register(0, 1, "2D\\Grid\\Tiles\\GrassTile2");
//       atlas.RegisterAll();   // auto-names every cell as AtlasPath/R{row}C{col}
//
//  After calling Register() the sub-texture is available via
//  Globals.content.Load<Texture2D>(name) just like a normal asset,
//  because TileAtlas inserts it into a shared cache that Basic2d
//  can look up through the TileAtlasCache singleton.
//
//  Scene-object variant:
//       var atlas = new TileAtlas("2D\\Tilesets\\Objects", 32, 48);
//  Object tiles are typically taller than they are wide — just
//  pass the correct cellWidth / cellHeight.
//
//  Margin / spacing support:
//  Many export tools add padding between cells.  Use the margin
//  and spacing parameters to account for this.
//       new TileAtlas(path, 16, 16, margin: 1, spacing: 1)
// ============================================================

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZeEngine
{
    public class TileAtlas
    {
        // members
        // img path
        // cell size
        // margin
        // spacing

        // ── Configuration ─────────────────────────────────────────────────────

        // Content path of the source spritesheet.
        public readonly string AtlasPath;

        // Pixel width of one cell in the atlas.
        public readonly int CellWidth;

        // Pixel height of one cell in the atlas.
        public readonly int CellHeight;

        // Pixels of border around the entire sheet (same on all sides).
        public readonly int Margin;

        // Pixels between each cell (between columns and rows).
        public readonly int Spacing;

        // ── Derived ───────────────────────────────────────────────────────────

        public int Columns { get; private set; }
        public int Rows    { get; private set; }

        // ── Private ───────────────────────────────────────────────────────────

        private Texture2D                     _source; // The original spritesheet texture
        private readonly Dictionary<(int,int), Texture2D> _cache = new Dictionary<(int, int), Texture2D>(); // Cache of extracted sub-textures

        // Maps registered name → (col, row) for lookup by content path
        private readonly Dictionary<string, (int col, int row)> _nameMap
            = new Dictionary<string, (int, int)>(); // Registered names for cells

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Load a tile atlas from the content pipeline.
        /// Call after Globals.content is initialised (i.e. inside LoadContent).
        /// </summary>
        /// <param name="atlasContentPath">Content path of the spritesheet.</param>
        /// <param name="cellWidth">Width of one tile/object cell in pixels.</param>
        /// <param name="cellHeight">Height of one tile/object cell in pixels.</param>
        /// <param name="margin">Border around the entire sheet (pixels, default 0).</param>
        /// <param name="spacing">Gap between cells (pixels, default 0).</param>
        public TileAtlas(string atlasContentPath,
                         int    cellWidth,
                         int    cellHeight,
                         int    margin  = 0,
                         int    spacing = 0)
        {
            AtlasPath  = atlasContentPath;
            CellWidth  = cellWidth;
            CellHeight = cellHeight;
            Margin     = margin;
            Spacing    = spacing;

            _source = Globals.content.Load<Texture2D>(atlasContentPath);

            // Derive grid dimensions from texture size
            int usableW = _source.Width  - Margin * 2; // usable width of the atlas (excluding margins)
            int usableH = _source.Height - Margin * 2; // usable height of the atlas (excluding margins)
            int stepW   = CellWidth  + Spacing; // width of one cell + spacing
            int stepH   = CellHeight + Spacing; // height of one cell + spacing

            Columns = usableW / stepW; // number of columns in the atlas
            Rows    = usableH / stepH; // number of rows in the atlas

            // Register atlas so Basic2d can resolve names registered on it
            TileAtlasCache.Register(this);
        }

        // ── Index-based access ────────────────────────────────────────────────

        // Get the sub-texture at grid position (col, row).
        // Textures are extracted on first access and cached.
        public Texture2D GetByIndex(int col, int row)
        {
            // check bounds
            if (col < 0 || row < 0 || col >= Columns || row >= Rows)
                throw new ArgumentOutOfRangeException(
                    $"TileAtlas '{AtlasPath}': ({col},{row}) out of range ({Columns}x{Rows})");

            // check cache first
            var key = (col, row); // tuple key for the cache
            if (_cache.TryGetValue(key, out Texture2D cached)) return cached; // return cached if already extracted

            Texture2D sub = ExtractCell(col, row); // extract the sub-texture from the source atlas
            _cache[key] = sub; // cache it for future access
            return sub; // return the extracted sub-texture
        }

        // ── Named registration ────────────────────────────────────────────────

        // Register a cell under a content path name so it can be looked up
        // exactly like a standalone texture (e.g. by Basic2d / TileStateDef).
        public void Register(string contentName, int col, int row)
        {
            // check bounds
            if (col < 0 || row < 0 || col >= Columns || row >= Rows)
                throw new ArgumentOutOfRangeException(
                    $"TileAtlas '{AtlasPath}': ({col},{row}) out of range ({Columns}x{Rows})");

            _nameMap[contentName] = (col, row); // map the content name to the cell's grid position

            // Pre-extract and cache so lookups are instant at runtime
            var key = (col, row);
            if (!_cache.ContainsKey(key))
                _cache[key] = ExtractCell(col, row);
        }

        // Auto-register every cell in the atlas using the naming convention:
        //   AtlasPath/R{row}C{col}
        // e.g. "2D\\Tilesets\\Terrain/R0C0", "2D\\Tilesets\\Terrain/R0C1", …
        //
        // Useful when you want to reference cells by index without manual naming.
        public void RegisterAll()
        {
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Columns; col++)
                    Register($"{AtlasPath}/R{row}C{col}", col, row);
        }

        /// <summary>
        /// Look up a sub-texture by the name it was registered with.
        /// Returns null if the name is unknown.
        /// </summary>
        public Texture2D GetByName(string contentName)
        {
            if (_nameMap.TryGetValue(contentName, out var idx))
                return GetByIndex(idx.col, idx.row);
            return null;
        }

        // ── Bulk helpers ──────────────────────────────────────────────────────

        // Register a row of cells as sequential terrain tile variants.
        // Names them baseName + 1, baseName + 2, …
        // Example: RegisterRow(1, "2D\\Grid\\Tiles\\GrassTile") registers
        //          cells (0,1)..(Columns-1,1) as GrassTile1, GrassTile2, …
        public void RegisterRow(int row, string baseName)
        {
            for (int col = 0; col < Columns; col++)
                Register($"{baseName}{col + 1}", col, row);
        }

        // Register a single column of cells as sequential variants.
        // Names them baseName + 1, baseName + 2, …
        public void RegisterColumn(int col, string baseName)
        {
            for (int row = 0; row < Rows; row++)
                Register($"{baseName}{row + 1}", col, row);
        }

        // ── Private: pixel extraction ─────────────────────────────────────────

        private Texture2D ExtractCell(int col, int row)
        {
            int x = Margin + col * (CellWidth  + Spacing);
            int y = Margin + row * (CellHeight + Spacing);

            var region = new Color[CellWidth * CellHeight];
            _source.GetData(0, new Rectangle(x, y, CellWidth, CellHeight),
                            region, 0, region.Length);

            var sub = new Texture2D(Globals.graphicsDevice ?? _source.GraphicsDevice,
                                    CellWidth, CellHeight);
            sub.SetData(region);
            return sub;
        }

       
    }

    // =========================================================================
    //  TileAtlasCache 
    //  A singleton registry that Basic2d queries when it can't find a texture
    //  via the normal content pipeline.  TileAtlas registers itself here on
    //  construction, so all named cells become transparently resolvable.
    // =========================================================================

    public static class TileAtlasCache
    {
        private static readonly List<TileAtlas> _atlases = new List<TileAtlas>(); // All registered atlases

        // Register a new atlas so its named cells can be resolved by Basic2d.
        public static void Register(TileAtlas atlas)
        {
            // Avoid duplicates
            if (!_atlases.Contains(atlas))
                _atlases.Add(atlas);
        }


        // Try to resolve a content name to a Texture2D via any registered atlas.
        // Returns null if no atlas knows the name.
        public static Texture2D Resolve(string contentName)
        {
            // Search all registered atlases for the name
            foreach (var atlas in _atlases)
            {
                Texture2D tex = atlas.GetByName(contentName); // returns null if not found
                if (tex != null) return tex; // found it, return the texture
            }
            return null; // not found in any atlas
        }

        // Clear all registered atlases (call on scene unload if needed). 
        public static void Clear() => _atlases.Clear();
    }

   
}