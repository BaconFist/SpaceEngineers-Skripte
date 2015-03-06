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
    class FirmwarePaWBauwagen
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
// Begin InGame-Script
        //PaW.Bauwagen Firmware 

        // Options
        const String kernel_textPanelName = "LCD Panel Firmware Ausgabe";
        const Int32 kernel_textPanelLineMax = 35;
        const Int32 kernel_textPanelColMax = 70;
        const String ReactorInfo_textPanel = "Text Panel Reaktor Info";
        const String ReactorInfo_reactor = "Reaktor (klein)";
        const String Clock_textPanel = "Textpanel Uhr";
        const String AssemblerCleaning_target_cargo = "Frachtcontainer Bauwagen";

        // Run-Settings
        const Int16 kernel_max_steps = 60;
        Int16 kernel_current_step = 1;
        IMyTextPanel _textPanel;
        List<String> _textPanelLines = new List<string>();
        long StartTime; 

        void Main()
        {
            boot();
            //Programms BEGIN
            (new ReactorInfo()).run(GridTerminalSystem, ReactorInfo_textPanel, ReactorInfo_reactor);
            (new Clock()).run(GridTerminalSystem, Clock_textPanel);
            if (isMatchingStep(30))
            {
                (new AssemblerCleaning()).run(GridTerminalSystem, AssemblerCleaning_target_cargo);
                debug("[" + DateTime.Now.ToString() + "] Fertigungsroboter bereinigt.");    
            }
            if (isMatchingStep(120))
            {
                (new ProductionBlockStandBy()).run(GridTerminalSystem);
                debug("[" + DateTime.Now.ToString() + "] inaktive Produktion in StandBy.");
            }
            if (isMatchingStep(60))
            {
                (new ProductionBlockWakeUp()).run(GridTerminalSystem);
                debug("[" + DateTime.Now.ToString() + "] Produktion .");
            }            
            //Programms END
            end();
        }

        class Clock
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem, String textPanel)
            {
                IMyTextPanel clock = (GridTerminalSystem.GetBlockWithName(textPanel) as IMyTextPanel);
                if (clock is IMyTextPanel)
                {
                    clock.WritePublicText(DateTime.Now.ToString().Replace(" ", "\n"));
                }
            }
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

        class ReactorInfo
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem, String textPanelName, String reactorName)
            {
                IMyTextPanel textPanel = (GridTerminalSystem.GetBlockWithName(textPanelName) as IMyTextPanel);
                IMyReactor reactor = (GridTerminalSystem.GetBlockWithName(reactorName) as IMyReactor);
                if ((textPanel is IMyTextPanel) && (reactor is IMyReactor))
                {
                    textPanel.WritePublicText(reactor.DetailedInfo + "\n\n letzte Aktualisierung:\n" + DateTime.Now.ToString());
                }
            }
        }

        class ProductionBlockStandBy
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyProductionBlock>(blocks, (x => (x as IMyFunctionalBlock).Enabled));
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (!(blocks[i] as IMyProductionBlock).IsProducing)
                    {
                        blocks[i].ApplyAction("OnOff_Off");
                    }
                }
            }
        }

        class ProductionBlockWakeUp
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => !(x as IMyFunctionalBlock).Enabled));
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction("OnOff_On");
                }
            }
        }

        //Kernel begin
        void debug(String line){
            _textPanelLines.AddList(SplitIntoChunks(line, kernel_textPanelColMax));
        }

        private List<string> SplitIntoChunks(string text, int chunkSize)
        {
            List<string> chunks = new List<string>();
            int offset = 0;
            while (offset < text.Length)
            {
                int size = Math.Min(chunkSize, text.Length - offset);
                chunks.Add(text.Substring(offset, size));
                offset += size;
            }
            return chunks;
        }

        void end()
        {
            if (_textPanel is IMyTextPanel)
            {
                Int16 lineMax = kernel_textPanelLineMax - 1;
                if (_textPanelLines.Count > lineMax)
                {
                    _textPanelLines = _textPanelLines.GetRange(_textPanelLines.Count - lineMax, lineMax);
                }
                StringBuilder text = new StringBuilder();
                
                for(int i=0;i<_textPanelLines.Count;i++){
                    text.AppendLine(_textPanelLines[i].Trim());                    
                }
                DateTime now = DateTime.Now;
                long runtime = (now.Ticks / TimeSpan.TicksPerMillisecond) - StartTime;
                _textPanel.WritePublicText("Bauwagen Firmware - " + now.ToString() + " - Laufzeit: " + runtime.ToString() + "ms - Step: " + kernel_current_step.ToString() + "/" + kernel_stepMax.ToString() + "\n" + text.ToString());
            }
        }

        void boot()
        {
            StartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            incStep();
            _textPanel = (GridTerminalSystem.GetBlockWithName(kernel_textPanelName) as IMyTextPanel);
            if (_textPanel is IMyTextPanel)
            {
                _textPanelLines.Clear();
                _textPanelLines.AddArray(_textPanel.GetPublicText().Trim().Split(new String[] { "\n\r", "\r\n", "\n", "\r" }, StringSplitOptions.None));
                if (_textPanelLines.Count > 0)
                {
                    _textPanelLines.RemoveAt(0);
                }
                
            }
        }

        void incStep()
        {
            Int16 _out;
            if (Int16.TryParse(Storage, out _out) == true)
            {
                kernel_current_step = Convert.ToInt16(Storage);
                kernel_current_step++;
            }
            if (kernel_current_step > kernel_max_steps)
            {
                kernel_current_step = 1;
            }
            Storage = kernel_current_step.ToString();
        }

        bool isMatchingStep(Int16 stepMultiplier)
        {
            if (stepMultiplier < 1)
            {
                return false;
            }
            return ((kernel_current_step % stepMultiplier) == 1);
        }
        //Kernel end
// End InGame-Script
    }
}

