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
    public class Animated2d : Basic2d
    {
        public Vector2 frames;
        public List<FrameAnimation> frameAnimationList = new List<FrameAnimation>();
        // public Color color;
        public bool frameAnimations;
        public int currentAnimation = 0;

        public Animated2d(string PATH, Vector2 POS, Vector2 DIMS, Vector2 FRAMES, Color COLOR) : base(PATH, POS, DIMS)
        {
            Frames = new Vector2(FRAMES.X, FRAMES.Y);
            color = COLOR;
        }

        #region Properties
        public Vector2 Frames
        {
            set
            {
                frames = value;
                if (tex != null)
                    frameSize = new Vector2(tex.Bounds.Width / frames.X, tex.Bounds.Height / frames.Y);
            }
            get { return frames; }
        }
        #endregion

        public override void Update()
        {
            if (frameAnimations && frameAnimationList != null && frameAnimationList.Count > currentAnimation)
                frameAnimationList[currentAnimation].Update();

            base.Update();
        }

        public virtual int GetAnimationFromName(string ANIMATIONNAME)
        {
            for (int i = 0; i < frameAnimationList.Count; i++)
            {
                if (frameAnimationList[i].name == ANIMATIONNAME)
                    return i;
            }
            return -1;
        }

        public virtual void SetAnimationByName(string NAME)
        {
            int tempAnimation = GetAnimationFromName(NAME);

            if (tempAnimation != -1)
            {
                if (tempAnimation != currentAnimation)
                    frameAnimationList[tempAnimation].Reset();

                currentAnimation = tempAnimation;
            }
        }

        public override void Draw()
        {
            if (frameAnimations && frameAnimationList != null && frameAnimationList.Count > currentAnimation)
                frameAnimationList[currentAnimation].Draw(tex, dims, frameSize, Vector2.Zero, pos, rot, color, new SpriteEffects());
            else
                base.Draw();
        }
    }
}