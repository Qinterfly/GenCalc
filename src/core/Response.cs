using System;
using System.Collections.Generic;

namespace GenCalc.Core.Project
{
    using PairDouble = Tuple<double, double>;

    public class Response
    {
        public Response(in string path, in string name, in double[] frequency, in double[] realPart, in double[] imaginaryPart)
        {
            Path = path;
            Name = name;
            Frequency = frequency;
            RealPart = realPart;
            ImaginaryPart = imaginaryPart;
            int nResponse = RealPart.Length;
            Amplitude = new double[nResponse];
            for (int i = 0; i != nResponse; ++i)
                Amplitude[i] = Math.Sqrt(Math.Pow(RealPart[i], 2) + Math.Pow(ImaginaryPart[i], 2));
        }

        public bool equals(Response another)
        {
            return Path == another.Path;
        }

        private PairDouble findPeak(double[] X, double[,] Y, int iColumn)
        {
            double maxValue = 0.0;
            double tempValue;
            double amplitude = 0.0;
            double root = -1.0;
            int nY = Y.GetLength(0);
            for (int i = 0; i != nY; ++i)
            {
                tempValue = Math.Abs(Y[i, iColumn]);
                if (tempValue > maxValue)
                {
                    maxValue = tempValue; 
                    amplitude = Math.Sqrt(Math.Pow(Y[i, 0], 2.0) + Math.Pow(Y[i, 1], 2.0));
                    root = X[i];
                }
            }
            return new PairDouble(root, amplitude);
        }

        private PairDouble findRoot(double[] X, double[,] Y, int iColumn)
        {
            double prevValue = Y[0, iColumn];
            double root = -1.0;
            double amplitude = 0.0;
            bool isFound = false;
            int nY = Y.GetLength(0);
            for (int i = 1; i != nY; ++i)
            {
                isFound = Y[i, iColumn] * prevValue < 0.0;
                if (isFound)
                {
                    root = (X[i] + X[i - 1]) / 2.0;
                    // Calculating amplitude
                    amplitude = Math.Sqrt(Math.Pow(Y[i - 1, 0], 2.0) + Math.Pow(Y[i - 1, 1], 2.0)); // First
                    amplitude += Math.Sqrt(Math.Pow(Y[i, 0], 2.0) + Math.Pow(Y[i, 1], 2.0));        // Second
                    amplitude /= 2.0;                                                               // Mean
                    break;
                }
                prevValue = Y[i, iColumn];
            }
            return isFound ? new PairDouble(root, amplitude) : null;
        }

        // Data
        public readonly double[] Frequency;
        public readonly double[] RealPart;
        public readonly double[] ImaginaryPart;
        public readonly double[] Amplitude;
        // Info
        public readonly string Name;
        public readonly string Path;
    }
}
