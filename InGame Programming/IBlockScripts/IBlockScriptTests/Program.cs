using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBlockScriptTests
{
    class Program
    {
        public static string samplefontdata = @"@FONT 5 5 samplefont
! 0010000100001000000000100
"" 0101001010000000000000000
# 0101011111010101111101010
$ 0111110100011100010111110
% 1100111010001000101110011
' 0010000100000000000000000
( 0001000100001000010000010
) 0100000100001000010001000
* 0101000100010100000000000
, 0000000000000000010001000
- 0000000000011100000000000
. 0000000000000000000000100
/ 0000100010001000100010000
0 0111010011101011100101110
1 0011000010000100001000111
2 1111000001011101000011111
3 1111000001001100000111110
4 0011001010100101111100010
5 1111110000111100000111110
6 0111110000111101000101110
7 1111100001000100010001000
8 0111010001011101000101110
9 0111010001011110000111110
: 0000000100000000010000000
; 0000000100000000010001000
< 0000100010001000001000001
= 0000001110000000111000000
> 1000001000001000100010000
? 0111000001001100000000100
@ 0111110001101111011001111
A 0010001010100011111110001
B 1111010001111101000111110
C 0111110000100001000001111
D 1111010001100011000111110
E 1111110000111001000011111
F 1111110000111001000010000
G 0111110000100111000101111
H 1000110001111111000110001
I 1111100100001000010011111
J 1111100010000101001001100
K 1000110010111001001010001
L 1000010000100001000011111
M 1000111011101011000110001
N 1000111001101011001110001
O 0111010001100011000101110
P 1111010001111101000010000
Q 0111010001101011001001101
R 1111010001111101000110001
S 0111110000011100000111110
T 1111100100001000010000100
U 1000110001100011000101110
V 1000110001100010101000100
W 1000110001101011101110001
X 1000101010001000101010001
Y 1000101010001000010000100
Z 1111100010001000100011111
[ 0011000100001000010000110
\ 1000001000001000001000001
] 0110000100001000010001100
^ 0010001010000000000000000
_ 0000000000000000000011111
` 0010000010000000000000000
{ 0011000100010000010000110
| 0010000100001000010000100
} 0110000100000100010001100
~ 0000001000101010001000000
@ENDFONT";

        static void Main(string[] args)
        {
            Font myFont = Font.createFromString(Program.samplefontdata);

            Dotmatrix bc = new Dotmatrix(100, 20, Dotmatrix.COLOR_DARK_GRAY);
            bc
                .color(Dotmatrix.COLOR_GREEN)
                .moveTo(3,3)
                .text("Hello World!", myFont)
                .moveTo(1,1)
                .rectangle(73, 9)
            ;

            StringBuilder sb = bc.getImage();
            System.Console.Write(sb.ToString());
            System.Console.WriteLine("press any key to exit");
            System.Console.ReadKey();
        }

        class Dotmatrix
        {

//            public const char COLOR_GREEN = '\uE001';
            public const char COLOR_GREEN = 'X';
            public const char COLOR_BLUE = '\uE002';
            public const char COLOR_RED = '\uE003';
            public const char COLOR_YELLOW = '\uE004';
            public const char COLOR_WHITE = '\uE006';
            public const char COLOR_LIGHT_GRAY = '\uE00E';
            public const char COLOR_MEDIUM_GRAY = '\uE00D';
            //public const char COLOR_DARK_GRAY = '\uE00F';
            public const char COLOR_DARK_GRAY = '-';

            private double pixelTolerance = 0.1;

            private int width;
            private int height;
            private char[][] matrixYX;
            private char currentColor;
            private Point cursor;

            public Dotmatrix(int width, int height, char background)
            {
                this.width = width;
                this.height = height;
                color(background);
                fillAll();
                moveTo(new Point(0, 0));
            }

            public StringBuilder getImage()
            {
                StringBuilder content = new StringBuilder();
                for(int i = 0; i < matrixYX.Length; i++)
                {
                    content.AppendLine(new String(matrixYX[i]));
                }

                return content;
            }

            public Dotmatrix fillAll()
            {
                matrixYX = new char[getHeight()][];
                for (int i = 0; i < getHeight(); i++)
                {
                    matrixYX[i] = (new String(currentColor, getWidth())).ToCharArray();
                }

                return this;
            }

            public int getWidth()
            {
                return width;
            }

            private int getXMax()
            {
                return getWidth() - 1;
            }

            private int getYMax()
            {
                return getHeight() - 1;
            }

            public int getHeight()
            {
                return height;
            }

            private void setCursor(Point point)
            {
                cursor = point;
            }

            private void setPixel(Point point)
            {
                if (isPointInViewport(point))
                {
                    matrixYX[point.Y][point.X] = currentColor;
                }
            }

            public Dotmatrix dot(Point point)
            {
                setPixel(point);
                return this;
            }

            public Dotmatrix moveTo(int x, int y)
            {
                return moveTo(new Point(x,y));
            }

            public Dotmatrix moveTo(Point point)
            {
                setCursor(point);
                return this;
            }

            public Dotmatrix lineTo(int x, int y)
            {
                return lineTo(new Point(x, y));
            }

            public Dotmatrix lineTo(Point point)
            {
                gbham(cursor.X, cursor.Y, point.X, point.Y);
                setCursor(point);

                return this;
            }

            private void gbham(int xstart, int ystart, int xend, int yend)
            {
                int x, y, t, dx, dy, incx, incy, pdx, pdy, ddx, ddy, es, el, err;

                /* Entfernung in beiden Dimensionen berechnen */
                dx = xend - xstart;
                dy = yend - ystart;

                /* Vorzeichen des Inkrements bestimmen */
                incx = Math.Sign(dx);
                incy = Math.Sign(dy);
                if (dx < 0) dx = -dx;
                if (dy < 0) dy = -dy;

                /* feststellen, welche Entfernung größer ist */
                if (dx > dy)
                {
                    /* x ist schnelle Richtung */
                    pdx = incx; pdy = 0;    /* pd. ist Parallelschritt */
                    ddx = incx; ddy = incy; /* dd. ist Diagonalschritt */
                    es = dy; el = dx;   /* Fehlerschritte schnell, langsam */
                }
                else
                {
                    /* y ist schnelle Richtung */
                    pdx = 0; pdy = incy; /* pd. ist Parallelschritt */
                    ddx = incx; ddy = incy; /* dd. ist Diagonalschritt */
                    es = dx; el = dy;   /* Fehlerschritte schnell, langsam */
                }

                /* Initialisierungen vor Schleifenbeginn */
                x = xstart;
                y = ystart;
                err = el / 2;
                this.dot(new Point(x, y));

                /* Pixel berechnen */
                for (t = 0; t < el; ++t) /* t zaehlt die Pixel, el ist auch Anzahl */
                {
                    /* Aktualisierung Fehlerterm */
                    err -= es;
                    if (err < 0)
                    {
                        /* Fehlerterm wieder positiv (>=0) machen */
                        err += el;
                        /* Schritt in langsame Richtung, Diagonalschritt */
                        x += ddx;
                        y += ddy;
                    }
                    else
                    {
                        /* Schritt in schnelle Richtung, Parallelschritt */
                        x += pdx;
                        y += pdy;
                    }
                    this.dot(new Point(x, y));
                }
            } /* gbham() */

            public Dotmatrix __lineTo(Point point)
            {
                Point origin = cursor;
                Point target = point;

                int xLow = System.Math.Min(origin.X, target.X);
                int xHight = System.Math.Max(origin.X, target.X);
                int yLow = System.Math.Min(origin.Y, target.Y);
                int yHight = System.Math.Max(origin.Y, target.Y);

                xLow = (xLow < 0) ? 0 : xLow;
                yLow = (yLow < 0) ? 0 : yLow;
                yHight = (yHight < matrixYX.Length) ? yHight : (matrixYX.Length - 1);

                for(int iY = yLow;iY <= yHight; iY++)
                {
                    xHight = (xHight < matrixYX[iY].Length) ? xHight : (matrixYX[iY].Length -1);
                    for(int iX=xLow;iX <= xHight; iX++)
                    {
                        Point dot = new Point(iX,iY);
                        if (isPointOnVector(dot, origin, target))
                        {
                            this.dot(dot);
                        }
                    }
                }
                
                moveTo(target);
                return this;
            }

            public Dotmatrix rectangle(int x, int y)
            {
                return rectangle(new Point(x,y));
            }

            public Dotmatrix rectangle(Point point)
            {
                PointList poly = new PointList();
                poly.Add(cursor);
                poly.AddPoint(point.X, cursor.Y);
                poly.Add(point);
                poly.AddPoint(cursor.X, point.Y);
                poly.Add(cursor);

                return polygon(poly);
            }

            public Dotmatrix polygon(PointList poly)
            {
                for (int i = 0; i < poly.Count; i++)
                {
                    if (i == 0)
                    {
                        moveTo(poly[i]);
                    }
                    else
                    {
                        lineTo(poly[i]);
                    }
                }

                return this;
            }

            public Dotmatrix text(String text, Font font)
            {
                char[] chars = text.ToCharArray();
                for(int i = 0; i < chars.Length; i++)
                {
                    string key = (new String(chars[i], 1)).ToUpper();
                    PointList points = new PointList();
                    if (font.getData().TryGetValue(key, out points))
                    {
                        polydot(points, true);
                        moveTo(cursor.X + font.getWidth() + 1, cursor.Y);
                    }
                }

                return this;
            }

            public Dotmatrix polydot(PointList dots, bool relative = false)
            {
                int xRel = (relative) ? cursor.X : 0;
                int yRel = (relative) ? cursor.Y : 0;
                for(int i = 0; i < dots.Count; i++)
                {
                    Point pos = new Point(dots[i].X+xRel, dots[i].Y+yRel);
                    if (isPointInViewport(pos))
                    {
                        setPixel(pos);
                    }
                }

                return this;
            }

            public Dotmatrix color(char color)
            {
                currentColor = color;

                return this;
            }

            private bool isPointInViewport(Point point)
            {
               if(0 <= point.Y && point.Y < matrixYX.Length)
                {
                    if(0 <= point.X && point.X < matrixYX[point.Y].Length)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool isPointOnVector(Point P, Point A, Point B)
            {
                if(P.Equals(A) || P.Equals(B))
                {
                    return true;
                }

                int diffABX = B.X - A.X;
                int diffABY = B.Y - A.Y;
                int diffPAX = P.X - A.X;
                int diffPAY = P.Y - A.Y;

                if(diffABX == 0)
                {
                    return A.X <= P.X && P.X <= B.X;
                }
                if(diffABY == 0)
                {
                    return A.Y <= P.Y && P.Y <= B.Y;
                }

                double eqX = (double)diffPAX / (double)diffABX;
                double eqY = (double)diffPAY / (double)diffABY;
                double diffEq = System.Math.Max(eqX,eqY) - System.Math.Min(eqX,eqY);

                return (diffEq <= pixelTolerance);
            }

            

        }

        class Font
        {
            private Dictionary<string,PointList> data = new Dictionary<string, PointList>();
            private String name;
            private int width;
            private int height;

            static public Font createFromString(String definition)
            {
                Font newFont = new Font();
                string[] fontDefinition = definition.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                for (int iFD = 0; iFD < fontDefinition.Length; iFD++)
                {
                    string[] lineData = fontDefinition[iFD].Split(' ');
                    if(lineData.Length > 0)
                    {
                        switch (lineData[0])
                        {
                            case "@FONT":
                                if (lineData.Length > 2)
                                {
                                    int w = 0;
                                    if (int.TryParse(lineData[1], out w))
                                    {
                                        newFont.setWidth(w);
                                    }
                                    else
                                    {
                                        throw new ArgumentException("no proper font width");
                                    }
                                    int h = 0;
                                    if (int.TryParse(lineData[2], out h))
                                    {
                                        newFont.setHeight(h);
                                    }
                                    else
                                    {
                                        throw new ArgumentException("no proper font height");
                                    }

                                    newFont.setName(lineData[3]);
                                }
                                break;
                            case "@ENDFONT":
                                newFont.getData().Add(" ", new PointList());
                                break;
                            default:
                                if(lineData.Length > 1)
                                {
                                    string chr = lineData[0];
                                    if (!newFont.getData().ContainsKey(chr))
                                    {
                                        newFont.getData().Add(chr, PointList.createFromBitmap(newFont.getHeight(), lineData[1]));
                                    }
                                }                                
                                break;
                        }
                    }
                }


                return newFont;
            }

            public void setWidth(int w)
            {
                width = w;
            }

            public void setHeight(int h)
            {
                height = h;
            }

            public int getWidth()
            {
                return width;
            }

            public int getHeight()
            {
                return height;
            }

            public void setName(string n)
            {
                name = n;
            }

            public Dictionary<string, PointList> getData()
            {
                return data;
            }
        }

        class PointList : List<Point>
        {
            public PointList AddPoint(int x, int y)
            {
                Add(new Point(x, y));
                return this;
            }

            static public PointList createFromBitmap(int width, string data)
            {
                PointList pl = new PointList();
                for (int i = data.IndexOf("1"); i > -1; i = data.IndexOf("1", i + 1))
                {
                    int x = 0;
                    int y = 0;
                    y = System.Math.DivRem(i, width, out x);
                    pl.AddPoint(x, y);
                }

                return pl;
            }
        }

        class Point
        {
            public int X;
            public int Y;

            public Point(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }

            public bool Equals(Point point)
            {
                return this.X == point.X && this.Y == point.Y;
            }
        }
    }
}
