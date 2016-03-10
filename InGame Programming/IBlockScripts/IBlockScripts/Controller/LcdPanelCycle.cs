using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class LcdPanelCycle : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           LcdPanelCycle
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
            show multiple panels at one.

           Abstract
           ------------------------------
            you can cycle to the content of multiple panel in one panel.

            1. Build Programmable Block an load this script
            2. Define a LCD as Display (it will show the Text). Name it !LPC:whatever. ("whatever" is the panel id)
            3. Define as much LCDs as Source as you want (From here we will get the text). Name the 1st !LPC:whatever:1, the 2nd !LPC:whatever:2 ans so on
            4. Run PB with commands to cycle and  update your panel
            

           Commands
           ------------------------------
            panel id+n = cycle 'n' steps to next panels
            panel id-n = cycle 'n' step to previous panels
            panel id= = updates current text
            panel id=n = show panel number 'n'
           
           Example
           ------------------------------
           You have a setup like in "Abstract":
            - 1 PB with this script
            - 1 LCD as Target with "!LPC:whatever" in Blockname
            - 2 LCDs as Source with "!LPC:whatever:1" and "!LPC:whatever:2" in Blockname

            Run PB with argument "whatever=1" to display text from "!LPC:whatever:1"
            Run PB with argument "whatever+" to display text from next panel (in this case "!LPC:whatever:2")
            Run PB with argument "whatever-" to display text from previous panel (in this case "!LPC:whatever:1")
            Run PB with argument "whatever" to update text from last source

            to get Live-Updates for your panel (in case you're using Configurable Automatic LCDs), you'll need to update current text automatically:
            - Setup a Timer to run itself and PB with "panel id="
            this will refresh the display with its current source

       */
        const string TAG_BEGIN = "!LPC";
        const string TAG_END = "!";

        private Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, string> Arguments = new Dictionary<string, string>();

        void Main(string args)
        {
            boot(args);
            run();
            shutdown();
        }

        private void run()
        {
            
            List<IMyTextPanel> SourcePanels = getSourcePanels(Arguments["ID"]);
            List<IMyTextPanel> TargetPanels = getTargetPanels(Arguments["ID"]);

            int souceIndexCur = int.Parse(getStorageValue(Arguments["ID"] + "_INDEX", "0"));
            int souceIndex = souceIndexCur;

            if (Arguments.ContainsKey("INC"))
            {
                 souceIndex = getNextIndex(SourcePanels, int.Parse(Arguments["INC"]));
            } else if (Arguments.ContainsKey("DEC"))
            {
                souceIndex = getPrevIndex(SourcePanels, int.Parse(Arguments["DEC"]));
            }
            else if(Arguments.ContainsKey("SEL"))
            {
                souceIndex = int.Parse(Arguments["SEL"]);
            }
            if (souceIndex < 0 || souceIndex > SourcePanels.Count)
            {
                souceIndex = souceIndexCur;
            }
            
        }


        private void boot(string args)
        {
            Arguments = BuildArguments(args);

            string[] segemnts = Storage.Split(';');
            for(int i = 0; i < segemnts.Length; i++)
            {
                string[] items = Storage.Split(':');
                if(items.Length == 2)
                {
                    data.Add(items[0], items[1]);
                }
            }
        }

        private void shutdown()
        {
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<string,string> set in data)
            {
                sb.Append(set.Key + ":" + set.Value + ";");
            }

            Storage = sb.ToString();
        }
        
        private string getStorageValue(string key, string defaultValue = null)
        {
            if (hasStroageValue(key))
            {
                return data[key];
            } else
            {
                return defaultValue;
            }
        }

        private bool hasStroageValue(string key)
        {
            return data.ContainsKey(key);
        }


        private int getNextIndex(List<IMyTextPanel> LCDs, int steps)
        {
            int cur_index = getIndex();
            int new_index = cur_index + steps;
            if(new_index > LCDs.Count)
            {
                new_index = new_index - LCDs.Count;
            }
            updateIndex(new_index);

            return new_index;
        }

        private int getPrevIndex(List<IMyTextPanel> LCDs, int steps)
        {
            int cur_index = getIndex();
            int new_index = cur_index - steps;
            if (new_index < 0)
            {
                new_index = new_index + LCDs.Count;
            }
            updateIndex(new_index);

            return new_index;
        }

        private int getIndex()
        {
            string key = Arguments["ID"] + "_INDEX";
            int index = int.Parse(getStorageValue(key, "0"));

            return index;
        }

        private void updateIndex(int newIndex)
        {
            string key = Arguments["ID"] + "_INDEX";
            if (hasStroageValue(key))
            {
                data[key] = newIndex.ToString();
            } else
            {
                data.Add(key, newIndex.ToString());
            }
        }
                 
        private Dictionary<string, string> BuildArguments(string args)
        {
            Dictionary<string, string> newArgs = new Dictionary<string, string>();
            int cmd_sign_index = -1;
            string cmd_value = "";

            cmd_sign_index = args.IndexOf('+');
            if(cmd_sign_index != -1)
            {
                cmd_value = args.Substring(cmd_sign_index + 1, args.Length - cmd_sign_index - 1);
                newArgs.Add("INC",cmd_value);
            } else
            {
                cmd_sign_index = args.IndexOf('-');
                if (cmd_sign_index != -1)
                {
                    cmd_value = args.Substring(cmd_sign_index + 1, args.Length - cmd_sign_index - 1);
                    newArgs.Add("DEC", cmd_value);
                }
                else
                {
                    cmd_sign_index = args.IndexOf('=');
                    if (cmd_sign_index != -1)
                    {
                        cmd_value = args.Substring(cmd_sign_index + 1, args.Length - cmd_sign_index - 1);
                        newArgs.Add("SEL", cmd_value);
                    }
                }
            }
            if(cmd_sign_index != -1)
            {
                newArgs.Add("ID",args.Substring(0, cmd_sign_index));
            } else
            {
                newArgs.Add("ID", args);
            }

            return newArgs;
        }
                
        private List<IMyTextPanel> getTargetPanels(string id)
        {
            string pattern = TAG_BEGIN;
            pattern += ":" + id + TAG_END;
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Blocks, (x => rgx.IsMatch((x as IMyTextPanel).CustomName)));

            return Blocks.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
        }

        private List<IMyTextPanel> getSourcePanels(string id)
        {
            string pattern = TAG_BEGIN;
            pattern += ":"+id+":\\d+" + TAG_END;            
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Blocks, (x => rgx.IsMatch((x as IMyTextPanel).CustomName)));

            List<IMyTextPanel> LCDs = new List<IMyTextPanel>();

            for(int i = 0; i < LCDs.Count; i++)
            {
                pattern = TAG_BEGIN + ":" + id + ":" + i.ToString() + TAG_END;
                IMyTerminalBlock match = Blocks.Find(x => x.CustomName.Contains(pattern));
                if(match != null)
                {
                    LCDs.Add(match as IMyTextPanel);
                }
            }

            return LCDs;
        }

        #endregion
    }
}
