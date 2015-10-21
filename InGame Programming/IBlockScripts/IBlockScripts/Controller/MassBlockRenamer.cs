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
    public class MassBlockRenamer : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           MassBlockRenamer
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  http://git.io/vORsd
           
           Summary
           ------------------------------
           Block renamer

           Abstract
           ------------------------------
            use argument for new Text and Text to replace splittet by ;

            Commands:
             * Commands are used instead oldText like "THECOMMAND;newText"
                CMD:ADD => adds newText to all blocks without replacing (adding ship id/name to all blocks)
                            Example:  "CMD:ADD; [My Awesome Ship]"
                CMD:DEL => delete newText 
                            Exapmle: "CMD:DEL;weird text to be removed"
           
           Example
           ------------------------------
            "oldText;new Text" (without " )
            
          
       */

        const string CMD_ADD = "CMD:ADD";
        const string CMD_DEL = "CMD:DEL";

        void Main(string args)
        {
            string[] argv = args.Split(';');
            if (argv.Length == 2)
            {
                string oldString = argv[0];
                string newString = argv[1];

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                if (oldString.Equals(CMD_ADD))
                {
                    Echo("ADD");
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
                    doAdd(blocks, newString);
                } else if (oldString.Equals(CMD_DEL))
                {
                    Echo("DEL");
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
                    doDel(blocks, newString);
                } else
                {
                    Echo("default");
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => (x as IMyTerminalBlock).CustomName.Contains(oldString)));
                    doReplace(blocks, oldString, newString);
                }               
            }
        }

        void doAdd(List<IMyTerminalBlock> blocks, string newText)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTerminalBlock block = (blocks[i] as IMyTerminalBlock);
                string oldName = block.CustomName;
                block.SetCustomName(oldName + newText);
            }
        }

        void doDel(List<IMyTerminalBlock> blocks, string newText)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTerminalBlock block = (blocks[i] as IMyTerminalBlock);
                string oldName = block.CustomName;
                block.SetCustomName(oldName.Replace(newText, ""));
            }
        }


        void doReplace(List<IMyTerminalBlock> blocks, string oldText, string newText)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTerminalBlock block = (blocks[i] as IMyTerminalBlock);
                string oldName = block.CustomName;
                block.SetCustomName(oldName.Replace(oldText, newText));
            }
        }

        #endregion
    }
}