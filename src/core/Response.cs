using System;
using System.Globalization;

namespace GenCalc.Core.Numerical
{
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
