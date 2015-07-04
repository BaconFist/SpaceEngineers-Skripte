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
    public class MyDebugger : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           MyDebugger
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           

           Abstract
           ------------------------------
          
           
           Example
           ------------------------------
          
       */
        void Main(string args)
        {
            MyDebug Debug = new MyDebug(this);
            Debug.write("Copy the MyDebug Clacc to your script.");
        }

        #region copy from here
        class MyDebug
        {
            MyGridProgram program;
            public MyDebug(MyGridProgram program)
            {
                this.program = program;
            }

            public void write(string msg)
            {
                string arg = "[" + program.ElapsedTime.TotalMilliseconds.ToString() + "] " + msg;
                program.Echo(arg);
            }
        }
        #endregion copy to here
        #endregion
    }
}
