﻿using System;
using System.Collections.Generic;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.RootFinding;

namespace GenCalc.Core.Numerical
{
    using PairDouble = Tuple<double, double>;
    public class ResponseCharacteristics
    {
        public ResponseCharacteristics(in Response response,
                                       ref PairDouble frequencyBoundaries,
                                       ref PairDouble levelsBoundaries,
                                       int numLevels,
                                       int numInterpolationPoints = 512)
        {
            // Correct limits of levels and frequencies
            if (!correctFrequencyBoundaries(response, ref frequencyBoundaries))
                return;
            correctLevelsBoundaries(ref levelsBoundaries);
            // Interpolate the response
            double[] frequency = response.Frequency;
            CubicSpline splineRealPart = CubicSpline.InterpolateNatural(frequency, response.RealPart);
            CubicSpline splineImagPart = CubicSpline.InterpolateNatural(frequency, response.ImaginaryPart);
            CubicSpline splineAmplitude = CubicSpline.InterpolateNatural(frequency, response.Amplitude);
            // Create mesh of levels
            double startLevel = levelsBoundaries.Item1;
            double endLevel = levelsBoundaries.Item2;
            double stepLevel = (endLevel - startLevel) / (numLevels - 1);
            Levels = new double[numLevels];
            for (int i = 0; i != numLevels; ++i)
                Levels[i] = startLevel + i * stepLevel;
            // Calculate decrements
            Decrement = new DecrementData();
            double resonanceFrequency = response.getFrequencyValue();
            resonanceFrequency = retrieveImagResonanceFrequency(splineImagPart, frequencyBoundaries, numInterpolationPoints, resonanceFrequency);
            calculateDecrementByImaginary(splineImagPart, frequencyBoundaries, numInterpolationPoints, resonanceFrequency);
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
                boundaries = new PairDouble(leftBoundary, rightBoundary);
            }
            bool isInputConsistent = leftBoundary < rightBoundary && leftBoundary >= startFrequency && rightBoundary <= endFrequency;
            return isInputConsistent;
        }

        private void correctLevelsBoundaries(ref PairDouble levelsBoundaries)
        {
            double resMin = levelsBoundaries.Item1;
            double resMax = levelsBoundaries.Item2;
            double tempValue;
            if (resMin < 0.0)
                resMin = 0.0;
            if (resMax > 1.0)
                resMax = 1.0;
            if (resMin > resMax)
            {
                tempValue = resMin;
                resMin = resMax;
                resMax = tempValue;
            }
            levelsBoundaries = new PairDouble(resMin, resMax);
        }

        private void calculateDecrementByImaginary(CubicSpline splineImag, in PairDouble frequencyBoundaries, int numInterpolationPoints, double resonanceFrequency)
        {
            double startFrequency = frequencyBoundaries.Item1;
            double endFrequency = frequencyBoundaries.Item2;
            double leftFrequency = startFrequency;
            double rightFrequency = endFrequency;
            PairDouble minMax = Utilities.findSplineMinMax(splineImag, startFrequency, endFrequency, numInterpolationPoints);
            double minValue = minMax.Item1;
            double levelsInterval = minMax.Item2 - minValue;
            double targetValue;
            double resonancePeak = splineImag.Interpolate(resonanceFrequency);
            int numLevels = Levels.Length;
            Decrement.Imaginary = new Dictionary<double, double>();
            for (int iLevel = 0; iLevel != numLevels; ++iLevel)
            {
                targetValue = Levels[iLevel] * levelsInterval + minValue;
                Func<double, double> fun = x => splineImag.Interpolate(x) - targetValue;
                Func<double, double> diffFun = x => splineImag.Differentiate(x);
                List<double> roots = Utilities.findAllRootsBisection(fun, startFrequency, endFrequency, numInterpolationPoints);
                int nRoots = roots.Count;
                if (nRoots < 2)
                    continue;
                leftFrequency = NewtonRaphson.FindRootNearGuess(fun, diffFun, roots[0], startFrequency, endFrequency);
                rightFrequency = NewtonRaphson.FindRootNearGuess(fun, diffFun, roots[nRoots - 1], startFrequency, endFrequency);
                if (leftFrequency < startFrequency || rightFrequency > endFrequency || leftFrequency > rightFrequency)
                    continue;
                double deltaFreq = (rightFrequency - leftFrequency) / resonanceFrequency;
                double fracAmp = Math.Abs(targetValue / resonancePeak);
                double decrement = Math.PI * deltaFreq * Math.Sqrt(fracAmp / (1.0 - fracAmp));
                if (!Double.IsNaN(decrement))
                    Decrement.Imaginary.Add(Levels[iLevel], decrement);
            }
        }

        private double retrieveImagResonanceFrequency(CubicSpline splineImag, in PairDouble frequencyBoundaries, int numPoints, double approximation)
        {
            Func<double, double> resonanceFunc = x => splineImag.Differentiate(x);
            Func<double, double> resonanceDiffFunc = x => splineImag.Differentiate2(x);
            List<double> resFrequencies = Utilities.findAllRootsBisection(resonanceFunc, frequencyBoundaries.Item1, frequencyBoundaries.Item2, numPoints);
            double distance;
            double minDistance = Double.MaxValue;
            int indClosestResonance = 0;
            int numFrequencies = resFrequencies.Count;
            for (int i = 0; i != numFrequencies; ++i)
            {
                resFrequencies[i] = NewtonRaphson.FindRootNearGuess(resonanceFunc, resonanceDiffFunc, resFrequencies[i], frequencyBoundaries.Item1, frequencyBoundaries.Item2);
                distance = Math.Abs(resFrequencies[i] - approximation);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    indClosestResonance = i;
                }

            }
            return resFrequencies[indClosestResonance];
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
        public Dictionary<double, double> Imaginary;
        public double[] Amplitude;
    }
}
