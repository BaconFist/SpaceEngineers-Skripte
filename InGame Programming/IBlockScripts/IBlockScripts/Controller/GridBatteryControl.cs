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
    public class GridBatteryControl : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           GridBatteryControl
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
        const string PID_RECHARGE = "Recharge";
        const string PID_DISCHARGE = "Discharge";
        const string PID_SEMIAUTO = "SemiAuto";

        const string ACTION_SEMIAUTO = "semiauto";
        const string ACTION_RECHARGE = "recharge";
        const string ACTION_DISCHARGE = "discharge";

        const string ARG_SET_TRUE = "on";
        const string ARG_SET_FALSE = "off";

        const string DEBUG_PREFIX = "@";

        private bool isDebug = false;
        
        void Main(string args)
        {
            args = normalizeArgs(args);
            info("Argument (normalized): " + args);
            if (hasCommand(args))
            {
                List<IMyBatteryBlock> Batteries = getBatteriesOnGrid(Me.CubeGrid);
                string[] argv = getArgv(args, ';');
                for(int i = 0; i < argv.Length; i++)
                {
                    doAction(Batteries, getArgv(argv[i], ':'));
                }
            }
        }

        string normalizeArgs(string args)
        {
            string originArgs = args;
            if (args.StartsWith(DEBUG_PREFIX))
            {
                args = args.Remove(0, 1);
                isDebug = true;
                debug("ENABLED");
            }
            args = args.ToLower();
            debug("raw argument: " + originArgs);
            return args;
        }

        private void info(string msg)
        {
            Echo(msg);
        }

        private void debug(string msg)
        {
            if (isDebug)
            {
                info("DEBUG: " + msg);
            }
        }

        private bool hasCommand(string args)
        {
            return (args.Contains(ACTION_DISCHARGE) || args.Contains(ACTION_RECHARGE) || args.Contains(ACTION_SEMIAUTO));
        }

        private string[] getArgv(string args, char seperator)
        {
            string[] argv = args.Split(seperator);

            return argv;
        }

        private void doAction(List<IMyBatteryBlock> Batteries, string[] argv)
        {
            if (argv.Length > 0)
            {
                switch (argv[0])
                {
                    case ACTION_DISCHARGE:
                        doActionDischarge(Batteries, argv);
                        break;
                    case ACTION_RECHARGE:
                        doActionRecharge(Batteries, argv);
                        break;
                    case ACTION_SEMIAUTO:
                        doActionSemiAuto(Batteries, argv);
                        break;
                    default:
                        break;
                }
            }
        }

        private void doActionDischarge(List<IMyBatteryBlock> Batteries, string[] argv)
        {
            updateValues(Batteries, isToggle(argv), getValueFromArg(argv, false), PID_DISCHARGE);
        }

        private void doActionRecharge(List<IMyBatteryBlock> Batteries, string[] argv)
        {
            updateValues(Batteries, isToggle(argv), getValueFromArg(argv, false), PID_RECHARGE);
        }

        private void doActionSemiAuto(List<IMyBatteryBlock> Batteries, string[] argv)
        {
            updateValues(Batteries, isToggle(argv), getValueFromArg(argv, false), PID_SEMIAUTO);
        }

        private bool isToggle(string[] argv)
        {
            if (argv.Length == 2)
            {
                return !(argv[1].Contains(ARG_SET_FALSE) || argv[1].Contains(ARG_SET_FALSE));
            }

            debug("set '" + argv[0] + " => 'toggle'");
            return true;
        }

        private bool getValueFromArg(string[] argv, bool defaultValue)
        {
            if (argv.Length == 2)
            {
                if (argv[1].Contains(ARG_SET_TRUE))
                {
                    debug("set '" + argv[0] + " => ON");
                    return true;
                }
                if (argv[1].Contains(ARG_SET_FALSE))
                {
                    debug("set '" + argv[0] + " => OFF");
                    return false;
                }
            }

            return defaultValue;
        }

        private void updateValues(List<IMyBatteryBlock> Batteries, bool toggle, bool defaultValue, string propertyId)
        {
            bool newValue = defaultValue;

            for (int i = 0; i < Batteries.Count; i++)
            {
                IMyBatteryBlock Battery = Batteries[i];
                if (toggle)
                {
                    newValue = !Battery.GetValueBool(propertyId);
                }
                Battery.SetValueBool(propertyId, newValue);
                debug("'" + Battery.CustomName + "'." + propertyId + " => " + (newValue?"ON":"OFF"));
            }
        }

        private List<IMyBatteryBlock> getBatteriesOnGrid(IMyCubeGrid Grid)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Blocks, (b => (b as IMyTerminalBlock).CubeGrid.Equals(Grid)));
            List<IMyBatteryBlock> Batteries = Blocks.ConvertAll<IMyBatteryBlock>(x => x as IMyBatteryBlock);

            debug("Found " + Batteries.Count + " Batteries on Grid(" + Grid.ToString() + ")");

            return Batteries;
        }
        #endregion
    }
}
