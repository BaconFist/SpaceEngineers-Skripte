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
            string[] argList = args.Split(';');
            for (int i = 0; i < argList.Length; i++)
            {
                string cmd = argList[i];
                MyBlockManipulation BlockManipulation = getBlockManipulation(cmd);
            }
        }

        private MyBlockManipulation getBlockManipulation(string cmd)
        {
            if (!MyBlockManipulationCache.data.ContainsKey(cmd))
            {
                MyCompiler Compiler = new MyCompiler();
                MyTokenChain TokenChain = Compiler.getTokenChain(cmd);
                MyBlockManipulation BlockManipulation = Compiler.compile(TokenChain);
                MyBlockManipulationCache.data.Add(cmd, BlockManipulation);
            }
            return MyBlockManipulationCache.data[cmd];
        }


        class MyBlockManipulationCache
        {
            static public Dictionary<string, MyBlockManipulation> data = new Dictionary<string, MyBlockManipulation>();
        }
        

        class MyCompiler {
            private string lexerRegexPattern = "(?<CustomName>(?<=\\(\"|,\"|\\]\")[^\"]+(?=\"))|(?:(?<=\\[)(?<EvalPropertyName>[^!=<>~]+)(?<EvalPropertyExpression>[!=<>~]{1,2})\"(?<EvalPropertyValue>[^\"]+)\"(?=\\]))|(?:(?<=<)(?<BlockType>[^>]+)(?=>))|(?:(?<=\\.)(?<ApplyPropertyName>[^(.]+)\\(\"(?<ApplyPropertyValue>[^\"]+)\"\\)(?=\\.|$))|(?:(?<=\\.)(?<ApplyAction>[^.]+)(?=\\.|$))";
            
            public MyTokenChain getTokenChain(string code)
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(lexerRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
                System.Text.RegularExpressions.MatchCollection Matches = regex.Matches(code);
                MyTokenChain tokenChain = new MyTokenChain(code);
                string[] token = tokenChain.getTokenNames();

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

            public MyBlockManipulation compile(MyTokenChain TokenChain)
            {
                MyBlockManipulation BlockManipulation = new MyBlockManipulation();
                string tempChangePropertyName = "";
                MyEvalProperty tempEvalProperty = new MyEvalProperty();
                for(int i=0;i<TokenChain.Count;i++){
                    KeyValuePair<string, string> token = TokenChain[i];
                    if (token.Key.Equals("CustomName"))
                    {
                        BlockManipulation.addBlockName(token.Value);
                    }
                    else if (token.Key.Equals("EvalPropertyName"))
                    {
                        tempEvalProperty.name = token.Value;
                    }
                    else if (token.Key.Equals("EvalPropertyExpression"))
                    {
                        tempEvalProperty.expression = token.Value;
                    }
                    else if (token.Key.Equals("EvalPropertyValue"))
                    {
                        tempEvalProperty.value = token.Value;
                        BlockManipulation.addEvalProperty(tempEvalProperty);
                        tempEvalProperty = new MyEvalProperty();
                    }
                    else if (token.Key.Equals("BlockType"))
                    {
                        BlockManipulation.addBlockType(token.Value);
                    }
                    else if (token.Key.Equals("ApplyPropertyName"))
                    {
                        tempChangePropertyName = token.Value;
                    }
                    else if (token.Key.Equals("ApplyPropertyValue"))
                    {
                        BlockManipulation.addChangeProperties(tempChangePropertyName, token.Value);
                        tempChangePropertyName = "";
                    }
                    else if (token.Key.Equals("ApplyAction"))
                    {
                        BlockManipulation.addAction(token.Value);
                    }                    
                }

                return BlockManipulation;
            }            
        }

        class MyTokenChain : List<KeyValuePair<string, string>>
        {
            private string[] tokenNames = new string[] { "CustomName", "EvalPropertyName", "EvalPropertyExpression", "EvalPropertyValue", "BlockType", "ApplyPropertyName", "ApplyPropertyValue", "ApplyAction" };
            private string key;

            public MyTokenChain(string key)
            {
                this.key = key;
            }

            public string getKey()
            {
                return key;
            }

            public string[] getTokenNames()
            {
                return this.tokenNames;
            }
        }


        class MyBlockManipulation
        {
            private List<string> BlockTypes = new List<string>();
            private List<string> BlockNames = new List<string>();
            private List<string> Actions = new List<string>();
            private List<MyEvalProperty> EvalProperties = new List<MyEvalProperty>();
            private Dictionary<string, string> ChangeProperties = new Dictionary<string, string>();

            public List<IMyTerminalBlock> getSelectedBlocks(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(Blocks);
                for (int i = Blocks.Count-1; i >= 0; i--)
                {
                    if (!isValidBlock(Blocks[i]))
                    {
                        Blocks.RemoveAt(i);
                    }
                }

                return Blocks;
            }

            public bool isValidBlock(IMyTerminalBlock Block)
            {
                if (!hasMatchingName(Block))
                {
                    return false;
                }
                for (int i = 0; i < EvalProperties.Count; i++)
                {

                }

                return true;
            }

            private bool hasMatchingName(IMyTerminalBlock Block)
            {
                string[] patterns = new string[BlockNames.Count];
                for (int i = 0; i < BlockNames.Count; i++)
                {
                    string tmp = BlockNames[i];
                    tmp = tmp.Replace(".", @"\.");
                    tmp = tmp.Replace("?", ".");
                    tmp = tmp.Replace("*", ".*?");
                    tmp = tmp.Replace(@"\", @"\\");
                    tmp = tmp.Replace(" ", @"\s");
                    patterns[i] = tmp;
                }
                string pattern = "(" + string.Join(")|(", patterns) + ")";

                return new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase).IsMatch(Block.CustomName);
            }

            public void addEvalProperty(MyEvalProperty EvalProperty)
            {
                if(!EvalProperties.Contains(EvalProperty)){
                    EvalProperties.Add(EvalProperty);
                }
            }

            public List<MyEvalProperty> getEvalProperties()
            {
                return EvalProperties;
            }

            public void addBlockType(string BlockType)
            {
                if (!BlockTypes.Contains(BlockType))
                {
                    BlockTypes.Add(BlockType);
                }
            }

            public List<string> getBlockTypes()
            {
                return BlockTypes;
            }

            public void addBlockName(string BlockName)
            {
                if (!BlockNames.Contains(BlockName))
                {
                    BlockNames.Add(BlockName);
                }
            }

            public List<string> getBlockNames()
            {
                return BlockNames;
            }

            public void addAction(string Action)
            {
                if (!Actions.Contains(Action))
                {
                    Actions.Add(Action);
                }                
            }

            public List<string> getActions()
            {
                return Actions;
            }

            public void addChangeProperties(string ChangeProperty, string Value)
            {
                if (ChangeProperties.ContainsKey(ChangeProperty))
                {
                    ChangeProperties[ChangeProperty] = Value;
                }
                else
                {
                    ChangeProperties.Add(ChangeProperty, Value);
                }
            }

            public Dictionary<string, string> getChangeProperties()
            {
                return ChangeProperties;
            }
        }

        class MyEvalProperty
        {
            public string name { get; set; }
            public string expression { get; set; }
            public string value { get; set; }

            public bool isMatch(IMyTerminalBlock Block)
            {
                ITerminalProperty property = Block.GetProperty(name);
                
                if (property is ITerminalProperty)
                {
                    if (property.TypeName.Equals("single"))
                    {
                        float castedValue;
                        if (float.TryParse(value, out castedValue))
                        {
                            float blockValue = property.AsFloat().GetValue(Block);
                            if (expression.Equals("="))
                            {
                                return blockValue == castedValue;
                            } else if (expression.Equals("<"))
                            {
                                return blockValue < castedValue;  
                            } else if (expression.Equals("<="))
                            {
                                return blockValue <= castedValue;
                            } else if (expression.Equals(">"))
                            {
                                return blockValue > castedValue;
                            } else if (expression.Equals(">="))
                            {
                                return blockValue >= castedValue;
                            } else if (expression.Equals("!="))
                            {
                                return blockValue != castedValue;
                            }  
                        }
                    }
                    else if (property.TypeName.Equals("bool"))
                    {
                        bool castedValue = (value.ToLower().Equals("true") || value.Equals("1")) ? true : false;
                        bool blockValue = property.AsBool().GetValue(Block);
                        if (expression.Equals("="))
                        {
                            return blockValue == castedValue;
                        }
                        if (expression.Equals("!="))
                        {
                            return blockValue != castedValue;
                        }
                        return false;
                    }
                    else if (property.TypeName.Equals("color"))
                    {
                        Color blockValue = property.AsColor().GetValue(Block);
                        Color castedValue = new Color();
                        byte r;
                        byte g;
                        byte b;
                        string[] rgb = value.Split(':');
                        if (byte.TryParse(rgb[0], out r) && byte.TryParse(rgb[1], out g) && byte.TryParse(rgb[2], out b))
                        {
                            castedValue.R = r;
                            castedValue.G = g;
                            castedValue.B = b;

                            if (expression.Equals("="))
                            {
                                return ((blockValue.R == castedValue.R) && (blockValue.G == castedValue.G) && (blockValue.B == castedValue.B));
                            }
                            else if (expression.Equals("!="))
                            {
                                return !((blockValue.R == castedValue.R) && (blockValue.G == castedValue.G) && (blockValue.B == castedValue.B));
                            }
                            else if (expression.Equals("<"))
                            {
                                return ((blockValue.R < castedValue.R) && (blockValue.G < castedValue.G) && (blockValue.B < castedValue.B));
                            }
                            else if (expression.Equals("<="))
                            {
                                return ((blockValue.R <= castedValue.R) && (blockValue.G <= castedValue.G) && (blockValue.B <= castedValue.B));
                            }
                            else if (expression.Equals(">"))
                            {
                                return ((blockValue.R > castedValue.R) && (blockValue.G > castedValue.G) && (blockValue.B > castedValue.B));
                            }
                            else if (expression.Equals(">="))
                            {
                                return ((blockValue.R >= castedValue.R) && (blockValue.G >= castedValue.G) && (blockValue.B >= castedValue.B));
                            }
                        }
                    }
                    else if (property.TypeName.Equals("string"))
                    {
                        string castedValue = value.ToString();
                        string blockValue = property.ToString();
                        if (expression.Equals("="))
                        {
                            return blockValue.Equals(castedValue);
                        }
                        else if (expression.Equals("!="))
                        {
                            return !blockValue.Equals(castedValue);
                        }
                        else if (expression.Equals("<"))
                        {
                            return blockValue.Length < castedValue.Length;
                        }
                        else if (expression.Equals("<="))
                        {
                            return blockValue.Length <= castedValue.Length;
                        }
                        else if (expression.Equals(">"))
                        {
                            return blockValue.Length > castedValue.Length;
                        }
                        else if (expression.Equals(">="))
                        {
                            return blockValue.Length >= castedValue.Length;
                        }
                        else if (expression.Equals("~"))
                        {
                            return blockValue.Contains(castedValue);
                        }
                    } 
                }

                return true;
            }
        }
                      
        #endregion
    }
}
