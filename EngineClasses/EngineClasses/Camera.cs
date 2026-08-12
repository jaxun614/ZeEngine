// ============================================================
//  Camera.cs  (Engine library)
//  Added: smoothed follow via exponential lerp so stair
//  teleports and any sudden position changes don't snap
//  the camera instantly.
//
//  SmoothSpeed controls how fast the camera catches up.
//  Higher = snappier. 8-12 feels like Stardew Valley.
//  Set to float.MaxValue to restore instant-snap behaviour.
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
    public class Camera
    {
        public Matrix Transform { get; private set; }

        // ── Smoothing ─────────────────────────────────────────────────────────

        /// <summary>
        /// Current camera centre in world space.
        /// Lerps toward the target each frame.
        /// </summary>
        private Vector2 _currentPos;

        /// <summary>
        /// Whether _currentPos has been initialised yet.
        /// On the first Follow() call we snap immediately so the
        /// player doesn't fly in from (0,0).
        /// </summary>
        private bool _initialised = false;

        /// <summary>
        /// Exponential lerp speed.  Higher = snappier camera.
        ///   4  = floaty / cinematic
        ///   8  = Stardew Valley feel (default)
        ///   16 = tight / action game
        ///   float.MaxValue = instant snap (original behaviour)
        /// </summary>
        public float SmoothSpeed = 8f;

        /// <summary>
        /// If the camera is further than this many pixels from the target,
        /// it snaps immediately instead of lerping.  Prevents the camera
        /// drifting across the map on first load or after a scene change.
        /// </summary>
        public float SnapThreshold = 400f;

        // ── Follow ────────────────────────────────────────────────────────────

        public void Follow(Animated2d TARGET)
        {
            // Target is the sprite centre
            Vector2 targetPos = TARGET.pos + new Vector2(TARGET.dims.X / 2f,
                                                          TARGET.dims.Y / 2f);

            if (!_initialised || Vector2.Distance(_currentPos, targetPos) > SnapThreshold)
            {
                // First call or too far away — snap immediately
                _currentPos  = targetPos;
                _initialised = true;
            }
            else
            {
                // Exponential lerp: fraction of remaining distance per second
                float dt = (float)Globals.gameTime.ElapsedGameTime.TotalSeconds;
                // Clamp dt so a frame-rate spike doesn't overshoot
                dt = Math.Min(dt, 0.05f);
                float t = 1f - (float)Math.Exp(-SmoothSpeed * dt);
                _currentPos = Vector2.Lerp(_currentPos, targetPos, t);
            }

            RebuildTransform();
        }

        /// <summary>
        /// Force the camera to a specific world position instantly,
        /// bypassing smoothing.  Useful for scene transitions or
        /// teleportation where you explicitly want a hard cut.
        /// </summary>
        public void SnapTo(Vector2 worldPos)
        {
            _currentPos  = worldPos;
            _initialised = true;
            RebuildTransform();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void RebuildTransform()
        {
            var position = Matrix.CreateTranslation(
                -_currentPos.X,
                -_currentPos.Y,
                0);

            var offset = Matrix.CreateTranslation(
                Globals.screenWidth  / 2f,
                Globals.screenHeight / 2f,
                0);

            Transform = position * offset;
        }

        // ── Utility ───────────────────────────────────────────────────────────

        /// <summary>
        /// Convert a screen-space position to world space using this camera's
        /// current transform.
        /// </summary>
        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            Matrix inv = Matrix.Invert(Transform);
            return Vector2.Transform(screenPos, inv);
        }

        /// <summary>
        /// Returns the camera's visible rectangle in world space,
        /// expanded by the given margin in pixels on each side.
        /// Use for tile/chunk draw culling.
        /// </summary>
        public Rectangle GetVisibleWorldRect(int marginPixels = 64)
        {
            Vector2 topLeft = ScreenToWorld(Vector2.Zero);
            return new Rectangle(
                (int)topLeft.X - marginPixels,
                (int)topLeft.Y - marginPixels,
                Globals.screenWidth  + marginPixels * 2,
                Globals.screenHeight + marginPixels * 2);
        }
    }
}