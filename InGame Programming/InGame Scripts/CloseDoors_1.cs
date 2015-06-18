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
    class CloseDoors_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            List<IMyTerminalBlock> doors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].ApplyAction("Open_Off");
            }
        }
        // End InGame-Script
    }
}
