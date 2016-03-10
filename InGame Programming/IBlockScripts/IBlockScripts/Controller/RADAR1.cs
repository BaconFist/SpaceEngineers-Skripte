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
    public class RADAR1 : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           RADAR1
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
        //Basic Settings:                                               
        int remoteRange = 200;                 // choose how far to scan with remote block       
        int sensorRange = 50;                   // choose how far to scan with sensors   
        int sensorTopExtent = 50;               // how far upwards to scan with sensors 
        int sensorBottomExtent = 50;            // how far downwards to scan with sensors 
        double sweepLineRPM = 18;                         // choose how fast the line rotates                      
        bool upsideDown = false;            // set to true if the rotor is upside down  
        string radarBlocksName = "RADAR";      // what string to search for to find radar blocks  

        //Color settings: green, blue, red, yellow, white, lightGray, mediumGray, darkGray:  
        char colorRCTargets = green;
        char colorSensorTargets = red;
        char colorInnerBackground = darkGray;
        char colorOuterBackground = darkGray;
        char colorRings = lightGray;
        char colorText = yellow;
        char colorFrontLine = yellow;
        char colorRightLine = yellow;
        char colorLeftLine = yellow;
        char colorBackLine = yellow;

        //Advanced Settings:                  
        int refreshRate = 20;               // script runs per second, for performance reasons. 60 for realtime.                                 
        int stepRange = 100;                //add or remove this much with arguments "increase" or "decrease"                  
        int screenResolution = 54;          //lower to 40 if you get script complexity errors, 160 for highest   
        bool multiScreenMode = false;       //activate to span over 4 screens (experimental)  

        // Not Settings                         
        List<IMyTerminalBlock> radarScreens = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> statScreens = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> spanDisplays = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> remotes = new List<IMyTerminalBlock>();
        //IMyRemoteControl remote;                
        IMyMotorStator rotor;
        IMySensorBlock sensorFront, sensorBack, sensorLeft, sensorRight;
        bool initialized = false;
        bool hasRotor = false;
        bool hasRemote = false;
        bool hasFrontSensor = false;
        bool hasBackSensor = false;
        bool hasSideSensor = false;
        int ticks = 0;
        string[] matrix;
        bool remoteDetecting = false;
        Vector3D remotePosition;
        bool sensorFrontDetecting = false;
        bool sensorBackDetecting = false;
        bool sensorRightDetecting = false;
        bool sensorLeftDetecting = false;
        Vector3D sensorFrontPosition, sensorBackPosition, sensorRightPosition, sensorLeftPosition;
        double remoteDistance = 0;
        double sensorFrontDistance = 0;
        double sensorBackDistance = 0;
        double sensorLeftDistance = 0;
        double sensorRightDistance = 0;
        float fontSize = 1f;
        int center;
        int[] lineXFront;
        int[] lineYFront;
        int[] oldLineXFront;
        int[] oldLineYFront;
        int[] lineXBack;
        int[] lineYBack;
        int[] oldLineXBack;
        int[] oldLineYBack;
        int[] lineXRight;
        int[] lineYRight;
        int[] oldLineXRight;
        int[] oldLineYRight;
        int[] lineXLeft;
        int[] lineYLeft;
        int[] oldLineXLeft;
        int[] oldLineYLeft;
        SortedList<string, Target> sensorScanData = new SortedList<string, Target>();
        SortedList<string, Target> remoteScanData = new SortedList<string, Target>();
        int rotorOffset = -90;
        float remoteBase = 0.1f;
        int iTimer = 0;
        int dAStep = 6;
        List<Vector3D> vecDims = new List<Vector3D>();
        double iAngle = 0;
        double iAngle2 = 0;
        double frontAngle = 0;
        double backAngle = 0;
        double leftAngle = 0;
        double rightAngle = 0;
        bool debugMode = false;
        const char green = '\uE001';
        const char blue = '\uE002';
        const char red = '\uE003';
        const char yellow = '\uE004';
        const char white = '\uE006';
        const char lightGray = '\uE00E';
        const char mediumGray = '\uE00D';
        const char darkGray = '\uE00F';

        public struct Target
        {
            public long BirthDay;
            public Vector3D Position;
            public double Distance;
        };


        void Init()
        {
            int halfRes = (int)screenResolution / 2;
            center = halfRes;
            lineXFront = new int[halfRes];
            lineYFront = new int[halfRes];
            oldLineXFront = new int[halfRes];
            oldLineYFront = new int[halfRes];
            lineXBack = new int[halfRes];
            lineYBack = new int[halfRes];
            oldLineXBack = new int[halfRes];
            oldLineYBack = new int[halfRes];
            lineXRight = new int[halfRes];
            lineYRight = new int[halfRes];
            oldLineXRight = new int[halfRes];
            oldLineYRight = new int[halfRes];
            lineXLeft = new int[halfRes];
            lineYLeft = new int[halfRes];
            oldLineXLeft = new int[halfRes];
            oldLineYLeft = new int[halfRes];
            fontSize = 0.4f;


            if (screenResolution < 41) fontSize = 0.4f;
            if (screenResolution >= 41 && screenResolution <= 59) fontSize = 0.3f;
            if (screenResolution >= 60 && screenResolution <= 85) fontSize = 0.2f;
            if (screenResolution >= 86 && screenResolution <= 160) fontSize = 0.1f;



            List<IMyTerminalBlock> lcdBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcdBlocks);

            radarScreens = lcdBlocks.FindAll(x => x.CustomName.Contains(radarBlocksName + " DISPLAY"));
            //foreach(IMyTextPanel lcd in radarScreens){ // doesn't work in .NET Framework 4.0 
            for (int i = 0; i < radarScreens.Count; i++)
            {
                IMyTextPanel lcd = radarScreens[i] as IMyTextPanel;
                lcd.SetValue<float>("FontSize", fontSize);
                lcd.SetValue<Color>("FontColor", new Color(128, 128, 128));
            }


            if (multiScreenMode)
            {
                spanDisplays = lcdBlocks.FindAll(x => x.CustomName.Contains(radarBlocksName + " MULTI"));
                //foreach(IMyTextPanel lcd in spanDisplays){ // doesn't work in .NET Framework 4.0 
                for (int i = 0; i < spanDisplays.Count; i++)
                {
                    IMyTextPanel lcd = spanDisplays[i] as IMyTextPanel;
                    lcd.SetValue<float>("FontSize", 0.6f);
                    lcd.SetValue<Color>("FontColor", new Color(128, 128, 128));
                }
            }

            statScreens = lcdBlocks.FindAll(x => x.CustomName.Contains(radarBlocksName + " STATS"));


            List<IMyTerminalBlock> remoteBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteBlocks);
            remotes = remoteBlocks.FindAll(x => x.CustomName.Contains(radarBlocksName));
            if (remotes.Count > 0) hasRemote = true;


            List<IMyTerminalBlock> rotorBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(rotorBlocks);
            if (rotorBlocks.Exists(x => x.CustomName.Contains(radarBlocksName)))
            {
                hasRotor = true;
                rotor = rotorBlocks.Find(x => x.CustomName.Contains(radarBlocksName)) as IMyMotorStator;
                rotor.SetValue<float>("Torque", 30000f);
                rotor.SetValue<float>("BrakingTorque", 30000f);
                rotor.SetValue<float>("Velocity", (float)sweepLineRPM);
            }

            List<IMyTerminalBlock> sensorBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensorBlocks);
            if (sensorBlocks.Exists(x => x.CustomName.Contains(radarBlocksName + " FRONT")))
            {
                hasFrontSensor = true;
                sensorFront = sensorBlocks.Find(x => x.CustomName.Contains(radarBlocksName + " FRONT")) as IMySensorBlock;
                sensorFront.SetValue<float>("Left", 1f);
                sensorFront.SetValue<float>("Right", 1f);
                sensorFront.SetValue<float>("Top", (float)sensorTopExtent);
                sensorFront.SetValue<float>("Bottom", (float)sensorBottomExtent);
                sensorFront.SetValue<float>("Back", 1f);
                sensorFront.SetValue<float>("Front", (float)sensorRange);
            }
            if (sensorBlocks.Exists(x => x.CustomName.Contains(radarBlocksName + " BACK")))
            {
                hasBackSensor = true;
                sensorBack = sensorBlocks.Find(x => x.CustomName.Contains(radarBlocksName + " BACK")) as IMySensorBlock;
                sensorBack.SetValue<float>("Left", 1f);
                sensorBack.SetValue<float>("Right", 1f);
                sensorBack.SetValue<float>("Top", (float)sensorTopExtent);
                sensorBack.SetValue<float>("Bottom", (float)sensorBottomExtent);
                sensorBack.SetValue<float>("Back", 1f);
                sensorBack.SetValue<float>("Front", sensorRange);
            }
            if (sensorBlocks.Exists(x => x.CustomName.Contains(radarBlocksName + " LEFT")) && sensorBlocks.Exists(x => x.CustomName.Contains(radarBlocksName + " RIGHT")))
            {
                hasSideSensor = true;
                sensorLeft = sensorBlocks.Find(x => x.CustomName.Contains(radarBlocksName + " LEFT")) as IMySensorBlock;
                sensorLeft.SetValue<float>("Left", 1f);
                sensorLeft.SetValue<float>("Right", 1f);
                sensorLeft.SetValue<float>("Top", (float)sensorTopExtent);
                sensorLeft.SetValue<float>("Bottom", (float)sensorBottomExtent);
                sensorLeft.SetValue<float>("Back", 1f);
                sensorLeft.SetValue<float>("Front", sensorRange);

                sensorRight = sensorBlocks.Find(x => x.CustomName.Contains(radarBlocksName + " RIGHT")) as IMySensorBlock;
                sensorRight.SetValue<float>("Left", 1f);
                sensorRight.SetValue<float>("Right", 1f);
                sensorRight.SetValue<float>("Top", (float)sensorTopExtent);
                sensorRight.SetValue<float>("Bottom", (float)sensorBottomExtent);
                sensorRight.SetValue<float>("Back", 1f);
                sensorRight.SetValue<float>("Front", sensorRange);
            }



            matrix = new string[screenResolution];

            for (int a = 0; a < screenResolution; a++)
            {
                matrix[a] = "";
                for (int b = 0; b < screenResolution; b++)
                {
                    matrix[a] += colorOuterBackground;
                }
                matrix[a] += "\n";
            }
            Refresh();
            initialized = true;
        }


        //Thank you Skleroz                   
        Vector3D GetVector(Vector3D vecFront, Vector3D vecPoM, double dFrontAng, double dPoMAng)
        {
            Vector3D vecRes = vecFront;
            if (vecFront.Dot(vecPoM) < 0.001 && Math.Abs(Math.Cos(dFrontAng)) + Math.Abs(Math.Cos(dPoMAng)) > 0.995)
            {
                vecFront = vecFront * Math.Cos(dFrontAng);
                vecPoM = vecPoM * Math.Cos(dPoMAng);
                vecRes = Vector3D.Add(vecFront, vecPoM);
                vecRes.Normalize();
            }
            return vecRes;
        }


        void Render(List<IMyTerminalBlock> screens, string str)
        {
            //foreach(IMyTextPanel lcd in screens){ // doesn't work in .NET Framework 4.0 
            for (int i = 0; i < screens.Count; i++)
            {
                IMyTextPanel lcd = screens[i] as IMyTextPanel;
                lcd.WritePublicText(str);
                lcd.ShowTextureOnScreen();
                lcd.ShowPublicTextOnScreen();
            }
        }

        void RenderMulti(List<IMyTerminalBlock> screens, string str)
        {
            string topLeft = "";
            string topRight = "";
            string bottomLeft = "";
            string bottomRight = "";

            List<string> txtArray = new List<string>(str.Split('\n'));
            int totalEnd = txtArray.Count;

            for (int i = 0; i < totalEnd; i++)
            {
                if (i <= totalEnd / 2)
                {
                    topLeft += txtArray[i].Substring(0, txtArray[i].Length / 2) + "\n";
                    topRight += txtArray[i].Substring((txtArray[i].Length / 2)) + "\n";
                }
                else if (i > totalEnd / 2)
                {
                    bottomLeft += txtArray[i].Substring(0, txtArray[i].Length / 2) + "\n";
                    bottomRight += txtArray[i].Substring(txtArray[i].Length / 2) + "\n";
                }
            }

            //foreach(IMyTextPanel lcd in screens){ // doesn't work in .NET Framework 4.0 
            for (int i = 0; i < screens.Count; i++)
            {
                IMyTextPanel lcd = screens[i] as IMyTextPanel;
                if (lcd.CustomName.Contains(radarBlocksName + " MULTI1")) lcd.WritePublicText(topLeft);
                if (lcd.CustomName.Contains(radarBlocksName + " MULTI2")) lcd.WritePublicText(topRight);
                if (lcd.CustomName.Contains(radarBlocksName + " MULTI3")) lcd.WritePublicText(bottomLeft);
                if (lcd.CustomName.Contains(radarBlocksName + " MULTI4")) lcd.WritePublicText(bottomRight);
                lcd.ShowTextureOnScreen();
                lcd.ShowPublicTextOnScreen();
            }

        }

        void Plot(int x, int y, char c)
        {
            if (x >= 0 && y >= 0 && x < screenResolution && y < screenResolution)
            {
                x = x;
                char[] chars = matrix[y].ToCharArray();
                chars[x] = c;
                matrix[y] = new string(chars);
            }
        }

        void PlotNumber(int startX, int startY, int number)
        {
            if (number == 1)
            {
                Plot(startX + 1, startY, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY, colorOuterBackground);
                Plot(startX, startY + 1, colorOuterBackground);
                Plot(startX, startY + 2, colorOuterBackground);
                Plot(startX, startY + 3, colorOuterBackground);
            }
            if (number == 2)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY + 1, colorOuterBackground);
                Plot(startX + 1, startY + 2, colorOuterBackground);
            }
            if (number == 3)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY + 1, colorOuterBackground);
                Plot(startX, startY + 2, colorOuterBackground);
            }
            if (number == 4)
            {
                Plot(startX, startY, colorText);
                Plot(startX, startY + 1, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX + 1, startY, colorOuterBackground);
                Plot(startX, startY + 2, colorOuterBackground);
                Plot(startX, startY + 3, colorOuterBackground);
            }
            if (number == 5)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX + 1, startY + 1, colorOuterBackground);
                Plot(startX, startY + 2, colorOuterBackground);
            }
            if (number == 6)
            {
                Plot(startX + 1, startY, colorText);
                Plot(startX, startY + 1, colorText);
                Plot(startX, startY + 2, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY, colorOuterBackground);
                Plot(startX + 1, startY + 1, colorOuterBackground);
            }
            if (number == 7)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY + 1, colorOuterBackground);
                Plot(startX, startY + 2, colorOuterBackground);
                Plot(startX, startY + 3, colorOuterBackground);
            }
            if (number == 8)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX, startY + 2, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY + 1, colorOuterBackground);
                Plot(startX + 1, startY + 1, colorOuterBackground);
            }
            if (number == 9)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX, startY + 1, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX + 1, startY + 3, colorText);

                Plot(startX, startY + 2, colorOuterBackground);
                Plot(startX, startY + 3, colorOuterBackground);

            }
            if (number == 0)
            {
                Plot(startX, startY, colorText);
                Plot(startX + 1, startY, colorText);
                Plot(startX, startY + 1, colorText);
                Plot(startX + 1, startY + 1, colorText);
                Plot(startX, startY + 2, colorText);
                Plot(startX + 1, startY + 2, colorText);
                Plot(startX, startY + 3, colorText);
                Plot(startX + 1, startY + 3, colorText);
            }
        }

        void PlotBigNumber(int startX, int startY, int num)
        {
            startX = startX - 3;
            List<int> listOfInts = new List<int>();
            while (num > 0)
            {
                listOfInts.Add(num % 10);
                num = num / 10;
            }
            listOfInts.Reverse();

            for (int i = 0; i < listOfInts.Count; i++)
            {
                PlotNumber(startX + (i * 3), startY, listOfInts[i]);
            }

        }

        void ArgumentHandler(string args)
        {
            args = args.ToLower();

            if (args == "increase")
            {
                remoteRange = remoteRange + stepRange;
            }
            if (args == "decrease")
            {
                if (remoteRange - stepRange > 0)
                    remoteRange = remoteRange - stepRange;
            }

        }

        double GetDistance(Vector3D target)
        {
            Vector3D myPos = remotes[0].GetPosition();
            double distance = (target - myPos).Length();
            return distance;
        }



        void Refresh()
        {
            if (radarScreens.Count > 0 || spanDisplays.Count >= 4)
            {
                string output = "\n\n";
                if (hasRemote)
                {
                    for (int i = 0; i < remoteScanData.Count; i++)
                    {
                        string key = remoteScanData.Keys[i];
                        int x = Int32.Parse(key.Split(':')[0]);
                        int y = Int32.Parse(key.Split(':')[1]);
                        Target target = remoteScanData.Values[i];
                        TimeSpan age = new TimeSpan(DateTime.Now.Ticks - target.BirthDay);
                        if (age.TotalSeconds < (int)(1 / (sweepLineRPM / 60))) Plot(x, y, colorRCTargets);
                    }
                }
                if (hasFrontSensor || hasBackSensor || hasSideSensor)
                {
                    for (int i = 0; i < sensorScanData.Count; i++)
                    {
                        string key = sensorScanData.Keys[i];
                        int x = Int32.Parse(key.Split(':')[0]);
                        int y = Int32.Parse(key.Split(':')[1]);
                        Target target = sensorScanData.Values[i];
                        TimeSpan age = new TimeSpan(DateTime.Now.Ticks - target.BirthDay);
                        if (age.TotalSeconds == 0) Plot(x, y, colorSensorTargets);
                    }
                }

                for (int i = 0; i < screenResolution; i++)
                {
                    output += matrix[i];
                }

                Render(radarScreens, output);
                if (multiScreenMode) RenderMulti(spanDisplays, output);

            }

            if (statScreens.Count > 0)
            {
                string statsOutput = "[RC Block: " + hasRemote.ToString() + "] [Front Sensor: " + hasFrontSensor.ToString() + "] [Back Sensor: " + hasBackSensor.ToString() + "] [Side Sensors: " + hasSideSensor.ToString() + "]\n\n";
                if (hasRemote)
                {
                    statsOutput += "Remote Control Block Scan Data:\n";

                    for (int i = 0; i < remoteScanData.Count; i++)
                    {
                        TimeSpan age = new TimeSpan(DateTime.Now.Ticks - remoteScanData.Values[i].BirthDay);
                        if (age.TotalSeconds < 10)
                        {
                            Vector3D detectedpos = remoteScanData.Values[i].Position;
                            string gps = "GPS:RC Target " + i.ToString() + ":" + Math.Round(detectedpos.GetDim(0), 2).ToString() + ":" + Math.Round(detectedpos.GetDim(1), 2).ToString() + ":" + Math.Round(detectedpos.GetDim(2), 2).ToString() + ":";
                            string dist = ((int)remoteScanData.Values[i].Distance).ToString();
                            statsOutput += "[" + gps + "] [Distance: " + dist + "m] [Last seen: " + ((int)age.TotalSeconds).ToString() + "s]\n";
                        }
                    }
                }
                if (hasFrontSensor || hasBackSensor || hasSideSensor)
                {

                    statsOutput += "\nSensor Scan Data:\n";

                    for (int i = 0; i < sensorScanData.Count; i++)
                    {
                        TimeSpan age = new TimeSpan(DateTime.Now.Ticks - sensorScanData.Values[i].BirthDay);
                        if (age.TotalSeconds < 10)
                        {
                            Vector3D detectedpos = sensorScanData.Values[i].Position;
                            string gps = "GPS:Sensor Target " + i.ToString() + ":" + Math.Round(detectedpos.GetDim(0), 2).ToString() + ":" + Math.Round(detectedpos.GetDim(1), 2).ToString() + ":" + Math.Round(detectedpos.GetDim(2), 2).ToString() + ":";
                            string dist = ((int)sensorScanData.Values[i].Distance).ToString();
                            statsOutput += "[" + gps + "] [Distance: " + dist + "m] [Last seen: " + ((int)age.TotalSeconds).ToString() + "s]\n";
                        }
                    }
                }
                Render(statScreens, statsOutput);
            }
        }

        void ScanSensors()
        {
            if (hasFrontSensor)
            {
                var entityFront = sensorFront.LastDetectedEntity;
                if (entityFront != null)
                {
                    sensorFrontDetecting = true;
                    sensorFrontPosition = entityFront.GetPosition();
                    sensorFrontDistance = SensorDistance(sensorFrontPosition);
                }
                else
                {
                    sensorFrontDetecting = false;
                }
            }
            if (hasBackSensor)
            {
                var entityBack = sensorBack.LastDetectedEntity;
                if (entityBack != null)
                {
                    sensorBackDetecting = true;
                    sensorBackPosition = entityBack.GetPosition();
                    sensorBackDistance = SensorDistance(sensorBackPosition);
                }
                else
                {
                    sensorBackDetecting = false;
                }
            }
            if (hasSideSensor)
            {
                var entityRight = sensorRight.LastDetectedEntity;
                var entityLeft = sensorLeft.LastDetectedEntity;
                if (entityRight != null)
                {
                    sensorRightDetecting = true;
                    sensorRightPosition = entityRight.GetPosition();
                    sensorRightDistance = SensorDistance(sensorRightPosition);
                }
                else
                {
                    sensorRightDetecting = false;
                }
                if (entityLeft != null)
                {
                    sensorLeftDetecting = true;
                    sensorLeftPosition = entityLeft.GetPosition();
                    sensorLeftDistance = SensorDistance(sensorLeftPosition);
                }
                else
                {
                    sensorLeftDetecting = false;
                }
            }
        }

        double SensorDistance(Vector3D target)
        {
            Vector3D myPos = rotor.GetPosition();
            double distance = (target - myPos).Length();
            return distance;
        }


        //I stole this from Pennywise, customized for my purposes by Skleroz                         
        void ScanRemote(IMyRemoteControl remote, Vector3D vecF, float Distance, float Base = 1.0f)
        {
            Vector3D MyPos, O1, O2, F1, F2, F3;
            MyPos = remote.GetPosition();
            if (vecF.GetDim(0) == 0)
            {
                O1 = MyPos + remote.WorldMatrix.Forward;
            }
            else
            {
                vecF.Normalize();
                O1 = MyPos + vecF;
            }
            if (Base == 1.0f)
            {
                Base = Distance * (float)Math.Asin(10 / (float)sweepLineRPM / 180 * Math.PI) * 2.0f;
            }

            F1 = remote.GetFreeDestination(O1, Distance, Base);
            if (O1 != F1)
            {
                O1 = Vector3D.Normalize(O1 - MyPos) * (F1 - MyPos).Length() + MyPos;
                O2 = Vector3D.Normalize(F1 - O1);
                F2 = remote.GetFreeDestination(O1 - O2, Distance, Base);
                F3 = remote.GetFreeDestination(O1 - (O2 * 2), Distance, Base);
                double sig;
                remotePosition = CircleBy3Points(F1, F2, F3);
            }
            else
            {
                remotePosition = MyPos + (O1 - MyPos) * Distance;
            }
        }

        //I stole this from Pennywise                               
        Vector3D CircleBy3Points(Vector3D p1, Vector3D p2, Vector3D p3)
        {
            Vector3D Center = new Vector3D(0, 0, 0);
            Vector3D t = p2 - p1;
            Vector3D u = p3 - p1;
            Vector3D v = p3 - p2;
            Vector3D w = Vector3D.Cross(t, u);
            double wsl = Math.Pow(w.Length(), 2);
            if (wsl < 10e-14)
            {
                return Center;
            }
            else
            {
                double iwsl2 = 1.0 / (2.0 * wsl);
                double tt = Vector3D.Dot(t, t);
                double uu = Vector3D.Dot(u, u);
                Center = p1 + (u * tt * Vector3D.Dot(u, v) - t * uu * Vector3D.Dot(t, v)) * iwsl2;
                return Center;
            }
        }

        void DrawSweepingLine()
        {
            for (int i = 0; i < screenResolution / 2; i++)
            {
                lineXFront[i] = (int)(center + (Math.Cos(iAngle) * i));
                lineYFront[i] = (int)(center + (Math.Sin(iAngle) * i));

                if (hasBackSensor)
                {
                    lineXBack[i] = (int)(center + (Math.Cos(backAngle) * i));
                    lineYBack[i] = (int)(center + (Math.Sin(backAngle) * i));

                    Plot(oldLineXBack[i], oldLineYBack[i], colorInnerBackground);
                    Plot(lineXBack[i], lineYBack[i], colorBackLine); //visible line   
                }
                if (hasSideSensor)
                {
                    lineXRight[i] = (int)(center + (Math.Cos(rightAngle) * i));
                    lineYRight[i] = (int)(center + (Math.Sin(rightAngle) * i));
                    lineXLeft[i] = (int)(center + (Math.Cos(leftAngle) * i));
                    lineYLeft[i] = (int)(center + (Math.Sin(leftAngle) * i));

                    Plot(oldLineXRight[i], oldLineYRight[i], colorInnerBackground);
                    Plot(oldLineXLeft[i], oldLineYLeft[i], colorInnerBackground);
                    Plot(lineXRight[i], lineYRight[i], colorRightLine); //visible line   
                    Plot(lineXLeft[i], lineYLeft[i], colorLeftLine); //visible line   
                }

                //range circles         
                int ring1 = (int)(screenResolution * 0.25 / 2);
                int ring2 = (int)(screenResolution * 0.50 / 2);
                int ring3 = (int)(screenResolution * 0.75 / 2);
                int ring4 = (int)(screenResolution / 2 - 1);

                if (i == ring1 || i == ring2 || i == ring3 || i == ring4)
                {
                    Plot(oldLineXFront[i], oldLineYFront[i], colorRings);
                    if (hasBackSensor) Plot(oldLineXBack[i], oldLineYBack[i], colorRings);
                    if (hasSideSensor)
                    {
                        Plot(oldLineXRight[i], oldLineYRight[i], colorRings);
                        Plot(oldLineXLeft[i], oldLineYLeft[i], colorRings);
                    }
                }
                else Plot(oldLineXFront[i], oldLineYFront[i], colorInnerBackground);

                Plot(lineXFront[i], lineYFront[i], colorFrontLine); //visible line rc block  


                oldLineXFront[i] = lineXFront[i];
                oldLineYFront[i] = lineYFront[i];

                if (hasBackSensor)
                {
                    oldLineXBack[i] = lineXBack[i];
                    oldLineYBack[i] = lineYBack[i];
                }
                if (hasSideSensor)
                {
                    oldLineXRight[i] = lineXRight[i];
                    oldLineYRight[i] = lineYRight[i];
                    oldLineXLeft[i] = lineXLeft[i];
                    oldLineYLeft[i] = lineYLeft[i];
                }
            }
        }


        void Main(string arguments)
        {
            if (arguments != "") ArgumentHandler(arguments);

            if (initialized == false) Init();

            if (hasRotor == false)
            {
                //Thank you Skleroz      
                double dRPM = 60 * 60 / sweepLineRPM;
                if (iTimer >= dRPM)
                {
                    iTimer = 0;
                }
                else
                {
                    iTimer = iTimer + 1;
                }
                iAngle = 2.0f * Math.PI * iTimer / dRPM + MathHelper.ToRadians(rotorOffset);
                iAngle2 = 2.0f * Math.PI * iTimer / dRPM;
            }
            if (hasRotor == true)
            {
                iAngle = MathHelper.ToRadians(MathHelper.ToDegrees(rotor.Angle) + rotorOffset % 360);
                iAngle2 = rotor.Angle;
                if (upsideDown) iAngle = MathHelper.ToRadians(180) - iAngle;
            }
            int dAngle = (int)MathHelper.ToDegrees(iAngle);
            frontAngle = iAngle;
            rightAngle = MathHelper.ToRadians(dAngle + 90 % 360);
            backAngle = MathHelper.ToRadians(dAngle + 180 % 360);
            leftAngle = MathHelper.ToRadians(dAngle + 270 % 360);

            //ticks/////////////////////////////////               
            ticks++;
            if (ticks == 60 / refreshRate)
            {
                ticks = 0;

                if (hasRemote)
                {
                    //Thank you Skleroz      

                    //Vector3D remotePosition = ScanRemote(vecCheck,(float)remoteRange,remoteBase);  
                    for (int i = 0; i < remotes.Count; i++)
                    {
                        Vector3D vecCheck = GetVector(remotes[i].WorldMatrix.Forward, remotes[i].WorldMatrix.Right, iAngle2, iAngle);
                        ScanRemote((IMyRemoteControl)remotes[i], vecCheck, (float)remoteRange, remoteBase);
                        remoteDistance = GetDistance(remotePosition);

                        if (remoteDistance >= remoteRange - 5)
                        {
                            remoteDetecting = false;
                        }
                        else
                        {
                            remoteDetecting = true;
                        }

                        if (remoteDetecting == true)
                        {
                            double x = center + ((Math.Cos(iAngle) * remoteDistance) / (remoteRange / (screenResolution / 2)));  //divide to downsize ratio, to fit on screen                            
                            double y = center + ((Math.Sin(iAngle) * remoteDistance) / (remoteRange / (screenResolution / 2)));
                            string sCoordinates = ((int)x).ToString() + ":" + ((int)y).ToString();
                            Target t;
                            t.BirthDay = DateTime.Now.Ticks;
                            t.Distance = remoteDistance;
                            t.Position = remotePosition;
                            if (remoteScanData.ContainsKey(sCoordinates) == false)
                            {
                                remoteScanData.Add(sCoordinates, t);
                            }
                            else
                            {
                                remoteScanData[sCoordinates] = t;
                            }

                        }
                    }

                }
                if (hasFrontSensor || hasBackSensor || hasSideSensor)
                {
                    ScanSensors();
                    if (sensorFrontDetecting == true)
                    {
                        int x = center + (int)((Math.Cos(frontAngle) * sensorFrontDistance) / (sensorRange / (screenResolution / 2)));  //divide to downsize ratio, to fit on screen                            
                        int y = center + (int)((Math.Sin(frontAngle) * sensorFrontDistance) / (sensorRange / (screenResolution / 2)));
                        string sCoordinates = x.ToString() + ":" + y.ToString();
                        Target t;
                        t.BirthDay = DateTime.Now.Ticks;
                        t.Distance = sensorFrontDistance;
                        t.Position = sensorFrontPosition;
                        if (sensorScanData.ContainsKey(sCoordinates) == false)
                        {
                            sensorScanData.Add(sCoordinates, t);
                        }
                        else
                        {
                            sensorScanData[sCoordinates] = t;
                        }
                    }
                    if (sensorBackDetecting == true)
                    {
                        int x = center + (int)((Math.Cos(backAngle) * sensorBackDistance) / (sensorRange / (screenResolution / 2)));  //divide to downsize ratio, to fit on screen                            
                        int y = center + (int)((Math.Sin(backAngle) * sensorBackDistance) / (sensorRange / (screenResolution / 2)));
                        string sCoordinates = ((int)x).ToString() + ":" + ((int)y).ToString();
                        Target t;
                        t.BirthDay = DateTime.Now.Ticks;
                        t.Distance = sensorBackDistance;
                        t.Position = sensorBackPosition;
                        if (sensorScanData.ContainsKey(sCoordinates) == false)
                        {
                            sensorScanData.Add(sCoordinates, t);
                        }
                        else
                        {
                            sensorScanData[sCoordinates] = t;
                        }
                    }
                    if (sensorLeftDetecting == true)
                    {
                        int x = center + (int)((Math.Cos(leftAngle) * sensorLeftDistance) / (sensorRange / (screenResolution / 2)));  //divide to downsize ratio, to fit on screen                            
                        int y = center + (int)((Math.Sin(leftAngle) * sensorLeftDistance) / (sensorRange / (screenResolution / 2)));
                        string sCoordinates = ((int)x).ToString() + ":" + ((int)y).ToString();
                        Target t;
                        t.BirthDay = DateTime.Now.Ticks;
                        t.Distance = sensorLeftDistance;
                        t.Position = sensorLeftPosition;
                        if (sensorScanData.ContainsKey(sCoordinates) == false)
                        {
                            sensorScanData.Add(sCoordinates, t);
                        }
                        else
                        {
                            sensorScanData[sCoordinates] = t;
                        }
                    }
                    if (sensorRightDetecting == true)
                    {
                        int x = center + (int)((Math.Cos(rightAngle) * sensorRightDistance) / (sensorRange / (screenResolution / 2)));  //divide to downsize ratio, to fit on screen                            
                        int y = center + (int)((Math.Sin(rightAngle) * sensorRightDistance) / (sensorRange / (screenResolution / 2)));
                        string sCoordinates = ((int)x).ToString() + ":" + ((int)y).ToString();
                        Target t;
                        t.BirthDay = DateTime.Now.Ticks;
                        t.Distance = sensorRightDistance;
                        t.Position = sensorRightPosition;
                        if (sensorScanData.ContainsKey(sCoordinates) == false)
                        {
                            sensorScanData.Add(sCoordinates, t);
                        }
                        else
                        {
                            sensorScanData[sCoordinates] = t;
                        }
                    }
                }


                if (hasRemote) PlotBigNumber(4, 0, remoteRange);
                if (hasFrontSensor || hasBackSensor || hasSideSensor) PlotBigNumber(4, 5, sensorRange);

            }//ticks /////////////////////////       

            DrawSweepingLine();
            Refresh();
        }

        #endregion
    }
}
