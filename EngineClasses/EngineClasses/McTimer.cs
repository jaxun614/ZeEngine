using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ZeEngine
{
    public class McTimer
    {
        public bool goodToGo; // flag for cooldowns, reload times, 
        protected int mSec; // the length of time in the timer
        protected TimeSpan timer = new TimeSpan(); // the timer object
        

        public McTimer(int m)
        {
            goodToGo = false;
            mSec = m;
        }
        public McTimer(int m, bool STARTLOADED)
        {
            goodToGo = STARTLOADED;
            mSec = m;
        }

        // getter and setter for the mili second length
        public int MSec
        {
            get { return mSec; }
            set { mSec = value; }
        }
        // getter for the timer lenfth
        public int Timer
        {
            get { return (int)timer.TotalMilliseconds; }
        }

        
        // update the timer with the elapsed game time, is called from the main game loop
        public void UpdateTimer()
        {
            timer += Globals.gameTime.ElapsedGameTime;
        }

        // update the timer with a speed up, called from main game loop
        public void UpdateTimer(float SPEED)
        {
            timer += TimeSpan.FromTicks((long)(Globals.gameTime.ElapsedGameTime.Ticks * SPEED));
        }

        // add mili secs to the timer
        public virtual void AddToTimer(int MSEC)
        {
            timer += TimeSpan.FromMilliseconds((long)(MSEC));
        }

        // test for the timer, if the timer is less than the total timer time or it is good to go return true
        public bool Test()
        {
            if(timer.TotalMilliseconds >= mSec || goodToGo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // reset the timer to zero and return false
        public void Reset()
        {
            timer = timer.Subtract(new TimeSpan(0, 0, mSec/60000, mSec/1000, mSec%1000));
            if(timer.TotalMilliseconds < 0)
            {
                timer = TimeSpan.Zero;
            }
            goodToGo = false;
        }

        // rset the timer and creat a new timer
        public void Reset(int NEWTIMER)
        {
            timer = TimeSpan.Zero;
            MSec = NEWTIMER;
            goodToGo = false;
        }

        // reset to zero and return false
        public void ResetToZero()
        {
            timer = TimeSpan.Zero;
            goodToGo = false;
        }

        // return a new XElement "Timer" with "mSec", and "timer"
        public virtual XElement ReturnXML()
        {
            XElement xml= new XElement("Timer",
                                    new XElement("mSec", mSec),
                                    new XElement("timer", Timer));



            return xml;
        }

        // sets the time of the timer with the given time span
        public void SetTimer(TimeSpan TIME)
        {
            timer = TIME;
        }

        // set the time of the timer with the given mili seconds
        public virtual void SetTimer(int MSEC)
        {
            timer = TimeSpan.FromMilliseconds((long)(MSEC));
        }
    }
}
