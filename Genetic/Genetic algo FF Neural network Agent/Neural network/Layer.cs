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
        public class Layer
        {
            int size;
            List<Neuron> neurons;

            public Layer(int size)
            {
                this.size = size;
                neurons = new List<Neuron>();

                for (int i = 0; i < size; i++)
                {
                    neurons.Add(new Neuron(0.00));
                }
            }

            public void SetValue(int i, double val)
            {
                neurons[i].SetValue(val);
            }

            public void SetActivationFunction(Func<double, double> activationFunct = null)
            {
                foreach(Neuron i in neurons)
                {
                    i.SetActivationFunction(activationFunct);
                }
            }

            public LayerMatrix MatrixifyVals()
            {
                LayerMatrix m = new LayerMatrix(1, neurons.Count, false);

                for (int i = 0; i < neurons.Count; i++)
                {
                    m.SetValue(0, i, neurons[i].GetValue());
                }

                return m;
            }

            public LayerMatrix MatrixifyActivatedVals()
            {
                LayerMatrix m = new LayerMatrix(1, neurons.Count, false);

                for (int i = 0; i < neurons.Count; i++)
                {
                    m.SetValue(0, i, neurons[i].GetActivatedValue());
                }

                return m;
            }

            public List<Neuron> GetNeurons()
            {
                return neurons;
            }
        }
    } 
}
