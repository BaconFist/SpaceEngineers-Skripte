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
           
                BaconGrafics bc = new BaconGrafics(150,150);
                bc
                    .color(BaconGrafics.COLOR_GREEN)
                    .moveTo(new Point(5,5))
                    .lineTo(new Point(30,40))
                    ;

                StringBuilder sb = bc.getImage();



            IMyTerminalBlock lcd = GridTerminalSystem.GetBlockWithName("LCD_GFX");
            if (lcd is IMyTextPanel)
            {
                (lcd as IMyTextPanel).WritePublicText(sb.ToString());
            }
        }
        

        
        class BaconGrafics
        {

            public const char COLOR_GREEN = '\uE001';
            public const char COLOR_BLUE = '\uE002';
            public const char COLOR_RED = '\uE003';
            public const char COLOR_YELLOW = '\uE004';
            public const char COLOR_WHITE = '\uE006';
            public const char COLOR_LIGHT_GRAY = '\uE00E';
            public const char COLOR_MEDIUM_GRAY = '\uE00D';
            public const char COLOR_DARK_GRAY = '\uE00F';

            private double pixelTolerance = 2;

            private int resolutionWidth;
            private int resolutionHeight;
            private char[] displayDataRaw = null;
            private char currentColor;
            private Point cursor;

            public BaconGrafics(int width, int heigth)
            {
                cursor = new Point(0, 0);
                color(COLOR_DARK_GRAY);
                resolutionWidth = width;
                resolutionHeight = heigth;                
                fillAll();
                color(COLOR_WHITE);                 
            }

            public StringBuilder getImage()
            {
                StringBuilder img = new StringBuilder();
                char[] chunk = new char[getResolutionWidth()];
                int count = getResolutionWidth();
                for(int i = 0; i < getResolutionHeight(); i++)
                {
                    int startIndex = i * getResolutionWidth();
                    Array.Copy(displayDataRaw, startIndex, chunk, 0, count);
                    img.AppendLine(new String(chunk));
                }
                
                return img;
            }

            public BaconGrafics fillAll()
            {
                displayDataRaw = new String(currentColor, getResolutionWidth() * getResolutionHeight()).ToCharArray();

                return this;
            }
            
            public int getResolutionWidth()
            {
                return resolutionWidth;
            }

            private int getXMax()
            {
                return getResolutionWidth() - 1;
            }

            private int getYMax()
            {
                return getResolutionHeight() - 1;
            }

            public int getResolutionHeight()
            {
                return resolutionHeight;
            }

            private void setCursor(Point point)
            {
                cursor = point;
            }

            private void setPixel(Point point)
            {
                int index = getValidDataIndedx(point);
                if (isIndexInRange(index))
                {
                    displayDataRaw[index] = currentColor;
                }
            }

            public BaconGrafics draw(Point point)
            {
                setPixel(point);
                return this;
            }

            public BaconGrafics moveTo(Point point)
            {
                setCursor(getNormalizedPoint(point));
                return this;
            }

            public BaconGrafics lineTo(Point point) {
                Point origin = getNormalizedPoint(cursor);
                Point target = getNormalizedPoint(point);

                int xLow = System.Math.Min(origin.X, target.X);
                int xHight = System.Math.Max(origin.X, target.X);
                int yLow = System.Math.Min(origin.Y, target.Y);
                int yHigh = System.Math.Max(origin.Y, target.Y);
                
                for(int ix= xLow; ix < xHight; ix++)
                {
                    for(int iy = yLow; iy < yHigh; iy++)
                    {
                        Point currentPos = new Point(ix,iy);
                        if (isPointOnVector(currentPos, origin, target)){
                            draw(currentPos);
                        }
                    }
                }


                moveTo(target);
                return this;
            }

            public BaconGrafics rectangle(Point point)
            {
                Polygon poly = new Polygon();
                poly.Add(cursor);
                poly.AddPoint(point.X,cursor.Y);
                poly.Add(point);
                poly.AddPoint(cursor.X,point.Y);
                poly.Add(cursor);

                return polygon(poly);
            }

            public BaconGrafics polygon(Polygon poly)
            {
                for(int i = 0; i < poly.Count; i++)
                {
                    if (i == 0)
                    {
                        moveTo(poly[i]);
                    } else
                    {
                        lineTo(poly[i]);
                    }
                }

                return this;
            }

            public BaconGrafics color(char color)
            {
                currentColor = color;

                return this;
            }

            private bool isIndexInRange(int index)
            {
                return index < displayDataRaw.Length;
            }

            private int getValidDataIndedx(Point point)
            {
                return getDataIndex(getNormalizedPoint(point));
            }

            private int getDataIndex(Point point)
            {
                return (getResolutionHeight() * point.Y) + point.X;
            }

            private Point getNormalizedPoint(Point point)
            {
                Point normalPoint = new Point(0, 0);

                normalPoint.X = (point.X < getXMax()) ? point.X : getXMax();
                normalPoint.Y = (point.Y < getYMax()) ? point.Y : getYMax();
                normalPoint.X = (point.X < 0) ? 0 : point.X;
                normalPoint.Y = (point.Y < 0) ? 0 : point.Y;

                return normalPoint;
            }

            private bool isPointOnVector(Point point, Point vecA, Point vecB)
            {
                double resX = (point.X - vecA.X) / (vecB.X - vecA.X);
                double rexY = (point.Y - vecA.Y) / (vecB.Y - vecA.Y);

                double diffX = System.Math.Max(resX, rexY) - System.Math.Min(resX, rexY);

                return (diffX <= pixelTolerance);
            }
                    
        }

        class Polygon : List<Point>
        {
            public void AddPoint(int x, int y)
            {
                Add(new Point(x,y));
            }
        }
        #endregion
    }
}
