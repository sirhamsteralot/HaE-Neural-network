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
        public class LayerMatrix
        {
            public List<List<double>> values;

            int numRows;
            int numCols;
            Random randGen;

            public LayerMatrix(int numRows, int numCols, bool isRandom, Random rng = null)
            {
                this.numRows = numRows;
                this.numCols = numCols;
                if (rng != null)
                {
                    randGen = rng;
                }
                else
                {
                    randGen = new Random();
                }

                values = new List<List<double>>();

                for (int i = 0; i < numRows; i++)
                {
                    List<double> tempValues = new List<double>();

                    for (int t = 0; t < numCols; t++)
                    {
                        double tempVal = 0.00;

                        if (isRandom)
                        {

                            tempVal = randGen.NextDouble();
                        }

                        tempValues.Add(tempVal);
                    }

                    values.Add(tempValues);
                }
            }

            public LayerMatrix Transpose()
            {
                LayerMatrix temp = new LayerMatrix(numCols, numRows, false);

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        temp.SetValue(j, i, GetValue(i, j));
                    }
                }

                return temp;
            }

            public LayerMatrix Multiply(LayerMatrix b)
            {
                LayerMatrix a = this;
                if (a.GetNumCols() != b.GetNumRows())
                    throw new Exception("A_cols: " + a.GetNumCols() + " != B_rows: " + b.GetNumRows());

                LayerMatrix result = new LayerMatrix(a.GetNumRows(), b.GetNumCols(), false);

                for (int i = 0; i < a.GetNumRows(); i++)
                {
                    for (int j = 0; j < b.GetNumCols(); j++)
                    {
                        for (int k = 0; k < b.GetNumRows(); k++)
                        {
                            double p = a.GetValue(i, k) * b.GetValue(k, j);
                            double newVal = result.GetValue(i, j) + p;
                            result.SetValue(i, j, newVal);
                        }
                    }
                }

                return result;
            }

            public void PrintToConsole(bool lcd = false)
            {

                if (!lcd)
                {
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            P.Echo(values[i][j] + "              ");
                        }

                        P.Echo("\n");
                    }
                } else
                {
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            P.PrintToLCD(values[i][j] + "              ");
                        }

                        P.PrintToLCD("\n");
                    }
                }

            }

            public List<double> ToList()
            {
                List<double> result = new List<double>();

                for (int i = 0; i < GetNumRows(); i++)
                {
                    for (int t = 0; t < GetNumRows(); t++)
                    {
                        result.Add(GetValue(i, t));
                    }
                }

                return result;
            }

            public void SetValue(int row, int col, double val)
            {
                values[row][col] = val;
            }

            public double GetValue(int row, int col)
            {
                return values[row][col];
            }

            public int GetNumRows()
            {
                return numRows;
            }

            public int GetNumCols()
            {
                return numCols;
            }
        }
    }

}
