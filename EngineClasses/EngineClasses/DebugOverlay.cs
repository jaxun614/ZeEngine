// ============================================================
//  DebugOverlay.cs  (Engine library)
//  Toggle with a single key press (default: I).
//
//  Shows in screen space (no camera transform needed):
//    • FPS (averaged over 60 frames)
//    • Frame time (ms)
//    • Loaded / total chunks
//    • Scene items in LevelDrawManager
//    • Player world position
//    • Player tile (col, row)
//    • Player height level
//    • Tile under cursor: terrain, biome, height, category
//    • Draw call count
//
//  Shows in world space (inside the camera SpriteBatch.Begin):
//    • Chunk borders (coloured grid lines with chunk coords)
//    • Stair tile highlights
//    • Wall face indicators
//
//  Usage:
//    1. Construct once in GamePlayState.Enter():
//         debugOverlay = new DebugOverlay(worldGrid, drawManager, camera);
//
//    2. In GamePlayState.Update():
//         debugOverlay.Update(player);
//
//    3. In GamePlayState.Draw(), INSIDE the world SpriteBatch.Begin block:
//         debugOverlay.DrawWorld();
//       And INSIDE the UI SpriteBatch.Begin block:
//         debugOverlay.DrawUI();
// ============================================================

#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion

namespace ZeEngine
{
    public class DebugOverlay
    {
        // ── References ────────────────────────────────────────────────────────
        private readonly ChunkManager    _chunks;
        private readonly LevelDrawManager _drawManager;
        private readonly Camera          _camera;

        // ── State ────────────────────────────────────────────────────────────
        public bool Visible       { get; private set; } = false;
        public bool ShowChunkGrid { get; private set; } = true;

        // ── FPS tracking ──────────────────────────────────────────────────────
        private const int FpsSampleCount = 60;
        private readonly Queue<float> _frameTimes = new Queue<float>();
        private float _fps       = 0f;
        private float _frameMs   = 0f;

        // ── Snapshot data (refreshed each Update) ─────────────────────────────
        private Vector2 _playerPos;
        private Vector2 _playerTile;
        private int     _playerHeightLevel;
        private int     _loadedChunks;
        private int     _totalChunks;
        private int     _sceneItemCount;
        private int     _drawCallCount;

        // Tile under mouse cursor
        private string _cursorTerrain   = " ";
        private string _cursorBiome     = " ";
        private string _cursorCategory  = " ";
        private float  _cursorHeight    = 0f;
        private int    _cursorHeightLvl = 0;
        private bool   _cursorIsStair   = false;
        private bool   _cursorHasWall   = false;

        // ── Layout constants ──────────────────────────────────────────────────
        private const int PanelX       = 10;
        private const int PanelY       = 10;
        private const int LineHeight    = 18;
        private const int PanelPadding  = 8;
        private const int PanelWidth    = 320;

        // Chunk grid colours
        private static readonly Color ChunkBorderColor  = new Color(255, 255,   0, 120);  // yellow
        private static readonly Color ChunkLabelColor   = new Color(255, 255,   0, 200);
        private static readonly Color StairHighlight    = new Color(  0, 255, 100, 160);  // green
        private static readonly Color WallHighlight     = new Color(255,  80,  40, 140);  // orange-red

        // Panel colours
        private static readonly Color PanelBg    = new Color( 0,  0,  0, 180);
        private static readonly Color HeaderColor = new Color(255, 220,  80);   // amber
        private static readonly Color LabelColor  = new Color(180, 180, 180);
        private static readonly Color ValueColor  = Color.White;
        private static readonly Color WarnColor   = new Color(255, 100,  80);   // red-ish

        // ── Constructor ──────────────────────────────────────────────────────

        /// <summary>
        /// Create the debug overlay.
        /// Pass the ChunkManager, LevelDrawManager, and Camera from GamePlayState.
        /// </summary>
        public DebugOverlay(ChunkManager chunkManager,
                            LevelDrawManager drawManager,
                            Camera camera)
        {
            _chunks      = chunkManager;
            _drawManager = drawManager;
            _camera      = camera;
        }

        // ── Update ────────────────────────────────────────────────────────────

