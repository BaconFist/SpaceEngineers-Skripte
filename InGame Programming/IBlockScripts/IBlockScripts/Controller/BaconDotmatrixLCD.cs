using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
////using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class BaconDisplayDriver : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BaconDisplayDriver
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


            Dotmatrix bc = new Dotmatrix(50, 50, Dotmatrix.COLOR_DARK_GRAY);
            bc
                .color(Dotmatrix.COLOR_GREEN)
                .polygon(
                    (new Polygon())
                    .AddPoint(9, 2)
                    .AddPoint(26, 2)
                    .AddPoint(32, 3)
                    .AddPoint(38, 6)
                    .AddPoint(42, 12)
                    .AddPoint(38, 36)
                    .AddPoint(28, 44)
                    .AddPoint(9, 44)
                    .AddPoint(5, 40)
                    .AddPoint(2, 32)
                    .AddPoint(2, 4)
                    .AddPoint(4, 5)
                    .AddPoint(9, 2)
                )
                .polygon(
                    (new Polygon())
                    .AddPoint(12, 11)
                    .AddPoint(10, 30)
                    .AddPoint(26, 32)
                    .AddPoint(12, 11)
                )
                .polygon(
                    (new Polygon())
                    .AddPoint(4, 32)
                    .AddPoint(9, 38)
                    .AddPoint(28, 38)
                    .AddPoint(38, 32)
                )
                .moveTo(6, 7)
                .rectangle(9, 10)
                .moveTo(28, 7)
                .rectangle(34, 10)
            ;

            StringBuilder sb = bc.getImage();



            IMyTerminalBlock lcd = GridTerminalSystem.GetBlockWithName("LCD_GFX");
            if (lcd is IMyTextPanel)
            {
                (lcd as IMyTextPanel).WritePublicText(sb.ToString());
            }
        }


        class Dotmatrix
        {

            public const char COLOR_GREEN = '\uE001';
            //public const char COLOR_GREEN = 'X';
            public const char COLOR_BLUE = '\uE002';
            public const char COLOR_RED = '\uE003';
            public const char COLOR_YELLOW = '\uE004';
            public const char COLOR_WHITE = '\uE006';
            public const char COLOR_LIGHT_GRAY = '\uE00E';
            public const char COLOR_MEDIUM_GRAY = '\uE00D';
            public const char COLOR_DARK_GRAY = '\uE00F';
            //public const char COLOR_DARK_GRAY = '-';

            private double pixelTolerance = 0.04;

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
                for (int i = 0; i < matrixYX.Length; i++)
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

            public Dotmatrix draw(int x, int y)
            {
                return draw(new Point(x, y));
            }

            public Dotmatrix draw(Point point)
            {
                setPixel(point);
                return this;
            }

            public Dotmatrix moveTo(int x, int y)
            {
                return moveTo(new Point(x, y));
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
                Point origin = cursor;
                Point target = point;

                int xLow = System.Math.Min(origin.X, target.X);
                int xHight = System.Math.Max(origin.X, target.X);
                int yLow = System.Math.Min(origin.Y, target.Y);
                int yHight = System.Math.Max(origin.Y, target.Y);

                xLow = (xLow < 0) ? 0 : xLow;
                yLow = (yLow < 0) ? 0 : yLow;
                yHight = (yHight < matrixYX.Length) ? yHight : (matrixYX.Length - 1);

                for (int iY = yLow; iY <= yHight; iY++)
                {
                    xHight = (xHight < matrixYX[iY].Length) ? xHight : (matrixYX[iY].Length - 1);
                    for (int iX = xLow; iX <= xHight; iX++)
                    {
                        Point dot = new Point(iX, iY);
                        if (isPointOnVector(dot, origin, target))
                        {
                            draw(dot);
                        }
                    }
                }

                moveTo(target);
                return this;
            }

            public Dotmatrix rectangle(int x, int y)
            {
                return rectangle(new Point(x, y));
            }

            public Dotmatrix rectangle(Point point)
            {
                Polygon poly = new Polygon();
                poly.Add(cursor);
                poly.AddPoint(point.X, cursor.Y);
                poly.Add(point);
                poly.AddPoint(cursor.X, point.Y);
                poly.Add(cursor);

                return polygon(poly);
            }

            public Dotmatrix polygon(Polygon poly)
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

            public Dotmatrix color(char color)
            {
                currentColor = color;

                return this;
            }

            private bool isPointInViewport(Point point)
            {
                if (0 <= point.Y && point.Y < matrixYX.Length)
                {
                    if (0 <= point.X && point.X < matrixYX[point.Y].Length)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool isPointOnVector(Point P, Point A, Point B)
            {
                if (P.Equals(A) || P.Equals(B))
                {
                    return true;
                }

                int diffABX = B.X - A.X;
                int diffABY = B.Y - A.Y;
                int diffPAX = P.X - A.X;
                int diffPAY = P.Y - A.Y;

                if (diffABX == 0)
                {
                    return A.X <= P.X && P.X <= B.X;
                }
                if (diffABY == 0)
                {
                    return A.Y <= P.Y && P.Y <= B.Y;
                }

                double eqX = (double)diffPAX / (double)diffABX;
                double eqY = (double)diffPAY / (double)diffABY;
                double diffEq = System.Math.Max(eqX, eqY) - System.Math.Min(eqX, eqY);

                return (diffEq <= pixelTolerance);
            }



        }

        class Polygon : List<Point>
        {
            public Polygon AddPoint(int x, int y)
            {
                Point P = new Point(x,y);
                Add(P);
                return this;
            }
        }

        //class Point
        //{
        //    public int X;
        //    public int Y;

        //    public Point(int X, int Y)
        //    {
        //        this.X = X;
        //        this.Y = Y;
        //    }

        //    public bool Equals(Point point)
        //    {
        //        return this.X == point.X && this.Y == point.Y;
        //    }
        //}
        #endregion
    }
}
