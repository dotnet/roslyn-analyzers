// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class UseNameofInPlaceOfStringTests : DiagnosticAnalyzerTestBase
    {
        #region Unit tests for no analyzer diagnostic

        [Fact]
        public void NoDiagnostic_NoArguments()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException([||]);
    }
}");
        }

        #endregion


        #region Unit tests for analyzer diagnostic(s)

        [Fact]
        public void Diagnostic_ArgumentMatchesAParameterInScope()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int x)
    {
        throw new ArgumentNullException([|""x""|]);
    }
}",
    GetCSharpNameofResultAt(7, 41));
        }

        [Fact]
        public void Diagnostic_NoMatchingParametersInScope()
        {
            VerifyCSharp(@"
using System;
class C
{
    void M(int y)
    {
        throw new ArgumentNullException([|""x""|]);
    }
}",
    GetCSharpNameofResultAt(7, 41));
        }


        private DiagnosticResult[] GetBasicNameofResultAt(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        private DiagnosticResult GetCSharpNameofResultAt(int line, int column
            
            )
        {
            string message = string.Format(MicrosoftMaintainabilityAnalyzersResources.UseNameOfInPlaceOfStringMessage, "test");
            return GetCSharpResultAt(line, column, UseNameofInPlaceOfStringAnalyzer.RuleId, message);
        }
        #endregion
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseNameofInPlaceOfStringAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseNameofInPlaceOfStringAnalyzer();
        }
    }
}
