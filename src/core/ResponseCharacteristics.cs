using System;
using System.Collections.Generic;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.RootFinding;

namespace GenCalc.Core.Numerical
{
    using PairDouble = Tuple<double, double>;

    public enum DecrementType
    {
        kImaginary,
        kAmplitude
    }

    public class ResponseCharacteristics
    {
        public ResponseCharacteristics(in Response acceleration,
                                       ref PairDouble frequencyBoundaries, ref PairDouble levelsBoundaries,
                                       int numLevels, int numInterpolationPoints = 512,
                                       double? manualFrequencyReal = null, double? manualFrequencyImaginary = null, double? manualFrequencyAmplitude = null,
                                       in ModalDataSet modalSet = null)
        {
            if (acceleration == null)
                return;
            // Perform the double integration of the acceleration signal
            Response displacement = acceleration.convert(ResponseType.kDisp);
            // Correct limits of levels and frequencies
            if (!correctFrequencyBoundaries(displacement, ref frequencyBoundaries))
                return;
            correctLevelsBoundaries(ref levelsBoundaries);
            // Interpolate the displacement
            double[] frequency = displacement.Frequency;
            CubicSpline splineRealDisplacement = CubicSpline.InterpolateNatural(frequency, displacement.RealPart);
            CubicSpline splineImagDisplacement = CubicSpline.InterpolateNatural(frequency, displacement.ImaginaryPart);
            CubicSpline splineAmplitudeDisplacement = CubicSpline.InterpolateNatural(frequency, displacement.Amplitude);
            // Create mesh of levels
            double startLevel = levelsBoundaries.Item1;
            double endLevel = levelsBoundaries.Item2;
            double stepLevel = (endLevel - startLevel) / (numLevels - 1);
            Levels = new double[numLevels];
            for (int i = 0; i != numLevels; ++i)
                Levels[i] = startLevel + i * stepLevel;
            // Finding resonance frequencies
            double startFrequency = acceleration.getFrequencyValue();
            // Real
            if (manualFrequencyReal == null)
                ResonanceFrequencyReal = retrieveResonanceFrequency(splineRealDisplacement, false, frequencyBoundaries, numInterpolationPoints, startFrequency);
            else
                ResonanceFrequencyReal = (double)manualFrequencyReal;
            // Imaginary
            if (manualFrequencyImaginary == null)
                ResonanceFrequencyImaginary = retrieveResonanceFrequency(splineImagDisplacement, true, frequencyBoundaries, numInterpolationPoints, startFrequency);
            else
                ResonanceFrequencyImaginary = (double)manualFrequencyImaginary;
            // Amplitude
            if (manualFrequencyAmplitude == null)
                ResonanceFrequencyAmplitude = retrieveResonanceFrequency(splineAmplitudeDisplacement, true, frequencyBoundaries, numInterpolationPoints, startFrequency);
            else
                ResonanceFrequencyAmplitude = (double)manualFrequencyAmplitude;
            // Calculate resonance peaks
            Decrement = new DecrementData();
            ResonanceRealPeak = splineRealDisplacement.Interpolate(ResonanceFrequencyReal);
            ResonanceImaginaryPeak = splineImagDisplacement.Interpolate(ResonanceFrequencyImaginary);
            ResonanceAmplitudePeak = splineAmplitudeDisplacement.Interpolate(ResonanceFrequencyAmplitude);
            // Compute decrements
            calculateDecrement(DecrementType.kImaginary, splineImagDisplacement, frequencyBoundaries, numInterpolationPoints);
            calculateDecrement(DecrementType.kAmplitude, splineAmplitudeDisplacement, frequencyBoundaries, numInterpolationPoints);
            calculateDecrementByReal(splineRealDisplacement, frequencyBoundaries, numInterpolationPoints);
            // Modal data
            if (modalSet != null && modalSet.isCorrect() && frequency.Length == modalSet.ReferenceResponse.Amplitude.Length)
            {
                ModalGeneral = new ModalParameters();
                Response referenceDisplacement = modalSet.ReferenceResponse.convert(ResponseType.kDisp);
                CubicSpline splineAmplitudeReference = CubicSpline.InterpolateNatural(frequency, referenceDisplacement.Amplitude);
                // By amplitudes
                CubicSpline splineGeneralForce = calculateGeneralForce(modalSet);
                if (splineGeneralForce != null)
                    calculateModal(splineAmplitudeReference, splineGeneralForce, frequencyBoundaries, numInterpolationPoints);
                // By complex power
                Tuple<CubicSpline, CubicSpline> complexPower = calculateComplexPower(modalSet);
                if (complexPower != null)
                {
                    ModalComplex = new ModalParameters();
                    double resonanceFrequencyComplexPower = retrieveResonanceFrequency(complexPower.Item1, false, frequencyBoundaries, numInterpolationPoints, ResonanceFrequencyImaginary);
                    estimateByComplexPower(complexPower, splineAmplitudeReference, resonanceFrequencyComplexPower);
                }
            }
            // Convert back the resonance peaks
            CubicSpline splineRealAcceleration = CubicSpline.InterpolateNatural(frequency, acceleration.RealPart);
            CubicSpline splineImagAcceleration = CubicSpline.InterpolateNatural(frequency, acceleration.ImaginaryPart);
            CubicSpline splineAmplitudeAcceleration = CubicSpline.InterpolateNatural(frequency, acceleration.Amplitude);
            ResonanceRealPeak = splineRealAcceleration.Interpolate(ResonanceFrequencyReal);
            ResonanceImaginaryPeak = splineImagAcceleration.Interpolate(ResonanceFrequencyImaginary);
            ResonanceAmplitudePeak = splineAmplitudeAcceleration.Interpolate(ResonanceFrequencyAmplitude);
        }

