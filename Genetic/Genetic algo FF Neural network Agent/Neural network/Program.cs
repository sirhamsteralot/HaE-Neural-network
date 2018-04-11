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
    partial class Program : MyGridProgram
    {
        bool doSerialize = false;
        int seed = 0;

        Net nn;
        Runningicon icon;
        public static Program P;
        ACPWrapper antenna;
        IMyRemoteControl rc;
        List<IMyCameraBlock> cameras;
        Shipcontrol control;

        Vector3D startPosition;


        public void Init()
        {
            //driving neccesities
            cameras = new List<IMyCameraBlock>();
            GridTerminalSystem.GetBlocksOfType(cameras);

            foreach (IMyCameraBlock cam in cameras)
            {
                cam.EnableRaycast = true;
            }

            rc = GridTerminalSystem.GetBlockWithName("Remote control") as IMyRemoteControl;
            control = new Shipcontrol(rc);

            //ETC
            icon = new Runningicon(3);

            //NN related
            List<double> input = new List<double>();
            input.Add(1);
            input.Add(0);
            input.Add(1);

            int[] topology =
            {
                12,
                12,
                8,
                7,
            };

            nn = new Net(topology, seed);
            nn.SetCurrentInput(input);

            nn.SetActivationFunction(topology.Length - 1, x => x);              //Activation function of the last layer to 0

            DeSerialize();

            startPosition = rc.GetPosition();

            //ACP
            antenna = new ACPWrapper(this);
        }

        public void Save()
        {
            Serialize();
        }

        public void SubMain(string argument)
        {
            /*========== ETC ==========*/
            switch (argument.ToLower())
            {
                case "clearserialization":
                    ClearSerializedData();
                    break;
            }

            //Comms
            long senderId;
            string[] msgs = antenna.Main(argument, out senderId);
            ParseMsg(msgs, senderId);

            /*=========== NN ===========*/

            double[] inputs = new double[cameras.Count];
            for (int i = 0; i < cameras.Count; i++)
            {
                MyDetectedEntityInfo info = cameras[i].Raycast(500);

                if (info.HitPosition.HasValue)
                    inputs[i] = Vector3D.Distance(rc.GetPosition(), info.HitPosition.Value);
                else
                    inputs[i] = 1000;
            }
            nn.SetCurrentInput(inputs.ToList());
            nn.FeedForward();

            double[] outputs = nn.GetOutput();

            Vector3D gyro = new Vector3D(outputs[0], outputs[1], 0);

            control.Rotate(gyro);

            control.Gas(outputs[2]);

            control.Handbrake(outputs[3]);

            nn.PrintToLcd();

            return;
        }


        //UTILS

        public void PrintToLCD(string val, bool append = true)
        {
            string lcdName = "panel";

            var block = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;

            block.WritePublicText(val, append);
        }

        public void DeSerialize()
        {
            if (doSerialize)
            {
                Serialization.DeSerializeNet(Storage, ref nn);
                Echo("DeSerialized!");
            }
        }

        public void Serialize()
        {

            if (doSerialize)
            {
                Storage = Serialization.SerializeNet(nn);

                Echo("Serialized!");
            }
        }

        public void ParseMsg(string[] msg, long senderId)
        {
            if (msg == null)
                return;

            if (msg[0] == "SendBreedingData")
            {

                string[] returnMsg = { "IncomingBreedingData", Serialization.SerializeNet(nn), GetFitness().ToString() };
                antenna.PrepareMSG(returnMsg, senderId);

            }
            else if (msg[0] == "SetTheseWeights")
            {

                Serialization.DeSerializeNet(msg[1], ref nn);

            }
        }

        public void ClearSerializedData()
        {
            Storage = "";
        }

        public double GetFitness()
        {
            return Vector3D.Distance(startPosition, rc.GetPosition());
        }

        //DEBUG

        public Program()
        {
            try
            {
                P = this;
                Init();
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();

                sb.AppendLine("Exception Message:");
                sb.AppendLine($"   {e.Message}");
                sb.AppendLine();

                sb.AppendLine("Stack trace:");
                sb.AppendLine(e.StackTrace);
                sb.AppendLine();

                var exceptionDump = sb.ToString();
                var lcd = this.GridTerminalSystem.GetBlockWithName("EXCEPTION DUMP") as IMyTextPanel;

                Echo(exceptionDump);
                lcd?.WritePublicText(exceptionDump, append: false);

                //Optionally rethrow
                throw;
            }
        }

        void Main(string argument)
        {
            try
            {
                SubMain(argument);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();

                sb.AppendLine("Exception Message:");
                sb.AppendLine($"   {e.Message}");
                sb.AppendLine();

                sb.AppendLine("Stack trace:");
                sb.AppendLine(e.StackTrace);
                sb.AppendLine();

                var exceptionDump = sb.ToString();
                var lcd = this.GridTerminalSystem.GetBlockWithName("EXCEPTION DUMP") as IMyTextPanel;

                Echo(exceptionDump);
                lcd?.WritePublicText(exceptionDump, append: false);

                //Optionally rethrow
                throw;
            }
        }
    }
}