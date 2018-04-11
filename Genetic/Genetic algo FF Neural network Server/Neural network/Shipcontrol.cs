using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Shipcontrol
        {
            float precision = 0.01f;
            List<IMyGyro> gyros;
            List<IMyThrust> forwardsThrust;

            IMyRemoteControl rc;

            public Shipcontrol(IMyRemoteControl rc)
            {
                gyros = new List<IMyGyro>();
                P.GridTerminalSystem.GetBlocksOfType(gyros);

                forwardsThrust = new List<IMyThrust>();
                P.GridTerminalSystem.GetBlocksOfType(forwardsThrust);

                this.rc = rc;
            }

            public void Gas(double amount)
            {
                foreach(IMyThrust t in forwardsThrust)
                {
                    t.SetValueFloat("Override", (float)amount);
                }
            }

            public void Handbrake(double val)
            {
                if (val > 0)
                    rc.HandBrake = true;
                else
                    rc.HandBrake = false;
            }

            public void Rotate(Vector3D rotate)
            {

                foreach (IMyGyro gyro in gyros)
                {
                    Matrix localMatrix;
                    gyro.Orientation.GetMatrix(out localMatrix);
                    localMatrix = Matrix.Transpose(localMatrix);
                    var localRotate = Vector3.TransformNormal(rotate, localMatrix);
                    localRotate.Y *= -1;

                    //CheckFor gyro - localRotate >= precision
                    if (Math.Abs(gyro.Yaw - localRotate.X) >= precision)
                    {
                        gyro.Yaw = localRotate.X;
                    }
                    if (Math.Abs(gyro.Pitch - localRotate.Y) >= precision)
                    {
                        gyro.Pitch = localRotate.Y;
                    }

                    if ((Math.Abs(gyro.Roll - localRotate.Z) >= precision) || (Math.Abs(gyro.Pitch - localRotate.Y) >= precision) || (Math.Abs(gyro.Yaw - localRotate.X) >= precision))
                    {
                        gyro.GyroOverride = true;
                    }
                    else
                    {
                        gyro.GyroOverride = false;
                    }
                }

            }
        }
    }
}
