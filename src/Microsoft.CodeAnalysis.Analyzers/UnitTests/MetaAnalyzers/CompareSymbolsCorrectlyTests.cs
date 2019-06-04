// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.CompareSymbolsCorrectlyAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.CompareSymbolsCorrectlyFix>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.CompareSymbolsCorrectlyAnalyzer,
    Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers.CompareSymbolsCorrectlyFix>;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class CompareSymbolsCorrectlyTests
    {
        private const string MinimalSymbolImplementationCSharp = @"
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis;

class Symbol : ISymbol {
    public SymbolKind Kind => throw new NotImplementedException();
    public string Language => throw new NotImplementedException();
    public string Name => throw new NotImplementedException();
    public string MetadataName => throw new NotImplementedException();
    public ISymbol ContainingSymbol => throw new NotImplementedException();
    public IAssemblySymbol ContainingAssembly => throw new NotImplementedException();
    public IModuleSymbol ContainingModule => throw new NotImplementedException();
    public INamedTypeSymbol ContainingType => throw new NotImplementedException();
    public INamespaceSymbol ContainingNamespace => throw new NotImplementedException();
    public bool IsDefinition => throw new NotImplementedException();
    public bool IsStatic => throw new NotImplementedException();
    public bool IsVirtual => throw new NotImplementedException();
    public bool IsOverride => throw new NotImplementedException();
    public bool IsAbstract => throw new NotImplementedException();
    public bool IsSealed => throw new NotImplementedException();
    public bool IsExtern => throw new NotImplementedException();
    public bool IsImplicitlyDeclared => throw new NotImplementedException();
    public bool CanBeReferencedByName => throw new NotImplementedException();
    public ImmutableArray<Location> Locations => throw new NotImplementedException();
    public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw new NotImplementedException();
    public Accessibility DeclaredAccessibility => throw new NotImplementedException();
    public ISymbol OriginalDefinition => throw new NotImplementedException();
    public bool HasUnsupportedMetadata => throw new NotImplementedException();

    public void Accept(SymbolVisitor visitor) => throw new NotImplementedException();
    public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) => throw new NotImplementedException();
    public bool Equals(ISymbol other) => throw new NotImplementedException();
    public ImmutableArray<AttributeData> GetAttributes() => throw new NotImplementedException();
    public string GetDocumentationCommentId() => throw new NotImplementedException();
    public string GetDocumentationCommentXml(CultureInfo preferredCulture, bool expandIncludes, CancellationToken cancellationToken) => throw new NotImplementedException();
    public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format) => throw new NotImplementedException();
    public string ToDisplayString(SymbolDisplayFormat format) => throw new NotImplementedException();
    public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format) => throw new NotImplementedException();
    public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format) => throw new NotImplementedException();

    public static bool operator ==(Symbol left, Symbol right) => throw new NotImplementedException();
    public static bool operator !=(Symbol left, Symbol right) => throw new NotImplementedException();
}
";
        private const string MinimalSymbolImplementationVisualBasic = @"
Imports System
Imports System.Collections.Immutable
Imports System.Globalization
Imports System.Threading
Imports Microsoft.CodeAnalysis

