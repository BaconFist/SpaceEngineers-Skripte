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
        const Int16 kernel_max_steps = 30;
        Int16 kernel_current_step = 1;
        IMyTextPanel _textPanel;
        List<String> _textPanelLines = new List<string>();
        long StartTime; 

        void Main()
        {
            boot();
            //Programms BEGIN
            (new ReactorInfo()).run(GridTerminalSystem, ReactorInfo_textPanel, ReactorInfo_reactor);
            debug("[" + DateTime.Now.ToString() + "] ReactorInfo.");
            (new Clock()).run(GridTerminalSystem, Clock_textPanel);
            debug("[" + DateTime.Now.ToString() + "] Clock.");
            (new RefreshTextPanel()).run(GridTerminalSystem);
            debug("[" + DateTime.Now.ToString() + "] RefreshTextPanel.");
            if (isMatchingStep(5))
            {
                (new DetailedCargoDisplay()).run(GridTerminalSystem);
                debug("[" + DateTime.Now.ToString() + "] DetailedCargoDisplay.");
            }
            if (isMatchingStep(10))
            {
                (new ProductionBlockStandBy()).run(GridTerminalSystem);
                debug("[" + DateTime.Now.ToString() + "] ProductionBlockStandBy.");
            }
            if (isMatchingStep(30))
            {
                (new AssemblerCleaning()).run(GridTerminalSystem, AssemblerCleaning_target_cargo);
                debug("[" + DateTime.Now.ToString() + "] AssemblerCleaning.");
            }
            if (isMatchingStep(30))
            {
                (new ProductionBlockWakeUp()).run(GridTerminalSystem);
                debug("[" + DateTime.Now.ToString() + "] ProductionBlockWakeUp.");
            }            
            //Programms END
            end();
        }

        class RefreshTextPanel
        {
            public void run(IMyGridTerminalSystem GridTerminalSystem)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks, (x => (x as IMyFunctionalBlock).Enabled));
                for (int i = 0; i < blocks.Count; i++)
                {
                    (blocks[i] as IMyFunctionalBlock).ApplyAction("OnOff_Off");
                    (blocks[i] as IMyFunctionalBlock).ApplyAction("OnOff_On");
                }
            }
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

        class DetailedCargoDisplay
        {

            //options
            const String textPanelNamesCSV = ";Textpanel DetailedCargoDisplay 1;Textpanel DetailedCargoDisplay 2";  // 1st char is seperator
            const Int16 inventorySelectionMode = 0; // 0: All; 1: By Group; 2: By Names; 3: By Substring in Name;
            const String inventorySelectionGroupNamesCSV = ";";  // 1st char is seperator
            const String inventorySelectionNamesCSV = ";";  // 1st char is seperator 
            const String inventorySelectionSubstringsCSV = ";";  // 1st char is seperator
            const Int16 textPanelMaxLines = 60;

            const Int16 ISM_ALL = 0;
            const Int16 ISM_GROUPS = 1;
            const Int16 ISM_NAMES = 2;
            const Int16 ISM_SUB = 3;

            IMyGridTerminalSystem GridTerminalSystem;

            public void run(IMyGridTerminalSystem _GridTerminalSystem)
            {
                GridTerminalSystem = _GridTerminalSystem;
                
                List<IMyTextPanel> textPanels = getTextPanelsFromCSV(textPanelNamesCSV);
                if (textPanels.Count > 0)
                {
                    List<IMyTerminalBlock> blocks = getCargoContainers();
                    if (blocks.Count > 0)
                    {
                        Dictionary<String, double> items = new Dictionary<string, double>();
                        List<String> keys = new List<string>();
                        for (int i_blocks = 0; i_blocks < blocks.Count; i_blocks++)
                        {
                            for (int i_inventory = 0; i_inventory < blocks[i_blocks].GetInventoryCount(); i_inventory++)
                            {
                                for (int i_item = 0; i_item < blocks[i_blocks].GetInventory(i_inventory).GetItems().Count; i_item++)
                                {
                                    IMyInventoryItem item = blocks[i_blocks].GetInventory(i_inventory).GetItems()[i_item];
                                    if (!items.ContainsKey(item.Content.SubtypeName))
                                    {
                                        items[item.Content.SubtypeName] = 0;
                                        keys.Add(item.Content.SubtypeName);
                                    }
                                    items[item.Content.SubtypeName] += Convert.ToDouble(item.Amount.ToString());
                                }
                            }
                        }
                        IMyTextPanel textPanel = textPanels[0];
                        Int16 i_textPanel = 0;
                        for (int i_key = 0; i_key < keys.Count; i_key++)
                        {
                            if (i_textPanel % (textPanelMaxLines - 1) == 0)
                            {
                                textPanel = textPanels[i_textPanel];
                                if (!(textPanel is IMyTextPanel))
                                {
                                    break;
                                }
                                textPanel.WritePublicText("Inventory Index - " + DateTime.Now.ToString(), false);
                                i_textPanel++;
                            }
                            textPanel.WritePublicText(keys[i_key] + ": " + String.Format("{0:N}", items[keys[i_key]]) + "\n");
                        }
                    }
                }
            }


            List<IMyTerminalBlock> getCargoContainers()
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                if (inventorySelectionMode == ISM_ALL)
                {
                    blocks = getInventoryBlocks(GridTerminalSystem.Blocks);
                }
                else if (inventorySelectionMode == ISM_GROUPS)
                {
                    String seperator = inventorySelectionGroupNamesCSV.Substring(0, 1);
                    String groupNames = inventorySelectionGroupNamesCSV + seperator;
                    List<IMyBlockGroup> groups = GridTerminalSystem.BlockGroups.FindAll(x => groupNames.Contains(seperator + x.Name + seperator));
                    for (int i = 0; i < groups.Count; i++)
                    {
                        blocks.AddList<IMyTerminalBlock>(getInventoryBlocks(groups[i].Blocks));
                    }
                }
                else if (inventorySelectionMode == ISM_NAMES)
                {
                    String seperator = inventorySelectionNamesCSV.Substring(0, 1);
                    String names = inventorySelectionNamesCSV + seperator;
                    blocks = GridTerminalSystem.Blocks.FindAll(x => (inventorySelectionNamesCSV.Contains(seperator + x.CustomName + seperator) && x.HasInventory()));
                }
                else if (inventorySelectionMode == ISM_SUB)
                {
                    List<String> subs = readCSV(inventorySelectionSubstringsCSV);
                    for (int i_subs = 0; i_subs < subs.Count; i_subs++)
                    {
                        blocks.AddList<IMyTerminalBlock>(GridTerminalSystem.Blocks.FindAll(x => (x.CustomName.Contains(subs[i_subs]) && x.HasInventory())));
                    }
                }

                return blocks;
            }

            List<IMyTerminalBlock> getInventoryBlocks(List<IMyTerminalBlock> list)
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                for (int i = 0; i < list.Count; i++)
                {
                    IMyTerminalBlock block = list[i];
                    if (block.HasInventory())
                    {
                        blocks.Add(block);
                    }
                }
                return blocks;
            }



            List<String> readCSV(String csv)
            {
                String seperator = csv.Substring(0, 1);
                return csv.Split(new String[] { seperator }, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
            }

            List<IMyTextPanel> getTextPanelsFromCSV(String csv)
            {
                List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
                List<String> textPanelNames = readCSV(textPanelNamesCSV);

                for (int i = 0; i < textPanelNames.Count; i++)
                {
                    IMyTextPanel tempTextPanel = (GridTerminalSystem.GetBlockWithName(textPanelNames[i]) as IMyTextPanel);
                    if (tempTextPanel is IMyTextPanel)
                    {
                        textPanels.Add(tempTextPanel);
                    }
                }

                return textPanels;
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
                _textPanel.WritePublicText("Bauwagen Firmware - " + now.ToString() + " - Laufzeit: " + runtime.ToString() + "ms - Step: " + kernel_current_step.ToString() + "/" + kernel_max_steps.ToString() + "\n" + text.ToString());
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

