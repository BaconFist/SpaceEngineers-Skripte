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
    public class MassBlockRenamer : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           MassBlockRenamer
           ==============================
           Copyright (c) 2016 Thomas Klose <thomas@bratler.net>
           Source:  http://git.io/vORsd
           
           Summary
           ------------------------------
           Change names of many Blocks at once
            
           Abstract
           ------------------------------
                simple: "oldText;newText"
                limited to Group: "Groupname;oldText;newText"
                Globs in oldText:
                        * -> matches any number of any characters including none
                        ? -> matches any single character
                        [abc] -> matches one character given in the bracket
                        [a-z] -> matches one character from the range given in the bracket
                Pipe oldText to newText with "|": (good for adding text to stuff)
                        Inject Text: "Light*;Shiny |" will replace "Light 1", "Light 22" or any matching "Light*" with "Shiny Light 1", "Shiny Light 22" and so on.
                        Append Text: "*;| MyShip"
                        Prepend Text: "*;My Ship |"
                        ! Caution !: something like "a;|derp" could end in a mess like "Smaderpll Readerpctor 11" 
                 
                Script is Limited to the CubeGrid of the PB to prevent unwanted changes on docked ships.
           
           Example
           ------------------------------
                Blocks: "Interior Light 1", "Interior Light 2", "Interior Light 3", "Small Reactor 11"
                
                Replace: "Small Reactor;Power Generator" => "Interior Light 1", "Interior Light 2", "Interior Light 3", "Power Generator 11"
                Append: "Light*;| CargonRoom 1" => "Interior Light 1 CargonRoom 1", "Interior Light 2 CargonRoom 1", "Interior Light 3 CargonRoom 1", "Small Reactor 11"
                Prepend: "*;MyShip |" => "MyShip Interior Light 1", "MyShip Interior Light 2", "MyShip Interior Light 3", "MyShip Small Reactor 11"
                Remove: "Interior;" => "Light 1", "Light 2", "Light 3", "Small Reactor 11"
         */
        const string MARKER_MATCH = "|";

        public void Main(string args)
        {
            Argument Arg = getArgument(args);
            Glob Filter = new Glob(Arg.glob);
            Echo(Arg.glob + " => " + Filter.Rgx.ToString());
            List<IMyTerminalBlock> Group = getBlockGroup(Arg);
            List<IMyTerminalBlock> Blocks = findBlocksByGlob(Group, Filter);
            replaceNamesInBlocklist(Blocks, Filter, Arg);
        } 

        public void replaceNamesInBlocklist(List<IMyTerminalBlock> Blocks, Glob Filter, Argument Arg)
        {
            for(int i=0;i<Blocks.Count; i++)
            {
                replaceBlockname(Blocks[i], Filter, Arg);
            }
        }

        public void replaceBlockname(IMyTerminalBlock Block, Glob Filter, Argument Arg)
        {
            StringBuilder slug = new StringBuilder(Block.CustomName);
            string[] matches = Filter.getMatches(Block.CustomName);
            for(int i = 0; i < matches.Length; i++)
            {
                slug = slug.Replace(matches[i], Arg.replacement.Replace(MARKER_MATCH, matches[i]));
            }
            Block.SetCustomName(slug.ToString());
        }

        public Argument getArgument(string args)
        {
            Argument Arg = new Argument();
            string[] argv = args.Split(';');
            if(argv.Length == 2)
            {
                // no Group
                Arg.glob = argv[0];
                Arg.replacement = argv[1];
            } else if(argv.Length > 2)
            {
                Arg.group = argv[0];
                Arg.glob = argv[1];
                Arg.replacement = argv[2];
            }


            return Arg;           
        }

        public List<IMyTerminalBlock> getBlockGroup(Argument Arg)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            if(Arg.group != null)
            {
                IMyBlockGroup Group = GridTerminalSystem.GetBlockGroupWithName(Arg.group);
                if(Group != null)
                {
                    Blocks = Group.Blocks;
                }
            } else
            {
                GridTerminalSystem.GetBlocks(Blocks);
            }

            return Blocks;
        }
        
        public List<IMyTerminalBlock> findBlocksByGlob(List<IMyTerminalBlock> BlockGroup, Glob Filter)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            for(int i = 0; i < BlockGroup.Count; i++)
            {
                if (Filter.isMatch(BlockGroup[i].CustomName) && BlockGroup[i].CubeGrid.Equals(Me.CubeGrid))
                {
                    Blocks.Add(BlockGroup[i]);
                }
            };

            return Blocks;
        }

        public class Argument
        {
            public string group = null;
            public string glob;
            public string replacement;
        }

        public class Glob
        {
            public System.Text.RegularExpressions.Regex Rgx;
            string pattern;

            public Glob(string pattern)
            {
                this.pattern = pattern;
                this.Rgx = getRegexFromGlob(pattern);
            }

            private System.Text.RegularExpressions.Regex getRegexFromGlob(string glob)
            {
                string pattern = glob
                    .Replace(@"*", @".*")
                    .Replace(@"?", @".")
                 //   .Replace(@"\[!([^\]]+)\]", @"[^$1]")
                    .Replace(@"\[([^\]]+)\]", @"[$1]");

                

                return new System.Text.RegularExpressions.Regex(pattern);
            }

            public bool isMatch(string input)
            {
                return Rgx.IsMatch(input);
            }

            public string[] getMatches(string input)
            {
                System.Text.RegularExpressions.MatchCollection RgxMatches = Rgx.Matches(input);
                List<string> Matches = new List<string>();
                for(int i = 0; i < RgxMatches.Count; i++)
                {
                    Matches.Add(RgxMatches[i].Value);
                }

                return Matches.ToArray();
            }
        }

        #endregion
    }
}