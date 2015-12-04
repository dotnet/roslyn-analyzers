using System;
using CsvHelper.TypeConversion;

namespace FileIssues
{
    /// <summary>
    /// Class that adjusts the string value of the "Dependency" column in the CSV file
    /// before setting the <see cref="PortingInfo.Dependency"/> property.
    /// </summary>
    internal class DependencyConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return type == typeof(string);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = Resources.DependencyNone;
            }

            return text;
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            throw new NotImplementedException();
        }
    }
}