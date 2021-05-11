using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using UmaMadoManager.Core.Extensions;

namespace UmaMadoManager.Windows.Converters
{
    class CurrentVersionEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == "")
            {
                return System.Windows.Visibility.Collapsed;
            }
            var currentVersionWithoutRevision = Assembly.GetExecutingAssembly().GetName().Version.Let(currentVersion => {
                return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
            });
            var formattedVersion = Version.Parse(value as string);
            return currentVersionWithoutRevision != formattedVersion ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
