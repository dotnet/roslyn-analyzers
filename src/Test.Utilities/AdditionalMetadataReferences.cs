// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Test.Utilities
{
    public static class AdditionalMetadataReferences
    {
        private static readonly ReferenceAssemblies _default =
#if NETCOREAPP
            ReferenceAssemblies.NetCore.NetCoreApp21;
#else
            ReferenceAssemblies.Default;
#endif

        public static ReferenceAssemblies Default { get; } = _default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Xml.Data"))
#endif
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis", "3.0.0")));

        public static ReferenceAssemblies DefaultWithoutRoslynSymbols { get; } = _default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Xml.Data"))
#endif
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis.Workspaces.Common", "3.0.0")));

        public static ReferenceAssemblies DefaultWithSystemWeb { get; } = Default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Web", "System.Web.Extensions"))
#endif
            ;

        public static ReferenceAssemblies DefaultForTaintedDataAnalysis { get; } = Default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("PresentationFramework", "System.DirectoryServices", "System.Web", "System.Web.Extensions", "System.Xaml"))
            .AddPackages(ImmutableArray.Create(new PackageIdentity("AntiXSS", "4.3.0")))
#endif
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.AspNetCore.Mvc", "2.2.0")));

        public static ReferenceAssemblies DefaultWithSerialization { get; } = Default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Runtime.Serialization"))
#endif
            ;

        public static ReferenceAssemblies DefaultWithAzureStorage { get; } = _default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("WindowsAzure.Storage", "9.0.0")));

        public static ReferenceAssemblies DefaultWithNewtonsoftJson { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Newtonsoft.Json", "10.0.1")));

        public static ReferenceAssemblies DefaultWithWinForms { get; } = Default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Windows.Forms"))
#endif
            ;

        public static ReferenceAssemblies DefaultWithWinHttpHandler { get; } = _default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("System.Net.Http.WinHttpHandler", "4.7.0")));

        public static ReferenceAssemblies DefaultWithAspNetCoreMvc { get; } = Default
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.AspNetCore", "1.1.7"),
                new PackageIdentity("Microsoft.AspNetCore.Mvc", "1.1.8"),
                new PackageIdentity("Microsoft.AspNetCore.Http", "1.1.2")));

        public static ReferenceAssemblies DefaultWithNUnit { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("NUnit", "3.12.0")));

        public static ReferenceAssemblies DefaultWithXUnit { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("xunit", "2.4.1")));

        public static ReferenceAssemblies DefaultWithMSTest { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("MSTest.TestFramework", "2.1.0")));

        public static ReferenceAssemblies DefaultWithAsyncInterfaces { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.1.0")));

        public static ReferenceAssemblies DefaultWithFullComposition { get; } = Default
#if !NETCOREAPP
            .AddAssemblies(ImmutableArray.Create("System.Composition.AttributedModel", "System.ComponentModel.Composition"))
#endif
            ;

        public static MetadataReference SystemCollectionsImmutableReference { get; } = MetadataReference.CreateFromFile(typeof(ImmutableHashSet<>).Assembly.Location);
        public static MetadataReference SystemCompositionReference { get; } = MetadataReference.CreateFromFile(typeof(System.Composition.ExportAttribute).Assembly.Location);
        public static MetadataReference SystemXmlDataReference { get; } = MetadataReference.CreateFromFile(typeof(System.Data.Rule).Assembly.Location);
        public static MetadataReference CodeAnalysisReference { get; } = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        public static MetadataReference WorkspacesReference { get; } = MetadataReference.CreateFromFile(typeof(Workspace).Assembly.Location);
        public static MetadataReference TestReferenceAssembly { get; } = MetadataReference.CreateFromFile(typeof(OtherDll.OtherDllStaticMethods).Assembly.Location);
    }
}
