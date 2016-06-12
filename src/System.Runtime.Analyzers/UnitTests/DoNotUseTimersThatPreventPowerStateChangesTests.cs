// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.CSharp.Analyzers;
using System.Runtime.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace System.Runtime.Analyzers.UnitTests
{
    public class DoNotUseTimersThatPreventPowerStateChangesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotUseTimersThatPreventPowerStateChangesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotUseTimersThatPreventPowerStateChangesAnalyzer();
        }
    }
}