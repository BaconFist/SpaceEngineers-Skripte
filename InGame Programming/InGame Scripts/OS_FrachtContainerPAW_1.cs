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
    class OS_FrachtContainerPAW_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            (new DetailedCargoDisplay()).run(GridTerminalSystem, "LCD FrachtContainer 1", ";FrachtContainer 1", "Lagerstand FrachtContainer 1");
            (new DetailedCargoDisplay()).run(GridTerminalSystem, "LCD FrachtContainer 2", ";FrachtContainer 2", "Lagerstand FrachtContainer 2");
        }

        class DetailedCargoDisplay
        {

            //options
            public String textPanelName = "LCD FrachtContainer 1";
            public Int16 inventorySelectionMode = 3; // 0: All; 1: By Group; 2: By Names; 3: By Substring in Name;
            public String inventorySelectionGroupNamesCSV = ";";  // 1st char is seperator
            public String inventorySelectionNamesCSV = ";FrachtContainer 1";  // 1st char is seperator 
            public String inventorySelectionSubstringsCSV = ";";  // 1st char is seperator
            public Int16 textPanelMaxLines = 14;
            public Int16 textPanelMaxChars = 70;
            public String inventoryIndexTitle = "Lagerstand";

            public Int16 ISM_ALL = 0;
            public Int16 ISM_GROUPS = 1;
            public Int16 ISM_NAMES = 2;
            public Int16 ISM_SUB = 3;

            IMyGridTerminalSystem GridTerminalSystem;

            public void run(IMyGridTerminalSystem _GridTerminalSystem, String _textPanelName, String _inventorySelectionSubstringsCSV, String _inventoryIndexTitle)
            {
                GridTerminalSystem = _GridTerminalSystem;
                textPanelName = _textPanelName;
                inventorySelectionSubstringsCSV = _inventorySelectionSubstringsCSV;
                inventoryIndexTitle = _inventoryIndexTitle;

                IMyTextPanel textPanel = (GridTerminalSystem.GetBlockWithName(textPanelName) as IMyTextPanel);
                if (textPanel is IMyTextPanel)
                {
                    List<IMyTerminalBlock> blocks = getCargoContainers();
                    if (blocks.Count > 0)
                    {
                        double maxVol = 0;
                        double curVol = 0;
                        Dictionary<String, double> items = new Dictionary<string, double>();
                        List<String> keys = new List<string>();
                        for (int i_blocks = 0; i_blocks < blocks.Count; i_blocks++)
                        {
                            for (int i_inventory = 0; i_inventory < blocks[i_blocks].GetInventoryCount(); i_inventory++)
                            {
                                IMyInventory inventory = blocks[i_blocks].GetInventory(i_inventory);
                                maxVol += (Convert.ToDouble(inventory.MaxVolume.ToString()) * 1000);
                                curVol += (Convert.ToDouble(inventory.CurrentVolume.ToString()) * 1000);
                                for (int i_item = 0; i_item < blocks[i_blocks].GetInventory(i_inventory).GetItems().Count; i_item++)
                                {
                                    IMyInventoryItem item = inventory.GetItems()[i_item];
                                    if (!items.ContainsKey(item.Content.SubtypeName))
                                    {
                                        items.Add(item.Content.SubtypeName, 0);
                                        keys.Add(item.Content.SubtypeName);
                                    }
                                    double amount = items[item.Content.SubtypeName];
                                    items.Remove(item.Content.SubtypeName);
                                    items.Add(item.Content.SubtypeName, amount + Convert.ToDouble(item.Amount.ToString()));
                                }
                            }
                        }

                        textPanel.WritePublicText(inventoryIndexTitle + " - " + DateTime.Now.ToString() + "\n", false);
                        textPanel.WritePublicText("Volumen: " + String.Format("{0:N2}", curVol) + " / " + String.Format("{0:N2}", maxVol) + " L - " + String.Format("{0:N2}", getPecent(maxVol, curVol)) + "%\n", true);

                        String lines = "Items:";
                        for (int i_key = 0; i_key < keys.Count; i_key++)
                        {
                            lines += " [" + keys[i_key] + ":" + String.Format("{0:N0}", Math.Round(items[keys[i_key]], 0)) + "]";
                        }
                        List<String> linesWrapped = WordWrap(lines, textPanelMaxChars);
                        for (int i = 0; i < linesWrapped.Count; i++)
                        {
                            textPanel.WritePublicText(linesWrapped[i] + "\n", true);
                        }
                    }
                }
            }

            double getPecent(double max, double val)
            {
                if (val == 0)
                {
                    return 0;
                }
                else if (val == max)
                {
                    return 100;
                }
                else
                {
                    return (100 * (val / max));
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
                    String[] subs = readCSV(inventorySelectionSubstringsCSV);
                    for (int i_subs = 0; i_subs < subs.Length; i_subs++)
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



            String[] readCSV(String csv)
            {
                String seperator = csv.Substring(0, 1);
                return csv.Split(new String[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
            }

            // WordWrap based on http://bryan.reynoldslive.com/post/Wrapping-string-data.aspx
            public List<String> WordWrap(string text, int maxLength)
            {
                return WordWrap(text, maxLength, ' ');
            }

            public List<String> WordWrap(string text, int maxLength, char wordSplitChar)
            {
                String[] words = text.Split(wordSplitChar);
                List<String> lines = new List<String>();
                String currentLine = "";

                for (int i = 0; i < words.Length; i++)
                {
                    String currentWord = words[i];
                    if ((currentLine.Length > maxLength) || ((currentLine.Length + currentWord.Length) > maxLength))
                    {
                        lines.Add(currentLine);
                        currentLine = "";
                    }
                    if (currentLine.Length > 0)
                    {
                        currentLine += wordSplitChar + currentWord;
                    }
                    else
                    {
                        currentLine += currentWord;
                    }
                }
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                }

                return lines;
            }
        }
        // End InGame-Script
    }
}
