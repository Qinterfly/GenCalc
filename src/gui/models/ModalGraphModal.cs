using System.Collections.Generic;
using ScottPlot;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    using ModalResults = Dictionary<string, ModalParameters>;

    abstract public class ModalGraphModel : AbstractGraphModel
    {
        public ModalGraphModel(in WpfPlot graph) : base(graph)
        {
            mGraph.plt.XLabel("Level");
            mGraph.plt.Legend(location: legendLocation.upperLeft);
        }

        public void setData(in ModalResults data)
        {
            mData = data;
        }

        public override bool isDataSet()
        {
            return mData != null && mData.Count > 0;
        }

        public bool initialize()
        {
            mGraph.plt.Clear();
            if (!isDataSet())
                return false;
            return true;
        }

        public void plot(string field)
        {
            if (!initialize())
                return;
            foreach (string key in mData.Keys)
            {
                ModalParameters item = mData[key];
                if (item == null)
                    continue;
                List<double> YData = item.GetType().GetField(field).GetValue(item) as List<double>;
                mGraph.plt.PlotScatterHighlight(item.Levels.ToArray(), YData.ToArray(), lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: key);
            }
            mGraph.Render(lowQuality: false);
        }

        protected ModalResults mData = null;
    }

    public class ModalMassGraphModel : ModalGraphModel
    {
        public ModalMassGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal mass, kg");
        }

        public override void plot()
        {
            plot("Mass");
        }
    }

    public class ModalStiffnessGraphModel : ModalGraphModel
    {
        public ModalStiffnessGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal stiffness, N/m");
        }

        public override void plot()
        {
            plot("Stiffness");
        }
    }

    public class ModalDampingGraphModel : ModalGraphModel
    {
        public ModalDampingGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal damping, kg/c^2");
        }

        public override void plot()
        {
            plot("Damping");
        }
    }

    public class ModalFrequencyGraphModel : ModalGraphModel
    {
        public ModalFrequencyGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal frequency, Hz");
        }

        public override void plot()
        {
            plot("Frequency");
        }
    }


}
