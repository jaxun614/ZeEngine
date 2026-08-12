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
//  NoiseHelper.cs  (Engine library)
//  Self-contained Perlin noise — no external dependencies.
//  Usage:
//      var n = new NoiseHelper(seed);
//      double v = n.Sample(x / scale, y / scale);   // returns [-1, 1]
//      double v01 = (v + 1) * 0.5;                  // remap to [0, 1]
// ============================================================


namespace ZeEngine
{
    public class NoiseHelper
    {
        private readonly int[] _perm;   // permutation table (512 entries)

        public NoiseHelper(int seed)
        {
            _perm = BuildPermutation(seed);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Classic 2-D Perlin noise.
        /// Returns a value in approximately [-1, 1].
        /// </summary>
        public double Sample(double x, double y)
        {
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;

            double xf = x - Math.Floor(x);
            double yf = y - Math.Floor(y);

            double u = Fade(xf);
            double v = Fade(yf);

            int aa = _perm[_perm[xi    ] + yi    ];
            int ab = _perm[_perm[xi    ] + yi + 1];
            int ba = _perm[_perm[xi + 1] + yi    ];
            int bb = _perm[_perm[xi + 1] + yi + 1];

            double x1 = Lerp(Grad(aa, xf,     yf    ),
                             Grad(ba, xf - 1, yf    ), u);
            double x2 = Lerp(Grad(ab, xf,     yf - 1),
                             Grad(bb, xf - 1, yf - 1), u);

            return Lerp(x1, x2, v);
        }

        /// <summary>
        /// Fractal Brownian Motion (fBm) — layered Perlin noise.
        /// Octaves add detail; persistence controls amplitude decay;
        /// lacunarity controls frequency growth between octaves.
        /// Returns approximately [0, 1].
        /// </summary>
        public double FBM(double x, double y,
                          int    octaves     = 4,
                          double persistence = 0.5,
                          double lacunarity  = 2.0)
        {
            double value     = 0;
            double amplitude = 0.5;
            double frequency = 1.0;
            double maxValue  = 0;

            for (int i = 0; i < octaves; i++)
            {
                value    += Sample(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            // Normalise to [0, 1]
            return (value / maxValue + 1.0) * 0.5;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static double Fade(double t)
            => t * t * t * (t * (t * 6 - 15) + 10);

        private static double Lerp(double a, double b, double t)
            => a + t * (b - a);

        private static double Grad(int hash, double x, double y)
        {
            // 4 gradient directions in 2-D
            switch (hash & 3)
            {
                case 0:  return  x + y;
                case 1:  return -x + y;
                case 2:  return  x - y;
                default: return -x - y;
            }
        }

        private static int[] BuildPermutation(int seed)
        {
            // Start with the classic Ken Perlin table then shuffle with seed.
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;

            var rng = new Random(seed == 0 ? Environment.TickCount : seed);
            for (int i = 255; i > 0; i--)
            {
                int j   = rng.Next(i + 1);
                int tmp = p[i]; p[i] = p[j]; p[j] = tmp;
            }

            // Double the table to avoid overflow in the lookup
            int[] perm = new int[512];
            for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
            return perm;
        }
    }
}