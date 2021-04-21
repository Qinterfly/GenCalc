using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Interpolation;

namespace GenCalc.Core.Numerical
{
    public static class Utilites
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

        public static double[] createUniformMesh(double startValue, double endValue, int numStep)
        {
            double[] mesh = new double[numStep];
            double step = (endValue - startValue) / (numStep - 1);
            for (int i = 0; i != numStep; ++i)
                mesh[i] = startValue + i * step;
            return mesh;
        }
    }
}