        private bool correctFrequencyBoundaries(in Response response, ref PairDouble boundaries)
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

        private double retrieveResonanceFrequency(CubicSpline spline, bool isDerivativeZero, in PairDouble frequencyBoundaries, int numPoints, double approximation)
        {
            Func<double, double> resonanceFunc;
            Func<double, double> resonanceDiffFunc;
            if (isDerivativeZero)
            {
                resonanceFunc = x => spline.Differentiate(x);
                resonanceDiffFunc = x => spline.Differentiate2(x);
            }
            else
            {
                resonanceFunc = x => spline.Interpolate(x);
                resonanceDiffFunc = x => spline.Differentiate(x);
            }
            List<double> resFrequencies = Utilities.findAllRootsBisection(resonanceFunc, frequencyBoundaries.Item1, frequencyBoundaries.Item2, numPoints);
            if (resFrequencies.Count == 0)
                return approximation;
            double distance;
            double minDistance = Double.MaxValue;
            int indClosestResonance = 0;
            int numFrequencies = resFrequencies.Count;
            for (int i = 0; i != numFrequencies; ++i)
            {
                try
                {
                    resFrequencies[i] = NewtonRaphson.FindRootNearGuess(resonanceFunc, resonanceDiffFunc, resFrequencies[i], frequencyBoundaries.Item1, frequencyBoundaries.Item2);
                }
                catch
                {
                    continue;
                }
                distance = Math.Abs(resFrequencies[i] - approximation);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    indClosestResonance = i;
                }
            }
            return resFrequencies[indClosestResonance];
        }

