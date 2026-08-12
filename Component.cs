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
    // base class for all 2d objects, including animated and static objects
    // all 2d objects are components, which can be added to a scene or other component
    public abstract class Component 
    {
        // ── Configuration ─────────────────────────────────────────────────────

        // draw order of the component, lower numbers are drawn first
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {}

        // update the component, called every frame
        public virtual void Update(GameTime gameTime) {}
    }
}