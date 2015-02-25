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
    class FrameWork
    {
        IMyGridTerminalSystem GridTerminalSystem;

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
    }
}
