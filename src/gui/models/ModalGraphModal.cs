using System.Collections.Generic;
using ScottPlot;
using GenCalc.Core.Numerical;

namespace GenCalc.Gui.Plot
{
    abstract public class ModalGraphModel : AbstractGraphModel
    {
        public ModalGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.XLabel("Level");
        }

        public abstract void setData(in ModalParameters data);

        public override bool isDataSet()
        {
            return mData != null && mYData != null;
        }

        public override void plot()
        {
            mGraph.plt.Clear();
            if (!isDataSet())
                return;
            if (mYData.Count > 1)
                mGraph.plt.PlotScatterHighlight(mData.Levels.ToArray(), mYData.ToArray(), lineWidth: mkLineWidth, markerSize: mkMarkerSize, label: "Imaginary");
            mGraph.Render(lowQuality: false);
        }

        protected ModalParameters mData = null;
        protected List<double> mYData = null;
    }

    public class ModalMassGraphModel : ModalGraphModel
    {
        public ModalMassGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal mass, kg");
        }

        public override void setData(in ModalParameters data)
        {
            mData = data;
            mYData = data.Mass;
        }
    }

    public class ModalStiffnessGraphModel : ModalGraphModel
    {
        public ModalStiffnessGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal stiffness, N/m");
        }

        public override void setData(in ModalParameters data)
        {
            mData = data;
            mYData = data.Stiffness;
        }
    }

    public class ModalDampingGraphModel : ModalGraphModel
    {
        public ModalDampingGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal damping, kg/c^2");
        }

        public override void setData(in ModalParameters data)
        {
            mData = data;
            mYData = data.Damping;
        }
    }

    public class ModalFrequencyGraphModel : ModalGraphModel
    {
        public ModalFrequencyGraphModel(in WpfPlot graph) : base(graph)
        {
            graph.plt.YLabel("Modal frequency, Hz");
        }

        public override void setData(in ModalParameters data)
        {
            mData = data;
            mYData = data.Frequency;
        }
    }


}
