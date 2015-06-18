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
            DasBaconFist_PanelMatrixWriter writer = new DasBaconFist_PanelMatrixWriter(GridTerminalSystem);
            writer.AutoUpdate();
        }

        class DasBaconFist_PanelMatrixWriter
        {
            /*
             * Version 1.1.0
             * 
             * WTF?
             * ====
             * This class can write Text to multiple panels arranged in a grid/matrix as a 2x2 Field or 3x5 field or whatever.
             * Features 
             * > use multiple IMyTextPanel as one big Display
             * > automatic wordwrap included
             * > update 1: 
             *          - automatic writing from other panel (mark the TextPanel u want to display on a Matrix with (TextPanelMatrix => *targetMatriyId )
             *          - line and col count is now resolved by the first panels (0:0) fonstSize -> chars is now Size * 100 & lines is size * 30
             *          - row & col count not longer required
             * 
             * 
             * License
             * =======
             * WTFPL – Do What the Fuck You Want to Public License
             * http://www.wtfpl.net/
             * 
             * Coded by: http://steamcommunity.com/profiles/76561198018146795/
             * 
             * Update & Bugs
             * =============
             * find latest Version at GitHub: http://git.io/hfAh
             * report bugs at GitHub (http://git.io/hfxk) or write a comment on Steam
             * 
             * Usage
             * =====
             * - Name you panels with (TextPanelMatrix:*Matrix-Id*:*rownumber*:*colnumer*)
             * - call (new DasBaconFist_PanelMatrixWriter(GridTerminalSystem)).writeToTextPanelMatrix(theTextToWrite, "Matrix-Id");
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
             *      (new DasBaconFist_PanelMatrixWriter(GridTerminalSystem)).writeToTextPanelMatrix("some text you want to display", "MyAwesomeMatrix");
             *                                                                                               ||                             ||       
             *        text to be written --------------------------------------------------------------------^^                             ||       
             *        Unique Id of the TextPanelMatrix -------------------------------------------------------------------------------------^^       
             *      
             * 
             */

            IMyGridTerminalSystem GridTerminalSystem;

            public DasBaconFist_PanelMatrixWriter(IMyGridTerminalSystem SE_GridTerminalSystem)
            {
                GridTerminalSystem = SE_GridTerminalSystem;
            }
            
            public void writeToTextPanelMatrix(string rawText, string matrixId)
            {
                List<List<IMyTextPanel>> panelMatrix = panelMatrixBuilder(matrixId);
                writeToTextPanelMatrix(rawText, panelMatrix);
            }

            private List<List<IMyTextPanel>> panelMatrixBuilder(string matrixId)
            {
                Size size = resolveMatrixSize(matrixId);
                int rows = size.row;
                int cols = size.col;

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

            private void writeToTextPanelMatrix(string rawText, List<List<IMyTextPanel>> panelMatrix)
            {
                if (! (panelMatrix.Count > 0 && panelMatrix[0].Count > 0))
                {
                    return;
                }
               // int panelLines = Convert.ToInt32(Math.Floor(Convert.ToDouble(panelMatrix[0][0].GetProperty("FontSize")) * 30));
               // int panelChars = Convert.ToInt32(Math.Floor(Convert.ToDouble(panelMatrix[0][0].GetProperty("FontSize")) * 100));

                 int panelLines = 15;
                 int panelChars = 60;

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


            /* writes Text from a single Panel to the Matrix
             * add following to the source (Panel with the text you want to display) => (TextPanelMatrix => *Matrix-Id*)
             * example: 
             * LCDs with matrix on it => "LCD-Screen 17 (TextPanelMatrix:TheCoon:3:2)
             * LCD wirh source => "LCD-Screen 32 My Cargo (TextPanelMatrix => TheCoon)
             *  now it will push the Text from LCD-Screen 32 to the Matrix defined in LCD-Screen 17
             * 
             * */
            public void AutoUpdate()
            {
                List<IMyTerminalBlock> sourcePanels = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(sourcePanels, (x => x.CustomName.Contains("TextPanelMatrix =>")));
                for (int i = 0; i < sourcePanels.Count; i++)
                {
                    string matrixId = parseTargetMatrix(sourcePanels[i].CustomName);
                    if (matrixId != null)
                    {
                        this.writeToTextPanelMatrix((sourcePanels[i] as IMyTextPanel).GetPublicText(), matrixId);
                    }
                }
            }

            private string parseTargetMatrix(string customName)
            {
                int start = customName.IndexOf("TextPanelMatrix =>");
                int count = customName.IndexOf(")", start) - start;
                String[] parts = customName.Substring(start, count).Split('>');
                if (parts.Length == 2)
                {
                    return parts[1].Trim(new Char[]{')',' '});
                }

                return null;
            }

            private Size resolveMatrixSize(string MatrixId)
            {
                Int32 col = 0;
                Int32 row = 0;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, (x => x.CustomName.Contains(MatrixId)));
                for (int i = 0; i < blocks.Count; i++)
                {
                    int start = blocks[i].CustomName.IndexOf("(TextPanelMatrix");
                    int count = blocks[i].CustomName.IndexOf(")", start) - start;

                    //0: TextPanelMatrix / 1:MatrixId / 2: rows / 3:cols
                    String[] parts = blocks[i].CustomName.Substring(start, count).Split(':');
                    if (parts.Length == 4)
                    {
                        Int32 t_row;
                        if (Int32.TryParse(parts[2].Trim(new Char[]{':',')'}), out t_row))
                        {
                            row = Math.Max(row, t_row);
                        }
                        Int32 t_col;
                        if (Int32.TryParse(parts[3].Trim(new Char[] { ':', ')' }), out t_col))
                        {
                            col = Math.Max(col, t_col);
                        }
                    }
                }
                Size size = new Size();
                size.col = col;
                size.row = row;

                return size;
            }

            class Size
            {
                public Int32 col;
                public Int32 row;
            }

        }
        
        // End InGame-Script
    }
}
