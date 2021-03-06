using System;

namespace UmaMadoManager.Core.Models
{
    public enum MuteCondition
    {
        Nop,
        Always,
        WhenBackground,
        WhenMinimize
    }

    public static class MuteConditionExtensions
    {
        public static bool ToIsMute(this MuteCondition self, ApplicationState currentState)
        {
            return (self, currentState) switch
            {
                (MuteCondition.Nop, _) => false,
                (MuteCondition.Always, _) => true,
                (_, ApplicationState.Foreground) => false,
                (MuteCondition.WhenBackground, ApplicationState.Background or ApplicationState.Minimized) => true,
                (MuteCondition.WhenMinimize, ApplicationState.Minimized) => true,
                (MuteCondition.WhenMinimize, ApplicationState.Background) => false,
            };
        }
    }
}
