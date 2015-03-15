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
    class SuckInventory {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
// Begin InGame-Script
        void Main()
        {
            IMyTerminalBlock target = GridTerminalSystem.GetBlockWithName("");
            if ((target is IMyTerminalBlock) && target.HasInventory())
            {
                IMyInventory targetInventory = target.GetInventory(0);
                for (int i_blocks = 0; i_blocks < GridTerminalSystem.Blocks.Count; i_blocks++)
                {
                    IMyTerminalBlock block = GridTerminalSystem.Blocks[i_blocks];
                    for (int i_inventory = 0; i_inventory < block.GetInventoryCount(); i_inventory++)
                    {
                        IMyInventory inventory = block.GetInventory(i_inventory);
                        for (int i_item = 0; i_item < inventory.GetItems().Count; i_item++)
                        {
                            targetInventory.TransferItemFrom(inventory, i_item);
                        }
                    }
                }
            }
        }
// End InGame-Script
    }
}
