using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        public Tests()
        {
            string filePath = Path.GetFullPath(baseDirectory + "/" + projectName);
            mProject = new LMSProject(filePath);
        }

        [TestMethod]
        public void Test1Selection()
        {
            mSelectedSignal = mProject.retrieveSelectedSignal(signalPath);
            Assert.IsTrue(mSelectedSignal != null);
        }

        [TestMethod]
        public void Test2Calculation()
        {
            Tuple<double, double> frequencyBoundaries = new Tuple<double, double>(-1.0, -1.0);
            Tuple<double, double> levelsBoundaries = new Tuple<double, double>(0.0, 1.0);
            ResponseCharacteristics characteristics = new ResponseCharacteristics(mSelectedSignal, ref frequencyBoundaries, ref levelsBoundaries, 512);
        }

        public static string baseDirectory = "../../../examples";
        public static string projectName = "Mitten.lms";
        public static string signalPath = "Section 2/Отч 6,9 СГИКр1 21,55Гц/ResponsesSpectra/Harmonic Spectrum W:56:-Y";
        public static LMSProject mProject = null;
        public static Response mSelectedSignal = null;
    }
}
