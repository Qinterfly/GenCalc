using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using GenCalc.Gui.Plot;

namespace GenCalc
{
    using PairDouble = Tuple<double, double>;

    public partial class MainWindow
    {
        private enum EventType
        {
            kUnknown,
            kFocus,
            kEnterKey
        }

        private EventType getEventType(RoutedEventArgs e)
        {
            string type = e.GetType().ToString();
            if (type.Equals("System.Windows.RoutedEventArgs"))
            {
                return EventType.kFocus;
            }
            else if (type.Equals("System.Windows.Input.KeyEventArgs"))
            {
                KeyEventArgs keyEvent = (KeyEventArgs)e;
                if (keyEvent.Key.Equals(Key.Enter))
                    return EventType.kEnterKey;
            }
            return EventType.kUnknown;
        }

        private bool isTypeValid(EventType type)
        {
            return type.Equals(EventType.kFocus) || type.Equals(EventType.kEnterKey);
        }

        private void baseNumericChanged(object sender, RoutedEventArgs e)
        {
            EventType type = getEventType(e);
            if (isTypeValid(type))
                calculateAndPlot();
        }

        private void numericFrequencyBoundariesChanged(object sender, RoutedEventArgs e)
        {
            if (!checkBoundaries(numericLeftFrequencyBoundary, numericRightFrequencyBoundary))
                return;
            EventType type = getEventType(e);
            if (isTypeValid(type))
                calculateAndPlot();
        }

        private void numericLevelsBoundariesChanged(object sender, RoutedEventArgs e)
        {
            if (!checkBoundaries(numericLeftLevelsBoundary, numericRightLevelsBoundary))
                return;
            EventType type = getEventType(e);
            if (isTypeValid(type))
                calculateAndPlot();
        }

        private bool checkBoundaries(NumericUpDown leftControl, NumericUpDown rightControl)
        {
            if (leftControl.Value == null || rightControl.Value == null)
                return false;
            double leftValue = (double)leftControl.Value;
            double rightValue = (double)rightControl.Value;
            if (leftValue > rightValue)
            {
                double temp = leftValue;
                leftValue = rightValue;
                rightValue = temp;
            }
            leftControl.Value = leftValue;
            rightControl.Value = rightValue;
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

        private void buttonSelectAcceleration_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectAcceleration(), "The acceleration was selected via TestLab", "An error occured while selecting an acceleration");
        }

        private void buttonSelectForces_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectForces(), "The forces were selected via TestLab", "An error occured while selecting forces");
        }

        private void buttonSelectResponses_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectResponses(), "The responses were selected via TestLab", "An error occured while selecting responses");
        }

        private void processSelectionWithStatus(bool resFun, string successMessage, string errorMessage)
        {
            if (resFun)
            {
                calculateAndPlot();
                setStatus(successMessage);
            }
            else
            {
                setStatus(errorMessage);
            }
        }

        private void frequencyBoundariesChanged(PairDouble boundaries)
        {
            numericLeftFrequencyBoundary.Value = boundaries.Item1;
            numericRightFrequencyBoundary.Value = boundaries.Item2;
            calculateAndPlot();
        }

        private void resonanceFrequencyRealChanged(double resonanceFrequency)
        {
            numericResonanceFrequencyReal.Value = resonanceFrequency;
            calculateAndPlot();
        }

        private void resonanceFrequencyImaginaryChanged(double resonanceFrequency)
        {
            numericResonanceFrequencyImaginary.Value = resonanceFrequency;
            calculateAndPlot();
        }

        private void resonanceFrequencyAmplitudeChanged(double resonanceFrequency)
        {
            numericResonanceFrequencyAmplitude.Value = resonanceFrequency;
            calculateAndPlot();
        }

        private void pointSelectionChanged(PairDouble coordinates)
        {
            const string format = "F4";
            string labelX = coordinates.Item1.ToString(format);
            string labelY = coordinates.Item2.ToString(format);
            setStatus($"Selected point: ({labelX}, {labelY})");
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                foreach (AbstractGraphModel model in mSignalGraphModels.Values)
                    model.clearPointSelection();
                foreach (AbstractGraphModel model in mModalGraphModels)
                    model.clearPointSelection();
                mHodographGraphModel.clearPointSelection();
                mMonophaseGraphModel.clearPointSelection();
                mDecrementGraphModel.clearPointSelection();
                clearStatus();
            }
        }

        private void removeAcceleration(object sender, RoutedEventArgs e)
        {
            if (mSelectedAcceleration != null)
            {
                mSelectedAcceleration = null;
                textBoxSelectedAcceleration.Text = "";
                clearCalculationData();
            }
        }

        private void removeForces(object sender, RoutedEventArgs e)
        {
            if (mSelectedModalSet.Forces != null)
            {
                mSelectedModalSet.Forces = null;
                listBoxForces.Items.Clear();
                calculateAndPlot();
            }
        }

        private void removeResponses(object sender, RoutedEventArgs e)
        {
            if (mSelectedModalSet.Responses != null)
            {
                mSelectedModalSet.Responses = null;
                listBoxResponses.Items.Clear();
                calculateAndPlot();
            }
        }

        void DeleteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            object source = e.OriginalSource;
            Type type = source.GetType();
            string content = "";
            if (type.Name.Equals("TextBox"))
            {
                TextBox textBox = (TextBox)source;
                content = textBox.Text;
            }
            if (type.Name.Equals("ListBoxItem"))
            {
                ListBoxItem listBox = (ListBoxItem)source;
                content = listBox.Content.ToString();
            }
            e.CanExecute = content.Length != 0;
        }

        void DeleteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {

        }
    }
}
