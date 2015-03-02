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
    class renamer
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            for (int i = 0; i < GridTerminalSystem.Blocks.Count; i++)
            {
                if (GridTerminalSystem.Blocks[i].CustomName.IndexOf("Hexler") < 0)
                {
                    GridTerminalSystem.Blocks[i].SetCustomName(GridTerminalSystem.Blocks[i].CustomName + " Hexler");
                }
            }
        }
        // End InGame-Script
    }
}
