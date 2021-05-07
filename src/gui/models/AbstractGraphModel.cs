using System;
using System.Windows.Input;
using System.Collections.Generic;
using ScottPlot;

namespace GenCalc.Gui.Plot
{
    using PairDouble = Tuple<double, double>;

    public abstract class AbstractGraphModel
    {
        public delegate void PointSelectionChangedHandler(PairDouble coordinates);
        public event PointSelectionChangedHandler PointSelectionChangedEvent;

        public AbstractGraphModel(in WpfPlot graph) 
        {
            mGraph = graph;
            mGraph.plt.AntiAlias(true, true, true);
            mGraph.plt.Grid(lineStyle: LineStyle.Dot);
            mGraph.Configure(lowQualityWhileDragging: false, lowQualityOnScrollWheel: false);
            mGraph.plt.XLabel(fontSize: mkFontSize);
            mGraph.plt.YLabel(fontSize: mkFontSize);
            mGraph.plt.Ticks(numericFormatStringX: "G9", numericFormatStringY: "G9");
            graph.MouseDown += selectPoint;
        }
        
        public abstract void plot();
        public abstract bool isDataSet();

        public void clear()
        {
            mGraph.plt.Clear();
            mGraph.Render();
        }

        private void selectPoint(object sender, MouseButtonEventArgs args)
        {
            bool isCtrlPress = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (args.LeftButton != MouseButtonState.Pressed || !isCtrlPress)
                return;
            (double coordinateX, double coordinateY) = mGraph.GetMouseCoordinates();
            List<Plottable> listPlottables = mGraph.plt.GetPlottables();
            double minGraphDistance = Double.MaxValue;
            int numTables = listPlottables.Count;
            int iNearestTable = -1;
            int iNearestPoint = 0;
            for (int iTable = 0; iTable != numTables; ++iTable)
            {
                Plottable plottable = listPlottables[iTable];
                bool isScatter = plottable.GetType().Name.Equals("PlottableScatterHighlight");
                // Check every scatter plot and find out which is the closest one
                if (isScatter)
                {
                    PlottableScatterHighlight scatterPlot = (PlottableScatterHighlight)plottable;
                    scatterPlot.HighlightClear();
                    if (scatterPlot.xs.Length < 2)
                        continue;
                    (double nearestCoordinateX, double nearestCoordinateY, int index) = scatterPlot.GetPointNearest(coordinateX, coordinateY);
                    double maxDistance = getMaxDistance(scatterPlot.xs, scatterPlot.ys);
                    if (maxDistance <= Double.Epsilon)
                        continue;
                    double currentDistance = Math.Sqrt(Math.Pow((nearestCoordinateX - coordinateX), 2.0) + Math.Pow((nearestCoordinateY - coordinateY), 2.0));
                    if (currentDistance <= maxDistance && currentDistance < minGraphDistance)
                    {
                        iNearestTable = iTable;
                        iNearestPoint = index;
                        minGraphDistance = currentDistance;
                    }
                }
            }
            if (iNearestTable >= 0)
            {
                PlottableScatterHighlight scatterPlot = (PlottableScatterHighlight)listPlottables[iNearestTable];
                (double X, double Y, int i) = scatterPlot.HighlightPoint(iNearestPoint);
                mGraph.Render(lowQuality: false);
                PointSelectionChangedEvent(new PairDouble(X, Y));
            }
        }

        private double getMaxDistance(double[] x, double[] y)
        {
            int numPoints = x.Length;
            double distance;
            double maxDistance = Double.MinValue;
            for (int i = 0; i != numPoints - 1; ++i)
            {
                distance = Math.Pow((x[i + 1] - x[i]), 2.0) + Math.Pow((y[i + 1] - y[i]), 2.0);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return Math.Sqrt(maxDistance);
        }

        public void clearPointSelection()
        {
            List<Plottable> listPlottables = mGraph.plt.GetPlottables();
            foreach (Plottable plottable in listPlottables)
            {
                bool isScatter = plottable.GetType().Name.Equals("PlottableScatterHighlight");
                if (isScatter)
                {
                    PlottableScatterHighlight scatterPlot = (PlottableScatterHighlight)plottable;
                    scatterPlot.HighlightClear();
                }
            }
            mGraph.Render(lowQuality: false);
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
