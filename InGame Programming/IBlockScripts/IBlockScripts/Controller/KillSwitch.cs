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
    public class KillSwitch : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           KillSwitch
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source: https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/KillSwitch.cs
           
           Summary
           ------------------------------
           Trigger TimerBlock if ship is out of control (no one on cockpit or Remote control)
            
           Abstract
           ------------------------------
            1 - Load Script in PB
            2 - Set a timer to run this script frequently
            3 - Add !KillSwitch! to the Timers you want to trigger
           
            you can give another tag than "!KillSwitch!" to the PB as an argument to use it
           
          
       */
        void Main(string args)
        {

            List<IMyShipController> ShipControllerCol = getShipController();
            List<IMyTimerBlock> TimerCol = getTimer(args);

            if(TimerCol.Count > 0)
            {
                bool isUnderControl = false;

                for(int i=0;i<ShipControllerCol.Count & !isUnderControl; i++)
                {
                    IMyShipController Cockpit = ShipControllerCol[i] as IMyShipController;
                    isUnderControl = isUnderControl | Cockpit.IsUnderControl;
                }

                if (!isUnderControl)
                {
                    for(int i = 0; i < TimerCol.Count; i++)
                    {
                        TimerCol[i].ApplyAction("TriggerNow");
                    }
                }
            }
        }

        private List<IMyTimerBlock> getTimer(string name)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(Blocks, (x => x.CustomName.Contains(name)));

            return Blocks.ConvertAll<IMyTimerBlock>(x => x as IMyTimerBlock);
        }

        private List<IMyShipController> getShipController()
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(Blocks, (x => x.CubeGrid.Equals(Me.CubeGrid)));

            return Blocks.ConvertAll<IMyShipController>(x => x as IMyShipController);
        }

        #endregion
    }
}
