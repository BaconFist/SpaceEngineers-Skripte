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
                public void color(char color, Canvas canvas)
                {
                    canvas.setColor(color);
                }

                public void point(Model.Pixel pixel, Canvas canvas)
                {
                    canvas.setPixel(pixel);
                }

                public void moveTo(Point point, Canvas canvas)
                {
                    canvas.getPencil().setPosition(point);
                }

                public void polygon(Model.Polygon polygon, Canvas canvas)
                {
                    for (int i = 0; i < polygon.getPixels().Count; i++)
                    {
                        lineTo(polygon.getPixel(i).getPosition(), canvas);
                    }
                }

                public void lineTo( Point point, Canvas canvas)
                {
                    if (canvas.isLineInClippingArea(point))
                    {
                        int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                        deltaX = point.X - canvas.getPencil().getPosition().X;
                        deltaY = point.Y - canvas.getPencil().getPosition().Y;

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
                        x = canvas.getPencil().getPosition().X;
                        y = canvas.getPencil().getPosition().Y;
                        err = el / 2;
                        this.point(new Model.Pixel(new Point(x, y), '1'), canvas);

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
                            this.point(new Model.Pixel(new Point(x, y), '1'), canvas);
                        }
                    } else
                    {
                        moveTo(point, canvas);
                    }
                }

                public void text(string text, Canvas canvas, Model.Font font)
                {
                    char[] chars = text.ToCharArray();
                    Point pencilPos = canvas.getPencil().getPosition();
                    for(int i = 0; i < chars.Length; i++)
                    {
                        if (i != 0)
                        {
                            moveTo(new Point((i * font.getDimension().width + pencilPos.X), pencilPos.Y), canvas);
                        }                        
                        bitmap(font.getGlyph(chars[i]), canvas);                        
                    }
                }

                public void bitmap(Model.Bitmap bitmap, Canvas canvas)
                {
                    multiPoint(bitmap.getPixels(), canvas, true);
                }

                public void multiPoint(List<Model.Pixel> pixels, Canvas canvas, bool relativePositions = false)
                {
                    for(int i = 0; i < pixels.Count; i++)
                    {
                        Model.Pixel pixel = (relativePositions)?getAbsolutePixel(pixels[i], canvas): pixels[i];
                        point(pixel, canvas);
                    }
                }

                private void canvas(Canvas toDraw, Canvas canvas)
                {
                    if (!toDraw.Equals(canvas))
                    {
                        multiPoint(toDraw.getPixelList(), canvas, true);
                    }                  
                }

                private Model.Pixel getAbsolutePixel(Model.Pixel pixel, Canvas canvas)
                {
                    int x = canvas.getPencil().getPosition().X + pixel.getPosition().X;
                    int y = canvas.getPencil().getPosition().Y + pixel.getPosition().Y;
                    pixel.setPosition(new Point(x,y));

                    return pixel;
                }
            }

            public class Canvas
            {
                private Dictionary<Point, Model.Pixel> pixels = new Dictionary<Point, Model.Pixel>();
                private Model.Dimension dimensions;
                private char bgColDefault;
                private Model.Pixel pencil;
                
                public Canvas(Model.Dimension dimensions, char pencilColor, char backgroundColor)
                {
                    this.dimensions = dimensions;
                    bgColDefault = backgroundColor;
                    pencil = new Model.Pixel(new Point(0, 0), pencilColor);
                } 

                public void setColor(char color)
                {
                    getPencil().setColor(color);
                }

                public char getColor()
                {
                    return getPencil().getColor();
                }

                public bool setPixel(Model.Pixel Pixel)
                {
                    bool isDrawed = false;
                    if (isPointInClippingArea(Pixel.getPosition()))
                    {
                        char color = Model.Color.get(Pixel.getColor());
                        if (color != Model.Color.TRANSPARENT) {
                            this.getPixel(Pixel.getPosition()).setColor(color);
                            isDrawed = true;
                        }
                    }
                    setPencil(Pixel);

                    return isDrawed;
                }
                public List<Model.Pixel> getPixelList()
                {
                    return pixels.Values.ToList<Model.Pixel>();
                }

                public Model.Pixel getPencil()
                {
                    return pencil;
                }

                public void setPencil(Model.Pixel point)
                {
                    pencil = point;
                }

                public Model.Dimension getDimensions()
                {
                    return dimensions;
                }
                
                public bool hasPixel(Point point)
                {
                    return pixels.ContainsKey(point);
                }

                public Model.Pixel getPixel(Point point)
                {
                    if (!hasPixel(point))
                    {
                        pixels.Add(point, new Model.Pixel(point, bgColDefault));
                    }
                    
                    return pixels[point];
                } 
                
                public bool isPointInClippingArea(Point point)
                {
                    return 0 <= point.X && point.X < getDimensions().width && 0 <= point.Y && point.Y < getDimensions().height;
                }

                public bool isLineInClippingArea(Point point)
                {
                    int posA = getPointPosition(getPencil().getPosition());
                    int posB = getPointPosition(point);

                    return (posA & posB) == 0;
                }

                private int getPointPosition(Point point)
                {
                    int valX = (point.X < 0) ? 1 : (point.X >= getDimensions().width) ? 2 : 0;
                    int valY = (point.Y < 0) ? 8 : (point.Y >= getDimensions().height) ? 4 : 0;

                    return valX + valY;
                }
            }
        }
        
        abstract class Model
        {
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

                private Dictionary<char, char> map = new Dictionary<char, char>();
                static private Color instance;

                public Color()
                {
                    map.Add('g', GREEN);
                    map.Add('G', GREEN);
                    map.Add('b', BLUE);
                    map.Add('B', BLUE);
                    map.Add('r', RED);
                    map.Add('R', RED);
                    map.Add('y', YELLOW);
                    map.Add('Y', YELLOW);
                    map.Add('w', WHITE);
                    map.Add('W', WHITE);
                    map.Add('l', LIGHT_GRAY);
                    map.Add('L', LIGHT_GRAY);
                    map.Add('m', MEDIUM_GRAY);
                    map.Add('M', MEDIUM_GRAY);
                    map.Add('d', DARK_GRAY);
                    map.Add('D', DARK_GRAY);
                    map.Add('0', TRANSPARENT);
                    map.Add('1', PENCIL);
                }

                static private Color getInstance()
                {
                    if((Color.instance == null) || !(Color.instance is Color)){
                        Color.instance = new Color();
                    }

                    return Color.instance;
                }

                static public char get(char key)
                {
                    Color color = Color.getInstance();
                    return color.map.ContainsKey(key) ? color.map[key] : key;
                }
            }

            public class Polygon : Base
            {
                private List<Model.Pixel> pixels = new List<Pixel>();

                public void addPixel(Pixel pixel)
                {
                    pixels.Add(pixel);
                }

                public List<Model.Pixel> getPixels()
                {
                    return pixels;
                }

                public Model.Pixel getPixel(int index)
                {
                    if (!(0 <= index && index < pixels.Count))
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return pixels[index];
                }
            }

            public class Font : Base
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

            public class Bitmap : Base
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

            public class Pixel : Base
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

                public void setPosition(Point point)
                {
                    position = point;
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

                public Model.Bitmap fromSingleLine(string pattern, Model.Dimension dimension)
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
                    Model.Font newFont = new Model.Font();
                    Parser.Font fontParser = new Parser.Font(new Factory.Bitmap());
                    string[] lines = definition.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    newFont = fontParser.parse<Model.Font>(lines, new Definition.Font(), new Factory.Font());
                                              
                    return newFont;
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
                    private Factory.Bitmap bitmapFactory;
                    
                    public Font(Factory.Bitmap bitmapFactory)
                    {
                        this.bitmapFactory = bitmapFactory;
                    }

                    protected Factory.Bitmap getBitmapFactory()
                    {
                        return bitmapFactory;
                    }

                    public override void parseCloseTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches)
                    {
                        
                    }

                    public override void parseCommentTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches)
                    {
                        
                    }

                    public override void parseLineTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches)
                    {
                        List<string> temp = new List<string>();
                        char glyph = ' ';
                        if (!(base.TryGetGroupValue(matches, "char", ref temp) && char.TryParse(temp.First(), out glyph)))
                        {
                            throw new ArgumentException("cant parse glyph");
                        }
                        temp.Clear();
                        if (!base.TryGetGroupValue(matches, "pattern", ref temp))
                        {
                            throw new ArgumentException("cant parse pattern");
                        } else
                        {   
                            Model.Bitmap bitmap = getBitmapFactory().fromSingleLine(temp.First(), (model as Model.Font).getDimension());
                            (model as Model.Font).addGlyph(glyph, bitmap);
                        }
                    }

                    public override void parseOpenTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches)
                    {
                        List<string> widths = new List<string>();
                        List<string> heights = new List<string>();
                        List<string> names = new List<string>();
                        if(base.TryGetGroupValue(matches, "width", ref widths) && base.TryGetGroupValue(matches, "height", ref heights) && base.TryGetGroupValue(matches, "name", ref names))
                        {
                            int width = 0;
                            int height = 0;
                            if(!(int.TryParse(widths.First(), out width) && int.TryParse(heights.First(), out height)))
                            {
                                throw new ArgumentException("can't parse dimensions");
                            }
                            (model as Model.Font).setName(names.First());
                            (model as Model.Font).getDimension().height = height;
                            (model as Model.Font).getDimension().width = width;
                        }
                    }
                }

                abstract public class Base
                {
                    public T parse<T>(string[] lines, Definition.Base definition, Factory.Base factory) where T : Model.Base
                    {
                        T model = factory.getModel<T>();

                        bool hasOpenTag = false;
                        bool hasCloseTag = false;
                        for(int i = 0; !hasCloseTag && i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (hasOpenTag && definition.isCommentTag(line))
                            {
                                parseCommentTag<T>(ref model, definition.getMatchesCommentTag(line));
                            } else if (!hasOpenTag && definition.isOpenTag(line))
                            {
                                parseOpenTag<T>(ref model, definition.getMatchesOpenTag(line));
                                hasOpenTag = true;
                            } else if (hasOpenTag && definition.isLineTag(line))
                            {
                                parseLineTag<T>(ref model, definition.getMatchesLineTag(line));
                            } else if (hasOpenTag && definition.isCloseTag(line))
                            {
                                parseCloseTag<T>(ref model, definition.getMatchesCloseTag(line));
                                hasCloseTag = true;
                            } else
                            {
                                throw new ArgumentException("unable to parse line "+i.ToString(), "string[] line");
                            }
                        }

                        return (model as T);
                    }
                                       
                    abstract public void parseOpenTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseLineTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseCloseTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;
                    abstract public void parseCommentTag<T>(ref T model, System.Text.RegularExpressions.MatchCollection matches) where T : Model.Base;

                    public bool TryGetGroupValue(System.Text.RegularExpressions.MatchCollection matches, string name, ref List<string> values)
                    {
                        bool success = false;

                        for(int i = 0; i < matches.Count; i++)
                        {
                            if (matches[i].Groups[name].Success)
                            {
                                values.Add(matches[i].Groups[name].Value);
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
                    this.openTag = new System.Text.RegularExpressions.Regex(@"^@BITMAP\s+(?<width>\d+)x(?<height>\d+)\s+(?<name>\S+)\s*$");
                    this.lineTag = new System.Text.RegularExpressions.Regex(@"^(\d+)$");
                    this.closeTag = new System.Text.RegularExpressions.Regex(@"^@ENDBITMAP$");
                }
            }

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
                
        #endregion
    }
}
