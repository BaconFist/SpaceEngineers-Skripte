using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace BaconfistSEInGameScript
{
    class OverallReactorUsage
    {
        IMyGridTerminalSystem GridTerminalSystem;
        
        //InGame Script BEGIN 
        public void Main()
        {

            IMyBlockGroup group = getGroup("OverallReactorUsageDisplay");
            IMyTerminalBlock display = getDisplayBlock(group);
            if (display != null)
            {
                BMyPowerUsage PowerUsage = new BMyPowerUsage(GridTerminalSystem);
                StringBuilder output = new StringBuilder();
                output.AppendLine("power consumption: " + PowerUsage.getCurrentOutputInPercent(2) + "%");
                output.AppendLine("Current Output: " + PowerUsage.getCurrentOutputHumanReadable());
                output.AppendLine("Max Output: " + PowerUsage.getMaxOutputHumanReadable());

                display.SetCustomName(output.ToString());
            }
        }

        class BMyPowerUsage
        {
            private IMyGridTerminalSystem GridTerminalSystem;
            private List<BMyPowerInfo> powerInfos = null;
            private double maxOutput;
            private double currentOutput;

            public BMyPowerUsage(IMyGridTerminalSystem _GridTerminalSystem)
            {
                GridTerminalSystem = _GridTerminalSystem;
            }

            public List<BMyPowerInfo> getPowerBlocks(bool forceRefresh = false)
            {
                if (forceRefresh || powerInfos == null)
                {
                    maxOutput = 0;
                    currentOutput = 0;
                    powerInfos = new List<BMyPowerInfo>();
                    powerInfos.Clear();
                    BMyPowerInfo tempInfo;
                    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyReactor>(blocks);
                    for(int i=0;i<blocks.Count;i++)
                    {
                        tempInfo = new BMyPowerInfo(blocks[i]);
                        powerInfos.Add(tempInfo);
                        maxOutput = maxOutput + tempInfo.getMaxOutput();
                        currentOutput = currentOutput + tempInfo.getCurrentOutput();
                    }
                }

                return powerInfos;
            }

            
            public double getMaxOutput()
            {
                return maxOutput;
            }

            public double getCurrentOutput()
            {
                return currentOutput;
            }

            public String getCurrentOutputHumanReadable()
            {
                return getValueHumanReadable(getCurrentOutput());
            }

            public String getMaxOutputHumanReadable()
            {
                return getValueHumanReadable(getMaxOutput());
            }

            public double getCurrentOutputInPercent(int decimals = 0)
            {
                return Math.Round(100 * (getCurrentOutput() / getMaxOutput()), decimals);
            }

            private String getValueHumanReadable(double _value)
            {
                String HRValue;
                int count = 0;
                while ((count < 3) && ((_value / 1000) > 1000))
                {
                    count++;
                    _value = _value / 1000;
                }
                switch (count)
                {
                    case 1:
                        HRValue = String.Format("{0:N2}", Math.Round(_value, 2)) + " KW";
                        break;
                    case 2:
                        HRValue = String.Format("{0:N2}", Math.Round(_value, 2)) + " MW";
                        break;
                    case 3:
                        HRValue = String.Format("{0:N2}", Math.Round(_value, 2)) + " GW";
                        break;
                    case 4:
                        HRValue = String.Format("{0:N2}", Math.Round(_value, 2)) + " TW";
                        break;
                    default:
                        HRValue =  String.Format("{0:N2}", Math.Round(_value, 2)) + " W";
                        break;
                }
                return HRValue;
            }
        }

        class BMyPowerInfo
        {
            private double maxOutput;
            private double currentOutput;
            private String maxOutputHR;
            private String currentOutputHR;
            private String CustomName;

            public BMyPowerInfo(IMyTerminalBlock block)
            {
                CustomName = block.CustomName;
                String[] info = block.DetailedInfo.Split(new String[] {"\n\r", "\r\n", "\n","\r"}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < info.Length; i++)
                {
                    if(info[i].IndexOf("Max Output") != -1)
                    {
                        maxOutput = getOutputValue(info[i]);
                        maxOutputHR = info[i].Replace("Max Output:","").Trim();
                    }
                    else if (info[i].IndexOf("Current Output") != -1)
                    {
                        currentOutput = getOutputValue(info[i]);
                        currentOutputHR = info[i].Replace("Current Output:", "").Trim();
                    }
                }                
            }

            private String getCustomName()
            {
                return CustomName;
            }

            private double getOutputValue(String infoLine)            
            {
                double rawValue = Double.Parse(infoLine);
                double multiplicator = getMultiplicator(infoLine);
                rawValue = rawValue * multiplicator;

                return rawValue;
            }
            
            private double getMultiplicator(String infoLine)
            {
                if (infoLine.IndexOf("GW") != -1)
                {
                    return 1000000000;
                }
                else if (infoLine.IndexOf("MW") != -1)
                {
                    return 1000000;
                }
                else if (infoLine.IndexOf("KW") != -1)
                {
                    return 1000;
                }          
                
                return 1;
            }

            

            public double getMaxOutput()
            {
                return maxOutput;
            }

            public double getCurrentOutput()
            {
                return currentOutput;
            }

            public String getCurrentOutputHumanReadable()
            {
                return currentOutputHR;
            }

            public String getMaxOutputHumanReadable()
            {
                return maxOutputHR;
            }

            public double getCurrentOutputInPercent(int decimals = 0)
            {
                return Math.Round(100 * (getCurrentOutput() / getMaxOutput()), decimals);
            }
        }


        //MicroFramework BEGIN  
        IMyBlockGroup getGroup(String name)
        {
            List<IMyBlockGroup> blockGroups = GridTerminalSystem.BlockGroups;
            for (int i = 0; i < blockGroups.Count; i++)
            {
                if (blockGroups[i].Name.IndexOf(name) == 0)
                {
                    return blockGroups[i];
                }
            }
            return null;
        }

        bool isDisplayBlock(IMyTerminalBlock block)
        {
            return isRadioAntenna(block) || isBeacon(block);
        }

        IMyTerminalBlock getDisplayBlock(IMyBlockGroup group)
        {
            IMyTerminalBlock block;
            block = getDisplayAntenna(group);
            if (isRadioAntenna(block))
            {
                block = getDisplayBeacon(group);
            }

            return block;
        }

        bool isRadioAntenna(IMyTerminalBlock testBlock)
        {
            return (testBlock as IMyRadioAntenna) != null;
        }

        IMyRadioAntenna getDisplayAntenna(IMyBlockGroup group)
        {
            for (int i = 0; i < group.Blocks.Count; i++)
            {
                if (isRadioAntenna(group.Blocks[i]))
                {
                    return (group.Blocks[i] as IMyRadioAntenna);
                }
            }

            return null;
        }

        bool isBeacon(IMyTerminalBlock testBlock)
        {
            return (testBlock as IMyBeacon) != null;
        }

        IMyBeacon getDisplayBeacon(IMyBlockGroup group)
        {
            for (int i = 0; i < group.Blocks.Count; i++)
            {
                if (isBeacon(group.Blocks[i]))
                {
                    return (group.Blocks[i] as IMyBeacon);
                }
            }

            return null;
        }

        //MicroFramework END
        //InGame Script END
    }
}
