// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;

namespace Microsoft.NetCore.Analyzers.Security.Helpers
{
    internal static class SecurityConstants
    {
        /// <summary>
        /// Deserialization methods for <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
        /// </summary>
        public static readonly ImmutableHashSet<string> BinaryFormatterDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "DeserializeMethodResponse",
                "UnsafeDeserialize",
                "UnsafeDeserializeMethodResponse");
    }
}
