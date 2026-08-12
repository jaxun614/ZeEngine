// ============================================================
//  WorldGenerator.cs  (Engine library)
//  Fixed height curve:
//  After noise + falloff the raw values bunch up near 0.
//  We now apply a redistribution step that remaps the usable
//  land range to fill [0,1] cleanly so every threshold band
//  gets a proportional share of the island area.
// ============================================================

#region Includes
using System;
using Microsoft.Xna.Framework;
#endregion

namespace ZeEngine
{
    // ── Per-tile generation output ───────────────────────────────────────────

    public struct GeneratedTileData
    {
        /// <summary>Normalised elevation [0,1] after falloff + redistribution.</summary>
        public float Height;

        public TileCategory Category;
        public BiomeType    Biome;

        /// <summary>Moisture [0,1] — drives biome selection and object scatter.</summary>
        public float Moisture;

        /// <summary>Temperature [0,1] — drives biome selection.</summary>
        public float Temperature;
    }

    // ── Full world output ─────────────────────────────────────────────────────

    public class GeneratedWorld
    {
        public readonly WorldConfig          Config;
        public readonly GeneratedTileData[,] Tiles;

        public GeneratedWorld(WorldConfig config)
        {
            Config = config;
            Tiles  = new GeneratedTileData[config.WorldTilesX, config.WorldTilesY];
        }
    }

    // ── Generator ────────────────────────────────────────────────────────────

    public class WorldGenerator
    {
        private readonly WorldConfig _cfg;
        private readonly NoiseHelper _elevNoise;
        private readonly NoiseHelper _elevDetail;
        private readonly NoiseHelper _moistureNoise;
        private readonly NoiseHelper _tempNoise;

        public WorldGenerator(WorldConfig config)
        {
            _cfg = config;
            int seed = config.Seed == 0 ? Environment.TickCount : config.Seed;
            _elevNoise     = new NoiseHelper(seed);
            _elevDetail    = new NoiseHelper(seed + 1337);
            _moistureNoise = new NoiseHelper(seed + 2674);
            _tempNoise     = new NoiseHelper(seed + 9001);
        }

        public GeneratedWorld Generate()
        {
            var world = new GeneratedWorld(_cfg);
            int W     = _cfg.WorldTilesX;
            int H     = _cfg.WorldTilesY;

            // ── Pass 1: raw noise + falloff ───────────────────────────────────
            // Compute raw elevation for every tile.
            double[,] raw = new double[W, H];
            double    maxRaw = 0;

            for (int col = 0; col < W; col++)
            {
                for (int row = 0; row < H; row++)
                {
                    double nx = col / (double)W;
                    double ny = row / (double)H;

                    // Primary fBm
                    double elev = _elevNoise.FBM(
                        nx * _cfg.ElevationScale,
                        ny * _cfg.ElevationScale,
                        octaves: 5, persistence: 0.5, lacunarity: 2.0);

                    // Detail octave
                    double detail = _elevDetail.FBM(
                        nx * _cfg.ElevationDetailScale,
                        ny * _cfg.ElevationDetailScale,
                        octaves: 2, persistence: 0.5, lacunarity: 2.0);

                    elev = elev * (1.0 - _cfg.ElevationDetailStrength)
                         + detail * _cfg.ElevationDetailStrength;

                    // Radial falloff forces ocean at edges
                    double falloff = ComputeFalloff(nx, ny);
                    elev = Math.Max(0.0, elev - falloff);

                    raw[col, row] = elev;
                    if (elev > maxRaw) maxRaw = elev;
                }
            }

            // ── Pass 2: redistribute to fill [0,1] and apply height curve ─────
            // Normalising by maxRaw ensures the tallest point = 1.0
            // Then we apply a power curve to push land tiles away from the
            // water threshold and give each band more distinct area.
            double invMax = maxRaw > 0 ? 1.0 / maxRaw : 1.0;

            for (int col = 0; col < W; col++)
            {
                for (int row = 0; row < H; row++)
                {
                    double nx = col / (double)W;
                    double ny = row / (double)H;

                    // Normalise
                    double elev = raw[col, row] * invMax;

                    // Power curve: < 1 = push values up (more land area at each band)
                    //              > 1 = push values down (more ocean, fewer highlands)
                    // 0.7 gives a nice island profile with visible highland zones.
                    elev = Math.Pow(elev, 0.7);

                    // Moisture + temperature (independent passes)
                    double moisture = _moistureNoise.FBM(
                        nx * _cfg.MoistureScale,
                        ny * _cfg.MoistureScale,
                        octaves: 3, persistence: 0.5, lacunarity: 2.0);

                    double temperature = _tempNoise.FBM(
                        nx * _cfg.TemperatureScale,
                        ny * _cfg.TemperatureScale,
                        octaves: 3, persistence: 0.5, lacunarity: 2.0);

                    // Bias temperature warmer near the equator (world centre Y)
                    double latFactor = 1.0 - Math.Abs(ny - 0.5) * 2.0;
                    temperature = temperature * 0.7 + latFactor * 0.3;

                    TileCategory category = Categorise(elev);
                    BiomeType    biome    = (category >= TileCategory.Sand)
                                           ? SelectBiome(category, moisture, temperature)
                                           : BiomeType.None;

                    world.Tiles[col, row] = new GeneratedTileData
                    {
                        Height      = (float)elev,
                        Category    = category,
                        Biome       = biome,
                        Moisture    = (float)moisture,
                        Temperature = (float)temperature,
                    };
                }
            }

            return world;
        }

