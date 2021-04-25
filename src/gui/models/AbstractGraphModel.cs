using ScottPlot;

namespace GenCalc.Gui.Plot
{
    public abstract class AbstractGraphModel
    {
        public AbstractGraphModel(in WpfPlot graph) 
        {
            mGraph = graph;
            mGraph.plt.AntiAlias(true, true, true);
            mGraph.plt.Grid(lineStyle: LineStyle.Dot);
            mGraph.Configure(lowQualityWhileDragging: false, lowQualityOnScrollWheel: false);
            mGraph.plt.XLabel(fontSize: mkFontSize);
            mGraph.plt.YLabel(fontSize: mkFontSize);
            mGraph.plt.Ticks(numericFormatStringX: "G9", numericFormatStringY: "G9");
        }
        
        public abstract void plot();
        public abstract bool isDataSet();

        public void clear()
        {
            mGraph.plt.Clear();
            mGraph.Render();
        }

        // Fields
        protected WpfPlot mGraph = null;
        protected const int mkLineWidth = 2;
        protected const int mkMarkerSize = 5;
        protected const int mkFontSize = 14;
        // Properties
        public WpfPlot Graph { get { return mGraph; } }
    }
}
