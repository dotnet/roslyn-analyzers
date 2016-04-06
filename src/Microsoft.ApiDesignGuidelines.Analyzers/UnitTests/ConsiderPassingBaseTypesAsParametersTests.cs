// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
#if false
    class SomethingBase
    {
        public int Value = 42;
        public void SomeAction() { }
        public int PropVal { get; set; }
        public string PropGetVal { get; set; }
        public event System.EventHandler OnNothingGood;
        public virtual void SomeVirtualAction() { }
        public void SomeHiddenAction() { }
    }
    class Something: SomethingBase {
        public int SubValue = 32;
        public override void SomeVirtualAction() { }
        public new void SomeHiddenAction() { }
    }
    class Driver
    {
        int FieldRef(Something p1) { return p1.Value; }
        void MethodBind(Something p2) { p2.SomeAction(); }
        int PropertyRef(Something p3) { return p3.PropVal; }
        string ROPropertyRef(Something p4) { return p4.PropGetVal ?? p4.Value.ToString(); }
        void EventRef(Something p5)
        {
            p5.OnNothingGood += (object sender, System.EventArgs e) => { p5.SubValue = 2; };
        }

        string AccessInLambda(Something p5) {
            Func<string> getV = () => p5.SubValue.ToString();
            return p5.PropGetVal ?? getV();
        }

        async Task<string> AccessInAsyncTask(Something p6)
        {
            return p6.PropGetVal ?? await Task.Run(() => p6.SubValue.ToString());
        }
    }
#endif

    public class ConsiderPassingBaseTypesAsParametersTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        const string CommonSomething = @"
    using System;
    using System.Threading.Tasks;

    class SomethingBase
    {
        public int Value = 42;
        public void SomeAction() { }
        public int PropVal { get; set; }
        public string PropGetVal { get; set; }
        public event EventHandler OnNothingGood;
        public virtual void SomeVirtualAction() { }
    }
    class Something: SomethingBase {
        public int SubValue = 32;
        public override void SomeVirtualAction() { }
    }
";

        [Fact]
        public void TestCSCheckForBaseMemberReferences()
        {
            // Test methods which only access base members (i.e. should report diagnostic)
            var code = CommonSomething + @"
    class BaseMemberReferences
    {
        int FieldRef(Something p1) { return p1.Value; }
        void MethodBind(Something p2) { p2.SomeAction(); }
        int PropertyRef(Something p3) { return p3.PropVal; }
        string ROPropertyRef(Something p4) { return p4.PropGetVal ?? ""Nop""; }
        string MultiMemberRef(Something p5) { return p5.PropGetVal ?? p5.Value.ToString(); }
    }
";
            var derivedClass = "Something";
            var baseClass = "SomethingBase";

            VerifyCSharp(code, 
                GetCA1011CSharpResultAt(21, 32, "p1", "FieldRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(22, 35, "p2", "MethodBind", derivedClass, baseClass),
                GetCA1011CSharpResultAt(23, 35, "p3", "PropertyRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(24, 40, "p4", "ROPropertyRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(25, 41, "p5", "MultiMemberRef", derivedClass, baseClass) );
        }

        [Fact]
        public void TestCSCheckForDerivedMemberReferences()
        {
            // Test methods which only access base AND derived members (i.e. should NOT report diagnostic)
            var code = CommonSomething + @"
    class DerivedMemberReferences
    {
        int FieldRef(Something p1) { return p1.SubValue; }
        void MethodBind(Something p2) { p2.SomeVirtualAction(); }
        int PropertyAndField(Something p3) { return p3.PropVal + p3.SubValue; }

        string AccessInLambda(Something p5) {
            Func<string> getV = () => p5.SubValue.ToString();
            return p5.PropGetVal ?? getV();
        }

        async Task<string> AccessInAsyncTask(Something p6)
        {
            return p6.PropGetVal ?? await Task.Run(() => p6.SubValue.ToString());
        }
    }
";
            VerifyCSharp(code);
        }

        internal static readonly string CA1011Name = "CA1011";
        internal static readonly string CA1011Message = MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersMessage;

        // Message Args: paramName, methodName, paramType, baseType
        private static DiagnosticResult GetCA1011CSharpResultAt(int line, int column, params string [] messageArgs)
        {
            return GetCSharpResultAt(line, column, CA1011Name, string.Format(CA1011Message, messageArgs));
        }

        private static DiagnosticResult GetCA1011BasicResultAt(int line, int column, string objectName)
        {
            return GetBasicResultAt(line, column, CA1011Name, string.Format(CA1011Message, objectName));
        }
    }
}