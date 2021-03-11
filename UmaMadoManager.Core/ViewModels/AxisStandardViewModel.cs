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

        private ReadOnlyReactiveProperty<IntPtr> targetWindowHandle;

        // FIXME: VMでやることじゃない
        public AxisStandardViewModel(INativeWindowManager nativeWindowManager)
        {
            Vertical = axisStandardSettings.Vertical;
            Horizontal = axisStandardSettings.Horizontal;

            // FIXME: PollingじゃなくてGlobalHookとかでやりたい
            targetWindowHandle = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(x => nativeWindowManager.GetWindowHandle("umamusume"))
                .Distinct()
                .ToReadOnlyReactiveProperty();

            var windowRect = targetWindowHandle.CombineLatest(Observable.Interval(TimeSpan.FromSeconds(1.5)))
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

            Disposable.Add(windowRect.CombineLatest(targetWindowHandle, Vertical, Horizontal)
            .Where(x => x.Second != IntPtr.Zero)
            .Subscribe(x =>
            {
                var containsScreen = nativeWindowManager.GetScreens()
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
