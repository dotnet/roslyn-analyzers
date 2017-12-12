// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidEmptyInterfacesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidEmptyInterfacesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidEmptyInterfacesAnalyzer();
        }

        [Fact]
        public void TestCSharpEmptyPublicInterface()
        {
            VerifyCSharp(@"
public interface I
{
}", CreateCSharpResult(2, 18));
        }

        [Fact]
        public void TestBasicEmptyPublicInterface()
        {
            VerifyBasic(@"
Public Interface I
End Interface", CreateBasicResult(2, 18));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void TestCSharpEmptyInternalInterface()
        {
            VerifyCSharp(@"
interface I
{
}");
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public void TestBasicEmptyInternalInterface()
        {
            VerifyBasic(@"
Interface I
End Interface");
        }

        [Fact]
        public void TestCSharpNonEmptyInterface1()
        {
            VerifyCSharp(@"
public interface I
{
    void DoStuff();
}");
        }

        [Fact]
        public void TestBasicNonEmptyInterface1()
        {
            VerifyBasic(@"
Public Interface I
    Function GetStuff() as Integer
End Interface");
        }

        [Fact]
        public void TestCSharpEmptyInterfaceWithNoInheritedMembers()
        {
            VerifyCSharp(@"
public interface I : IBase
{
}

public interface IBase { }", CreateCSharpResult(2, 18), CreateCSharpResult(6, 18));
        }

        [Fact]
        public void TestBasicEmptyInterfaceWithNoInheritedMembers()
        {
            VerifyBasic(@"
Public Interface I
    Inherits IBase
End Interface

Public Interface IBase
End Interface", CreateBasicResult(2, 18), CreateBasicResult(6, 18));
        }

        [Fact]
        public void TestCSharpEmptyInterfaceWithInheritedMembers()
        {
            VerifyCSharp(@"
public interface I : IBase
{
}

public interface IBase 
{
    void DoStuff(); 
}");
        }

        [Fact]
        public void TestBasicEmptyInterfaceWithInheritedMembers()
        {
            VerifyBasic(@"
Public Interface I
    Inherits IBase
End Interface

Public Interface IBase
    Sub DoStuff()
End Interface");
        }

        private static DiagnosticResult CreateCSharpResult(int line, int col)
        {
            return GetCSharpResultAt(line, col, AvoidEmptyInterfacesAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidEmptyInterfacesMessage);
        }

        private static DiagnosticResult CreateBasicResult(int line, int col)
        {
            return GetBasicResultAt(line, col, AvoidEmptyInterfacesAnalyzer.RuleId, MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidEmptyInterfacesMessage);
        }
    }
}