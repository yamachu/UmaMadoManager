using System;
using System.Windows.Forms;
using UmaMadoManager.Windows.Services;

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
            // Native.Win32API.AllocConsole();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var nativeWindowManager = new NativeWindowManager();
            var screenManager = new ScreenManager();
            var audioManager = new AudioManager();
            var versionRepository = new VersionRepository();
            var settingService = new SettingService();
            settingService.Init();

            var _ = new Views.UmaMadoManagerUI(
                new Core.ViewModels.AxisStandardViewModel(
                    nativeWindowManager,
                    screenManager,
                    audioManager,
                    versionRepository,
                    settingService));
            Application.Run();
        }
    }
}
