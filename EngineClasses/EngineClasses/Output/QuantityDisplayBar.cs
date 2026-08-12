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
    public class QuantityDisplayBar
    {
        public int boarder;
        public Vector2 pos;
        public Basic2d bar, barBKG;
        public Color color;

        public QuantityDisplayBar(Vector2 POS, Vector2 DIMS, int BOARDER, Color COLOR)
        {
            boarder = BOARDER;
            color = COLOR;
            pos = POS;

            barBKG = new Basic2d("2D\\Misc\\BlankWhite", POS, DIMS);
            bar    = new Basic2d("2D\\Misc\\BlankWhite", new Vector2(POS.X + BOARDER, POS.Y + BOARDER), new Vector2(DIMS.X - BOARDER * 2, DIMS.Y - BOARDER * 2));
        }

        // call this to move the bar to a new world position
        public virtual void SetPos(Vector2 POS)
        {
            barBKG.pos = POS;
            bar.pos    = new Vector2(POS.X + boarder, POS.Y + boarder);
        }

        public virtual void Update(float CURRENT, float MAX)
        {
            bar.dims = new Vector2(CURRENT / MAX * (barBKG.dims.X - boarder * 2), bar.dims.Y);
        }

        public virtual void Draw()
        {
            // Globals.normalEffect.Parameters["xSize"].SetValue(1.0f);
            // Globals.normalEffect.Parameters["ySize"].SetValue(1.0f);
            // Globals.normalEffect.Parameters["xDraw"].SetValue(1.0f);
            // Globals.normalEffect.Parameters["yDraw"].SetValue(1.0f);
            // Globals.normalEffect.Parameters["filterColor"].SetValue(Color.Black.ToVector4());
            // Globals.normalEffect.CurrentTechnique.Passes[0].Apply();

            // barBKG.Draw(Vector2.Zero, Color.Black);

            // Globals.normalEffect.Parameters["filterColor"].SetValue(color.ToVector4());
            // Globals.normalEffect.CurrentTechnique.Passes[0].Apply();

            bar.Draw(Vector2.Zero, color);
        }
    }
}