        /// <summary>
        /// Call every frame from GamePlayState.Update().
        /// player can be null if called from CreativeModeState with no live player.
        /// playerHeightLevel is the player's current discrete height level.
        /// </summary>
        public void Update(Vector2 playerWorldPos, int playerHeightLevel)
        {
            // Toggle visibility
            if (Globals.keyboard.GetSinglePress("I"))
                Visible = !Visible;

            // Toggle chunk grid independently while overlay is open
            if (Visible && Globals.keyboard.GetSinglePress("C"))
                ShowChunkGrid = !ShowChunkGrid;

            // Always track FPS even when hidden (smoother on first open)
            float elapsed = (float)Globals.gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsed > 0)
            {
                _frameTimes.Enqueue(elapsed);
                if (_frameTimes.Count > FpsSampleCount) _frameTimes.Dequeue();

                float avg = 0f;
                foreach (float t in _frameTimes) avg += t;
                avg      /= _frameTimes.Count;
                _fps      = avg > 0 ? 1f / avg : 0f;
                _frameMs  = avg * 1000f;
            }

            if (!Visible) return;

            // Snapshot world state
            _playerPos         = playerWorldPos;
            _playerHeightLevel = playerHeightLevel;
            _sceneItemCount    = _drawManager.drawables.Count;
            _drawCallCount     = _drawManager.drawables.Count;  // 1:1 in this engine

            // Chunk stats
            int loaded = 0, total = 0;
            for (int cc = 0; cc < _chunks.Config.ChunksX; cc++)
                for (int cr = 0; cr < _chunks.Config.ChunksY; cr++)
                {
                    total++;
                    if (_chunks.GetChunk(cc, cr)?.IsLoaded == true) loaded++;
                }
            _loadedChunks = loaded;
            _totalChunks  = total;

            // Player tile
            _playerTile = _chunks.WorldPosToGrid(playerWorldPos);

            // Tile under mouse cursor
            MouseState ms       = Mouse.GetState();
            Matrix inv          = Matrix.Invert(_camera.Transform);
            Vector2 mouseWorld  = Vector2.Transform(new Vector2(ms.X, ms.Y), inv);
            ITile rawTile       = _chunks.GetTileAtWorldPos(mouseWorld);

            // Use reflection-free duck typing via interface + casting
            // The engine only knows ITile; game-specific fields accessed via dynamic cast
            // stored as object so we can safely null-check in DrawUI.
            _cursorTerrain  = "-";
            _cursorBiome    = "-";
            _cursorCategory = "-";
            _cursorHeight   = 0f;
            _cursorHeightLvl = 0;
            _cursorIsStair  = false;
            _cursorHasWall  = false;

            if (rawTile is IDebugInspectable d)
            {
                _cursorTerrain   = d.TerrainName;
                _cursorBiome     = d.BiomeName;
                _cursorCategory  = d.CategoryName;
                _cursorHeight    = d.Height;
                _cursorHeightLvl = d.HeightLevel;
                _cursorIsStair   = d.IsStair;
                _cursorHasWall   = d.HasWallFace;
            }
        }

        // ── DrawWorld (call inside camera SpriteBatch.Begin) ─────────────────