Class Symbol
    Implements ISymbol

    Public ReadOnly Property Kind As SymbolKind Implements ISymbol.Kind
    Public ReadOnly Property Language As String Implements ISymbol.Language
    Public ReadOnly Property Name As String Implements ISymbol.Name
    Public ReadOnly Property MetadataName As String Implements ISymbol.MetadataName
    Public ReadOnly Property ContainingSymbol As ISymbol Implements ISymbol.ContainingSymbol
    Public ReadOnly Property ContainingAssembly As IAssemblySymbol Implements ISymbol.ContainingAssembly
    Public ReadOnly Property ContainingModule As IModuleSymbol Implements ISymbol.ContainingModule
    Public ReadOnly Property ContainingType As INamedTypeSymbol Implements ISymbol.ContainingType
    Public ReadOnly Property ContainingNamespace As INamespaceSymbol Implements ISymbol.ContainingNamespace
    Public ReadOnly Property IsDefinition As Boolean Implements ISymbol.IsDefinition
    Public ReadOnly Property IsStatic As Boolean Implements ISymbol.IsStatic
    Public ReadOnly Property IsVirtual As Boolean Implements ISymbol.IsVirtual
    Public ReadOnly Property IsOverride As Boolean Implements ISymbol.IsOverride
    Public ReadOnly Property IsAbstract As Boolean Implements ISymbol.IsAbstract
    Public ReadOnly Property IsSealed As Boolean Implements ISymbol.IsSealed
    Public ReadOnly Property IsExtern As Boolean Implements ISymbol.IsExtern
    Public ReadOnly Property IsImplicitlyDeclared As Boolean Implements ISymbol.IsImplicitlyDeclared
    Public ReadOnly Property CanBeReferencedByName As Boolean Implements ISymbol.CanBeReferencedByName
    Public ReadOnly Property Locations As ImmutableArray(Of Location) Implements ISymbol.Locations
    Public ReadOnly Property DeclaringSyntaxReferences As ImmutableArray(Of SyntaxReference) Implements ISymbol.DeclaringSyntaxReferences
    Public ReadOnly Property DeclaredAccessibility As Accessibility Implements ISymbol.DeclaredAccessibility
    Public ReadOnly Property OriginalDefinition As ISymbol Implements ISymbol.OriginalDefinition
    Public ReadOnly Property HasUnsupportedMetadata As Boolean Implements ISymbol.HasUnsupportedMetadata

    Public Sub Accept(visitor As SymbolVisitor) Implements ISymbol.Accept
        Throw New NotImplementedException()
    End Sub

    Public Function GetAttributes() As ImmutableArray(Of AttributeData) Implements ISymbol.GetAttributes
        Throw New NotImplementedException()
    End Function

    Public Function Accept(Of TResult)(visitor As SymbolVisitor(Of TResult)) As TResult Implements ISymbol.Accept
        Throw New NotImplementedException()
    End Function

    Public Function GetDocumentationCommentId() As String Implements ISymbol.GetDocumentationCommentId
        Throw New NotImplementedException()
    End Function

    Public Function GetDocumentationCommentXml(Optional preferredCulture As CultureInfo = Nothing, Optional expandIncludes As Boolean = False, Optional cancellationToken As CancellationToken = Nothing) As String Implements ISymbol.GetDocumentationCommentXml
        Throw New NotImplementedException()
    End Function

    Public Function ToDisplayString(Optional format As SymbolDisplayFormat = Nothing) As String Implements ISymbol.ToDisplayString
        Throw New NotImplementedException()
    End Function

    Public Function ToDisplayParts(Optional format As SymbolDisplayFormat = Nothing) As ImmutableArray(Of SymbolDisplayPart) Implements ISymbol.ToDisplayParts
        Throw New NotImplementedException()
    End Function

    Public Function ToMinimalDisplayString(semanticModel As SemanticModel, position As Integer, Optional format As SymbolDisplayFormat = Nothing) As String Implements ISymbol.ToMinimalDisplayString
        Throw New NotImplementedException()
    End Function

    Public Function ToMinimalDisplayParts(semanticModel As SemanticModel, position As Integer, Optional format As SymbolDisplayFormat = Nothing) As ImmutableArray(Of SymbolDisplayPart) Implements ISymbol.ToMinimalDisplayParts
        Throw New NotImplementedException()
    End Function

    Public Function Equals(other As ISymbol) As Boolean Implements IEquatable(Of ISymbol).Equals
        Throw New NotImplementedException()
    End Function

    Public Shared Operator =(left As Symbol, right As Symbol) As Boolean
        Throw New NotImplementedException()
    End Operator

    Public Shared Operator <>(left As Symbol, right As Symbol) As Boolean
        Throw New NotImplementedException()
    End Operator
