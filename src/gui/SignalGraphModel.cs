using System;
using System.Drawing;
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
        public event Action<double> ResonanceFrequencyEvent;

        public AbstractSignalGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.MouseUpPlottable += RaiseFrequencyBoundariesEvent;
            graph.MouseUpPlottable += RaiseResonanceFrequencyEvent;
            mGraph.plt.XLabel("Frequencies, Hz");
            mGraph.plt.YLabel("Acceleration, m/s^2");
        }

        public abstract void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries, double resonanceFrequency);

        protected void setBaseData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries, double resonanceFrequency)
        {
            mSignal = signal;
            mFrequencyBoundaries = frequencyBoundaries;
            mLevelsBoundaries = levelsBoundaries;
            mResonanceFrequency = resonanceFrequency;
        }

        public override bool isDataSet()
        {
            return mSignal != null 
                   && mFrequencyBoundaries != null && mLevelsBoundaries != null
                   && mYData != null && mResonanceFrequency > 0;
        }

        public override void plot()
        {
            if (!isDataSet())
                return;
            mGraph.plt.Clear();
            // Frequency boundaries
            PairDouble dragFrequencyLimits = mSignal.getFrequencyBoundaries();
            mFrequencyBoundariesSpan = mGraph.plt.PlotHSpan(mFrequencyBoundaries.Item1, mFrequencyBoundaries.Item2, draggable: true, alpha: 0.1,
                                                            dragLimitLower: dragFrequencyLimits.Item1, dragLimitUpper: dragFrequencyLimits.Item2);
            // Resonance frequency
            mResonanceFrequencyLine = mGraph.plt.PlotVLine(mResonanceFrequency, draggable: true, lineStyle: LineStyle.Dot, color: Color.Black, lineWidth: 1,
                                                           dragLimitLower: dragFrequencyLimits.Item1, dragLimitUpper: dragFrequencyLimits.Item2);
            // Signal
            mGraph.plt.PlotScatter(mSignal.Frequency, mYData, lineWidth: 2, markerSize: 5);
            mGraph.Render(lowQuality: false);
        }

        public void RaiseFrequencyBoundariesEvent(object sender, PlottableDragEventArgs args)
        {
            if (FrequencyBoundariesEvent != null && mFrequencyBoundariesSpan != null)
            {
                double frequencyLeftPosition = mFrequencyBoundariesSpan.position1;
                double frequencyRightPosition = mFrequencyBoundariesSpan.position2;
                if (mFrequencyBoundaries.Item1 != mFrequencyBoundariesSpan.position1 || mFrequencyBoundaries.Item2 != frequencyRightPosition)
                {
                    if (frequencyLeftPosition > frequencyRightPosition)
                    {
                        double temp = frequencyLeftPosition;
                        frequencyLeftPosition = frequencyRightPosition;
                        frequencyRightPosition = temp;
                    }
                    FrequencyBoundariesEvent(new PairDouble(frequencyLeftPosition, frequencyRightPosition));
                }
            }
        }

        public void RaiseResonanceFrequencyEvent(object sender, PlottableDragEventArgs args)
        {
            if (FrequencyBoundariesEvent != null && mResonanceFrequencyLine != null)
            {
                if (mResonanceFrequency != mResonanceFrequencyLine.position)
                    ResonanceFrequencyEvent(mResonanceFrequencyLine.position);
            }
        }

        protected Response mSignal = null;
        protected PairDouble mFrequencyBoundaries = null;
        protected PairDouble mLevelsBoundaries = null;
        protected double mResonanceFrequency = -1.0;
        protected PlottableVLine mResonanceFrequencyLine = null;
        protected PlottableHSpan mFrequencyBoundariesSpan = null;
        protected double[] mYData = null;
    }

    public class ImaginaryPartGraphModel : AbstractSignalGraphModel
    {
        public ImaginaryPartGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries, double resonanceFrequency)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries, resonanceFrequency);
            mYData = signal.ImaginaryPart;
        }
    }

    public class RealPartGraphModel : AbstractSignalGraphModel
    {
        public RealPartGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries, double resonanceFrequency)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries, resonanceFrequency);
            mYData = signal.RealPart;
        }
    }

    public class AmplitudeGraphModel : AbstractSignalGraphModel
    {
        public AmplitudeGraphModel(in WpfPlot graph) : base(graph)
        {

        }

        public override void setData(in Response signal, in PairDouble frequencyBoundaries, in PairDouble levelsBoundaries, double resonanceFrequency)
        {
            setBaseData(signal, frequencyBoundaries, levelsBoundaries, resonanceFrequency);
            mYData = signal.Amplitude;
        }
    }
}
