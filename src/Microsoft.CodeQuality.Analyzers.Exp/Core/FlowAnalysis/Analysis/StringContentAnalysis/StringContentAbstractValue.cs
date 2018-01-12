// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Analyzer.Utilities;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    /// <summary>
    /// Abstract string content data value for symbol/operation tracked by <see cref="StringContentAnalysis"/>.
    /// </summary>
    internal partial class StringContentAbstractValue : IEquatable<StringContentAbstractValue>
    {
        public static readonly StringContentAbstractValue DefaultMaybe = new StringContentAbstractValue();
        public static readonly StringContentAbstractValue DefaultNo = new StringContentAbstractValue(literalState: StringContainsState.No, nonLiteralState: StringContainsState.No);

        public StringContentAbstractValue(string literal)
        {
            LiteralState = StringContainsState.Yes;
            NonLiteralState = StringContainsState.No;
            LiteralValues = ImmutableHashSet.Create(literal);
            NonLiteralValues = ImmutableHashSet<IOperation>.Empty;
        }

        public StringContentAbstractValue(IOperation nonLiteral)
        {
            LiteralState = StringContainsState.No;
            NonLiteralState = StringContainsState.Yes;
            LiteralValues = ImmutableHashSet<string>.Empty;
            NonLiteralValues = ImmutableHashSet.Create(nonLiteral);
        }

        private StringContentAbstractValue(StringContainsState literalState = StringContainsState.Maybe, StringContainsState nonLiteralState = StringContainsState.Maybe)
            : this(literalState, nonLiteralState, ImmutableHashSet<string>.Empty, ImmutableHashSet<IOperation>.Empty)
        {
            Debug.Assert(literalState == StringContainsState.No || literalState == StringContainsState.Maybe);
            Debug.Assert(nonLiteralState == StringContainsState.No || nonLiteralState == StringContainsState.Maybe);
        }

        private StringContentAbstractValue(StringContainsState literalState, StringContainsState nonLiteralState, ImmutableHashSet<string> literalValues, ImmutableHashSet<IOperation> nonLiteralValues)
        {
            LiteralState = literalState;
            NonLiteralState = nonLiteralState;
            LiteralValues = literalValues;
            NonLiteralValues = nonLiteralValues;
        }

        /// <summary>
        /// Indicates if this string variable contains string literals or not.
        /// </summary>
        public StringContainsState LiteralState { get; }

        /// <summary>
        /// Indicates if this string variable contains non literal string operands or not.
        /// </summary>
        public StringContainsState NonLiteralState { get; }

        /// <summary>
        /// Gets a collection of the string literals that could possibly make up the contents of this string <see cref="Operand"/>.
        /// </summary>
        public ImmutableHashSet<string> LiteralValues { get; }

        /// <summary>
        /// Gets a collection of the non literal string operations that could possibly make up the contents of this string <see cref="Operand"/>.
        /// </summary>
        public ImmutableHashSet<IOperation> NonLiteralValues { get; }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as StringContentAbstractValue);
        }

        public bool Equals(StringContentAbstractValue other)
        {
            return other != null &&
                LiteralState == other.LiteralState &&
                NonLiteralState == other.NonLiteralState &&
                LiteralValues.Equals(other.LiteralValues) &&
                NonLiteralValues.Equals(other.NonLiteralValues);
        }

        public override int GetHashCode()
        {
            return
                HashUtilities.Combine(LiteralValues.GetHashCode(),
                HashUtilities.Combine(NonLiteralValues.GetHashCode(),
                HashUtilities.Combine(LiteralState.GetHashCode(), NonLiteralState.GetHashCode())));
        }

        /// <summary>
        /// Performs the union with this state and the other state 
        /// and returns a new <see cref="StringContentAbstractValue"/> with the result.
        /// </summary>
        public StringContentAbstractValue Merge(StringContentAbstractValue otherState)
        {
            // + Y M N
            // Y Y Y Y
            // M Y M M
            // N Y M N
            if (otherState == null)
            {
                throw new ArgumentNullException(nameof(otherState));
            }

            // Merge Literals
            StringContainsState mergedLiteralState;
            ImmutableHashSet<string> mergedLiteralValues;
            if (LiteralState == StringContainsState.Yes)
            {
                mergedLiteralState = StringContainsState.Yes;
                if (otherState.LiteralState == StringContainsState.Yes)
                {
                    // FxCop compat: Only merge literalValues if both states are Yes
                    mergedLiteralValues = LiteralValues.Union(otherState.LiteralValues);
                }
                else
                {
                    mergedLiteralValues = LiteralValues;
                }
            }
            else if (otherState.LiteralState == StringContainsState.Yes)
            {
                mergedLiteralState = StringContainsState.Yes;
                mergedLiteralValues = otherState.LiteralValues;
            }
            else if (LiteralState == StringContainsState.Maybe ||
                    otherState.LiteralState == StringContainsState.Maybe)
            {
                mergedLiteralState = StringContainsState.Maybe;
                mergedLiteralValues = ImmutableHashSet<string>.Empty;
            }
            else
            {
                mergedLiteralState = StringContainsState.No;
                mergedLiteralValues = ImmutableHashSet<string>.Empty;
            }

            // Merge NonLiterals state
            StringContainsState mergedNonLiteralState;
            ImmutableHashSet<IOperation> mergedNonLiteralValues;
            if (this.NonLiteralState == StringContainsState.Yes ||
                otherState.NonLiteralState == StringContainsState.Yes)
            {
                mergedNonLiteralState = StringContainsState.Yes;
                mergedNonLiteralValues = this.NonLiteralValues.Union(otherState.NonLiteralValues);
            }
            else if (this.NonLiteralState == StringContainsState.Maybe ||
                otherState.NonLiteralState == StringContainsState.Maybe)
            {
                mergedNonLiteralState = StringContainsState.Maybe;
                mergedNonLiteralValues = this.NonLiteralValues.Union(otherState.NonLiteralValues);
            }
            else
            {
                mergedNonLiteralState = StringContainsState.No;
                mergedNonLiteralValues = ImmutableHashSet<IOperation>.Empty;
            }

            return new StringContentAbstractValue(mergedLiteralState, mergedNonLiteralState, mergedLiteralValues, mergedNonLiteralValues);
        }

        /// <summary>
        /// Performs the union with this state and the other state 
        /// and returns a new <see cref="StringContentAbstractValue"/> with the result.
        /// </summary>
        public StringContentAbstractValue MergeBinaryAdd(StringContentAbstractValue otherState, IOperation binaryOperation)
        {
            // + Y M N
            // Y Y Y Y
            // M Y M M
            // N Y M N
            if (otherState == null)
            {
                throw new ArgumentNullException(nameof(otherState));
            }

            // Merge Literals
            StringContainsState mergedLiteralState;
            ImmutableHashSet<string> mergedLiteralValues;
            if (LiteralState == StringContainsState.Yes &&
                otherState.LiteralState == StringContainsState.Yes)
            {
                mergedLiteralState = StringContainsState.Yes;

                var builder = ImmutableHashSet.CreateBuilder<string>();
                foreach (var leftLiteral in LiteralValues)
                {
                    foreach (var rightLiteral in otherState.LiteralValues)
                    {
                        builder.Add(leftLiteral + rightLiteral);
                    }
                }

                mergedLiteralValues = builder.ToImmutable();
            }
            else if (LiteralState != StringContainsState.No ||
                    otherState.LiteralState != StringContainsState.No)
            {
                mergedLiteralState = StringContainsState.Maybe;
                mergedLiteralValues = ImmutableHashSet<string>.Empty;
            }
            else
            {
                mergedLiteralState = StringContainsState.No;
                mergedLiteralValues = ImmutableHashSet<string>.Empty;
            }

            // Merge NonLiterals state
            StringContainsState mergedNonLiteralState;
            ImmutableHashSet<IOperation> mergedNonLiteralValues;
            if (this.NonLiteralState == StringContainsState.Yes ||
                this.NonLiteralState == StringContainsState.Yes)
            {
                mergedNonLiteralState = StringContainsState.Yes;
                mergedNonLiteralValues = ImmutableHashSet.Create(binaryOperation);
            }
            else if (this.NonLiteralState == StringContainsState.Maybe ||
                otherState.NonLiteralState == StringContainsState.Maybe)
            {
                mergedNonLiteralState = StringContainsState.Maybe;
                mergedNonLiteralValues = ImmutableHashSet.Create(binaryOperation);
            }
            else
            {
                mergedNonLiteralState = StringContainsState.No;
                mergedNonLiteralValues = ImmutableHashSet<IOperation>.Empty;
            }

            return new StringContentAbstractValue(mergedLiteralState, mergedNonLiteralState, mergedLiteralValues, mergedNonLiteralValues);
        }

        /// <summary>
        /// Returns a string representation of <see cref="StringContentsState"/>.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "L:{0}({1}) NL:{2}({3})",
                LiteralState.ToString()[0], LiteralValues.Count,
                NonLiteralState.ToString()[0], NonLiteralValues.Count);
        }
    }
}
