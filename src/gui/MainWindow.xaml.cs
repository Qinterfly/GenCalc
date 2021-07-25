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
            initializeData();
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
            mModalGraphModels = new List<ModalGraphModel>();
            mModalGraphModels.Add(new ModalMassGraphModel(graphModalMass));
            mModalGraphModels.Add(new ModalStiffnessGraphModel(graphModalStiffness));
            mModalGraphModels.Add(new ModalDampingGraphModel(graphModalDamping));
            mModalGraphModels.Add(new ModalFrequencyGraphModel(graphModalFrequency));
        }

        private void initializeData()
        {
            mSelectedModalSet = new ModalDataSet();
        }

        private void setGraphEvents()
        {
            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
            {
                model.FrequencyBoundariesEvent += frequencyBoundariesChanged;
                model.PointSelectionChangedEvent += pointSelectionChanged;
            }
            mSignalGraphModels[0].ResonanceFrequencyEvent += resonanceFrequencyImaginaryChanged;
            mSignalGraphModels[1].ResonanceFrequencyEvent += resonanceFrequencyRealChanged;
            mSignalGraphModels[2].ResonanceFrequencyEvent += resonanceFrequencyAmplitudeChanged;
            mHodographGraphModel.PointSelectionChangedEvent += pointSelectionChanged;
            mMonophaseGraphModel.PointSelectionChangedEvent += pointSelectionChanged;
            mDecrementGraphModel.PointSelectionChangedEvent += pointSelectionChanged;
            foreach (AbstractGraphModel model in mModalGraphModels)
                model.PointSelectionChangedEvent += pointSelectionChanged;
        }

        public void openProject(string filePath)
        {
            if (mProject != null && mProject.isOpened())
                clearAll();
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
            {
                mSelectedAcceleration = signal;
            }
            else
            {
                textBoxSelectedAcceleration.Text = "";
                return false;
            }
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
            enableNumericControl(numericResonanceFrequencyReal,      minFrequency, maxFrequency);
            enableNumericControl(numericResonanceFrequencyImaginary, minFrequency, maxFrequency);
            enableNumericControl(numericResonanceFrequencyAmplitude, minFrequency, maxFrequency);
            // Decrement by real part
            numericDecrementByReal.Value = null;
            return true;
        }

        public bool selectForces(List<string> listPathForces = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            listBoxForces.Items.Clear();
            mSelectedModalSet.Forces = new List<Response>();
            List<Response> selectedSignals = mProject.retrieveSelectedSignals(listPathForces);
            foreach (Response signal in selectedSignals)
            {
                if (signal.Type == ResponseType.kForce)
                {
                    mSelectedModalSet.Forces.Add(signal);
                    listBoxForces.Items.Add(signal.Name);
                }
            }
            if (mSelectedModalSet.Forces.Count == 0)
                mSelectedModalSet.Forces = null;
            return mSelectedModalSet.Forces != null;
        }

        public bool selectResponses(List<string> listPathResponses = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            listBoxResponses.Items.Clear();
            mSelectedModalSet.Responses = new List<Response>();
            List<Response> selectedSignals = mProject.retrieveSelectedSignals(listPathResponses);
            foreach (Response signal in selectedSignals)
            {
                if (signal.Type == ResponseType.kAccel)
                {
                    mSelectedModalSet.Responses.Add(signal);
                    listBoxResponses.Items.Add(signal.Name);
                }
            }
            if (mSelectedModalSet.Responses.Count == 0)
                mSelectedModalSet.Responses = null;
            return mSelectedModalSet.Responses != null;
        }

        public bool selectReferenceResponse(string pathReferenceResponse = null)
        {
            if (mProject == null || !mProject.isOpened())
                return false;
            mSelectedModalSet.ReferenceResponse = null;
            Response referenceResponse = mProject.retrieveSelectedSignal(pathReferenceResponse);
            if (referenceResponse != null)
            {
                mSelectedModalSet.ReferenceResponse = referenceResponse;
                textBoxReferenceResponse.Text = referenceResponse.Name;
                return true;
            }
            else
            {
                textBoxReferenceResponse.Text = "";
                return false;
            }
        }

        private void enableNumericControl(NumericUpDown numericControl, double minValue, double maxValue)
        {
            numericControl.IsReadOnly = false;
            numericControl.Value = null;
            numericControl.Minimum = minValue;
            numericControl.Maximum = maxValue;
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
            if (mSelectedModalSet != null)
            {
                if (mSelectedModalSet.Forces != null && mSelectedModalSet.Responses != null && mSelectedModalSet.ReferenceResponse == null)
                    mSelectedModalSet.ReferenceResponse = mSelectedAcceleration;
            }
            ResponseCharacteristics characteristics = new ResponseCharacteristics(mSelectedAcceleration, ref frequencyBoundaries, ref levelsBoundaries,
                                                                                  numLevels, numInterpolationPoints,
                                                                                  numericResonanceFrequencyReal.Value, numericResonanceFrequencyImaginary.Value, numericResonanceFrequencyAmplitude.Value,
                                                                                  mSelectedModalSet);
            // Set signal data to plot
            mSignalGraphModels[0].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, characteristics.ResonanceFrequencyImaginary);
            mSignalGraphModels[1].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, characteristics.ResonanceFrequencyReal);
            mSignalGraphModels[2].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, characteristics.ResonanceFrequencyAmplitude);
            mHodographGraphModel.setData(mSelectedAcceleration, characteristics.ResonanceRealPeak, characteristics.ResonanceImaginaryPeak);
            mMonophaseGraphModel.setData(mSelectedAcceleration);
            // Set results
            mDecrementGraphModel.setData(characteristics.Decrement);
            ModalParameters modalResults = characteristics.Modal;
            if (modalResults != null)
            {
                foreach (ModalGraphModel model in mModalGraphModels)
                    model.setData(modalResults);
            }
            // Correct input parameters
            numericLeftFrequencyBoundary.Value = frequencyBoundaries.Item1;
            numericRightFrequencyBoundary.Value = frequencyBoundaries.Item2;
            numericLeftLevelsBoundary.Value = levelsBoundaries.Item1;
            numericRightLevelsBoundary.Value = levelsBoundaries.Item2;
            if (characteristics.ResonanceFrequencyReal > 0)
                numericResonanceFrequencyReal.Value = characteristics.ResonanceFrequencyReal;
            if (characteristics.ResonanceFrequencyImaginary > 0)
                numericResonanceFrequencyImaginary.Value = characteristics.ResonanceFrequencyImaginary;
            if (characteristics.ResonanceFrequencyAmplitude > 0)
                numericResonanceFrequencyAmplitude.Value = characteristics.ResonanceFrequencyAmplitude;
            numericDecrementByReal.Value = null;
            if (characteristics.Decrement != null)
            { 
                if (characteristics.Decrement.Real > 0)
                    numericDecrementByReal.Value = characteristics.Decrement.Real;
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
            foreach (ModalGraphModel model in mModalGraphModels)
                model.plot();
        }

        public void calculateAndPlot()
        {
            if (mSelectedAcceleration == null)
                return;
            calculateCharacteristics();
            plotInput();
            plotResults();
        }

        public void clearAll()
        {
            // Fields
            mProject = null;
            mSelectedAcceleration = null;
            mSelectedModalSet = null;
            // Project
            textBoxProjectPath.Text = "";
            textBoxSelectedAcceleration.Text = "";
            // Modal data
            listBoxForces.Items.Clear();
            listBoxResponses.Items.Clear();
            textBoxReferenceResponse.Text = "";
            // Calculation pararmeters
            numericLeftFrequencyBoundary.Value = null;
            numericRightFrequencyBoundary.Value = null;
            // Output
            numericDecrementByReal.Value = null;
            numericResonanceFrequencyReal.Value = null;
            numericResonanceFrequencyImaginary.Value = null;
            numericResonanceFrequencyAmplitude.Value = null;
            // Graphs
            clearInput();
            clearResults();
        }

        public void clearInput()
        {

            foreach (AbstractSignalGraphModel model in mSignalGraphModels)
                model.clear();
            mHodographGraphModel.clear();
            mMonophaseGraphModel.clear();
        }

        public void clearResults()
        {

            mDecrementGraphModel.clear();
            foreach (ModalGraphModel model in mModalGraphModels)
                model.clear();
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
        private ModalDataSet mSelectedModalSet = null;
        // Input models
        private List<AbstractSignalGraphModel> mSignalGraphModels;
        private HodographGraphModel mHodographGraphModel;
        private MonophaseGraphModel mMonophaseGraphModel;
        // Output models
        private DecrementGraphModel mDecrementGraphModel;
        private List<ModalGraphModel> mModalGraphModels;
    }
}
