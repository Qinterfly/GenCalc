using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using GenCalc.Core.Project;
using GenCalc.Core.Numerical;
using GenCalc;

namespace Tests
{
    [TestClass]
    class TestsApplication
    {
        public TestsApplication()
        {
            Thread thread = new Thread(() =>
            {
                application = new App();
                App.ResourceAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                application.InitializeComponent();
                application.Dispatcher.InvokeAsync(() =>
                {
                    MainWindow window = (MainWindow)application.MainWindow;
                    window.Show();
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                application.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        [TestMethod]
        public void Test1CalculateAndPlot()
        {
            window.openProject(filePath);
            window.selectAcceleration(accelerationPath);
            window.calculateAndPlot();
        }

        public static string filePath = Path.GetFullPath("../../../examples/Mitten.lms");
        public static string accelerationPath = "Section 2/Отч 6,9 СГИКр1 21,55Гц/ResponsesSpectra/Harmonic Spectrum W:56:+X";
        public static App application = null;
        public static MainWindow window = null;
    }
}
