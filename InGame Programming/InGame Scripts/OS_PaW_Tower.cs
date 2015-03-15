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
    class OS_PaW_Tower
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
// Begin InGame-Script
        void Main()
        {
            OsKernel OS = new OsKernel(GridTerminalSystem, 1);
            OS.setOutput("LCD Panel 9 OS (Tower)");
            OS.setOutputChars(70);
            OS.setOutputLines(17);
            OS.setTitle("PaW MainBase");

            (new Clock()).run(OS, "LCD Panel 7 Tür/Uhr (Tower)", OS.getTitle() + "\n", "");
            (new Weapons()).run(OS, "Waffen (Tower)");
            

        }

        class Clock
        {
            public void run(OsKernel OS, String textPanel, String append, String prepend)
            {
                IMyTextPanel clock = (OS.GridTerminalSystem.GetBlockWithName(textPanel) as IMyTextPanel);
                if (clock is IMyTextPanel)
                {
                    clock.WritePublicText(String.Join("\n", OS.WordWrap(append + DateTime.Now.ToString().Replace(" ", "\n") + prepend, 10).ToArray()));
                }
            }
        }

        class Weapons
        {
            IMyGridTerminalSystem GridTerminalSystem;
            OsKernel OS;
            public void run(OsKernel OS, String textPanel)
            {
                GridTerminalSystem = OS.GridTerminalSystem;
                List<IMyTerminalBlock> textPanels = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels, (x => (x as IMyTerminalBlock).CustomName.Contains(textPanel)));
                if (textPanels.Count > 0)
                {
                    List<IMyTerminalBlock> weapons = new List<IMyTerminalBlock>();
                    StringBuilder infoLines = new StringBuilder();
                    GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(weapons);
                    if (weapons.Count > 0)
                    {
                        for (int weaponIndex = 0; weaponIndex < weapons.Count; weaponIndex++)
                        {
                            IMyLargeTurretBase turret = (weapons[weaponIndex] as IMyLargeTurretBase);
                            infoLines.Append("[" + turret.CustomName + "]:\n");
                            infoLines.Append((turret.Enabled?"An":"Aus"));
                            infoLines.Append(", " + getAmmo(turret));
                            infoLines.Append(", " + String.Format("{0:N0} Meter", turret.Range));
                            if (weaponIndex < weapons.Count - 1)
                            {
                                infoLines.Append("\n");
                            }
                        }
                    }
                    else
                    {
                        infoLines.AppendLine("keine Waffen gefunden.");
                    }
                    OS.writeToTextpanels(textPanels, infoLines.ToString(), 30, 12, OS.replaceTocken("Waffen\n[HR]", 30));
                }
            }

            string getAmmo(IMyLargeTurretBase turret)
            {
                String info = null;
                if (turret.HasInventory())
                {
                    List<IMyInventoryItem> items = turret.GetInventory(0).GetItems();
                    if (items.Count > 0)
                    {
                        double ammount = 0;
                        for (int k = 0; k < items.Count; k++)
                        {
                            if (info == null)
                            {
                                info = "x" + items[k].Content.SubtypeId.ToString();
                            }
                            ammount += Convert.ToDouble(items[k].Amount.ToString());
                        }
                        info = String.Format("{0:N0}", ammount) + info;
                    }
                    else
                    {
                        info = "leer";
                    }
                }
                else
                {
                    info = "kein Inventar";
                }

                return info;
            }

            void log(string msg)
            {
                OS.output("[Waffen]: " + msg );
            }
        }

        class OsKernel
        {
            static Int32 _tick = 0;
            Int32 _tick_limit;
            Int32 outputTextPanel_lines = 30;
            Int32 outputTextPanel_chars = 30;
            IMyTextPanel outputTextPanel;
            public IMyGridTerminalSystem GridTerminalSystem;
            String title;

            public OsKernel(IMyGridTerminalSystem _GridTerminalSystem, Int32 tick_limit)
            {
                _tick_limit = tick_limit;
                GridTerminalSystem = _GridTerminalSystem;
                tickProcess();
            }

            public void setTitle(String val)
            {
                title = val;
            }

            public string getTitle()
            {
                return this.title;
            }

            public void setOutputChars(Int32 val)
            {
                outputTextPanel_chars = val;
            }

            public void setOutputLines(Int32 val)
            {
                outputTextPanel_lines = val;
            }

            public void setOutput(String outputPanelName)
            {
                outputTextPanel = (GridTerminalSystem.GetBlockWithName(outputPanelName) as IMyTextPanel);
            }

            public void output(String message)
            {
                if (outputTextPanel is IMyTextPanel)
                {
                    String time = DateTime.Now.ToString();
                    List<String> lines = new List<string>();
                    lines.AddArray(outputTextPanel.GetPublicText().Split(new String[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None));
                    List<String> chunks = WordWrap("[" + time + "] " + message, outputTextPanel_chars);


                    for (int i = 0; i < chunks.Count; i++)
                    {
                        lines.Add(chunks[i]);
                    }
                    if (lines.Count > outputTextPanel_lines - 1)
                    {
                        Int32 range = Convert.ToInt32(lines.Count - (outputTextPanel_lines - 1));
                        lines.RemoveRange(0, range);
                    }
                    outputTextPanel.WritePublicText(title + " - " + DateTime.Now.ToString() + " - Tick: " + _tick.ToString() + "/" + _tick_limit.ToString(), false);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        outputTextPanel.WritePublicText("\n" + lines[i], true);
                    }
                }
            }

            public void writeToTextpanels(List<IMyTerminalBlock> textpanels, String text, int charsPerLine, int linesPerPanel)
            {
                writeToTextpanels(textpanels, text, charsPerLine, linesPerPanel, "");
            }

            public void writeToTextpanels(List<IMyTerminalBlock> textpanels, String text, int charsPerLine, int linesPerPanel, String header)
            {
                List<String> wrappedLines = this.WordWrap(text, charsPerLine);
                List<String> headerLines = WordWrap(header, charsPerLine);
                int maxLines = linesPerPanel - headerLines.Count;
                for (int i = 0; i < textpanels.Count; i++)
                {
                    if (wrappedLines.Count > 0)
                    {
                        IMyTextPanel curTextpanel = textpanels[i] as IMyTextPanel;
                        if (curTextpanel is IMyTextPanel)
                        {
                            int range = maxLines;
                            if (maxLines > wrappedLines.Count)
                            {
                                range = wrappedLines.Count;
                            }
                            curTextpanel.WritePublicText(String.Join("\n", headerLines.ToArray())+"\n", false);
                            curTextpanel.WritePublicText(String.Join("\n", wrappedLines.GetRange(0, maxLines).ToArray()), true);
                            wrappedLines.RemoveRange(0, range);
                        }
                    }
                }
            }

            void tickProcess()
            {
                _tick++;
                if (_tick > _tick_limit)
                {
                    _tick = 1;
                }
            }

            public bool isTick(Int32 tick)
            {
                return ((tick % _tick) == 0);
            }

            public string replaceTocken(string text, int charsMax)
            {
                text = text.Replace("[HR]", "".PadLeft(charsMax, '-'));

                return text;
            }

            // WordWrap based on http://bryan.reynoldslive.com/post/Wrapping-string-data.aspx

            public String WordWrapS(string text)
            {
                return WordWrapS(text, outputTextPanel_chars);
            }

            public String WordWrapS(string text, int maxLength)
            {
                return String.Join("\n", WordWrap(text, maxLength).ToArray());
            }

            public List<String> WordWrap(string text)
            {
                return WordWrap(text, outputTextPanel_chars);
            }

            public List<String> WordWrap(string text, int maxLength)
            {
                String[] words = text.Split(' ');
                List<String> lines = new List<String>();
                String currentLine = "";

                for (int i = 0; i < words.Length; i++)
                {
                    String currentWord = words[i];
                    if ((currentLine.Length > maxLength) || ((currentLine.Length + currentWord.Length) > maxLength))
                    {
                        lines.Add(currentLine);
                        currentLine = "";
                    }
                    if (currentLine.Length > 0)
                    {
                        currentLine += " " + currentWord;
                    }
                    else
                    {
                        currentLine += currentWord;
                    }
                }
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                }

                return lines;
            }
        }

        // End InGame-Script
    }
}
