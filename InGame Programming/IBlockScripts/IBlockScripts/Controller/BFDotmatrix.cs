using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;
using VRage.Game.ModAPI;

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
        
        abstract class SpaceEngineersBridge
        {
            public class BlockFinder
            {
                public List<T> findAllByTag<T>(string tag, IMyGridTerminalSystem GridTerminalSystem, IMyCubeGrid CubeGrid = null) where T : IMyTerminalBlock
                {
                    if (tag.StartsWith("T:"))
                    {
                        tag = tag.Remove(0, 2);
                    } else
                    {
                        CubeGrid = null;
                    }

                    List<IMyTerminalBlock> MatchingBlocks = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<T>(MatchingBlocks, (x => x.CustomName.Contains(tag) && (CubeGrid == null || x.CubeGrid.Equals(CubeGrid)) ));

                    return MatchingBlocks.ConvertAll<T>(x => (T)x);
                }
            }

            public class TextPanelRW
            {
                private IMyTextPanel TextPanel;
                
                public TextPanelRW(IMyTextPanel TextPanel, IMyGridTerminalSystem GridTerminalSystem)
                {
                    this.TextPanel = TextPanel;
                }

                public void setImage(Model.Canvas Canvas)
                {
                    TextPanel.WritePublicText(Canvas.getWorkspace().ToString());
                }

                public string getConfig()
                {
                    return TextPanel.GetPrivateText();
                }
                
            }
        }

        abstract class Drawing
        {
            public class Vector2D
            {
                public void color(char color, Model.Canvas Canvas)
                {
                    Canvas.setColor(color);
                }

                public void point(Model.Pixel Pixel, Model.Canvas Canvas)
                {
                    Canvas.setPixel(Pixel);
                }

                public void moveTo(Point Point, Model.Canvas Canvas)
                {
                    Canvas.getPencil().setPosition(Point);
                }

                public void polygon(Model.Polygon Polygon, Model.Canvas Canvas)
                {
                    for (int i = 0; i < Polygon.getPixels().Count; i++)
                    {
                        lineTo(Polygon.getPixel(i).getPosition(), Canvas);
                    }
                }

                public void lineTo( Point Point, Model.Canvas Canvas)
                {
                    if (Canvas.isLineInClippingArea(Point))
                    {
                        int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                        deltaX = Point.X - Canvas.getPencil().getPosition().X;
                        deltaY = Point.Y - Canvas.getPencil().getPosition().Y;

                        incrementX = Math.Sign(deltaX);
                        incrementY = Math.Sign(deltaY);
                        if (deltaX < 0) deltaX = -deltaX;
                        if (deltaY < 0) deltaY = -deltaY;

                        if (deltaX > deltaY)
                        {
                            pdx = incrementX; pdy = 0;
                            ddx = incrementX; ddy = incrementY;
                            es = deltaY; el = deltaX;
                        }
                        else
                        {
                            pdx = 0; pdy = incrementY;
                            ddx = incrementX; ddy = incrementY;
                            es = deltaX; el = deltaY;
                        }
                        x = Canvas.getPencil().getPosition().X;
                        y = Canvas.getPencil().getPosition().Y;
                        err = el / 2;
                        this.point(new Model.Pixel(new Point(x, y), Canvas.getColor()), Canvas);

                        for (t = 0; t < el; ++t)
                        {
                            err -= es;
                            if (err < 0)
                            {
                                err += el;
                                x += ddx;
                                y += ddy;
                            }
                            else
                            {
                                x += pdx;
                                y += pdy;
                            }
                            this.point(new Model.Pixel(new Point(x, y), Canvas.getColor()), Canvas);
                        }
                    } else
                    {
                        moveTo(Point, Canvas);
                    }
                }

                public void text(string text, Model.Canvas Canvas, Model.Font Font)
                {
                    char[] chars = text.ToCharArray();
                    Point PencilStartPosition = Canvas.getPencil().getPosition();
                    for(int i = 0; i < chars.Length; i++)
                    {
                        if (i != 0)
                        {
                            moveTo(new Point((i * Font.getDimension().width + PencilStartPosition.X), PencilStartPosition.Y), Canvas);
                        }                        
                        bitmap(Font.getGlyph(chars[i]), Canvas);                        
                    }
                }

                public void bitmap(Model.Bitmap Bitmap, Model.Canvas Canvas)
                {
                    multiPoint(Bitmap.getPixels(), Canvas, true);
                }

                public void multiPoint(List<Model.Pixel> Pixels, Model.Canvas Canvas, bool relativePositions = false)
                {
                    for(int i = 0; i < Pixels.Count; i++)
                    {
                        Model.Pixel pixel = (relativePositions)?getAbsolutePixel(Pixels[i], Canvas): Pixels[i];
                        point(pixel, Canvas);
                    }
                }

                private void canvas(Model.Canvas ToDraw, Model.Canvas Canvas)
                {
                    if (!ToDraw.Equals(Canvas))
                    {
                        multiPoint(ToDraw.getPixelList(), Canvas, true);
                    }                  
                }

                private Model.Pixel getAbsolutePixel(Model.Pixel Pixel, Model.Canvas Canvas)
                {
                    int x = Canvas.getPencil().getPosition().X + Pixel.getPosition().X;
                    int y = Canvas.getPencil().getPosition().Y + Pixel.getPosition().Y;
                    Pixel.setPosition(new Point(x,y));

                    return Pixel;
                }
            }
        }
        
        abstract class Model
        {
            public class Canvas : Base
            {
                private Dictionary<Point, Pixel> Pixels = new Dictionary<Point, Pixel>();
                private char[][] workspace; // [Y][X]
                private Dimension Dimension;
                private char defaultBackgroundColor;
                private Pixel Pencil;

                public Canvas(Dimension Dimension, char pencilColor, char defaultBackgroundColor)
                {
                    this.Dimension = Dimension;
                    this.defaultBackgroundColor = defaultBackgroundColor;
                    Pencil = new Pixel(new Point(0, 0), pencilColor);
                    createWorkspace(Dimension.width, Dimension.height, defaultBackgroundColor);
                }

                public StringBuilder getWorkspace()
                {
                    StringBuilder sb = new StringBuilder();
                    for(int i = 0; i < workspace.Length; i++)
                    {
                        sb.AppendLine(new String(workspace[i]));
                    }

                    return sb;
                }

                private void createWorkspace(int width, int height, char color)
                {
                    workspace = new Char[height][];
                    for(int i = 0; i < workspace.Length; i++)
                    {
                        workspace[i] = (new String(color, width)).ToCharArray();
                    }
                }

                private void updateWorkspace(int x, int y, char color)
                {
                    if(workspace.Length > y && workspace[y].Length > x) 
                    {
                        workspace[y][x] = color;
                    }
                }

                public void setColor(char color)
                {
                    getPencil().setColor(color);
                }

                public char getColor()
                {
                    return getPencil().getColor();
                }

                public void setPixel(Pixel Pixel)
                {
                    char color = Color.get(Pixel.getColor());
                    if (color == Color.TRANSPARENT)
                    {
                        if (hasPixelAt(Pixel.getPosition()))
                        {
                            Pixels.Remove(Pixel.getPosition());
                        }
                    } else
                    {
                        this.getPixelAt(Pixel.getPosition()).setColor(color);
                    }
                    updateWorkspace(Pixel.getPosition().X, Pixel.getPosition().Y, color);

                    setPencil(Pixel);
                }

                public List<Pixel> getPixelList()
                {
                    List<Pixel> Values = new List<Pixel>();
                    Values.AddRange(Pixels.Values);
                    return Values;
                }

                public Pixel getPencil()
                {
                    return Pencil;
                }

                public void setPencil(Pixel Pencil)
                {
                    this.Pencil = Pencil;
                }

                public Dimension getDimensions()
                {
                    return Dimension;
                }

                public bool hasPixelAt(Point Point)
                {
                    return Pixels.ContainsKey(Point);
                }

                public Pixel getPixelAt(Point Point)
                {
                    if (!hasPixelAt(Point))
                    {
                        Pixels.Add(Point, new Pixel(Point, defaultBackgroundColor));
                    }

                    return Pixels[Point];
                }

                public bool isLineInClippingArea(Point Point)
                {
                    int clippingAreaCodePencil = getClippingAreaCode(getPencil().getPosition());
                    int clippingAreaCodePoint = getClippingAreaCode(Point);

                    return (clippingAreaCodePencil & clippingAreaCodePoint) == 0;
                }

                private int getClippingAreaCode(Point Point)
                {
                    int clippingCodeXAxis = (Point.X < 0) ? 1 : (Point.X >= getDimensions().width) ? 2 : 0;
                    int clippingCodeYAxis = (Point.Y < 0) ? 8 : (Point.Y >= getDimensions().height) ? 4 : 0;

                    return clippingCodeXAxis + clippingCodeYAxis;
                }
            }


            public class Color : Base
            {
                public const char TRANSPARENT = '0';
                public const char PENCIL = '1';
                public const char GREEN = '\uE001';
                public const char BLUE = '\uE002';
                public const char RED = '\uE003';
                public const char YELLOW = '\uE004';
                public const char WHITE = '\uE006';
                public const char LIGHT_GRAY = '\uE00E';
                public const char MEDIUM_GRAY = '\uE00D';
                public const char DARK_GRAY = '\uE00F';

                private Dictionary<char, char> ColorMap = new Dictionary<char, char>();
                static private Color Instance;

                public Color()
                {
                    ColorMap.Add('g', GREEN);
                    ColorMap.Add('G', GREEN);
                    ColorMap.Add('b', BLUE);
                    ColorMap.Add('B', BLUE);
                    ColorMap.Add('r', RED);
                    ColorMap.Add('R', RED);
                    ColorMap.Add('y', YELLOW);
                    ColorMap.Add('Y', YELLOW);
                    ColorMap.Add('w', WHITE);
                    ColorMap.Add('W', WHITE);
                    ColorMap.Add('l', LIGHT_GRAY);
                    ColorMap.Add('L', LIGHT_GRAY);
                    ColorMap.Add('m', MEDIUM_GRAY);
                    ColorMap.Add('M', MEDIUM_GRAY);
                    ColorMap.Add('d', DARK_GRAY);
                    ColorMap.Add('D', DARK_GRAY);
                    ColorMap.Add('0', TRANSPARENT);
                    ColorMap.Add('1', PENCIL);
                }

                static private Color getInstance()
                {
                    if((Color.Instance == null) || !(Color.Instance is Color)){
                        Color.Instance = new Color();
                    }

                    return Color.Instance;
                }

                static public char get(char colorUid)
                {
                    Color Color = Color.getInstance();
                    return Color.ColorMap.ContainsKey(colorUid) ? Color.ColorMap[colorUid] : colorUid;
                }
            }

            public class Polygon : Base
            {
                private List<Model.Pixel> Pixels = new List<Pixel>();

                public void addPixel(Pixel pixel)
                {
                    Pixels.Add(pixel);
                }

                public List<Model.Pixel> getPixels()
                {
                    return Pixels;
                }

                public Model.Pixel getPixel(int index)
                {
                    if (!(0 <= index && index < Pixels.Count))
                    {
                        return null;
                    }

                    return Pixels[index];
                }
            }

            public class Font : Base
            {
                private string name;
                private Dimension Dimension;
                private Dictionary<char, Model.Bitmap> Glyphs = new Dictionary<char, Bitmap>();

                public Font()
                {
                    Dimension = new Dimension(0,0);
                }

                public Dimension getDimension()
                {
                    return Dimension;
                }

                public string getName()
                {
                    return name;
                }

                public void setName(string name)
                {
                    this.name = name;
                }

                public bool addGlyph(char glyph, Model.Bitmap Bitmap)
                {
                    if (!hasGlyph(glyph))
                    {
                        Glyphs.Add(glyph, Bitmap);
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
                        return null;
                    } else
                    {
                        return Glyphs[glyph];  
                    }
                }

            }

            public class Bitmap : Base
            {
                private Dimension Dimension;
                private List<Pixel> Pixels = new List<Pixel>();

                public Bitmap(Dimension Dimension)
                {
                    this.Dimension = Dimension;
                }

                public List<Pixel> getPixels()
                {
                    return this.Pixels;
                }

                public Dimension getDimension()
                {
                    return this.Dimension;
                }
            }

            public class Pixel : Base
            {
                private char color;
                private Point Position;

                public Pixel(Point Position, char color)
                {
                    this.Position = Position;
                    this.color = color;
                }

                public void setColor(char color)
                {
                    this.color = color;
                }

                public Point getPosition()
                {
                    return this.Position;
                }

                public void setPosition(Point Position)
                {
                    this.Position = Position;
                }

                public char getColor()
                {
                    return this.color;
                }
            }

            public class Dimension : Base
            {
                public int width;
                public int height;
                
                public Dimension(int width, int height)
                {
                    this.width = width;
                    this.height = height;
                }
            }

            public class Base
            {

            }
        }

        abstract class Factory
        {
            public class Bitmap : Base
            {

                public Model.Bitmap fromSingleLine(string pattern, Model.Dimension Dimension)
                {
                    Model.Bitmap Bitmap = new Model.Bitmap(Dimension);
                    pattern = clipPattern(pattern, Bitmap.getDimension().height * Bitmap.getDimension().width);
                    int subPartLength = Bitmap.getDimension().width;
                    for(int y = 0; y < Bitmap.getDimension().height; y++)
                    {
                        int start = y * Bitmap.getDimension().width;
                        char[] row = pattern.Substring(start, subPartLength).ToCharArray();
                        for(int x = 0; x < row.Length; x++)
                        {
                            Bitmap.getPixels().Add(new Model.Pixel(new Point(x,y), row[x]));
                        }
                    }    

                    return Bitmap;
                }

                public override T getModel<T>()
                {
                    return new Model.Bitmap(new Model.Dimension(0,0)) as T;
                }

                private string clipPattern(string pattern, int mustSize)
                {
                    if(pattern.Length < mustSize)
                    {
                        pattern = pattern + new String('0', mustSize-pattern.Length);
                    } else if(mustSize < pattern.Length)
                    {
                        pattern = pattern.Substring(0, mustSize);
                    }

                    return pattern;
                }
            }

            public class Font : Base
            {
                public Model.Font fromString(string fontConfiguration)
                {
                    Model.Font Font = new Model.Font();
                    Parser.Font FontParser = new Parser.Font(new Factory.Bitmap());
                    string[] lines = fontConfiguration.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    Font = FontParser.parse<Model.Font>(lines, new ParserDefinition.Font(), new Factory.Font());
                                              
                    return Font;
                }

                public override T getModel<T>()
                {
                    return new Model.Font() as T;
                }
            }

            abstract public class Base
            {
                abstract public T getModel<T>() where T : Model.Base;
            }
        }

        abstract class Parser
        {
                public class Font : Base
                {
                    private Factory.Bitmap BitmapFactory;
                    
                    public Font(Factory.Bitmap BitmapFactory)
                    {
                        this.BitmapFactory = BitmapFactory;
                    }

                    protected Factory.Bitmap getBitmapFactory()
                    {
                        return BitmapFactory;
                    }

                    public override void parseCloseTag<T>(ref T Model, System.Text.RegularExpressions.MatchCollection Matches)
                    {
                        
                    }

                    public override void parseCommentTag<T>(ref T Model, System.Text.RegularExpressions.MatchCollection Matches)
                    {
                        
                    }

                    public override void parseLineTag<T>(ref T Model, System.Text.RegularExpressions.MatchCollection Matches)
                    {
                        List<string> CharValues = new List<string>();
                        char glyph = ' ';
                        if (!(base.TryGetGroupValues(Matches, "char", ref CharValues) && char.TryParse(CharValues[0], out glyph)))
                        {
                            throw new ArgumentException("cant parse glyph");
                        }
                        List<string> PattenValues = new List<string>();
                        if (!base.TryGetGroupValues(Matches, "pattern", ref PattenValues))
                        {
                            throw new ArgumentException("cant parse pattern");
                        } else
                        {   
                            Model.Bitmap GlyphBitmap = getBitmapFactory().fromSingleLine(PattenValues[0], (Model as Model.Font).getDimension());
                            (Model as Model.Font).addGlyph(glyph, GlyphBitmap);
                        }
                    }

                    public override void parseOpenTag<T>(ref T Model, System.Text.RegularExpressions.MatchCollection Matches)
                    {
                        List<string> WidthValues = new List<string>();
                        List<string> HeightValues = new List<string>();
                        List<string> NameValues = new List<string>();
                        if(base.TryGetGroupValues(Matches, "width", ref WidthValues) && base.TryGetGroupValues(Matches, "height", ref HeightValues) && base.TryGetGroupValues(Matches, "name", ref NameValues))
                        {
                            int width = 0;
                            int height = 0;
                            if(!(int.TryParse(WidthValues[0], out width) && int.TryParse(HeightValues[0], out height)))
                            {
                                throw new ArgumentException("can't parse dimensions");
                            }
                            (Model as Model.Font).setName(NameValues[0]);
                            (Model as Model.Font).getDimension().height = height;
                            (Model as Model.Font).getDimension().width = width;
                        }
                    }
                }

                abstract public class Base
                {
                    public T parse<T>(string line, ParserDefinition.Base ParserDefinition, Factory.Base Factory) where T : Model.Base
                    {
                        string[] lines = ParserDefinition.getLines(line);
                        return parse<T>(lines, ParserDefinition, Factory);                   
                    }
                
                    public T parse<T>(string[] lines, ParserDefinition.Base ParserDefinition, Factory.Base Factory) where T : Model.Base
                    {
                        T Model = Factory.getModel<T>();

                        bool hasOpenTag = false;
                        bool hasCloseTag = false;
                        for(int i = 0; !hasCloseTag && i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (hasOpenTag && ParserDefinition.isCommentTag(line))
                            {
                                parseCommentTag<T>(ref Model, ParserDefinition.getMatchesCommentTag(line));
                            } else if (!hasOpenTag && ParserDefinition.isOpenTag(line))
                            {
                                parseOpenTag<T>(ref Model, ParserDefinition.getMatchesOpenTag(line));
                                hasOpenTag = true;
                            } else if (hasOpenTag && ParserDefinition.isLineTag(line))
                            {
                                parseLineTag<T>(ref Model, ParserDefinition.getMatchesLineTag(line));
                            } else if (hasOpenTag && ParserDefinition.isCloseTag(line))
                            {
                                parseCloseTag<T>(ref Model, ParserDefinition.getMatchesCloseTag(line));
                                hasCloseTag = true;
                            } else
                            {
                                throw new ArgumentException("unable to parse line "+i.ToString(), "string[] line");
                            }
                        }

                        return (Model as T);
                    }
                                       
                    abstract public void parseOpenTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseLineTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseCloseTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseCommentTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;

                    public bool TryGetGroupValues(System.Text.RegularExpressions.MatchCollection Matches, string groupName, ref List<string> GroupValues)
                    {
                        bool hasFoundGroupValue = false;

                        for(int i = 0; i < Matches.Count; i++)
                        {
                            if (Matches[i].Groups[groupName].Success)
                            {
                                GroupValues.Add(Matches[i].Groups[groupName].Value);
                                hasFoundGroupValue = true;
                            }
                        }

                        return hasFoundGroupValue;                        
                    }
            }
        }

        abstract class ParserDefinition
        {
            public class Bitmap : Base
            {
                public Bitmap()
                {
                    this.OpenTag = new System.Text.RegularExpressions.Regex(@"^@BITMAP\s+(?<width>\d+)x(?<height>\d+)\s+(?<name>\S+)\s*$");
                    this.LineTag = new System.Text.RegularExpressions.Regex(@"^(\d+)$");
                    this.CloseTag = new System.Text.RegularExpressions.Regex(@"^@ENDBITMAP$");
                }
            }

            public class Font : Base
            {
                public Font()
                {
                    this.OpenTag = new System.Text.RegularExpressions.Regex(@"^@FONT\s+(?<width>\d+)x(?<height>\d+)\s+(?<name>\S+)\s*$");
                    this.LineTag = new System.Text.RegularExpressions.Regex(@"^(?<char>\S)\s(?<pattern>[01]+)$");
                    this.CloseTag = new System.Text.RegularExpressions.Regex(@"^@ENDFONT$");
                }
            }

            abstract public class Base
            {
                protected System.Text.RegularExpressions.Regex OpenTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex LineTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex CloseTag = new System.Text.RegularExpressions.Regex(@".*");
                protected System.Text.RegularExpressions.Regex CommentTag = new System.Text.RegularExpressions.Regex(@"//.*");
                protected char[] splitChars = new Char[] { ';', '\n', '\r' };

                public string[] getLines(string line)
                {
                    return line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                }

                public bool isOpenTag(string line)
                {
                    return this.OpenTag.IsMatch(line);
                }

                public bool isLineTag(string line)
                {
                    return this.LineTag.IsMatch(line);
                }

                public bool isCloseTag(string line)
                {
                    return this.LineTag.IsMatch(line);
                }

                public bool isCommentTag(string line)
                {
                    return this.CommentTag.IsMatch(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesOpenTag(string line)
                {
                    return this.OpenTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesLineTag(string line)
                {
                    return this.LineTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesCloseTag(string line)
                {
                    return this.CloseTag.Matches(line);
                }

                public System.Text.RegularExpressions.MatchCollection getMatchesCommentTag(string line)
                {
                    return this.CommentTag.Matches(line);
                }
            }
        }
                
        #endregion
    }
}
