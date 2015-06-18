using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using VRage;

namespace BaconfistSEInGameScript
{
    class DynamicActionScript
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        IMyProgrammableBlock Me;
        Action<string> Echo;
        TimeSpan ElapsedTime;

        // Begin InGame-Script
        string blockPattern = "";
        string action = "";

        void Main(string argument)
        {
            string[] argList = argument.Split(';');

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


        // End InGame-Script
    }
}