        private void calculateDecrement(DecrementType type, CubicSpline spline, in PairDouble frequencyBoundaries, int numInterpolationPoints)
        {
            double startFrequency = frequencyBoundaries.Item1;
            double endFrequency = frequencyBoundaries.Item2;
            double targetValue;
            double resonancePeak;
            int numLevels = Levels.Length;
            double resonanceFrequency;
            switch (type)
            {
                case DecrementType.kImaginary:
                    Decrement.Imaginary = new Dictionary<double, double>();
                    resonancePeak = ResonanceImaginaryPeak;
                    resonanceFrequency = ResonanceFrequencyImaginary;
                    break;
                case DecrementType.kAmplitude:
                    Decrement.Amplitude = new Dictionary<double, double>();
                    resonancePeak = ResonanceAmplitudePeak;
                    resonanceFrequency = ResonanceFrequencyAmplitude;
                    break;
                default:
                    return;
            }
            for (int iLevel = 0; iLevel != numLevels; ++iLevel)
            {
                targetValue = Levels[iLevel] * resonancePeak;
                PairDouble levelFrequencyBoundaries = findLevelRootsAroundResonance(spline, targetValue, frequencyBoundaries, numInterpolationPoints, resonanceFrequency);
                if (levelFrequencyBoundaries == null)
                    continue;
                double leftFrequency = levelFrequencyBoundaries.Item1;
                double rightFrequency = levelFrequencyBoundaries.Item2;
                if (leftFrequency < startFrequency || rightFrequency > endFrequency || leftFrequency > rightFrequency)
                    continue;
                double deltaFreq = (rightFrequency - leftFrequency) / resonanceFrequency;
                double fracAmp = Math.Abs(targetValue / resonancePeak);
                double decrement;
                switch (type)
                {
                    case DecrementType.kImaginary:
                        decrement = Math.PI * deltaFreq * Math.Sqrt(fracAmp / (1.0 - fracAmp));
                        if (!Double.IsNaN(decrement))
                            Decrement.Imaginary.Add(Levels[iLevel], decrement);
                        break;
                    case DecrementType.kAmplitude:
                        decrement = Math.PI * deltaFreq * fracAmp / Math.Sqrt(1.0 - Math.Pow(fracAmp, 2.0));
                        if (!Double.IsNaN(decrement))
                            Decrement.Amplitude.Add(Levels[iLevel], decrement);
                        break;
                }
            }
        }

        private void calculateDecrementByReal(CubicSpline spline, in PairDouble frequencyBoundaries, int numInterpolationPoints)
        {
            double startFrequency = frequencyBoundaries.Item1;
            double endFrequency = frequencyBoundaries.Item2;
            Func<double, double> fun = x => spline.Differentiate(x);
            Func<double, double> diffFun = x => spline.Differentiate2(x);
            List<double> roots = Utilities.findAllRootsBisection(fun, startFrequency, endFrequency, numInterpolationPoints);
            int numRoots = roots.Count;
            if (numRoots < 2)
            {
                Decrement.Real = -1.0;
                return;
            }
            double leftFrequency = roots[0];
            double rightFrequency = roots[numRoots - 1];
            try
            {
                leftFrequency = NewtonRaphson.FindRootNearGuess(fun, diffFun, leftFrequency, startFrequency, endFrequency, maxIterations: mkNumRootFindingIterations);
                rightFrequency = NewtonRaphson.FindRootNearGuess(fun, diffFun, rightFrequency, startFrequency, endFrequency, maxIterations: mkNumRootFindingIterations);
            }
            catch
            {

            }
            Decrement.Real = Math.PI * (rightFrequency - leftFrequency) / ResonanceFrequencyReal;
        }

