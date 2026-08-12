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
    // A basic 2d is a 2d object that can be drawn to the sceene.
    // 

    public class Basic2d : Component
    {
        public float rot; // rotation for the object, in rad

        public Vector2 pos, dims, frameSize; // position, dimensions, and frame size of the object

        public Texture2D tex; // 
        public Color color = Color.White;


        public Basic2d(string PATH, Vector2 POS, Vector2 DIMS)
        {
            pos = new Vector2(POS.X, POS.Y);
            dims = new Vector2(DIMS.X, DIMS.Y);
            rot = 0.0f;

            tex = Globals.content.Load<Texture2D>(PATH);
        }

        // Component contract stubs
        public override void Update(GameTime gameTime) { }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }

        // Engine-facing methods
        public virtual void Update() { }

        public virtual bool Hover()
        {
            return HoverImg();
        }

        public virtual bool HoverImg()
        {
            Vector2 mousePos = new Vector2(Globals.mouse.newMousePos.X, Globals.mouse.newMousePos.Y);

            if (mousePos.X >= pos.X - dims.X / 2 && mousePos.X <= pos.X + dims.X / 2 &&
                mousePos.Y >= pos.Y - dims.Y / 2 && mousePos.Y <= pos.Y + dims.Y / 2)
            {
                return true;
            }

            return false;
        }

        public virtual bool HoverFirst(Vector2 OFFSET)
        {
            Vector2 mousePos = new Vector2(Globals.mouse.firstMousePos.X, Globals.mouse.firstMousePos.Y);

            if (mousePos.X >= (pos.X + OFFSET.X)-dims.X/2 && mousePos.X <= (pos.X + OFFSET.X)+dims.X/2 && mousePos.Y >= (pos.Y + OFFSET.Y)-dims.Y/2 && mousePos.Y <= (pos.Y + OFFSET.Y)+dims.Y/2)
            {
                return true;
            }
            return false;
        }

        public virtual void Draw()
        {
            if (tex != null)
            {
                Globals.spriteBatch.Draw(tex,
                                        new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X, (int)dims.Y),
                                        null, color, rot,
                                        new Vector2(tex.Bounds.Width / 2, tex.Bounds.Height / 2),
                                        new SpriteEffects(), 0);
            }
        }

        // draw with explicit origin and color
        public virtual void Draw(Vector2 ORIGIN, Color COLOR)
        {
            if (tex != null)
            {
                Globals.spriteBatch.Draw(tex,
                    new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X, (int)dims.Y),
                    null, COLOR, rot,
                    new Vector2(ORIGIN.X, ORIGIN.Y),
                    new SpriteEffects(), 0);
            }
        }
    }
}