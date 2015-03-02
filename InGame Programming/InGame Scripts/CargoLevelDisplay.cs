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
    class CargoLevelDisplay
    {
        IMyGridTerminalSystem GridTerminalSystem;
// Begin InGame-Script 
        void CargoCapacityV2() {
            IMyCargoContainer cargo = (GridTerminalSystem.GetBlockWithName("Frachtcontainer") as IMyCargoContainer);
            IMyTextPanel textPanel = (GridTerminalSystem.GetBlockWithName("Textpanel Frachtinfo") as IMyTextPanel);
            if ((cargo is IMyCargoContainer) && (textPanel is IMyTextPanel))
            {
                IMyInventory Inventory =  cargo.GetInventory(0);
                StringBuilder text = new StringBuilder();
                text.AppendLine("::: Lager '" + cargo.CustomName + "':::");
                text.AppendLine("");
                text.AppendLine("Belegt zu " + (100 / (Convert.ToDouble(Inventory.MaxVolume.ToString()) / Convert.ToDouble(Inventory.CurrentVolume.ToString()))).ToString() + "%");
                text.AppendLine("Aktuelles Volumen");
                text.AppendLine("  " + (Convert.ToDouble(Inventory.CurrentVolume.ToString())).ToString() + " L");
                text.AppendLine("Freies Volumen");
                text.AppendLine("  " + (Convert.ToDouble(Inventory.MaxVolume.ToString()) - Convert.ToDouble(Inventory.CurrentVolume.ToString())).ToString() + " L");
                text.AppendLine("Maximales Volumen");
                text.AppendLine("  " + (Convert.ToDouble(Inventory.MaxVolume.ToString())).ToString() + " L");
                
                textPanel.WritePublicText(text.ToString());
                textPanel.ShowTextureOnScreen();
                textPanel.ShowPublicTextOnScreen();
            }


        }



        List<String> cargoBlockGroupNames = new List<string>();
        int opt_digits = 1;

        void cargoGroups()
        {
            cargoBlockGroupNames.Add("Lageranzeige Lager 1");
            cargoBlockGroupNames.Add("Lageranzeige Lager 2"); 
            cargoBlockGroupNames.Add("Lageranzeige Lager 3"); 
            cargoBlockGroupNames.Add("Lageranzeige Lager 4"); 
            cargoBlockGroupNames.Add("Lageranzeige Raffinerie 01");  
        }

        void Main()
        {
            this.cargoGroups();
            String cargoBlockGroupName = "";
            for (int cargoIndex = 0; cargoIndex < this.cargoBlockGroupNames.Count; cargoIndex++)
            {
                cargoBlockGroupName = this.cargoBlockGroupNames[cargoIndex];
                IMyBlockGroup CargoBlockGroup = this.getBlockGroupByName(cargoBlockGroupName);
                if (this.isBlockGroup(CargoBlockGroup))
                {
                    IMyTextPanel LcdPanel = this.getLcdPanelFromGroup(CargoBlockGroup);
                    if (LcdPanel is IMyTextPanel)
                    {
                        StringBuilder LcdText = new StringBuilder();
                        LcdText.AppendLine("Last Update: " + DateTime.Now.ToString());

                        String curLine = "";
                        IMyTerminalBlock CurBlock;
                        for (int i = 0; i < CargoBlockGroup.Blocks.Count; i++)
                        {
                            curLine = "";
                            if (CargoBlockGroup.Blocks[i].HasInventory())
                            {
                                CurBlock = CargoBlockGroup.Blocks[i];
                                if (CurBlock.GetInventoryCount().Equals(1))
                                {
                                    curLine = this.getInventoryCapacityLine(CurBlock, CurBlock.CustomName, 0);
                                }
                                else
                                {
                                    curLine = CurBlock.CustomName + ": ";
                                    curLine += this.getInventoryCapacityLine(CurBlock, "In", 0);
                                    curLine += " | ";
                                    curLine += this.getInventoryCapacityLine(CurBlock, "Out", 1);
                                }
                                LcdText.AppendLine(curLine);
                            }
                        }
                        this.WritePublicTextToLcd(LcdPanel, LcdText.ToString());
                    }
                }
            }
        }

        String getInventoryCapacityLine(IMyTerminalBlock TerminalBlock, String customName, int inventoryIndex)
        {
            StringBuilder Line = new StringBuilder();
            IMyInventory Inventory = TerminalBlock.GetInventory(inventoryIndex);
            if (Inventory is IMyInventory)
            {
                double levelRaw = this.getInventoryFuelLevel(Inventory);
                double levelRounded = Math.Round(levelRaw, this.opt_digits);
                String levelFormatted = String.Format("{0:N" + Convert.ToString(this.opt_digits) + "}", levelRounded);
                Line.Append(customName);
                Line.Append(": ");
                Line.Append(this.getLevelDisplaybar(levelRaw) + " ");
                Line.Append(levelFormatted + "% ");
            }

            return Line.ToString();
        }

        String getLevelDisplaybar(double level)
        {
            String bar = "[";
            double levelCalc = level;
            for (int i = 0; i < 10; i++)
            {
                levelCalc -= 10;
                if (levelCalc > 0)
                {
                    bar += ":";
                }
                else
                {
                    bar += ".";
                }
            }
            bar += "]";

            return bar;
        }

        double getInventoryFuelLevel(IMyInventory Inventory)
        {
            if (Inventory.IsFull)
            {
                return (double)100;
            }
            else
            {
                double max = Convert.ToDouble(Inventory.MaxVolume.ToString());
                double cur = Convert.ToDouble(Inventory.CurrentVolume.ToString());
                double per = 100 / (max / cur);

                return per;
            }
        }

        IMyTextPanel getLcdPanelFromGroup(IMyBlockGroup BlockGroup)
        {
            for (int i = 0; i < BlockGroup.Blocks.Count; i++)
            {
                if (BlockGroup.Blocks[i] is IMyTextPanel)
                {
                    return (BlockGroup.Blocks[i] as IMyTextPanel);
                }
            }

            return null;
        }

        bool isBlockGroup(IMyBlockGroup BlockGroup)
        {
            return (BlockGroup is IMyBlockGroup);
        }

        IMyBlockGroup getBlockGroupByName(String blockGroupName)
        {
            List<IMyBlockGroup> blockGroups = GridTerminalSystem.BlockGroups;
            for (int i = 0; i < blockGroups.Count; i++)
            {
                if (blockGroups[i].Name.IndexOf(blockGroupName) == 0)
                {
                    return blockGroups[i];
                }
            }
            return null;
        }

        void WritePublicTextToLcd(IMyTextPanel LcdPanel, String Text)
        {
            Text = LcdPanel.GetPublicTitle() + "\n" + Text;
            LcdPanel.ShowTextureOnScreen();
            LcdPanel.WritePublicText(Text);            
            LcdPanel.ShowPublicTextOnScreen();            
        }


// End InGame-Script
    }
}
