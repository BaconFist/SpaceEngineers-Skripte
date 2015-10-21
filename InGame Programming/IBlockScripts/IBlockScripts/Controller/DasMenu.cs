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
    public class DasMenu : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           DasMenu
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source: https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/DasMenu.cs
           
           Summary
           ------------------------------


           Abstract
           ------------------------------

           
           Example
           ------------------------------          

       */

        const char CONFIG_PATH_SEPERATOR = '/';
        StringBuilder exampleData = new StringBuilder();


        void Main(string args)
        {
            exampleData.AppendLine("Storage/Cargo MB 1");
            exampleData.AppendLine("Storage/Cargo MB 2");
            exampleData.AppendLine("System/Power/Small Reactor 3");

            MenuFactory MF = new MenuFactory(GridTerminalSystem);
            Item MenuItem = MF.createFromConfig(exampleData.ToString());

            MenuView view = new MenuView();

            Echo(view.getContent(MenuItem));
          
        }

        class MenuView
        {
            public string getContent(Item RootItem)
            {
                ItemRepository ItemRepository = new ItemRepository();
                List<Item> Items = RootItem.getChilds(); 

                StringBuilder content = new StringBuilder();
                for (int i_Items = 0; i_Items < Items.Count; i_Items++)
                {
                    Item Item = Items[i_Items];
                    content.AppendLine((new string('-', ItemRepository.countParents(Item))) + " " + Item.getLabel() + (Item.hasParent()? " (" + Item.getParent().getLabel() + ")":""));

                    if (Item.hasChilds())
                    {
                        content.Append(getContent(Item));
                    }
                }

                return content.ToString();
            }


        }

        class MenuFactory
        {
            public IMyGridTerminalSystem GridTerminalSystem;

            public MenuFactory(IMyGridTerminalSystem value)
            {
                GridTerminalSystem = value;
            }

            public Item createFromConfig(string data)
            {
                RootItem RootItem = (new ItemFactory()).createRootItem();
                parseConfig(data, RootItem);

                return RootItem;
            }

            private List<string> getLines(string data)
            {
                List<string> Lines = new List<string>();
                string[] rawLines = data.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                Lines.AddRange(rawLines);

                return Lines;
            }

            private List<string> getSegments(string data)
            {
                List<string> Segments = new List<string>();
                Segments.AddRange(data.Split(CONFIG_PATH_SEPERATOR));

                return Segments;
            }

            private void parseConfig(string data, RootItem RootItem)
            {
                List<string> dataLines = getLines(data);
                for(int i_dataLines = 0; i_dataLines < dataLines.Count; i_dataLines++)
                {
                    string dataLine = dataLines[i_dataLines];
                    parseLine(dataLine, RootItem);
                }
            }

            private void parseLine(string data, RootItem RootItem)
            {
                List<string> dataSegments = new List<string>();
                Item Parent = RootItem;
                dataSegments = getSegments(data);
                for (int i_dataSegments = 0; i_dataSegments < dataSegments.Count; i_dataSegments++)
                {
                    string dataSegment = dataSegments[i_dataSegments];
                    if (isLastOfCollection(dataSegments, i_dataSegments))
                    {
                        parseBlockItems(dataSegment, Parent);
                        Parent = RootItem;
                    }
                    else
                    {
                        Parent = parsePathItem(dataSegment, Parent);                        
                    }
                }
            }

            private PathItem parsePathItem(string data, Item Parent)
            {
                PathItem PathItem = (new ItemFactory()).createPathItem(data);
                Parent.addChild(PathItem);

                return PathItem;
            }

            private void parseBlockItems(string data, Item Parent)
            {
                List<IMyTerminalBlock> Blocks = getBlocksFromDataSegment(data);
                for (int i_Blocks = 0; i_Blocks < Blocks.Count; i_Blocks++)
                {
                    IMyTerminalBlock Block = Blocks[i_Blocks];
                    parseSingleBlockItem(Block, Parent);
                }
            }

            private void parseSingleBlockItem(IMyTerminalBlock Block, Item Parent)
            {
                BlockItem BlockItem = (new ItemFactory()).createBlockItem(Block);
                Parent.addChild(BlockItem);
                parseActions(BlockItem);
            }

            private void parseActions(BlockItem BlockItem)
            {
                List<ITerminalAction> Actions = getActions(BlockItem.getBlock());
                for(int i_Actions = 0; i_Actions < Actions.Count; i_Actions++)
                {
                    ITerminalAction Action = Actions[i_Actions];
                    parseSingleAction(Action, BlockItem);
                }                
            }

            private void parseSingleAction(ITerminalAction Action, Item Parent)
            {
                ActionItem ActionItem = (new ItemFactory()).createActionItem(Action);
                Parent.addChild(ActionItem);
            }

            private List<ITerminalAction> getActions(IMyTerminalBlock Block)
            {
                List<ITerminalAction> Actions = new List<ITerminalAction>();
                Block.GetActions(Actions);

                return Actions;
            }

            private List<IMyTerminalBlock> getBlocksFromDataSegment(string data)
            {
                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => (x as IMyTerminalBlock).CustomName.Contains(data)));

                return Blocks;
            }

            private bool isLastOfCollection(System.Collections.ICollection Collection, int index)
            {
                return (Collection.Count-1 == index);
            }
        }

        class ItemFactory
        {
            public ActionItem createActionItem(ITerminalAction Action)
            {
                ActionItem BuildItem = new ActionItem();
                BuildItem.setAction(Action);

                return BuildItem;
            }

            public BlockItem createBlockItem(IMyTerminalBlock Block)
            {
                BlockItem BuildItem = new BlockItem();
                BuildItem.setBlock(Block);

                return BuildItem;
            }

            public PathItem createPathItem(string label)
            {
                PathItem BuildItem = new PathItem();
                BuildItem.setLabel(label);

                return BuildItem;
            }

            public RootItem createRootItem()
            {
                RootItem BuildItem = new RootItem();
                return BuildItem;
            }

        }

        class ItemRepository
        {

            public List<Item> findAllByLabel(string label, Item root)
            {
                List<Item> Matches = new List<Item>();
                if (root.hasChilds())
                {
                    Matches = root.getChilds().FindAll(x => x.getLabel().Equals(label));
                    for(int i_Childs = 0; i_Childs < root.getChilds().Count; i_Childs++)
                    {
                        Matches.AddRange(findAllByLabel(label, root.getChilds()[i_Childs]));
                    }
                }

                return Matches;
            }

 
            public List<String> getPathFromItem(Item Item)
            {
                List<String> BuildPath = new List<String>();
                BuildPath.Add(Item.getLabel());
                if (Item.hasParent())
                {
                    BuildPath.AddRange(getPathFromItem(Item.getParent()));
                }
                return BuildPath;
            }

            public int countParents(Item Item)
            {
                int count = 0;
                if (Item.hasParent() && !(Item.getParent() is RootItem))
                {
                    count++;
                    count += countParents(Item.getParent());
                }

                return count;
            }
            
        }

        class Item
        {
            private string label;
            private Item Parent;
            private List<Item> Childs = new List<Item>();
                        
            public void setLabel(string value)
            {
                label = value;
            }

            public string getLabel()
            {
                return label;
            }

            public void setParent(Item value)
            {
                Parent = value;
            }

            public Item getParent()
            {
                return Parent;
            }

            public bool hasParent()
            {
                return (this.getParent() != null);
            }

            public void setChilds(List<Item> value)
            {
                Childs = value;
            }

            public List<Item> getChilds()
            {
                return Childs;
            }

            public void addChild(Item value)
            {
                if (!getChilds().Contains(value))
                {
                    getChilds().Add(value);
                    if (value.hasParent())
                    {
                        value.getParent().removeChild(value);
                    }
                    value.setParent(this);
                }
            }

            public void removeChild(Item value)
            {
                if (getChilds().Contains(value))
                {
                    getChilds().Remove(value);
                    value.setParent(null);
                }
            } 

            public bool hasChilds()
            {
                return (getChilds().Count > 0);
            }
              
        }

        class PathItem : Item
        {

        }

        class BlockItem : Item
        {
            private IMyTerminalBlock Block;

            public void setBlock(IMyTerminalBlock value)
            {
                Block = value;
                setLabel(getBlock().CustomName);
            }

            public IMyTerminalBlock getBlock()
            {
                return Block;
            }
        }

        class ActionItem : BlockItem
        {
            private ITerminalAction Action;
            
            public void setAction(ITerminalAction value)
            {
                Action = value;
                setLabel(getAction().Id);
            }     
            
            public ITerminalAction getAction()
            {
                return Action;
            }       
        }

        class RootItem : Item
        {

        }
                
        #endregion
    }
}
