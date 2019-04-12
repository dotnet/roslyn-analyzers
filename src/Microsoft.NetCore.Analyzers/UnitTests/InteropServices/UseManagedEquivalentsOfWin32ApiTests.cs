// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.InteropServices;
using Microsoft.NetCore.VisualBasic.Analyzers.InteropServices;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.InteropServices.CSharpUseManagedEquivalentsOfWin32ApiAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.InteropServices.CSharpUseManagedEquivalentsOfWin32ApiFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.InteropServices.BasicUseManagedEquivalentsOfWin32ApiAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.InteropServices.BasicUseManagedEquivalentsOfWin32ApiFixer>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class UseManagedEquivalentsOfWin32ApiTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicUseManagedEquivalentsOfWin32ApiAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpUseManagedEquivalentsOfWin32ApiAnalyzer();
        }
    }
}