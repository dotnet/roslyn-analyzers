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

        private DisposeAbstractValue(IOperation disposingOperation, DisposeAbstractValueKind kind)
            : this(ImmutableHashSet.Create(disposingOperation), kind)
        {
        }

        public DisposeAbstractValue(ImmutableHashSet<IOperation> disposingOperations, DisposeAbstractValueKind kind)
        {
            VerifyArguments(disposingOperations, kind);
            DisposingOperations = disposingOperations;
            Kind = kind;
        }

        public DisposeAbstractValue WithNewDisposingOperation(IOperation disposingOperation)
        {
            Debug.Assert(Kind != DisposeAbstractValueKind.NotDisposable);

            return new DisposeAbstractValue(DisposingOperations.Add(disposingOperation), DisposeAbstractValueKind.Disposed);
        }

        public DisposeAbstractValue WithNewEscapingOperation(IOperation escapingOperation)
        {
            Debug.Assert(Kind != DisposeAbstractValueKind.NotDisposable);

            return new DisposeAbstractValue(DisposingOperations.Add(escapingOperation), DisposeAbstractValueKind.MaybeDisposed);
        }

        [Conditional("DEBUG")]
        private static void VerifyArguments(ImmutableHashSet<IOperation> disposingOperations, DisposeAbstractValueKind kind)
        {
            Debug.Assert(disposingOperations != null);

            switch (kind)
            {
                case DisposeAbstractValueKind.NotDisposable:
                case DisposeAbstractValueKind.NotDisposed:
                    Debug.Assert(disposingOperations.Count == 0);
                    break;

                case DisposeAbstractValueKind.Disposed:
                    Debug.Assert(disposingOperations.Count > 0);
                    break;
            }
        }

        public ImmutableHashSet<IOperation> DisposingOperations { get; }
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
                DisposingOperations.SetEquals(other.DisposingOperations);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DisposeAbstractValue);
        }

        public override int GetHashCode()
        {
            int hashCode = HashUtilities.Combine(Kind.GetHashCode(), DisposingOperations.Count.GetHashCode());
            foreach (var operation in DisposingOperations)
            {
                hashCode = HashUtilities.Combine(operation.GetHashCode(), hashCode);
            }

            return hashCode;
        }
    }
}
