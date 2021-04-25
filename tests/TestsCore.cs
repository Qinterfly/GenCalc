using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;

namespace Tests
{
    [TestClass]
    public class TestsCore
    {

        [TestMethod]
        public void Test1SelectAcceleration()
        {
            acceleration = project.retrieveSelectedSignal(baseAccelerationPath + "Harmonic Spectrum HTP:106:-Y");
            Assert.IsTrue(acceleration != null);
        }

        [TestMethod]
        public void Test2SelectForces()
        {
            List<string> listPathForces = new List<string>() { baseForcePath + "Harmonic Spectrum HTP:106:+Y",
                                                               baseForcePath + "Harmonic Spectrum HTP:85:+Y" };
            modalSet.Forces = project.retrieveSelectedSignals(listPathForces);
            Assert.AreEqual(modalSet.Forces.Count, 2);
        }

        [TestMethod]
        public void Test3SelectResponses()
        {
            List<string> listPathResponses = new List<string>() { baseAccelerationPath + "Harmonic Spectrum HTP:106:-Y",
                                                                  baseAccelerationPath + "Harmonic Spectrum HTP:85:-Y" };
            modalSet.Responses = project.retrieveSelectedSignals(listPathResponses);
            Assert.AreEqual(modalSet.Responses.Count, 2);
        }

        [TestMethod]
        public void Test4Calculate()
        {
            modalSet.ReferenceResponse = acceleration;
            Tuple<double, double> frequencyBoundaries = new Tuple<double, double>(-1, -1);
            Tuple<double, double> levelsBoundaries = new Tuple<double, double>(0.1, 0.9);
            ResponseCharacteristics characteristics = new ResponseCharacteristics(acceleration, ref frequencyBoundaries, ref levelsBoundaries, 20, numInterpolationPoints: 20, modalSet: modalSet);
        }

        public static string projectPath = Path.GetFullPath("../../../examples/Mitten.lms");
        public static string baseForcePath = "Section1/Отч 8,5 АИСт1 13,06Гц/ReferenceSpectra/";
        public static string baseAccelerationPath = "Section1/Отч 8,5 АИСт1 13,06Гц/ResponsesSpectra/";
        public static LMSProject project = new LMSProject(projectPath);
        public static Response acceleration = null;
        public static ModalDataSet modalSet = new ModalDataSet();
    }
}
