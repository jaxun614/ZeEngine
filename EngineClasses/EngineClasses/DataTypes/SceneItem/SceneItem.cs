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
    public class SceneItem : Animated2d
    {
        public int drawLocId, drawLayer;
        public Vector2 sortOffset;
        public PassObject DrawManagerDel;

        public SceneItem(string PATH, Vector2 POS, Vector2 DIMS, Vector2 FRAMES, Color COLOR)
            : base(PATH, POS, DIMS, FRAMES, COLOR)
        {
            drawLocId  = 0;
            drawLayer  = 5;
            sortOffset = Vector2.Zero;
        }

        public virtual Vector2 SortDrawPos
        {
            get => pos + sortOffset;
            set => pos = value - sortOffset;
        }

        public virtual void Update(Vector2 OFFSET, LevelDrawManager LEVELDRAWMANAGER)
        {
            if (LEVELDRAWMANAGER != null)
                UpdateDraw(OFFSET, LEVELDRAWMANAGER);
        }

        public virtual void UpdateDraw(Vector2 OFFSET, LevelDrawManager LEVELDRAWMANAGER)
        {
            if (drawLocId == 0 && LEVELDRAWMANAGER != null)
                LEVELDRAWMANAGER.AddOrUpdateDraws(this, true);

            DrawManagerDel?.Invoke(new DrawSlotUpdatePackage(OFFSET, true));
        }

        // offset overload used by DrawSlot
        public virtual void Draw(Vector2 OFFSET)
        {
            pos += OFFSET;
            base.Draw();
            pos -= OFFSET;
        }
    }
}

