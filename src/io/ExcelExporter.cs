using OfficeOpenXml;
using OfficeOpenXml.Style;
using GenCalc.Core.Numerical;
using System;
using System.Collections.Generic;

namespace GenCalc.IO
{
    public class ExcelExporter
    {
        public ExcelExporter(in ResponseCharacteristics characteristics, in ModalDataSet dataSet)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            mPackage = new ExcelPackage();
            setProperties();
            createCharacteristicsWorksheet(characteristics);
            createDataSetWorksheet(dataSet);
            createParametersWorksheet(characteristics);
        }

        public void write(string pathFile)
        {
            mPackage.SaveAs(pathFile);
        }

        private void setProperties()
        {
            mPackage.Workbook.Properties.Author  = "GenCalc";
            mPackage.Workbook.Properties.Title   = "Computed data";
            mPackage.Workbook.Properties.Subject = "TestLab";
            mPackage.Workbook.Properties.Created = DateTime.Now;
        }

        private void createCharacteristicsWorksheet(in ResponseCharacteristics characteristics)
        {
            const int kNumRowsHeader = 2;
            const int kNumColsHeader = 13;

            var worksheet = mPackage.Workbook.Worksheets.Add("Results");

            // Set the style
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1, kNumRowsHeader, kNumColsHeader].Style.Font.Bold = true;

            // Create the header
            worksheet.Cells[1, 1].Value  = "Level";
            worksheet.Cells[1, 10].Value = "Decrement";
            worksheet.Cells[2, 11].Value = "Imaginary";
            worksheet.Cells[2, 12].Value = "Amplitude";
            worksheet.Cells[2, 13].Value = "General";

            // Merge the headers
            worksheet.Cells[1, 1, kNumRowsHeader, 1].Merge = true;
            worksheet.Cells[1, 10, 1, 13].Merge            = true;

            // Check if there are any characteristics to write
            if (characteristics is null)
                return;

            // Set the levels data
            int iStartRowData = kNumRowsHeader + 1;
            int numLevels = characteristics.Levels.Length;
            for (int i = 0; i != numLevels; ++i)
                worksheet.Cells[iStartRowData + i, 1].Value = characteristics.Levels[i];

            // Output the modal data
            setModalData(worksheet, characteristics.Levels, characteristics.ModalGeneral, 1, 2, "General");
            setModalData(worksheet, characteristics.Levels, characteristics.ModalComplex, 1, 6, "Complex");

            // Output the decrements
            setDecrement(worksheet, characteristics.Levels, characteristics.Decrement.Imaginary, kNumRowsHeader, 10, "Imaginary");
            setDecrement(worksheet, characteristics.Levels, characteristics.Decrement.Amplitude, kNumRowsHeader, 11, "Amplitude");
            setDecrement(worksheet, characteristics.Levels, characteristics.Decrement.General, kNumRowsHeader,   12, "General");
            setDecrement(worksheet, characteristics.Levels, characteristics.Decrement.Complex, kNumRowsHeader,   13, "Complex");

            // Set the border style
            setBorders(worksheet);