        private void calculateModal(CubicSpline amplitude, in CubicSpline force, in PairDouble frequencyBoundaries, int numInterpolationPoints)
        {
            const double kTwoPi = 2.0 * Math.PI;
            int numLevels = Levels.Length;
            Decrement.General = new Dictionary<double, double>();
            for (int iLevel = 0; iLevel != numLevels; ++iLevel)
            {
                double levelValue = Levels[iLevel];
                double targetValue = levelValue * ResonanceAmplitudePeak;
                PairDouble levelFrequencyBoundaries = findLevelRootsAroundResonance(amplitude, targetValue, frequencyBoundaries, numInterpolationPoints, ResonanceFrequencyAmplitude);
                if (levelFrequencyBoundaries == null)
                    continue;
                double[] frequency = Utilities.createUniformMesh(levelFrequencyBoundaries.Item1, levelFrequencyBoundaries.Item2, numInterpolationPoints);
                double freqI, freqJ;
                double omegaI, omegaJ, omegaI2, omegaJ2, omegaI4, omegaJ4;
                double forceI, forceJ, forceI2, forceJ2;
                double ampI, ampJ, ampI2, ampJ2, ampI4, ampJ4;
                double[] f = new double[3] { 0.0, 0.0, 0.0 };
                double[] d = new double[3] { 0.0, 0.0, 0.0 };
                double[] dampingNumerators = new double[4] { 0.0, 0.0, 0.0, 0.0 };
                double dampingDenominator = 0.0;
                for (int i = 0; i != numInterpolationPoints; ++i)
                {
                    // Frequency
                    freqI = frequency[i];
                    omegaI = freqI * kTwoPi;
                    omegaI2 = Math.Pow(omegaI, 2.0);
                    omegaI4 = Math.Pow(omegaI, 4.0);
                    // Displacement
                    ampI = amplitude.Interpolate(freqI);
                    ampI2 = Math.Pow(ampI, 2.0);
                    ampI4 = Math.Pow(ampI, 4.0);
                    // Force
                    forceI = force.Interpolate(freqI);
                    forceI2 = Math.Pow(forceI, 2.0);
                    for (int j = 0; j != numInterpolationPoints; ++j)
                    {
                        // Frequency
                        freqJ = frequency[j];
                        omegaJ = freqJ * kTwoPi;
                        omegaJ2 = Math.Pow(omegaJ, 2.0);
                        omegaJ4 = Math.Pow(omegaJ, 4.0);
                        // Displacement
                        ampJ = amplitude.Interpolate(freqJ);
                        ampJ2 = Math.Pow(ampJ, 2.0);
                        ampJ4 = Math.Pow(ampJ, 4.0);
                        // Force
                        forceJ = force.Interpolate(freqJ);
                        forceJ2 = Math.Pow(forceJ, 2.0);
                        // Calculation "f"
                        f[0] += ampI4 * ampJ4 * omegaJ4 * (omegaJ4 - omegaI4);
                        f[1] += ampI4 * ampJ4 * omegaI4 * (omegaJ2 - omegaI2);
                        f[2] += ampI2 * ampJ2 * omegaI4 * (ampI2 * forceJ2 - ampJ2 * forceI2);
                        // Calculation "d"
                        d[0] += ampI4 * ampJ4 * omegaI2 * (omegaI4 - omegaJ4);
                        d[1] += ampI4 * ampJ4 * omegaI2 * (omegaJ2 - omegaI2);
                        d[2] += ampI2 * ampJ2 * omegaI2 * (ampI2 * forceJ2 - ampJ2 * forceI2);
                    } // J
                    dampingNumerators[0] += forceI2 * ampI2;
                    dampingNumerators[1] += ampI4;
                    dampingNumerators[2] += ampI4 * omegaI4;
                    dampingNumerators[3] += ampI4 * omegaI2;
                    dampingDenominator += ampI4;
                } // I
                f[1] *= 2.0;
                d[1] *= 2.0;
                // Calculate modal mass and stiffness
                double b = (f[1] * d[2] - f[2] * d[1]) / (f[0] * d[1] - f[1] * d[0]);
                if (b < 0)
                    continue;
                double mass = Math.Sqrt(b);
                double stiffness = -(b * d[0] + d[2]) / (d[1] * mass);
                if (stiffness < 0)
                    continue;
                double resFrequency = Math.Sqrt(stiffness / mass) / kTwoPi;
                // Calculate damping 
                dampingNumerators[1] *= -Math.Pow(stiffness, 2.0);
                dampingNumerators[2] *= -Math.Pow(mass, 2.0);
                dampingNumerators[3] *= 2.0 * mass * stiffness;
                double damping = 0.0;
                foreach (double num in dampingNumerators)
                    damping += num;
                damping /= dampingDenominator;
                if (damping < 0)
                    continue;
                damping = Math.Sqrt(damping);
                double decrementDenominator = Math.Sqrt(4.0 * Math.Pow(stiffness / damping, 2.0) - 1);
                if (decrementDenominator > 0)
                {
                    double decrement = 2.0 * Math.PI / decrementDenominator;
                    Decrement.General.Add(levelValue, decrement);
                }
                // Add results
                ModalGeneral.Levels.Add(levelValue);
                ModalGeneral.Mass.Add(mass);
                ModalGeneral.Stiffness.Add(stiffness);
                ModalGeneral.Frequency.Add(resFrequency);
                ModalGeneral.Damping.Add(damping);
            }
        }

