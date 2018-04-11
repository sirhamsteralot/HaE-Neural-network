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
        public class Net
        {

            int[] topology;
            List<double> input;
            List<Layer> layers;
            Random rng;

            public List<LayerMatrix> weightMatrices;


            public Net(int[] topology, Random rng = null)
            {
                this.topology = topology;
                layers = new List<Layer>();
                weightMatrices = new List<LayerMatrix>();
                input = new List<double>();
                if (rng != null)
                    this.rng = new Random();
                else
                    this.rng = rng;


                for (int i = 0; i < topology.Length; i++)
                {

                    Layer l = new Layer(topology[i]);
                    layers.Add(l);
                }

                for (int i = 0; i < topology.Length - 1; i++)
                {
                    LayerMatrix m = new LayerMatrix(topology[i], topology[i + 1], true);

                    weightMatrices.Add(m);
                }
            }
            
            public void FeedForward()
            {
                for (int i = 0; i < layers.Count - 1; i++)
                {
                    LayerMatrix a = GetNeuronMatrix(i);

                    if (i != 0)
                    {
                        a = GetActivatedNeuronMatrix(i);
                    }

                    LayerMatrix b = GetWeightMatrix(i);

                    LayerMatrix c = a.Multiply(b);

                    for (int c_index = 0; c_index < c.GetNumCols(); c_index++)
                    {
                        SetNeuronValue(i + 1, c_index, c.GetValue(0, c_index));
                    }
                }
            }

            public void Mate(Net mommy)
            {
                for (int i = 0; i < weightMatrices.Count; i++)
                {
                    weightMatrices[i].CrossOver(mommy.weightMatrices[i]);
                }
            }

            public void Mutate(double chance)
            {
                for (int i = 0; i < weightMatrices.Count; i++)
                {
                    weightMatrices[i].Mutate(chance);
                }
            }

            public void SetActivationFunction(int layerIndex, Func<double, double> activationFunct = null)
            {

                    layers[layerIndex].SetActivationFunction(activationFunct);
            }

            public void PrintToLcd(bool showLiteralInput = true)
            {
                StringBuilder sb = new StringBuilder();

                for (int j = 0; j < layers.Count; j++)
                {
                    for (int i = 0; i < layers[j].GetNeurons().Count; i++)
                    {

                        if (j != 0 || !showLiteralInput)
                            sb.Append(String.Format("{0,-10}", Math.Round(layers[j].GetNeurons()[i].GetActivatedValue(), 3).ToString()));
                        else
                            sb.Append(String.Format("{0,-10}", Math.Round(layers[j].GetNeurons()[i].GetValue(), 3).ToString()));
                    }

                    sb.Append($"\n{new String('=', 100)}\n");
                }

                P.PrintToLCD(sb.ToString(), false);
            }


            public double[] GetOutput()
            {
                List<double> outputdata = new List<double>();

                for(int i = 0; i < layers[layers.Count - 1].GetNeurons().Count; i++)
                {
                    outputdata.Add(layers[layers.Count - 1].GetNeurons()[i].GetActivatedValue());
                }

                return outputdata.ToArray();
            }


            public void SetCurrentInput(List<double> input)
            {
                this.input = input;

                for (int i = 0; i < input.Count; i++)
                {
                    layers[0].SetValue(i, input[i]);
                }
            }

            public void SetNeuronValue(int indexLayer, int indexNeuron, double val)
            {
                layers[indexLayer].SetValue(indexNeuron, val);
            }

            public LayerMatrix GetNeuronMatrix(int index)
            {
                return layers[index].MatrixifyVals();
            }

            public LayerMatrix GetActivatedNeuronMatrix(int index)
            {
                return layers[index].MatrixifyActivatedVals();
            }

            public LayerMatrix GetWeightMatrix(int index)
            {
                return weightMatrices[index];
            }
        }
    }

}
