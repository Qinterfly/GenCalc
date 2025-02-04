﻿using System.Collections.Generic;
using LMSTestLabAutomation;
using GenCalc.Core.Numerical;

namespace GenCalc.Core.Project
{
    public class LMSProject
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
            mUnitSystem = mApp.UnitSystem;
        }

        public bool isOpened() { return mApp != null; }

        public List<Response> retrieveSelectedSignals(List<string> listPathSignals = null)
        {
            List<Response> selectedSignals = new List<Response>();
            try
            {
                IBlock2 signal = null;
                if (listPathSignals == null)
                {
                    DataWatch dataWatch = mApp.ActiveBook.FindDataWatch("Navigator_SelectedOIDs");
                    IData dataSelected = dataWatch.Data;
                    AttributeMap attributeMap = dataSelected.AttributeMap;
                    int nSelected = attributeMap.Count;
                    for (int iSignal = 0; iSignal != nSelected; ++iSignal)
                    {
                        DataWatch blockWatch = mApp.FindDataWatch(attributeMap[iSignal]);
                        if (blockWatch.Data.Type != "LmsHq::DataModelI::Expression::CBufferIBlock")
                            continue;
                        // Retrieving path
                        IData dataOID = attributeMap[iSignal].AttributeMap["OID"];
                        string pathSignal = dataOID.AttributeMap["Path"].AttributeMap["PathString"];
                        // Retreiving signals
                        signal = blockWatch.Data;
                        if (signal != null)
                        {
                            Response response = acquireResponse(pathSignal, signal, signal.Properties);
                            if (response != null)
                                selectedSignals.Add(response);
                        }
                    }
                }
                else
                {
                    foreach (string pathSignal in listPathSignals)
                    {
                        signal = (IBlock2)mDatabase.GetItem(pathSignal);
                        if (signal != null)
                        {
                            Response response = acquireResponse(pathSignal, signal, signal.Properties);
                            if (response != null)
                                selectedSignals.Add(response);
                        }
                    }
                }

            }
            catch
            {
                selectedSignals = new List<Response>();
            }
            return selectedSignals;
        }

        public Response retrieveSelectedSignal(string pathSignal = null)
        {
            List<Response> selectedSignals = new List<Response>();
            if (pathSignal == null)
                selectedSignals = retrieveSelectedSignals();
            else
                selectedSignals = retrieveSelectedSignals(new List<string>() { pathSignal });
            return selectedSignals.Count > 0 ? selectedSignals[0] : null;
        }

        private Response acquireResponse(in string path, in IBlock2 signal, in AttributeMap properties)
        {
            ResponseType type = ResponseType.kUnknown;
            string measuredQuantity = properties["Measured quantity"];
            // Determination of the type of the signal
            // 15A version
            if (measuredQuantity != null)
            {
                if (measuredQuantity.Equals("Acceleration"))
                    type = ResponseType.kAccel;
                else if (measuredQuantity.Equals("Force"))
                    type = ResponseType.kForce;
            }
            // 12A version
            else
            {
                IQuantity quantityY = properties["Y axis unit"];
                string unitY = mUnitSystem.Label(quantityY);
                if (unitY.Equals("g"))
                    type = ResponseType.kAccel;
                if (unitY.Equals("N"))
                    type = ResponseType.kForce;
            }
            if (type == ResponseType.kUnknown)
                return null;
            double[] frequency = (double[])signal.XValues;
            double[,] data = (double[,])signal.YValues; // Units: m/s^2 or (m/s^2)/N 
            if (frequency.Length <= 1)
                return null;
            // Retrieving additional info
            double sign = 1.0;
            if (properties["Point direction sign"] == "-")
                sign = -1.0;
            // Normalizing signals
            int nResponse = data.GetLength(0);
            double[] realPart = new double[nResponse];
            double[] imaginaryPart = new double[nResponse];
            for (int k = 0; k != nResponse; ++k)
            {
                realPart[k] = data[k, 0] * sign;
                imaginaryPart[k] = data[k, 1] * sign;
            }
            string originalRun = properties["Original run"].AttributeMap["Contents"];
            string node = properties["Point id"];
            string direction = properties["Point direction absolute"];
            Response currentResponse = new Response(type, path, signal.Label, originalRun, node, direction, frequency, realPart, imaginaryPart);
            return currentResponse;
        }

        private string mPath;
        private Application mApp = null;
        private IDatabase mDatabase = null;
        private IUnitSystem mUnitSystem = null;
    }
}
