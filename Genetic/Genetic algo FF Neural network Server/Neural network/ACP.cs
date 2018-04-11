﻿using Sandbox.Game.EntityComponents;
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
        public class ACPWrapper
        {
            /*========| Customize |========*/
            public readonly string CUSTOMNAME = "HaE Server"; //Whatever you want it to be.
            public readonly char[] HEADDIVIDER = new char[] { (char)0x1c };    //(char)0x1c == HaE standard
            public readonly char DIVIDER = (char)0x1f;        //(char)0x1f == HaE standard
            public readonly string MATING = "PING";           //PING == HaE standard
            public readonly string MATINGRESPONSE = "PONG";   //PONG == HaE standard
            public readonly int STRINGMAXLENGTH = 100000;     //Maximum msg length, must be 100k or lower!

            /*========| Main vars |========*/
            private Program P;

            public List<IMyRadioAntenna> antennaList;
            private bool isList = false;

            public IMyRadioAntenna antenna;
            public IMyLaserAntenna lAntenna;
            private bool isLaser = false;

            /*=====| Messaging vars |=====*/
            private List<string> msgQueue;
            public MyTransmitTarget TRANSMITTARGET;

            public HashSet<Adress> adressBook;


            /*======| Constructors |======*/
            public ACPWrapper(Program p, Func<IMyTerminalBlock, bool> collect = null)
            {
                CommonInit(p);

                antennaList = new List<IMyRadioAntenna>();

                P.GridTerminalSystem.GetBlocksOfType(antennaList, collect);

                isList = true;

                Ping();
            }

            public ACPWrapper(Program p, bool HasList, List<IMyRadioAntenna> list = null)
            {
                CommonInit(p);

                if (list != null)
                    antennaList = list;

                Ping();
            }

            public ACPWrapper(Program p, IMyLaserAntenna lAntenna)
            {
                CommonInit(p);
                this.lAntenna = lAntenna;
                isLaser = true;

                Ping();
            }

            public ACPWrapper(Program p, IMyRadioAntenna antenna)
            {
                CommonInit(p);
                this.antenna = antenna;

                Ping();
            }

            public ACPWrapper(Program p, string antennaName)
            {
                CommonInit(p);

                IMyTerminalBlock temp = P.GridTerminalSystem.GetBlockWithName(antennaName);

                if (temp.GetType() == typeof(IMyRadioAntenna))
                {
                    this.antenna = (IMyRadioAntenna)temp;

                }
                else if (temp.GetType() == typeof(IMyLaserAntenna))
                {
                    this.lAntenna = (IMyLaserAntenna)temp;
                    isLaser = true;
                }

                Ping();
            }

            private void CommonInit(Program p)
            {
                this.P = p;

                msgQueue = new List<string>();
                adressBook = new HashSet<Adress>();
                TRANSMITTARGET = MyTransmitTarget.Default;
            }

            /*========| Public Methods |========*/
            public string[] Main(string Argument, out long senderId)
            {                  //Main method, call each tick/when you received a msg.
                               //Each tick

                if (Argument == "")
                    SendMSG();

                //Process MSG
                senderId = 0;
                string[] msgparts = Argument.Split(HEADDIVIDER, 2);

                if (msgparts.Length > 0)
                {
                    if (IsDirectedAtMe(msgparts[0], out senderId))
                    {
                        List<string> parameters = msgparts[1].Split(DIVIDER).ToList();

                        if (parameters.Count > 0)
                        {                                            //check if all parameters have a value
                            for (int i = parameters.Count - 1; i >= 0; i--)
                            {
                                if (parameters[i] == "")
                                {
                                    parameters.RemoveAt(i);
                                }
                            }

                            if (parameters[0] == MATING && parameters.Count == 2)
                            {               //Add to aderssbook
                                Pong(senderId);

                                AddAdress(senderId, parameters[1]);

                                return null;
                            }
                            else if (parameters[0] == MATINGRESPONSE && parameters.Count == 2)
                            {
                                AddAdress(senderId, parameters[1]);

                                return null;
                            }
                        }

                        if (parameters.Count > 0)
                            return parameters.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }

            public bool PrepareMSG(string parameter, long To, bool anon = false)
            {
                return PrepareMSG(new string[] { parameter }, To, anon);
            }

            public void AddAdress(long Id, string Name)
            {
                adressBook.Add(new Adress(Id, Name));
            }

            public bool PrepareMSG(string[] parameters, long To, bool anon = false)
            {
                StringBuilder constructedMSG = new StringBuilder();

                //Head
                if (!anon)
                {
                    constructedMSG.Append(P.Me.EntityId.ToString());
                }
                else
                {
                    constructedMSG.Append('0');
                }

                constructedMSG.Append(DIVIDER);
                constructedMSG.Append(To.ToString());
                constructedMSG.Append(HEADDIVIDER);

                //Body
                foreach (string parameter in parameters)
                {
                    constructedMSG.Append(parameter);
                    constructedMSG.Append(DIVIDER);
                }

                if (constructedMSG.Length > STRINGMAXLENGTH)
                    return false;

                msgQueue.Add(constructedMSG.ToString());
                return true;
            }

            public bool PrepareMSG(string[] parameters, string To, bool anon = false)
            { //Prepares a message, returns false if it fails.
                StringBuilder constructedMSG = new StringBuilder();

                //Head
                if (!anon)
                {
                    constructedMSG.Append(P.Me.EntityId.ToString());
                }
                else
                {
                    constructedMSG.Append('0');
                }

                constructedMSG.Append(DIVIDER);

                long? ToId = GetIdWithName(To);
                if (!ToId.HasValue)
                    return false;
                constructedMSG.Append(ToId.Value);

                constructedMSG.Append(HEADDIVIDER);

                //Body
                foreach (string parameter in parameters)
                {
                    constructedMSG.Append(parameter);
                    constructedMSG.Append(DIVIDER);
                }

                if (constructedMSG.Length > STRINGMAXLENGTH)
                    return false;

                msgQueue.Add(constructedMSG.ToString());
                return true;
            }

            public void Ping()
            {            //Initial ping out, prompts receivers to send a message back.
                PrepareMSG(new string[] { MATING, CUSTOMNAME }, 0);
            }

            public void Pong(long Id)
            {     //Ping response.
                PrepareMSG(new string[] { MATINGRESPONSE, CUSTOMNAME }, Id);
            }

            public void SetLaserTarget(Vector3D location)
            {
                lAntenna.SetTargetCoords($"GPS:T:{location.X}:{location.Y}:{location.Z}:");
            }

            public void SetLaserTarget(string GPS)
            { //Expects gps in format GPS:Name:X:Y:Z:
                lAntenna.SetTargetCoords(GPS);
            }

            public long? GetIdWithName(string Name)
            { //Returns first ID with name in the adressBook.
                foreach (Adress I in adressBook)
                {
                    if (I.Name == Name)
                        return I.Id;
                }
                return null;
            }

            /*========| Private Methods |========*/
            private void SendMSG()
            {
                if (msgQueue.Count > 0)
                {
                    if (!isList)
                    {
                        IndividualSend();
                    }
                    else
                    {
                        ListSend();
                    }
                }
            }

            private bool IsDirectedAtMe(string Head, out long SenderID)
            {
                string[] headparts = Head.Split(DIVIDER);
                SenderID = 0;

                if (headparts.Length == 2)
                {
                    long MyID;

                    Int64.TryParse(headparts[0], out SenderID);    //Parses the sender ID

                    if (Int64.TryParse(headparts[1], out MyID))
                    {  //Parses My ID
                        if (MyID == P.Me.EntityId)
                            return true;

                        return false;
                    }
                }
                return false;
            }

            private void IndividualSend()
            {
                if (isLaser)
                {
                    if (lAntenna.TransmitMessage(msgQueue[0]))
                        msgQueue.RemoveAt(0);
                }
                else
                {
                    if (antenna.TransmitMessage(msgQueue[0], TRANSMITTARGET))
                        msgQueue.RemoveAt(0);
                }

            }

            private void ListSend()
            {                               //Sends messages from the queue using lists.
                for (int i = 0; i < antennaList.Count; i++)
                {
                    if (msgQueue.Count > 0)
                    {
                        if (antennaList[i].TransmitMessage(msgQueue[0]))
                            msgQueue.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            public class Adress
            {
                public long Id;
                public string Name;

                public Adress(long Id, string Name)
                {
                    this.Id = Id;
                    this.Name = Name;
                }
            }
        }
    }
}
