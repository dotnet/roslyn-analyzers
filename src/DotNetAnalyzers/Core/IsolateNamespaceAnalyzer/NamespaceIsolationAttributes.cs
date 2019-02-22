// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace DotNetAnalyzers.IsolateNamespaceAnalyzer
{
    // NOTE these attributes are provided for `nameof` operations, and as a reference for
    // consumers of this analyzer to copy into their own code base.
    //
    // These attributes may exist in any namespace and must be applied to the assembly.
    //
    // e.g.:
    //
    // [assembly: IsolateNamespace("Project.PrivateNamespace")]

    /// <summary>
    /// Signals to the <c>DotNetAnalyzers.IsolateNamespaceAnalyzer</c> that types within a
    /// namespace may not be referenced from other namespaces within the assembly, other than
    /// those specified in <see cref="AllowFrom"/>.
    /// </summary>
    [Conditional("EMIT_ISOLATION_ATTRIBUTES")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class IsolateNamespaceAttribute : Attribute
    {
        /// <summary>
        /// Gets the string that identifies the global namespace.
        /// </summary>
        public const string GlobalNamespace = "<global namespace>";

        /// <summary>
        /// Gets the namespace to isolate within this assembly.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// An optional set of namespaces whose member types are allowed to reference
        /// those within <see cref="Namespace"/> in the same assembly.
        /// </summary>
        public string[] AllowFrom { get; set; }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolateNamespaceAttribute"/>.
        /// </summary>
        /// <param name="namespace">The namespace to isolate.</param>
        public IsolateNamespaceAttribute(string @namespace)
        {
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }
    }

    /// <summary>
    /// Signals to the <c>DotNetAnalyzers.IsolateNamespaceAnalyzer</c> that, within an assembly,
    /// types belonging to the specified group of namespaces may mutually reference one another,
    /// yet are isolated from types in other namespaces of that same assembly.
    /// </summary>
    [Conditional("EMIT_ISOLATION_ATTRIBUTES")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class IsolateNamespaceGroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the string that identifies the global namespace.
        /// </summary>
        public const string GlobalNamespace = "<global namespace>";

        /// <summary>
        /// Gets the set of namespaces that belong to this isolation group.
        /// </summary>
        public string[] Namespaces { get; }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolateNamespaceGroupAttribute"/>.
        /// </summary>
        /// <param name="namespaces">The set of namespaces that belong to this isolation group.</param>
        public IsolateNamespaceGroupAttribute(params string[] namespaces)
        {
            Namespaces = namespaces;
        }
    }
}