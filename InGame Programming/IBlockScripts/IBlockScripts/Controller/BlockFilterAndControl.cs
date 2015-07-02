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
    public class BlockFilterAndControl : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BlockFilterAndControl
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           

           Abstract
           ------------------------------
          
           
           Example
           ------------------------------
          
       */
        void Main(string args)
        {

        }

        class Parser
        {
            public IMyGridTerminalSystem GridTerminalSystem { get; }
            public MyTokenChain TokenChain { set; get; }
            public Parser(IMyGridTerminalSystem GridTerminalSystem)
            {
                this.GridTerminalSystem = GridTerminalSystem;
            }

            public void doParse()
            {
                if(!(TokenChain is MyTokenChain))
                {
                    return;
                }
                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                string EvalPropertyName = null;
                string EvalPropertyExpression = null;
                string EvalPropertyValue = null;
                for (int i = 0; i < TokenChain.Count; i++)
                {
                    string value = TokenChain[i].Value;
                    string token = TokenChain[i].Key;
                    if (token.Equals("CustomName"))
                    {
                        if (value[0].Equals('!'))
                        {
                            Blocks = removeBlocksByName(value, Blocks);
                        } else
                        {
                            Blocks = addBlocksByName(value, Blocks);
                        }                        
                    } else if (token.Equals("EvalPropertyName"))
                    {
                        EvalPropertyName = value;
                    }
                    else if (token.Equals("EvalPropertyExpression"))
                    {
                        EvalPropertyExpression = value;
                    }
                    else if (token.Equals("EvalPropertyValue"))
                    {
                        EvalPropertyValue = value;
                      //  Blocks = filterListByProperty(EvalPropertyName, EvalPropertyExpression, EvalPropertyValue, Blocks);
                        EvalPropertyName = null;
                        EvalPropertyExpression = null;
                        EvalPropertyExpression = null;
                    } else if (token.Equals("BlockType"))
                    {

                    }
                }
            }

            private List<IMyTerminalBlock> filterListByBlockType(string BlockType, List<IMyTerminalBlock> Blocks)
            {

                return Blocks;
            }

            private List<IMyTerminalBlock> filterListByProperty(string name, string expression, string value, List<IMyTerminalBlock> Blocks)
            {
                for (int i = Blocks.Count - 1; i > -1; i--)
                {
                    if (Blocks[i] is IMyTerminalBlock && !(evalProperty(name, expression, value, Blocks[i])))
                    {
                        Blocks.RemoveAt(i);
                    }
                }
                return Blocks;
            }

            private bool evalProperty(string name, string expression, string value, IMyTerminalBlock Block)
            {
                ITerminalProperty Property = Block.GetProperty(name);
                if (Property is ITerminalProperty)
                {
                    var castedValue = castValue(Property, value);
                    if (expression.Equals("="))
                    {
                        return Property.Equals(castedValue);
                    } else if (expression.Equals("!="))
                    {
                        return !Property.Equals(castedValue);
                    }
                    else if(expression.Equals("<"))
                    {
                        return Property < castedValue;
                    } else if (expression.Equals("<="))
                    {
                        return Property <= castedValue;
                    }
                    else if(expression.Equals(">"))
                    {
                        return Property > castedValue;
                    } else if (expression.Equals(">="))
                    {
                        return Property >= castedValue;
                    }
                    else if(expression.Equals("~"))
                    {
                        return Property.ToString().Contains(value);
                    }
                }

                return false;
            }

            private dynamic castValue(ITerminalProperty Property, string value)
            {
                return null;
            } 

            private List<IMyTerminalBlock> removeBlocksByName(string CustomName, List<IMyTerminalBlock> Blocks)
            {
                CustomName = CustomName.Replace("!", "");
                for(int i = Blocks.Count-1; i > -1; i--)
                {
                    if (Blocks[i] is IMyTerminalBlock && Blocks[i].CustomName.Contains(CustomName))
                    {
                        Blocks.RemoveAt(i);
                    }
                }
                return Blocks;
            }

            private List<IMyTerminalBlock> addBlocksByName(string CustomName, List<IMyTerminalBlock> Blocks)
            {
                List<IMyTerminalBlock> newBlocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.SearchBlocksOfName(CustomName, newBlocks);
                
                for(int i = 0; i < newBlocks.Count; i++)
                {
                    if (!Blocks.Contains(newBlocks[i]))
                    {
                        Blocks.Add(newBlocks[i]);
                    }
                }
                return Blocks;
            }
        }

        class MyTokenChain : List<KeyValuePair<string, string>>
        {
        }

        class MyLexer
        {
            string regexPattern = "(?<CustomName>(?<=\\(\"|,\"|\\]\")[^\"]+(?=\"))|(?:(?<=\\[)(?<EvalPropertyName>[^!=<>~]+)(?<EvalPropertyExpression>[!=<>~]{1,2})\"(?<EvalPropertyValue>[^\"]+)\"(?=\\]))|(?:(?<=<)(?<BlockType>[^>]+)(?=>))|(?:(?<=\\.)(?<ApplyPropertyName>[^(.]+)\\(\"(?<ApplyPropertyValue>[^\"]+)\"\\)(?=\\.|$))|(?:(?<=\\.)(?<ApplyAction>[^.]+)(?=\\.|$))";
            string[] token = new string[] { "CustomName", "EvalPropertyName", "EvalPropertyExpression", "EvalPropertyValue", "BlockType", "ApplyPropertyName", "ApplyPropertyValue", "ApplyAction" };


            public MyTokenChain getTokenChain(string code)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
                System.Text.RegularExpressions.MatchCollection Matches = regex.Matches(code);
                MyTokenChain tokenChain = new MyTokenChain();

                for (int i_matches = 0; i_matches < Matches.Count; i_matches++)
                {
                    System.Text.RegularExpressions.Match m = Matches[i_matches];
                    for (int i_token = 0; i_token < token.Length; i_token++)
                    {
                        if (m.Groups[token[i_token]].Success == true)
                        {
                            tokenChain.Add(new KeyValuePair<string, string>(token[i_token], m.Groups[token[i_token]].Value));
                        }
                    }
                }

                return tokenChain;
            }
        }

        #endregion
    }
}
