using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
////using Sandbox.Common.ObjectBuilders;
using VRage;
using VRageMath;

namespace IBlockScripts
{
    public class AreaDefenceDroneAI : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
           AreaDefenceDroneAI
           ==============================
           Copyright (c) 2015 Thomas Klose <thomas@bratler.net>
           Source:  
           
           Summary
           ------------------------------
           

           Abstract
           ------------------------------
          
           
           Example
           ------------------------------
          
       */
        void Main(string args)
        {
            IMyRemoteControl RemoteControl = GridTerminalSystem.GetBlockWithName("RC") as IMyRemoteControl;
            if(RemoteControl != null)
            {
            //GPS: ORIGIN: -56148.91:23763.12:-2721.73:
                Vector3D origin = new Vector3D(-56148.91, 23763.12, -2721.73);

                
                RemoteControl.SetAutoPilotEnabled(false);
                RemoteControl.ClearWaypoints();
                RemoteControl.AddWaypoint(origin, "WP_Name");
                RemoteControl.ApplyAction("CollisionAvoidance_On");
                RemoteControl.ApplyAction("AutoPilot_On");
                RemoteControl.ApplyAction("DockingMode_Off");
                RemoteControl.SetAutoPilotEnabled(true);

                // ADDAI Brain = new ADDAI(RC, GridTerminalSystem, origin, 500.0, 50.0);
                // Brain.run();
            }
        }



        class ADDAI
        {
            private IMyGridTerminalSystem GridTerminalSystem;
            private Vector3D originPosition = new Vector3D(0,0,0);
            private Vector3D enemyPosition = new Vector3D(0, 0, 0);
            private Vector3D dronePosition = new Vector3D(0, 0, 0);
            private double operationRadius = 0;
            private double minTurretRange = 0;
            private double minDistanceFactor = 0.8;
            private double originParkingRange = 0;
            private IMyRemoteControl RemoteControl;
            List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();
            
            public ADDAI(IMyRemoteControl rc, IMyGridTerminalSystem gts, Vector3D origin, double opRadius, double origPark)
            {
                this.RemoteControl = rc;
                this.GridTerminalSystem = gts;
                this.originPosition = origin;
                this.operationRadius = opRadius;
                this.originParkingRange = origPark;
                bootTurrets();
            }

            public bool run()
            {
                return doAction_AttackEnemy() || doAction_ReturnToOrigin();
            }

            private void bootTurrets()
            {
                List<IMyTerminalBlock> Tmp = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(Tmp);
                Turrets = Tmp.ConvertAll<IMyLargeTurretBase>(x => x as IMyLargeTurretBase);
                minTurretRange = double.MaxValue;
                for (int i = 0; i < Turrets.Count; i++)
                {
                    double range = Turrets[i].Range;
                    minTurretRange = (range < minTurretRange) ? range: minTurretRange;
                }
            }



            private bool isDroneCombatReady()
            {
                return isManoeuvringFunctional() && isTurretsFunctionalAndLoaded();
            }

            private bool isTurretsFunctionalAndLoaded(double minLoadFactor = 0.2)
            {
                for(int i_Turrets = 0; i_Turrets < Turrets.Count; i_Turrets++)
                {
                    IMyLargeTurretBase Turret = Turrets[i_Turrets];
                    if (!Turret.IsWorking ||!Turret.IsFunctional)
                    {
                        return false;
                    }
                    double maximumAmmoLoad = (double)Turret.GetInventory(0).MaxVolume;
                    double currentAmmoLoad = (double)Turret.GetInventory(0).CurrentVolume;
                    double currentLoadFactor = (maximumAmmoLoad<= 0.0)?(0.0):(currentAmmoLoad / maximumAmmoLoad);
                    if(currentLoadFactor < minLoadFactor)
                    {
                        return false;
                    }
                }
                return (Turrets.Count > 0)? true :false;
            }

            private bool isManoeuvringFunctional()
            {
                bool hasManoeuveringBlocks = false;

                List<IMyTerminalBlock> UnusableThruster = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(UnusableThruster, (x=> !x.IsFunctional || !x.IsWorking));

                List<IMyTerminalBlock> Gyros = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyros, (x => x.IsFunctional && x.IsWorking && !(x as IMyGyro).GyroOverride));
                hasManoeuveringBlocks = hasManoeuveringBlocks || ((UnusableThruster.Count <= 0) && (Gyros.Count > 0));
                if (!hasManoeuveringBlocks)
                {
                    List<IMyTerminalBlock> UnusableSuspensions = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyMotorSuspension>(UnusableSuspensions, x => !x.IsWorking || !x.IsFunctional);
                    hasManoeuveringBlocks = hasManoeuveringBlocks || (UnusableSuspensions.Count <= 0);
                }

                return hasManoeuveringBlocks;
            }

            private bool doAction_ReturnToOrigin()
            {
                if (!isManoeuvringFunctional())
                {
                    return false;
                }
                double dist = Vector3D.Distance(dronePosition, originPosition);                
                if(dist > originParkingRange)
                {
                    setupAndStartDrone(originPosition, "Origin", true);
                }
                return true;
            }

            private bool doAction_AttackEnemy()
            {
                if (!isDroneCombatReady())
                {
                    return false;
                }
                Vector3D EnemyPos = new Vector3D(0, 0, 0);
                RemoteControl.GetNearestPlayer(out EnemyPos);
                if (EnemyPos.Equals(new Vector3D(0, 0, 0)))
                {
                    return false;
                }
                if (!isTargetInOperationArea(EnemyPos))
                {
                    return false;
                }
                setupAndStartDrone(EnemyPos, "Enemy", false);

                return true;
            }

            private bool isTargetInOperationArea(Vector3D Target)
            {
                if(operationRadius > 0)
                {
                    double targetDistance = Vector3D.Distance(originPosition, Target);
                    if(targetDistance < operationRadius)
                    {
                        return true;
                    }
                }

                return false;
            }
            
            private void setupAndStartDrone(Vector3D Target, string WP_Name, bool dockingMode)
            {
                RemoteControl.SetAutoPilotEnabled(false);
                RemoteControl.ClearWaypoints();
                RemoteControl.AddWaypoint(Target, WP_Name);
                RemoteControl.ApplyAction("CollisionAvoidance_On");
                RemoteControl.ApplyAction("AutoPilot_On");
                RemoteControl.ApplyAction((dockingMode)? "DockingMode_On" : "DockingMode_Off");                
                RemoteControl.SetAutoPilotEnabled(true);

            }

            /*
                RemoteControl Actions:
                ControlThrusters
                ControlWheels
                HandBrake
                DampenersOverride
                MainCockpit
                HorizonIndicator
                AutoPilot
                AutoPilot_On
                AutoPilot_Off
                CollisionAvoidance
                CollisionAvoidance_On
                CollisionAvoidance_Off
                DockingMode
                DockingMode_On
                DockingMode_Off
                Forward
                Backward
                Left
                Right
                Up
                Down
             */

        }
        #endregion
    }
}
