using System;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using UmaMadoManager.Windows.Services;

[assembly:AssemblyKeyFileAttribute("keyfile.snk")]
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

            var nativeWindowManager = new NativeWindowManager();
            var screenManager = new ScreenManager();
            var audioManager = new AudioManager();
            var versionRepository = new VersionRepository();
            var settingService = new SettingService();
            var debugService = new DebugService();
            settingService.Init();

            var isDebugMode = System.Environment.GetCommandLineArgs().Count(v => v == "--debug") == 1;
            if (isDebugMode) {
                debugService.AllocConsole();
            }

            var _ = new Views.UmaMadoManagerUI(
                new Core.ViewModels.AxisStandardViewModel(
                    nativeWindowManager,
                    screenManager,
                    audioManager,
                    versionRepository,
                    settingService,
                    debugService),
                isDebugMode);
            Application.Run();
        }
    }
}
