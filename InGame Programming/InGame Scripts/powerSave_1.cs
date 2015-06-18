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
    class powerSave_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        const string  DEBUG_PANEL = "*debug*";
        const string PS_IGNORE = "*PSave Ignore*";

        class ram
        {
            static public Dictionary<Vector3I, bool> data = new Dictionary<Vector3I, bool>();
        }

        void Main()
        {
            Dictionary<Vector3I, bool> newData = new Dictionary<Vector3I, bool>();
            debug("Log @ " + DateTime.Now.ToString(), false);
            debug("Storage: " + Storage);
            if (Storage.Equals("OnOff_On"))
            {
                Storage = "OnOff_Off";
            }
            else
            {
                Storage = "OnOff_On";
            }
            debug(" - changed to => " + Storage);

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            addBlocks<IMyLightingBlock>(blocks);
            addBlocks<IMyProductionBlock>(blocks);
            addBlocks<IMyProjector>(blocks);
            addBlocks<IMyShipToolBase>(blocks);
            addBlocks<IMyProgrammableBlock>(blocks);
            addBlocks<IMyTimerBlock>(blocks);
            addBlocks<IMyGravityGeneratorBase>(blocks);
            addBlocks<IMyTextPanel>(blocks);
            for (int i = 0; i < blocks.Count; i++)
            {
                if (allowAction(blocks[i], Storage))
                {
                    blocks[i].ApplyAction(Storage);
                    debug("apply " + Storage + "@" + blocks[i].Position.ToString() + blocks[i].CustomName);
                    newData.Add(blocks[i].Position, (blocks[i] as IMyFunctionalBlock).Enabled);
                }
            }
            ram.data = newData;
        }

        public void addBlocks<T>(List<IMyTerminalBlock> blocks)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<T>(tmp, (x => (ram.data.Count == 0 || (ram.data.ContainsKey(x.Position) && (x as IMyFunctionalBlock).Enabled == ram.data[x.Position]))));
            blocks.AddList(tmp);
            debug("add " + typeof(T).ToString() + "  => blocks:  " + blocks.Count.ToString() + ", new: " + tmp.Count.ToString());
        }

        void debug(string txt, bool append = true){
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(tmp, (x => x.CustomName.Contains(DEBUG_PANEL)));
            for (int i = 0; i < tmp.Count; i++)
            {
                (tmp[i] as IMyTextPanel).WritePublicText(txt + "\n", append);
            }
        }

        bool allowAction(IMyTerminalBlock block, string action)
        {
            if (!(block is IMyFunctionalBlock))
            {
                return false;
            }
            if (block.CustomName.Contains(PS_IGNORE))
            {
                return false;
            }
            if(block is IMyProjector && action.Equals("OnOff_On")){
                return false;
            }
            if (block is IMyShipToolBase && action.Equals("OnOff_On"))
            {
                return false;
            }
            if (block is IMyReflectorLight && action.Equals("OnOff_On"))
            {
                return false;
            }

            return true;
        }
        // End InGame-Script
    }
}
