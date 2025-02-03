
using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.Interpolation;
using MPFitLib;
using ControlzEx.Standard;

namespace GenCalc.Core.Numerical
{

    public class FitData
    {
        public double[] xData;
        public double[] yData;
        public int numSeries;
    }

    // a0 + a1 * cos(w * t) + b1 * sin(w * t) + ...
    public class FourierSeries : IInterpolation
    {
        private readonly double _a0;
        private readonly double _w;
        private readonly double[] _a;
        private readonly double[] _b;
        private readonly double _sX;

        bool IInterpolation.SupportsDifferentiation => true;
        bool IInterpolation.SupportsIntegration => true;

        public FourierSeries(double[] X, double[] Y, int numSeries, double initFrequency = Math.PI, int maxIter = 5000)
        {
            // Create configuration
            mp_config config = new mp_config();
            config.ftol = 1E-10;
            config.xtol = 1E-10;
            config.gtol = 1E-10;
            config.stepfactor = 100.0;
            config.nprint = 1;
            config.epsfcn = 2.220446E-16;
            config.maxiter = maxIter;
            config.douserscale = 0;
            config.maxfev = 0;
            config.covtol = 1E-14;
            config.nofinitecheck = 0;

            // Prepare the data for computing residuals
            FitData data = new FitData();
            int numData = X.Length;
            data.numSeries = numSeries;
            data.xData = new double[numData];
            data.yData = new double[numData];
            _sX = -X[0];
            for (int i = 0; i != numData; ++i)
            {
                data.xData[i] = X[i] + _sX;
                data.yData[i] = Y[i];
            }

            // Create parameters
            int numParameters = 2 * (1 + numSeries);
            double[] parameters = new double[numParameters];
            parameters[0] = initFrequency;

            // Run the solver
            mp_result result = new mp_result(numParameters);
            int status = MPFit.Solve(computeResiduals, numData, numParameters, parameters, null, config, data, ref result);
            if (status < 1)
                return;

            // Set the resulting data
            _w = parameters[0];
            _a0 = parameters[1];
            _a = parameters.Skip(2).Take(numSeries).ToArray();
            _b = parameters.Skip(2 + numSeries).Take(numSeries).ToArray();
        }

        public bool isValid()
        {
            return _a.Length > 0 && _b.Length > 0 && _w != 0.0;
        }

        // Interpolate at point t
        public double Interpolate(double x)
        {
            x += _sX;
            double result = _a0;
            for (int i = 0; i != _a.Length; ++i)
            {
                int k = i + 1;
                double t = k * _w * x;
                result += _a[i] * Math.Cos(t);
                result += _b[i] * Math.Sin(t);
            }
            return result;
        }

        // Differentiate at point t
        public double Differentiate(double x)
        {
            x += _sX;
            double result = 0;
            for (int i = 0; i != _a.Length; ++i)
            {
                int k = i + 1;
                double A = k * _w;
                double t = k * _w * x;
                result += -_a[i] * A * Math.Sin(t);
                result += _b[i] * A * Math.Cos(t);
            }
            return result;
        }

        // Differentiate twice at point t
        public double Differentiate2(double x)
        {
            x += _sX;
            double result = 0;
            for (int i = 0; i != _a.Length; ++i)
            {
                int k = i + 1;
                double A = Math.Pow(k * _w, 2.0);
                double t = k * _w * x;
                result += -_a[i] * A * Math.Cos(t);
                result += -_b[i] * A * Math.Sin(t);
            }
            return result;
        }

        // Indefinite integral at point t
        public double Integrate(double x)
        {
            x += _sX;
            double result = _a0 * x;
            for (int i = 0; i != _a.Length; ++i)
            {
                int k = i + 1;
                double A = 1 / (k * _w);
                double t = k * _w * x;
                result += _a[i] * A * Math.Sin(t);
                result += -_b[i] * A * Math.Cos(t);
            }
            return result;
        }

        // Definite integral between points a and b
        public double Integrate(double a, double b)
        {
            return Integrate(b) - Integrate(a);
        }

        public static int computeResiduals(double[] p, double[] dy, IList<double>[] dvec, object vars)
        {
            // Slice variables
            var v = (FitData)vars;
            int numSeries = v.numSeries;
            double[] xData = v.xData;
            double[] yData = v.yData;

            // Slice parameters
            double w = p[0];
            double a0 = p[1];
            var a = p.Skip(2).Take(numSeries).ToArray();
            var b = p.Skip(2 + numSeries).Take(numSeries).ToArray();

            // Evaluate residuals
            int numData = xData.Length;
            for (int i = 0; i != numData; ++i)
            {
                double x = xData[i];
                double f = a0;
                for (int j = 0; j != numSeries; ++j)
                {
                    int k = j + 1;
                    double t = k * w * x;
                    f += a[j] * Math.Cos(t);
                    f += b[j] * Math.Sin(t);
                }
                dy[i] = f - yData[i];
            }

            return 0;
        }
    };
}

