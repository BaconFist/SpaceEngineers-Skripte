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
    class OS_Production
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            OsProduction OS = new OsProduction(GridTerminalSystem, 300);
            OS.setOutput("LCD Panel (Verarbeitung A)");
            OS.setOutputChars(70);
            OS.setOutputLines(17);
            OS.setTitle("OS Verarbeitung A");


            if(OS.isTick(300)){
                (new AssemblerCleaning()).run(GridTerminalSystem, "FrachtContainer 1", OS);
            }            
        }

        class AssemblerCleaning
        {
            OsProduction OsProduction;

            public void run(IMyGridTerminalSystem GridTerminalSystem, String cargoName, OsProduction OS)
            {
                OsProduction = OS;
                IMyCargoContainer cargo = (GridTerminalSystem.GetBlockWithName(cargoName) as IMyCargoContainer);

                if (cargo is IMyCargoContainer)
                {
                    IMyInventory cargoInventory = cargo.GetInventory(0);
                    IMyInventory assemblerInventory = null;
                    List<String> cleanedAssemblers = new List<String>();
                    List<String> skippedAssemblers = new List<String>();
                    List<IMyInventoryItem> items = null;
                    List<IMyTerminalBlock> assembler = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(assembler, (x => (x is IMyAssembler)));
                    for (int i = 0; i < assembler.Count; i++)
                    {
                        if (assembler[i].GetInventory(0).IsConnectedTo(cargoInventory))
                        {
                            for (int i_inv = 0; i_inv < assembler[i].GetInventoryCount(); i_inv++)
                            {
                                assemblerInventory = assembler[i].GetInventory(i_inv);
                                items = assemblerInventory.GetItems();
                                for (int ii = 0; ii < items.Count; ii++)
                                {
                                    cargoInventory.TransferItemFrom(assemblerInventory, ii, null, true);
                                }
                            }
                            cleanedAssemblers.Add(assembler[i].CustomName);
                        }
                        else
                        {
                            skippedAssemblers.Add(assembler[i].CustomName);
                        }
                    }
                    String sumOutput = "";
                    if (cleanedAssemblers.Count > 0)
                    {
                        String _cleanedAssemblersLine = String.Join(", ", cleanedAssemblers.ToArray());
                        sumOutput += cleanedAssemblers.Count.ToString() + " Fertigungsroboter bereinigt. [" + _cleanedAssemblersLine + "]. ";
                    }
                    if (skippedAssemblers.Count > 0)
                    {
                        String _skippedAssemblersLine = String.Join(", ", skippedAssemblers.ToArray());
                        sumOutput += skippedAssemblers.Count.ToString() + " Fertigungsroboter übersprungen. [" + _skippedAssemblersLine + "]";
                    }
                    if (sumOutput.Length > 0)
                    {
                        OS.output(sumOutput);
                    }                    
                }
                else
                {
                    OS.output("Lager nicht gefunden: " + cargoName + ". Föredrbandverbindung überprüfen!");
                }
            }
        }

        class OsProduction
        {
            static Int32 _tick = 0;
            Int32 _tick_limit;
            Int32 outputTextPanel_lines = 30;
            Int32 outputTextPanel_chars = 30;
            IMyTextPanel outputTextPanel;
            IMyGridTerminalSystem GridTerminalSystem;
            String title;

            public OsProduction(IMyGridTerminalSystem _GridTerminalSystem, Int32 tick_limit)
            {
                _tick_limit = tick_limit;
                GridTerminalSystem = _GridTerminalSystem;
                tickProcess();                
            }

            public void setTitle(String val)
            {
                title = val;
            }

            public void setOutputChars(Int32 val)
            {
                outputTextPanel_chars = val;
            }

            public void setOutputLines(Int32 val)
            {
                outputTextPanel_lines = val;
            }

            public void setOutput(String outputPanelName){
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
                    if(lines.Count > outputTextPanel_lines - 1){
                        Int32 range = Convert.ToInt32(lines.Count - (outputTextPanel_lines - 1));
                        lines.RemoveRange(0, range);                        
                    }
                    outputTextPanel.WritePublicText(title + " - " + DateTime.Now.ToString() + " - Tick: " + _tick.ToString() + "/" + _tick_limit.ToString(), false);
                    for(int i=0;i<lines.Count;i++){
                        outputTextPanel.WritePublicText("\n" + lines[i], true);
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

            // WordWrap based on http://bryan.reynoldslive.com/post/Wrapping-string-data.aspx
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
