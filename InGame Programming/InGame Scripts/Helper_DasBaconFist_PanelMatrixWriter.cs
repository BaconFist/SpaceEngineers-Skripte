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
    
    class Helper_DasBaconFist_PanelMatrixWriter
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script

        void Main()
        {
           // Description in class                       
           // (new DasBaconFist_PanelMatrixWriter(GridTerminalSystem)).writeToTextPanelMatrix("some text you want to display", "MyAwesomeMatrix", 2, 2, 10, 30);
        }

        class DasBaconFist_PanelMatrixWriter
        {
            /*
             * WTF?
             * ====
             * This class can write Text to multiple panels arranged in a grid/matrix as a 2x2 Field or 3x5 field or whatever.
             * Features 
             * > use multiple IMyTextPanel as one big Display
             * > automatic wordwrap included
             * 
             * License
             * =======
             * WTFPL – Do What the Fuck You Want to Public License
             * http://www.wtfpl.net/
             * 
             * Usage
             * =====
             * - Name you panels with (TextPanelMatrix:*Matrix-Id*:*rownumber*:*colnumer*)
             * - call (new DasBaconFist_PanelMatrixWriter(GridTerminalSystem)).writeToTextPanelMatrix(theTextToWrite, "Matrix-Id", *number of rows*, *number of columns*, *number of lines that fit on one TextPanel*, *number of chars in a line per panel*);
             * 
             * Example
             * =======
             * If you have build 4 Panels like this: top-left,top-right,bottom-left,bottom-right.
             * On one panel is place for 10 lines in height and 30 characters in width (depends on Fontsize) 
             * 
             * You Choose the Matrix-ID as "MyAwesomeMatrix"
             * top-left Panel's Name must include "(TextPanelMatrix:MyAwesomeMatrix:0:0)"
             * top-right Panel's Name must include "(TextPanelMatrix:MyAwesomeMatrix:0:1)"
             * bottom-left Panel's Name must include "(TextPanelMatrix:MyAwesomeMatrix:1:0)"
             * bottom-left Panel's Name must include "(TextPanelMatrix:MyAwesomeMatrix:1:1)"
             *      (!?) => "TextPanelMatrix" is static, dont change it or it would not work
             *      (!?) =>  "MyAwesomeMatrix" is a unique per-matrix id. Must be the same on all Panels in one Matrix
             *      (!?) => first number is the zero-based index of the TextPanel-Row (from top to bottom)
             *      (!?) => first number is the zero-based index of the TextPanel-Column (from left to right)
             * 
             * 
             * with this setup you have to call it like this:
             *      (new DasBaconFist_PanelMatrixWriter(GridTerminalSystem)).writeToTextPanelMatrix("some text you want to display", "MyAwesomeMatrix", 2, 2, 10, 30);
             *                                                                                               ||                             ||          |  |   |   |
             *        text to be written --------------------------------------------------------------------^^                             ||          |  |   |   |
             *        Unique Id of the TextPanelMatrix -------------------------------------------------------------------------------------^^          |  |   |   |
             *        number of rows (panels from top to bottom) ---------------------------------------------------------------------------------------^  |   |   |
             *        number of columns (panels from left to right) ---------------------------------------------------------------------------------------^   |   |
             *        lines per panel -------------------------------------------------------------------------------------------------------------------------^   |
             *        chars per line per panel --------------------------------------------------------------------------------------------------------------------^           * 
             *      
             * 
             */

            IMyGridTerminalSystem GridTerminalSystem;

            public DasBaconFist_PanelMatrixWriter(IMyGridTerminalSystem SE_GridTerminalSystem)
            {
                GridTerminalSystem = SE_GridTerminalSystem;
            }

            
            public void writeToTextPanelMatrix(string rawText, string matrixId, int rows, int cols, int panelLines, int panelChars)
            {
                List<List<IMyTextPanel>> panelMatrix = panelMatrixBuilder("test", 2, 2);
                writeToTextPanelMatrix(rawText, panelMatrix, panelLines, panelChars);
            }

            private List<List<IMyTextPanel>> panelMatrixBuilder(string matrixId, int rows, int cols)
            {
                List<List<IMyTextPanel>> matrix = new List<List<IMyTextPanel>>();
                for (int i_row = 0; i_row < rows; i_row++)
                {
                    List<IMyTextPanel> row = new List<IMyTextPanel>();
                    for (int i_col = 0; i_col < cols; i_col++)
                    {
                        string uid = "(TextPanelMatrix:" + matrixId + ":" + i_row.ToString() + ":" + i_col.ToString() + ")";
                        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks, (x => x.CustomName.Contains(uid)));
                        if (blocks.Count > 0)
                        {
                            row.Add(blocks[0] as IMyTextPanel);
                        }
                    }
                    matrix.Add(row);
                }

                return matrix;
            }

            private void writeToTextPanelMatrix(string rawText, List<List<IMyTextPanel>> panelMatrix, int panelLines, int panelChars)
            {
                string text = rawText;
                int panelRowCount = panelMatrix.Count;
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
                        if (i_linesCount % panelLines == 0)
                        {
                            panelIndex++;
                            panelColumnCount = panelMatrix[panelIndex].Count;
                            for (int i = 0; i < panelColumnCount; i++)
                            {
                                panelMatrix[panelIndex][i].WritePublicText("", false);
                            }
                        }
                        for (int i_indexPanelCol = 0; i_indexPanelCol < panelColumnCount; i_indexPanelCol++)
                        {
                            int subCount = (panelChars > text.Length) ? text.Length : panelChars;
                            subLenght = text.LastIndexOf(" ", subCount) + 1;
                            nextBreak = text.IndexOf('\n');
                            if (nextBreak > -1 && nextBreak < subLenght)
                            {
                                subLenght = nextBreak - subStart;
                            }

                            if (subLenght > text.Length)
                            {
                                subLenght = text.Length;
                            }

                            lineBreak = (text.IndexOf('\n') == subLenght) ? "" : "\n";

                            int textSubCount = (subLenght > -1) ? subLenght : 1;
                            panelMatrix[panelIndex][i_indexPanelCol].WritePublicText(text.Substring(0, textSubCount) + lineBreak, true);
                            text = text.Remove(0, textSubCount);
                        }
                    }
                }
            }
        }
        
        // End InGame-Script
    }
}
