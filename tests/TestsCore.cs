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
            ResponseCharacteristics characteristics = new ResponseCharacteristics(acceleration, ref frequencyBoundaries, ref levelsBoundaries,
                                                                                  numSeries: 6, numLevels: 20, numInterpolationPoints: 20, modalSet: modalSet);
        }

        [TestMethod]
        public void Test5FourierSeries()
        {
            var xData = new double[]
            {
                5.32999992370605, 5.33500003814697, 5.34000015258789, 5.34499979019165, 5.34999990463257,
                5.35500001907349, 5.36000013351440, 5.36499977111816, 5.36999988555908, 5.37500000000000,
                5.38000011444092, 5.38500022888184, 5.38999986648560, 5.39499998092651, 5.40000009536743,
                5.40500020980835, 5.40999984741211, 5.41499996185303, 5.42000007629395, 5.42500019073486,
                5.42999982833862, 5.43499994277954, 5.44000005722046, 5.44500017166138, 5.44999980926514,
                5.45499992370605, 5.46000003814697, 5.46500015258789, 5.46999979019165
            };
            var yData = new double[]
            {
                -0.000402205684328734, -0.000426122451244750, -0.000453447447942587, -0.000471466762867726, -0.000483534743380427,
                -0.000479737520312616, -0.000441671411359956, -0.000364431426327896, -0.000244713966864232, -0.000110801878237550,
                1.05091941985764e-05, 0.000117299700283617, 0.000203535702863452, 0.000271726908689982, 0.000325400886195295,
                0.000371604031876007, 0.000403041634634438, 0.000426216593336135, 0.000442092546212783, 0.000451469531029303,
                0.000456704399542531, 0.000458604568612620, 0.000458404637134094, 0.000453508381747045, 0.000446271705815979,
                0.000438360376782558, 0.000430368024961240, 0.000419475864074129, 0.000409106369064493
            };
            var series = new FourierSeries(xData, yData, 8);
            int numData = xData.Length;
            double[] errors = new double[numData];
            for (int i = 0; i != numData; ++i)
            {
                errors[i] = Math.Abs(series.Interpolate(xData[i]) - yData[i]);
                Assert.IsTrue(errors[i] < 1e-5);
            }
        }

        public static string projectPath = Path.GetFullPath("../../../examples/Mitten.lms");
        public static string baseForcePath = "Section1/Отч 8,5 АИСт1 13,06Гц/ReferenceSpectra/";
        public static string baseAccelerationPath = "Section1/Отч 8,5 АИСт1 13,06Гц/ResponsesSpectra/";

        public static LMSProject project = new LMSProject(projectPath);
        public static Response acceleration = null;
        public static ModalDataSet modalSet = new ModalDataSet();
    }
}
