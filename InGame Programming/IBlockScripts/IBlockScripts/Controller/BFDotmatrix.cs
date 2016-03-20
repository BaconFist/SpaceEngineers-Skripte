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
                public void point(Model.Pixel pixel, Canvas canvas)
                {
                    canvas.setPixel(pixel);
                }

                public void moveTo( Point point, Canvas canvas)
                {
                    canvas.setPencil(point);
                }

                public void polygon(Model.Polygon polygon, Canvas canvas)
                {
                    for (int i = 0; i < polygon.getPixels().Count; i++)
                    {
                        lineTo(polygon.getPixel(i), canvas);
                    }
                }

                public void lineTo( Model.Pixel point, Canvas canvas)
                {
                    int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                    deltaX = point.getPosition().X - canvas.getPencil().X;
                    deltaY = point.getPosition().Y - canvas.getPencil().Y;
                    
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
                    x = canvas.getPencil().X;
                    y = canvas.getPencil().Y;
                    err = el / 2;
                    this.point(new Model.Pixel(new Point(x, y), point.getColor()), canvas);
                   
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
                        this.point(new Model.Pixel(new Point(x, y), point.getColor()), canvas);
                    }
                }

                public void bitmap(Model.Bitmap bitmap, Canvas canvas)
                {
                    multiPoint(bitmap.getPixels(), canvas);
                }

                public void multiPoint(List<Model.Pixel> pixels, Canvas canvas)
                {
                    for(int i = 0; i < pixels.Count; i++)
                    {
                        point(pixels[i], canvas);
                    }
                }
            }

            public class Canvas
            {
                private Dictionary<Point, Model.Pixel> pixels = new Dictionary<Point, Model.Pixel>();
                private Model.Dimension dimensions;
                private char bgColDefault;
                private Point pencil;
                
                public Canvas(Model.Dimension dimensions, char backgroundColor)
                {
                    this.dimensions = dimensions;
                    bgColDefault = backgroundColor;
                    pencil = new Point(0, 0);
                } 

                public bool setPixel(Model.Pixel Pixel)
                {
                    bool isDrawed = false;
                    if (isInClippingArea(Pixel.getPosition()))
                    {
                        this.getPixel(Pixel.getPosition()).setColor(Pixel.getColor());
                        isDrawed = true;
                    }
                    setPencil(Pixel.getPosition());

                    return isDrawed;
                }

                public Point getPencil()
                {
                    return pencil;
                }

                public void setPencil(Point point)
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
                
                public bool isInClippingArea(Point point)
                {
                    return 0 <= point.X && point.X < getDimensions().width && 0 <= point.Y && point.Y < getDimensions().height;
                }
            }
        }
        
        abstract class Model
        {
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
