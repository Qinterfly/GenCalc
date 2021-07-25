using System;
using ScottPlot;
using GenCalc.Core.Numerical;
using System.Drawing;
using System.Collections.Generic;

namespace GenCalc.Gui.Plot
{
    public class MonophaseGraphModel : AbstractGraphModel
    {
        public MonophaseGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.XLabel("Frequency");
            mGraph.plt.YLabel("Real/Imaginary");
            mGraph.plt.Legend(location: legendLocation.lowerRight);
            mFrequency = new Dictionary<string, double[]>();
            mMonophaseParameter = new Dictionary<string, double[]>();
        }

        public void setData(List<Response> responses)
        {
            mFrequency.Clear();
            mMonophaseParameter.Clear();
            if (responses == null)
                return;
            foreach (Response signal in responses)
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
                double[] resFrequency = new double[numMonophase];
                double[] resMonophaseParameter = new double[numMonophase];
                numMonophase = 0;
                for (int i = 0; i != numSignal; ++i)
                {
                    if (Math.Abs(imaginaryPart[i]) > Double.MinValue)
                    {
                        resFrequency[numMonophase] = frequency[i];
                        resMonophaseParameter[numMonophase] = realPart[i] / imaginaryPart[i];
                        ++numMonophase;
                    }
                }
                if (numMonophase > 0)
                {
                    mFrequency.Add(signal.Name, resFrequency);
                    mMonophaseParameter.Add(signal.Name, resMonophaseParameter);
                }
            }
        }

        public override bool isDataSet()
        {
            return mFrequency.Count > 0 && mMonophaseParameter.Count > 0;
        }

        public override void plot()
        {
            mGraph.plt.Clear();
            if (!isDataSet())
                return;
            foreach (string signalName in mFrequency.Keys)
                mGraph.plt.PlotScatterHighlight(mFrequency[signalName], mMonophaseParameter[signalName], lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: signalName);
            mGraph.Render(lowQuality: false);
        }

        private Dictionary<string, double[]> mFrequency;
        private Dictionary<string, double[]> mMonophaseParameter;
    }
}
