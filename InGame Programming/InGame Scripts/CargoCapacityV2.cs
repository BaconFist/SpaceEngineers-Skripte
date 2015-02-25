using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class LeInventory : IMyInventory {}
    public class LeCockpit : IMyCockpit {}
    public class LEReaktor : IMyReactor {}
    

    public class CargoCapacityV2
    {

        IMyGridTerminalSystem GridTerminalSystem;
//InGame Script BEGIN        
        List<String> Groups = new List<String>();

        void Main()
        {
            Groups.Add("Lageranzeige Lager 1");

            doDisplay();
        }

        void doDisplay()
        {
            BMyInventoryCollectionShow show;
            IMyBlockGroup BlockGroup;
            IMyTerminalBlock display;
            for (int i = 0; i < Groups.Count; i++)
            {
                    BlockGroup = getGroup(Groups[i]);
                    if ((BlockGroup as IMyBlockGroup) != null)
                    {
                        display = getDisplayBlock(BlockGroup);
                        if (isDisplayBlock(display))
                        {
                            display.SetCustomName(BlockGroup.Name);
                            show = new BMyInventoryCollectionShow(new BMyInventoryCollection(BlockGroup));
                            display.SetCustomName(show.getGeneratedOutput());
                        } 
                    }
            }            
        }


        public class BMyInventoryInfo
        {
            public double maxVolume = 0;
            public double currentVolume = 0;
            public string customName = "";

            public void setMaxVolume(double _maxVolume)
            {
                maxVolume = _maxVolume;
            }

            public double getMaxVolume()
            {
                return maxVolume;
            }

            public void setCurrentVolume(double _currentVolume)
            {
                currentVolume = _currentVolume;
            }

            public double getCurrentVolume()
            {
                return currentVolume;
            }

            public void setCustomName(string _customName)
            {
                customName = _customName;
            }

            public bool isMaxVolumeSet()
            {
                return this.maxVolume > 0;
            }

            public bool isFull(bool approximately = true)
            {
                if (approximately)
                {
                    return getFillLevelInPercent() > 99;
                }
                else
                {
                    return getCurrentVolume() == getMaxVolume(); 
                }

            }

            public double getFillLevelInPercent(int decimals = 0){
                if(isMaxVolumeSet() && getCurrentVolume() > 0){
                    return Math.Round(100 * (getCurrentVolume() / getMaxVolume()), decimals);
                }
                return 0;
            }                        
        }

        public class BMyInventory
        {
            public BMyInventoryInfo incomingInventory = null;
            public BMyInventoryInfo outgoingInventory = null;
            public IMyTerminalBlock container = null;

            public void setContainer(IMyTerminalBlock _container)
            {
                container = _container;
            }

            public IMyTerminalBlock getContainer()
            {
                return container;
            }

            public bool hasContainer()
            {
                return (container as IMyTerminalBlock) != null;  
            }

            public void setIncomingInventory(BMyInventoryInfo _incomingInventory)
            {
                incomingInventory = _incomingInventory;
            }

            public BMyInventoryInfo getIncomingInventory()
            {
                return incomingInventory;
            }

            public bool hasIncomingInventory()
            {
                return (incomingInventory as BMyInventoryInfo) != null;
            }

            public void setOutgoingInventory(BMyInventoryInfo _outgoingInventory)
            {
                outgoingInventory = _outgoingInventory;
            }

            public BMyInventoryInfo getOutgoingInventory()
            {
                return outgoingInventory;
            }

            public bool hasOutgoingInventory()
            {
                return (outgoingInventory as BMyInventoryInfo) != null;
            }

            public int inventoryCount()
            {
                int count = 0;
                if (hasIncomingInventory())
                {
                    count++;
                }
                if (hasOutgoingInventory())
                {
                    count++;
                }

                return count;
            }
        }

        public class BMyInventoryFactory
        {
            public BMyInventory buildInventoryFromBlock(IMyTerminalBlock block)
            {
                BMyInventory inventoryRecord = new BMyInventory();
                inventoryRecord.setContainer(block);
                inventoryRecord.setIncomingInventory(buildInventoryInfo(getIncominginventoryFromBlock(block), block));
                inventoryRecord.setOutgoingInventory(buildInventoryInfo(getOutgoingInventoryFromBlock(block), block));
                
                return inventoryRecord;
            }

            public IMyInventory getIncominginventoryFromBlock(IMyTerminalBlock block)
            {
                return getInventoryById(block, 0);
            }

            public IMyInventory getOutgoingInventoryFromBlock(IMyTerminalBlock block)
            {
                return getInventoryById(block, 1);
            }

            public IMyInventory getInventoryById(IMyTerminalBlock block, int id)
            {
                IMyInventoryOwner inventoryOwner = (block as IMyInventoryOwner);
                if (id > 0 && inventoryOwner.GetInventory(0).Equals(inventoryOwner.GetInventory(id)))
                {
                    return null;
                }
                return inventoryOwner.GetInventory(id);
            }

            public BMyInventoryInfo buildInventoryInfo(IMyInventory inventory, IMyTerminalBlock block){
                BMyInventoryInfo info = new BMyInventoryInfo();
                info.setCustomName(block.CustomName);
                info.setCurrentVolume(inventory.CurrentVolume.RawValue);
                info.setMaxVolume(inventory.MaxVolume.RawValue);

                return info;
            }

        }

        public class BMyInventoryCollection
        {
            List<BMyInventory> inventories = new List<BMyInventory>();
            BMyInventoryFactory factory = new BMyInventoryFactory();
            String customName = "";

            public BMyInventoryCollection(IMyBlockGroup group)
            {
                setCutomName(group.Name);
                addContainersFromList(group.Blocks);
            }

            public void setCutomName(String _customName)
            {
                customName = _customName;
            }

            public string getCustomName()
            {
                return customName;
            }

            public void addContainersFromList(List<IMyTerminalBlock> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    addContainerToList(list[i]);
                }
            }

            public void addContainerToList(IMyTerminalBlock block)
            {
                BMyInventory inventory = factory.buildInventoryFromBlock(block);
                if (inventory.inventoryCount() > 0)
                {
                    inventories.Add(inventory);
                }                
            }
                               
            public List<BMyInventory> getInventories()
            {
                return inventories;
            }                        
        }

        public class BMyInventoryCollectionShow
        {
            public BMyInventoryCollection inventoryCollection;
            public String templateIsFull = " [Full]";
            public String template = "{GroupName}\r\n-------------------------------------{Items}";
            public String itemTemplateSingle = "\r\n{CustomName}: {Percent}% [{curVol}/{maxVol}L]{IsFULL}";
            public String itemTemplateIncoming = "\r\n{CustomName}: [IN] {Percent}% [{curVol}/{maxVol}L]{IsFULL}";
            public String itemTemplateOutgoing = " [OUT] {Percent}% [{curVol}/{maxVol}L]{IsFULL}";

            public BMyInventoryCollectionShow(BMyInventoryCollection _inventoryCollection)
            {
                inventoryCollection = _inventoryCollection;
            }

            public void setTemplateIsFull(String _template)
            {
                templateIsFull = _template;
            }

            public String getTemplateIsFull()
            {
                return templateIsFull;
            }

            public void setTemplate(String _template)
            {
                template = _template;
            }

            public void setItemTemplateSingle(String _template)
            {
                itemTemplateSingle = _template;
            }

            public void setItemTemplateIncoming(String _template)
            {
                itemTemplateIncoming = _template;
            }

            public void setItemTemplateOutgoing(String _template)
            {
                itemTemplateOutgoing = _template;
            }

            public String getTemplate()
            {
                return template;
            }

            public String getItemTemplateSingle()
            {
                return itemTemplateSingle;
            }

            public String getItemTemplateIncoming()
            {
                return itemTemplateIncoming;
            }

            public String getItemTemplateOutgoing()
            {
                return itemTemplateOutgoing;
            }
            
            public String getWellFormedPercent(double _percent)
            {
                if (_percent < 1 && _percent != 0)
                {
                    return "< 1";
                }
                if (_percent > 99 && _percent < 100)
                {
                    return "> 99";
                }

                return String.Format("{0:N0}", _percent);
            }

            public String getGeneratedOutput()
            {
                String items = "", output = "";
                List<BMyInventory> inventories = inventoryCollection.getInventories();
                for (int i = 0; i < inventories.Count; i++)
                {
                    if(inventories[i].inventoryCount() > 0){
                        if (inventories[i].hasOutgoingInventory())
                        {
                            items = items + replace(getItemTemplateIncoming(),
                                new String[] { "{CustomName}", "{Percent}", "{curVol}", "{maxVol}", "{IsFULL}" },
                                new String[] { 
                                        inventories[i].getContainer().CustomName, 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getFillLevelInPercent()), 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getCurrentVolume()), 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getMaxVolume()),
                                        inventories[i].getIncomingInventory().isFull(true)?getTemplateIsFull():""
                                    }
                                );
                            items = items + replace(getItemTemplateOutgoing(),
                                new String[] { "{CustomName}", "{Percent}", "{curVol}", "{maxVol}", "{IsFULL}" },
                                new String[] { 
                                        inventories[i].getContainer().CustomName, 
                                        String.Format("{0:N0}", inventories[i].getOutgoingInventory().getFillLevelInPercent()), 
                                        String.Format("{0:N0}", inventories[i].getOutgoingInventory().getCurrentVolume()), 
                                        String.Format("{0:N0}", inventories[i].getOutgoingInventory().getMaxVolume()),
                                        inventories[i].getOutgoingInventory().isFull(true)?getTemplateIsFull():"" 
                                    }
                                );
                        }
                        else
                        {
                            items = items + replace(getItemTemplateSingle(),
                                new String[] { "{CustomName}", "{Percent}", "{curVol}", "{maxVol}", "{IsFULL}" },
                                new String[] { 
                                        inventories[i].getContainer().CustomName, 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getFillLevelInPercent()), 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getCurrentVolume()), 
                                        String.Format("{0:N0}", inventories[i].getIncomingInventory().getMaxVolume()),
                                        inventories[i].getIncomingInventory().isFull(true)?getTemplateIsFull():"" 
                                    }
                                );
                        }
                    }                    
                }
                output = replace(getItemTemplateSingle(),
                                new String[] { "{GroupName}", "{Items}" },
                                new String[] { 
                                        inventoryCollection.getCustomName(),
                                        items
                                    }
                                );

                return output;
            }

            public String replace(String source, String[] oldVal, String[] newVal)
            {
                if (oldVal.Length == newVal.Length)
                {
                    for (int i = 0; i < oldVal.Length; i++)
                    {
                        source = source.Replace(oldVal[i], newVal[i]);
                    }
                }

                return source;
            }

        }

        //MicroFramework BEGIN  
        IMyBlockGroup getGroup(String name)
        {
            List<IMyBlockGroup> blockGroups = GridTerminalSystem.BlockGroups;
            for (int i = 0; i < blockGroups.Count; i++)
            {
                if (blockGroups[i].Name.IndexOf(name) == 0)
                {
                    return blockGroups[i];
                }
            }
            return null;
        }

        bool isDisplayBlock(IMyTerminalBlock block)
        {
            return isRadioAntenna(block) || isBeacon(block);
        }

        IMyTerminalBlock getDisplayBlock(IMyBlockGroup group)
        {
            IMyTerminalBlock block;
            block = getDisplayAntenna(group);
            if (isRadioAntenna(block))
            {
                block = getDisplayBeacon(group);
            }

            return block;
        }

        bool isRadioAntenna(IMyTerminalBlock testBlock)
        {
            return (testBlock as IMyRadioAntenna) != null;
        }

        IMyRadioAntenna getDisplayAntenna(IMyBlockGroup group)
        {
            for (int i = 0; i < group.Blocks.Count; i++)
            {
                if (isRadioAntenna(group.Blocks[i]))
                {
                    return (group.Blocks[i] as IMyRadioAntenna);
                }
            }

            return null;
        }

        bool isBeacon(IMyTerminalBlock testBlock)
        {
            return (testBlock as IMyBeacon) != null;
        }

        IMyBeacon getDisplayBeacon(IMyBlockGroup group)
        {
            for (int i = 0; i < group.Blocks.Count; i++)
            {
                if (isBeacon(group.Blocks[i]))
                {
                    return (group.Blocks[i] as IMyBeacon);
                }
            }

            return null;
        }

        //MicroFramework END
//InGame Script BEGIN
    }
}