        private PairDouble findLevelRootsAroundResonance(CubicSpline spline, double targetValue, in PairDouble frequencyBoundaries, int numPoints, double resonanceFrequency)
        {
            double startFrequency = frequencyBoundaries.Item1;
            double endFrequency = frequencyBoundaries.Item2;
            Func<double, double> fun = x => spline.Interpolate(x) - targetValue;
            Func<double, double> diffFun = x => spline.Differentiate(x);
            List<double> roots = Utilities.findAllRootsBisection(fun, startFrequency, endFrequency, numPoints);
            int nRoots = roots.Count;
            if (nRoots < 2)
                return null;
            int leftIndex = roots.FindLastIndex(x => x < resonanceFrequency);
            int rightIndex = roots.FindIndex(x => x > resonanceFrequency);
            if (leftIndex < 0 || rightIndex < 0)
            {
                leftIndex = 0;
                rightIndex = nRoots - 1;
            }
            double leftRoot = roots[leftIndex];
            double rightRoot = roots[rightIndex];
            try
            {
                leftRoot = NewtonRaphson.FindRootNearGuess(fun, diffFun, leftRoot, startFrequency, endFrequency, maxIterations: mkNumRootFindingIterations);
                rightRoot = NewtonRaphson.FindRootNearGuess(fun, diffFun, rightRoot, startFrequency, endFrequency, maxIterations: mkNumRootFindingIterations);
            }
            catch
            {

            }
            if (leftRoot > resonanceFrequency || rightRoot < resonanceFrequency)
                return null;
            return new PairDouble(leftRoot, rightRoot);
        }

        private CubicSpline calculateGeneralForce(ModalDataSet modalSet)
        {
            double kTwoPi = 2.0 * Math.PI;
            List<int> links = modalSet.getForceLinks();
            if (links == null)
                return null;
            int numForce = modalSet.Forces.Count;
            int lengthSignal = modalSet.Forces[0].Length;
            if (lengthSignal < 2)
                return null;
            double[] frequency = modalSet.Forces[0].Frequency;
            double[] generalForce = new double[lengthSignal];
            double[] omega2 = new double[lengthSignal];
            for (int i = 0; i != lengthSignal; ++i)
            {
                generalForce[i] = 0.0;
                omega2[i] = Math.Pow(frequency[i] * kTwoPi, 2.0);
            }
            for (int iForce = 0; iForce != numForce; ++iForce)
            {
                int iResponse = links[iForce];
                double[] forceAmplitude = modalSet.Forces[iForce].Amplitude;
                double[] responseImag = modalSet.Responses[iResponse].ImaginaryPart;
                for (int i = 0; i != lengthSignal; ++i)
                    generalForce[i] += Math.Abs(forceAmplitude[i] * responseImag[i] / omega2[i]);
            }
            double[] referenceImag = modalSet.ReferenceResponse.ImaginaryPart;
            for (int i = 0; i != lengthSignal; ++i)
                generalForce[i] = Math.Abs(generalForce[i] / referenceImag[i] * omega2[i]);
            return CubicSpline.InterpolateNatural(frequency, generalForce);
        }

