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
    class Helper_DasBaconfist_WordWrap
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("Textpanel") as IMyTextPanel;
            if (panel is IMyTextPanel)
            {
                panel.WritePublicText(wordWrap("This class can write Text to multiple panels arranged in a grid/matrix as a 2x2 Field or 3x5 field or whatever.", 30));
            }
        }

        /*
         * WTF?
         * ====
         * automatic word-wrap
         * 
         * > text = text to be wrapped
         * > lineWidth = max characters per line
         * > keepLineBreaks =   true-> keep existing linebreaks, false-> remove linebreakes and repalce with space
         */
        public string wordWrap(string text, int lineWidth, bool keepLineBreaks = true)
        {
            Char[] trimChars = new Char[] { ' ' , '\n'};
            StringBuilder wrapped = new StringBuilder();
            if (keepLineBreaks == false)
            {
                text = text.Replace('\n', ' ');
            }

            int loopLimit = text.Length;

            text = text.Trim(trimChars);
            for (int i = 0; (text.Length > 0) && i < loopLimit; i++)
            {
                int maxChars = (lineWidth < text.Length)?lineWidth:text.Length;
                int count = text.LastIndexOf(' ', maxChars-1);
                if (keepLineBreaks == true)
                {
                    int newLine = text.IndexOf('\n');
                    count = (newLine != -1 && newLine < count) ? newLine : count;
                }                
                count = (count == -1) ? maxChars : count;
                wrapped.AppendLine(text.Substring(0, count).Trim(trimChars));
                text = text.Remove(0, count).Trim(trimChars);
            }

            return wrapped.ToString();
        }

        // End InGame-Script
    }
}
