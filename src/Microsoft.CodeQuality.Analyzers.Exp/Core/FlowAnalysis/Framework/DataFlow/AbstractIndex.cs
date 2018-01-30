// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Represents an abstract index into a location.
    /// It is used by an <see cref="AnalysisEntity"/> for operations such as an <see cref="IArrayElementReferenceOperation"/>, index access <see cref="IPropertyReferenceOperation"/>, etc.
    /// </summary>
    internal abstract partial class AbstractIndex : IEquatable<AbstractIndex>
    {
        public static AbstractIndex Create(uint index) => new ConstantValueIndex(index);
        public static AbstractIndex Create(AnalysisEntity analysisEntity) => new AnalysisEntityBasedIndex(analysisEntity);
        public static AbstractIndex Create(IOperation operation) => new OperationBasedIndex(operation);

        public override bool Equals(object obj)
        {
            return Equals(obj as AbstractIndex);
        }

        public static bool operator ==(AbstractIndex value1, AbstractIndex value2)
        {
            if ((object)value1 == null)
            {
                return (object)value2 == null;
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(AbstractIndex value1, AbstractIndex value2)
        {
            return !(value1 == value2);
        }

        public abstract bool Equals(AbstractIndex other);

        public override abstract int GetHashCode();
    }
}
