using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class ProductionBlockOn
    {
        IMyGridTerminalSystem GridTerminalSystem;

        // Script BEGIN
        void Main()
        {
            (new ProductionBlockWakeUp()).run(GridTerminalSystem);
        }

        class ProductionBlockWakeUp
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => !(x as IMyFunctionalBlock).Enabled));
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction("OnOff_On");
                }
            }
        }
        

        // SCript END
    }
}
