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
    public class FormOption
    {
        public string name; // name of option, used for display and as xml 
        public object value; // value of option, can be any object

        public FormOption(string NAME, object VALUE)
        {
            name = NAME;
            value = VALUE;
        }
    }
}
