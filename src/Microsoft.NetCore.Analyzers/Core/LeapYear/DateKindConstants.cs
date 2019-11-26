// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.LeapYear
{
    public static class DateKindConstants
    {
        public const string DateTimeQualifiedName = "System.DateTime";
        public const string DateTimeOffsetQualifiedName = "System.DateTimeOffset";
        public const string IntQualifiedName = "int";
        public const string YearParameterIdentifer = "year";
        public const string MonthParameterIdentifier = "month";
        public const string DayParameterIndentifier = "day";

        public static readonly string[] DateKindArgumentIdentifiers = { YearParameterIdentifer, MonthParameterIdentifier, DayParameterIndentifier };
    }
}
