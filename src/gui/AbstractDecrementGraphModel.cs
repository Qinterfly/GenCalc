using System.Collections.Generic;
using ScottPlot;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    public class DecrementGraphModel : AbstractGraphModel
    {
        public DecrementGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.Legend(location: legendLocation.upperRight);
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
            double[] X;
            double[] Y;
            dataToVectors(mData.Imaginary, out X, out Y);
            mGraph.plt.PlotScatter(X, Y, lineWidth: kLineWidth, markerSize: kMarkerSize, label: "Imaginary");
            dataToVectors(mData.Amplitude, out X, out Y);
            mGraph.plt.PlotScatter(X, Y, lineWidth: kLineWidth, markerSize: kMarkerSize, label: "Amplitude");
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

        protected DecrementData mData = null;
    }
}
