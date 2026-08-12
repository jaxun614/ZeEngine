// ============================================================
//  StairData.cs  (Engine library)
//  Stored on WorldTile when that tile is a stair.
//  Describes which height levels the stair connects and which
//  direction the player exits on (so they land on the correct
//  adjacent tile after climbing/descending).
//
//  Stairs always connect two adjacent height levels (diff = 1).
//  The "exit direction" is the compass direction the player
//  travels through the stairs:
//      Ascending  → player walks North (up the screen)
//      Descending → player walks South (down the screen)
//  But since the world is 2-D, "exit" is a tile offset so the
//  player is teleported to the tile on the other side.
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
    public class StairData
    {
        /// <summary>Height level at the bottom of the stairs.</summary>
        public int LowerLevel;

        /// <summary>Height level at the top of the stairs.</summary>
        public int UpperLevel;

        /// <summary>
        /// World-space position the player is moved to after ascending
        /// (landing on the upper tile just north of the stair).
        /// Set by WorldGrid.BuildWalls() at generation time.
        /// Editable in creative mode.
        /// </summary>
        public Vector2 AscendExitPos;

        /// <summary>
        /// World-space position the player is moved to after descending
        /// (landing on the lower tile just south of the stair).
        /// </summary>
        public Vector2 DescendExitPos;

        public StairData(int lowerLevel, int upperLevel,
                         Vector2 ascendExitPos, Vector2 descendExitPos)
        {
            LowerLevel     = lowerLevel;
            UpperLevel     = upperLevel;
            AscendExitPos  = ascendExitPos;
            DescendExitPos = descendExitPos;
        }
    }
}