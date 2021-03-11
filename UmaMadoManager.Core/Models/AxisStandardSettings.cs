using System;
using Reactive.Bindings;

namespace UmaMadoManager.Core.Models
{
    public class AxisStandardSettings
    {
        public ReactiveProperty<AxisStandard> Vertical { get; }
        public ReactiveProperty<AxisStandard> Horizontal { get; }

        public AxisStandardSettings()
        {
            Vertical = new ReactiveProperty<AxisStandard>(AxisStandard.Application);
            Horizontal = new ReactiveProperty<AxisStandard>(AxisStandard.Application);
        }
    }
}
