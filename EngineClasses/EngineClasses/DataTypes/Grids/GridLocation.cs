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
    public class GridLocation
    {

        public bool filled, impassable, unPathable, hasBeenUsed, isViewable; // whether or not this location is filled, impassable, unpathable,  has been used in path finding, and is viewable to the player
        public float fScore, cost, currentDist; // fScore is used for path finding, cost is the cost of moving onto this location, currentDist is the distance from the start location used for path finding
        public Vector2 parent, pos; // parent is the location that comes before this one in a path, used for path finding, pos is the position of this location in the grid

        public GridLocation(float COST, bool FILLED)
        {
            cost = COST;
            filled = FILLED;

            hasBeenUsed = false;
            isViewable = false;
            unPathable = false;
            impassable = false;
        }

        public GridLocation(Vector2 POS, float COST, bool FILLED, float FSCORE)
        {
            cost = COST;
            filled = FILLED;
            impassable = FILLED;
            unPathable = false;
            hasBeenUsed = false;
            isViewable = false;

            pos = POS;

            fScore = FSCORE;
        }

        // sets parent and path finding scores for this location
        public void SetNode(Vector2 PARENT, float FSCORE, float CURRENTDIST)
        {
            parent = PARENT;
            fScore = FSCORE;
            currentDist = CURRENTDIST;
        }
        // sets this location to filled and impassable or not impassable based on param
        public virtual void SetToFilled(bool IMPASSIBLE)
        {
            filled = true;
            impassable = IMPASSIBLE;
        }
    }
}
