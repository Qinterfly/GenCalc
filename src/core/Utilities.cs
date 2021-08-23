using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Interpolation;

namespace GenCalc.Core.Numerical
{
    using PairDouble = Tuple<double, double>;

    public static class Utilities
    {
        public static double[] interpolate(in double[] XValues, in double[] YValues, in double[] interpMesh)
        {
            int numInterpolationPoints = interpMesh.Length;
            double[] interpYValues = new double[numInterpolationPoints];
            CubicSpline spline = CubicSpline.InterpolateNaturalSorted(XValues, YValues);
            for (int i = 0; i != numInterpolationPoints; ++i)
                interpYValues[i] = spline.Interpolate(interpMesh[i]);
            return interpYValues;
        }

        public static double[] createUniformMesh(double startValue, double endValue, int numPoints)
        {
            double[] mesh = new double[numPoints];
            double step = (endValue - startValue) / (numPoints - 1);
            for (int i = 0; i != numPoints; ++i)
                mesh[i] = startValue + i * step;
            return mesh;
        }

        public static PairDouble findSplineMinMax(CubicSpline spline, double startValue, double endValue, int numPoints)
        {
            double step = (endValue - startValue) / (numPoints - 1);
            double currentFrequency;
            double currentValue;
            double maxValue = Double.MinValue;
            double minValue = Double.MaxValue;
            for (int i = 0; i != numPoints; ++i)
            {
                currentFrequency = startValue + i * step;
                currentValue = spline.Interpolate(currentFrequency);
                if (currentValue > maxValue)
                    maxValue = currentValue;
                if (currentValue < minValue)
                    minValue = currentValue;
            }
            return new PairDouble(minValue, maxValue);
        }

        public static List<double> findAllRootsBisection(Func<double, double> fun, double leftBound, double rightBound, int numSteps)
        {
            List<double> roots = new List<double>();
            double step = (rightBound - leftBound) / (numSteps - 1);
            double previousX = leftBound;
            double previousY = fun(leftBound);
            double currentX, currentY;
            for (int i = 1; i != numSteps; ++i)
            {
                currentX = previousX + step;
                currentY = fun(currentX);
                if (currentY * previousY < 0.0)
                    roots.Add((currentX + previousX) / 2.0);
                if (Math.Abs(currentY) <= double.Epsilon)
                    roots.Add(currentX);
                previousX = currentX;
                previousY = currentY;
            }
            return roots;
        }
    }
}
