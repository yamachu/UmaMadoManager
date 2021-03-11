using System;
using System.Windows.Forms;
using System.Linq;
using System.Reactive.Linq;

namespace UmaMadoManager.Windows
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var nativeWindowManager = new Core.Services.NativeWindowManager(Screen.AllScreens.Select(s =>
            {
                return new Core.Models.Screen
                {
                    Bounds = s.Bounds,
                    WorkingArea = s.WorkingArea,
                };
            }).ToArray());

            var _ = new Views.UmaMadoManagerUI(new Core.ViewModels.AxisStandardViewModel(nativeWindowManager));
            Application.Run();
        }
    }
}
