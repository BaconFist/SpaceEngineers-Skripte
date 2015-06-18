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
    class LcdMenu
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        IMyProgrammableBlock Me;
        Action<string> Echo;
        TimeSpan ElapsedTime;

        // Begin InGame-Script

        IMyTextPanel LCD;
        class ram
        {
            static public string selected;
        }

        string action;

        void Main(string argument)
        {
            string[] args = getArguments(argument);
            if (args.Length > 0)
            {
                action = (args.Length > 1) ? args[1] : null;
                LCD = GridTerminalSystem.GetBlockWithName(args[0]) as IMyTextPanel;
                if (LCD is IMyTextPanel)
                {
                    buildMenuFromConfig(LCD.GetPrivateText());
                }
            }
        }

        void buildMenuFromConfig(string cfg)
        {
            string[] cfgLines = cfg.Split('\n');
            StringBuilder menu = new StringBuilder();
            for (int i = 0; i < cfgLines.Length; i++)
            {
                
            }               
        }

        string[] getArguments(string arg, char split = ':')
        {
            return arg.Split(split);
        }

        void parseLine(string line){

        }        
        // End InGame-Script
    }
}