        // ── Falloff ──────────────────────────────────────────────────────────

        private double ComputeFalloff(double nx, double ny)
        {
            // Squircle distance: blend of circular and square falloff.
            // Produces more natural, irregular coastlines than a pure circle.
            double x = nx * 2.0 - 1.0;
            double y = ny * 2.0 - 1.0;
            double d = Math.Max(Math.Abs(x), Math.Abs(y));

            double a = _cfg.FalloffSteepness;
            double k = _cfg.FalloffRamp;
            return Math.Pow(d, a) / (Math.Pow(d, a) + Math.Pow(k - k * d, a));
        }

        // ── Category ─────────────────────────────────────────────────────────

        private TileCategory Categorise(double h)
        {
            if (h < _cfg.ThresholdDeepWater)    return TileCategory.DeepWater;
            if (h < _cfg.ThresholdShallowWater) return TileCategory.ShallowWater;
            if (h < _cfg.ThresholdSand)         return TileCategory.Sand;
            if (h < _cfg.ThresholdLowland)      return TileCategory.Lowland;
            if (h < _cfg.ThresholdHighland)     return TileCategory.Highland;
            return TileCategory.Mountain;
        }

        // ── Biome ────────────────────────────────────────────────────────────

        private BiomeType SelectBiome(TileCategory category, double moisture, double temperature)
        {
            if (category == TileCategory.Sand) return BiomeType.None;

            if (category == TileCategory.Mountain)
                return temperature < 0.5 ? BiomeType.Snowy : BiomeType.Volcanic;

            bool cold      = temperature < 0.35;
            bool hot       = temperature >= 0.65;
            bool dry       = moisture    < 0.35;
            bool wet       = moisture    >= 0.70;

            if (cold)
            {
                return dry ? BiomeType.Tundra : BiomeType.Taiga;
            }
            if (hot)
            {
                if (dry) return BiomeType.Desert;
                if (wet) return BiomeType.Jungle;
                return BiomeType.Savanna;
            }
            // Temperate
            if (dry) return BiomeType.Grassland;
            if (wet) return moisture > 0.85 ? BiomeType.Swamp : BiomeType.Forest;
            return category == TileCategory.Highland ? BiomeType.Forest : BiomeType.Grassland;
        }
    }
}