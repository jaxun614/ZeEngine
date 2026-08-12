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

    public delegate void PassObject(object i); // delegate for passing an object to a method, used for passing information to the draw manager through the delegate in the SceneItem class without having to reference the draw manager in the SceneItem class


    public delegate object PassObjectAndReturn(object i); // delegate for passing an object to a method and returning an object, used for passing information to the draw manager through the delegate in the SceneItem class and getting information back from the draw manager without having to reference the draw manager in the SceneItem class



    public class Globals
    {
        public static int screenHeight, screenWidth, gameState = 0; // screenHeight and screenWidth are the dimensions of the game window, used for adjusting the position of items based on the size of the game window and for adjusting the position of the mouse based on the size of the game window, gameState is the current state of the game, used for determining what to update and draw in the main game loop

        public static string appDataFilePath; // the file path for the app data folder, used for saving and loading data to and from the app data folder

        public static Random rand = new Random(); // random number generator, used for generating random numbers for various purposes in the game

        public static System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US"); // culture info for parsing numbers, used for parsing numbers from strings in a cosistent way regardless of the culture settings of the computer running the game

        public static ContentManager content; // the content manager for the game, used for loading and managing game content such as textures, sounds, and effects


        public static SpriteBatch spriteBatch; // the sprite batch for the game, used for drawing textures to the screen
        public static GraphicsDevice graphicsDevice; // the graphics device for the game, used for creating textures and other graphics resources
        public static SpriteFont font;
        public static Effect normalEffect; 

        //public static OptionsMenu optionsMenu; // the options menu for the game, used for storing and managing the options menu and the options in the options menu

        public static Save save; // the save for the game, used for storing and managing the save data for the game

        public static McKeyboard keyboard; // the keyboard control for the game, used for checking the state of the keyboard and for checking if certain keys are pressed
        public static McMouseControl mouse; // 

        public static Texture2D pixel; // a 1x1 white pixel texture, used for drawing rectangles and other shapes to the screen

        public static GameTime gameTime; // the game time for the game, used for getting the elapsed time since the last update and for getting the total game time since the start of the game

        // get the distance from the current pos to a target pos
        public static float GetDistance(Vector2 pos, Vector2 target)
        {
            return (float)Math.Sqrt(Math.Pow(pos.X - target.X, 2) + Math.Pow(pos.Y - target.Y, 2));
        }

        // moves the current pos towards target pos
        public static Vector2 RadialMovement(Vector2 pos, Vector2 focus, float speed)
        {
            float dist = Globals.GetDistance(pos, focus);

            if(dist <= speed)
            {
                return focus - pos;
            }
            else
            {
                return (focus - pos) * speed/dist;
            }
        }

        // rotates the current pos towards a target pos
        public static float RotateTowards(Vector2 Pos, Vector2 focus)
        {

            float h, sineTheta, angle;
            if(Pos.Y-focus.Y != 0)
            {
                h = (float)Math.Sqrt(Math.Pow(Pos.X-focus.X, 2) + Math.Pow(Pos.Y-focus.Y, 2));
                sineTheta = (float)(Math.Abs(Pos.Y-focus.Y)/h); //* ((item.Pos.Y-focus.Y)/(Math.Abs(item.Pos.Y-focus.Y))));
            }
            else
            {
                h = Pos.X-focus.X;
                sineTheta = 0;
            }

            angle = (float)Math.Asin(sineTheta);

            // Drawing diagonial lines here.
            //Quadrant 2
            if(Pos.X-focus.X > 0 && Pos.Y-focus.Y > 0)
            {
                angle = (float)(Math.PI*3/2 + angle);
            }
            //Quadrant 3
            else if(Pos.X-focus.X > 0 && Pos.Y-focus.Y < 0)
            {
                angle = (float)(Math.PI*3/2 - angle);
            }
            //Quadrant 1
            else if(Pos.X-focus.X < 0 && Pos.Y-focus.Y > 0)
            {
                angle = (float)(Math.PI/2 - angle);
            }
            else if(Pos.X-focus.X < 0 && Pos.Y-focus.Y < 0)
            {
                angle = (float)(Math.PI/2 + angle);
            }
            else if(Pos.X-focus.X > 0 && Pos.Y-focus.Y == 0)
            {
                angle = (float)Math.PI*3/2;
            }
            else if(Pos.X-focus.X < 0 && Pos.Y-focus.Y == 0)
            {
                angle = (float)Math.PI/2;
            }
            else if(Pos.X-focus.X == 0 && Pos.Y-focus.Y > 0)
            {
                angle = (float)0;
            }
            else if(Pos.X-focus.X == 0 && Pos.Y-focus.Y < 0)
            {
                angle = (float)Math.PI;
            }

            return angle;
        }
    }
}
