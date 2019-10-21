// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    /// <summary>
    /// Info for a tainted data sanitizer type, which makes tainted data untainted.
    /// </summary>
    internal sealed class SanitizerInfo : ITaintedDataInfo, IEquatable<SanitizerInfo>
    {
        public SanitizerInfo(string fullTypeName, bool isInterface, bool isConstructorSanitizing, ImmutableDictionary<string, (bool, bool, ImmutableHashSet<string>)> sanitizingMethods)
        {
            FullTypeName = fullTypeName ?? throw new ArgumentNullException(nameof(fullTypeName));
            IsInterface = isInterface;
            IsConstructorSanitizing = isConstructorSanitizing;
            SanitizingMethods = sanitizingMethods ?? throw new ArgumentNullException(nameof(sanitizingMethods));
        }

        /// <summary>
        /// Full type name of the...type (namespace + type).
        /// </summary>
        public string FullTypeName { get; }

        /// <summary>
        /// Indicates that this sanitizer type is an interface.
        /// </summary>
        public bool IsInterface { get; }

        /// <summary>
        /// Indicates that any tainted data entering a constructor becomes untainted.
        /// </summary>
        public bool IsConstructorSanitizing { get; }

        /// <summary>
        /// Methods that untaint one of the (return value, instance, arguments).
        /// </summary>
        public ImmutableDictionary<string, (bool SanitizeReturn, bool SanitizeInstance, ImmutableHashSet<string> SanitizedArguments)> SanitizingMethods { get; }

        /// <summary>
        /// Indicates that this <see cref="SanitizerInfo"/> uses <see cref="ValueContentAbstractValue"/>s.
        /// </summary>
        public bool RequiresValueContentAnalysis => false;

        public override int GetHashCode()
        {
            return HashUtilities.Combine(this.SanitizingMethods,
                HashUtilities.Combine(StringComparer.Ordinal.GetHashCode(this.FullTypeName),
                this.IsConstructorSanitizing.GetHashCode()));
        }

        public override bool Equals(object obj)
        {
            return obj is SanitizerInfo other ? this.Equals(other) : false;
        }

        public bool Equals(SanitizerInfo other)
        {
            return other != null
                && this.FullTypeName == other.FullTypeName
                && this.IsConstructorSanitizing == other.IsConstructorSanitizing
                && this.SanitizingMethods == other.SanitizingMethods;
        }
    }
}
