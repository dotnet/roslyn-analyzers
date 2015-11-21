using System;
using CsvHelper.TypeConversion;

namespace FileIssues
{
    /// <summary>
    /// Class that converts the string value of the "Port?" column in the CSV file
    /// into a Boolean value for the <see cref="PortingInfo.ShouldPort"/> property.
    /// </summary>
    internal class ShouldPortConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return type == typeof(bool);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            return string.Compare(text, "yes", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            throw new NotImplementedException();
        }
    }
}