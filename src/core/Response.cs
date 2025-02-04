﻿using System;
using System.Globalization;
using System.IO;

namespace GenCalc.Core.Numerical
{
    public enum ResponseType
    {
        kUnknown,
        kDisp,
        kVeloc,
        kAccel,
        kForce
    }

    public class Response
    {
        public Response(ResponseType type, in string path, in string name, in string originalRun, in string node, in string direction,
                        in double[] frequency, in double[] realPart, in double[] imaginaryPart)
        {
            Type = type;
            Path = path;
            Name = name;
            OriginalRun = originalRun;
            Node = node;
            Direction = direction;
            Frequency = frequency;
            RealPart = realPart;
            ImaginaryPart = imaginaryPart;
            int nResponse = RealPart.Length;
            Amplitude = new double[nResponse];
            for (int i = 0; i != nResponse; ++i)
                Amplitude[i] = Math.Sqrt(Math.Pow(RealPart[i], 2) + Math.Pow(ImaginaryPart[i], 2));
        }

        public Response convert(ResponseType type)
        {
            if (this.Type == ResponseType.kAccel)
            {
                double[] resRealPart = RealPart.Clone() as double[];
                double[] resImaginaryPart = ImaginaryPart.Clone() as double[];
                double twoPI = 2.0 * Math.PI;
                double delimiter;
                int nResponse = RealPart.Length;
                switch (type)
                {
                    case ResponseType.kDisp:
                        for (int i = 0; i != nResponse; ++i)
                        {
                            delimiter = Math.Pow(twoPI * Frequency[i], 2.0);
                            resRealPart[i] /= delimiter;
                            resImaginaryPart[i] /= delimiter;
                        }
                        break;
                    case ResponseType.kVeloc:
                        double tValue;
                        for (int i = 0; i != nResponse; ++i)
                        {
                            delimiter = twoPI * Frequency[i];
                            tValue = -resRealPart[i];
                            resRealPart[i] = resImaginaryPart[i] / delimiter;
                            resImaginaryPart[i] = tValue / delimiter;
                        }
                        break;
                    default:
                        return null;
                }

                return new Response(type, Path, Name, OriginalRun, Node, Direction, Frequency, resRealPart, resImaginaryPart);
            }
            return null;
        }

        public bool equals(Response another)
        {
            return Path == another.Path;
        }

        public int Length { get { return RealPart.Length; } }

        public double getFrequencyValue()
        {
            double frequency;
            int indFrequency = Math.Max(Path.IndexOf("Гц"), Path.IndexOf("Hz"));
            if (indFrequency < 0)
                return 0.0;
            string strFrequency = null;
            bool isValue;
            bool isSequence = false;
            string path = Path.Replace(",", ".");
            while (indFrequency > 0)
            {
                --indFrequency;
                isValue = double.TryParse(path[indFrequency].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double tempVal);
                if (isValue || path[indFrequency] == '.')
                {
                    strFrequency = path[indFrequency] + strFrequency;
                    isSequence = true;
                }
                else if (isSequence)
                {
                    break;
                }

            }
            if (double.TryParse(strFrequency, NumberStyles.Any, CultureInfo.InvariantCulture, out frequency))
                return frequency;
            else
                return 0.0;
        }

        public Tuple<double, double> getFrequencyBoundaries()
        {
            return new Tuple<double, double>(Frequency[0], Frequency[Frequency.Length - 1]);
        }

        public void write(string pathFile)
        {
            using (StreamWriter stream = new StreamWriter(pathFile))
            {
                for (int i = 0; i < Frequency.Length; i++)
                    stream.Write("{0}\t{1}\t{2}\t{3}\n", Frequency[i], RealPart[i], ImaginaryPart[i], Amplitude[i]);
            }
        }

        // Data
        public readonly double[] Frequency;
        public readonly double[] RealPart;
        public readonly double[] ImaginaryPart;
        public readonly double[] Amplitude;
        // Info
        public readonly ResponseType Type;
        public readonly string Name;
        public readonly string Path;
        public readonly string OriginalRun;
        public readonly string Node;
        public readonly string Direction;
    }
}
