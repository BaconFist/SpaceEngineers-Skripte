using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class BaconPowerManager : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BaconPowerManager
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

        string CONF_TAG_EMERGENCY_POWER = "!BPM_EP!";
        double CONF_LIMIT_LOW = 20.0;
        double CONF_LIMIT_HIGH = 80.0;

        const int BATTERY_VALUE_INDEX_MAX = 3;
        const int BATTERY_VALUE_INDEX_STORED = 6;

        void Main(string args)
        {
            resolveArgs(args);
            List<IMyTerminalBlock> EmergPowerBlocks = getEmergencyPowerBlocksOnGrid(Me.CubeGrid, CONF_TAG_EMERGENCY_POWER);
            double averageStored = getAverageLoadingState(getCausalBatteriesOnGrid(Me.CubeGrid, EmergPowerBlocks));
            
            if(averageStored <= CONF_LIMIT_LOW)
            {
                enableAll(EmergPowerBlocks);
            } 
            if(averageStored >= CONF_LIMIT_HIGH)
            {
                disableAll(EmergPowerBlocks);
            }
        }


        private void disableAll(List<IMyTerminalBlock> Blocks)
        {

        }

        private void enableAll(List<IMyTerminalBlock> Blocks)
        {

        }

        private void resolveArgs(string args)
        {
            if(args.Length > 0)
            {
                string[] argv = args.Split(';');
                for(int i = 0; i < argv.Length; i++)
                {
                    string[] tmp = argv[i].Split('=');
                    if(tmp.Length == 1)
                    {
                        CONF_TAG_EMERGENCY_POWER = tmp[0];
                    } else if (tmp.Length == 2)
                    {
                        if (tmp[0].ToLower().Equals("min"))
                        {
                            double tmpMin = 0;
                            if(double.TryParse(tmp[1] , out tmpMin) && tmpMin <= 100 && tmpMin >= 0)
                            {
                                CONF_LIMIT_LOW = tmpMin;
                            }
                        } else if (tmp[0].ToLower().Equals("max"))
                        {
                            double tmpMax = 0;
                            if (double.TryParse(tmp[1], out tmpMax) && tmpMax <= 100 && tmpMax >= 0)
                            {
                                CONF_LIMIT_HIGH = tmpMax;
                            }
                        }
                    }
                }
            }
        }

        private double getAverageLoadingState(List<IMyBatteryBlock> Batteries)
        {
            double allMax = 0.0;
            double allStored = 0.0;
            double average = 0;

            for(int i = 0; i < Batteries.Count; i++)
            {
                IMyBatteryBlock Battery = Batteries[i];
                DetailedInfo DI = new DetailedInfo(Battery);
                allMax += parsePower(DI.getValue(BATTERY_VALUE_INDEX_MAX).getValue());
                allStored += parsePower(DI.getValue(BATTERY_VALUE_INDEX_STORED).getValue());
            }
            if(allMax > 0)
            {
                average = allStored / allMax;
            }

            return average * 100;
        }

        private double parsePower(string value)
        {
            double result = 0;
            value = value.ToLower();
            int f = 1;
            if (value.Contains("g"))
            {
                f = 1000 * 1000 * 1000;
            }
            else if (value.Contains("m"))
            {
                f = 1000 * 1000;
            }
            else if (value.Contains("k"))
            {
                f = 1000;
            }
            value = value.Replace('w', ' ').Replace('k', ' ').Replace('m', ' ').Replace('g', ' ').Trim(' ');

            double numberValue = 0;
            if (double.TryParse(value, out numberValue))
            {
                result = (numberValue * f);
            }

            return result;
        }

        private List<IMyBatteryBlock> getCausalBatteriesOnGrid(IMyCubeGrid Grid, List<IMyTerminalBlock> excludeBlocksList)
        {
            List<IMyTerminalBlock> Batteries = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Batteries, ( x => !excludeBlocksList.Contains(x) && x.CubeGrid.Equals(Grid)));

            return Batteries.ConvertAll<IMyBatteryBlock>(x => x as IMyBatteryBlock);
        }

        private List<IMyTerminalBlock> getEmergencyPowerBlocksOnGrid(IMyCubeGrid Grid, string tag)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            Blocks.AddRange(getBlocksFromGroup<IMyBatteryBlock>(tag, Grid).ConvertAll<IMyTerminalBlock>(x => x as IMyTerminalBlock));
            Blocks.AddRange(getBlocksFromGroup<IMyReactor>(tag, Grid).ConvertAll<IMyTerminalBlock>(x => x as IMyTerminalBlock));
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Blocks, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(Grid)));
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(Blocks, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(Grid)));
            
            return uniqueList<IMyTerminalBlock>(Blocks);
        }

        private List<T> getBlocksFromGroup<T>(string tag, IMyCubeGrid CubeGrid = null) where T : IMyTerminalBlock
        {
            List<IMyBlockGroup> Groups = new List<IMyBlockGroup>();
            List<T> Blocks = new List<T>();
            GridTerminalSystem.GetBlockGroups(Groups);
            for(int i_Groups = 0; i_Groups < Groups.Count; i_Groups++)
            {
                IMyBlockGroup Group = Groups[i_Groups];
                if (Group.Name.Contains(tag))
                {
                    for (int i_Blocks = 0; i_Blocks < Group.Blocks.Count; i_Blocks++)
                    {
                        IMyTerminalBlock Block = Group.Blocks[i_Blocks];
                        if(Block is T && (CubeGrid == null || Block.CubeGrid.Equals(CubeGrid)))
                        {
                            object newBlock = Block;
                            Blocks.Add((T)newBlock);
                        }
                    }
                }
            }

            return Blocks;
        }

        private List<T> uniqueList<T>(List<T> Collection)
        {
            List<T> result = new List<T>();
            for(int i = 0; i < Collection.Count; i++)
            {
                T tmp = Collection[i];
                if (!result.Contains(tmp))
                {
                    result.Add(tmp);
                }
            }

            return result;
        }

        class DetailedInfo
        {
            private List<DetailedInfoValue> storage = new List<DetailedInfoValue>();

            public DetailedInfo(IMyTerminalBlock Block)
            {
                string[] Info = Block.DetailedInfo.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Info.Length; i++)
                {
                    List<string> data = new List<string>();
                    data.AddRange(Info[i].Split(':'));
                    if (data.Count > 1)
                    {
                        storage.Add(new DetailedInfoValue(data[0], String.Join(":", data.GetRange(1, data.Count - 1))));
                    }
                }
            }

            public DetailedInfoValue getValue(int index)
            {
                if (index < storage.Count && index > -1)
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
        #endregion
    }
}
