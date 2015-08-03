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

           
           Example
           ------------------------------
            "oldText;new Text" (without " )
            
          
       */
        void Main(string args)
        {
            string[] argv = args.Split(';');
            if(argv.Length == 2)
            {
                string oldString = argv[0];
                string newString = argv[1];

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => (x as IMyTerminalBlock).CustomName.Contains(oldString)));
                
                for(int i = 0; i < blocks.Count; i++)
                {
                    (blocks[i] as IMyTerminalBlock).SetCustomName((blocks[i] as IMyTerminalBlock).CustomName.Replace(oldString,newString));
                }
                
            }
        }
        #endregion
    }
}
