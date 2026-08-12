// ============================================================
//  HeightLevel.cs  (Engine library)
//  Maps TileCategory to a discrete integer height level 0–3.
//
//  Level 0 — Water          (impassable)
//  Level 1 — Beach/Lowland  (ground floor)
//  Level 2 — Highland       (elevated)
//  Level 3 — Mountain       (high elevation)
//
//  Wall faces are drawn on the south edge of any tile whose
//  level is greater than the tile immediately to its south.
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
    public static class HeightLevel
    {
        public const int Water    = 0;
        public const int Ground   = 1;
        public const int Highland = 2;
        public const int Mountain = 3;

        /// <summary>
        /// Convert a TileCategory to its discrete height level (0–3).
        /// </summary>
        public static int FromCategory(TileCategory category)
        {
            switch (category)
            {
                case TileCategory.DeepWater:
                case TileCategory.ShallowWater:
                    return Water;

                case TileCategory.Sand:
                case TileCategory.Lowland:
                    return Ground;

                case TileCategory.Highland:
                    return Highland;

                case TileCategory.Mountain:
                case TileCategory.Peak:
                    return Mountain;

                default:
                    return Ground;
            }
        }

        /// <summary>
        /// Returns true if moving from fromLevel to toLevel is a valid
        /// stair transition (adjacent levels only).
        /// </summary>
        public static bool IsValidTransition(int fromLevel, int toLevel)
            => System.Math.Abs(fromLevel - toLevel) == 1;
    }
}