            // Autofit
            worksheet.Cells.AutoFitColumns();
        }

        private void createDataSetWorksheet(in ModalDataSet dataSet)
        {
            const int kNumRowsHeader = 3;

            var worksheet = mPackage.Workbook.Worksheets.Add("DataSet");

            // Set the style
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Set the reference response as well as frequencies
            int iColumn = 1;
            int iStartRowData = kNumRowsHeader + 1;
            if (dataSet.ReferenceResponse != null)
            {
                // Frequency header
                worksheet.Cells[1, iColumn].Value = "Frequency";
                worksheet.Cells[1, iColumn].Style.Font.Bold = true;
                worksheet.Cells[1, iColumn, 3, iColumn].Merge = true;

                // Frequency data
                int numData = dataSet.ReferenceResponse.Frequency.Length;
                for (int k = 0; k != numData; ++k)
                    worksheet.Cells[iStartRowData + k, iColumn].Value = dataSet.ReferenceResponse.Frequency[k];
                ++iColumn;
                
                // Selected response
                worksheet.Cells[1, iColumn].Value = "Selected response";
                worksheet.Cells[1, iColumn].Style.Font.Bold = true;
                worksheet.Cells[1, iColumn, 1, iColumn + 1].Merge = true;
                setResponses(new List<Response> { dataSet.ReferenceResponse }, worksheet, ref iColumn);
            }

            // Set the forces
            if (dataSet.Forces != null)
            {
                worksheet.Cells[1, iColumn].Value = "Forces";
                worksheet.Cells[1, iColumn].Style.Font.Bold = true;
                int iStartColumn = iColumn;
                setResponses(dataSet.Forces, worksheet, ref iColumn);
                worksheet.Cells[1, iStartColumn, 1, iColumn - 1].Merge = true;
            }

            // Set the responses
            if (dataSet.Responses != null)
            {
                worksheet.Cells[1, iColumn].Value = "Responses";
                worksheet.Cells[1, iColumn].Style.Font.Bold = true;
                int iStartColumn = iColumn;
                setResponses(dataSet.Responses, worksheet, ref iColumn);
                worksheet.Cells[1, iStartColumn, 1, iColumn - 1].Merge = true;
            }

            // Set the border style
            setBorders(worksheet);

            // Autofit
            worksheet.Cells.AutoFitColumns();
        }

        private void createParametersWorksheet(in ResponseCharacteristics characteristics)
        {
            var worksheet = mPackage.Workbook.Worksheets.Add("Parameters");

            // Set the style
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Assign the header
            worksheet.Cells[1, 1].Value = "Resonance frequency";
            worksheet.Cells[2, 1].Value = "Real";
            worksheet.Cells[2, 2].Value = "Imaginary";
            worksheet.Cells[2, 3].Value = "Amplitude";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1, 1, 3].Merge = true;

            // Check if there are any characteristics to write
            if (characteristics is null)
                return;

            // Set the data
            worksheet.Cells[3, 1].Value = characteristics.ResonanceFrequencyReal;
            worksheet.Cells[3, 2].Value = characteristics.ResonanceFrequencyImaginary;
            worksheet.Cells[3, 3].Value = characteristics.ResonanceFrequencyAmplitude;

            // Set the border style
            setBorders(worksheet);

            // Autofit
            worksheet.Cells.AutoFitColumns();
        }

        private void setResponses(List<Response> responses, ExcelWorksheet worksheet, ref int iColumn)
        {
            const int kRowName = 2;
            const int kRowPart = 3;

            int numResponses = responses.Count;
            for (int iResponse = 0; iResponse != numResponses; ++iResponse)
            {
                Response response = responses[iResponse];

                // Header
                worksheet.Cells[kRowName, iColumn].Value = response.Name;
                worksheet.Cells[kRowPart, iColumn].Value = "Real";
                worksheet.Cells[kRowPart, iColumn + 1].Value = "Imag";
                worksheet.Cells[kRowName, iColumn, kRowName, iColumn + 1].Merge = true;

                // Data
                int iStartRowData = kRowPart + 1;
                for (int k = 0; k != response.Length; ++k)
                {
                    worksheet.Cells[iStartRowData + k, iColumn].Value = response.RealPart[k];
                    worksheet.Cells[iStartRowData + k, iColumn + 1].Value = response.ImaginaryPart[k];
                }
                iColumn += 2;
            }
        }

        private void setDecrement(ExcelWorksheet worksheet, in double[] levels, in Dictionary<double, double> decrement, int iHeaderRow, int iColumn, string name)
        {
            // Create the subheader
            worksheet.Cells[iHeaderRow, iColumn].Value = name;

            // Set the data
            double[] keys;
            double[] values;
            Utilities.dictionaryToVectors(decrement, out keys, out values);
            int numKeys = keys.Length;
            int iStartRow = iHeaderRow + 1;
            for (int i = 0; i != numKeys; ++i)
            {
                int iInsertRow = Array.IndexOf(levels, keys[i]);
                if (iInsertRow < 0)
                    continue;
                iInsertRow += iStartRow;
                worksheet.Cells[iInsertRow, iColumn].Value = values[i];
            }
        }

        private void setModalData(ExcelWorksheet worksheet, in double[] levels, in ModalParameters parameters, int iHeaderRow, int iStartColumn, string name)
        {
            // Create the header
            worksheet.Cells[iHeaderRow, iStartColumn].Value = name;
            worksheet.Cells[iHeaderRow, iStartColumn, iHeaderRow, iStartColumn + 3].Merge = true;

            // Create the subheader
            ++iHeaderRow;
            worksheet.Cells[iHeaderRow, iStartColumn    ].Value = "Mass";
            worksheet.Cells[iHeaderRow, iStartColumn + 1].Value = "Stiffness";
            worksheet.Cells[iHeaderRow, iStartColumn + 2].Value = "Damping";
            worksheet.Cells[iHeaderRow, iStartColumn + 3].Value = "Frequency";

            // Set the data
            int iStartRow = iHeaderRow + 1;
            int numModalLevels = parameters.Levels.Count;
            for (int i = 0; i != numModalLevels; ++i)
            {
                int iInsertRow = Array.IndexOf(levels, parameters.Levels[i]);
                if (iInsertRow < 0)
                    continue;
                iInsertRow += iStartRow;
                worksheet.Cells[iInsertRow, iStartColumn    ].Value = parameters.Mass[i];
                worksheet.Cells[iInsertRow, iStartColumn + 1].Value = parameters.Stiffness[i];
                worksheet.Cells[iInsertRow, iStartColumn + 2].Value = parameters.Damping[i];
                worksheet.Cells[iInsertRow, iStartColumn + 3].Value = parameters.Frequency[i];
            }
        }

        private void setBorders(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension is null)
                return;
            var borderStyle = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].Style.Border;
            borderStyle.Top.Style = ExcelBorderStyle.Thin;
            borderStyle.Right.Style = ExcelBorderStyle.Thin;
            borderStyle.Bottom.Style = ExcelBorderStyle.Thin;
            borderStyle.Left.Style = ExcelBorderStyle.Thin;
        }

        private ExcelPackage mPackage;
    }
}
