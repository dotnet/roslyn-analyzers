// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ImportedMembersShouldNotBeReadOnly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Roslyn.Diagnostics.Analyzers.ImportedMembersShouldNotBeReadOnly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class ImportedMembersShouldNotBeReadOnlyTests
    {
        [Theory]
        [CombinatorialData]
        public async Task ReadWriteField_CSharp(
            [CombinatorialValues("System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
using System.Collections.Generic;
using {mefNamespace};

[Export]
class C {{
    [{importAttribute}]
    private IEnumerable<object> _values;
}}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadOnlyField_CSharp(
            [CombinatorialValues("System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
using System.Collections.Generic;
using {mefNamespace};

[Export]
class C {{
    [[|{importAttribute}|]]
    private readonly IEnumerable<object> _values;
}}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadWriteField_VisualBasic(
            [CombinatorialValues("System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
Imports System.Collections.Generic
Imports {mefNamespace}

<Export>
Class C
    <{importAttribute}>
    Private _values As IEnumerable(Of Object)
End Class
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadOnlyField_VisualBasic(
            [CombinatorialValues("System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
Imports System.Collections.Generic
Imports {mefNamespace}

<Export>
Class C
    <[|{importAttribute}|]>
    Private ReadOnly _values As IEnumerable(Of Object)
End Class
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadWriteProperty_CSharp(
            [CombinatorialValues("System.Composition", "System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
using System.Collections.Generic;
using {mefNamespace};

[Export]
class C {{
    [{importAttribute}]
    private IEnumerable<object> Values {{ get; set; }}
}}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadOnlyProperty_CSharp(
            [CombinatorialValues("System.Composition", "System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
using System.Collections.Generic;
using {mefNamespace};

[Export]
class C {{
    [[|{importAttribute}|]]
    private IEnumerable<object> Values {{ get; }}
}}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadWriteProperty_VisualBasic(
            [CombinatorialValues("System.Composition", "System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
Imports System.Collections.Generic
Imports {mefNamespace}

<Export>
Class C
    <{importAttribute}>
    Private Property Values As IEnumerable(Of Object)
End Class
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task ReadOnlyProperty_VisualBasic(
            [CombinatorialValues("System.Composition", "System.ComponentModel.Composition")]
            string mefNamespace,
            [CombinatorialValues("Import", "ImportMany")]
            string importAttribute)
        {
            var source = $@"
Imports System.Collections.Generic
Imports {mefNamespace}

<Export>
Class C
    <[|{importAttribute}|]>
    Private ReadOnly Property Values As IEnumerable(Of Object)
End Class
";

            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalReferences = { AdditionalMetadataReferences.SystemCompositionReference, AdditionalMetadataReferences.SystemComponentModelCompositionReference },
                },
            }.RunAsync();
        }
    }
}
