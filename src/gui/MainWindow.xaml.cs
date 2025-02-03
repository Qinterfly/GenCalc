using System;
using System.Collections.Generic;
using System.Windows.Input;
using MahApps.Metro.Controls;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;
using GenCalc.Gui.Plot;

namespace GenCalc
{
    using PairDouble = Tuple<double, double>;
    using ModalResults = Dictionary<string, ModalParameters>;

    public enum SignalModelType
    {
        kImaginary,
        kReal,
        kAmplitude
    }

    public enum ModalResultType
    {
        kGeneral,
        kComplex
    }

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
            mSignalGraphModels = new Dictionary<SignalModelType, AbstractSignalGraphModel>();
            mSignalGraphModels.Add(SignalModelType.kImaginary, new ImaginaryPartGraphModel(graphImaginaryPart));
            mSignalGraphModels.Add(SignalModelType.kReal, new RealPartGraphModel(graphRealPart));
            mSignalGraphModels.Add(SignalModelType.kAmplitude, new AmplitudeGraphModel(graphAmplitude));
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
            foreach (AbstractSignalGraphModel model in mSignalGraphModels.Values)
            {
                model.FrequencyBoundariesEvent += frequencyBoundariesChanged;
                model.PointSelectionChangedEvent += pointSelectionChanged;
            }
            mSignalGraphModels[SignalModelType.kImaginary].ResonanceFrequencyEvent += resonanceFrequencyImaginaryChanged;
            mSignalGraphModels[SignalModelType.kReal].ResonanceFrequencyEvent += resonanceFrequencyRealChanged;
            mSignalGraphModels[SignalModelType.kAmplitude].ResonanceFrequencyEvent += resonanceFrequencyAmplitudeChanged;
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
            enableNumericControl(numericResonanceFrequencyReal, minFrequency, maxFrequency);
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
            int numSeries = (int)numericSeriesLength.Value;
            int numLevels = (int)numericLevelsNumber.Value;
            int numInterpolationPoints = (int)numericInterpolationLength.Value;
            if (mSelectedModalSet != null)
                mSelectedModalSet.ReferenceResponse = mSelectedAcceleration;

            // Estimate the characteristics
            mCharacteristics = new ResponseCharacteristics(mSelectedAcceleration, ref frequencyBoundaries, ref levelsBoundaries,
                                                                                  numSeries, numLevels, numInterpolationPoints,
                                                                                  numericResonanceFrequencyReal.Value, numericResonanceFrequencyImaginary.Value, numericResonanceFrequencyAmplitude.Value,
                                                                                  mSelectedModalSet);

            // Set signal data to plot
            mSignalGraphModels[SignalModelType.kImaginary].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, mCharacteristics.ResonanceFrequencyImaginary, numSeries);
            mSignalGraphModels[SignalModelType.kReal].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, mCharacteristics.ResonanceFrequencyReal, numSeries);
            mSignalGraphModels[SignalModelType.kAmplitude].setData(mSelectedAcceleration, frequencyBoundaries, levelsBoundaries, mCharacteristics.ResonanceFrequencyAmplitude, numSeries);
            mHodographGraphModel.setData(mSelectedAcceleration, mCharacteristics.ResonanceRealPeak, mCharacteristics.ResonanceImaginaryPeak);
            if (mSelectedModalSet != null)
                mMonophaseGraphModel.setData(mSelectedModalSet.Responses);

            // Set results
            mDecrementGraphModel.setData(mCharacteristics.Decrement);
            ModalResults modalResults = new ModalResults();
            modalResults.Add("Обобщенный", mCharacteristics.ModalGeneral);
            modalResults.Add("Комплексный", mCharacteristics.ModalComplex);
            foreach (ModalGraphModel model in mModalGraphModels)
                model.setData(modalResults);

            // Correct input parameters
            numericLeftFrequencyBoundary.Value = frequencyBoundaries.Item1;
            numericRightFrequencyBoundary.Value = frequencyBoundaries.Item2;
            numericLeftLevelsBoundary.Value = levelsBoundaries.Item1;
            numericRightLevelsBoundary.Value = levelsBoundaries.Item2;
            if (mCharacteristics.ResonanceFrequencyReal > 0)
                numericResonanceFrequencyReal.Value = mCharacteristics.ResonanceFrequencyReal;
            if (mCharacteristics.ResonanceFrequencyImaginary > 0)
                numericResonanceFrequencyImaginary.Value = mCharacteristics.ResonanceFrequencyImaginary;
            if (mCharacteristics.ResonanceFrequencyAmplitude > 0)
                numericResonanceFrequencyAmplitude.Value = mCharacteristics.ResonanceFrequencyAmplitude;
            numericDecrementByReal.Value = null;
            if (mCharacteristics.Decrement != null)
            {
                if (mCharacteristics.Decrement.Real > 0)
                    numericDecrementByReal.Value = mCharacteristics.Decrement.Real;
            }
        }

        public void plotInput()
        {
            foreach (AbstractSignalGraphModel model in mSignalGraphModels.Values)
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
            // Calculation data
            clearCalculationData();
        }

        public void clearCalculationData()
        {
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

            foreach (AbstractSignalGraphModel model in mSignalGraphModels.Values)
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
        private Dictionary<SignalModelType, AbstractSignalGraphModel> mSignalGraphModels;
        private HodographGraphModel mHodographGraphModel;
        private MonophaseGraphModel mMonophaseGraphModel;
        // Output models
        private DecrementGraphModel mDecrementGraphModel;
        private List<ModalGraphModel> mModalGraphModels;
        // Computed data
        private ResponseCharacteristics mCharacteristics;
    }
}
