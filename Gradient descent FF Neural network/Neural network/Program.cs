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
        int maxTrainingPass = 10000;

        Net nn;
        Runningicon icon;
        public static Program P;
        double currentError;
        double treshold = 0.001;
        long trainingPass;

        IEnumerator<bool> trainingSession;


        public void Init()
        {
            //ETC
            icon = new Runningicon(3);

            //NN related
            List<double> input = new List<double>();
            input.Add(1);
            input.Add(0);
            input.Add(1);

            int[] topology =
            {
                3,
                2,
                3,
            };
            nn = new Net(topology);
            nn.SetCurrentInput(input);
            nn.SetCurrentTarget(input);

            DeSerialize();

            trainingSession = TrainNet();
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
                case "exportexel":
                    Me.CustomData = nn.GetHistoricalErrors();
                    break;
            }

            /*====== NN training ======*/
            if (trainingSession != null && !trainingSession.MoveNext())
            {
                trainingSession.Dispose();
                trainingSession = null;
            }
            else
            {
                Echo("Current Error:" + currentError);
                Echo($"Pass: {trainingPass}");
                icon.DoTick();
            }


            return;
        }

        public IEnumerator<bool> TrainNet()
        {

            while ((currentError > treshold || currentError == 0) && trainingPass < maxTrainingPass)
            {
                nn.FeedForward();
                yield return true;

                nn.SetErrors();
                nn.BackPropagation();
                yield return true;

                nn.PrintToLcd();

                currentError = Math.Abs(nn.GetTotalError());

                trainingPass++;
            }

            if (Double.IsNaN(currentError))
            {
                currentError = 0;
                Init();
            }
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

        public void ClearSerializedData()
        {
            Storage = "";
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