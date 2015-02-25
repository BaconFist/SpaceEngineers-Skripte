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
    class AssemblerCleanUp
    {


        IMyGridTerminalSystem GridTerminalSystem;
        //InGame Script BEGIN        
        void Main()
        {
            IMyCargoContainer cargo = (GridTerminalSystem.GetBlockWithName("Barrencontainer") as IMyCargoContainer);
            if(cargo != null){
                IMyInventory cargoInventory = cargo.GetInventory(0);
                IMyInventory assemblerInventory = null;
                List<IMyInventoryItem> items = null;

                List<IMyTerminalBlock> assembler = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(assembler, delegate(IMyTerminalBlock block) { return (block is IMyAssembler);});
                if (assembler.Count == 0)
                {
                    throw new Exception("Kein Fertigungsroboter gefunden.");
                }

                for(int i=0;i<assembler.Count;i++){
                        assemblerInventory = assembler[i].GetInventory(0);
                        if (assemblerInventory.IsConnectedTo(cargoInventory))
                        {
                            items = assemblerInventory.GetItems();
                            for (int ii = 0; ii < items.Count; ii++)
                            {
                                cargoInventory.TransferItemFrom(assemblerInventory, ii, null, true);
                            }
                        }
                }
            }
            else
            {
                throw new Exception("Barrencontainer nicht gefunden.");
            }

        }
        //InGame Script END
    }
}
