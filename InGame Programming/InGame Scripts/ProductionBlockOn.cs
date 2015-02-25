using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace BaconfistSEInGameScript
{
    class ProductionBlockOn
    {
        IMyGridTerminalSystem GridTerminalSystem;

        // Script BEGIN
        void Main()
        {
            List<IMyTerminalBlock> productionBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProductionBlock>(productionBlocks);
            for (int i = 0; i < productionBlocks.Count; i++)
            {
                productionBlocks[i].GetActionWithName("OnOff_On").Apply(productionBlocks[i]);
            }
        }

        // SCript END
    }
}
