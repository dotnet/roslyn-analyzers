using System;
using CsvHelper.TypeConversion;

namespace FileIssues
{
    /// <summary>
    /// Class that converts the string value of the "Port?" column in the CSV file
    /// into a value for the <see cref="PortingInfo.Disposition"/> property.
    /// </summary>
    internal class DispositionConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return type == typeof(Disposition);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            Disposition disposition = Disposition.Unknown;

            if (string.IsNullOrWhiteSpace(text))
            {
                disposition = Disposition.NeedsReview;
            }
            else if (string.Compare(text, "yes", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                disposition = Disposition.Port;
            }

            return disposition;
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            throw new NotImplementedException();
        }
    }
}