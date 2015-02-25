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
    class BuildLevel
    {

        IMyGridTerminalSystem GridTerminalSystem;

        //InGame Script BEGIN  

        void Main()
        {
            IMyTerminalBlock projector = GridTerminalSystem.GetBlockWithName("Projektor Klein (Werft)");
            if (projector != null)
            {

                float buildLevelRatio = 0;
                Int32 buildBlocksCount = 0;
                IMyCubeGrid projectorGrid = projector.CubeGrid;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

                StringBuilder text = new StringBuilder();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if (blocks[i].CubeGrid.Equals(projectorGrid))
                    {
                        IMySlimBlock slimBlock = projectorGrid.GetCubeBlock(blocks[i].Position) as IMySlimBlock;
                        if (slimBlock != null)
                        {
                            blocks[i].RequestShowOnHUD(true);
                            buildLevelRatio += slimBlock.BuildLevelRatio;
                            buildBlocksCount++;
                            text.Append("[" + blocks[i].Name + ":" + slimBlock.BuildLevelRatio + "]");

                        }
                    }
                }
                if (buildBlocksCount > 0)
                {
                    buildLevelRatio = (buildLevelRatio / buildBlocksCount) * 100;
                }

                IMyTerminalBlock display = GridTerminalSystem.GetBlockWithName("CC 01 - Text Panel 14");
                if (display != null)
                {
                    display.SetCustomName("CC 01 - Text Panel 14 [" + String.Format("{0:N2}", Math.Round(buildLevelRatio, 2)) + " %]");
                }

            }
        }

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

        //InGame Script END


    }
}
