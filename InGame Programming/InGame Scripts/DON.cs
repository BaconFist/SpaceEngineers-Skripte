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
    class DON
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        const string PANEL_NAME = "LCD Mainframe";
        const string CONTAINER_NAME = "Station Container (Component:P2)";
        const int PANEL_LINES = 22;
        //Schwellenwerte
        const int min_maximum = 500000;
        const int min_high = 200000;
        const int min_medium = 50000;
        const int min_small = 15000;
        const int min_tiny = 1000;
        const int min_micro = 100;
        int lineOffset = 0;

        void Main()
        {
            try
            {
                List<IMyTerminalBlock> work = new List<IMyTerminalBlock>();
                Dictionary<String, float> consolidated = new Dictionary<String, float>();
                GridTerminalSystem.SearchBlocksOfName(PANEL_NAME, work);
                IMyTextPanel panel = null;
                for (int i = 0; i < work.Count; i++)
                {
                    if (work[i] is IMyTextPanel)
                    {
                        panel = (IMyTextPanel)work[i];
                        break;
                    }
                }
                List<IMyTerminalBlock> containerList = new List<IMyTerminalBlock>();
                GridTerminalSystem.SearchBlocksOfName(CONTAINER_NAME, containerList);

                for (int i = 0; i < containerList.Count; i++)
                {
                    CheckItem("SteelPlate", containerList[i]);
                    CheckItem("Construction", containerList[i]);
                    CheckItem("Computer", containerList[i]);
                    CheckItem("MetalGrid", containerList[i]);
                    CheckItem("Motor", containerList[i]);
                    CheckItem("Display", containerList[i]);
                    CheckItem("InteriorPlate", containerList[i]);
                    CheckItem("SmallTube", containerList[i]);
                    CheckItem("LargeTube", containerList[i]);
                    CheckItem("BulletproofGlass", containerList[i]);
                    CheckItem("Reactor", containerList[i]);
                    CheckItem("Thrust", containerList[i]);
                    CheckItem("GravityGenerator", containerList[i]);
                    CheckItem("Medical", containerList[i]);
                    CheckItem("RadioCommunication", containerList[i]);
                    CheckItem("Detector", containerList[i]);
                    CheckItem("SolarCell", containerList[i]);
                    CheckItem("PowerCell", containerList[i]);
                    CheckItem("AzimuthSupercharger", containerList[i]);
                    CheckItem("Magna", containerList[i]);
                    CheckItem("magno", containerList[i]);
                    CheckItem("PDAmmo", containerList[i]);
                    CheckItem("NATO_25x184mm", containerList[i]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
        }

        string CheckItem(String name, IMyTerminalBlock container)
        {
            String AssemblerName = "Assembler (" + name + ")";
            int Minimum = getMinimum(name);
            float maxVolume = 0.0f;
            float currentVolume = 0.0f;

            if (container is IMyCargoContainer)
            {
                List<IMyTerminalBlock> AssemblerList = new List<IMyTerminalBlock>();
                GridTerminalSystem.SearchBlocksOfName(AssemblerName, AssemblerList);

                var containerInventory = container.GetInventory(0);
                maxVolume += (float)containerInventory.MaxVolume;
                currentVolume += (float)containerInventory.CurrentVolume;
                var containerItems = containerInventory.GetItems();

                for (int j = containerItems.Count - 1; j >= 0; j--)
                {
                    float amount = (float)containerItems[j].Amount;
                    if (containerItems[j].Content.SubtypeName == name)
                    {
                        if (amount < getMinimum(name))
                        {
                            for (int k = AssemblerList.Count - 1; k >= 0; k--)
                            {
                                AssemblerList[k].ApplyAction("OnOff_On");
                            }
                        }
                        else
                        {
                            for (int k = AssemblerList.Count - 1; k >= 0; k--)
                            {
                                AssemblerList[k].ApplyAction("OnOff_Off");
                            }
                        }
                    }
                    else
                    {
                        for (int k = AssemblerList.Count - 1; k >= 0; k--)
                        {
                            AssemblerList[k].ApplyAction("OnOff_On");
                        }
                    }
                }
            }
            return name;
        }

        int getMinimum(String name)
        {
            if (name.Equals("Construction")) { return min_medium; }
            if (name.Equals("MetalGrid")) { return min_small; }
            if (name.Equals("InteriorPlate")) { return min_medium; }
            if (name.Equals("SteelPlate")) { return min_high; }
            if (name.Equals("SmallTube")) { return min_medium; }
            if (name.Equals("Computer")) { return min_medium; }
            if (name.Equals("LargeTube")) { return min_small; }
            if (name.Equals("BulletproofGlass")) { return min_small; }
            if (name.Equals("Reactor")) { return min_medium; }
            if (name.Equals("Thrust")) { return min_medium; }
            if (name.Equals("GravityGenerator")) { return min_tiny; }
            if (name.Equals("Medical")) { return min_micro; }
            if (name.Equals("RadioCommunication")) { return min_tiny; }
            if (name.Equals("Detector")) { return min_small; }
            if (name.Equals("SolarCell")) { return min_small; }
            if (name.Equals("Motor")) { return min_small; }
            if (name.Equals("Display")) { return min_small; }
            if (name.Equals("AutomaticRifleItem")) { return 0; }
            if (name.Equals("AutomaticRocketLauncher")) { return 0; }
            if (name.Equals("WelderItem")) { return 0; }
            if (name.Equals("AngleGrinderItem")) { return 0; }
            if (name.Equals("HandDrillItem")) { return 0; }
            if (name.Equals("AzimuthSupercharger")) { return min_small; }
            if (name.Equals("Magna")) { return min_tiny; }
            if (name.Equals("magno")) { return min_tiny; }
            if (name.Equals("PDAmmo")) { return min_medium; }
            if (name.Equals("NATO_25x184mm")) { return min_medium; }
            //if (name.Equals("")) { return ""; }
            return 0;
        }
        // End InGame-Script
    }
}
