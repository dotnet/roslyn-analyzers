// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetCore.CSharp.Analyzers.InteropServices;
using Microsoft.NetCore.VisualBasic.Analyzers.InteropServices;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.InteropServices.CSharpMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.InteropServices.CSharpMarkBooleanPInvokeArgumentsWithMarshalAsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.InteropServices.BasicMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.InteropServices.BasicMarkBooleanPInvokeArgumentsWithMarshalAsFixer>;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class MarkBooleanPInvokeArgumentsWithMarshalAsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer();
        }
    }
}