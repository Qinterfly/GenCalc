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
        }
        
        public abstract void plot();
        public abstract bool isDataSet();

        // Fields
        protected WpfPlot mGraph = null;
        // Properties
        public WpfPlot Graph { get { return mGraph; } }
    }
}
