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
            Update: Activates Dampeners.
            
           Abstract
           ------------------------------
            1 - Load Script in PB
            2 - Set a timer to run this script frequently
            3 - Add !KillSwitch! to the Timers you want to trigger
           
            you can give another tag than "!KillSwitch!" to the PB as an argument to use it

            Arguments
            --------------------------
            "!myTag!" to change "!KillSwitch!" to "!myTag!" for this run
            ";false" or ";true" to force de-/activate Emergency Dampeners
            can be combined like "!myTag!;false" 

           
          
       */
        string CONF_TAG = "!KillSwitch!";
        bool CONF_DAMPENERS_ON = true;

        void Main(string args)
        {

            List<IMyShipController> ShipControllerCol = getShipController();
            List<IMyTimerBlock> TimerCol = getTimer(args);

            if(TimerCol.Count > 0)
            {
                bool isUnderControl = false;
                IMyShipController Cockpit = null;
                for (int i=0;i<ShipControllerCol.Count & !isUnderControl; i++)
                {
                    Cockpit = ShipControllerCol[i] as IMyShipController;
                    isUnderControl = isUnderControl | Cockpit.IsUnderControl;
                }

                if (!isUnderControl)
                {
                    doDampeners(Cockpit);
                    for(int i = 0; i < TimerCol.Count; i++)
                    {
                        TimerCol[i].ApplyAction("TriggerNow");
                    }
                }
            }
        }

        private void doDampeners(IMyShipController Cockpit)
        {
            if(CONF_DAMPENERS_ON == true && Cockpit != null && !Cockpit.DampenersOverride)
            {
                Cockpit.ApplyAction("DampenersOverride");
            }
        }

        private void getArgs(string args)
        {
            string[] argv = args.Split(';');
            if(argv.Length >= 1)
            {
                CONF_TAG = argv[0];
                if (argv.Length >= 2)
                {
                    CONF_DAMPENERS_ON = argv[1].ToLower().Equals("true") ? true : false;
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
