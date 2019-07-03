﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
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
        /// <param name="descriptionResourceStringName">Name of the resource string inside <see cref="MicrosoftNetCoreSecurityResources"/> for the diagnostic's descrption.</param>
        /// <param name="helpLinkUri">Help link URI.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See Microsoft.CodeAnalysis.WellKnownDiagnosticTags for some well known tags.</param>
        /// <returns>New DiagnosticDescriptor.</returns>
        public static DiagnosticDescriptor CreateDiagnosticDescriptor(
            string id,
            string titleResourceStringName,
            string messageResourceStringName,
            bool isEnabledByDefault,
            string helpLinkUri,
            string descriptionResourceStringName = null,
            params string[] customTags)
        {
            return CreateDiagnosticDescriptor(
                id,
                typeof(MicrosoftNetCoreSecurityResources),
                titleResourceStringName,
                messageResourceStringName,
                isEnabledByDefault,
                helpLinkUri,
                descriptionResourceStringName,
                customTags);
        }

        /// <summary>
        /// Creates a DiagnosticDescriptor with <see cref="LocalizableResourceString"/>s from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="id">Diagnostic identifier.</param>
        /// <param name="resourceSource">Type containing the resource strings.</param>
        /// <param name="titleResourceStringName">Name of the resource string inside <paramref name="resourceSource"/> for the diagnostic's title.</param>
        /// <param name="messageResourceStringName">Name of the resource string inside <paramref name="resourceSource"/> for the diagnostic's message.</param>
        /// <param name="isEnabledByDefault">Flag indicating the diagnostic is enabled by default</param>
        /// <param name="descriptionResourceStringName">Name of the resource string inside <paramref name="resourceSource"/> for the diagnostic's descrption.</param>
        /// <param name="helpLinkUri">Help link URI.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See Microsoft.CodeAnalysis.WellKnownDiagnosticTags for some well known tags.</param>
        /// <returns>New DiagnosticDescriptor.</returns>
        public static DiagnosticDescriptor CreateDiagnosticDescriptor(
            string id,
            Type resourceSource,
            string titleResourceStringName,
            string messageResourceStringName,
            bool isEnabledByDefault,
            string helpLinkUri,
            string descriptionResourceStringName = null,
            params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id,
                GetResourceString(resourceSource, titleResourceStringName),
                GetResourceString(resourceSource, messageResourceStringName),
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault,
                descriptionResourceStringName != null ? GetResourceString(resourceSource, descriptionResourceStringName) : null,
                helpLinkUri,
                customTags);
        }

        /// <summary>
        /// Deserialization methods for <see cref="T:System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>.
        /// </summary>
        [SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "The comment references a type that is not referenced by this compilation.")]
        public static readonly ImmutableHashSet<string> BinaryFormatterDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "DeserializeMethodResponse",
                "UnsafeDeserialize",
                "UnsafeDeserializeMethodResponse");

        /// <summary>
        /// Deserialization methods for <see cref="T:System.Runtime.Serialization.NetDataContractSerializer"/>.
        /// </summary>
        [SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "The comment references a type that is not referenced by this compilation.")]
        public static readonly ImmutableHashSet<string> NetDataContractSerializerDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "ReadObject");

        /// <summary>
        /// Deserialization methods for <see cref="T:System.Web.Script.Serialization.JavaScriptSerializer"/>.
        /// </summary>
        [SuppressMessage("Documentation", "CA1200:Avoid using cref tags with a prefix", Justification = "The comment references a type that is not referenced by this compilation.")]
        public static readonly ImmutableHashSet<string> JavaScriptSerializerDeserializationMethods =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Deserialize",
                "DeserializeObject");

        private static readonly ImmutableDictionary<Type, ResourceManager> ResourceManagerMapping =
            ImmutableDictionary.CreateRange<Type, ResourceManager>(
                new[]
                {
                    (typeof(MicrosoftNetCoreSecurityResources), MicrosoftNetCoreSecurityResources.ResourceManager),
                    (typeof(SystemSecurityCryptographyResources), SystemSecurityCryptographyResources.ResourceManager),
                }.Select(o => new KeyValuePair<Type, ResourceManager>(o.Item1, o.Item2)));

        /// <summary>
        /// Gets a <see cref="LocalizableResourceString"/> from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="resourceSource">Type containing the resource strings.</param>
        /// <param name="name">Name of the resource string to retrieve.</param>
        /// <returns>The corresponding <see cref="LocalizableResourceString"/>.</returns>
        private static LocalizableResourceString GetResourceString(Type resourceSource, string name)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }

            if (!ResourceManagerMapping.TryGetValue(resourceSource, out ResourceManager resourceManager))
            {
                throw new ArgumentException($"No mapping found for {resourceSource}", nameof(resourceSource));
            }

#if DEBUG
            if (resourceManager.GetString(name, System.Globalization.CultureInfo.InvariantCulture) == null)
            {
                throw new ArgumentException($"Resource string '{name}' not found in {resourceSource}", nameof(name));
            }
#endif

            LocalizableResourceString localizableResourceString = new LocalizableResourceString(name, resourceManager, resourceSource);
            return localizableResourceString;
        }
    }
}
