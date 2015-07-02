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
            
            Example
            ------------------------------
            "Door*Airlock 1:Open_Off;Air Vent 4 Airlock 1:Depressurize_Off" // close airlock doors an pressurise airlock
            "InteriorLight*:OnOff_On" // switch all Interior Lights On
            "*Light*Portside*:OnOff;Grinder 5:OnOff_On" // Switch state of  Portside Lights and enable Grinder 5

           
        */

        string blockPattern = "";
        string action = "";

        void Main(string args)
        {
            string[] argList = args.Split(';');

            for (int i_argList = 0; i_argList < argList.Length; i_argList++)
            {
                if (parseArgument(argList[i_argList]))
                {
                    List<IMyTerminalBlock> matches = findBlocks();
                    if (matches.Count > 0)
                    {
                        for (int i_match = 0; i_match < matches.Count; i_match++)
                        {
                            matches[i_match].ApplyAction(action);
                        }
                    }
                }
            }
        }

        List<IMyTerminalBlock> findBlocks()
        {
            List<IMyTerminalBlock> matches = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(matches, (x => (WildcardMatch.IsLike(blockPattern, (x as IMyTerminalBlock).CustomName, false)) && (x as IMyTerminalBlock).HasAction(action)));

            return matches;
        }

        bool parseArgument(string arg, char sep = ':')
        {
            string[] args = arg.Split(sep);
            if (args.Length == 2)
            {
                this.blockPattern = args[0];
                this.action = args[1];

                return true;
            }

            return false;
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
        #endregion
    }
}
