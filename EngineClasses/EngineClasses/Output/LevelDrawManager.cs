#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion

namespace ZeEngine
{
    public class LevelDrawManager
    {
        public int nextId;
        public List<DrawSlot> drawables = new List<DrawSlot>();

        public LevelDrawManager()
        {
            nextId = 1;
        }

        public virtual void Update()
        {
            Sort();
        }

        public virtual void Sort()
        {
            // Stable sort: primary key = drawLayer, secondary key = SortDrawPos.Y.
            // OrderBy is stable in .NET, so items with identical keys keep their
            // insertion order — no flicker from ties.
            // This runs synchronously on the main thread so Draw() always sees a
            // consistent list — the previous async Task.Run caused a race condition
            // where Draw() read the list while Sort() was mutating it mid-pass.
            var sorted = drawables
                .OrderBy(d => d.item.drawLayer)
                .ThenBy(d => d.item.SortDrawPos.Y)
                .ToList();

            drawables.Clear();
            for (int i = 0; i < sorted.Count; i++)
                drawables.Add(sorted[i]);
        }

        public virtual void AddOrUpdateDraws(SceneItem ITEM, bool DRAWABLE)
        {
            if (ITEM.drawLocId == 0 || !SearchForItemById(ITEM.drawLocId))
            {
                bool add = true;
                for (int i = 0; i < drawables.Count; i++)
                {
                    if (drawables[i].item == ITEM)
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                    InsertItemByPos(ITEM, DRAWABLE);
            }
        }

        public virtual int GetNextID()
        {
            return nextId++;
        }

        public virtual void InsertItemByPos(SceneItem ITEM, bool DRAWABLE)
        {
            ITEM.drawLocId = GetNextID();

            bool added = false;
            for (int i = 0; i < drawables.Count; i++)
            {
                if (drawables[i].item.SortDrawPos.Y >= ITEM.SortDrawPos.Y ||
                   (drawables[i].item.drawLayer > ITEM.drawLayer &&
                    drawables[i].item.SortDrawPos.Y == ITEM.SortDrawPos.Y))
                {
                    drawables.Insert(Math.Max(0, i - 1), new DrawSlot(ITEM, DRAWABLE));
                    added = true;
                    break;
                }
            }

            if (!added)
                drawables.Add(new DrawSlot(ITEM, DRAWABLE));
        }

        public virtual void Remove(SceneItem ITEM)
        {
            ITEM.drawLocId = 0;
            for (int i = 0; i < drawables.Count; i++)
            {
                if (drawables[i].item == ITEM)
                {
                    drawables.RemoveAt(i);
                    break;
                }
            }
        }

        public virtual bool SearchForItemById(int ID)
        {
            for (int i = 0; i < drawables.Count; i++)
                if (drawables[i].id == ID)
                    return true;
            return false;
        }

        public virtual void Draw()
        {
            for (int i = 0; i < drawables.Count; i++)
                if (drawables[i].drawing)
                    drawables[i].Draw();
        }
    }
}