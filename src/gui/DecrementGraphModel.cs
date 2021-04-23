using System.Collections.Generic;
using ScottPlot;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    public class DecrementGraphModel : AbstractGraphModel
    {
        public DecrementGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.Legend(location: legendLocation.upperLeft);
            mGraph.plt.XLabel("Level");
            mGraph.plt.YLabel("Logarithmic Decrement");
        }

        public void setData(DecrementData data)
        {
            mData = data;
        }

        public override bool isDataSet()
        {
            return mData != null;
        }

        public override void plot()
        {
            if (!isDataSet())
                return;
            const int kLineWidth = 2;
            const int kMarkerSize = 5;
            mGraph.plt.Clear();
            dataToVectors(mData.Imaginary, out double[] XImag, out double[] YImag);
            if (XImag.Length > 0 && YImag.Length > 0)
                mGraph.plt.PlotScatter(XImag, YImag, lineWidth: kLineWidth, markerSize: kMarkerSize, label: "Imaginary");
            dataToVectors(mData.Amplitude, out double[] XAmp, out double[] YAmp);
            if (XAmp.Length > 0 && YAmp.Length > 0)
                mGraph.plt.PlotScatter(XAmp, YAmp, lineWidth: kLineWidth, markerSize: kMarkerSize, label: "Amplitude");
            mGraph.Render(lowQuality: false);
        }

        private void dataToVectors(in Dictionary<double, double> data, out double[] X, out double[] Y)
        {
            int numData = data.Count;
            X = new double[numData];
            Y = new double[numData];
            int i = 0;
            foreach (double key in data.Keys)
            {
                X[i] = key;
                Y[i] = data[key];
                ++i;
            }
        }

        private DecrementData mData = null;
    }
}
