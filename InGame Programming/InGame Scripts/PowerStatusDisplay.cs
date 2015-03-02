using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using VRage;


namespace BaconfistSEInGameScript
{
    class PowerStatusDisplay
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
// Begin InGame-Script
        void Main()
        {
            IMyTextPanel textPanel = (GridTerminalSystem.GetBlockWithName("") as IMyTextPanel);
            if (!(textPanel is IMyTextPanel))
            {
                return;
            }
            IMyFunctionalBlock block;
            double powerUsageMax = 0;
            double powerUsageNow = 0;
            double powerAvailable = 0;
            double powerAvailableBySolar = 0;

            for (int i = 0; i < GridTerminalSystem.Blocks.Count; i++)
            {
                block = (GridTerminalSystem.Blocks[i] as IMyFunctionalBlock);
                if (!(block is IMyFunctionalBlock))
                {
                    continue;
                }
                if (block is IMyReactor)
                {
                    
                } else if(block is IMySolarPanel) {

                }
                else if ((block is IMyBatteryBlock) && isBatteryPoweroutputActive(block))
                {

                }
                else
                {

                }
                
            }
        }

        bool isBatteryPoweroutputActive(IMyFunctionalBlock block)
        {
            return false;
        }

        double getPowerUsage(IMyFunctionalBlock block)
        {
            return 0;
        }

        double getPowerUsageMax(IMyFunctionalBlock block)
        {
            return 0;
        }

        double getPowerOutput(IMyFunctionalBlock block)
        {
            return 0;
        }

       



// End InGame-Script
    }
}

