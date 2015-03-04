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
        const String main_cargo = "Frachtcontainer Bauwagen";
        const String main_textPanelName = "LCD Panel Firmware Ausgabe";
        const Int32 main_textPanelLineMax = 35;
        const Int32 main_textPanelColMax = 70;
        const String ReactorInfo_textPanel = "Text Panel Reaktor Info";
        const String ReactorInfo_reactor = "Reaktor (klein)";
        const String Clock_textPanel = "Textpanel Uhr";
        // Run-Settings
        const Int16 _stepMax = 100;
        Int16 _step = 1;
        IMyTextPanel _textPanel;
        List<String> _textPanelLines = new List<string>();
        long StartTime; 

        void Main()
        {
            boot();
            //Programms BEGIN
            ReactorInfo();
            Clock();
            if (isMatchingStep(30))
            {
                AssemblerCleaning();
                debug("[" + DateTime.Now.ToString() + "] Fertigungsroboter bereinigt.");    
            }
            //Programms END
            end();
        }

        void Clock()
        {
            IMyTextPanel uhr = (GridTerminalSystem.GetBlockWithName(Clock_textPanel) as IMyTextPanel);
            if (uhr is IMyTextPanel)
            {
                uhr.WritePublicText(DateTime.Now.ToString().Replace(" ", "\n"));
            }
        }

        void AssemblerCleaning()
        {
            IMyCargoContainer cargo = (GridTerminalSystem.GetBlockWithName(main_cargo) as IMyCargoContainer);
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

        void ReactorInfo()
        {
            IMyTextPanel textPanel = (GridTerminalSystem.GetBlockWithName(ReactorInfo_textPanel) as IMyTextPanel);
            IMyReactor reactor = (GridTerminalSystem.GetBlockWithName(ReactorInfo_reactor) as IMyReactor);
            if ((textPanel is IMyTextPanel) && (reactor is IMyReactor))
            {
                textPanel.WritePublicText(reactor.DetailedInfo + "\n\n letzte Aktualisierung:\n" + DateTime.Now.ToString());
            }
        }

        //Kernel begin
        void debug(String line){
            _textPanelLines.AddList(SplitIntoChunks(line, main_textPanelColMax));
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
                Int16 lineMax = main_textPanelLineMax - 1;
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
                _textPanel.WritePublicText("Bauwagen Firmware - " + now.ToString() + " - Laufzeit: " + runtime.ToString() + "ms - Step: " + _step.ToString() + "/" + _stepMax.ToString() + "\n" + text.ToString());
            }
        }

        void boot()
        {
            StartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            incStep();
            _textPanel = (GridTerminalSystem.GetBlockWithName(main_textPanelName) as IMyTextPanel);
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
                _step = Convert.ToInt16(Storage);
                _step++;
            }
            if (_step > _stepMax)
            {
                _step = 1;
            }
            Storage = _step.ToString();
        }

        bool isMatchingStep(Int16 stepMultiplier)
        {
            if (stepMultiplier < 1)
            {
                return false;
            }
            return ((_step % stepMultiplier) == 1);
        }

        //Kernel end
// End InGame-Script
    }
}