End Class
";

        private const string MinimumComparerDeclarationCSharp =
@"
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public class EqualityComparer : IEqualityComparer<ISymbol>
{
    public static EqualityComparer Instance = new EqualityComparer();

    public bool Equals(ISymbol x, ISymbol y) => false;
    public int GetHashCode(ISymbol obj) => 0;
}

public class SimpleComparer : IComparer<ISymbol>
{
    public static SimpleComparer Instance = new SimpleComparer();
    public int Compare(ISymbol x, ISymbol y) => 0;
}

public class EqualityComparerAndComparer : IEqualityComparer<ISymbol>, IComparer<ISymbol>
{
    public static EqualityComparerAndComparer Instance = new EqualityComparerAndComparer();

    public int Compare(ISymbol x, ISymbol y) => 0;
    public bool Equals(ISymbol x, ISymbol y) => false;
    public int GetHashCode(ISymbol obj) => 0;
}";

        private const string MinimumComparerDeclarationVB =
@"
Imports System
Imports System.Collections.Generic
Imports Microsoft.CodeAnalysis

Class EqualityComparer
    Implements IEqualityComparer(Of ISymbol)

    Public Function Equals(x As ISymbol, y As ISymbol) As Boolean Implements IEqualityComparer(Of ISymbol).Equals
        Throw New NotImplementedException()
    End Function

    Public Function GetHashCode(obj As ISymbol) As Integer Implements IEqualityComparer(Of ISymbol).GetHashCode
        Throw New NotImplementedException()
    End Function
End Class

Class SimpleComparer
    Implements IComparer(Of ISymbol)

    Public Function Compare(x As ISymbol, y As ISymbol) As Integer Implements IComparer(Of ISymbol).Compare
        Throw New NotImplementedException()
    End Function
End Class

Class EqualityComparerAndComparer
    Implements IEqualityComparer(Of ISymbol)
    Implements IComparer(Of ISymbol)

    Public Function Compare(x As ISymbol, y As ISymbol) As Integer Implements IComparer(Of ISymbol).Compare
        Throw New NotImplementedException()
    End Function

    Public Function Equals(x As ISymbol, y As ISymbol) As Boolean Implements IEqualityComparer(Of ISymbol).Equals
        Throw New NotImplementedException()
    End Function

    Public Function GetHashCode(obj As ISymbol) As Integer Implements IEqualityComparer(Of ISymbol).GetHashCode
        Throw New NotImplementedException()
    End Function
