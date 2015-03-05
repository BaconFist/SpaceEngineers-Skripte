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
    class DetailedCargoDisplay
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script

        void Main()
        {
            (new DetailedCargoDisplay()).run(GridTerminalSystem);
        }

        class DetailedCargoDisplay
        {

            //options
            const String textPanelNamesCSV = ";Textpanel DetailedCargoDisplay 1;Textpanel DetailedCargoDisplay 2";  // 1st char is seperator
            const Int16 inventorySelectionMode = 0; // 0: All; 1: By Group; 2: By Names; 3: By Substring in Name;
            const String inventorySelectionGroupNamesCSV = ";";  // 1st char is seperator
            const String inventorySelectionNamesCSV = ";";  // 1st char is seperator 
            const String inventorySelectionSubstringsCSV = ";";  // 1st char is seperator
            const Int16 textPanelMaxLines = 60;

            const Int16 ISM_ALL = 0;
            const Int16 ISM_GROUPS = 1;
            const Int16 ISM_NAMES = 2;
            const Int16 ISM_SUB = 3;

            IMyGridTerminalSystem GridTerminalSystem;

            public void run(IMyGridTerminalSystem _GridTerminalSystem)
            {
                GridTerminalSystem = _GridTerminalSystem;
                
                List<IMyTextPanel> textPanels = getTextPanelsFromCSV(textPanelNamesCSV);
                if (textPanels.Count > 0)
                {
                    List<IMyTerminalBlock> blocks = getCargoContainers();
                    if (blocks.Count > 0)
                    {
                        Dictionary<String, double> items = new Dictionary<string, double>();
                        List<String> keys = new List<string>();
                        for (int i_blocks = 0; i_blocks < blocks.Count; i_blocks++)
                        {
                            for (int i_inventory = 0; i_inventory < blocks[i_blocks].GetInventoryCount(); i_inventory++)
                            {
                                for (int i_item = 0; i_item < blocks[i_blocks].GetInventory(i_inventory).GetItems().Count; i_item++)
                                {
                                    IMyInventoryItem item = blocks[i_blocks].GetInventory(i_inventory).GetItems()[i_item];
                                    if (!items.ContainsKey(item.Content.SubtypeName))
                                    {
                                        items[item.Content.SubtypeName] = 0;
                                        keys.Add(item.Content.SubtypeName);
                                    }
                                    items[item.Content.SubtypeName] += Convert.ToDouble(item.Amount.ToString());
                                }
                            }
                        }
                        IMyTextPanel textPanel = textPanels[0];
                        Int16 i_textPanel = 0;
                        for (int i_key = 0; i_key < keys.Count; i_key++)
                        {
                            if (i_textPanel % (textPanelMaxLines - 1) == 0)
                            {
                                textPanel = textPanels[i_textPanel];
                                if (!(textPanel is IMyTextPanel))
                                {
                                    break;
                                }
                                textPanel.WritePublicText("Inventory Index - " + DateTime.Now.ToString(), false);
                                i_textPanel++;
                            }
                            textPanel.WritePublicText(keys[i_key] + ": " + String.Format("{0:N}", items[keys[i_key]]) + "\n");
                        }
                    }
                }
            }


            List<IMyTerminalBlock> getCargoContainers()
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                if (inventorySelectionMode == ISM_ALL)
                {
                    blocks = getInventoryBlocks(GridTerminalSystem.Blocks);
                }
                else if (inventorySelectionMode == ISM_GROUPS)
                {
                    String seperator = inventorySelectionGroupNamesCSV.Substring(0, 1);
                    String groupNames = inventorySelectionGroupNamesCSV + seperator;
                    List<IMyBlockGroup> groups = GridTerminalSystem.BlockGroups.FindAll(x => groupNames.Contains(seperator + x.Name + seperator));
                    for (int i = 0; i < groups.Count; i++)
                    {
                        blocks.AddList<IMyTerminalBlock>(getInventoryBlocks(groups[i].Blocks));
                    }
                }
                else if (inventorySelectionMode == ISM_NAMES)
                {
                    String seperator = inventorySelectionNamesCSV.Substring(0, 1);
                    String names = inventorySelectionNamesCSV + seperator;
                    blocks = GridTerminalSystem.Blocks.FindAll(x => (inventorySelectionNamesCSV.Contains(seperator + x.CustomName + seperator) && x.HasInventory()));
                }
                else if (inventorySelectionMode == ISM_SUB)
                {
                    List<String> subs = readCSV(inventorySelectionSubstringsCSV);
                    for (int i_subs = 0; i_subs < subs.Count; i_subs++)
                    {
                        blocks.AddList<IMyTerminalBlock>(GridTerminalSystem.Blocks.FindAll(x => (x.CustomName.Contains(subs[i_subs]) && x.HasInventory())));
                    }
                }

                return blocks;
            }

            List<IMyTerminalBlock> getInventoryBlocks(List<IMyTerminalBlock> list)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                for (int i = 0; i < list.Count; i++)
                {
                    IMyTerminalBlock block = list[i];
                    if (block.HasInventory())
                    {
                        blocks.Add(block);
                    }
                }
                return blocks;
            }



            List<String> readCSV(String csv)
            {
                String seperator = csv.Substring(0, 1);
                return csv.Split(new String[] { seperator }, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
            }

            List<IMyTextPanel> getTextPanelsFromCSV(String csv)
            {
                List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
                List<String> textPanelNames = readCSV(textPanelNamesCSV);

                for (int i = 0; i < textPanelNames.Count; i++)
                {
                    IMyTextPanel tempTextPanel = (GridTerminalSystem.GetBlockWithName(textPanelNames[i]) as IMyTextPanel);
                    if (tempTextPanel is IMyTextPanel)
                    {
                        textPanels.Add(tempTextPanel);
                    }
                }

                return textPanels;
            }
        }
        // End InGame-Script
    }
}
