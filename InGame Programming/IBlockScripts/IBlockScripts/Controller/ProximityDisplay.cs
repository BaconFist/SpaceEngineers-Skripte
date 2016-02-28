using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class ProximityDisplay : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           ProximityDisplay
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           

           Abstract
           ------------------------------
          
           
           Example
           ------------------------------
          
       */
        string tag = "!PROX_LCD!";

        void Main(string args)
        {
            if (args.Length > 0)
            {
                tag = args;
            }
            List<IMySensorBlock> SensorList = getSensors();
            List<IMyTextPanel> TextPanelList = getTextPanels();
            if(TextPanelList.Count > 0 && SensorList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < SensorList.Count; i++)
                {
                     sb.AppendLine((SensorList[i].IsActive ? "[PROX]" : "[CLEAR]") + " " +SensorName(SensorList[i]));
                }
                for(int i = 0; i < TextPanelList.Count; i++)
                {
                    TextPanelList[i].WritePublicText(sb.ToString());
                    
                }
            }
        }
        
        double SensorDistance(Vector3D target, IMySensorBlock Sensor)
        {
            

            return Vector3D.Distance(Sensor.GetPosition(), target);
        }

        private string SensorName(IMySensorBlock Sensor)
        {
            return Sensor.CustomName.Substring(0, Sensor.CustomName.IndexOf(tag)-1).Trim();
        }
        
        private List<IMySensorBlock> getSensors()
        {
            List<IMyTerminalBlock> SensorList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(SensorList, (x => (x.CustomName.Contains(tag))));

            return SensorList.ConvertAll<IMySensorBlock>(x => x as IMySensorBlock);
        }

        private List<IMyTextPanel> getTextPanels()
        {
            List<IMyTerminalBlock> SensorList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(SensorList, (x => (x.CustomName.Contains(tag))));

            return SensorList.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
        }
        #endregion
    }
}
