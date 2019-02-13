// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetFramework.CSharp.Analyzers;
using Microsoft.NetFramework.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Microsoft.NetFramework.CSharp.Analyzers.CSharpImplementSerializationMethodsCorrectlyAnalyzer,
    Microsoft.NetFramework.CSharp.Analyzers.CSharpImplementSerializationMethodsCorrectlyFixer>;
using VerifyVB = Microsoft.CodeAnalysis.VisualBasic.Testing.XUnit.CodeFixVerifier<
    Microsoft.NetFramework.VisualBasic.Analyzers.BasicImplementSerializationMethodsCorrectlyAnalyzer,
    Microsoft.NetFramework.VisualBasic.Analyzers.BasicImplementSerializationMethodsCorrectlyFixer>;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public class ImplementSerializationMethodsCorrectlyTests
    {
    }
}