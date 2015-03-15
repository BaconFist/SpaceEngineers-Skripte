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
                GridTerminalSystem.Blocks[i].SetCustomName(GridTerminalSystem.Blocks[i].CustomName + " (Planetenfresser MK1)");
            }
        }
        // End InGame-Script
    }
}
