using ScottPlot;
using GenCalc.Core.Numerical;
using System.Drawing;

namespace GenCalc.Gui.Plot
{
    public class HodographGraphModel : AbstractGraphModel
    {
        public HodographGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.XLabel("Real, m/s^2");
            mGraph.plt.YLabel("Imaginary, m/s^2");
        }

        public void setData(Response signal, double resonanceRealPeak, double resonanceImaginaryPeak)
        {
            mSignal = signal;
            mResonanceRealPeak = resonanceRealPeak;
            mResonanceImaginaryPeak = resonanceImaginaryPeak;
        }

        public override bool isDataSet()
        {
            return mSignal != null;
        }

        public override void plot()
        {
            if (!isDataSet())
                return;
            mGraph.plt.Clear();
            mGraph.plt.PlotScatter(mSignal.RealPart, mSignal.ImaginaryPart, lineWidth: 2, markerSize: 5);
            mGraph.plt.PlotPoint(mResonanceRealPeak, mResonanceImaginaryPeak, color: Color.Red, markerShape: MarkerShape.filledDiamond, markerSize: 10);
            mGraph.Render(lowQuality: false);
        }

        private Response mSignal = null;
        private double mResonanceRealPeak;
        private double mResonanceImaginaryPeak;
    }
}
