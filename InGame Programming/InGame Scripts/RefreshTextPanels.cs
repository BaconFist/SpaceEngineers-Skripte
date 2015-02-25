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
    class RefreshTextPanels
    {
        IMyGridTerminalSystem GridTerminalSystem;
// Begin InGame-Script
        void Main()
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks);
            IMyTextPanel tp;
            for (int i = 0; i < blocks.Count; i++)
            {
                tp = (blocks[i] as IMyTextPanel);
                if (tp is IMyTextPanel)
                {
                    tp.GetActionWithName("OnOff_Off").Apply(tp);

                    tp.ShowTextureOnScreen();
                    tp.WritePublicText(tp.GetPublicText());
                    tp.ShowPublicTextOnScreen();

                    tp.GetActionWithName("OnOff_On").Apply(tp);
                }
            }
        }
// End InGame-Script
    }
}

