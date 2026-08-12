// ============================================================
//  WallFace.cs  (Engine library)
//  Stardew Valley style cliff face.
//
//  Three layers stacked vertically:
//  1. Top shadow strip  — dark semi-transparent bar (Globals.pixel)
//     Creates the overhang shadow that makes cliff edges pop.
//  2. Main wall body    — the supplied wall texture stretched to fill
//  3. Bottom fade strip — soft gradient into the lower tile (Globals.pixel)
//
//  Total height = tileHeight * heightDiff
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
    public class WallFace
    {
        // ── Visual constants (tweak for look) ────────────────────────────────
        private const int   ShadowStripH = 3;      // pixels — dark overhang at top
        private const int   FadeStripH   = 3;      // pixels — soft fade at bottom
        private const float ShadowAlpha  = 0.65f;  // darkness of the overhang shadow
        private const float FadeAlpha    = 0.35f;  // darkness of the bottom fade

        private static readonly Color ShadowColor = new Color(0, 0, 0, (int)(255 * ShadowAlpha));
        private static readonly Color FadeColor   = new Color(0, 0, 0, (int)(255 * FadeAlpha));

        // ── Data ──────────────────────────────────────────────────────────────
        public int heightDiff;

        private Rectangle _shadowRect;
        private Rectangle _bodyRect;
        private Rectangle _fadeRect;
        private Basic2d   _bodySprite;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Create a Stardew-style wall face below an elevated tile.
        /// </summary>
        /// <param name="elevatedTilePos">worldPos (centre) of the elevated tile.</param>
        /// <param name="tileWidth">Tile pixel width (Config.TilePixels).</param>
        /// <param name="tileHeight">Tile pixel height (Config.TilePixels).</param>
        /// <param name="heightDiff">Height levels this cliff spans (usually 1).</param>
        /// <param name="texturePath">Content path for the wall body texture.</param>
        public WallFace(Vector2 elevatedTilePos,
                        int     tileWidth,
                        int     tileHeight,
                        int     heightDiff,
                        string  texturePath)
        {
            this.heightDiff = heightDiff;

            int totalH = tileHeight * heightDiff;
            int bodyH  = Math.Max(1, totalH - ShadowStripH - FadeStripH);

            // Wall top = south edge of the elevated tile
            int wallTopY  = (int)(elevatedTilePos.Y + tileHeight / 2f);
            int wallLeftX = (int)(elevatedTilePos.X - tileWidth  / 2f);

            _shadowRect = new Rectangle(wallLeftX, wallTopY,                              tileWidth, ShadowStripH);
            _bodyRect   = new Rectangle(wallLeftX, wallTopY + ShadowStripH,               tileWidth, bodyH);
            _fadeRect   = new Rectangle(wallLeftX, wallTopY + ShadowStripH + bodyH,       tileWidth, FadeStripH);

            // Basic2d expects a centre position
            Vector2 bodyCentre = new Vector2(
                _bodyRect.X + _bodyRect.Width  / 2f,
                _bodyRect.Y + _bodyRect.Height / 2f);

            _bodySprite = new Basic2d(texturePath, bodyCentre,
                                      new Vector2(_bodyRect.Width, _bodyRect.Height));
        }

        // ── Draw ──────────────────────────────────────────────────────────────

        public void Draw()
        {
            // 1. Wall body texture
            _bodySprite?.Draw();

            if (Globals.pixel == null) return;

            // 2. Dark overhang shadow at the very top
            Globals.spriteBatch.Draw(Globals.pixel, _shadowRect, ShadowColor);

            // 3. Soft fade at the bottom
            Globals.spriteBatch.Draw(Globals.pixel, _fadeRect, FadeColor);
        }
    }
}