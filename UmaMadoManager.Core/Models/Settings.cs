using System;
using System.Configuration;

namespace UmaMadoManager.Core.Models
{
    public class Settings : ApplicationSettingsBase
    {
        // https://social.msdn.microsoft.com/Forums/ja-JP/3b5560a0-0626-4843-bd51-5fbee8eb61db/1245012503125221246512540124711251912531353732345012398-upgrade?forum=csharpgeneralja
        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValue("false")]
        public bool IsUpgrated
        {
            get { return (bool)this["IsUpgrated"]; }
            set { this["IsUpgrated"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValue("false")]
        public bool UseCurrentVerticalUserSetting
        {
            get { return (bool)this["UseCurrentVerticalUserSetting"]; }
            set { this["UseCurrentVerticalUserSetting"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValue("false")]
        public bool UseCurrentHorizontalUserSetting
        {
            get { return (bool)this["UseCurrentHorizontalUserSetting"]; }
            set { this["UseCurrentHorizontalUserSetting"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValue("umamusume")]
        public string TargetApplicationName
        {
            get { return (string)this["TargetApplicationName"]; }
            set { this["TargetApplicationName"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public MuteCondition MuteCondition
        {
            get { return this["MuteCondition"] == null ? MuteCondition.Nop : (MuteCondition)this["MuteCondition"]; }
            set { this["MuteCondition"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public AxisStandard Vertical
        {
            get { return this["Vertical"] == null ? AxisStandard.Application : (AxisStandard)this["Vertical"]; }
            set { this["Vertical"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public AxisStandard Horizontal
        {
            get { return this["Horizontal"] == null ? AxisStandard.Application : (AxisStandard)this["Horizontal"]; }
            set { this["Horizontal"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public WindowRect UserDefinedVerticalWindowRect
        {
            get { return this["UserDefinedVerticalWindowRect"] == null ? WindowRect.Empty : (WindowRect)this["UserDefinedVerticalWindowRect"]; }
            set { this["UserDefinedVerticalWindowRect"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public WindowRect UserDefinedHorizontalWindowRect
        {
            get { return this["UserDefinedHorizontalWindowRect"] == null ? WindowRect.Empty : (WindowRect)this["UserDefinedHorizontalWindowRect"]; }
            set { this["UserDefinedHorizontalWindowRect"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        public WindowFittingStandard WindowFittingStandard
        {
            get { return this["WindowFittingStandard"] == null ? WindowFittingStandard.LeftTop : (WindowFittingStandard)this["WindowFittingStandard"]; }
            set { this["WindowFittingStandard"] = value; }
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Configuration.DefaultSettingValue("false")]
        public bool IsMostTop
        {
            get { return (bool)this["IsMostTop"]; }
            set { this["IsMostTop"] = value; }
        }
    }
}
