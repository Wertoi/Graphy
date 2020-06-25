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
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }

    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        {
            
        }
    }

    public sealed class BooleanToBooleanConverter : BooleanConverter<bool>
    {
        public BooleanToBooleanConverter() :
            base(false, true)
        {

        }
    }

    public sealed class BooleanToFontWeightConverter : BooleanConverter<FontWeight>
    {
        public BooleanToFontWeightConverter() :
            base(FontWeights.Bold, FontWeights.Normal)
        {

        }
    }

    public sealed class BooleanToFontStyleConverter : BooleanConverter<FontStyle>
    {
        public BooleanToFontStyleConverter() :
            base(FontStyles.Italic, FontStyles.Normal)
        {

        }
    }

    public sealed class BooleanToTextDecorationConverter : BooleanConverter<TextDecorationCollection>
    {
        public BooleanToTextDecorationConverter() :
            base(TextDecorations.Underline, null)
        {

        }
    }
}
