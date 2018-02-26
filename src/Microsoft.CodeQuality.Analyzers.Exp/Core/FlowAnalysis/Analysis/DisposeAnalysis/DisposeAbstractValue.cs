// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis
{
    /// <summary>
    /// Abstract dispose data tracked by <see cref="DisposeAnalysis"/>.
    /// It contains the set of <see cref="IOperation"/>s that dispose an associated disposable <see cref="AbstractLocation"/> and
    /// the dispose <see cref="Kind"/>.
    /// </summary>
    internal class DisposeAbstractValue : IEquatable<DisposeAbstractValue>
    {
        public static readonly DisposeAbstractValue NotDisposable = new DisposeAbstractValue(DisposeAbstractValueKind.NotDisposable);
        public static readonly DisposeAbstractValue NotDisposed = new DisposeAbstractValue(DisposeAbstractValueKind.NotDisposed);
        public static readonly DisposeAbstractValue Unknown = new DisposeAbstractValue(DisposeAbstractValueKind.MaybeDisposed);

        private DisposeAbstractValue(DisposeAbstractValueKind kind)
            : this(ImmutableHashSet<IOperation>.Empty, kind)
        {
            Debug.Assert(kind != DisposeAbstractValueKind.Disposed);
        }

        private DisposeAbstractValue(IOperation disposingOrEscapingOperation, DisposeAbstractValueKind kind)
            : this(ImmutableHashSet.Create(disposingOrEscapingOperation), kind)
        {
        }

        public DisposeAbstractValue(ImmutableHashSet<IOperation> disposingOrEscapingOperations, DisposeAbstractValueKind kind)
        {
            VerifyArguments(disposingOrEscapingOperations, kind);
            DisposingOrEscapingOperations = disposingOrEscapingOperations;
            Kind = kind;
        }

        public DisposeAbstractValue WithNewDisposingOperation(IOperation disposingOperation)
        {
            Debug.Assert(Kind != DisposeAbstractValueKind.NotDisposable);

            return new DisposeAbstractValue(DisposingOrEscapingOperations.Add(disposingOperation), DisposeAbstractValueKind.Disposed);
        }

        public DisposeAbstractValue WithNewEscapingOperation(IOperation escapingOperation)
        {
            Debug.Assert(Kind != DisposeAbstractValueKind.NotDisposable);

            return new DisposeAbstractValue(DisposingOrEscapingOperations.Add(escapingOperation), DisposeAbstractValueKind.MaybeDisposed);
        }

        [Conditional("DEBUG")]
        private static void VerifyArguments(ImmutableHashSet<IOperation> disposingOrEscapingOperations, DisposeAbstractValueKind kind)
        {
            Debug.Assert(disposingOrEscapingOperations != null);

            switch (kind)
            {
                case DisposeAbstractValueKind.NotDisposable:
                case DisposeAbstractValueKind.NotDisposed:
                    Debug.Assert(disposingOrEscapingOperations.Count == 0);
                    break;

                case DisposeAbstractValueKind.Disposed:
                    Debug.Assert(disposingOrEscapingOperations.Count > 0);
                    break;
            }
        }

        public ImmutableHashSet<IOperation> DisposingOrEscapingOperations { get; }
        public DisposeAbstractValueKind Kind { get; }

        public static bool operator ==(DisposeAbstractValue value1, DisposeAbstractValue value2)
        {
            if ((object)value1 == null)
            {
                return (object)value2 == null;
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(DisposeAbstractValue value1, DisposeAbstractValue value2)
        {
            return !(value1 == value2);
        }

        public bool Equals(DisposeAbstractValue other)
        {
            return other != null &&
                Kind == other.Kind &&
                DisposingOrEscapingOperations.SetEquals(other.DisposingOrEscapingOperations);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DisposeAbstractValue);
        }

        public override int GetHashCode()
        {
            int hashCode = HashUtilities.Combine(Kind.GetHashCode(), DisposingOrEscapingOperations.Count.GetHashCode());
            foreach (var operation in DisposingOrEscapingOperations)
            {
                hashCode = HashUtilities.Combine(operation.GetHashCode(), hashCode);
            }

            return hashCode;
        }
    }
}
