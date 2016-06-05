using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class BFsPresets : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           BFsPresets
           ==============================
           Copyright (c) 2016 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
            Cloning properties from one block to others.
           
           Abstract
           ------------------------------
            Clone all settings from a Master-Block to a bunch on Slave-Blocks
            
            1 -  Load Program to a PB
            2 -  add (!MSC:My group of Lights) the Block you want as Master
            3 -  add (MSC:My group of Lights) to all Blocks you want as Slaves
            4 -  adjust the settings of Master-Block
            5 -  run the Program and all settings are cloned from Master to all Slaves         
                                  
       */


        public void Main(string argument)
        {
            Dictionary<string, MasterSlaveGroup>  matches = findGroups();
            List<MasterSlaveGroup> groups = new List<MasterSlaveGroup>(matches.Values);
            for (int gi = 0; gi < groups.Count; gi++)
            {
                updateSlaves(groups[gi]);
            }
        }

        public void updateSlaves(MasterSlaveGroup Group)
        {
            for(int m=0; m < Group.Masters.Count; m++)
            {
                IMyTerminalBlock Master = Group.Masters[m];
                for(int s = 0; s < Group.Slaves.Count; s++)
                {
                    IMyTerminalBlock Slave = Group.Slaves[s];
                    updateSlave(Master, Slave);
                }
            }
        }

        public void updateSlave(IMyTerminalBlock Master, IMyTerminalBlock Slave)
        {
            if (Master.BlockDefinition.TypeIdString.Equals(Slave.BlockDefinition.TypeIdString))
            {
                List<ITerminalProperty> Properties = new List<ITerminalProperty>();
                Master.GetProperties(Properties);
                for (int pi = 0; pi < Properties.Count; pi++)
                {
                    ITerminalProperty Property = Properties[pi];
                    switch (Property.TypeName)
                    {
                        case "Boolean":
                            Property.AsBool().SetValue(Slave, Property.AsBool().GetValue(Master));
                            break;
                        case "Single":
                            Property.AsFloat().SetValue(Slave, Property.AsFloat().GetValue(Master));
                            break;
                        case "Color":
                            Property.AsColor().SetValue(Slave, Property.AsColor().GetValue(Master));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public Dictionary<string, MasterSlaveGroup> findGroups()
        {
            System.Text.RegularExpressions.Regex RgxSlave = new System.Text.RegularExpressions.Regex(@"\(MSC\:([^)]+)\)");
            System.Text.RegularExpressions.Regex RgxMaster = new System.Text.RegularExpressions.Regex(@"\(\!MSC\:([^)]+)\)");

            Dictionary<string, MasterSlaveGroup> Groups = new Dictionary<string, MasterSlaveGroup>();

            List<IMyTerminalBlock> BlocksCache = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(BlocksCache, (x => x.CubeGrid.Equals(Me.CubeGrid) && x.CustomName.Contains("MSC")));
            for (int bci = 0; bci < BlocksCache.Count; bci++)
            {
                IMyTerminalBlock Block = BlocksCache[bci];
                if (RgxMaster.IsMatch(Block.CustomName))
                {
                    addMatch(RgxMaster, Groups, Block, true);
                } else if (RgxSlave.IsMatch(Block.CustomName))
                {
                    addMatch(RgxSlave, Groups, Block);
                }
            }

            return Groups;
        }

        public void addMatch(System.Text.RegularExpressions.Regex Rgx, Dictionary<string, MasterSlaveGroup> Groups, IMyTerminalBlock Block, bool isMaster = false)
        {
            System.Text.RegularExpressions.MatchCollection Matches = Rgx.Matches(Block.CustomName);
            string name = Matches[0].Value.Replace("(!MSC:", "").Replace("(MSC:", "").Replace(")","");
            if (!Groups.ContainsKey(name))
            {
                Groups.Add(name, new MasterSlaveGroup());
            }
            if (isMaster)
            {
                Groups[name].Masters.Add(Block);
            } else
            {
                Groups[name].Slaves.Add(Block);
            }
        }

        public class MasterSlaveGroup
        {
            public List<IMyTerminalBlock> Slaves = new List<IMyTerminalBlock>();
            public List<IMyTerminalBlock> Masters = new List<IMyTerminalBlock>();
        }
            

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}
