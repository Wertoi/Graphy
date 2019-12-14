using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphy.Attribute;
using System.Windows.Data;

namespace Graphy.Converter
{
    public class LocalizedEnumToStringConverter : IValueConverter
    {
        public LocalizedEnumToStringConverter()
        {
            
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.Empty;
            if (value is System.Enum enumValue)
            {
                var type = enumValue.GetType();

                // Look for our 'LocalizationKeyAttribute' in the field's custom attributes
                var field = type.GetField(enumValue.ToString());
                var key = string.Empty;
                if (field.GetCustomAttributes(typeof(LocalizationKeyAttribute), false) is LocalizationKeyAttribute[] attributes && attributes.Length > 0)
                {
                    key = attributes[0].LocKey;
                }

                result = string.IsNullOrWhiteSpace(key) ? string.Empty : TranslationSource.Instance.ResourceManager.GetString(key, TranslationSource.Instance.CurrentCulture);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
