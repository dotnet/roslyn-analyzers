﻿// <auto-generated/>

#nullable enable
using System.Reflection;


namespace NS
{
    internal static partial class Resources
    {
        private static global::System.Resources.ResourceManager? s_resourceManager;
        internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(Resources)));
        internal static global::System.Globalization.CultureInfo? Culture { get; set; }
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull("defaultValue")]
        internal static string? GetResourceString(string resourceKey, string? defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture) ?? defaultValue;
        /// <summary>value</summary>
        internal static string @Name => GetResourceString("Name")!;

    }
}
