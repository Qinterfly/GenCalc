using System;
using System.Collections.Generic;
using MahApps.Metro.Controls;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;
using GenCalc.Gui.Plot;

namespace GenCalc
{
    using PairDouble = Tuple<double, double>;

    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            initializeGraphs();
            setGraphEvents();
        }

        private void initializeGraphs()
        {
            // Input
            mSignalGraphModels = new List<AbstractSignalGraphModel>();
            mSignalGraphModels.Add(new ImaginaryPartGraphModel(graphImaginaryPart));
            mSignalGraphModels.Add(new RealPartGraphModel(graphRealPart));
            mSignalGraphModels.Add(new AmplitudeGraphModel(graphAmplitude));
            mHodographGraphModel = new HodographGraphModel(graphHodograph);
            mMonophaseGraphModel = new MonophaseGraphModel(graphMonophase);
            // Output
            mDecrementGraphModel = new DecrementGraphModel(graphDecrement);
        }

        private void setGraphEvents()
        {
            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
            {
                model.FrequencyBoundariesEvent += frequencyBoundariesChanged;
                model.ResonanceFrequencyEvent += resonanceFrequencyChanged;
            }
                
        }

        public void openProject(string filePath)
        {
            mProject = new LMSProject(filePath);
            if (!mProject.isOpened())
                return;
            textBoxProjectPath.Text = filePath;
        }

        public bool selectAcceleration(string pathSignal = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            mSelectedAcceleration = null;
            Response signal = mProject.retrieveSelectedSignal(pathSignal);
            if (signal != null && signal.Type == ResponseType.kAccel)
                mSelectedAcceleration = signal;
            else
                return false;
            textBoxSelectedAcceleration.Text = mSelectedAcceleration.Name;
            // Set boundaries of levels and frequencies
            PairDouble frequencyBoundaries = mSelectedAcceleration.getFrequencyBoundaries();
            double minFrequency = frequencyBoundaries.Item1;
            double maxFrequency = frequencyBoundaries.Item2;
            // Left frequency correction
            setFrequencyBoundary(numericLeftFrequencyBoundary, minFrequency, maxFrequency);
            numericLeftFrequencyBoundary.Value = minFrequency;
            // Right frequency correction
            setFrequencyBoundary(numericRightFrequencyBoundary, minFrequency, maxFrequency);
            numericRightFrequencyBoundary.Value = maxFrequency;
            // Resonance frequency
            numericResonanceFrequency.IsReadOnly = false;
            numericResonanceFrequency.Value = null;
            numericResonanceFrequency.Maximum = maxFrequency;
            numericResonanceFrequency.Minimum = minFrequency;
            // Decrement by real part
            numericDecrementByReal.Value = null;
            return true;
        }

        public bool selectForce(string pathSignal = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            mSelectedForce = null;
            Response signal = mProject.retrieveSelectedSignal(pathSignal);
            if (signal != null && signal.Type == ResponseType.kForce)
                mSelectedForce = signal;
            else
                return false;
            textBoxSelectedForce.Text = mSelectedForce.Name;
            return true;
        }

        private void setFrequencyBoundary(NumericUpDown numericControl, double minValue, double maxValue)
        {
            if (numericControl.Value != null && numericControl.Value < minValue)
                numericControl.Value = minValue;
            if (numericControl.Value != null && numericControl.Value > maxValue)
                numericControl.Value = maxValue;
            numericControl.Minimum = minValue;
            numericControl.Maximum = maxValue;
        }

        public void calculateCharacteristics()
        {
            PairDouble frequencyBoundaries = new PairDouble((double)numericLeftFrequencyBoundary.Value, (double)numericRightFrequencyBoundary.Value);
            PairDouble levelsBoundaries = new PairDouble((double)numericLeftLevelsBoundary.Value, (double)numericRightLevelsBoundary.Value);
            int numLevels = (int)numericLevelsNumber.Value;
            int numInterpolationPoints = (int)numericInterpolationLength.Value;
            mResponseCharacteristics = new ResponseCharacteristics(mSelectedAcceleration, mSelectedForce, ref frequencyBoundaries, ref levelsBoundaries, numLevels, numInterpolationPoints, numericResonanceFrequency.Value);
            // Set signal data to plot
            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
                model.setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, mResponseCharacteristics.ResonanceFrequency);
            mHodographGraphModel.setData(mSelectedAcceleration, mResponseCharacteristics.ResonanceRealPeak, mResponseCharacteristics.ResonanceImaginaryPeak);
            mMonophaseGraphModel.setData(mSelectedAcceleration);
            // Set results
            mDecrementGraphModel.setData(mResponseCharacteristics.Decrement);
            // Correct input parameters
            numericLeftFrequencyBoundary.Value = frequencyBoundaries.Item1;
            numericRightFrequencyBoundary.Value = frequencyBoundaries.Item2;
            numericLeftLevelsBoundary.Value = levelsBoundaries.Item1;
            numericRightLevelsBoundary.Value = levelsBoundaries.Item2;
            if (mResponseCharacteristics.ResonanceFrequency > 0)
                numericResonanceFrequency.Value = mResponseCharacteristics.ResonanceFrequency;
            if (mResponseCharacteristics.Decrement != null)
            { 
                if (mResponseCharacteristics.Decrement.Real > 0)
                    numericDecrementByReal.Value = mResponseCharacteristics.Decrement.Real;
            }
        }

        public void plotInput()
        {
            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
                model.plot();
            mHodographGraphModel.plot();
            mMonophaseGraphModel.plot();
        }

        public void plotResults()
        {
            mDecrementGraphModel.plot();
        }

        public void calculateAndPlot()
        {
            if (mSelectedAcceleration == null)
                return;
            calculateCharacteristics();
            plotInput();
            plotResults();
        }

        private void clearStatus()
        {
            textBlockStatusBar.Text = "";
        }

        private void setStatus(string info)
        {
            textBlockStatusBar.Text = info;
        }

        private LMSProject mProject = null;
        private Response mSelectedAcceleration = null;
        private Response mSelectedForce = null;
        private ResponseCharacteristics mResponseCharacteristics = null;
        private List<AbstractSignalGraphModel> mSignalGraphModels;
        private DecrementGraphModel mDecrementGraphModel;
        private HodographGraphModel mHodographGraphModel;
        private MonophaseGraphModel mMonophaseGraphModel;
    }
}
