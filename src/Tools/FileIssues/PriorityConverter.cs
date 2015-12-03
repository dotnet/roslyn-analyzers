using System;
using CsvHelper.TypeConversion;

namespace FileIssues
{
    /// <summary>
    /// Class that converts the string values of the "Original Priority" and
    /// "Revised Priority" columns in the CSV file into values for the
    /// <see cref="PortingInfo.OriginalPriority"/> and <see cref="PortingInfo.RevisedPriority"/>
    /// properties.
    /// </summary>
    internal class PriorityConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return false;
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            Priority priority = Priority.None;

            if (text.StartsWith("low", StringComparison.InvariantCultureIgnoreCase))
            {
                priority = Priority.Low;
            }
            else if (text.StartsWith("high", StringComparison.InvariantCultureIgnoreCase))
            {
                priority = Priority.High;
            }

            return priority;
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            throw new NotImplementedException();
        }
    }
}