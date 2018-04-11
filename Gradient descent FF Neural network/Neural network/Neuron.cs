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
            double derivedValue;
            double activatedValue;


            Func<double, double> activationFunction;
            Func<double, double> activationFunctionDerived;

            public Neuron(double val, Func<double, double> activationFunct = null, Func<double, double> activationFunctDeriv = null)
            {
                if (activationFunct != null && activationFunctDeriv != null)
                {

                    //Set custom activationfunction
                    activationFunction = activationFunct;
                    activationFunctionDerived = activationFunctDeriv;
                }
                else
                {

                    //fast Sigmoid function by default
                    activationFunction = delegate (double I) { return I / (1 + Math.Abs(I)); };
                    activationFunctionDerived = delegate (double I) { return I * (1 - I); };
                }

                neuronValue = val;
                Activate();
                Derive();
            }

            public void Activate()
            {
                activatedValue = activationFunction(neuronValue);
            }

            public void Derive()
            {
                derivedValue = activationFunctionDerived(activatedValue);
            }

            public void SetValue(double val)
            {
                neuronValue = val;
                Activate();
                Derive();
            }

            public void SetActivationFunction(Func<double, double> activationFunct = null, Func<double, double> activationFunctDeriv = null)
            {
                if (activationFunct != null && activationFunctDeriv != null)
                {

                    //Set custom activationfunction
                    activationFunction = activationFunct;
                    activationFunctionDerived = activationFunctDeriv;
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

            public double GetDerivedValue()
            {
                return derivedValue;
            }
        }
    }
}
