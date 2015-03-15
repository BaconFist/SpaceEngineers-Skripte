﻿using System;
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
            const String textPanelName = "Textpanel";  // 1st char is seperator
            const Int16 inventorySelectionMode = 0; // 0: All; 1: By Group; 2: By Names; 3: By Substring in Name;
            const String inventorySelectionGroupNamesCSV = ";";  // 1st char is seperator
            const String inventorySelectionNamesCSV = ";";  // 1st char is seperator 
            const String inventorySelectionSubstringsCSV = ";";  // 1st char is seperator
            const Int16 textPanelMaxLines = 44;
            const String inventoryIndexTitle = "Lagerstand";

            const Int16 ISM_ALL = 0;
            const Int16 ISM_GROUPS = 1;
            const Int16 ISM_NAMES = 2;
            const Int16 ISM_SUB = 3;

            IMyGridTerminalSystem GridTerminalSystem;

            public void run(IMyGridTerminalSystem _GridTerminalSystem)
            {
                GridTerminalSystem = _GridTerminalSystem;

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
                                for (int i_item = 0; i_item < blocks[i_blocks].GetInventory(i_inventory).GetItems().Count; i_item++)
                                {
                                    IMyInventory inventory = blocks[i_blocks].GetInventory(i_inventory);
                                    maxVol += Convert.ToDouble(inventory.MaxVolume.ToString());
                                    curVol += Convert.ToDouble(inventory.CurrentVolume.ToString());
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
                        textPanel.WritePublicText(String.Format("{0:N0}", curVol) + "/" + String.Format("{0:N0}", maxVol) + "L - " + getPecent(maxVol, curVol).ToString() + "%\n", true);
                        
                        String lines = "Items:";
                        for (int i_key = 0; i_key < keys.Count; i_key++)
                        {
                            lines += " [" + keys[i_key] + ":" + String.Format("{0:N0}", Math.Round(items[keys[i_key]], 0)) + "]";
                        }
                        List<String> linesWrapped = WordWrap(lines, textPanelMaxLines);
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
                    return Convert.ToInt32(Math.Round(100 * (val / max), 0));
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
