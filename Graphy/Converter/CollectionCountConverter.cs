using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Graphy.Converter
{
    public class CollectionCountConverter<T> : IValueConverter
    {
        public CollectionCountConverter(T emptyValue, T nonEmptyValue)
        {
            EmptyValue = emptyValue;
            NonEmptyValue = nonEmptyValue;
        }

        public T EmptyValue { get; set; }
        public T NonEmptyValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && (int)value != 0 ? NonEmptyValue : EmptyValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, EmptyValue);
        }
    }

    public sealed class CollectionCountToVisibiltyConverter : CollectionCountConverter<Visibility>
    {
        public CollectionCountToVisibiltyConverter():
            base(Visibility.Visible, Visibility.Collapsed)
        {

        }
    }
}
