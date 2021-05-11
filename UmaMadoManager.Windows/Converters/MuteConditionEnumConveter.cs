using System;
using System.Globalization;
using System.Windows.Data;
using UmaMadoManager.Core.Models;

namespace UmaMadoManager.Windows.Converters
{
    class MuteConditionEnumConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var targetEnum = (MuteCondition)parameter;
            var passedEnum = (MuteCondition)value;

            return targetEnum == passedEnum;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
