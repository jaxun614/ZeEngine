// ============================================================
//  ITile.cs  (Engine library)
//  Minimal interface that the engine's Chunk / ChunkManager
//  use so they never need to reference the game's WorldTile.
//
//  WorldTile implements this in the Main project.
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
    public interface ITile
    {
        /// <summary>Pixel-space centre of this tile in the world.</summary>
        Vector2 WorldPos { get; }

        /// <summary>Draw the tile's terrain layer.</summary>
        void Draw();
    }
}