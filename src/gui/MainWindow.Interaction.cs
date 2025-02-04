﻿using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using GenCalc.Gui.Plot;
using GenCalc.IO;

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
            openFileDialog.Filter = "LMS Test.Lab Project (*.lms)|*.lms";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                openProject(openFileDialog.FileName);
                setStatus("Проект успешно открыт");
            }
            else
            {
                setStatus("Произошла ошибка при открытии файла проекта");
                return;
            }
        }

        private void buttonSelectAcceleration_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectAcceleration(), "Ускорение выбрано через TestLab", "Произошла ошибка при выборе сигнала ускорения");
        }

        private void buttonSelectForces_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectForces(), "Силы выбраны через TestLab", "Произошла ошибка при выборе сил");
        }

        private void buttonSelectResponses_Click(object sender, RoutedEventArgs e)
        {
            processSelectionWithStatus(selectResponses(), "Отклики выбраны через TestLab", "Произошла ошибка при выборе откликов");
        }

        private void buttonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            clearStatus();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel document (*.xlsx)|*.xlsx";
            dialog.FilterIndex = 1;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExcelExporter exporter = new ExcelExporter(mCharacteristics, mSelectedModalSet);
                    exporter.write(dialog.FileName);
                    setStatus("Документ Excel успешно создан");
                }
                catch (System.Runtime.InteropServices.COMException exc)
                {
                    setStatus(exc.ToString());
                }
            }
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
            setStatus($"Выбрана точка: ({labelX}, {labelY})");
            try
            {
                Clipboard.SetText(labelY, TextDataFormat.Text);
            }
            catch { }
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

        private void removeAcceleration(object sender = null, RoutedEventArgs e = null)
        {
            if (mSelectedAcceleration != null)
            {
                mSelectedAcceleration = null;
                textBoxSelectedAcceleration.Text = "";
                clearCalculationData();
            }
        }

        private void removeForces(object sender = null, RoutedEventArgs e = null)
        {
            if (mSelectedModalSet.Forces != null)
            {
                mSelectedModalSet.Forces = null;
                listBoxForces.Items.Clear();
                calculateAndPlot();
            }
        }

        private void removeResponses(object sender = null, RoutedEventArgs e = null)
        {
            if (mSelectedModalSet.Responses != null)
            {
                mSelectedModalSet.Responses = null;
                listBoxResponses.Items.Clear();
                calculateAndPlot();
            }
        }
    }
}
