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
    class CargoPercent
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
// Begin InGame-Script
        void Main()
        {
            IMyTextPanel textpanel = (GridTerminalSystem.GetBlockWithName("Textpanel Lagerstand Hexler") as IMyTextPanel);
            if (textpanel is IMyTextPanel)
            {
                Int32 percent = 0;
                Int32 activeToolCount = 0;
                IMyFunctionalBlock block;
                IMyInventory inventory;
                double max = 0;
                double cur = 0;
                double t_max = 0;
                double t_cur = 0;
                for (int i = 0; i < GridTerminalSystem.Blocks.Count; i++)
                {
                    block = (GridTerminalSystem.Blocks[i] as IMyFunctionalBlock);
                    if ((block is IMyFunctionalBlock) && block.HasInventory() && !(block is IMyReactor))
                    {
                        inventory = block.GetInventory(0);
                        t_max = Convert.ToDouble(inventory.MaxVolume.ToString());
                        t_cur = Convert.ToDouble(inventory.CurrentVolume.ToString()); 
                        max += t_max;
                        cur += t_cur;
                        if ((block is IMyShipToolBase))
                        {
                            if ((t_max - t_cur) < 0.1)
                            {
                                block.ApplyAction("OnOff_Off");
                            }                            
                            if (block.Enabled)
                            {
                                activeToolCount++;
                            }
                        }

                    }
                }

                if (cur == 0)
                {
                    percent = 0;
                }
                else if (cur == max)
                {
                    percent = 100;
                }
                else
                {
                    percent = Convert.ToInt32(Math.Round(100 * (cur / max), 0));
                }

                textpanel.WritePublicText(percent.ToString() + "%\An:" + activeToolCount.ToString());
                textpanel.ShowTextureOnScreen();
                textpanel.ShowPublicTextOnScreen();
            }            
        }
// End InGame-Script
    }
}
