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
        int populationCount = 25;
        int seed = 666;

        Runningicon icon;
        public static Program P;
        ACPWrapper antenna;
        List<Species> dnaList;
        Random rng;
        IMyTimerBlock timer;
        static Action<string> println;


        public void Init()
        {
            //ETC
            icon = new Runningicon(3);
            timer = GridTerminalSystem.GetBlockWithName("timer") as IMyTimerBlock;
            println = Echo;

            //NN related
            dnaList = new List<Species>();

            rng = new Random(seed);
            
            int[] topology = {
                12,
                12,
                8,
                7,
            };

            for (int i = 0; i < populationCount; i++) { }
                dnaList.Add(new Species(topology, rng, rng.Next(0, 25) / 100));

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
                case "15SecTimer":
                    NextSpecies();
                    break;
            }

            //Comms
            long senderId;
            string[] msgs = antenna.Main(argument, out senderId);
            ParseMsg(msgs, senderId);

            /*=========== NN ===========*/

        }


        //UTILS

        public void NextSpecies()
        {
            //TODO:
            
        }

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
                //TODO:

                Echo("DeSerialized!");
            }
        }

        public void Serialize()
        {

            if (doSerialize)
            {
                //TODO:

                Echo("Serialized!");
            }
        }

        public void ParseMsg(string[] msg, long senderId)
        {
            if (msg == null)
                return;

            //TODO:
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