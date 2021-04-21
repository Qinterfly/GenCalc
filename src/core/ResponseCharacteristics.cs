using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCalc.Core.Numerical
{
    public class ResponseCharacteristics
    {
        public ResponseCharacteristics(in Response response,
                                       ref Tuple<double, double> frequencyBoundaries,
                                       ref Tuple<double, double> levelsBoundaries,
                                       int numLevelsSteps,
                                       int numInterpolationPoints = 512)
        {
            
            if (!correctFrequencyBoundaries(response, ref frequencyBoundaries))
                return;
            // Create a mesh for the further interpolation
            double[] frequency = response.Frequency;
            double[] interpFrequency = Utilites.createUniformMesh(frequencyBoundaries.Item1, frequencyBoundaries.Item2, numInterpolationPoints);
            // Interpolate the response
            double[] interpRealPart = Utilites.interpolate(frequency, response.RealPart, interpFrequency);
            double[] interpImaginaryPart = Utilites.interpolate(frequency, response.ImaginaryPart, interpFrequency);
            double[] interpAmplitude = Utilites.interpolate(frequency, response.Amplitude, interpFrequency);
            // TODO
        }

        private bool correctFrequencyBoundaries(in Response response, ref Tuple<double, double> boundaries)
        {
            int numResponse = response.RealPart.Length;
            double startFrequency = response.Frequency[0];
            double endFrequency = response.Frequency[numResponse - 1];
            double leftBoundary = boundaries.Item1;
            double rightBoundary = boundaries.Item2;
            if (leftBoundary < 0 || rightBoundary < 0)
            {
                leftBoundary = startFrequency;
                rightBoundary = endFrequency;
                boundaries = new Tuple<double, double>(leftBoundary, rightBoundary);
            }
            bool isInputConsistent = leftBoundary < rightBoundary && leftBoundary >= startFrequency && rightBoundary <= endFrequency;
            return isInputConsistent;
        }

        private void correctLevelsBoundaries(in Response response, ref Tuple<double, double> boundaries)
        {
            // TODO
        }

        public readonly double[] Levels;
        public readonly DecrementData Decrement;
        public readonly ModalCharateristics ModalData;
    }

    public class ModalCharateristics
    {
        public double[] mass;
        public double[] stiffness;
        public double[] damping;
    }

    public class DecrementData
    {
        public double[] Imaginary;
        public double[] Real;
        public double[] Amplitude;
    }
}