End Class";

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareTwoSymbolsEquals_CSharp(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method({symbolType} x, {symbolType} y) {{
        return {{|{DiagnosticIds.CompareSymbolsCorrectlyRuleId}:x == y|}};
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method({symbolType} x, {symbolType} y) {{
        return Equals(x, y);
    }}
}}
";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Theory]
        [WorkItem(2335, "https://github.com/dotnet/roslyn-analyzers/issues/2335")]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareTwoSymbolsByIdentity_CSharp(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method1({symbolType} x, {symbolType} y) {{
        return (object)x == y;
    }}
    bool Method2({symbolType} x, {symbolType} y) {{
        return x == (object)y;
    }}
    bool Method3({symbolType} x, {symbolType} y) {{
        return (object)x == (object)y;
    }}
}}
";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        [WorkItem(2336, "https://github.com/dotnet/roslyn-analyzers/issues/2336")]
        public async Task CompareTwoSymbolImplementations_CSharp()
        {
            var source = $@"
class TestClass {{
    bool Method(Symbol x, Symbol y) {{
        return x == y;
    }}
}}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source, MinimalSymbolImplementationCSharp } },
            }.RunAsync();
        }

        [Theory]
        [WorkItem(2336, "https://github.com/dotnet/roslyn-analyzers/issues/2336")]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareSymbolImplementationWithInterface_CSharp(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method(Symbol x, {symbolType} y) {{
        return {{|{DiagnosticIds.CompareSymbolsCorrectlyRuleId}:x == y|}};
    }}
}}
";
            var fixedSource = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method(Symbol x, {symbolType} y) {{
        return Equals(x, y);
    }}
}}
";

            await new VerifyCS.Test
            {
                TestState = { Sources = { source, MinimalSymbolImplementationCSharp } },
                FixedState = { Sources = { fixedSource, MinimalSymbolImplementationCSharp } },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task CompareSymbolWithNull_CSharp(
            [CombinatorialValues(nameof(ISymbol), nameof(INamedTypeSymbol))] string symbolType,
            [CombinatorialValues("==", "!=")] string @operator,
            [CombinatorialValues("null", "default", "default(ISymbol)")] string value)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method1({symbolType} x) {{
        return x {@operator} {value};
    }}

    bool Method2({symbolType} x) {{
        return {value} {@operator} x;
    }}
}}
";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareSymbolWithNullPattern_CSharp(string symbolType)
        {
            var source = $@"
using Microsoft.CodeAnalysis;
class TestClass {{
    bool Method1({symbolType} x) {{
        return x is null;
    }}
}}
";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareTwoSymbolsEquals_VisualBasic(string symbolType)
        {
            var source = $@"
Imports Microsoft.CodeAnalysis
Class TestClass
    Function Method(x As {symbolType}, y As {symbolType}) As Boolean
        Return {{|{DiagnosticIds.CompareSymbolsCorrectlyRuleId}:x Is y|}}
    End Function
End Class
";
            var fixedSource = $@"
Imports Microsoft.CodeAnalysis
Class TestClass
    Function Method(x As {symbolType}, y As {symbolType}) As Boolean
        Return Equals(x, y)
    End Function
End Class
";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Theory]
        [WorkItem(2335, "https://github.com/dotnet/roslyn-analyzers/issues/2335")]
        [InlineData(nameof(ISymbol))]
        [InlineData(nameof(INamedTypeSymbol))]
        public async Task CompareTwoSymbolsByIdentity_VisualBasic(string symbolType)
        {
            var source = $@"
Imports Microsoft.CodeAnalysis
Class TestClass
    Function Method(x As {symbolType}, y As {symbolType}) As Boolean
        Return DirectCast(x, Object) Is y
    End Function
End Class
";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [WorkItem(2336, "https://github.com/dotnet/roslyn-analyzers/issues/2336")]
        [CombinatorialData]
        public async Task CompareTwoSymbolImplementations_VisualBasic(
            [CombinatorialValues("Symbol", nameof(ISymbol), nameof(INamedTypeSymbol))] string symbolType,
            [CombinatorialValues("=", "<>", "Is", "IsNot")] string @operator)
        {
            var source = $@"
Imports Microsoft.CodeAnalysis
Class TestClass
    Function Method1(x As Symbol, y As {symbolType}) As Boolean
        Return x {@operator} y
    End Function

    Function Method2(x As Symbol, y As {symbolType}) As Boolean
        Return y {@operator} x
    End Function
End Class
";

            await new VerifyVB.Test
            {
                TestState = { Sources = { source, MinimalSymbolImplementationVisualBasic } },
            }.RunAsync();
        }

        [Theory]
        [CombinatorialData]
        public async Task CompareSymbolWithNull_VisualBasic(
            [CombinatorialValues(nameof(ISymbol), nameof(INamedTypeSymbol))] string symbolType,
            [CombinatorialValues("Is", "IsNot")] string @operator)
        {
            var source = $@"
Imports Microsoft.CodeAnalysis
Class TestClass
    Function Method1(x As {symbolType}) As Boolean
        Return x {@operator} Nothing
    End Function

    Function Method2(x As {symbolType}) As Boolean
        Return Nothing {@operator} x
    End Function
End Class
";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Dictionary<ISymbol, string>", "EqualityComparer")]
        [InlineData("Dictionary<ISymbol, string>", "EqualityComparerAndComparer")]
        [InlineData("SortedDictionary<ISymbol, string>", "SimpleComparer")]
        [InlineData("SortedDictionary<ISymbol, string>", "EqualityComparerAndComparer")]
        [InlineData("SortedSet<ISymbol>", "SimpleComparer")]
        [InlineData("SortedSet<ISymbol>", "EqualityComparerAndComparer")]
        [InlineData("HashSet<ISymbol>", "EqualityComparer")]
        [InlineData("HashSet<ISymbol>", "EqualityComparerAndComparer")]
        public async Task CompareSymbolWithComparisonTypes_WithComparer_CSharp(string dataType, string comparisonType)
        {
            var source = $@"
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
class TestClass {{
    private {dataType} testData = new {dataType}({comparisonType}.Instance);
}}
";
            await new VerifyCS.Test()
            {
                TestState = { Sources = { source, MinimumComparerDeclarationCSharp } }
            }.RunAsync();
        }

        [Theory]
        [InlineData("Dictionary<ISymbol, string>", 52)]
        [InlineData("SortedDictionary<ISymbol, string>", 58)]
        [InlineData("SortedSet<ISymbol>", 43)]
        [InlineData("HashSet<ISymbol>", 41)]
        public async Task CompareSymbolWithComparisonTypes_WithoutComparer_CSharp(string dataType, int column)
        {
            var source = $@"
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
class TestClass {{
    private {dataType} testData = new {dataType}();
}}
";
            const int line = 5;

            await VerifyCS.VerifyAnalyzerAsync(source,
                DiagnosticResult.CompilerWarning(DiagnosticIds.UseComparersForSymbols)
                    .WithLocation(line, column));
        }

        [Theory]
        [InlineData("Dictionary(Of ISymbol, String)", "EqualityComparer")]
        [InlineData("Dictionary(Of ISymbol, String)", "EqualityComparerAndComparer")]
        [InlineData("SortedDictionary(Of ISymbol, String)", "SimpleComparer")]
        [InlineData("SortedDictionary(Of ISymbol, string)", "EqualityComparerAndComparer")]
        [InlineData("SortedSet(Of ISymbol)", "SimpleComparer")]
        [InlineData("SortedSet(Of ISymbol)", "EqualityComparerAndComparer")]
        [InlineData("HashSet(Of ISymbol)", "EqualityComparer")]
        [InlineData("HashSet(Of ISymbol)", "EqualityComparerAndComparer")]
        public async Task CompareSymbolWithComparisonTypes_WithComparer_VB(string dataType, string comparerType)
        {
            var source = $@"
Imports System.Collections.Generic
Imports Microsoft.CodeAnalysis

Class TestClass
    Public Sub T()
        Dim comparer As New {comparerType}()
        Dim col As New {dataType}(comparer)
    End Sub
End Class";

            await new VerifyVB.Test()
            {
                TestState = { Sources = { source, MinimumComparerDeclarationVB } }
            }.RunAsync();
        }

        [Theory]
        [InlineData("Dictionary(Of ISymbol, String)")]
        [InlineData("SortedDictionary(Of ISymbol, String)")]
        [InlineData("SortedSet(Of ISymbol)")]
        [InlineData("HashSet(Of ISymbol)")]
        public async Task CompareSymbolWithComparisonTypes_WithoutComparer_VB(string dataType)
        {
            var source = $@"
Imports System.Collections.Generic
Imports Microsoft.CodeAnalysis

Class TestClass
    Public Sub T()
        Dim col As New {dataType}()
    End Sub
End Class";

            const int line = 7;
            const int column = 20;

            await VerifyVB.VerifyAnalyzerAsync(source,
                DiagnosticResult.CompilerWarning(DiagnosticIds.UseComparersForSymbols)
                    .WithLocation(line, column));
        }
    }
}
