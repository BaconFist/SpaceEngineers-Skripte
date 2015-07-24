using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class DeadmanSwitch : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           DeadmanSwitch
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source: http://git.io/vYcKE
           
           Summary
           ------------------------------
            Will check if Ship is controlled. If not activates Dampeners + all Thrusters + all Gyros for an Emergency Stop.

           Abstract
           ------------------------------
            Used to Emergency stop a Ship. Helpfull when loosing connection to a Server and flying without dampeners.
           
           Example
           ------------------------------
            To be called frequently by a timer.
            Setup:
                1 - build 1 Timer and 1 Programable Block.
                2 - load this script to Programable Block. Compile and Save.
                3 - Setup Timer Actions -> one to Start the Timer itself and one to run Programable Block.
                4 - Set Timer Injterval for your needs.
                5 - Start Timer -> success.
       */
        void Main(string args)
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks);
            
            if(blocks.Count > 0)
            {
                IMyShipController currentControl;
                bool IsUnderControl = false;
                for (int i = 0; i < blocks.Count; i++)
                {
                    currentControl = (blocks[i] as IMyShipController);
                    IsUnderControl = IsUnderControl || currentControl.IsUnderControl;  
                }
                if (!IsUnderControl)
                {                
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        currentControl = (blocks[i] as IMyShipController);
                        if (currentControl.DampenersOverride == false)
                        {
                            currentControl.ApplyAction("DampenersOverride");
                        }                
                    }
                    List<IMyTerminalBlock> movementBlocks = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(movementBlocks, ( x => (x is IMyThrust) || (x is IMyGyro)));
                    for(int i=0;i< movementBlocks.Count; i++)
                    {
                        (movementBlocks[i] as IMyThrust).ApplyAction("OnOff_On");
                    }
                }
            } 
        }
        #endregion
    }
}
