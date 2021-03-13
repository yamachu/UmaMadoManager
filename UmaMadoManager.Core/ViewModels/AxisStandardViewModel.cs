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
                    Observable.FromEventPattern<bool>(nativeWindowManager, nameof(nativeWindowManager.OnForeground)).Select(x => x.EventArgs).StartWith(false),
                    Observable.FromEventPattern<bool>(nativeWindowManager, nameof(nativeWindowManager.OnMinimized)).Select(x => x.EventArgs).StartWith(false)
                ).Select(x => {
                    var first = x[0];
                    var second = x[1];
                    return (first, second) switch {
                        (true, true) => ApplicationState.Minimized,
                        (false, true) => ApplicationState.Minimized,
                        (true, false) => ApplicationState.Foreground,
                        (false, false) => ApplicationState.Background
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
