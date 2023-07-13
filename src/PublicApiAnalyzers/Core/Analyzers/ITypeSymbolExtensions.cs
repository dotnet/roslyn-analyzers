﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Lightup
{
    internal static class ITypeSymbolExtensions
    {
        private static readonly Func<ITypeSymbol, NullableAnnotation> s_nullableAnnotation
            = LightupHelpers.CreateSymbolPropertyAccessor<ITypeSymbol, NullableAnnotation>(typeof(ITypeSymbol), nameof(NullableAnnotation), fallbackResult: Lightup.NullableAnnotation.None);

        public static NullableAnnotation NullableAnnotation(this ITypeSymbol typeSymbol)
            => s_nullableAnnotation(typeSymbol);
    }
}
