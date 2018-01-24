// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    internal class SetAbstractDomain<T> : AbstractDomain<ISet<T>>
    {
        public static SetAbstractDomain<T> Default = new SetAbstractDomain<T>();

        public override ISet<T> Bottom => new HashSet<T>();

        public override int Compare(ISet<T> oldValue, ISet<T> newValue)
        {
            if ((oldValue == null && newValue != null) ||
                (oldValue != null && newValue == null))
                return -1;

            if (oldValue == null && newValue == null) return 0;
            if (ReferenceEquals(oldValue, newValue)) return 0;

            int result;
            var isSubset = oldValue.IsSubsetOf(newValue);

            if (isSubset &&
                oldValue.Count == newValue.Count)
            {
                // oldValue == newValue
                result = 0;
            }
            else if (isSubset)
            {
                // oldValue < newValue
                result = -1;
            }
            else
            {
                // oldValue > newValue
                result = 1;
            }

            return result;
        }

        public override ISet<T> Merge(ISet<T> value1, ISet<T> value2)
        {
            if (value1 == null && value2 != null) return value2;
            if (value1 != null && value2 == null) return value1;
            if (value1 == null && value2 == null) return null;

            var result = new HashSet<T>();

            result.UnionWith(value1);
            result.UnionWith(value2);

            return result;
        }
    }
}