        private Tuple<CubicSpline, CubicSpline> calculateComplexPower(ModalDataSet modalSet)
        {
            List<int> links = modalSet.getForceLinks();
            if (links == null)
                return null;
            int numForce = modalSet.Forces.Count;
            int lengthSignal = modalSet.Forces[0].Length;
            if (lengthSignal < 2)
                return null;
            List<Response> velocities = new List<Response>(numForce);
            foreach (Response response in modalSet.Responses)
                velocities.Add(response.convert(ResponseType.kVeloc));
            double[] resRealPart = new double[lengthSignal];
            double[] resImagPart = new double[lengthSignal];
            for (int iForce = 0; iForce != numForce; ++iForce)
            {
                int iResponse = links[iForce];
                double[] forceReal = modalSet.Forces[iForce].RealPart;
                double[] forceImag = modalSet.Forces[iForce].ImaginaryPart;
                double[] velocityReal = velocities[iResponse].RealPart;
                double[] velocityImag = velocities[iResponse].ImaginaryPart;
                for (int i = 0; i != lengthSignal; ++i)
                {
                    resRealPart[i] += 0.5 * (forceReal[i] * velocityReal[i] - forceImag[i] * velocityImag[i]);
                    resImagPart[i] += 0.5 * (forceReal[i] * velocityImag[i] + forceImag[i] * velocityReal[i]);
                }
            }
            double[] frequency = modalSet.Forces[0].Frequency;
            CubicSpline resRealSpline = CubicSpline.InterpolateNatural(frequency, resRealPart);
            CubicSpline resImagSpline = CubicSpline.InterpolateNatural(frequency, resImagPart);
            return new Tuple<CubicSpline, CubicSpline>(resRealSpline, resImagSpline);
        }

        private void estimateByComplexPower(Tuple<CubicSpline, CubicSpline> complexPower, CubicSpline reference, double frequency)
        {
            const double kTwoPI = 2.0 * Math.PI;
            double radFrequency = kTwoPI * frequency;
            double slope = complexPower.Item2.Differentiate(frequency) / kTwoPI;

            // Estimate the modal parameters
            double mass = -1.0 / Math.Pow(reference.Interpolate(frequency) * radFrequency, 2.0) * slope;
            double stiffness = mass * Math.Pow(radFrequency, 2.0);
            double damping = complexPower.Item1.Interpolate(frequency) / (Math.Pow(radFrequency, 3.0) * mass * Math.Pow(reference.Interpolate(frequency), 2.0));
            double decrement = kTwoPI * damping;
            double resFrequency = Math.Sqrt(stiffness / mass) / kTwoPI;

            // Insert the results
            Decrement.Complex = new Dictionary<double, double>();
            foreach (double level in Levels)
            {
                ModalComplex.Levels.Add(level);
                ModalComplex.Mass.Add(mass);
                ModalComplex.Stiffness.Add(stiffness);
                ModalComplex.Frequency.Add(resFrequency);
                ModalComplex.Damping.Add(damping);
                Decrement.Complex.Add(level, decrement);
            }
        }

        public readonly double[] Levels;
        public readonly DecrementData Decrement;
        public readonly ModalParameters ModalGeneral;
        public readonly ModalParameters ModalComplex;
        public readonly double ResonanceFrequencyReal;
        public readonly double ResonanceFrequencyImaginary;
        public readonly double ResonanceFrequencyAmplitude;
        public readonly double ResonanceRealPeak;
        public readonly double ResonanceImaginaryPeak;
        public readonly double ResonanceAmplitudePeak;
        private int mkNumRootFindingIterations = 200;
    }

    public class DecrementData
    {
        public Dictionary<double, double> Imaginary;
        public Dictionary<double, double> Amplitude;
        public double Real;
        public Dictionary<double, double> General;
        public Dictionary<double, double> Complex;
    }
}
