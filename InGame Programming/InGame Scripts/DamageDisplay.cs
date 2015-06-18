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
    class DamageDisplay
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        const string LCD_Marker = "!DMG!";

        public Dictionary<String, StringBuilder> cache;

        void Main()
        {
            cache = new Dictionary<string, StringBuilder>();
            List<IMyTerminalBlock> lcd = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcd, (x => x.CustomName.Contains(LCD_Marker)));
            if (lcd.Count > 0)
            {
                for (int i = 0; i < lcd.Count; i++)
                {
                    IMyTextPanel panel = lcd[i] as IMyTextPanel;
                    panel.WritePublicText("", false);
                    String[] cmds = getCmd(panel);
                    for (int c = 0; c < cmds.Length; c++)
                    {
                        cmd(cmds[c], panel);
                    }                    
                }
            }

            IMyProjector pr = GridTerminalSystem.GetBlockWithName("Projektor") as IMyProjector;
            IMyTextPanel tp = GridTerminalSystem.GetBlockWithName("LCD Projektor") as IMyTextPanel;
            if (tp is IMyTextPanel && pr is IMyProjector )
            {
                
            }
        }

        public String[] getCmd(IMyTextPanel lcd)
        {
            return lcd.GetPublicTitle().Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
        }


        public void cmd(string cmd, IMyTextPanel lcd)
        {
            StringBuilder lcdText = new StringBuilder();
            
            if (inCache(cmd))
            {
                lcdText = getCached(cmd);
            }
            else
            {
                if(isCmd("Echo", cmd)){
                    lcdText.AppendLine(cmd_Echo(cmd));
                }
                else if(isCmd("Damage", cmd)){
                    lcdText.AppendLine(cmd_Damage(cmd));
                }

                cache.Add(cmd, lcdText);
            }

            lcd.WritePublicText(lcdText.ToString(), true);
        }

        public bool isCmd(string cmdName, string cmd)
        {
            return (cmd.IndexOf(cmdName) == 0);
        }

        public StringBuilder getCached(string cmd)
        {
            try
            {
                return cache[cmd];
            }
            catch (Exception)
            {                
                return new StringBuilder();
            }
        }

        public bool inCache(string cmd)
        {
            return cache.ContainsKey(cmd);
        }

        public string cmd_Echo(string cmd)
        {
            string res = cmd.Remove(0, 4).Trim();
            return res
                .Replace("Time", DateTime.Now.ToString("hh:mm:ss"))
                .Replace("Date", DateTime.Now.ToString("dd.MM.yyyy"))
                ;
        }

        public string cmd_Damage(String cmd)
        {
            IMyFunctionalBlock trigger = null;
            int _start = cmd.IndexOf("HUD:") + 4;
            int _count = cmd.IndexOf(":HUD") - _start;
            if (_start > -1 && _count > 0 && _count + _start < cmd.Length)
            {
                trigger = getBlockNamed(cmd.Substring(_start, _count)) as IMyFunctionalBlock;
            }              
            
            
            StringBuilder res = new StringBuilder();
            List<IMyTerminalBlock> blocks = GridTerminalSystem.Blocks;
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTerminalBlock block = blocks[i];
                IMySlimBlock slim = block.CubeGrid.GetCubeBlock(block.Position);
                float ratio = (float)slim.BuildLevelRatio;
                
                if (ratio < 1)
                {
                    if (trigger is IMyFunctionalBlock)
                    {
                        if (trigger.Enabled)
                        {
                            block.RequestShowOnHUD(true);
                        }
                        else
                        {
                            block.RequestShowOnHUD(false);
                        }
                    }
                    res.AppendLine(block.CustomName + " - Build: " + ratioFrmt(ratio) + "%" + ifBool(slim.HasDeformation, " [Deformiert] ") + ifBool(slim.IsDestroyed, " [Zerstört] "));
                    Dictionary<string,int> missingComponents = new Dictionary<string,int>();
                    slim.GetMissingComponents(missingComponents);
                    if (missingComponents.Count > 0)
                    {
                        res.AppendLine(">  Fehlende Teile: ");
                        String[] comps = new String[missingComponents.Keys.Count];
                        missingComponents.Keys.CopyTo(comps, 0);
                        for (int ci = 0; ci < comps.Length; ci++)
                        {
                            if (missingComponents.ContainsKey(comps[ci]))
                            {
                                res.AppendLine(">  " + comps[ci] + " x " + missingComponents[comps[ci]].ToString());
                            }
                        }
                    }
                }
                else
                {
                    if (trigger is IMyFunctionalBlock)
                    {
                        if (!trigger.Enabled)
                        {
                            block.RequestShowOnHUD(false);
                        }
                    }
                }
            }

            return res.ToString();
        }

        public string ifBool(bool val, string frm = "{0}")
        {
            return (val) ? formatBool(val, frm) : "";
        }

        public string formatBool(bool val, string frm = "{0}")
        {
            return String.Format(frm, val.ToString());
        }

        public String ratioFrmt(float num, string frm = "{0:N3}")
        {
            return format(num * 100, frm);
        }

        public String format(float num, string frm = "{0:N3}")
        {
            return String.Format(frm, num);
        }

        public IMyTerminalBlock getBlockNamed(string needle)
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => x.CustomName.Contains(needle)));
            if (blocks.Count > 0)
            {
                return blocks[0];
            }
            return null;
        }
        // End InGame-Script
    }
}
