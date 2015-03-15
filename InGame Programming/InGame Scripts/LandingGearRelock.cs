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
    class LandingGearRelock
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        const String ProgrammableBlock_NAME = "";
        const String BeaconName = "";
        static double count;

        void Main()
        {
            
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(blocks, x => IsLocked(x as IMyTerminalBlock));
            for (int i = 0; i < blocks.Count;i++)
            {
                blocks[i].ApplyAction("Unlock ");
                blocks[i].ApplyAction("OnOff_Off");
                blocks[i].ApplyAction("OnOff_On");
                blocks[i].ApplyAction("Lock ");
            }

            IMyProgrammableBlock ProgrammableBlock = GridTerminalSystem.GetBlockWithName(ProgrammableBlock_NAME) as IMyProgrammableBlock;
            if (ProgrammableBlock is IMyProgrammableBlock)
            {
                ProgrammableBlock.ApplyAction("Run");
            }
            List<IMyTerminalBlock> bl = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBeacon>(bl, x => (x as IMyTerminalBlock).CustomName.IndexOf(BeaconName) == 0);
            if (bl.Count > 0)
            {
                if (!count.IsValid())
                {
                    count = 0;
                }
                count++;
                
                (bl[0] as IMyBeacon).SetCustomName(BeaconName + "[" + count.ToString() + "]");
            }
        }

        bool IsLocked(IMyTerminalBlock block)
        {
            var builder = new StringBuilder();
            block.GetActionWithName("SwitchLock").WriteValue(block, builder);

            return builder.ToString() == "Locked";
        }
        // End InGame-Script
    }
}
