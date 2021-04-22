using System;
using ScottPlot;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    using PairDouble = Tuple<double, double>;

    public abstract class AbstractSignalGraphModel : AbstractGraphModel
    {
        public AbstractSignalGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries)
        {
            mSignal = signal;
            mFrequencyBoundaries = frequencyBoundaries;
            mLevelsBoundaries = levelsBoundaries;
            mGraph.plt.Title(mSignal.Name);
        }

        public override bool isDataSet()
        {
            return mSignal != null && mFrequencyBoundaries != null && mLevelsBoundaries != null;
        }

        protected Response mSignal = null;
        protected PairDouble mFrequencyBoundaries = null;
        protected PairDouble mLevelsBoundaries = null;
    }

    public class ImaginaryPartGraphModel : AbstractSignalGraphModel
    {
        public ImaginaryPartGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.XLabel("Frequencies, Hz");
            mGraph.plt.YLabel("Acceleration, m/s^2");
        }

        public override void plot()
        {
            if (!isDataSet())
                return;
            const int kLineWidth = 2;
            const int kMarkerSize = 5;
            mGraph.plt.Clear();
            // Frequency boundaries
            PairDouble dragFrequencyLimits = mSignal.getFrequencyBoundaries();
            mGraph.plt.PlotHSpan(mFrequencyBoundaries.Item1, mFrequencyBoundaries.Item2, draggable: true, alpha: 0.1,
                                 dragLimitLower: dragFrequencyLimits.Item1, dragLimitUpper: dragFrequencyLimits.Item2);
            // Signal
            mGraph.plt.PlotScatter(mSignal.Frequency, mSignal.ImaginaryPart, lineWidth: kLineWidth, markerSize: kMarkerSize);
            mGraph.Render(lowQuality: false);
        }
    }
}
