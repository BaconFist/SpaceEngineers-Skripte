using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class OnPlayerRange : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           OnPlayerRange
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           
            CAN'T FIND PLAYER . PB & REMOTE MUST BE HOSTILE


           Abstract
           ------------------------------
          
           
           Example
           ------------------------------
          
       */
        string GROUP_TAG = "!PIR!";
        const double CUT_OFF_FACTOR = 0.5;
        const bool DEBUG = true;

        void Main(string args)
        {
            debug("BEGIN");
            if(args.Trim().Length > 0)
            {
                GROUP_TAG = args;
            }

            debug("Groupname: " + GROUP_TAG);

            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(Blocks,(x => x.IsWorking && x.CubeGrid.Equals(Me.CubeGrid)));
            if(Blocks.Count > 0)
            {
                IMyRemoteControl RC = Blocks[0] as IMyRemoteControl;
                debug("Using Remotecontrol: " + RC.CustomName);
                List<IMyBlockGroup> Groups = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(Groups);
                Blocks.Clear();
                for (int i=0; i<Groups.Count; i++)
                {
                    if (Groups[i].Name.Contains(GROUP_TAG))
                    {
                        for (int j = 0; j < Groups[i].Blocks.Count; j++)
                        {
                            if (
                                ((Groups[i].Blocks[j] is IMyRadioAntenna) || (Groups[i].Blocks[j] is IMyBeacon))
                                && Groups[i].Blocks[j].IsFunctional
                                && Groups[i].Blocks[j].CubeGrid.Equals(Me.CubeGrid)
                                )
                            {
                                Blocks.Add(Groups[i].Blocks[j]);
                            }
                        }
                    }
                }

                Vector3D PlayerPos;
                Vector3D PbPos = RC.GetPosition();
                RC.GetNearestPlayer(out PlayerPos);
                debug("Nearest Player: (" + String.Format("{0:0}", Vector3D.Distance(PlayerPos,PbPos)) + " m)" + PlayerPos.ToString());
                debug("Functional Antennas/Beacons: " + Blocks.Count.ToString());
                for(int i = 0; i < Blocks.Count; i++)
                {
                    IMyTerminalBlock RA = Blocks[i] as IMyTerminalBlock;
                    int d_pos = RA.CustomName.IndexOf("DEBUG");
                    string d_name = RA.CustomName.Trim();
                    if (d_pos > -1)
                    {
                        d_name = RA.CustomName.Substring(0, d_pos).Trim();
                        RA.SetCustomName(d_name);
                    }

                    double radius = 0.0;
                    if (RA is IMyRadioAntenna)
                    {
                        radius = Convert.ToDouble((RA as IMyRadioAntenna).Radius);
                    }
                    else if (RA is IMyBeacon)
                    {
                        radius = Convert.ToDouble((RA as IMyBeacon).Radius);
                    }                        

                    double cutOffPoint = radius * CUT_OFF_FACTOR;
                    double distance = Vector3D.Distance(PlayerPos, PbPos);
                    string info = "% (radius: " + String.Format("{0:0}", radius) + " m; cut-off point: " + String.Format("{0:0}", cutOffPoint) + " m; distance: " + String.Format("{0:0}", distance) + " m)";
                    if (distance < cutOffPoint)
                    {
                        info = "Enable: "+ info;
                      //  RA.ApplyAction("OnOff_On");
                    } else
                    {
                        info = "Disable '" + info;
                      //  RA.ApplyAction("OnOff_Off");
                    }
                    Echo(info.Replace("%", RA.CustomName));

                    if (DEBUG)
                    {
                        d_pos = RA.CustomName.IndexOf("DEBUG");
                        d_name = RA.CustomName.Trim();
                        if(d_pos > -1)
                        {
                            d_name = RA.CustomName.Substring(0, d_pos).Trim();
                        }
                        RA.SetCustomName(d_name + " DEBUG: " + info.Replace("%",""));
                    } 
                }
            } else
            {
                debug("Remote Control not found.");
            }
            debug("END");
        }

        public void debug(string msg)
        {
            if (DEBUG)
            {
                Echo(msg);
            }
        }


        #endregion
    }
}