        /// <summary>
        /// Draw world-space debug visuals (chunk grid, stair/wall highlights).
        /// Call inside the SpriteBatch.Begin block that uses camera.Transform.
        /// </summary>
        public void DrawWorld()
        {
            if (!Visible || Globals.pixel == null) return;

            WorldConfig cfg = _chunks.Config;
            Rectangle   wb  = _chunks.WorldBoundsPixels;

            if (ShowChunkGrid)
            {
                int chunkPx = cfg.ChunkSize * cfg.TilePixels;

                for (int cc = 0; cc <= cfg.ChunksX; cc++)
                {
                    int x = wb.Left + cc * chunkPx;
                    DrawWorldLine(new Vector2(x, wb.Top), new Vector2(x, wb.Bottom),
                                  ChunkBorderColor, 2);
                }
                for (int cr = 0; cr <= cfg.ChunksY; cr++)
                {
                    int y = wb.Top + cr * chunkPx;
                    DrawWorldLine(new Vector2(wb.Left, y), new Vector2(wb.Right, y),
                                  ChunkBorderColor, 2);
                }

                // Chunk coord labels — only for loaded chunks to avoid clutter
                if (Globals.font != null)
                {
                    for (int cc = 0; cc < cfg.ChunksX; cc++)
                    {
                        for (int cr = 0; cr < cfg.ChunksY; cr++)
                        {
                            if (_chunks.GetChunk(cc, cr)?.IsLoaded != true) continue;

                            float lx = wb.Left + cc * chunkPx + 4;
                            float ly = wb.Top  + cr * chunkPx + 4;
                            Globals.spriteBatch.DrawString(
                                Globals.font,
                                $"{cc},{cr}",
                                new Vector2(lx, ly),
                                ChunkLabelColor * 0.85f,
                                0f, Vector2.Zero, 0.55f,
                                SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            // Stair + wall highlights — scan loaded chunks only
            for (int cc = 0; cc < cfg.ChunksX; cc++)
            {
                for (int cr = 0; cr < cfg.ChunksY; cr++)
                {
                    Chunk chunk = _chunks.GetChunk(cc, cr);
                    if (chunk == null || !chunk.IsLoaded) continue;

                    for (int lc = 0; lc < cfg.ChunkSize; lc++)
                    {
                        for (int lr = 0; lr < cfg.ChunkSize; lr++)
                        {
                            ITile raw = chunk.GetLocal(lc, lr);
                            if (raw == null) continue;

                            if (raw is IDebugInspectable dbg)
                            {
                                if (dbg.IsStair)
                                    DrawWorldTileHighlight(raw.WorldPos, cfg.TilePixels, StairHighlight);
                                else if (dbg.HasWallFace)
                                    DrawWorldTileHighlight(raw.WorldPos, cfg.TilePixels, WallHighlight);
                            }
                        }
                    }
                }
            }
        }

        // ── DrawUI (call inside UI SpriteBatch.Begin, no camera transform) ───

        /// <summary>
        /// Draw the screen-space debug panel.
        /// Call inside the SpriteBatch.Begin block WITHOUT a camera transform.
        /// </summary>
        public void DrawUI()
        {
            if (!Visible || Globals.font == null || Globals.pixel == null) return;

            var lines = BuildLines();
            int panelH = lines.Count * LineHeight + PanelPadding * 2;

            // Panel background
            Globals.spriteBatch.Draw(Globals.pixel,
                new Rectangle(PanelX - PanelPadding,
                               PanelY - PanelPadding,
                               PanelWidth,
                               panelH),
                PanelBg);

            // Draw each line
            int y = PanelY;
            foreach (var (label, value, color) in lines)
            {
                if (label == null)   // section divider
                {
                    DrawHRule(PanelX, y + LineHeight / 2 - 1, PanelWidth - PanelPadding * 2);
                    y += LineHeight;
                    continue;
                }

                if (value == null)   // header
                {
                    Globals.spriteBatch.DrawString(Globals.font, label,
                        new Vector2(PanelX, y), HeaderColor,
                        0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
                    y += LineHeight;
                    continue;
                }

                // Label + value pair
                Globals.spriteBatch.DrawString(Globals.font, label + ":",
                    new Vector2(PanelX, y), LabelColor,
                    0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);

                Globals.spriteBatch.DrawString(Globals.font, value,
                    new Vector2(PanelX + 150, y), color,
                    0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);

                y += LineHeight;
            }

            // Key hint strip at bottom
            string hint = "[I] Close  [C] Toggle chunk grid";
            Globals.spriteBatch.DrawString(Globals.font, hint,
                new Vector2(PanelX, y + 2), LabelColor * 0.7f,
                0f, Vector2.Zero, 0.55f, SpriteEffects.None, 0f);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        // (label, value, color) — label=null → divider, value=null → section header
        private List<(string label, string value, Color color)> BuildLines()
        {
            var L = new List<(string, string, Color)>();

            // ── Performance ───────────────────────────────────────────────────
            L.Add(("- PERFORMANCE", null, Color.White));
            L.Add(("FPS",       $"{_fps:F1}", _fps < 30 ? WarnColor : ValueColor));
            L.Add(("Frame",     $"{_frameMs:F2} ms", ValueColor));

            L.Add((null, null, Color.White));  // divider

            // ── World ─────────────────────────────────────────────────────────
            L.Add(("- WORLD", null, Color.White));
            L.Add(("Chunks loaded", $"{_loadedChunks} / {_totalChunks}", ValueColor));
            L.Add(("Scene items",   $"{_sceneItemCount}", ValueColor));
            L.Add(("Draw slots",    $"{_drawCallCount}", ValueColor));

            L.Add((null, null, Color.White));

            // ── Player ────────────────────────────────────────────────────────
            L.Add(("- PLAYER", null, Color.White));
            L.Add(("World pos",   $"({_playerPos.X:F0}, {_playerPos.Y:F0})", ValueColor));
            L.Add(("Tile",        $"col {_playerTile.X:F0}  row {_playerTile.Y:F0}", ValueColor));
            L.Add(("Height level",$"{_playerHeightLevel}  ({LevelName(_playerHeightLevel)})", ValueColor));

            L.Add((null, null, Color.White));

            // ── Cursor tile ───────────────────────────────────────────────────
            L.Add(("- TILE UNDER CURSOR", null, Color.White));
            L.Add(("Terrain",  _cursorTerrain,              ValueColor));
            L.Add(("Category", _cursorCategory,             ValueColor));
            L.Add(("Biome",    _cursorBiome,                ValueColor));
            L.Add(("Height",   $"{_cursorHeight:F3}",       ValueColor));
            L.Add(("Level",    $"{_cursorHeightLvl}  ({LevelName(_cursorHeightLvl)})", ValueColor));
            L.Add(("Stair",    _cursorIsStair ? "YES" : "no",
                               _cursorIsStair ? StairHighlight : LabelColor));
            L.Add(("Wall face",_cursorHasWall ? "YES" : "no",
                               _cursorHasWall ? WallHighlight  : LabelColor));

            return L;
        }

        private static string LevelName(int level)
        {
            switch (level)
            {
                case HeightLevel.Water:    return "Water";
                case HeightLevel.Ground:   return "Ground";
                case HeightLevel.Highland: return "Highland";
                case HeightLevel.Mountain: return "Mountain";
                default:                   return "?";
            }
        }

        private void DrawHRule(int x, int y, int width)
        {
            Globals.spriteBatch.Draw(Globals.pixel,
                new Rectangle(x, y, width, 1),
                new Color(120, 120, 120, 140));
        }

        /// <summary>Draw a 1-pixel-wide line in world space using Globals.pixel.</summary>
        private void DrawWorldLine(Vector2 a, Vector2 b, Color color, int thickness = 1)
        {
            Vector2 delta  = b - a;
            float   length = delta.Length();
            float   angle  = (float)Math.Atan2(delta.Y, delta.X);

            Globals.spriteBatch.Draw(
                Globals.pixel,
                a,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f);
        }

        /// <summary>Draw a hollow rectangle highlight over a tile in world space.</summary>
        private void DrawWorldTileHighlight(Vector2 tileCenter, int tilePixels, Color color)
        {
            int tp   = tilePixels;
            int x    = (int)(tileCenter.X - tp / 2);
            int y    = (int)(tileCenter.Y - tp / 2);
            int t    = 2;   // border thickness

            // Top, bottom, left, right
            Globals.spriteBatch.Draw(Globals.pixel, new Rectangle(x,          y,          tp, t),  color);
            Globals.spriteBatch.Draw(Globals.pixel, new Rectangle(x,          y + tp - t, tp, t),  color);
            Globals.spriteBatch.Draw(Globals.pixel, new Rectangle(x,          y,          t,  tp), color);
            Globals.spriteBatch.Draw(Globals.pixel, new Rectangle(x + tp - t, y,          t,  tp), color);
        }
    }

    // ── IDebugInspectable ──────────────────────────────────────────────────────
    // WorldTile implements this so DebugOverlay can read game-specific fields
    // without the engine library referencing the game project.

    /// <summary>
    /// Engine-side interface that WorldTile implements so DebugOverlay
    /// can read tile metadata without a circular project reference.
    /// Add this to the Engine project alongside ITile.
    /// </summary>
    public interface IDebugInspectable
    {
        string TerrainName  { get; }
        string BiomeName    { get; }
        string CategoryName { get; }
        float  Height       { get; }
        int    HeightLevel  { get; }
        bool   IsStair      { get; }
        bool   HasWallFace  { get; }
    }
}