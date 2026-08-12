// ============================================================
//  Chunk.cs  (Engine library)
//  Stores ITile references — no dependency on the game project.
//  WorldTile (in the game) implements ITile.
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
    public class Chunk
    {
        public readonly int ChunkCol;
        public readonly int ChunkRow;
        public readonly int TileColStart;
        public readonly int TileRowStart;
        public readonly int Size;

        private readonly ITile[,] _tiles;

        public bool IsLoaded { get; private set; }

        public Chunk(int chunkCol, int chunkRow, int chunkSize)
        {
            ChunkCol     = chunkCol;
            ChunkRow     = chunkRow;
            Size         = chunkSize;
            TileColStart = chunkCol * chunkSize;
            TileRowStart = chunkRow * chunkSize;
            _tiles       = new ITile[chunkSize, chunkSize];
        }

        // ── Tile access ───────────────────────────────────────────────────────

        /// <summary>Get a tile by LOCAL coordinates. Returns null if out of range.</summary>
        public ITile GetLocal(int localCol, int localRow)
        {
            if (localCol < 0 || localRow < 0 || localCol >= Size || localRow >= Size)
                return null;
            return _tiles[localCol, localRow];
        }

        /// <summary>
        /// Get a tile by LOCAL coordinates cast to T.
        /// Use from the game project: chunk.GetLocal&lt;WorldTile&gt;(lc, lr)
        /// </summary>
        public T GetLocal<T>(int localCol, int localRow) where T : class, ITile
            => GetLocal(localCol, localRow) as T;

        /// <summary>Store a tile by LOCAL coordinates.</summary>
        public void SetLocal(int localCol, int localRow, ITile tile)
        {
            _tiles[localCol, localRow] = tile;
        }

        // ── Load / Unload ────────────────────────────────────────────────────

        public void Load()  => IsLoaded = true;

        public void Unload()
        {
            IsLoaded = false;
            // Future extension: serialise tile state to disk here.
        }

        // ── Draw ─────────────────────────────────────────────────────────────

        /// <summary>Draw every tile in the chunk (no culling).</summary>
        public void Draw()
        {
            for (int lc = 0; lc < Size; lc++)
                for (int lr = 0; lr < Size; lr++)
                    _tiles[lc, lr]?.Draw();
        }

        /// <summary>Camera-culled draw — skips tiles outside visibleWorldRect.</summary>
        public void Draw(Rectangle visibleWorldRect)
        {
            for (int lc = 0; lc < Size; lc++)
            {
                for (int lr = 0; lr < Size; lr++)
                {
                    ITile t = _tiles[lc, lr];
                    if (t == null) continue;

                    int px = (int)t.WorldPos.X;
                    int py = (int)t.WorldPos.Y;

                    if (px < visibleWorldRect.Left  || px > visibleWorldRect.Right ||
                        py < visibleWorldRect.Top   || py > visibleWorldRect.Bottom)
                        continue;

                    t.Draw();
                }
            }
        }

        public override string ToString()
            => $"Chunk({ChunkCol},{ChunkRow}) tiles[{TileColStart}..{TileColStart+Size-1}," +
               $"{TileRowStart}..{TileRowStart+Size-1}] loaded={IsLoaded}";
    }
}