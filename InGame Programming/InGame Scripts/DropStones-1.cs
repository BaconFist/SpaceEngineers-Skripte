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
    class DropStones_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
           

            IMyShipConnector Container = (GridTerminalSystem.GetBlockWithName("Verbinder") as IMyShipConnector);
            List<IMyInventoryItem> itemsToMove = new List<IMyInventoryItem>();
            if (Container.HasInventory())
            {
                IMyInventory Inventory;
                List<IMyInventoryItem> Items;
                IMyInventoryItem Item;
                for (int i = 0; i < GridTerminalSystem.Blocks.Count; i++)
                {
                    if (GridTerminalSystem.Blocks[i].HasInventory())
                    {
                        for (int ic = 0; ic < GridTerminalSystem.Blocks[i].GetInventoryCount(); ic++)
                        {
                            Inventory = GridTerminalSystem.Blocks[i].GetInventory(ic);
                            Items = Inventory.GetItems();
                            for (int i2 = Items.Count-1; i2 > -1; i2--)
                            {
                                Item = Items[i2];
                                if (Item.Content.SubtypeName.Contains("Stone"))
                                {
                                    Container.GetInventory(0).TransferItemFrom(Inventory, i2, null, true, null);
                                }
                            }
                        }
                    }
                }
            }
        }
        // End InGame-Script
    }
}
