using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Windows.Services;
using Application = System.Windows.Application;

namespace UmaMadoManager.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private Core.ViewModels.AxisStandardViewModel viewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var nativeWindowManager = new NativeWindowManager();
            var screenManager = new ScreenManager();
            var audioManager = new AudioManager();
            var versionRepository = new VersionRepository();
            var settingService = new SettingService();
            var debugService = new DebugService();
            var applicationService = new ApplicationService();
            settingService.Init();

            var isDebugMode = System.Environment.GetCommandLineArgs().Count(v => v == "--debug") == 1;
            if (isDebugMode)
            {
                debugService.AllocConsole();
            }

            viewModel = new Core.ViewModels.AxisStandardViewModel(
                    nativeWindowManager,
                    screenManager,
                    audioManager,
                    versionRepository,
                    settingService,
                    applicationService);

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            var icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("UmaMadoManager.Windows.Resources.TrayIcon.ico"));
            notifyIcon.Icon = icon;
            notifyIcon.DataContext = viewModel;

            viewModel.OnClickedOpenVerticalUserDefinedModal.Subscribe(s =>
            {
                var view = new Views.UserDefineModal(!viewModel.UserDefinedVerticalWindowRect.Value.IsEmpty);
                view.OnClickUsingPrevious = () =>
                {
                    viewModel.UseCurrentVerticalUserSetting.Value = false;
                    viewModel.Vertical.Value = AxisStandard.User;
                    view.Close();
                };
                view.OnClickUsingCurrent = () =>
                {
                    viewModel.UseCurrentVerticalUserSetting.Value = true;
                    viewModel.Vertical.Value = AxisStandard.User;
                    view.Close();
                };
                view.Show();
            });

            viewModel.OnClickedOpenHorizontalUserDefinedModal.Subscribe(_ =>
            {
                var view = new Views.UserDefineModal(!viewModel.UserDefinedHorizontalWindowRect.Value.IsEmpty);
                view.OnClickUsingPrevious = () =>
                {
                    viewModel.UseCurrentHorizontalUserSetting.Value = false;
                    viewModel.Horizontal.Value = AxisStandard.User;
                    view.Close();
                };
                view.OnClickUsingCurrent = () =>
                {
                    viewModel.UseCurrentHorizontalUserSetting.Value = true;
                    viewModel.Horizontal.Value = AxisStandard.User;
                    view.Close();
                };
                view.Show();
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
