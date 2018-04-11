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
        public class Neuron
        {
            double neuronValue;
            double activatedValue;


            Func<double, double> activationFunction;

            public Neuron(double val, Func<double, double> activationFunct = null)
            {
                if (activationFunct != null)
                {

                    //Set custom activationfunction
                    activationFunction = activationFunct;
                }
                else
                {

                    //fast Sigmoid function by default
                    activationFunction = delegate (double I) { return I / (1 + Math.Abs(I)); };
                }

                neuronValue = val;
                Activate();
            }

            public void Activate()
            {
                activatedValue = activationFunction(neuronValue);
            }


            public void SetValue(double val)
            {
                neuronValue = val;
                Activate();
            }

            public void SetActivationFunction(Func<double, double> activationFunct = null)
            {
                if (activationFunct != null)
                {

                    //Set custom activationfunction
                    activationFunction = activationFunct;
                }
            }

            public double GetValue()
            {
                return neuronValue;
            }

            public double GetActivatedValue()
            {
                return activatedValue;
            }
        }
    }
}
