using System;
using System.Linq;
using System.Reactive.Concurrency;
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
        public ReactiveProperty<bool> IsMostTop { get; }

        public ReactiveProperty<bool> IsRemoveBorder { get; }

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
        public Action OnAllocateDebugConsoleClicked;

        // FIXME: VMでやることじゃない
        public AxisStandardViewModel(
            INativeWindowManager nativeWindowManager,
            IScreenManager screenManager,
            IAudioManager audioManager,
            IVersionRepository versionRepository,
            ISettingService settingService)
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
            IsMostTop = BindSettings(settings.IsMostTop, nameof(settings.IsMostTop));
            IsRemoveBorder = BindSettings(settings.IsRemoveBorder, nameof(settings.IsRemoveBorder));

            // FIXME: PollingじゃなくてGlobalHookとかでやりたい
            targetWindowHandle = Observable.Interval(TimeSpan.FromSeconds(1))
                .CombineLatest(TargetApplicationName)
                .Select(x => nativeWindowManager.GetWindowHandle(x.Second))
                .Distinct()
                .ToReadOnlyReactiveProperty();

            // FIXME: TargetApplicationNameが変わってもThreadが変わって動かなくなるわ…
            Disposable.Add(TargetApplicationName.Subscribe(x => nativeWindowManager.SetHook(x)));
            Disposable.Add(targetWindowHandle.Subscribe(x => nativeWindowManager.SetTargetProcessHandler(x)));

            var observableBorderChanged = Observable.FromEventPattern(nativeWindowManager, nameof(nativeWindowManager.OnBorderChanged)).StartWith(new object[] { null });
            var observableOnMoveChanged = Observable.FromEventPattern(nativeWindowManager, nameof(nativeWindowManager.OnMoveOrSizeChanged)).Throttle(TimeSpan.FromMilliseconds(200)).StartWith(new object[] { null });

            var windowRect = targetWindowHandle
                .CombineLatest(
                    observableOnMoveChanged,
                    observableBorderChanged.Delay(TimeSpan.FromMilliseconds(500))
                )
                .Select(x =>
                {
                    if (x.First == IntPtr.Zero)
                    {
                        return (WindowRect.Empty, WindowRect.Empty);
                    }
                    var windowClientRectPair = nativeWindowManager.GetWindowRect(x.First);
                    var (r, _) = windowClientRectPair;
                    if (r.IsEmpty)
                    {
                        return (WindowRect.Empty, WindowRect.Empty);
                    }
                    return windowClientRectPair;
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
                var (r, _) = nativeWindowManager.GetWindowRect(handle);
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
                var (r, _) = nativeWindowManager.GetWindowRect(handle);
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
                )
                .DistinctUntilChanged()
                .Select(x =>
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
            .DistinctUntilChanged()
            .ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(x =>
            {
                var (handle, condition, state) = x;
                audioManager.SetMute(handle, condition.ToIsMute(state));
            }));

            Disposable.Add(windowRect.DistinctUntilChanged().CombineLatest(targetWindowHandle, Vertical.CombineLatest(WindowFittingStandard, UserDefinedVerticalWindowRect), Horizontal.CombineLatest(UserDefinedHorizontalWindowRect), IsRemoveBorder)
                .Where(x => x.Second != IntPtr.Zero)
                .Subscribe(x =>
                {
                    var containsScreen = screenManager.GetScreens()
                        .Where(s => s.ContainsWindow(x.First.Item1))
                        .Cast<Screen?>()
                        .FirstOrDefault();
                    if (containsScreen == null)
                    {
                        return;
                    }
                    System.Console.WriteLine("--- start current Window Rect ---");
                    System.Console.WriteLine(x.First);
                    System.Console.WriteLine("--- end current Window Rect ---");
                    switch (x.First.Item1.Direction)
                    {
                        case WindowDirection.Horizontal:
                            {
                                var (axis, userDefinedRect) = x.Fourth;
                                switch (axis)
                                {
                                    case AxisStandard.Application:
                                        nativeWindowManager.RemoveBorder(x.Second, false);
                                        return;
                                    case AxisStandard.User:
                                        {
                                            nativeWindowManager.RemoveBorder(x.Second, false);
                                            if (userDefinedRect.IsEmpty)
                                            {
                                                return;
                                            }
                                            nativeWindowManager.ResizeWindow(x.Second, userDefinedRect);
                                            return;
                                        }
                                    case AxisStandard.Full:
                                        {
                                            var nextSize = containsScreen.Value.MaxContainerbleWindowRect(x.Fifth ? x.First.Item2 : x.First.Item1, x.First.Item2, Models.WindowFittingStandard.LeftTop /* 使わないので固定値 */);
                                            System.Console.WriteLine("--- start next Window Rect ---");
                                            System.Console.WriteLine(nextSize);
                                            System.Console.WriteLine("--- end next Window Rect ---");
                                            nativeWindowManager.RemoveBorder(x.Second, x.Fifth);
                                            nativeWindowManager.ResizeWindow(x.Second, nextSize);
                                            return;
                                        }
                                    default:
                                        return;
                                }
                            }
                        case WindowDirection.Vertical:
                            {
                                var (axis, fittingStandard, userDefinedRect) = x.Third;
                                switch (axis)
                                {
                                    case AxisStandard.Application:
                                        nativeWindowManager.RemoveBorder(x.Second, false);
                                        return;
                                    case AxisStandard.User:
                                        {
                                            nativeWindowManager.RemoveBorder(x.Second, false);
                                            if (userDefinedRect.IsEmpty)
                                            {
                                                return;
                                            }
                                            nativeWindowManager.ResizeWindow(x.Second, userDefinedRect);
                                            return;
                                        }
                                    case AxisStandard.Full:
                                        {
                                            var nextSize = containsScreen.Value.MaxContainerbleWindowRect(x.Fifth ? x.First.Item2 : x.First.Item1, x.First.Item2, fittingStandard);
                                            System.Console.WriteLine("--- start next Window Rect ---");
                                            System.Console.WriteLine(nextSize);
                                            System.Console.WriteLine("--- end next Window Rect ---");
                                            nativeWindowManager.RemoveBorder(x.Second, x.Fifth);
                                            nativeWindowManager.ResizeWindow(x.Second, nextSize);
                                            return;
                                        }
                                    default:
                                        return;
                                }
                            }
                    }
                }));

            Disposable.Add(
                Observable.FromAsync<string>(() => versionRepository.GetLatestVersion()).Subscribe(v => LatestVersion.Value = v)
            );

            Disposable.Add(targetWindowHandle.Where(x => x != IntPtr.Zero).CombineLatest(IsMostTop, windowRect.DistinctUntilChanged()).Subscribe(x =>
            {
                var (handle, doTop, _) = x;
                nativeWindowManager.SetTopMost(handle, doTop);
            }));

            OnExit = () =>
            {
                settingService.Save();
            };
        }
    }
}
