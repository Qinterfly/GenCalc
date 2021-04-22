using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Win32;
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
        }

        private void initializeGraphs()
        {
            mSignalGraphModels = new List<AbstractSignalGraphModel>();
            mSignalGraphModels.Add(new ImaginaryPartGraphModel(graphImaginaryPart));
            mDecrementGraphModel = new DecrementGraphModel(graphDecrement);
        }

        public void openProject(string filePath)
        {
            mProject = new LMSProject(filePath);
            if (!mProject.isOpened())
                return;
            textBoxProjectPath.Text = filePath;
        }

        public bool selectSignal(string pathSignal = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            mSelectedSignal = mProject.retrieveSelectedSignal(pathSignal);
            if (mSelectedSignal == null)
                return false;
            textBoxSelectedSignal.Text = mSelectedSignal.Name;
            // Set boundaries of levels and frequencies
            PairDouble frequencyBoundaries = mSelectedSignal.getFrequencyBoundaries();
            double minFrequency = frequencyBoundaries.Item1;
            double maxFrequency = frequencyBoundaries.Item2;
            // Left frequency correction
            setFrequencyBoundary(numericLeftFrequencyBoundary, minFrequency, maxFrequency);
            if (numericLeftFrequencyBoundary.Value == null)
                numericLeftFrequencyBoundary.Value = minFrequency;
            // Right frequency correction
            setFrequencyBoundary(numericRightFrequencyBoundary, minFrequency, maxFrequency);
            if (numericRightFrequencyBoundary.Value == null)
                numericRightFrequencyBoundary.Value = maxFrequency;
            numericResonanceFrequency.Value = null;
            return true;
        }

        private void buttonOpenProject_Click(object sender, RoutedEventArgs e)
        {
            clearStatus();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "LMS Test.Lab 15A Project (*.lms)|*.lms";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                openProject(openFileDialog.FileName);
                setStatus("The project was successfuly opened");
            }
            else
            {
                setStatus("An error occured while choosing a project file");
                return;
            }
        }

        private void buttonSelectSignal_Click(object sender, RoutedEventArgs e)
        {
            if (selectSignal())
            {
                calculateAndPlot();
                setStatus("The signal was selected via TestLab");
            }
            else
            {
                setStatus("An error occured while selecting the signal");
            }
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
            mResponseCharacteristics = new ResponseCharacteristics(mSelectedSignal, ref frequencyBoundaries, ref levelsBoundaries, numLevels, numInterpolationPoints);
            // Set signal data to plot
            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
                model.setData(mSelectedSignal, frequencyBoundaries, levelsBoundaries);
            // Set results
            mDecrementGraphModel.setData(mResponseCharacteristics.Decrement);
            // Correct input parameters
            numericLeftFrequencyBoundary.Value = frequencyBoundaries.Item1;
            numericRightFrequencyBoundary.Value = frequencyBoundaries.Item2;
            numericLeftLevelsBoundary.Value = levelsBoundaries.Item1;
            numericRightLevelsBoundary.Value = levelsBoundaries.Item2;
            if (numericResonanceFrequency.Value == null)
                numericResonanceFrequency.Value = mResponseCharacteristics.ResonanceFrequency;
        }

        public void plotSignals()
        {
            foreach (AbstractGraphModel model in mSignalGraphModels)
                model.plot();
        }

        public void plotResults()
        {
            mDecrementGraphModel.plot();
        }

        public void calculateAndPlot(object sender = null, RoutedPropertyChangedEventArgs<double?> e = null)
        {
            if (mSelectedSignal == null)
                return;
            calculateCharacteristics();
            plotSignals();
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
        private Response mSelectedSignal = null;
        private ResponseCharacteristics mResponseCharacteristics = null;
        private List<AbstractSignalGraphModel> mSignalGraphModels;
        private DecrementGraphModel mDecrementGraphModel;
    }
}
