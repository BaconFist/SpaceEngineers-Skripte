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
            (new AssemblerCleaning()).run(GridTerminalSystem, "target_cargo"); 
        }

        class AssemblerCleaning
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem, String cargoName)
            {
                IMyCargoContainer cargo = (GridTerminalSystem.GetBlockWithName(cargoName) as IMyCargoContainer);
                if (cargo is IMyCargoContainer)
                {
                    IMyAssembler assembler;
                    for (int i = 0; i < GridTerminalSystem.Blocks.Count; i++)
                    {
                        assembler = (GridTerminalSystem.Blocks[i] as IMyAssembler);

                    }
                }


                if (cargo != null)
                {
                    IMyInventory cargoInventory = cargo.GetInventory(0);
                    IMyInventory assemblerInventory = null;
                    List<IMyInventoryItem> items = null;
                    List<IMyTerminalBlock> assembler = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(assembler, delegate(IMyTerminalBlock block) { return (block is IMyAssembler); });
                    for (int i = 0; i < assembler.Count; i++)
                    {
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
            }
        }
        //InGame Script END
    }
}
