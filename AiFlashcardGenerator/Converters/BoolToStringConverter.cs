using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFlashcardGenerator.Converters
{
    /// <summary>
    /// A custom IValueConverter to convert a boolean to one of two strings 
    /// based on a pipe-separated parameter: 'FalseString|TrueString'.
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                // The parameter should be in the format 'FalseValue|TrueValue'
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    // If true, return parts[1] (the TrueValue). If false, return parts[0] (the FalseValue).
                    return boolValue ? parts[1].Trim() : parts[0].Trim();
                }
            }
            // Fallback for non-boolean input or invalid parameter format
            return value?.ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Conversion back from string to bool is not supported for this use case
            throw new NotSupportedException("Conversion from string to bool is not supported by this converter.");
        }
    }
}
