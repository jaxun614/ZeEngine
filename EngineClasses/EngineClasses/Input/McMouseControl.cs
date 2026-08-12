#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace ZeEngine
{
    public class McMouseControl
    {
        public bool dragging, rightDrag; // left click is dragging, right click is rightDrag

        public Vector2 newMousePos, oldMousePos, firstMousePos, newMouseAdjustedPos, systemCursorPos, screenLoc; 

        public MouseState newMouse, oldMouse, firstMouse;

        public McMouseControl()
        {
            dragging = false;

            newMouse = Mouse.GetState();
            oldMouse = newMouse;
            firstMouse = newMouse;

            newMousePos = new Vector2(newMouse.Position.X, newMouse.Position.Y);
            oldMousePos = new Vector2(newMouse.Position.X, newMouse.Position.Y);
            firstMousePos = new Vector2(newMouse.Position.X, newMouse.Position.Y);

            GetMouseAndAdjust();

            //screenLoc = new Vector2((int)(systemCursorPos.X/Globals.screenWidth), (int)(systemCursorPos.Y/Globals.screenHeight));

        }

        #region Properties

        public MouseState First
        {
            get { return firstMouse; }
        }

        public MouseState New
        {
            get { return newMouse; }
        }

        public MouseState Old
        {
            get { return oldMouse; }
        }

        #endregion

        // update the mouse, including updating the mouse position and checking for clicks and drags
        public void Update()
        {
            GetMouseAndAdjust();


            if(newMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && oldMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                firstMouse = newMouse;
                firstMousePos = newMousePos = GetScreenPos(firstMouse);
            }

            
        }

        // update the old mouse state and position, should be called at the end of each update after all input has been processed
        public void UpdateOld()
        {
            oldMouse = newMouse;
            oldMousePos = GetScreenPos(oldMouse);
        }

        // get the distance from the first mouse click, used for checking if the mouse has been dragged a distance
        public virtual float GetDistanceFromClick()
        {
            return Globals.GetDistance(newMousePos, firstMousePos);
        }

        // get the current mouse state and position, and adjust the position for screen scaling if necessarty
        public virtual void GetMouseAndAdjust()
        {
            newMouse = Mouse.GetState();
            newMousePos = GetScreenPos(newMouse);

        }



        // get the change in the mouse wheel value, used for checking if the mouse wheel has been scroll
        public int GetMouseWheelChange()
        {
            return newMouse.ScrollWheelValue - oldMouse.ScrollWheelValue;
        }

        // get the mouse position in screen coordinates, used for checking where the mouse is on the screen and for adjusting the mouse position for screen scaling
        public Vector2 GetScreenPos(MouseState MOUSE)
        {
            Vector2 tempVec = new Vector2(MOUSE.Position.X, MOUSE.Position.Y);


            return tempVec;
        }

        // check if the left mouse button was just clicked this update, used for detecting single clicks
        public virtual bool LeftClick()
        {
            if( newMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && oldMouse.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed && newMouse.Position.X >= 0 && newMouse.Position.X <= Globals.screenWidth && newMouse.Position.Y >= 0 && newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        // check if the left mouse button is being held down, used for detecting drags and for checking if the mouse is being held down
        public virtual bool LeftClickHold()
        {
            bool holding = false;

            if( newMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && oldMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && newMouse.Position.X >= 0 && newMouse.Position.X <= Globals.screenWidth && newMouse.Position.Y >= 0 && newMouse.Position.Y <= Globals.screenHeight)
            {
                holding = true;

                if(Math.Abs(newMouse.Position.X - firstMouse.Position.X) > 8 || Math.Abs(newMouse.Position.Y - firstMouse.Position.Y) > 8)
                {
                    dragging = true;
                }
            }

            

            return holding;
        }

        // check if the left mouse button was just released this update, used for detecting single clicks
        public virtual bool LeftClickRelease()
        {
            if(newMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && oldMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                dragging = false;
                return true;
            }

            return false;
        }

        // check if the right mouse button was just clicked this update, used for detecting single clicks
        public virtual bool RightClick()
        {
            if(newMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && oldMouse.RightButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed && newMouse.Position.X >= 0 && newMouse.Position.X <= Globals.screenWidth && newMouse.Position.Y >= 0 && newMouse.Position.Y <= Globals.screenHeight)
            {
                return true;
            }

            return false;
        }

        // check if the right mouse button is being held down, used for detecting drags and for checking if the mouse is being held down
        public virtual bool RightClickHold()
        {
            bool holding = false;

            if( newMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && oldMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && newMouse.Position.X >= 0 && newMouse.Position.X <= Globals.screenWidth && newMouse.Position.Y >= 0 && newMouse.Position.Y <= Globals.screenHeight)
            {
                holding = true;

                if(Math.Abs(newMouse.Position.X - firstMouse.Position.X) > 8 || Math.Abs(newMouse.Position.Y - firstMouse.Position.Y) > 8)
                {
                    rightDrag = true;
                }
            }



            return holding;
        }

        // check if the right mouse button was just released this update, used for detecting single clicks
        public virtual bool RightClickRelease()
        {
            if( newMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released && oldMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                dragging = false;
                return true;
            }

            return false;
        }

        // check if the mouse is currently within a rectangle defined by a position and dimensions, used for checking if the mouse is hovering over a button or other UI element
        public void SetFirst()
        {

        }
    }
}
