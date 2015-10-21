﻿using System;
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

        const char ARGS_SPLIT = ';';
        const string ARGS_LCDNAME_FORMAT = "!DasMenu#{ID}!";

        const char CONFIG_PATH_SEPERATOR = '/';
        const string CONFIG_CMD_PREFIX = "#";


        void Main(string args)
        {
            DasMenuArgs MenuArgs = new DasMenuArgs(args, ARGS_SPLIT);
            IMyTextPanel LcdBlock = findLcd(MenuArgs.getLcdPattern());
            if(LcdBlock != null)
            {
                Echo("Found TextPanel: " + LcdBlock.ToString());
                DasMenuConfig Config = new DasMenuConfig(LcdBlock.GetPrivateText());
                DasMenuFactory MenuFactory = new DasMenuFactory(GridTerminalSystem);
                DasMenuItem MenuItem = MenuFactory.createFromConfig(Config);

                DasMenuView view = new DasMenuView();

                string content = view.getContent(MenuItem, Config);
                LcdBlock.WritePublicText(content);
            } else
            {
                Echo("Can't resolve TextPanel with Pattern \"" + MenuArgs.getLcdPattern() + "\"");
            }

        }

        public IMyTextPanel findLcd(string id)
        {
            string pattern = ARGS_LCDNAME_FORMAT.Replace("{ID}", id);
            IMyTextPanel LcdBlock = null;
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Blocks, (x => (x as IMyTerminalBlock).CustomName.Contains(pattern)));
            if (Blocks.Count > 0)
            {
                 LcdBlock = Blocks[0] as IMyTextPanel;
            }

            return LcdBlock;
        }

        class DasMenuArgs
        {
            private string LcdPattern;
            private string Command;

            public DasMenuArgs(string args, char splitchar)
            {
                string[] argv = args.Split(splitchar);
                if(argv.Length > 0)
                {
                    setLcdPattern(argv[0]);
                }
            }
            
            public void setLcdPattern(string value)
            {
                LcdPattern = value;
            }

            public string getLcdPattern()
            {
                return LcdPattern;
            }
        }

        class DasMenuConfig
        {
            private List<string> MenuItems = new List<string>();
            private string Title;
            private string Selector = "> ";
            private string Indent = "-";
            private string Format = "{INDENT}{INDENTSPACE}{LABEL}";

            const string CMD_TITLE = "Title";
            const string CMD_SELECTOR = "Selector";
            const string CMD_INDENT = "Indent";
            const string CMD_FORMAT = "Format";

            public const string FORMAT_INDENT = "{INDENT}";
            public const string FORMAT_INDENTSPACE = "{INDENTSPACE}";
            public const string FORMAT_LABEL = "{LABEL}";

            public DasMenuConfig(string configData)
            {
                parseConfig(configData);
            }

            public List<string> getMenuItems()
            {
                return MenuItems;
            }

            public void setTitle(string value)
            {
                Title = value;
            }

            public string getTitle()
            {
                return Title;
            }

            public void setSelector(string value)
            {
                Selector = value;
            }

            public string getSelector()
            {
                return Selector;
            }

            public void setIndent(string value)
            {
                Indent = value;
            }

            public string getIndent()
            {
                return Indent;
            }

            public void setFormat(string value)
            {
                Format = value;
            }

            public string getFormat()
            {
                return Format;
            }

            private void parseConfig(string data)
            {
                string[] Lines = data.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < Lines.Length; i++)
                {
                    string Line = Lines[i];
                    if (hasCommand(Line))
                    {
                       if(isCommand(Line, CMD_TITLE))
                        {
                            setTitle(getValue(Line));
                        } else if (isCommand(Line, CMD_INDENT))
                        {
                            setIndent(getValue(Line));
                        } else if (isCommand(Line, CMD_SELECTOR))
                        {
                            setSelector(getValue(Line));
                        }
                        else if (isCommand(Line, CMD_FORMAT))
                        {
                            setFormat(getValue(Line));
                        }

                    } else
                    {
                        MenuItems.Add(Line);
                    }
                }                                                
            }

            private bool hasCommand(string data)
            {
                return data.StartsWith(CONFIG_CMD_PREFIX);
            }          

            private bool isCommand(string data, string command)
            {
                string lineStart = CONFIG_CMD_PREFIX + command + '=';
                return (data.StartsWith(lineStart));
            }             

            private string getValue(string data, string defaultValue = "")
            {
                string value = defaultValue; 
                if (data.Contains("="))
                {
                    value = data.Remove(0, data.IndexOf('=') + 1);
                }

                return value;
            }
        }


        class DasMenuView
        {
            public string getContent(DasMenuItem RootItem, DasMenuConfig Config)
            {
                StringBuilder content = new StringBuilder();
                if (Config.getTitle().Length > 0)
                {
                    content.AppendLine(Config.getTitle());
                }
                content.Append(getContentChilds(RootItem, Config));

                return content.ToString();
            }

            private StringBuilder getContentChilds(DasMenuItem RootItem, DasMenuConfig Config)
            {
                List<DasMenuItem> Items = RootItem.getChilds();

                StringBuilder content = new StringBuilder();


                for (int i_Items = 0; i_Items < Items.Count; i_Items++)
                {
                    DasMenuItem Item = Items[i_Items];
                    content.AppendLine(renderLine(Item, Config));

                    if (Item.hasChilds())
                    {
                        content.Append(getContentChilds(Item, Config).ToString());
                    }
                }

                return content;
            }

            private string renderLine(DasMenuItem Item, DasMenuConfig Config)
            {
                string content = Config.getFormat();
                string indent = getIndent(Item, Config);

                content = content.Replace(DasMenuConfig.FORMAT_INDENT, indent);
                if(indent.Length > 0)
                {
                    content = content.Replace(DasMenuConfig.FORMAT_INDENTSPACE, " ");
                } else
                {
                    content = content.Replace(DasMenuConfig.FORMAT_INDENTSPACE, "");
                }
                content = content.Replace(DasMenuConfig.FORMAT_LABEL, Item.getLabel());

                return content;
            }

            private string getIndent(DasMenuItem Item, DasMenuConfig Config)
            {
                string indent = "";
                int count = Item.countParents();
                for(int i = 0; i < count; i++)
                {
                    indent += Config.getIndent();
                }

                return indent;
            }


        }

        class DasMenuFactory
        {
            public IMyGridTerminalSystem GridTerminalSystem;

            public DasMenuFactory(IMyGridTerminalSystem value)
            {
                GridTerminalSystem = value;
            }

            public DasMenuItem createFromConfig(DasMenuConfig Config)
            {
                DasMenuRootItem RootItem = (new DasMenuItemFactory()).createRootItem();


                parseConfig(Config, RootItem);

                return RootItem;
            }

            private List<string> getSegments(string data)
            {
                List<string> Segments = new List<string>();
                Segments.AddRange(data.Split(CONFIG_PATH_SEPERATOR));

                return Segments;
            }

            private void parseConfig(DasMenuConfig Config, DasMenuRootItem RootItem)
            {
                List<string> dataLines = Config.getMenuItems();
                for(int i_dataLines = 0; i_dataLines < dataLines.Count; i_dataLines++)
                {
                    string dataLine = dataLines[i_dataLines];
                    parseLine(dataLine, RootItem);
                }
            }

            private void parseLine(string data, DasMenuRootItem RootItem)
            {
                List<string> dataSegments = new List<string>();
                DasMenuItem Parent = RootItem;
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

            private DasMenuPathItem parsePathItem(string data, DasMenuItem Parent)
            {
                DasMenuPathItem PathItem = (new DasMenuItemFactory()).createPathItem(data);
                Parent.addChild(PathItem);

                return PathItem;
            }

            private void parseBlockItems(string data, DasMenuItem Parent)
            {
                List<IMyTerminalBlock> Blocks = getBlocksFromDataSegment(data);
                for (int i_Blocks = 0; i_Blocks < Blocks.Count; i_Blocks++)
                {
                    IMyTerminalBlock Block = Blocks[i_Blocks];
                    parseSingleBlockItem(Block, Parent);
                }
            }

            private void parseSingleBlockItem(IMyTerminalBlock Block, DasMenuItem Parent)
            {
                DasMenuBlockItem BlockItem = (new DasMenuItemFactory()).createBlockItem(Block);
                Parent.addChild(BlockItem);
                parseActions(BlockItem);
            }

            private void parseActions(DasMenuBlockItem BlockItem)
            {
                List<ITerminalAction> Actions = getActions(BlockItem.getBlock());
                for(int i_Actions = 0; i_Actions < Actions.Count; i_Actions++)
                {
                    ITerminalAction Action = Actions[i_Actions];
                    parseSingleAction(Action, BlockItem);
                }                
            }

            private void parseSingleAction(ITerminalAction Action, DasMenuItem Parent)
            {
                DasMenuActionItem ActionItem = (new DasMenuItemFactory()).createActionItem(Action);
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
                return (Collection.Count - 1 == index);
            }
        }

        class DasMenuItemFactory
        {
            public DasMenuActionItem createActionItem(ITerminalAction Action)
            {
                DasMenuActionItem BuildItem = new DasMenuActionItem();
                BuildItem.setAction(Action);

                return BuildItem;
            }

            public DasMenuBlockItem createBlockItem(IMyTerminalBlock Block)
            {
                DasMenuBlockItem BuildItem = new DasMenuBlockItem();
                BuildItem.setBlock(Block);

                return BuildItem;
            }

            public DasMenuPathItem createPathItem(string label)
            {
                DasMenuPathItem BuildItem = new DasMenuPathItem();
                BuildItem.setLabel(label);

                return BuildItem;
            }

            public DasMenuRootItem createRootItem()
            {
                DasMenuRootItem BuildItem = new DasMenuRootItem();
                return BuildItem;
            }

        }

        class DasMenuItem
        {
            private string Label;
            private DasMenuItem Parent;
            private List<DasMenuItem> Childs = new List<DasMenuItem>();
                        

            public virtual string getUid()
            {
                return this.getBreadCrumbs("" + CONFIG_PATH_SEPERATOR);
            }

            public virtual string getBreadCrumbs(string seperator)
            {
                string BreadCrumbs = "/";
                BreadCrumbs += getLabel();
                if (hasParent())
                {
                    BreadCrumbs = getParent().getBreadCrumbs(seperator) + BreadCrumbs;
                }

                return BreadCrumbs;
            }

            public void setLabel(string value)
            {
                Label = value;
            }

            public string getLabel()
            {
                return Label;
            }

            public void setParent(DasMenuItem value)
            {
                Parent = value;
            }

            public DasMenuItem getParent()
            {
                return Parent;
            }

            public bool hasParent()
            {
                return (this.getParent() != null);
            }

            public void setChilds(List<DasMenuItem> value)
            {
                Childs = value;
            }

            public List<DasMenuItem> getChilds()
            {
                return Childs;
            }

            public void addChild(DasMenuItem value)
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

            public void removeChild(DasMenuItem value)
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

            public int countParents()
            {
                int count = 0;
                if (this.hasParent() && !(this.getParent() is DasMenuRootItem))
                {
                    count++;
                    count += this.getParent().countParents();
                }

                return count;
            }

            public List<DasMenuItem> getParents()
            {
                List<DasMenuItem> Parents = new List<DasMenuItem>();
                if (this.hasParent())
                {
                    Parents.Add(this.getParent());
                    Parents.AddRange(this.getParent().getParents());
                }

                return Parents;
            }
        }

        class DasMenuPathItem : DasMenuItem
        {

        }

        class DasMenuBlockItem : DasMenuItem
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

            public override string getUid()
            {
                return getBlock().ToString();
            }
        }

        class DasMenuActionItem : DasMenuBlockItem
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
            
            public override string getUid()
            {
                string uid = getAction().Id;
                if(hasParent() && getParent() is DasMenuBlockItem)
                {
                    uid = (getParent() as DasMenuBlockItem).getUid() + "_" + uid;
                }

                return uid;
            }  
        }

        class DasMenuRootItem : DasMenuItem
        {
            public override string getBreadCrumbs(string seperator)
            {
                return "";
            }
        }
                
        #endregion
    }
}
