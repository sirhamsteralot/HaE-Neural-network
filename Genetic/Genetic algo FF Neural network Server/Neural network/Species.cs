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
        public class Species
        {
            public Net nn;
            public double mutationChance;
            public long Id;
            public double fitness;

            public Species(int[] topology, Random rng, double mutationChance)
            {
                nn = new Net(topology, rng);
            }

            public void Mutate()
            {
                nn.Mutate(mutationChance);
            }

            public void Mate(Species mommy)
            {
                nn.Mate(mommy.nn);
            }
        }
    }
}
