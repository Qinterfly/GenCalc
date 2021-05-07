using System;
using ScottPlot;
using GenCalc.Core.Numerical;
using System.Drawing;

namespace GenCalc.Gui.Plot
{
    public class MonophaseGraphModel : AbstractGraphModel
    {
        public MonophaseGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.XLabel("Frequency");
            mGraph.plt.YLabel("Real/Imaginary");
        }

        public void setData(Response signal)
        {
            double[] frequency = signal.Frequency;
            double[] realPart = signal.RealPart;
            double[] imaginaryPart = signal.ImaginaryPart;
            int numSignal = realPart.Length;
            int numMonophase = 0;
            for (int i = 0; i != numSignal; ++i)
            {
                if (Math.Abs(imaginaryPart[i]) > Double.MinValue)
                    ++numMonophase;
            }
            mFrequency = new double[numMonophase];
            mMonophaseParameter = new double[numMonophase];
            numMonophase = 0;
            for (int i = 0; i != numSignal; ++i)
            {
                if (Math.Abs(imaginaryPart[i]) > Double.MinValue)
                {
                    mFrequency[numMonophase] = frequency[i];
                    mMonophaseParameter[numMonophase] = realPart[i] / imaginaryPart[i];
                    ++numMonophase;
                }
            }
        }

        public override bool isDataSet()
        {
            return mFrequency != null && mMonophaseParameter != null
                   && mFrequency.Length > 1 && mMonophaseParameter.Length > 1;
        }

        public override void plot()
        {
            mGraph.plt.Clear();
            if (!isDataSet())
                return;
            mGraph.plt.PlotScatterHighlight(mFrequency, mMonophaseParameter, lineWidth: mkLineWidth, markerSize: mkMarkerSize);
            mGraph.Render(lowQuality: false);
        }

        private double[] mFrequency = null;
        private double[] mMonophaseParameter = null;
    }
}
