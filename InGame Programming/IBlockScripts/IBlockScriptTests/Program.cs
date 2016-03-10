using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBlockScriptTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BaconGFX bc = new BaconGFX(60, 20, BaconGFX.COLOR_DARK_GRAY);
            bc
                .color(BaconGFX.COLOR_GREEN)
                .polygon(
                    (new Polygon())
                    .AddPoint(0,10)
                    .AddPoint(30,0)
                    .AddPoint(59,10)
                    .AddPoint(30,19)
                    .AddPoint(0,10)
                )
                .moveTo(3,3)
                .rectangle(56,16)                
            ;

            StringBuilder sb = bc.getImage();
            System.Console.Write(sb.ToString());
            System.Console.WriteLine("press any key to exit");
            System.Console.ReadKey();
        }

        class BaconGFX
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

            private double pixelTolerance = 0.04;

            private int width;
            private int height;
            private char[][] matrixYX;
            private char currentColor;
            private Point cursor;

            public BaconGFX(int width, int height, char background)
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

            public BaconGFX fillAll()
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

            public BaconGFX draw(Point point)
            {
                setPixel(point);
                return this;
            }

            public BaconGFX moveTo(int x, int y)
            {
                return moveTo(new Point(x,y));
            }

            public BaconGFX moveTo(Point point)
            {
                setCursor(point);
                return this;
            }

            public BaconGFX lineTo(int x, int y)
            {
                return lineTo(new Point(x, y));
            }

            public BaconGFX lineTo(Point point)
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
                            draw(dot);
                        }
                    }
                }
                
                moveTo(target);
                return this;
            }

            public BaconGFX rectangle(int x, int y)
            {
                return rectangle(new Point(x,y));
            }

            public BaconGFX rectangle(Point point)
            {
                Polygon poly = new Polygon();
                poly.Add(cursor);
                poly.AddPoint(point.X, cursor.Y);
                poly.Add(point);
                poly.AddPoint(cursor.X, point.Y);
                poly.Add(cursor);

                return polygon(poly);
            }

            public BaconGFX polygon(Polygon poly)
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

            public BaconGFX color(char color)
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

        class Polygon : List<Point>
        {
            public Polygon AddPoint(int x, int y)
            {
                Add(new Point(x, y));
                return this;
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
