using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class BFDotmatrix : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BFDotmatrix
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
        
        abstract class Model
        {
            public class Font
            {
                private string name;
                private Dimension dimension;
                private Dictionary<char, Model.Bitmap> Glyphs = new Dictionary<char, Bitmap>();

                public Font()
                {
                    dimension = new Dimension(0,0);
                }

                public Dimension getDimension()
                {
                    return dimension;
                }

                public string getName()
                {
                    return name;
                }

                public void setName(string name)
                {
                    this.name = name;
                }

                public bool addGlyph(char glyph, Model.Bitmap bitmap)
                {
                    if (!hasGlyph(glyph))
                    {
                        Glyphs.Add(glyph, bitmap);
                        return hasGlyph(glyph);
                    } else
                    {
                        return false;
                    }
                }

                public bool hasGlyph(char glyph)
                {
                    return Glyphs.ContainsKey(glyph);
                }

                public Model.Bitmap getGlyph(char glyph)
                {
                    if (!hasGlyph(glyph))
                    {
                        throw new IndexOutOfRangeException();
                    } else
                    {
                        return Glyphs[glyph];  
                    }
                }

            }

            public class Bitmap
            {
                private Dimension dimension;
                private List<Pixel> pixels = new List<Pixel>();

                public Bitmap(Dimension dimension)
                {
                    this.dimension = dimension;
                }

                public List<Pixel> getPixels()
                {
                    return this.pixels;
                }

                public Dimension getDimension()
                {
                    return this.dimension;
                }
            }

            public class Pixel
            {
                private char color;
                private Point position;

                public Pixel(Point position, char color)
                {
                    this.position = position;
                    this.color = color;
                }

                public void setColor(char color)
                {
                    this.color = color;
                }

                public Point getPosition()
                {
                    return this.position;
                }

                public char getColor()
                {
                    return this.color;
                }
            }

            public class Dimension
            {
                public int width;
                public int height;
                
                public Dimension(int width, int height)
                {
                    this.width = width;
                    this.height = height;
                }
            }
        }

        abstract class Factory
        {
            public class Bitmap
            {
                public Model.Bitmap fromString(string pattern, Model.Dimension dimension)
                {
                    Model.Bitmap newBitmap = new Model.Bitmap(dimension);
                    pattern = resizePattern(pattern, newBitmap.getDimension().height * newBitmap.getDimension().width);
                    int count = newBitmap.getDimension().width;
                    for(int y = 0; y < newBitmap.getDimension().height; y++)
                    {
                        int start = y * newBitmap.getDimension().width;
                        char[] row = pattern.Substring(start, count).ToCharArray();
                        for(int x = 0; x < row.Length; x++)
                        {
                            newBitmap.getPixels().Add(new Model.Pixel(new Point(x,y), row[x]));
                        }
                    }    

                    return newBitmap;
                }
                
                private string resizePattern(string pattern, int newSize)
                {
                    if(pattern.Length < newSize)
                    {
                        pattern = pattern + new String('0', newSize-pattern.Length);
                    } else if(newSize < pattern.Length)
                    {
                        pattern = pattern.Substring(0, newSize);
                    }

                    return pattern;
                }
            }

            public class Font
            {
                public Model.Font fromString(string definition)
                {
                    Model.Font newFont = new Model.Font();
                       
                    return newFont;
                }

                private void renderBlock(string[] lines, Definition.Font fontDefinition, Bitmap bitmapFactory)
                {
                    Model.Font newFont = new Model.Font();
                    System.Text.RegularExpressions.MatchCollection matches;
                    bool hasCloseTag = false;
                    bool hasOpenTag = false;
                    for(int i_lines = 0; (!hasCloseTag) && (i_lines < lines.Length); i_lines++)
                    {
                        string currentLine = lines[i_lines];
                        if (fontDefinition.isCommentTag(currentLine))
                        {
                            // ignore, it's a comment
                        } else if (fontDefinition.isOpenTag(currentLine))
                        {
                            bool hasOpenTahWidth = false;
                            bool hasOpenTagHeight = false;
                            bool hasOpenTagName = false;
                            matches = fontDefinition.getMatchesOpenTag(currentLine);
                            for(int i_matches = 0; i_matches < matches.Count; i_matches++)
                            {
                                System.Text.RegularExpressions.Match match = matches[i_matches];
                                if(match.Groups["width"].Success)
                                {
                                    int w = 0;
                                    if (Int32.TryParse(match.Groups["width"].Value, out w))
                                    {
                                        newFont.getDimension().width = w;
                                        hasOpenTahWidth = true;
                                    }                                   
                                } else if (match.Groups["height"].Success)
                                {
                                    int h = 0;
                                    if (Int32.TryParse(match.Groups["height"].Value, out h))
                                    {
                                        newFont.getDimension().height = h;
                                        hasOpenTagHeight = true;
                                    }
                                }
                                else if(match.Groups["name"].Success)
                                {
                                    newFont.setName(match.Groups["name"].Value);
                                    hasOpenTagName = true;
                                }
                            }
                            hasOpenTag = hasOpenTagHeight && hasOpenTahWidth && hasOpenTagName;
                        } else if(hasOpenTag && fontDefinition.isLineTag(currentLine))
                        {
                            matches = fontDefinition.getMatchesLineTag(currentLine);
                            bool hasNewGlyph = false;
                            char glyph = ' ';
                            Model.Bitmap glyphBitmap;
                            for(int i_matches = 0; i_matches < matches.Count; i_matches++)
                            {
                                System.Text.RegularExpressions.Match match = matches[i_matches];
                                if (match.Groups["char"].Success)
                                {
                                    if (char.TryParse(match.Groups["char"].Value, out glyph))
                                    {
                                        hasNewGlyph = true;                            
                                    } else
                                    {
                                        hasNewGlyph = false;
                                    }
                                } else if (hasNewGlyph && match.Groups["pattern"].Success)
                                {
                                    glyphBitmap = bitmapFactory.fromString(match.Groups["pattern"].Value, newFont.getDimension());
                                    newFont.addGlyph(glyph, glyphBitmap);
                                    hasNewGlyph = false;
                                }
                            }
                        } else if (hasOpenTag && fontDefinition.isCloseTag(currentLine))
                        {
                            hasCloseTag = true;
                        } else {

                        }
                    }
                }
            }
        }

        abstract class Definition
        {
            public class Font : Base
            {
                public Font()
                {
                    this.openTag = new System.Text.RegularExpressions.Regex(@"^@FONT\s+(?<width>\d+)x(?<height>\d+)\s+(?<name>\S+)\s*$");
                    this.lineTag = new System.Text.RegularExpressions.Regex(@"^(?<char>\S)\s(?<pattern>[01]+)$");
                    this.closeTag = new System.Text.RegularExpressions.Regex(@"^@ENDFONT$");
                }
            }

            abstract public class Base
            {
                protected System.Text.RegularExpressions.Regex openTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex lineTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex closeTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex commentTag = new System.Text.RegularExpressions.Regex(@"//.*");

                public bool isOpenTag(string line)
                {
                    return this.openTag.IsMatch(line);
                }

                public bool isLineTag(string line)
                {
                    return this.lineTag.IsMatch(line);
                }

                public bool isCloseTag(string line)
                {
                    return this.lineTag.IsMatch(line);
                }

                public bool isCommentTag(string line)
                {
                    return this.commentTag.IsMatch(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesOpenTag(string line)
                {
                    return this.openTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesLineTag(string line)
                {
                    return this.lineTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesCloseTag(string line)
                {
                    return this.closeTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesCommentTag(string line)
                {
                    return this.commentTag.Matches(line);
                }
            }
        }
        
        class PointCollection : List<Point>
        {
            public PointCollection AddPoint(int x, int y)
            {
                Add(new Point(x,y));
                return this;
            }
        }       
        
        #endregion
    }
}
