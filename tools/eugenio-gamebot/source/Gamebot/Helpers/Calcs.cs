using Gamebot.Models;
using System;
using System.Collections.Generic;

namespace Gamebot.Helpers
{

    public static class Calcs
    {
        public static List<int> GetBestFichesAvailable(int initialSum)
        {
            List<int> fichesVanilla = Calcs.GetFichesVanilla(initialSum);
            List<int> fichesDoubles = Calcs.GetFichesRaddoppia(initialSum);
            List<int> fichesQuadrup = Calcs.GetFichesQuadruplica(initialSum);
            if (fichesVanilla.Count <= fichesDoubles.Count)
            {
                if (fichesVanilla.Count <= fichesQuadrup.Count)
                {
                    return fichesVanilla;
                }
                return fichesQuadrup;
            }
            else
            {
                if (fichesDoubles.Count <= fichesQuadrup.Count)
                {
                    return fichesDoubles;
                }
                return fichesQuadrup;
            }
        }

        private static List<int> GetFichesVanilla(int initialSum)
        {
            List<int> fiches = new List<int>();
            int valueCount = initialSum;
            foreach (int fish in Constants.availableFiches)
            {
                while (valueCount >= fish)
                {
                    fiches.Add(fish);
                    valueCount -= fish;
                }
            }
            return fiches;
        }

        private static List<int> GetFichesRaddoppia(int initialSum)
        {
            List<int> fiches = new List<int>();
            fiches = Calcs.GetFichesVanilla(initialSum / 2);
            fiches.Add(222);
            int valueCount = initialSum - Calcs.CountValues(fiches);
            if (valueCount > 0)
            {
                foreach (int fish in Constants.availableFiches)
                {
                    while (valueCount >= fish)
                    {
                        fiches.Add(fish);
                        valueCount -= fish;
                    }
                }
            }
            return fiches;
        }

        private static List<int> GetFichesQuadruplica(int initialSum)
        {
            List<int> fiches = new List<int>();
            fiches = Calcs.GetFichesVanilla(initialSum / 4);
            fiches.Add(222);
            fiches.Add(222);
            int valueCount = initialSum - Calcs.CountValues(fiches);
            if (valueCount > 0)
            {
                foreach (int fish in Constants.availableFiches)
                {
                    while (valueCount >= fish)
                    {
                        fiches.Add(fish);
                        valueCount -= fish;
                    }
                }
            }
            return fiches;
        }

        public static List<double> GetBestCustomFichesAvailable(double initialSum)
        {
            List<double> fichesVanilla = Calcs.GetCustomFichesVanilla(initialSum);
            List<double> fichesDoubles = Calcs.GetCustomFichesRaddoppia(initialSum);
            List<double> fichesQuadrup = Calcs.GetCustomFichesQuadruplica(initialSum);
            if (fichesVanilla.Count <= fichesDoubles.Count)
            {
                if (fichesVanilla.Count <= fichesQuadrup.Count)
                {
                    return fichesVanilla;
                }
                return fichesQuadrup;
            }
            else
            {
                if (fichesDoubles.Count <= fichesQuadrup.Count)
                {
                    return fichesDoubles;
                }
                return fichesQuadrup;
            }
        }

        private static List<double> GetCustomFichesVanilla(double initialSum)
        {
            List<double> fiches = new List<double>();
            double valueCount = initialSum;
            foreach (double fish in Runtime.availableCustomFiches)
            {
                while (valueCount >= fish)
                {
                    fiches.Add(fish);
                    valueCount = Math.Round((valueCount - fish), 2);
                }
            }
            return fiches;
        }

        private static List<double> GetCustomFichesRaddoppia(double initialSum)
        {
            List<double> fiches = new List<double>();
            fiches = Calcs.GetCustomFichesVanilla(initialSum / 2);
            fiches.Add(-1);
            double valueCount = initialSum - Calcs.CountValues(fiches);
            int lowest = Calcs.getLowestCustomFicheValue();
            if (valueCount >= lowest)
            {
                foreach (double fish in Runtime.availableCustomFiches)
                {
                    while (valueCount >= fish)
                    {
                        fiches.Add(fish);
                        valueCount -= fish;
                    }
                }
            }
            return fiches;
        }

        private static List<double> GetCustomFichesQuadruplica(double initialSum)
        {
            List<double> fiches = new List<double>();
            fiches = Calcs.GetCustomFichesVanilla(initialSum / 4);
            fiches.Add(-1);
            fiches.Add(-1);
            double valueCount = initialSum - Calcs.CountValues(fiches);
            int lowest = Calcs.getLowestCustomFicheValue();
            if (valueCount >= lowest)
            {
                foreach (double fish in Runtime.availableCustomFiches)
                {
                    while (valueCount >= fish)
                    {
                        fiches.Add(fish);
                        valueCount -= fish;
                    }
                }
            }
            return fiches;
        }

        private static int getLowestCustomFicheValue()
        {
            int minVal = int.MaxValue;
            foreach (int v in Runtime.availableCustomFiches)
            {
                if (v < minVal)
                {
                    minVal = v;
                }
            }
            return minVal;
        }

        private static int CountValues(List<int> list)
        {
            int val = 0;
            foreach (int value in list)
            {
                if (value == 222)
                {
                    val *= 2;
                }
                else
                {
                    val += value;
                }
            }
            return val;
        }

        private static double CountValues(List<double> list)
        {
            double val = 0;
            foreach (double value in list)
            {
                if (value == 222)
                {
                    val *= 2;
                }
                else
                {
                    val += value;
                }
            }
            return val;
        }

        public static List<int> GetBestFichesEver(int initialSum)
        {
            List<int> fiches = new List<int>();
            int valueCount = initialSum;
            foreach (int fish in Constants.availableFiches)
            {
                while (valueCount >= fish)
                {
                    fiches.Add(fish);
                    valueCount -= fish;
                }
            }
            List<int> fichesVanilla = Calcs.GetFichesVanilla(initialSum);
            List<int> fichesDoubles = Calcs.GetFichesRaddoppia(initialSum);
            List<int> fichesQuadrup = Calcs.GetFichesQuadruplica(initialSum);
            if (fichesVanilla.Count < fichesDoubles.Count)
            {
                if (fichesVanilla.Count < fichesQuadrup.Count)
                {
                    return fichesVanilla;
                }
                return fichesQuadrup;
            }
            else
            {
                if (fichesDoubles.Count < fichesQuadrup.Count)
                {
                    return fichesDoubles;
                }
                return fichesQuadrup;
            }
        }

        public static List<int> GetBestFiches(int initialSum)
        {
            List<int> fiches = new List<int>();
            if (initialSum == 0)
            {
                return fiches;
            }
            int i = 0;
            for (; ; )
            {
                int tempValue = initialSum;
                new List<int>();
                foreach (int fish in Constants.availableFiches)
                {
                    while (tempValue >= fish)
                    {
                        fiches.Add(fish);
                        tempValue -= fish;
                    }
                }
                i++;
            }
        }
    }
}
