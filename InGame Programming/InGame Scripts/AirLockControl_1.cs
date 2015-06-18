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
    class AirLockControl_1
    {
        IMyGridTerminalSystem GridTerminalSystem;
        String Storage;
        // Begin InGame-Script
        void Main()
        {

        }
        // End InGame-Script
    }

    class Airlock
    {
        public IMyDoor innerDoor { get; set; }
        public IMyDoor outerDoor { get; set; }
        public IMyLightingBlock alertLight { get; set; }
        public IMyAirVent airVent { get; set; }

        private string id;

        public Airlock()
        {
            this.innerDoor = innerDoor;
            this.outerDoor = outerDoor;
            this.alertLight = alertLight;
            this.airVent = airVent;
        }


        
    }
}
