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
    
    class FormatHelper
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script

        // demo begin
        void Main()
        {
            String[][] panels = new String[][] {
                new String[] { "TextPanel 1_1", "TextPanel 1_2", "TextPanel 1_3", }, 
                new String[] { "TextPanel 2_1", "TextPanel 2_2", "TextPanel 2_3", }, 
                new String[] { "TextPanel 3_1", "TextPanel 3_2", "TextPanel 3_3" } 
            };

            string superLongText = "imagine a super long text";

            List<List<IMyTextPanel>> panelMatrix = buildPanelMatrix(panels);

            writeToTextPanelMatrix(superLongText, panelMatrix, 10, 30);

            //append to existingn text
            string text = demo_getTextFromPanelMatrix(panelMatrix);
            writeToTextPanelMatrix(text + superLongText, panelMatrix, 10, 30);
        }


        public string demo_getTextFromPanelMatrix(List<List<IMyTextPanel>> panelMatrix)
        {
            string text = "";
            for (int i_row = 0; i_row < panelMatrix.Count(); i_row++)
            {
                for (int i_col = 0; i_col < panelMatrix[i_row].Count(); i_col++)
                {
                    text += panelMatrix[i_row][i_col].GetPublicText();
                }
            }

            return text;
        }

        public List<List<IMyTextPanel>> buildPanelMatrix(String[][] panels)
        {
            List<List<IMyTextPanel>> panelMap = new List<List<IMyTextPanel>>();
            for (int i = 0; i < panels.Length; i++)
            {
                List<IMyTextPanel> temp = new List<IMyTextPanel>();
                for (int k = 0; k < panels[i].Length; k++)
                {
                    IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(panels[i][k]);
                    if (block is IMyTextPanel)
                    {
                        temp.Add(block as IMyTextPanel);
                    }
                }
                panelMap.Add(temp);
            }
            return panelMap;
        }
        // demo end


        /**
         * writeToTextPanelMatrix
         * 
         * - Writes [text] to multiple IMyTextPanels.
         * - automatic word-wrap
         * 
         * @param string text: the Text to Display
         * @param List<List<IMyTextPanel>> panelMatrix: a List of Textpanel-Rows. (top-left is first in list of list)
         * @param int panelLines: max Lines per Panel
         * @param int panelChars: max Chars per Line on a Panel
         */
        public void writeToTextPanelMatrix(string text, List<List<IMyTextPanel>> panelMatrix, int panelLines, int panelChars)
        {
            int panelRowCount = panelMatrix.Count();
            int panelColumnCount = 0;
            int linesAvailable = panelLines * panelRowCount;
            int panelIndex = -1;
            int subLenght = panelChars;
            int subStart = 0;
            int nextBreak = 0;
            string lineBreak = "";
            if (panelRowCount > 0 && panelLines > 0 && panelChars > 0)
            {
                for (int i_linesCount = 0; i_linesCount < linesAvailable && panelIndex < panelRowCount; i_linesCount++)
                {
                    panelColumnCount = panelMatrix[panelIndex].Count();
                    if (i_linesCount % panelLines == 0)
                    {
                        panelIndex++;
                        for (int i = 0; i < panelColumnCount; i++)
                        {
                            panelMatrix[panelIndex][0].WritePublicText("", true);
                        }
                    }
                    for (int i_indexPanelCol = 0; i_indexPanelCol < panelColumnCount;i_indexPanelCol++)
                    {
                        subLenght = text.LastIndexOfAny(new Char[] { ' ', '\n' }, 0, panelChars);
                        nextBreak = text.IndexOf('\n');
                        if (nextBreak < subLenght)
                        {
                            subLenght = nextBreak - subStart;
                        }
                        if (subLenght > text.Length)
                        {
                            subLenght = text.Length;
                        }
                        if (i_indexPanelCol == panelColumnCount - 1)
                        {
                            lineBreak = (text.IndexOf('\n') == subLenght)? "" : "\n";
                        }                        
                        panelMatrix[panelIndex][i_indexPanelCol].WritePublicText(text.Remove(0, subLenght) + lineBreak, true);  
                    }                                     
                }
            }
        }
        
        // End InGame-Script
    }
}
