using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using UmaMadoManager.Core.Extensions;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Core.ViewModels
{
    public class AxisStandardViewModel
    {
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        private Settings settings;
        public ReactiveProperty<AxisStandard> Vertical { get; }
        public ReactiveProperty<AxisStandard> Horizontal { get; }
        public ReactiveProperty<WindowFittingStandard> WindowFittingStandard { get; }
        public ReactiveProperty<bool> UseCurrentVerticalUserSetting { get; }
        public ReactiveProperty<bool> UseCurrentHorizontalUserSetting { get; }
        public ReactiveProperty<WindowRect> UserDefinedVerticalWindowRect { get; }
        public ReactiveProperty<WindowRect> UserDefinedHorizontalWindowRect { get; }

        public ReactiveProperty<MuteCondition> MuteCondition { get; }

        public ReactiveProperty<string> TargetApplicationName { get; }

        public ReactiveProperty<string> LatestVersion { get; }

        private ReadOnlyReactiveProperty<IntPtr> targetWindowHandle;

        private ReactiveProperty<T> BindSettings<T>(T val, string nameofParameter, ReactivePropertyMode mode = ReactivePropertyMode.Default)
        {
            return new ReactiveProperty<T>(val, mode).Also(v =>
            {
                Disposable.Add(v.Subscribe(vv =>
                {
                    typeof(Settings).GetProperty(nameofParameter).SetValue(settings, vv);
                }));
            });
        }

        public Action OnExit;

        // FIXME: VMでやることじゃない
        public AxisStandardViewModel(
            INativeWindowManager nativeWindowManager,
            IScreenManager screenManager,
            IAudioManager audioManager,
            IVersionRepository versionRepository,
            ISettingService settingService
            )
        {
            settings = settingService.Instance();
            Vertical = BindSettings(settings.Vertical, nameof(settings.Vertical));
            Horizontal = BindSettings(settings.Horizontal, nameof(settings.Horizontal));
            WindowFittingStandard = BindSettings(settings.WindowFittingStandard, nameof(settings.WindowFittingStandard));
            MuteCondition = BindSettings(settings.MuteCondition, nameof(settings.MuteCondition));
            TargetApplicationName = BindSettings(settings.TargetApplicationName, nameof(TargetApplicationName));
            LatestVersion = new ReactiveProperty<string>("");

            UseCurrentVerticalUserSetting = BindSettings(settings.UseCurrentVerticalUserSetting, nameof(settings.UseCurrentVerticalUserSetting), ReactivePropertyMode.RaiseLatestValueOnSubscribe);
            UseCurrentHorizontalUserSetting = BindSettings(settings.UseCurrentHorizontalUserSetting, nameof(settings.UseCurrentHorizontalUserSetting), ReactivePropertyMode.RaiseLatestValueOnSubscribe);
            UserDefinedVerticalWindowRect = BindSettings(settings.UserDefinedVerticalWindowRect, nameof(settings.UserDefinedVerticalWindowRect));
            UserDefinedHorizontalWindowRect = BindSettings(settings.UserDefinedHorizontalWindowRect, nameof(settings.UserDefinedHorizontalWindowRect));

            // FIXME: PollingじゃなくてGlobalHookとかでやりたい
            targetWindowHandle = Observable.Interval(TimeSpan.FromSeconds(1))
                .CombineLatest(TargetApplicationName)
                .Select(x => nativeWindowManager.GetWindowHandle(x.Second))
                .Distinct()
                .ToReadOnlyReactiveProperty();

            Disposable.Add(TargetApplicationName.Subscribe(x => nativeWindowManager.SetHook(x)));

            var windowRect = targetWindowHandle
                .CombineLatest(
                    Observable.FromEventPattern(nativeWindowManager, nameof(nativeWindowManager.OnMoveOrSizeChanged))
                )
                .Select(x =>
                {
                    if (x.First == IntPtr.Zero)
                    {
                        return WindowRect.Empty;
                    }
                    var r = nativeWindowManager.GetWindowRect(x.First);
                    if (r.IsEmpty)
                    {
                        return WindowRect.Empty;
                    }
                    return r;
                });

            Disposable.Add(UseCurrentHorizontalUserSetting.Subscribe(x =>
            {
                if (!x)
                {
                    return;
                }
                var handle = targetWindowHandle.Value;
                if (handle == IntPtr.Zero)
                {
                    return;
                }
                var r = nativeWindowManager.GetWindowRect(handle);
                if (r.IsEmpty)
                {
                    UserDefinedHorizontalWindowRect.Value = WindowRect.Empty;
                    return;
                }
                UserDefinedHorizontalWindowRect.Value = r;
                return;
            }));

            Disposable.Add(UseCurrentVerticalUserSetting.Subscribe(x =>
            {
                if (!x)
                {
                    return;
                }
                var handle = targetWindowHandle.Value;
                if (handle == IntPtr.Zero)
                {
                    return;
                }
                var r = nativeWindowManager.GetWindowRect(handle);
                if (r.IsEmpty)
                {
                    UserDefinedVerticalWindowRect.Value = WindowRect.Empty;
                    return;
                }
                UserDefinedVerticalWindowRect.Value = r;
                return;
            }));

            Disposable.Add(targetWindowHandle.CombineLatest(
                MuteCondition,
                Observable.CombineLatest(
                    Observable.FromEventPattern<bool>(nativeWindowManager, nameof(nativeWindowManager.OnForeground))
                        .Select(x => x.EventArgs.ToDefaultableBooleanLike()).StartWith(DefaultableBooleanLike.Default),
                    Observable.FromEventPattern<bool>(nativeWindowManager, nameof(nativeWindowManager.OnMinimized))
                        .Select(x => x.EventArgs.ToDefaultableBooleanLike()).StartWith(DefaultableBooleanLike.Default)
                ).Select(x =>
                {
                    var maybeForeground = x[0];
                    var maybeMinimized = x[1];
                    return (maybeForeground, maybeMinimized) switch
                    {
                        (DefaultableBooleanLike.Default, DefaultableBooleanLike.Default) => ApplicationState.Foreground,
                        (DefaultableBooleanLike.Default, DefaultableBooleanLike.True) => ApplicationState.Minimized,
                        (DefaultableBooleanLike.Default, DefaultableBooleanLike.False) => ApplicationState.Background,
                        (DefaultableBooleanLike.True, DefaultableBooleanLike.Default) => ApplicationState.Foreground,
                        (DefaultableBooleanLike.True, DefaultableBooleanLike.True) => ApplicationState.Minimized,
                        (DefaultableBooleanLike.True, DefaultableBooleanLike.False) => ApplicationState.Foreground,
                        (DefaultableBooleanLike.False, DefaultableBooleanLike.Default) => ApplicationState.Background,
                        (DefaultableBooleanLike.False, DefaultableBooleanLike.True) => ApplicationState.Minimized,
                        (DefaultableBooleanLike.False, DefaultableBooleanLike.False) => ApplicationState.Background
                    };
                })
            )
            .Subscribe(x =>
            {
                var (handle, condition, state) = x;
                audioManager.SetMute(handle, condition.ToIsMute(state));
            }));

            Disposable.Add(windowRect.DistinctUntilChanged().CombineLatest(targetWindowHandle, Vertical.CombineLatest(WindowFittingStandard, UserDefinedVerticalWindowRect), Horizontal.CombineLatest(UserDefinedHorizontalWindowRect))
                .Where(x => x.Second != IntPtr.Zero)
                .Subscribe(x =>
                {
                    var containsScreen = screenManager.GetScreens()
                        .Where(s => s.ContainsWindow(x.First))
                        .Cast<Screen?>()
                        .FirstOrDefault();
                    if (containsScreen == null)
                    {
                        return;
                    }
                    switch (x.First.Direction)
                    {
                        case WindowDirection.Horizontal:
                            {
                                var (axis, userDefinedRect) = x.Fourth;
                                if (axis == AxisStandard.Application)
                                {
                                    return;
                                }
                                if (axis == AxisStandard.User)
                                {
                                    if (userDefinedRect.IsEmpty)
                                    {
                                        return;
                                    }
                                    nativeWindowManager.ResizeWindow(x.Second, userDefinedRect);
                                    return;
                                }

                                // Now supports Full Only
                                nativeWindowManager.ResizeWindow(x.Second, containsScreen.Value.MaxContainerbleWindowRect(x.First, Models.WindowFittingStandard.LeftTop /* 使わないので固定値 */));

                                return;
                            }
                        case WindowDirection.Vertical:
                            {
                                var (axis, fittingStandard, userDefinedRect) = x.Third;
                                switch (axis)
                                {
                                    case AxisStandard.Application:
                                        return;
                                    case AxisStandard.User:
                                        {
                                            if (userDefinedRect.IsEmpty)
                                            {
                                                return;
                                            }
                                            nativeWindowManager.ResizeWindow(x.Second, userDefinedRect);
                                            return;
                                        }
                                    case AxisStandard.Full:
                                        nativeWindowManager.ResizeWindow(x.Second, containsScreen.Value.MaxContainerbleWindowRect(x.First, fittingStandard));
                                        return;
                                    default:
                                        return;
                                }
                            }
                    }
                }));

            Disposable.Add(
                Observable.FromAsync<string>(() => versionRepository.GetLatestVersion()).Subscribe(v => LatestVersion.Value = v)
            );

            OnExit = () =>
            {
                settingService.Save();
            };
        }
    }
}
