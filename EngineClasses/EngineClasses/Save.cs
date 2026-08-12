#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion


namespace ZeEngine
{
    public class Save
    {
        public int gameId, userId; // 
        public string gameName, baseFolder, backupFolder, backupPath;

        public bool loadingID = true;

        public XDocument saveFile; // an XDocument to hold the new save data



        public Save(int GAMEID, string GAMENAME)
        {
            gameId = GAMEID;
            gameName = GAMENAME;
            //heroLevel = 1;

            //LoadGame();
            backupFolder = "bzaxcyk";
            backupPath = "bath";

            baseFolder = Globals.appDataFilePath +"\\"+gameName+"";

            CreateBaseFolders();

        }

        // creates the base folders for the game, one with the games name, one for xml, and the other for savedGames
        public void CreateBaseFolders()
        {
            CreateFolder(Globals.appDataFilePath +"\\"+gameName+"");
            CreateFolder(Globals.appDataFilePath +"\\"+gameName+"\\XML");
            CreateFolder(Globals.appDataFilePath +"\\"+gameName+"\\XML\\SavedGames");
        }

        // takes in a path and creates a folder
        public void CreateFolder(string s)
        {
            DirectoryInfo CreateSiteDirectory = new DirectoryInfo(s);
            if(!CreateSiteDirectory.Exists)
            {
                CreateSiteDirectory.Create();
            }
        }
        
        // takes in a file path and checks if it exists
        public virtual bool CheckIfFileExists(string PATH)
        {
            bool fileExists;

            fileExists = File.Exists(Globals.appDataFilePath + "\\"+gameName+"\\"+PATH);


            return fileExists;
            //return true;
        }




        // takes in a file path and deletes it
        public virtual void DeleteFile(string PATH)
        {
            File.Delete(PATH);
        }

        #region Getting XML Files

        // takes in a file path and checks if the file exists before returning it, otherwide it returns null
        public XDocument GetFile(string FILE)
        {
            if(CheckIfFileExists(FILE))
            {
                return XDocument.Load(Globals.appDataFilePath + "\\"+gameName+"\\"+FILE);
            }

            return null;
        }


        #endregion

        // gets the file and returns the XDocument
        public virtual XDocument LoadFile(string FILEPATH, bool CHECKKEY = true)
        {
            XDocument xml = GetFile(FILEPATH);


            return xml;
        }



        // takes in the xml doc and convers it to a string, then it goes to binary, and then the ascii is incoded
        public virtual void HandleSaveFormates(XDocument xml)
        {

            byte[] compress = Encoding.ASCII.GetBytes(StringToBinary(xml.ToString()));
            File.WriteAllBytes(Globals.appDataFilePath + "\\"+gameName+"\\XML\\SavedGames\\"+Convert.ToString(gameId, Globals.culture), compress);



        }

        // takes in an XDoc and a file path. then it saves the path to the xml 
        public virtual void HandleSaveFormates(XDocument xml, string PATH)
        {
            xml.Save(Globals.appDataFilePath + "\\"+gameName+"\\XML\\"+PATH);
        }


        #region Converting to Binary and back

        // takes in a string an converts it to binary
        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach(char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        // takes in a binary string an convers it to a string
        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for(int i = 0;i < data.Length;i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }

            return Encoding.ASCII.GetString(byteList.ToArray());
        }
        #endregion

    }
}
