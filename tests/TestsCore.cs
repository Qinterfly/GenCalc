using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;

namespace Tests
{
    [TestClass]
    public class TestsCore
    {
        public TestsCore()
        {
            string filePath = Path.GetFullPath(baseDirectory + "/" + projectName);
            project = new LMSProject(filePath);
        }

        [TestMethod]
        public void Test1Selection()
        {
            acceleration = project.retrieveSelectedSignal(accelerationPath);
            force = project.retrieveSelectedSignal(forcePath);
            Assert.IsTrue(acceleration != null);
            Assert.IsTrue(force != null);
        }

        [TestMethod]
        public void Test2Calculation()
        {
            Tuple<double, double> frequencyBoundaries = new Tuple<double, double>(-1, -1);
            Tuple<double, double> levelsBoundaries = new Tuple<double, double>(0.1, 0.9);
            ResponseCharacteristics characteristics = new ResponseCharacteristics(acceleration, force, ref frequencyBoundaries, ref levelsBoundaries, 20);
        }

        public static string baseDirectory = "../../../examples";
        public static string projectName = "Mitten.lms";
        public static string accelerationPath = "Section 2/Отч 6,9 СГИКр1 21,55Гц/ResponsesSpectra/Harmonic Spectrum W:56:+X";
        public static string forcePath = "Section 2/Отч 6,9 СГИКр1 21,55Гц/ReferenceSpectra/Harmonic Spectrum W:57:+X";
        public static LMSProject project = null;
        public static Response acceleration = null;
        public static Response force = null;
    }
}
