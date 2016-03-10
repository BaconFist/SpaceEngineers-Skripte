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
    public class BlockActionProxy : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
            BlockActionProxy
            ==============================
            Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
            Source: https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/BlockActionProxy.cs
            
            Summary
            ------------------------------
            Call blockactions using Programmable block's argument.
            (no Timer required, script runs on request)

            Abstract
            ------------------------------
            This script allows you to map multiple Actions to one Button or Sensor-Action.
            The script needs a Block's Name (a simple glob pattern) and the Action to apply as an argument.
            Pattern for the Argument is  BLOCKNAME:ACTION. You can repeat this Pattern seperaten with ";" (See Example). 
            
            The Script applys only an action to a Block if the Block is cappable. For example if you pass "Some*Light*:UseConveyor" it will not try to force a Light to use Conveyors.

            Actual benefit? Instead of filling your Terminal with Groups you can just do things like "Air Vent*:Depressurize_On" to stroke your enemies to death.

            !!Warning!! The number of blocks you can apply an action to seems to be limited. 

            Glob Pattern:
            -  * = 0 or many characters
            -  ? = 1 character

            Update 1: Shows Debug info in Terminal.
            Update 2: BlockType test => Its now psiible to match only certain Blocktypes. "<IMyDoor>*:Open_Off" => close all doors
            
            Example
            ------------------------------
            "Door*Airlock 1:Open_Off;Air Vent 4 Airlock 1:Depressurize_Off" // close airlock doors an pressurise airlock
            "InteriorLight*:OnOff_On" // switch all Interior Lights On
            "*Light*Portside*:OnOff;Grinder 5:OnOff_On" // Switch state of  Portside Lights and enable Grinder 5

           
        */

        string blockPattern = "";
        string action = "";
        string type = null;
        MyDebug Debug;

        void Main(string args)
        {
            Debug = new MyDebug(this);
            string[] argList = args.Split(';');
            Debug.write("Start Script: " + argList.Length.ToString() + " Commands in \"" + args + "\"");
            for (int i_argList = 0; i_argList < argList.Length; i_argList++)
            {
                Debug.write("Try parse => " + argList[i_argList]);
                if (parseArgument(argList[i_argList]))
                {
                    Debug.write("succeed to parse command.");
                    List<IMyTerminalBlock> matches = findBlocks();
                    Debug.write("Found " + matches.Count.ToString() + " matching Blocks");
                    if (matches.Count > 0)
                    {                        
                        for (int i_match = 0; i_match < matches.Count; i_match++)
                        {
                            Debug.write("Apply " + action + " to " + matches[i_match].CustomName);
                            matches[i_match].ApplyAction(action);
                        }
                    }
                } else
                {
                    Debug.write("Failed to parse command.");
                }
            }
        }

        List<IMyTerminalBlock> findBlocks()
        {
            List<IMyTerminalBlock> matches = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(matches, (x => (WildcardMatch.IsLike(blockPattern, (x as IMyTerminalBlock).CustomName, false) && ( type == null || isInstanceOf(type, (x as IMyTerminalBlock)) ) ) && (x as IMyTerminalBlock).HasAction(action)));

            return matches;
        }

        bool parseArgument(string arg, char sep = ':')
        {
            string[] args = arg.Split(sep);
            if (args.Length == 2)
            {
                this.blockPattern = args[0];
                this.action = args[1];
                if (args[0].Contains("<") && args[0].Contains(">"))
                {
                    string[] tmp = args[0].Split('>');
                    if (tmp.Length == 2)
                    {
                        this.type = tmp[0].Replace("<", "");
                        this.blockPattern = tmp[1];
                    }
                }                

                return true;
            }

            return false;
        }

        public bool isInstanceOf(string className, IMyTerminalBlock Block)
        {
            
            switch (className.Trim())
            {
                case "IMyProductionBlock": return (Block is IMyProductionBlock);
                case "IMyCargoContainer": return (Block is IMyCargoContainer);
                case "IMyTextPanel": return (Block is IMyTextPanel);
                case "IMyAssembler": return (Block is IMyAssembler);
                case "IMyRefinery": return (Block is IMyRefinery);
                case "IMyReactor": return (Block is IMyReactor);
                case "IMySolarPanel": return (Block is IMySolarPanel);
                case "IMyBatteryBlock": return (Block is IMyBatteryBlock);
                case "IMyBeacon": return (Block is IMyBeacon);
                case "IMyRadioAntenna": return (Block is IMyRadioAntenna);
                case "IMyAirVent": return (Block is IMyAirVent);
                case "IMyConveyorSorter": return (Block is IMyConveyorSorter);
                case "IMyOxygenTank": return (Block is IMyOxygenTank);
                case "IMyOxygenGenerator": return (Block is IMyOxygenGenerator);
                case "IMyOxygenFarm": return (Block is IMyOxygenFarm);
                case "IMyLaserAntenna": return (Block is IMyLaserAntenna);
                case "IMyThrust": return (Block is IMyThrust);
                case "IMyGyro": return (Block is IMyGyro);
                case "IMySensorBlock": return (Block is IMySensorBlock);
                case "IMyShipConnector": return (Block is IMyShipConnector);
                case "IMyReflectorLight": return (Block is IMyReflectorLight);
                case "IMyInteriorLight": return (Block is IMyInteriorLight);
                case "IMyLandingGear": return (Block is IMyLandingGear);
                case "IMyProgrammableBlock": return (Block is IMyProgrammableBlock);
                case "IMyTimerBlock": return (Block is IMyTimerBlock);
                case "IMyMotorStator": return (Block is IMyMotorStator);
                case "IMyPistonBase": return (Block is IMyPistonBase);
                case "IMyProjector": return (Block is IMyProjector);
                case "IMyShipMergeBlock": return (Block is IMyShipMergeBlock);
                case "IMySoundBlock": return (Block is IMySoundBlock);
                case "IMyCollector": return (Block is IMyCollector);
                case "IMyJumpDrive": return (Block is IMyJumpDrive);
                case "IMyDoor": return (Block is IMyDoor);
                case "IMyGravityGeneratorSphere": return (Block is IMyGravityGeneratorSphere);
                case "IMyGravityGenerator": return (Block is IMyGravityGenerator);
                case "IMyShipDrill": return (Block is IMyShipDrill);
                case "IMyShipGrinder": return (Block is IMyShipGrinder);
                case "IMyShipWelder": return (Block is IMyShipWelder);
                case "IMyLargeGatlingTurret": return (Block is IMyLargeGatlingTurret);
                case "IMyLargeInteriorTurret": return (Block is IMyLargeInteriorTurret);
                case "IMyLargeMissileTurret": return (Block is IMyLargeMissileTurret);
                case "IMySmallGatlingGun": return (Block is IMySmallGatlingGun);
                case "IMySmallMissileLauncherReload": return (Block is IMySmallMissileLauncherReload);
                case "IMySmallMissileLauncher": return (Block is IMySmallMissileLauncher);
                case "IMyVirtualMass": return (Block is IMyVirtualMass);
                case "IMyWarhead": return (Block is IMyWarhead);
                case "IMyFunctionalBlock": return (Block is IMyFunctionalBlock);
                case "IMyLightingBlock": return (Block is IMyLightingBlock);
                case "IMyControlPanel": return (Block is IMyControlPanel);
                case "IMyCockpit": return (Block is IMyCockpit);
                case "IMyMedicalRoom": return (Block is IMyMedicalRoom);
                case "IMyRemoteControl": return (Block is IMyRemoteControl);
                case "IMyButtonPanel": return (Block is IMyButtonPanel);
                case "IMyCameraBlock": return (Block is IMyCameraBlock);
                case "IMyOreDetector": return (Block is IMyOreDetector);
                default: return true;
            }
        }
        

        public static class WildcardMatch
        {
            #region Public Methods 
            public static bool IsLike(string pattern, string text, bool caseSensitive = false)
            {
                pattern = pattern.Replace(".", @"\.");
                pattern = pattern.Replace("?", ".");
                pattern = pattern.Replace("*", ".*?");
                pattern = pattern.Replace(@"\", @"\\");
                pattern = pattern.Replace(" ", @"\s");
                return new System.Text.RegularExpressions.Regex(pattern, caseSensitive ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase).IsMatch(text);
            }
            #endregion
        }
        

        #region copy from here
        class MyDebug
        {
            MyGridProgram program;
            public MyDebug(MyGridProgram program)
            {
                this.program = program;
            }

            public void write(string msg)
            {
                string arg = "[" + program.ElapsedTime.TotalMilliseconds.ToString() + "] " + msg;
                program.Echo(arg);
            }
        }
        #endregion copy to here
        #endregion
    }
}
