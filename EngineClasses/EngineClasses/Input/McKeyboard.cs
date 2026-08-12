#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace ZeEngine
{
    public class McKeyboard
    {

        public KeyboardState newKeyboard, oldKeyboard; // newKeyboard is the current state of the keyboard, oldKeyboard is the state of the keyboard in the previous update, used for detecting single key presses

        public List<McKey> pressedKeys = new List<McKey>(), previousPressedKeys = new List<McKey>(); // pressedKeys is a list of the keys that are currently being pressed, previousPressedKeys is a list of the keys that were being pressed in the previous update, used for detecting single key presses

        public McKeyboard()
        {

        }

        // update the keyboard, including updating the state of each key and the list of pressed keys
        public virtual void Update()
        {
            newKeyboard = Keyboard.GetState();

            GetPressedKeys();

        }

        // update the old keyboard state and the list of previously pressed keys, should be called at the end of each update after all input has been processed
        public void UpdateOld()
        {
            oldKeyboard = newKeyboard;

            previousPressedKeys = new List<McKey>();
            for(int i=0;i<pressedKeys.Count;i++)
            {
                previousPressedKeys.Add(pressedKeys[i]);
            }
        }

        // check if a key is currently being pressed, used for checking if a key is being held down
        public bool GetPress(string KEY)
        {

            for(int i=0;i<pressedKeys.Count;i++)
            {

                if(pressedKeys[i].key == KEY)
                {
                    return true;
                }

            }
            

            return false;
        }

        // get the list of currently pressed keys and update the pressedKeys list, used for detecting single key presses and for checking if a key is being held down
        public virtual void GetPressedKeys()
        {
            bool found = false;

            pressedKeys.Clear();
            for(int i=0; i<newKeyboard.GetPressedKeys().Length; i++)
            {

                    pressedKeys.Add(new McKey(newKeyboard.GetPressedKeys()[i].ToString(), 1));
  
            }
        }


        // check if a key was just pressed this update, used for detecting single key presses
        public bool GetSinglePress(string KEY)
        {

            for(int i=0;i<pressedKeys.Count;i++)
            {
                bool isIn = false;

                for(int j=0;j<previousPressedKeys.Count;j++)
                {
                    if(pressedKeys[i].key == previousPressedKeys[j].key)
                    {
                        isIn = true;
                        break;
                    }
                }


                if(!isIn && (pressedKeys[i].key == KEY || pressedKeys[i].print == KEY))
                {
                    return true;
                }
            }



            return false;
        }

        public bool IsHeld(string KEY)
        {
            for(int i=0;i<pressedKeys.Count;i++)
            {
                if(pressedKeys[i].key == KEY || pressedKeys[i].print == KEY)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
