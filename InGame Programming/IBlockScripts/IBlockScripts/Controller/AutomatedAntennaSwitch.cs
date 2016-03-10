using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class AutomatedAntennaSwitch : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           AutomatedAntennaSwitch
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
        void Main(string args)
        {
            List<IMyTerminalBlock> antennas = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antennas, x => x.CubeGrid.Equals(Me.CubeGrid));
            Echo(antennas.Count.ToString() + " Antennas");
            List<IMyTerminalBlock> connectors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors, x => x.CubeGrid.Equals(Me.CubeGrid));
            Echo(connectors.Count.ToString() + " connectors");
            bool locked = false;
            for(int i = 0; i < connectors.Count && !locked; i++)
            {
                locked = (connectors[i] as IMyShipConnector).IsConnected;
                Echo(connectors[i].CustomName + " " + (locked?"LOCKED":"UNLOCKED"));
            }
            string action = locked ? "OnOff_Off" : "OnOff_On";
            Echo("Action = " + action);
            for(int i = 0; i < antennas.Count; i++)
            {
                antennas[i].ApplyAction(action);
                Echo(antennas[i].CustomName + " Apply " + action);
            }
        }
        #endregion
    }
}
