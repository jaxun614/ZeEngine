// ============================================================
//  WorldConfig.cs  (Engine library)
//  Updated defaults for cleaner height band separation.
// ============================================================

using Microsoft.Xna.Framework;

namespace ZeEngine
{
    public class WorldConfig
    {
        // ── World size ───────────────────────────────────────────────────────
        public int WorldTilesX = 128;
        public int WorldTilesY = 128;

        // ── Chunk layout ────────────────────────────────────────────────────
        public int ChunkSize  = 16;
        public int LoadRadius = 3;

        // ── Physical tile size ───────────────────────────────────────────────
        public int TilePixels = 32;

        // ── Generation seed ──────────────────────────────────────────────────
        public int Seed = 0;

        // ── Noise scales ────────────────────────────────────────────────────
        public double ElevationScale          = 3.5;
        public double ElevationDetailScale    = 10.0;
        public double ElevationDetailStrength = 0.20;
        public double MoistureScale           = 5.0;
        public double TemperatureScale        = 4.5;

        // ── Island falloff ───────────────────────────────────────────────────
        public double FalloffSteepness = 2.0;
        public double FalloffRamp      = 3.0;

        // ── Height thresholds ────────────────────────────────────────────────
        //
        // Values in [0,1] after noise + falloff.
        // Each band spans from the previous threshold to the next.
        //
        // Band widths with these defaults:
        //   Deep water    : 0.00 – 0.32  (wide ocean border)
        //   Shallow water : 0.32 – 0.40  (surf zone)
        //   Sand/beach    : 0.40 – 0.44  (narrow coastal strip)
        //   Lowland       : 0.44 – 0.68  (most of the island interior)
        //   Highland      : 0.68 – 0.82  (elevated mid-zone)
        //   Mountain      : 0.82 – 1.00  (peaks only)

        public double ThresholdDeepWater    = 0.32;
        public double ThresholdShallowWater = 0.40;
        public double ThresholdSand         = 0.44;
        public double ThresholdLowland      = 0.68;
        public double ThresholdHighland     = 0.82;

        // ── Derived helpers ──────────────────────────────────────────────────
        public int ChunksX => WorldTilesX / ChunkSize;
        public int ChunksY => WorldTilesY / ChunkSize;

        public Vector2 WorldOrigin =>
            new Vector2(
                -(WorldTilesX * TilePixels) / 2f,
                -(WorldTilesY * TilePixels) / 2f);
    }
}