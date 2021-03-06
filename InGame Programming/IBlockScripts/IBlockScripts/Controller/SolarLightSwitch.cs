﻿using System;
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
    public class SolarLightSwitch : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           SolarLightDimmer
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/SolarLightSwitch.cs
           
           Summary
           ------------------------------
           Swithcing Lights By Solarpanel Powerlevel

            Quicksstart Setup: add "!SOL_SW!" to the Lights you want to switch an call the Programmable Block without arguments.

           Abstract
           ------------------------------
            Script uses solarpanels to determine day or night an toggles lights

            Parameter: "key@powerlevel"
            key: a key in the Lightblock's Names to identify the lights.
            powerlevel: Maxoutput level of solarpanels as trigger in watt (below means lights on, above lights off)
                
           
           Example
           ------------------------------
           switch lights with "!AutoLight_01!" in Name by average powerlevel of 20 KW: Run block with argument "!AutoLight_01!@20000";
           if average Solarpower is less then 20000 Watt all lights with "!AutoLight_01!" will turn on (off if 20000 and above)

       */

        const bool DEBUG = true;

        string key = "!SOL_SW!";
        double AveragePowerMin = 15000.0;

        void Main(string args)
        {
            if(args.Length > 0)
            {
                string[] argv = args.Split('@');
                if(argv.Length == 2)
                {
                    if(argv[0].Length > 0)
                    {
                        key = argv[0];
                    }
                    if (argv[1].Length > 0)
                    {
                        double tmp = 0;
                        if(double.TryParse(argv[1], out tmp))
                        {
                            AveragePowerMin = tmp;
                        }   
                    }
                }
            }

            debug("Param: key = " + key + "; AveragePowerMin = " + AveragePowerMin.ToString());

            List<IMyTerminalBlock> SolarPanels = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> Lights = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(SolarPanels, (x => x.CubeGrid.Equals(Me.CubeGrid)));
            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(Lights, (x => x.CustomName.Contains(key) && x.CubeGrid.Equals(Me.CubeGrid)));

            debug("SolarPanels x " + SolarPanels.Count.ToString());
            debug("Lights x " + Lights.Count.ToString());

            double AveragePower = getAverageSolarPanelPowerWatt(SolarPanels);
            debug("AveragePower = " + AveragePower.ToString());


            string Action = "OnOff_Off";
            if(AveragePower < AveragePowerMin)
            {
                Action = "OnOff_On";
            }

            for(int i = 0; i < Lights.Count; i++)
            {
                Lights[i].ApplyAction(Action);
            }
        }
        

        public double getAverageSolarPanelPowerWatt(List<IMyTerminalBlock> SolarPanelList)
        {
            double PowerSum = 0;
            for (int i=0;i<SolarPanelList.Count;i++)
            {
                double cur = getSolarpanelPowerWatt(SolarPanelList[i] as IMySolarPanel);
                debug("Power ("+SolarPanelList[i].CustomName+") = " + cur.ToString());
                PowerSum += cur;
            }
            debug("PowerSum = " + PowerSum.ToString());
            return PowerSum / SolarPanelList.Count;
        }

        double getSolarpanelPowerWatt(IMySolarPanel SolarPanel)
        {
            debug("Detail (" + SolarPanel.CustomName + ") > " + SolarPanel.DetailedInfo);
            DetailedInfo DI = new DetailedInfo(SolarPanel);

            return parsePower(DI.getValue(1).getValue());
        }

        double parsePower(string value)
        {
            double result = 0;
            value = value.ToLower();
            debug("Parsepower RAW = " + value);
            int f = 1;
            if (value.Contains("g"))
            {
                f = 1000*1000*1000;
            } else if (value.Contains("m"))
            {
                f = 1000*1000;
            }
            else if (value.Contains("k"))
            {
                f = 1000;
            }
            value = value.Replace('w', ' ').Replace('k', ' ').Replace('m', ' ').Replace('g', ' ').Trim(' ');

            double numberValue = 0;
            if(double.TryParse(value, out numberValue))
            {
                result = (numberValue * f);
            }

            return result;
        }

        class DetailedInfo
        {
            private List<DetailedInfoValue> storage = new List<DetailedInfoValue>();

            public DetailedInfo(IMyTerminalBlock Block)
            {
                string[] Info = Block.DetailedInfo.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i=0; i<Info.Length; i++)
                {
                    List<string> data = new List<string>();
                    data.AddRange(Info[i].Split(':'));
                    if(data.Count > 1)
                    {
                        storage.Add(new DetailedInfoValue(data[0], String.Join(":", data.GetRange(1, data.Count - 1))));
                    }
                }
            }

            public DetailedInfoValue getValue(int index)
            {
                if(index < storage.Count && index > -1)
                {
                    return storage[index];
                }

                return null;
            }
        }

        class DetailedInfoValue
        {
            private string key;
            private string value;

            public DetailedInfoValue(string k, string v)
            {
                key = k;
                value = v;
            }

            public string getKey()
            {
                return key;
            }

            public string getValue()
            {
                return value;
            }
        }

        void debug(string msg)
        {
            if (DEBUG)
            {
                Echo(msg);
            }
        }


        #endregion
    }
}
