// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Test.Utilities
{
    public static class AdditionalMetadataReferences
    {
        public static ReferenceAssemblies Default { get; } = ReferenceAssemblies.Default
            .AddAssemblies(ImmutableArray.Create("System.Xml.Data"))
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis", "3.0.0")));

        public static ReferenceAssemblies DefaultWithoutRoslynSymbols { get; } = ReferenceAssemblies.Default
            .AddAssemblies(ImmutableArray.Create("System.Xml.Data"))
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.CodeAnalysis.Workspaces.Common", "3.0.0")));

        public static ReferenceAssemblies DefaultWithSystemWeb { get; } = Default
            .AddAssemblies(ImmutableArray.Create("System.Web", "System.Web.Extensions"));

        public static ReferenceAssemblies DefaultForTaintedDataAnalysis { get; } = Default
            .AddAssemblies(ImmutableArray.Create("PresentationFramework", "System.DirectoryServices", "System.Web", "System.Web.Extensions", "System.Xaml"))
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.AspNetCore.Mvc", "2.2.0")));

        public static ReferenceAssemblies DefaultWithSerialization { get; } = Default
            .AddAssemblies(ImmutableArray.Create("System.Runtime.Serialization"));

        public static ReferenceAssemblies DefaultWithAzureStorage { get; } = ReferenceAssemblies.Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("WindowsAzure.Storage", "9.0.0")));

        public static ReferenceAssemblies DefaultWithNewtonsoftJson { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Newtonsoft.Json", "10.0.1")));

        public static ReferenceAssemblies DefaultWithWinForms { get; } = Default
            .AddAssemblies(ImmutableArray.Create("System.Windows.Forms"));

        public static ReferenceAssemblies DefaultWithNUnit { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("NUnit", "3.12.0")));

        public static ReferenceAssemblies DefaultWithxunit { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("xunit", "2.4.1")));

        public static ReferenceAssemblies DefaultWithMSTest { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("MSTest.TestFramework", "2.1.0")));

        public static ReferenceAssemblies DefaultWithAsyncInterfaces { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "1.1.0")));

        public static ReferenceAssemblies DefaultWithWinHttpHandler { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("System.Net.Http.WinHttpHandler", "4.7.0")));

        public static ReferenceAssemblies DefaultWithAspNetCoreMvc { get; } = Default
            .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.AspNetCore.Mvc", "2.2.0")));

        public static ReferenceAssemblies DefaultWithFullComposition { get; } = Default
            .AddAssemblies(ImmutableArray.Create("System.Composition.AttributedModel", "System.ComponentModel.Composition"));


        public static MetadataReference TestReferenceAssembly { get; } = MetadataReference.CreateFromFile(typeof(OtherDll.OtherDllStaticMethods).Assembly.Location);
    }
}
