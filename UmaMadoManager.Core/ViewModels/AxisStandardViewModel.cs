using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using UmaMadoManager.Core.Models;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Core.ViewModels
{
    public class AxisStandardViewModel
    {
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        private AxisStandardSettings axisStandardSettings = new AxisStandardSettings();
        public ReactiveProperty<AxisStandard> Vertical { get; }
        public ReactiveProperty<AxisStandard> Horizontal { get; }

        public ReactiveProperty<MuteCondition> MuteCondition { get; }

        private ReadOnlyReactiveProperty<IntPtr> targetWindowHandle;

        // FIXME: VMでやることじゃない
        public AxisStandardViewModel(
            INativeWindowManager nativeWindowManager,
            IScreenManager screenManager,
            IAudioManager audioManager)
        {
            Vertical = axisStandardSettings.Vertical;
            Horizontal = axisStandardSettings.Horizontal;
            MuteCondition = new ReactiveProperty<MuteCondition>(Models.MuteCondition.Nop);

            // FIXME: PollingじゃなくてGlobalHookとかでやりたい
            targetWindowHandle = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(x => nativeWindowManager.GetWindowHandle("umamusume"))
                .Distinct()
                .ToReadOnlyReactiveProperty();

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
                switch ((condition, state))
                {
                    case (_, ApplicationState.Foreground):
                        audioManager.SetMute(handle, false);
                        return;
                    case (Models.MuteCondition.WhenBackground, ApplicationState.Background):
                    case (Models.MuteCondition.WhenBackground, ApplicationState.Minimized):
                        audioManager.SetMute(handle, true);
                        return;
                    case (Models.MuteCondition.WhenMinimize, ApplicationState.Minimized):
                        audioManager.SetMute(handle, true);
                        return;
                    case (Models.MuteCondition.WhenMinimize, ApplicationState.Background):
                        audioManager.SetMute(handle, false);
                        return;
                    default:
                        return;
                }
            }));

            Disposable.Add(windowRect.CombineLatest(targetWindowHandle, Vertical, Horizontal)
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
                            if (x.Fourth == AxisStandard.Application)
                            {
                                return;
                            }
                            if (x.Fourth == AxisStandard.User)
                            {
                                return;
                            }

                            // Now supports Full Only
                            nativeWindowManager.ResizeWindow(x.Second, containsScreen.Value.MaxContainerbleWindowRect(x.First));

                            return;
                        case WindowDirection.Vertical:
                            if (x.Third == AxisStandard.Application)
                            {
                                return;
                            }
                            if (x.Third == AxisStandard.User)
                            {
                                return;
                            }

                            // Now supports Full Only
                            nativeWindowManager.ResizeWindow(x.Second, containsScreen.Value.MaxContainerbleWindowRect(x.First));

                            return;
                    }
                }));
        }
    }
}
