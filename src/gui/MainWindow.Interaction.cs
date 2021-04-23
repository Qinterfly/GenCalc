using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MahApps.Metro.Controls;

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

        private void frequencyBoundariesChanged(PairDouble boundaries)
        {
            numericLeftFrequencyBoundary.Value = boundaries.Item1;
            numericRightFrequencyBoundary.Value = boundaries.Item2;
            calculateAndPlot();
        }
    }
}
