using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class StargateModDHDAdressChanger : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           StargateModDHDAdressChanger
           ==============================
           Copyright (c) 2016 Thomas Klose <thomas@bratler.net>
           Source:  https://github.com/BaconFist/SpaceEngineers-Skripte/blob/master/InGame%20Programming/IBlockScripts/IBlockScripts/Controller/StargateModDHDAdressChanger.cs
           
           Summary
           ------------------------------
           Change your Stargates address with a press of a button.

           Made for this Stargate Mod: http://steamcommunity.com/sharedfiles/filedetails/?id=307034320
            
           Install Guide
           ------------------------------
            1. Build a Stargate.
            2. Build at least one DHD. (You can use as many as you want)
            3. Build a Programmable Block and load this script.
            4. Build a TextPanel to display the address. (You can use as many as you want)
            5. Choose a TAG. (could be [MYDHDSTUFF] or anything el/se.)
            6. Include this tag in all DHDs and TextPanels you have set up in Steps 2 and 4
            7. Run the script with argument "install@[MYDHDSTUFF]". (repalce [MYDHDSTUFF] with your tag)
            8. ???
            9. Profit.

             
                                  
           How to alter an DHDs address
           ------------------------------
            ! - You have to setup this script like describes in the Install Guide - !

            1. Call the Programmable Block with the new address as an argument.
            2. Done.
                                              
       */

        // Change this to alter the text on LCDs //
        const string format = "Stargate address: {address}";

        // Do not modify below this line. //
        const string cmd_install = "install@";
        string DHDBlockUids = "";
        string TAG = "";

        void Main(string args)
        {
            loadData();
            if (!installer(args))
            {
                switchAddress(args);
            }
            saveData();
        }

        private void switchAddress(string address)
        {
            string dhdUids = Storage;
            if(dhdUids.Length > 0)
            {
                Echo("Change adress for " + TAG);
                List<IMyTerminalBlock> DHDs = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyDoor>(DHDs, x => dhdUids.Contains(x.ToString()));
                if(DHDs.Count > 0)
                {
                    for(int i = 0; i < DHDs.Count; i++)
                    {
                        DHDs[i].SetCustomName(address);
                        Echo("Update " + DHDs[i].ToString() + " to " + address);
                    }
                    List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LCDs, x => x.CustomName.Contains(TAG));
                    string text = format.Replace("{address}", address);
                    for(int i = 0; i < LCDs.Count; i++)
                    {
                        (LCDs[i] as IMyTextPanel).WritePublicText(text);
                    }
                }
            }
        }

        private bool installer(string args)
        {
            if (args.StartsWith(cmd_install))
            {
                string uids = "";
                string tag = args.Replace(cmd_install, "").Trim();
                List<IMyTerminalBlock> dhds = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyDoor>(dhds, x => x.CustomName.Contains(tag));
                Echo("Installing for " + tag);
                for(int i = 0; i < dhds.Count; i++)
                {
                    uids += dhds[i].ToString();
                    Echo("[" + i.ToString() + "/" + dhds.Count.ToString() + "]: " + dhds[i].ToString());
                }
                DHDBlockUids = uids;
                TAG = tag;
                Echo("Install complete.");
                return true;
            }

            return false;
        }

        private void saveData()
        {
            Storage = TAG + "|" + DHDBlockUids;
        }

        private void loadData()
        {
            string[] data = Storage.Split('|');
            if (data.Length > 1)
            {
                TAG = data[0];
                DHDBlockUids = data[1];
            }
        }
        
        #endregion
    }
}
