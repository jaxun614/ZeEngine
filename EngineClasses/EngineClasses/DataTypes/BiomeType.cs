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
// ============================================================
//  BiomeType.cs  (Engine library)
//  Enums shared by WorldGenerator, WorldTile, and the game.
// ============================================================

namespace ZeEngine
{
    // ── Terrain categories ───────────────────────────────────────────────────
    // Used by tools / gameplay logic to query what kind of tile this is
    // without caring about the exact biome variant.
    public enum TileCategory
    {
        DeepWater,
        ShallowWater,
        Sand,
        Lowland,    // flat, walkable, biome-driven (grass / savanna / tundra floor …)
        Highland,   // elevated, walkable, biome-driven (forest floor / rocky highland …)
        Mountain,   // steep; typically impassable or slow
        Peak,       // snow-capped summit
    }

    // ── Biomes ───────────────────────────────────────────────────────────────
    // Assigned only to land tiles (Sand and above).
    // Water tiles use BiomeType.None.
    public enum BiomeType
    {
        None,

        // Temperate
        Grassland,      // moderate moisture, temperate
        Forest,         // high moisture, temperate

        // Cold
        Tundra,         // low moisture, cold
        Taiga,          // high moisture, cold  — TODO: add Taiga tile art

        // Hot / dry
        Desert,         // low moisture, hot
        Savanna,        // moderate moisture, hot  — TODO: add Savanna tile art

        // Wet / tropical
        Swamp,          // very high moisture, temperate/warm — TODO: add Swamp tile art
        Jungle,         // very high moisture, hot            — TODO: add Jungle tile art

        // Special
        Volcanic,       // near mountains, hot, low moisture  — TODO: add Volcanic tile art
        Snowy,          // mountains + cold — replaces Peak in cold biomes
    }
}