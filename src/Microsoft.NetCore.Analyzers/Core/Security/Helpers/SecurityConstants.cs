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
                "Deserialize",
                "DeserializeMethodResponse",
                "UnsafeDeserialize",
                "UnsafeDeserializeMethodResponse");
    }
}
