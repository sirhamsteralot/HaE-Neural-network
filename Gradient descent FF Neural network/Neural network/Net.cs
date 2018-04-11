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
            double error;
            double[] errors;
            List<double> historicalerrors;
            List<double> target;

            int[] topology;
            List<double> input;
            List<Layer> layers;
            List<LayerMatrix> gradientMatrices;

            public List<LayerMatrix> weightMatrices;


            public Net(int[] topology)
            {
                this.topology = topology;
                layers = new List<Layer>();
                weightMatrices = new List<LayerMatrix>();
                input = new List<double>();

                errors = new double[topology[topology.Length - 1]];
                historicalerrors = new List<double>();



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

            public void BackPropagation()
            {
                List<LayerMatrix> newWeights = new List<LayerMatrix>();

                // output to hidden
                int outputLayerIndex = layers.Count - 1;
                LayerMatrix derivedValuesYToZ = layers[outputLayerIndex].MatrixifyDerivedVals();
                LayerMatrix gradientsYToZ = new LayerMatrix(1, layers[outputLayerIndex].GetNeurons().Count, false);

                for (int i = 0; i < errors.Length; i++)
                {
                    double d = derivedValuesYToZ.GetValue(0, i);
                    double e = errors[i];
                    double g = d * e;
                    gradientsYToZ.SetValue(0, i, g);
                }

                int lastHiddenLayerIndex = outputLayerIndex - 1;
                Layer lastHiddenLayer = layers[lastHiddenLayerIndex];
                LayerMatrix weightsOutputToHidden = weightMatrices[lastHiddenLayerIndex];
                LayerMatrix deltaOutputToHidden = gradientsYToZ.Transpose().Multiply(lastHiddenLayer.MatrixifyActivatedVals()).Transpose();

                LayerMatrix newWeightsOutputToHidden = new LayerMatrix(deltaOutputToHidden.GetNumRows(), deltaOutputToHidden.GetNumCols(), false);

                for (int r = 0; r < deltaOutputToHidden.GetNumRows(); r++)
                {
                    for (int c = 0; c < deltaOutputToHidden.GetNumCols(); c++)
                    {
                        double originalWeight = weightsOutputToHidden.GetValue(r, c);
                        double deltaWeight = deltaOutputToHidden.GetValue(r, c);
                        newWeightsOutputToHidden.SetValue(r, c, originalWeight - deltaWeight);
                    }
                }
                newWeights.Add(newWeightsOutputToHidden);

                //Moving from last hidden layer to input layer
                for (int i = lastHiddenLayerIndex; i > 0; i--)
                {

                    Layer l = layers[i];
                    LayerMatrix derivedHidden = l.MatrixifyDerivedVals();
                    LayerMatrix derivedGradients = new LayerMatrix(1, l.GetNeurons().Count, false);
                    LayerMatrix activatedHidden = l.MatrixifyActivatedVals();

                    LayerMatrix weightMatrix = weightMatrices[i];
                    LayerMatrix originalWeight = weightMatrices[i - 1];


                    for(int r = 0; r < weightMatrix.GetNumRows(); r++)
                    {
                        double sum = 0.0;

                        for(int c = 0; c < weightMatrix.GetNumCols(); c++)
                        {
                            double p = gradientsYToZ.GetValue(0, c) * weightMatrix.GetValue(r, c);
                            sum += p;
                        }

                        double g = sum * activatedHidden.GetValue(0, r);
                        derivedGradients.SetValue(0, r, g);
                    }


                    LayerMatrix leftNeurons;
                    if (i - 1 == 0)
                    {
                        leftNeurons = layers[0].MatrixifyVals();
                    }
                    else
                    {
                        leftNeurons = layers[i - 1].MatrixifyActivatedVals();
                    }

                    LayerMatrix deltaWeights = derivedGradients.Transpose().Multiply(leftNeurons).Transpose();

                    LayerMatrix newWeightsHiddenToInput = new LayerMatrix(deltaWeights.GetNumRows(), deltaWeights.GetNumCols(), false);

                    for (int r = 0; r < newWeightsHiddenToInput.GetNumRows(); r++)
                    {
                        for (int c = 0; c < newWeightsHiddenToInput.GetNumCols(); c++)
                        {
                            double w = originalWeight.GetValue(r, c);
                            double d = deltaWeights.GetValue(r, c);
                            double n = w - d;
                            newWeightsHiddenToInput.SetValue(r, c, n);
                        }
                    }

                    newWeights.Add(newWeightsHiddenToInput);
                }
                newWeights.Reverse();
                weightMatrices = newWeights;
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

            public void SetActivationFunction(Func<double, double> activationFunct = null, Func<double, double> activationFunctDeriv = null)
            {
                foreach(Layer i in layers)
                {
                    i.SetActivationFunction(activationFunct, activationFunctDeriv);
                }
            }

            public void SetErrors()
            {
                if (target.Count == 0)
                    throw new Exception("No target for neural network!");

                if (target.Count != layers[layers.Count - 1].GetNeurons().Count)
                    throw new Exception("Wrong target for neural network!");


                error = 0.0;
                int outputLayerIndex = layers.Count - 1;

                List<Neuron> outputNeurons = layers[outputLayerIndex].GetNeurons();

                for (int i = 0; i < target.Count; i++)
                {
                    double tempErr = outputNeurons[i].GetActivatedValue() - target[i];
                    //double tempErr = outputNeurons[i].GetActivatedValue() - target[i]; // initial function
                    errors[i] = tempErr;
                    error += Math.Pow(tempErr, 2);
                }

                error /= 2;
                historicalerrors.Add(Math.Round(error, 4));
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

            public string GetHistoricalErrors()
            {
                StringBuilder sb = new StringBuilder();

                foreach (double error in historicalerrors)
                {
                    sb.Append(error + ";");
                }

                return sb.ToString();
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

            public double GetTotalError()
            {
                return error;
            }

            public double[] GetErrors()
            {
                return errors;
            }

            public void SetCurrentTarget(List<double> target)
            {
                this.target = target;
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

            public LayerMatrix GetDerivedNeuronMatrix(int index)
            {
                return layers[index].MatrixifyDerivedVals();
            }

            public LayerMatrix GetWeightMatrix(int index)
            {
                return weightMatrices[index];
            }
        }
    }

}
