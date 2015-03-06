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
    class RefreshTxtPanels
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            (new RefreshTextPanel()).run(GridTerminalSystem);
        }

        class RefreshTextPanel
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks, (x => (x as IMyFunctionalBlock).Enabled));
                for (int i = 0; i < blocks.Count; i++)
                {
                    (blocks[i] as IMyFunctionalBlock).ApplyAction("OnOff_Off");
                    (blocks[i] as IMyFunctionalBlock).ApplyAction("OnOff_On");
                }
            }
        }
        // End InGame-Script
    }
}
