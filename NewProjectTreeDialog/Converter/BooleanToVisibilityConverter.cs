namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog
{
    using System;
    using System.Globalization;
    using System.Windows;

    class BooleanToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility falseVisibility = Visibility.Collapsed; ;
            if (parameter != null)
            {
                Visibility v = Visibility.Collapsed;
                if (parameter is Visibility)
                {
                    falseVisibility = (Visibility)parameter;
                }
                else if (Enum.TryParse<Visibility>(parameter.ToString(), out v))
                {
                    falseVisibility = v;
                }
            }

            if (value is bool)
            {
                return (bool)value ? Visibility.Visible : falseVisibility;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            return false;
        }
    }
}
