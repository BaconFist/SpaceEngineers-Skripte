using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class SimpleEvents : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           SimpleEvents
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
            Interpreter interpreter = new Interpreter();
            var tokens = interpreter.getTokenized().ToList();
            for(int i = 0; i < tokens.Count; i++)
            {
                Echo(tokens[i].ToString());
            }
        }

        class Interpreter
        {
            public IEnumerable<SimpleLexer.Token> getTokenized()
            {
                SimpleLexer.Lexer lexer = new SimpleLexer.Lexer();

                addDefinitionToLexer(lexer, "(operator)", @"\*|\/|\+|\-");
                addDefinitionToLexer(lexer, "(literal)", @"\d+");
                addDefinitionToLexer(lexer, "(white-space)", @"\s+");
                IEnumerable<SimpleLexer.Token> tokens = lexer.Tokenize("1 * 2 / 3 + 4 - 5");

                return tokens;
            }

            private void addDefinitionToLexer(SimpleLexer.Lexer Lexer, string type, string pattern, bool isIgnored = false)
            {
                Lexer.AddDefinition(new SimpleLexer.TokenDefinition(type, new System.Text.RegularExpressions.Regex(pattern), isIgnored));
            }

        }

        class SimpleLexer
        {
            public class TokenPosition
            {
                public TokenPosition(int index, int line, int column)
                {
                    Index = index;
                    Line = line;
                    Column = column;
                }

                public int Column { get; private set; }
                public int Index { get; private set; }
                public int Line { get; private set; }
            }
            public interface ILexer
            {
                void AddDefinition(TokenDefinition tokenDefinition);
                IEnumerable<Token> Tokenize(string source);
            }

            public class Lexer : ILexer
            {
                System.Text.RegularExpressions.Regex endOfLineRegex = new System.Text.RegularExpressions.Regex(@"\r\n|\r|\n", System.Text.RegularExpressions.RegexOptions.Compiled);
                IList<TokenDefinition> tokenDefinitions = new List<TokenDefinition>();

                public void AddDefinition(TokenDefinition tokenDefinition)
                {
                    tokenDefinitions.Add(tokenDefinition);
                }

                public IEnumerable<Token> Tokenize(string source)
                {
                    int currentIndex = 0;
                    int currentLine = 1;
                    int currentColumn = 0;

                    while (currentIndex < source.Length)
                    {
                        TokenDefinition matchedDefinition = null;
                        int matchLength = 0;

                        foreach (var rule in tokenDefinitions)
                        {
                            var match = rule.Regex.Match(source, currentIndex);

                            if (match.Success && (match.Index - currentIndex) == 0)
                            {
                                matchedDefinition = rule;
                                matchLength = match.Length;
                                break;
                            }
                        }

                        if (matchedDefinition == null)
                        {
                            throw new Exception(string.Format("Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).", source[currentIndex], currentIndex, currentLine, currentColumn));
                        }
                        else
                        {
                            var value = source.Substring(currentIndex, matchLength);

                            if (!matchedDefinition.IsIgnored)
                                yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentLine, currentColumn));

                            var endOfLineMatch = endOfLineRegex.Match(value);
                            if (endOfLineMatch.Success)
                            {
                                currentLine += 1;
                                currentColumn = value.Length - (endOfLineMatch.Index + endOfLineMatch.Length);
                            }
                            else
                            {
                                currentColumn += matchLength;
                            }

                            currentIndex += matchLength;
                        }
                    }

                    yield return new Token("(end)", null, new TokenPosition(currentIndex, currentLine, currentColumn));
                }
            }
            public class Token
            {
                public Token(string type, string value, TokenPosition position)
                {
                    Type = type;
                    Value = value;
                    Position = position;
                }

                public TokenPosition Position { get; set; }
                public string Type { get; set; }
                public string Value { get; set; }

                public override string ToString()
                {
                    return string.Format("Token: {{ Type: \"{0}\", Value: \"{1}\", Position: {{ Index: \"{2}\", Line: \"{3}\", Column: \"{4}\" }} }}", Type, Value, Position.Index, Position.Line, Position.Column);
                }
            }

            public class TokenDefinition
            {
                public TokenDefinition(
                    string type,
                    System.Text.RegularExpressions.Regex regex)
                    : this(type, regex, false)
                {
                }

                public TokenDefinition(
                    string type,
                    System.Text.RegularExpressions.Regex regex,
                    bool isIgnored)
                {
                    Type = type;
                    Regex = regex;
                    IsIgnored = isIgnored;
                }

                public bool IsIgnored { get; private set; }
                public System.Text.RegularExpressions.Regex Regex { get; private set; }
                public string Type { get; private set; }
            }
        }



        #endregion
    }
}
