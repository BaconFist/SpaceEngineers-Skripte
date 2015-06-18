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
    class DasBaconfist_InventoryDisplay
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            IMyCargoContainer cargo = GridTerminalSystem.GetBlockWithName("Frachtcontainer \"Cargo 1\"") as IMyCargoContainer;
            IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("LCD-Schirm \"Cargo 1\"") as IMyTextPanel;
            IMyInventory inventory = cargo.GetInventory(0);

            if (cargo is IMyCargoContainer && panel is IMyTextPanel && inventory is IMyInventory)
            {
                panel.WritePublicText(cargo.CustomName + "\n", false);
                panel.WritePublicText("Kapazität: " + String.Format("{0:N2}", Math.Round(Convert.ToDouble(inventory.MaxVolume.ToString())*1000, 2)) + " L\n", true);
                panel.WritePublicText("Belegt:    " + String.Format("{0:N2}", Math.Round(Convert.ToDouble(inventory.CurrentVolume.ToString()) * 1000, 2)) + " L " + String.Format("{0:N2}", Math.Round((100 * (Convert.ToDouble(inventory.CurrentVolume.ToString())) / Convert.ToDouble(inventory.MaxVolume.ToString())), 2)) + "%\n", true);
                panel.WritePublicText("Frei:      " + String.Format("{0:N2}", Math.Round(Convert.ToDouble(inventory.MaxVolume.ToString()) - Convert.ToDouble(inventory.CurrentVolume.ToString()) * 1000, 2)) + " L \n", true);
                panel.WritePublicText("\n", true);
                panel.WritePublicText("Inventar:\n", true);

                if (inventory.GetItems().Count == 0)
                {
                    panel.WritePublicText("Leer\n", true);
                }
                else
                {
                    for (int i = 0; i < inventory.GetItems().Count; i++)
                    {
                        panel.WritePublicText("[" + i.ToString() + "] " + inventory.GetItems()[i].Content.TypeId.ToString().Replace("MyObjectBuilder_", "") + "." + inventory.GetItems()[i].Content.SubtypeId.ToString() + " x " + ammount(inventory.GetItems()[i]) + "\n", true);                        
                    }
                }

            }
        }


        string ammount(IMyInventoryItem item)
        {
            double count = Convert.ToDouble(item.Amount.ToString());
            if(count >= 1000){
                return String.Format("{0:N2}K", Math.Round(count/1000, 2));
            }
            else
            {
                return String.Format("{0:N2}", Math.Round(count, 2));
            }            
        }
        // End InGame-Script
    }
}
