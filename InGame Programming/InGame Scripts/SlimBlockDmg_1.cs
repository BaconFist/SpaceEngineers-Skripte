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
    class SlimBlockDmg_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {
            IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("Breiter LCD-Schirm") as IMyTextPanel;
            if (panel is IMyTextPanel)
            {
                
                StringBuilder output = new StringBuilder();
                output.AppendLine(DateTime.Now.ToString());
                IMyCubeGrid grid = GridTerminalSystem.Blocks[0].CubeGrid;
                IMySlimBlock slim;

                Pos posMin = new Pos(grid.Min);
                Pos posMax = new Pos(grid.Max);
                int x_max = posMax.X;
                int y_max = posMax.Y;
                int z_max = posMax.Z;
                output.AppendLine("Min: " + posMin.ToString() + "; Max: " + posMax.ToString());
                output.AppendLine("Min: " + posMin.X.ToString() + ";" + posMin.Y.ToString() + ";" + posMin.Z.ToString() + "; Max: " + x_max + ";" + y_max + ";" + z_max );
                for (int x = posMin.X; x < x_max; x++)
                {
                    for (int y = posMin.Y; y < y_max; y++)
                    {
                        for (int z = posMin.Z; z < z_max; z++)
                        {
                            slim = null;
                            Vector3I vector = new Vector3I(x, y, z);
                            try
                            {                                
                                slim = grid.GetCubeBlock(vector);                                
                                           }
                            catch (Exception)
                            {
                            }
                            if (slim is IMySlimBlock)
                            {
                                if (slim.FatBlock is IMyCubeBlock)
                                {
                                    output.Append(" Name: " + slim.FatBlock.DisplayNameText);
                                }
                                output.Append(" Vector: " + vector.ToString());
                                output.AppendLine(getSlimBlockDmg(slim));                                
                            }
                        }
                    }
                }
                panel.WritePublicText(output.ToString());
            }           
        }

        public string getSlimBlockDmg(IMySlimBlock slim)
        {
            string r = "";
            r += "DamageRatio: " + ratioFrmt(slim.DamageRatio) + " ";
            r += "BuildLevelRatio: " + ratioFrmt(slim.BuildLevelRatio) + " ";
            r += "AccumulatedDamage: " + ratioFrmt(slim.AccumulatedDamage) + " ";

            return r;
        }

        public String ratioFrmt(float num, string frm = "{0:N3}")
        {
            return format(num * 100, frm);
        }

        public String format(float num, string frm = "{0:N3}")
        {
            return String.Format(frm, num);
        }

        class Pos
        {
            public int X = 0;
            public int Y = 0;
            public int Z = 0;

            public Pos(Vector3I vector)
            {

                string[] parts = remove(vector.ToString()).Split(new Char[] { ',' });
                X = Int32.Parse(parts[0]);
                Y = Int32.Parse(parts[1]);
                Z = Int32.Parse(parts[2]);
            }

            private string remove(string txt)
            {
                return txt.Replace("[","").Replace("X","").Replace(":","").Replace("Y","").Replace("Z","").Replace("]","");
            }

            public override string ToString()
            {
                return "[X:" + X.ToString() + ", Y:" + Y.ToString() + ", Z:" + Z.ToString() + "]";
            }
        }
        // End InGame-Script
    }
}
