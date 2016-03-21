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
                        this.point(new Model.Pixel(new Point(x, y), '1'), Canvas);

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
                            this.point(new Model.Pixel(new Point(x, y), '1'), Canvas);
                        }
                    } else
                    {
                        moveTo(Point, Canvas);
                    }
                }

                public void text(string text, Model.Canvas Canvas, Model.Font Font)
                {
                    char[] chars = text.ToCharArray();
                    Point PencilPos = Canvas.getPencil().getPosition();
                    for(int i = 0; i < chars.Length; i++)
                    {
                        if (i != 0)
                        {
                            moveTo(new Point((i * Font.getDimension().width + PencilPos.X), PencilPos.Y), Canvas);
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
                private Dimension Dimension;
                private char bgColDefault;
                private Pixel Pencil;

                public Canvas(Dimension Dimension, char pencilColor, char backgroundColor)
                {
                    this.Dimension = Dimension;
                    bgColDefault = backgroundColor;
                    Pencil = new Pixel(new Point(0, 0), pencilColor);
                }

                public void setColor(char color)
                {
                    getPencil().setColor(color);
                }

                public char getColor()
                {
                    return getPencil().getColor();
                }

                public bool setPixel(Pixel Pixel)
                {
                    bool isDrawed = false;
                    if (isPointInClippingArea(Pixel.getPosition()))
                    {
                        char color = Color.get(Pixel.getColor());
                        if (color != Color.TRANSPARENT)
                        {
                            this.getPixel(Pixel.getPosition()).setColor(color);
                            isDrawed = true;
                        }
                    }
                    setPencil(Pixel);

                    return isDrawed;
                }
                public List<Pixel> getPixelList()
                {
                    return Pixels.Values.ToList();
                }

                public Pixel getPencil()
                {
                    return Pencil;
                }

                public void setPencil(Pixel Point)
                {
                    Pencil = Point;
                }

                public Dimension getDimensions()
                {
                    return Dimension;
                }

                public bool hasPixel(Point Point)
                {
                    return Pixels.ContainsKey(Point);
                }

                public Pixel getPixel(Point Point)
                {
                    if (!hasPixel(Point))
                    {
                        Pixels.Add(Point, new Pixel(Point, bgColDefault));
                    }

                    return Pixels[Point];
                }

                public bool isPointInClippingArea(Point Point)
                {
                    return 0 <= Point.X && Point.X < getDimensions().width && 0 <= Point.Y && Point.Y < getDimensions().height;
                }

                public bool isLineInClippingArea(Point Point)
                {
                    int posA = getPointPosition(getPencil().getPosition());
                    int posB = getPointPosition(Point);

                    return (posA & posB) == 0;
                }

                private int getPointPosition(Point Point)
                {
                    int valX = (Point.X < 0) ? 1 : (Point.X >= getDimensions().width) ? 2 : 0;
                    int valY = (Point.Y < 0) ? 8 : (Point.Y >= getDimensions().height) ? 4 : 0;

                    return valX + valY;
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

                private Dictionary<char, char> Map = new Dictionary<char, char>();
                static private Color Instance;

                public Color()
                {
                    Map.Add('g', GREEN);
                    Map.Add('G', GREEN);
                    Map.Add('b', BLUE);
                    Map.Add('B', BLUE);
                    Map.Add('r', RED);
                    Map.Add('R', RED);
                    Map.Add('y', YELLOW);
                    Map.Add('Y', YELLOW);
                    Map.Add('w', WHITE);
                    Map.Add('W', WHITE);
                    Map.Add('l', LIGHT_GRAY);
                    Map.Add('L', LIGHT_GRAY);
                    Map.Add('m', MEDIUM_GRAY);
                    Map.Add('M', MEDIUM_GRAY);
                    Map.Add('d', DARK_GRAY);
                    Map.Add('D', DARK_GRAY);
                    Map.Add('0', TRANSPARENT);
                    Map.Add('1', PENCIL);
                }

                static private Color getInstance()
                {
                    if((Color.Instance == null) || !(Color.Instance is Color)){
                        Color.Instance = new Color();
                    }

                    return Color.Instance;
                }

                static public char get(char key)
                {
                    Color Color = Color.getInstance();
                    return Color.Map.ContainsKey(key) ? Color.Map[key] : key;
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
                        throw new IndexOutOfRangeException();
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
                        throw new IndexOutOfRangeException();
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

                public void setPosition(Point Point)
                {
                    Position = Point;
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
                    pattern = resizePattern(pattern, Bitmap.getDimension().height * Bitmap.getDimension().width);
                    int count = Bitmap.getDimension().width;
                    for(int y = 0; y < Bitmap.getDimension().height; y++)
                    {
                        int start = y * Bitmap.getDimension().width;
                        char[] row = pattern.Substring(start, count).ToCharArray();
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

            public class Font : Base
            {
                public Model.Font fromString(string definition)
                {
                    Model.Font Font = new Model.Font();
                    Parser.Font FontParser = new Parser.Font(new Factory.Bitmap());
                    string[] lines = definition.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    Font = FontParser.parse<Model.Font>(lines, new Definition.Font(), new Factory.Font());
                                              
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
                        List<string> temp = new List<string>();
                        char glyph = ' ';
                        if (!(base.TryGetGroupValue(Matches, "char", ref temp) && char.TryParse(temp.First(), out glyph)))
                        {
                            throw new ArgumentException("cant parse glyph");
                        }
                        temp.Clear();
                        if (!base.TryGetGroupValue(Matches, "pattern", ref temp))
                        {
                            throw new ArgumentException("cant parse pattern");
                        } else
                        {   
                            Model.Bitmap Bitmap = getBitmapFactory().fromSingleLine(temp.First(), (Model as Model.Font).getDimension());
                            (Model as Model.Font).addGlyph(glyph, Bitmap);
                        }
                    }

                    public override void parseOpenTag<T>(ref T Model, System.Text.RegularExpressions.MatchCollection Matches)
                    {
                        List<string> widths = new List<string>();
                        List<string> heights = new List<string>();
                        List<string> names = new List<string>();
                        if(base.TryGetGroupValue(Matches, "width", ref widths) && base.TryGetGroupValue(Matches, "height", ref heights) && base.TryGetGroupValue(Matches, "name", ref names))
                        {
                            int width = 0;
                            int height = 0;
                            if(!(int.TryParse(widths.First(), out width) && int.TryParse(heights.First(), out height)))
                            {
                                throw new ArgumentException("can't parse dimensions");
                            }
                            (Model as Model.Font).setName(names.First());
                            (Model as Model.Font).getDimension().height = height;
                            (Model as Model.Font).getDimension().width = width;
                        }
                    }
                }

                abstract public class Base
                {
                    public T parse<T>(string[] lines, Definition.Base Definition, Factory.Base Factory) where T : Model.Base
                    {
                        T Model = Factory.getModel<T>();

                        bool hasOpenTag = false;
                        bool hasCloseTag = false;
                        for(int i = 0; !hasCloseTag && i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (hasOpenTag && Definition.isCommentTag(line))
                            {
                                parseCommentTag<T>(ref Model, Definition.getMatchesCommentTag(line));
                            } else if (!hasOpenTag && Definition.isOpenTag(line))
                            {
                                parseOpenTag<T>(ref Model, Definition.getMatchesOpenTag(line));
                                hasOpenTag = true;
                            } else if (hasOpenTag && Definition.isLineTag(line))
                            {
                                parseLineTag<T>(ref Model, Definition.getMatchesLineTag(line));
                            } else if (hasOpenTag && Definition.isCloseTag(line))
                            {
                                parseCloseTag<T>(ref Model, Definition.getMatchesCloseTag(line));
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

                    public bool TryGetGroupValue(System.Text.RegularExpressions.MatchCollection Matches, string name, ref List<string> Values)
                    {
                        bool success = false;

                        for(int i = 0; i < Matches.Count; i++)
                        {
                            if (Matches[i].Groups[name].Success)
                            {
                                Values.Add(Matches[i].Groups[name].Value);
                                success = true;
                            }
                        }

                        return success;                        
                    }
            }
        }

        abstract class Definition
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
