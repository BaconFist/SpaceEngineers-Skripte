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
    class RedAlert_v01
    {

        String Storage;
        IMyGridTerminalSystem GridTerminalSystem;
        // Begin InGame-Script
        void Main()
        {
            List<IMyTerminalBlock> lights = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(lights);
            List<IMyTerminalBlock> doors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);

            if (Storage.IndexOf("[AlarmState:ON]") > -1)
            {
                String hash;
                for (int i = 0; i < lights.Count; i++)
                {
                        hash = "[" + Convert.ToString((lights[i] as IMyLightingBlock).GetPosition().ToString()) + "]";
                        if (Storage.IndexOf(hash) > -1)
                        {
                            (lights[i] as IMyLightingBlock).ApplyAction("OnOff_Off");
                        }                        
                }
                for (int i = 0; i < doors.Count; i++)
                {
                        hash = "[" + Convert.ToString((doors[i] as IMyDoor).GetPosition().ToString()) + "]";
                        if (Storage.IndexOf(hash) > -1)
                        {
                            (doors[i] as IMyDoor).ApplyAction("Open_On");
                        }
                }
                Storage = "";
            }
            else
            {
                Storage = "[AlarmState:ON]";

                for (int i = 0; i < lights.Count; i++)
                {
                    if (!(lights[i] as IMyLightingBlock).Enabled)
                    {
                        Storage += "[" + Convert.ToString((lights[i] as IMyLightingBlock).GetPosition().ToString()) + "]";
                        (lights[i] as IMyLightingBlock).ApplyAction("OnOff_On");
                    }
                }
                for (int i = 0; i < doors.Count; i++)
                {
                    if ((doors[i] as IMyDoor).Open)
                    {
                        Storage += "[" + Convert.ToString((doors[i] as IMyDoor).GetPosition().ToString()) + "]";
                        (doors[i] as IMyDoor).ApplyAction("Open_Off");
                    }
                }
            }            
        }
        // End InGame-Script
    }
}
