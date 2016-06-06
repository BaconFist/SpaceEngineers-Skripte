using System;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using System.Text;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

public class Program : MyGridProgram
{
    #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
    /**
	   $safeitemname$
	   ==============================
	   Copyright (c) 2016 Thomas Klose <thomas@bratler.net>
	   Source:  
	   
	   Summary
	   ------------------------------
	   
	   Abstract
	   ------------------------------
				   
	   Example
	   ------------------------------          
	  
   */

    public Program()
    {

        // The constructor, called only once every session and
        // always before any other method is called. Use it to
        // initialize your script. 
        //     
        // The constructor is optional and can be removed if not
        // needed.

    }

    public void Save()
    {

        // Called when the program needs to save its state. Use
        // this method to save your state to the Storage field
        // or some other means. 
        // 
        // This method is optional and can be removed if not
        // needed.

    }

    public void Main(string argument)
    {
        // The main entry point of the script, invoked every time
        // one of the programmable block's Run actions are invoked.
        // 
        // The method itself is required, but the argument above
        // can be removed if not needed.
    }
    #endregion End of  Game Code - Copy/Paste Code to this region into Block Script Window in Game
}