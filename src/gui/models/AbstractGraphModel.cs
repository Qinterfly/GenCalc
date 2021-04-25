using ScottPlot;

namespace GenCalc.Gui.Plot
{
    public abstract class AbstractGraphModel
    {
        public AbstractGraphModel(in WpfPlot graph) 
        {
            const int kFontSize = 14;
            mGraph = graph;
            mGraph.plt.AntiAlias(true, true, true);
            mGraph.plt.Grid(lineStyle: LineStyle.Dot);
            mGraph.Configure(lowQualityWhileDragging: false, lowQualityOnScrollWheel: false);
            mGraph.plt.XLabel(fontSize: kFontSize);
            mGraph.plt.YLabel(fontSize: kFontSize);
            mGraph.plt.Ticks(numericFormatStringX: "G9", numericFormatStringY: "G9");
        }
        
        public abstract void plot();
        public abstract bool isDataSet();

        // Fields
        protected WpfPlot mGraph = null;
        protected const int mkLineWidth = 2;
        protected const int mkMarkerSize = 5;
        // Properties
        public WpfPlot Graph { get { return mGraph; } }
    }
}
