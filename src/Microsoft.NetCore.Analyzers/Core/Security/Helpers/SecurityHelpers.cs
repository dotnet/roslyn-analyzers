using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;

#pragma warning disable CA1054 // Uri parameters should not be strings

namespace Microsoft.NetCore.Analyzers.Security.Helpers
{
    internal static class SecurityHelpers
    {
        /// <summary>
        /// Creates a DiagnosticDescriptor with <see cref="LocalizableResourceString"/>s from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="id">Diagnostic identifier.</param>
        /// <param name="titleResourceStringName">Name of the resource string inside <see cref="MicrosoftNetCoreSecurityResources"/> for the diagnostic's title.</param>
        /// <param name="messageResourceStringName">Name of the resource string inside <see cref="MicrosoftNetCoreSecurityResources"/> for the diagnostic's message.</param>
        /// <param name="isEnabledByDefault">Flag indicating the diagnostic is enabled by default</param>
        /// <param name="helpLinkUri">Help link URI.</param>
        /// <param name="descriptionResourceStringName">Name of the resource string inside <see cref="MicrosoftNetCoreSecurityResources"/> for the diagnostic's descrption.</param>
        /// <returns></returns>
        public static DiagnosticDescriptor CreateDiagnosticDescriptor(
            string id,
            string titleResourceStringName,
            string messageResourceStringName,
            bool isEnabledByDefault,
            string helpLinkUri,
            string descriptionResourceStringName = null)
        {
            return new DiagnosticDescriptor(
                id,
                GetResourceString(titleResourceStringName),
                GetResourceString(messageResourceStringName),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault,
                descriptionResourceStringName != null ? GetResourceString(descriptionResourceStringName) : null,
                helpLinkUri);
        }

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

        /// <summary>
        /// Deserialization methods for <see cref="System.Runtime.Serialization.NetDataContractSerializer"/>.
        /// </summary>
        public static readonly ImmutableHashSet<string> NetDataContractSerializerDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "ReadObject");

        /// <summary>
        /// Deserialization methods for <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>.
        /// </summary>
        public static readonly ImmutableHashSet<string> JavaScriptSerializerDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "DeserializeObject");

        /// <summary>
        /// Gets a <see cref="LocalizableResourceString"/> from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="name">Name of the resource string to retrieve.</param>
        /// <returns>The corresponding <see cref="LocalizableResourceString"/>.</returns>
        private static LocalizableResourceString GetResourceString(string name)
        {
            return new LocalizableResourceString(
                    name,
                    MicrosoftNetCoreSecurityResources.ResourceManager,
                    typeof(MicrosoftNetCoreSecurityResources));
        }
    }
}
