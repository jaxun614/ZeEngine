// ============================================================
//  ChunkManager.cs  (Engine library)
//  Owns the full 2-D array of Chunks. Uses ITile so it has
//  no dependency on the game's WorldTile class.
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
    public class ChunkManager
    {
        public readonly WorldConfig Config;

        private readonly Chunk[,] _chunks;

        public readonly Rectangle WorldBoundsPixels;

        // ── Constructor ──────────────────────────────────────────────────────

        public ChunkManager(WorldConfig config)
        {
            Config  = config;
            _chunks = new Chunk[config.ChunksX, config.ChunksY];

            for (int cc = 0; cc < config.ChunksX; cc++)
                for (int cr = 0; cr < config.ChunksY; cr++)
                    _chunks[cc, cr] = new Chunk(cc, cr, config.ChunkSize);

            Vector2 origin = config.WorldOrigin;
            WorldBoundsPixels = new Rectangle(
                (int)origin.X,
                (int)origin.Y,
                config.WorldTilesX * config.TilePixels,
                config.WorldTilesY * config.TilePixels);
        }

        // ── Tile access ──────────────────────────────────────────────────────

        /// <summary>Get tile as ITile at world col/row. Returns null if out of range.</summary>
        public ITile GetTile(int col, int row)
        {
            if (!InBounds(col, row)) return null;
            Chunk c = ChunkOf(col, row);
            return c.GetLocal(col - c.TileColStart, row - c.TileRowStart);
        }

        /// <summary>
        /// Get tile cast to T at world col/row.
        /// Use from the game project: chunkManager.GetTile&lt;WorldTile&gt;(col, row)
        /// </summary>
        public T GetTile<T>(int col, int row) where T : class, ITile
            => GetTile(col, row) as T;

        /// <summary>Store an ITile at world col/row.</summary>
        public void SetTile(int col, int row, ITile tile)
        {
            if (!InBounds(col, row)) return;
            Chunk c = ChunkOf(col, row);
            c.SetLocal(col - c.TileColStart, row - c.TileRowStart, tile);
        }

        /// <summary>Get tile at a pixel world position.</summary>
        public ITile GetTileAtWorldPos(Vector2 worldPos)
        {
            Vector2 g = WorldPosToGrid(worldPos);
            return GetTile((int)g.X, (int)g.Y);
        }

        /// <summary>Get tile at a pixel world position cast to T.</summary>
        public T GetTileAtWorldPos<T>(Vector2 worldPos) where T : class, ITile
            => GetTileAtWorldPos(worldPos) as T;

        /// <summary>Convert pixel world-position to (col, row).</summary>
        public Vector2 WorldPosToGrid(Vector2 worldPos)
        {
            Vector2 origin = Config.WorldOrigin;
            int col = Math.Max(0, Math.Min(
                (int)((worldPos.X - origin.X) / Config.TilePixels), Config.WorldTilesX - 1));
            int row = Math.Max(0, Math.Min(
                (int)((worldPos.Y - origin.Y) / Config.TilePixels), Config.WorldTilesY - 1));
            return new Vector2(col, row);
        }

        /// <summary>Returns the pixel-space centre of a world-space tile.</summary>
        public Vector2 TileCenterPos(int col, int row)
        {
            Vector2 origin = Config.WorldOrigin;
            return origin + new Vector2(
                col * Config.TilePixels + Config.TilePixels * 0.5f,
                row * Config.TilePixels + Config.TilePixels * 0.5f);
        }

        // ── Chunk access ──────────────────────────────────────────────────────

        public Chunk GetChunk(int chunkCol, int chunkRow)
        {
            if (chunkCol < 0 || chunkRow < 0 ||
                chunkCol >= Config.ChunksX || chunkRow >= Config.ChunksY)
                return null;
            return _chunks[chunkCol, chunkRow];
        }

        public Chunk ChunkOf(int tileCol, int tileRow)
            => _chunks[tileCol / Config.ChunkSize, tileRow / Config.ChunkSize];

        // ── Loading ──────────────────────────────────────────────────────────

        /// <summary>
        /// Call once per frame with the player's world position.
        /// Loads chunks within LoadRadius, unloads those outside LoadRadius + 1.
        /// </summary>
        public void Update(Vector2 playerWorldPos)
        {
            Vector2 origin = Config.WorldOrigin;
            int pcCol = (int)((playerWorldPos.X - origin.X) / Config.TilePixels) / Config.ChunkSize;
            int pcRow = (int)((playerWorldPos.Y - origin.Y) / Config.TilePixels) / Config.ChunkSize;

            pcCol = Math.Max(0, Math.Min(pcCol, Config.ChunksX - 1));
            pcRow = Math.Max(0, Math.Min(pcRow, Config.ChunksY - 1));

            int loadR   = Config.LoadRadius;
            int unloadR = loadR + 1;

            for (int cc = 0; cc < Config.ChunksX; cc++)
            {
                for (int cr = 0; cr < Config.ChunksY; cr++)
                {
                    int dist  = Math.Max(Math.Abs(cc - pcCol), Math.Abs(cr - pcRow));
                    Chunk chunk = _chunks[cc, cr];

                    if (dist <= loadR  && !chunk.IsLoaded) chunk.Load();
                    else if (dist > unloadR && chunk.IsLoaded) chunk.Unload();
                }
            }
        }

        // ── World boundary clamping ──────────────────────────────────────────

        /// <summary>
        /// Clamp a world-space pixel position so the entity never leaves the world.
        /// Pass the entity's half-size (e.g. new Vector2(8, 8) for a 16px sprite).
        /// </summary>
        public Vector2 ClampToWorld(Vector2 worldPos, Vector2 entityHalfSize)
        {
            float left   = WorldBoundsPixels.Left   + entityHalfSize.X;
            float right  = WorldBoundsPixels.Right  - entityHalfSize.X;
            float top    = WorldBoundsPixels.Top    + entityHalfSize.Y;
            float bottom = WorldBoundsPixels.Bottom - entityHalfSize.Y;

            return new Vector2(
                Math.Max(left,  Math.Min(right,  worldPos.X)),
                Math.Max(top,   Math.Min(bottom, worldPos.Y)));
        }

        // ── Draw ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Draws all loaded chunks, culled to the visible world rectangle.
        /// visibleWorldRect should be the camera's viewport in world space.
        /// </summary>
        public void Draw(Rectangle visibleWorldRect)
        {
            for (int cc = 0; cc < Config.ChunksX; cc++)
            {
                for (int cr = 0; cr < Config.ChunksY; cr++)
                {
                    Chunk chunk = _chunks[cc, cr];
                    if (!chunk.IsLoaded) continue;

                    int chunkPixelX = (int)(Config.WorldOrigin.X + chunk.TileColStart * Config.TilePixels);
                    int chunkPixelY = (int)(Config.WorldOrigin.Y + chunk.TileRowStart * Config.TilePixels);
                    int chunkPixelW = chunk.Size * Config.TilePixels;

                    Rectangle chunkRect = new Rectangle(chunkPixelX, chunkPixelY, chunkPixelW, chunkPixelW);
                    if (!chunkRect.Intersects(visibleWorldRect)) continue;

                    chunk.Draw(visibleWorldRect);
                }
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool InBounds(int col, int row)
            => col >= 0 && row >= 0 && col < Config.WorldTilesX && row < Config.WorldTilesY;
    }
}