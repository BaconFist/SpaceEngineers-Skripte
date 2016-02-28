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
        //---- DEBUG OPTIONS BEGIN --------
        private const int DBG_INFO = 0;
        //---- DEBUG OPTIONS END ----------
        private const string GPS_PARSE_PATTERN = @"GPS:(?<name>[^:]+):(?<x>[^:]+):(?<y>[^:]+):(?<z>[^:]+):";

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
                updatePanel(blocks[i] as IMyTextPanel);
            }            
        }

        private bool updatePanel(IMyTextPanel TextPanel)
        {
            List<BfGps> Waypoints = getGpsWaypoints(TextPanel);
            StringBuilder Text = new StringBuilder();
            if(TextPanel.GetPublicTitle().Trim().Length > 0)
            {
                Text.AppendLine(TextPanel.GetPublicTitle());
            }
            for(int i = 0; i < Waypoints.Count; i++)
            {
                BfGps Gps = Waypoints[i];
                if(Gps != null)
                {
                    double distance = Math.Round(Vector3D.Distance(PbPos, Gps.vector), 2);
                    Text.AppendLine(Gps.name + ": " + distance.ToString("### ### ### ###.00") + " m");
                }
            }

            return (displayOnPrivateText(TextPanel, Text) || displayOnPublicText(TextPanel, Text));
        }

        private bool displayOnPrivateText(IMyTextPanel TextPanel, StringBuilder Text)
        {
            string toPrivateTag = TAG.ToLower() + ">private";
            if (TextPanel.CustomName.ToLower().Contains(toPrivateTag))
            {
                return TextPanel.WritePrivateText(Text.ToString());
            }

            return false;
        }

        private bool displayOnPublicText(IMyTextPanel TextPanel, StringBuilder Text)
        {
            return TextPanel.WritePublicText(Text.ToString());
        }

        private List<BfGps> getGpsWaypoints(IMyTextPanel TextPanel)
        {
            Dictionary<string, BfGps> Waypoints = new Dictionary<string, BfGps>();
            List<IMyTextPanel> Panels = getPipeFromTextPanel(TextPanel);
            Panels.Add(TextPanel);
            for(int i = 0; i < Panels.Count; i++)
            {
                IMyTextPanel TempPanel = Panels[i];
                Waypoints = addWaypointsToDict(TempPanel.CustomName, Waypoints);
                Waypoints = addWaypointsToDict(TempPanel.GetPublicText(), Waypoints);
                Waypoints = addWaypointsToDict(TempPanel.GetPrivateTitle(), Waypoints);
                Waypoints = addWaypointsToDict(TempPanel.GetPrivateText(), Waypoints);
            }
            List<BfGps> GpsPoints = new List<BfGps>();
            foreach (KeyValuePair<string,BfGps> item in Waypoints)
            {
                GpsPoints.Add(item.Value);
            }

            return GpsPoints;
        }

        private List<IMyTextPanel> getPipeFromTextPanel(IMyTextPanel TextPanel)
        {
            string pipeTag = ">" + TAG.ToLower();
            pipeTag = System.Text.RegularExpressions.Regex.Escape(pipeTag);
            string pattern = @"\S+" + pipeTag;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.MatchCollection Matches = regex.Matches(TextPanel.CustomName);
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            for(int i = 0; i < Matches.Count; i++)
            {
                string srcTag = Matches[i].Value.Replace(pipeTag, "");
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => x.CustomName.Contains(srcTag)));
            }

            return Blocks.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
        }

        private Dictionary<string, BfGps> addWaypointsToDict(string data, Dictionary<string, BfGps> Waypoints)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(GPS_PARSE_PATTERN);
            System.Text.RegularExpressions.MatchCollection matches = regex.Matches(data);
            for(int i = 0; i < matches.Count; i++)
            {
                BfGps gps = getGPSVectorFromArg(matches[i].Value);
                if (!Waypoints.ContainsKey(gps.name))
                {
                    Waypoints.Add(gps.name, gps);
                }
            }

            return Waypoints;
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
