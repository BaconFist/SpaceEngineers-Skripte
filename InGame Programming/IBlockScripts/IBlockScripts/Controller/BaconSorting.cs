using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class BaconSorting : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BaconSorting
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           simple sorting using #tags

           Abstract
           ------------------------------
            Tags:
             - Grouped Types: ammo, component, bottle, ingot, ore, handtool
             - Specific Types:  the items SubTypeID (mostly the english name without spaces)
            // case doesn't matter

            How it works:
            * Add any tag to a Container with an Inventory to pull items of this type in it.
            * Script is lazy, it will sort a item only if it is not in a Container with a matching tag. (Example: It will not pull an SteelPlate from an container called "Large Cargo Container #component")
            * Source & Target Block must match some requirements: 1. Same Grid as PB, 2. UseConveyorSystem must be ON, 3. Must be connected throug Conveyor-System, 4. Must be Enabled aka turned ON
            
            
            Known Bugs:
            * not working with Hydrogen Tanks. (will fix this as soon as i know why.)
                                
           Example
           ------------------------------
            Blocks:
            "Cargo 1 #ingot #ammo", "Cargo 2 #component", Cargo 3"
            This will try to sort all Ingots and ammo in Cargo 1 and all components in Cargo 2.
          
       */
        //--- CONFIG START
        private string TAG_LOG_DEBUG_LCD = "!debug!"; //Debug/Log-data will be dispalyed on all LCDs with this tag in name
        private int usedLogLevel = LOG_LVL_SILENT; //must be one of LOG_LVL_*
        //--- CONFIG END

        private const int LOG_LVL_DEBUG = 2;
        private const int LOG_LVL_INFO = 1;
        private const int LOG_LVL_SILENT = 0;

        private Dictionary<string, string> typeMap = new Dictionary<string, string>();
        
        void Main(string args)
        {
            boot();
            List<IMyTerminalBlock> Container = getAllCargo();
            debug(" Found " + Container.Count.ToString() + " Source Blocks.");
            Dictionary<string, List<IMyTerminalBlock>> targets = getTargets(Container);
            debug(" Found " + targets.Count.ToString() + " Target Blocks.");
            for (int i_Container = 0; i_Container < Container.Count; i_Container++)
            {
                debug("     Start Sorting " + (i_Container+1).ToString() + "/" + Container.Count.ToString() + ": " + Container[i_Container].CustomName);
                doSortContainer(Container[i_Container], targets);
                debug("     End Sorting " + (i_Container+1).ToString() + "/" + Container.Count.ToString() + ": " + Container[i_Container].CustomName);
            }
        }

        private void doSortContainer(IMyTerminalBlock Block, Dictionary<string, List<IMyTerminalBlock>> targets)
        {
            debug("         Inventory Count: " + Block.GetInventoryCount().ToString());
            for (int i_Inventory = 0; i_Inventory < Block.GetInventoryCount(); i_Inventory++)
            {
                debug("             Start Sorting Inventory " + (i_Inventory+1).ToString() + " of " + Block.CustomName);
                doSortInventory(Block, Block.GetInventory(i_Inventory), targets);
                debug("             End Sorting Inventory " + (i_Inventory + 1).ToString() + " of " + Block.CustomName);
            }
        }

        private void doSortInventory(IMyTerminalBlock Block, IMyInventory Inventory, Dictionary<string, List<IMyTerminalBlock>> targets)
        {
            debug("                 Item Count: " + Inventory.GetItems().Count.ToString());
            for (int i_Item = Inventory.GetItems().Count-1; i_Item >= 0; i_Item--)
            {
                IMyInventoryItem Item = Inventory.GetItems()[i_Item];
                debug("                     Start Sorting Item " + (i_Item+1).ToString()+"/" + Inventory.GetItems().Count.ToString() + ": " + Item.ToString());
                doSortItem(Block, Inventory, Item, targets);
                debug("                     End Sorting Item " + (i_Item + 1).ToString() + "/" + Inventory.GetItems().Count.ToString() + ": " + Item.ToString());
            }
        }

        private void doSortItem(IMyTerminalBlock Block, IMyInventory Inventory, IMyInventoryItem Item, Dictionary<string, List<IMyTerminalBlock>> targets)
        {
            string TypeName = getSimpleType(Item.Content.TypeId.ToString());
            string SubTypeName = Item.Content.SubtypeName.ToLower();
            debug("                         Itemtype: " + TypeName + "/" + SubTypeName);
            bool isInRightInventory = hasContainerTag(TypeName, SubTypeName, Block);
            debug("                         Item is" + (isInRightInventory?" ":" not ") + "in matching Inventory.");
            if (!isInRightInventory)
            {
                List<IMyTerminalBlock> TargetBlocks = getTargetsByItem(new string[] {TypeName, SubTypeName }, targets);
                debug("                         Found " + TargetBlocks.Count.ToString() + " Target Blocks.");
                tryMoveItem(Block ,Inventory, TargetBlocks, Item);
            }
        }

        private void tryMoveItem(IMyTerminalBlock Parent, IMyInventory SourceInventory, List<IMyTerminalBlock> TargetBlocks, IMyInventoryItem Item)
        {
            bool isPending = true;
            for (int i = 0; i < TargetBlocks.Count && isPending; i++)
            {
                debug("                             Target Block ("+(i+1).ToString()+") " + TargetBlocks[i].CustomName + " has " + TargetBlocks[i].GetInventoryCount().ToString() + " inventories");
                for (int inv = 0; inv < TargetBlocks[i].GetInventoryCount() && isPending; inv++)
                {
                    IMyInventory TargetInventory = TargetBlocks[i].GetInventory(inv);
                    bool connected = SourceInventory.IsConnectedTo(TargetInventory);
                    debug("                             Inventory is" + (connected ? " " : " not ") + " connected.");
                    if (connected)
                    {
                        isPending = !SourceInventory.TransferItemTo(TargetInventory, SourceInventory.GetItems().IndexOf(Item), null, true);
                        debug("                             Try transfering \""+Item.ToString()+"\" from \""+Parent.CustomName+"\" to \""+TargetBlocks[i].CustomName+"\" => " + (isPending ? "still pending." : "done."), LOG_LVL_INFO);
                    }
                }
            }
        }

        private List<IMyTerminalBlock> getTargetsByItem(string[] tags, Dictionary<string, List<IMyTerminalBlock>> targets)
        {
            debug("                             Looking for Targets (" + tags.Length.ToString() + " Tags)");
            List<IMyTerminalBlock> targetInventories = new List<IMyTerminalBlock>();
            for(int i = 0; i < tags.Length; i++)
            {
                bool contains = targets.ContainsKey(tags[i]);
                debug("                                 Tag " + tags[i] + " is"+(contains?" ":" not ")+"available as target");
                if (contains)
                {
                    for(int r = 0; r < targets[tags[i]].Count; r++)
                    {
                        targetInventories.Add(targets[tags[i]][r]);
                        debug("                                     Target > " + ((targets[tags[i]][r] is IMyTerminalBlock) ? targets[tags[i]][r].CustomName : "Is not a TerminalBlock !"));
                    }                    
                }
            }

            return targetInventories;
        }

        private bool hasContainerTag(string Type, string SubType, IMyTerminalBlock Block)
        {
            return Block.CustomName.Contains("#" + Type) || Block.CustomName.Contains("#" + SubType);
        }

        private void boot()
        {
            clearDebug();
            buildTypeMap();
        }
        
        private List<IMyTerminalBlock> getAllCargo()
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();            
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => x.CubeGrid.Equals(Me.CubeGrid) && (x.GetInventoryCount() > 0) && x.IsWorking && x.GetUseConveyorSystem()));

            return Blocks;
        }

        private Dictionary<string, List<IMyTerminalBlock>> getTargets(List<IMyTerminalBlock> Container)
        {
            Dictionary<string, List<IMyTerminalBlock>> targets = new Dictionary<string, List<IMyTerminalBlock>>();
            for(int i_Container = 0; i_Container < Container.Count; i_Container++)
            {
                IMyTerminalBlock Block = Container[i_Container];
                List<string> tags = getTags(Block.CustomName);
                debug("     Found " + tags.Count.ToString() + " Tags ["+String.Join(", ", tags)+"] in " + Block.CustomName);
                for (int i = 0; i < tags.Count; i++)
                {
                    string tTag = tags[i];
                    if (!targets.ContainsKey(tTag))
                    {
                        targets.Add(tTag, new List<IMyTerminalBlock>());
                        debug("         Create List for " + tTag);
                    }
                    if (!targets[tTag].Contains(Block))
                    {
                        targets[tTag].Add(Block);
                        debug("         Add " + Block.CustomName + " to List for " + tTag);
                    } else
                    {
                        debug("         " + Block.CustomName + " is already in List for " + tTag);
                    }
                }
            }

            return targets;
        }

        private List<string> getTags(string name)
        {
            List<string> tags = new List<string>();
            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(name, @"(#\S+)");

            for(int i = 0; i < matches.Count; i++)
            {
                string tag = name.Substring(matches[i].Index + 1, matches[i].Length -1).ToLower();
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }

            return tags;            
        }
        
        private void buildTypeMap()
        {
            typeMap = new Dictionary<string, string>();
            typeMap.Add("MyObjectBuilder_AmmoMagazine", "ammo");
            typeMap.Add("MyObjectBuilder_Component", "component");
            typeMap.Add("MyObjectBuilder_GasContainerObject", "bottle");
            typeMap.Add("MyObjectBuilder_OxygenContainerObject", "bottle");            
            typeMap.Add("MyObjectBuilder_Ingot", "ingot");
            typeMap.Add("MyObjectBuilder_Ore", "ore");
            typeMap.Add("MyObjectBuilder_PhysicalGunObject", "handtool");
        }

        private string getSimpleType(string objectName)
        {
            string response = objectName;
            if (typeMap.ContainsKey(objectName))
            {
                response = typeMap[objectName];
            }
            return response;
        }

        private int identLevel = 0;

        private void clearDebug()
        {
            if (usedLogLevel != LOG_LVL_SILENT)
            {
                List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LCDs, (x => x.CustomName.Contains(TAG_LOG_DEBUG_LCD) && x.CubeGrid.Equals(Me.CubeGrid)));

                for (int i = 0; i < LCDs.Count; i++)
                {
                    IMyTextPanel LCD = LCDs[i] as IMyTextPanel;
                    LCD.WritePublicText("");
                }
            }
        }

        private void debug(string line, int logLevel = LOG_LVL_DEBUG)
        {
            if (usedLogLevel != LOG_LVL_SILENT)
            {
                if (usedLogLevel >= logLevel)
                {
                    string prefix = "";
                    if (usedLogLevel >= LOG_LVL_DEBUG)
                    {
                        prefix = "[Tick#" + this.ElapsedTime.Ticks.ToString() + "]";
                    }

                    line = prefix + line;

                    List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LCDs, (x => x.CustomName.Contains(TAG_LOG_DEBUG_LCD) && x.CubeGrid.Equals(Me.CubeGrid)));

                    for (int i = 0; i < LCDs.Count; i++)
                    {
                        IMyTextPanel LCD = LCDs[i] as IMyTextPanel;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(line);
                        sb.Append(LCD.GetPublicText());
                        LCD.WritePublicText(sb.ToString());
                    }
                }
            }
        }

        #endregion
    }
}
