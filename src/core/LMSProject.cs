using System;
using LMSTestLabAutomation;

namespace GenCalc.Core.Project
{
    public partial class LMSProject
    {
        public LMSProject(in string filePath)
        {
            try
            {
                mApp = new Application();
                if (mApp.Name == "")
                    mApp.Init("-w DesktopStandard ");
                mApp.OpenProject(filePath);
            }
            catch (System.Runtime.InteropServices.COMException exc)
            {
                mApp = null;
                throw exc;
            }
            // Retrieve properties
            mPath = filePath;
            mDatabase = mApp.ActiveBook.Database();
        }

        public bool retrieveSelection(string pathSignal = null)
        {
            mSelectedSignal = null;
            try
            {
                IBlock2 signal = null;
                if (pathSignal == null)
                {
                    DataWatch dataWatch = mApp.ActiveBook.FindDataWatch("Navigator_SelectedOIDs");
                    IData dataSelected = dataWatch.Data;
                    AttributeMap attributeMap = dataSelected.AttributeMap;
                    int nSelected = attributeMap.Count;
                    for (int iSignal = 0; iSignal != nSelected; ++iSignal) { 
                        DataWatch blockWatch = mApp.FindDataWatch(attributeMap[iSignal]);
                        if (blockWatch.Data.Type != "LmsHq::DataModelI::Expression::CBufferIBlock")
                            continue;
                        // Retrieving path
                        IData dataOID = attributeMap[iSignal].AttributeMap["OID"];
                        pathSignal = dataOID.AttributeMap["Path"].AttributeMap["PathString"];
                        // Retreiving signals
                        signal = blockWatch.Data;
                        break;
                    }
                }
                else
                {
                    signal = (IBlock2)mDatabase.GetItem(pathSignal);
                }
                if (signal == null)
                    return false;
                AttributeMap properties = signal.Properties;
                string measuredQuantity = properties["Measured quantity"];
                if (measuredQuantity.Equals("Acceleration"))
                    mSelectedSignal = retrieveAcceleration(pathSignal, signal, properties);
            }
            catch
            {
                
            }
            return mSelectedSignal == null;
        }

        private Response retrieveAcceleration(in string path, in IBlock2 signal, in AttributeMap properties)
        {
            double[] frequency = (double[])signal.XValues;
            double[,] data = (double[,])signal.YValues; // Units: m/s^2 or (m/s^2)/N 
            // Retrieving additional info
            double sign = 1.0;
            if (properties["Point direction sign"] == "-")
                sign = -1.0;
            // Normalizing signals
            int nResponse = data.GetLength(0);
            double[] realPart = new double[nResponse];
            double[] imaginaryPath = new double[nResponse];
            for (int k = 0; k != nResponse; ++k)
            {
                realPart[k]      = data[k, 0] * sign;
                imaginaryPath[k] = data[k, 1] * sign * (-1.0);
            }
            Response currentResponse = new Response(signal.Label, path, frequency, realPart, imaginaryPath);
            return currentResponse;
        }

        // Properties and fields
        private string mPath;
        private readonly Application mApp = null;
        private readonly IDatabase mDatabase = null;
        private Response mSelectedSignal = null; 
    }
}
