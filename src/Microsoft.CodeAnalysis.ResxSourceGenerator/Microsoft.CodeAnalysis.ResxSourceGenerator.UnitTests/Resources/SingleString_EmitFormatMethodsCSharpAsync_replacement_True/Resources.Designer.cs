﻿// <auto-generated/>

#nullable enable
using System.Reflection;


namespace TestProject
{
    internal static partial class Resources
    {
        private static global::System.Resources.ResourceManager? s_resourceManager;
        public static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(Resources)));
        public static global::System.Globalization.CultureInfo? Culture { get; set; }
        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull("defaultValue")]
        internal static string? GetResourceString(string resourceKey, string? defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture) ?? defaultValue;

        private static string GetResourceString(string resourceKey, string[]? formatterNames)
        {
           var value = GetResourceString(resourceKey) ?? "";
           if (formatterNames != null)
           {
               for (var i = 0; i < formatterNames.Length; i++)
               {
                   value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
               }
           }
           return value;
        }

        /// <summary>value {replacement}</summary>
        public static string @Name_with_dots => GetResourceString("Name.with.dots")!;
        /// <summary>value {replacement}</summary>
        internal static string FormatName_with_dots(object? replacement)
           => string.Format(Culture, GetResourceString("Name.with.dots", new[] { "replacement" }), replacement);


    }
}
