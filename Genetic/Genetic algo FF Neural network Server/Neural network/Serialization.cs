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
        public static class Serialization
        { 
            public static string SerializeNet(Net net)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < net.weightMatrices.Count; i++)
                {
                    sb.Append(SerializeMatrix(net.GetWeightMatrix(i)) + "|");
                }

                return sb.ToString();
            }

            public static void DeSerializeNet(string serializedValue, ref Net net)
            {
                

                try
                {
                    List<LayerMatrix> weights = new List<LayerMatrix>();


                    List<string> splitMatrices = serializedValue.Split('|').ToList();
                    splitMatrices.RemoveAll(x => x == "");

                    if (splitMatrices.Count != net.weightMatrices.Count)
                        return;

                    foreach (string s in splitMatrices)
                    {
                        weights.Add(DeSerializeMatrix(s));
                    }

                    net.weightMatrices = weights;
                }
                catch
                {
                    P.Echo("Deserialization failed!");
                }

                
            }

            //LAYER SERIALIZATION
            public static LayerMatrix DeSerializeMatrix(string serializedValue)
            {
                int collumAmount = 0;

                List<List<double>> rows = new List<List<double>>();
                
                List<string> splitRows = new List<string>();
                

                splitRows = serializedValue.Split('*').ToList();
                splitRows.RemoveAll(x => x == "");


                for (int i = 0; i < splitRows.Count; i++)
                {
                    List<double> cols = new List<double>();
                    List<string> splitCols = new List<string>();

                    splitCols = splitRows[i].Split(';').ToList();
                    splitCols.RemoveAll(x => x == "");

                    for (int j = 0; j < splitCols.Count; j++)
                    {
                        cols.Add(double.Parse(splitCols[j]));
                    }

                    collumAmount = cols.Count;
                    rows.Add(cols);
                }

                LayerMatrix result = new LayerMatrix(splitRows.Count, collumAmount, false);
                result.values = rows;

                return result;
            }

            public static string SerializeMatrix(LayerMatrix matrix)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < matrix.GetNumRows(); i++)
                {
                    for (int j = 0; j < matrix.GetNumCols(); j++)
                    {
                        sb.Append(matrix.values[i][j] + ";");
                    }

                    sb.Append("*");
                }

                return sb.ToString();
            }
        }
    }
}
