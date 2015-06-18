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
    class DoorLockdown
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        IMyProgrammableBlock Me;
        Action<string> Echo;
        TimeSpan ElapsedTime;

        // Begin InGame-Script

        const string TIMER = "Zeitschaltuhr 24 PC-3 Computerraum 1";
        const string CMD_CLOSE = "Open_Off";
        const string CMD_OFF = "OnOff_Off";

        void Main(string argument)
        {
            argument = (argument.Length == 0) ? CMD_OFF : argument;

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(blocks, x => ((x as IMyTerminalBlock).HasAction(argument)));
            if (argument.Equals(CMD_CLOSE))
            {
                for(int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction(CMD_CLOSE);
                }
                IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER) as IMyTimerBlock;
                if(timer is IMyTimerBlock)
                {
                    timer.ApplyAction("Start");
                }
            }
            if (argument.Equals(CMD_OFF))
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction(CMD_OFF);
                }
            }
        }



        // End InGame-Script
    }
}
