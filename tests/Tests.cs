using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;
using GenCalc;
using MahApps.Metro.Controls;

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
            Tuple<double, double> frequencyBoundaries = new Tuple<double, double>(-1, -1);
            Tuple<double, double> levelsBoundaries = new Tuple<double, double>(0.1, 0.9);
            ResponseCharacteristics characteristics = new ResponseCharacteristics(mSelectedSignal, ref frequencyBoundaries, ref levelsBoundaries, 20);
        }

        [TestMethod]
        public void Test3Plot()
        {
            var t = new Thread(() =>
            {
                var window = new MainWindow();
                string filePath = Path.GetFullPath(baseDirectory + "/" + projectName);
                window.openProject(filePath);
                window.selectSignal(signalPath);
                window.calculateCharacteristics();
                window.plotInput();
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        public static string baseDirectory = "../../../examples";
        public static string projectName = "Mitten.lms";
        public static string signalPath = "Section 2/Отч 6,9 СГИКр1 21,55Гц/ResponsesSpectra/Harmonic Spectrum W:56:+X";
        public static LMSProject mProject = null;
        public static Response mSelectedSignal = null;
    }
}
