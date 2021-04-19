using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using GenCalc.Core.Project;
using GenCalc;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        public Tests()
        {
            string filePath = Path.GetFullPath(mBaseDirectory + "/" + mProjectName);
            mProject = new LMSProject(filePath);
            
        }

        [TestMethod]
        public void TestSelection()
        {
            bool isSelected = mProject.retrieveSelection("Section 2/Отч 6,9 СГИКр1 21,55Гц/ResponsesSpectra/Harmonic Spectrum W:56:-Y");
            Assert.IsTrue(isSelected);
        }


        [TestMethod]
        public void TestCalculation()
        {

        }

        [TestMethod]
        public void TestGui()
        {
            MainWindow window = null;
            var thread = new Thread(() =>
            {
                window = new MainWindow();
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private const string mBaseDirectory = "../../../examples";
        private const string mProjectName = "Mitten.lms";
        private LMSProject mProject = null;
    }
}
