// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
    public static class LocationExtensions
    {
        /// <summary>
        /// Returns the first location at which a symbol is declared.
        /// </summary>
        /// <param name="symbol">
        /// The symbol whose location is desired.
        /// </param>
        /// <returns>
        /// The first location at which <paramref name="symbol"/>, or null
        /// if there are no locations.
        /// </returns>
        /// <remarks>
        /// Many analyzers arbitrarily choose to report the first location when
        /// reporting a diagnostic on a symbol. This convenience method make it
        /// easier.
        /// </remarks>
        public static Location FirstLocation(this ISymbol symbol)
        {
            return symbol.Locations.FirstOrDefault();
        }
    }
}
