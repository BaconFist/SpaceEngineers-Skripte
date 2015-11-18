using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class GpsDistance : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           GpsDistance
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/GpsDistance.cs
           
           Summary
           ------------------------------
            Displays distance to GPS Coordinates on LCD


           Abstract
           ------------------------------
            This script will display the distances to a one or more GPS Coordinate on a LCD-Panel.

            Setup:
            1. You need 1x Programable Block and 1x LCD-Panel
            2. Add !GPS_DISTANCE!" to LCD-Panel's Name
            3. Add GPS Coordinates to LCD-Panel's PrivateText (One per Line)(Copy Paste from GPS-TAB)
            4. Change LCD-Panel's PublicTitle to whatever you want to display as heading on LCD. (leave blank for hiding heading)
            5. Set LCD-Panel to Show PublicText
            6. Load Script to Programmable Block and run.

            Tweaks:
            - To refresh distances run Script with a timer
            - to change the !GPS_DISTANCE!-Tag to something else just pass a tag as argument

          
       */
        private string TAG = "!GPS_DISTANCE!";

        private Vector3D PbPos;

        void Main(string args)
        {
            if(args.Length > 0)
            {
                TAG = args;
            }

            //PbPos = Me.Position;
            PbPos = Me.GetPosition();
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks, (x => (x as IMyTerminalBlock).CustomName.Contains(TAG)));
            for(int i = 0; i < blocks.Count; i++)
            {
                displayDistance(blocks[i] as IMyTextPanel);
            }

            

        }

        private void displayDistance(IMyTextPanel lcd)
        {
            StringBuilder sb = new StringBuilder();
            if(lcd.GetPublicTitle().Length > 0)
            {
                sb.AppendLine(lcd.GetPublicTitle());
            }
            string[] gpslist = lcd.GetPrivateText().Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < gpslist.Length; i++)
            {
                string args = gpslist[i];
                BfGps GPS = getGPSVectorFromArg(args);
                if (GPS != null)
                {
                    double distance = Math.Round(Vector3D.Distance(PbPos, GPS.vector), 2);
                    sb.AppendLine(GPS.name + ": " + distance.ToString("### ### ### ###.00") + " m");
                    
                }
            }
            lcd.WritePublicText(sb.ToString());
        }

        

        private BfGps getGPSVectorFromArg(string arg)
        {
           
            string[] argv = arg.Split(':');
            if(argv.Length >= 5)
            {
                BfGps gps = new BfGps();
                gps.name = argv[1];
                gps.vector = new Vector3D(parseDouble(argv[2]), parseDouble(argv[3]), parseDouble(argv[4]));
                return gps;
            } 

            return null;
        }

        

        public double parseDouble(string num)
        {
            double tmp = 0;
            double.TryParse(num, out tmp);
            
            return tmp;
        }

        class BfGps
        {
            public string name;
            public Vector3D vector;
        }
        #endregion
    }
}
