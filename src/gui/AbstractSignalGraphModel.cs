using System;
using ScottPlot;
using ScottPlot.WinForms.Events;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    using PairDouble = Tuple<double, double>;

    public abstract class AbstractSignalGraphModel : AbstractGraphModel
    {
        public delegate void BoundariesEventHandler(PairDouble boundaries);
        public event BoundariesEventHandler FrequencyBoundariesEvent;

        public AbstractSignalGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.MouseUpPlottable += RaiseFrequencyBoundariesEvent;
            mGraph.plt.XLabel("Frequencies, Hz");
            mGraph.plt.YLabel("Acceleration, m/s^2");
        }

        public abstract void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries);

        protected void setBaseData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries)
        {
            mSignal = signal;
            mFrequencyBoundaries = frequencyBoundaries;
            mLevelsBoundaries = levelsBoundaries;
            mGraph.plt.Title(mSignal.Name);
        }

        public override bool isDataSet()
        {
            return mSignal != null 
                   && mFrequencyBoundaries != null && mLevelsBoundaries != null
                   && mYData != null;
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
            mFrequencySpan = mGraph.plt.PlotHSpan(mFrequencyBoundaries.Item1, mFrequencyBoundaries.Item2, draggable: true, alpha: 0.1,
                                                  dragLimitLower: dragFrequencyLimits.Item1, dragLimitUpper: dragFrequencyLimits.Item2);
            // Signal
            mGraph.plt.PlotScatter(mSignal.Frequency, mYData, lineWidth: kLineWidth, markerSize: kMarkerSize);
            mGraph.Render(lowQuality: false);
        }

        public void RaiseFrequencyBoundariesEvent(object sender, PlottableDragEventArgs args)
        {
            if (FrequencyBoundariesEvent != null && mFrequencySpan != null)
            {
                double frequencyLeftPosition = mFrequencySpan.position1;
                double frequencyRightPosition = mFrequencySpan.position2;
                if (mFrequencyBoundaries.Item1 != mFrequencySpan.position1 || mFrequencyBoundaries.Item2 != frequencyRightPosition)
                {
                    if (frequencyLeftPosition > frequencyRightPosition)
                    {
                        double temp = frequencyLeftPosition;
                        frequencyLeftPosition = frequencyRightPosition;
                        frequencyRightPosition = temp;
                    }
                    PairDouble frequencyLimits = mSignal.getFrequencyBoundaries();
                    if (frequencyLeftPosition < frequencyLimits.Item1)
                        frequencyLeftPosition = frequencyLimits.Item1;
                    if (frequencyRightPosition > frequencyLimits.Item2)
                        frequencyRightPosition = frequencyLimits.Item2;
                    FrequencyBoundariesEvent(new PairDouble(frequencyLeftPosition, frequencyRightPosition));
                }
            }
        }

        protected Response mSignal = null;
        protected PairDouble mFrequencyBoundaries = null;
        protected PairDouble mLevelsBoundaries = null;
        protected PlottableHSpan mFrequencySpan = null;
        protected double[] mYData = null;
    }

    public class ImaginaryPartGraphModel : AbstractSignalGraphModel
    {
        public ImaginaryPartGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries);
            mYData = signal.ImaginaryPart;
        }
    }

    public class RealPartGraphModel : AbstractSignalGraphModel
    {
        public RealPartGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries);
            mYData = signal.RealPart;
        }
    }

    public class AmplitudeGraphModel : AbstractSignalGraphModel
    {
        public AmplitudeGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries);
            mYData = signal.Amplitude;
        }
    }
}
