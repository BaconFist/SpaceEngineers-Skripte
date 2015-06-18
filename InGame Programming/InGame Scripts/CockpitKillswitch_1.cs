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
    class CockpitKillswitch_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
            for (int i = 0; i < blocks.Count; i++)
            {
                if ((blocks[i] as IMyShipController).)
                {
                    blocks[i].SetCustomName(blocks[i].CustomName.Replace("[TRUE]", "").Replace("[FALSE]", "") + "[TRUE]");
                }
                else
                {
                    blocks[i].SetCustomName(blocks[i].CustomName.Replace("[TRUE]", "").Replace("[FALSE]", "") + "[FALSE]");
                }
            }
        }
        // End InGame-Script
    }
}
