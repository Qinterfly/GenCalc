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
            mGraph.plt.Clear();
            if (!isDataSet())
                return;
            // Imaginary 
            dataToVectors(mData.Imaginary, out double[] XImag, out double[] YImag);
            if (XImag.Length > 1)
                mGraph.plt.PlotScatterHighlight(XImag, YImag, lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: "Imaginary");
            // Amplitude
            dataToVectors(mData.Amplitude, out double[] XAmp, out double[] YAmp);
            if (XAmp.Length > 1)
                mGraph.plt.PlotScatterHighlight(XAmp, YAmp, lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: "Amplitude");
            // General characteristics
            if (mData.General != null && mData.General.Count > 0)
            {
                dataToVectors(mData.General, out double[] XGen, out double[] YGen);
                mGraph.plt.PlotScatterHighlight(XGen, YGen, lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: "General");
            }
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
