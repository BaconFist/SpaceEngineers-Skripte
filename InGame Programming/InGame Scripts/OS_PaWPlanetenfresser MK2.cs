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
    class OS_PaWPlanetenfresser_MK2
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
            String textPanelName = "LCD-Schirm"; 
            Int16 inventorySelectionMode = 3; // 0: All; 1: By Group; 2: By Names; 3: By Substring in Name;
            String inventorySelectionGroupNamesCSV = ";";  // 1st char is seperator
            String inventorySelectionNamesCSV = ";";  // 1st char is seperator 
            String inventorySelectionSubstringsCSV = ";Drill;Fracht;Verbinder";  // 1st char is seperator
            Int16 textPanelMaxLines = 22;
            Int16 textPanelMaxChars = 90;
            String inventoryIndexTitle = "Lagerstand";

            Int16 ISM_ALL = 0;
            Int16 ISM_GROUPS = 1;
            Int16 ISM_NAMES = 2;
            Int16 ISM_SUB = 3;

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
                        StringBuilder lines = new StringBuilder();

                        lines.AppendLine(inventoryIndexTitle + " - " + DateTime.Now.ToString());
                        lines.AppendLine("Volumen: " + String.Format("{0:N2}", curVol) + " / " + String.Format("{0:N2}", maxVol) + " L - " + String.Format("{0:N2}", getPecent(maxVol, curVol)) + "%");
                        for (int i_key = 0; i_key < keys.Count; i_key++)
                        {
                            lines.AppendLine("[" + keys[i_key] + ":" + String.Format("{0:N0}", Math.Round(items[keys[i_key]], 0)) + "]");
                        }
                       String linesWrapped = wordWrap(lines.ToString(), textPanelMaxChars, true);
                       textPanel.WritePublicText(linesWrapped, false);
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

            /*
        * WTF?
        * ====
        * automatic word-wrap
        * 
        * > text = text to be wrapped
        * > lineWidth = max characters per line
        * > keepLineBreaks =   true-> keep existing linebreaks, false-> remove linebreakes and repalce with space
        */
            public string wordWrap(string text, int lineWidth, bool keepLineBreaks = true)
            {
                int computedLineWidth = lineWidth - 2;
                Char[] trimChars = new Char[] { ' ', '\n' };
                StringBuilder wrapped = new StringBuilder();
                if (keepLineBreaks == false)
                {
                    text = text.Replace('\n', ' ');
                }

                int loopLimit = text.Length;

                text = text.Trim(trimChars);
                for (int i = 0; (text.Length > 0) && i < loopLimit; i++)
                {
                    int maxChars = (computedLineWidth < text.Length) ? computedLineWidth : text.Length;
                    int count = text.LastIndexOf(' ', maxChars - 1);
                    if (keepLineBreaks == true)
                    {
                        int newLine = text.IndexOf('\n');
                        count = (newLine != -1 && newLine < count) ? newLine : count;
                    }
                    count = (count == -1) ? maxChars : count;
                    wrapped.AppendLine(" " + text.Substring(0, count).Trim(trimChars) + " ");
                    text = text.Remove(0, count).Trim(trimChars);
                }

                return wrapped.ToString();
            }
        }
        // End InGame-Script
    }
}
