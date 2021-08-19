﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseDataSetReadXml,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseDataSetReadXmlTests
    {
        [Fact]
        public async Task ReadXml_DiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Unsafe(Stream s)
        {
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(s);
        }
    }
}",
                GetCSharpResultAt(12, 13, "XmlReadMode DataSet.ReadXml(Stream stream)"));
        }

        [Fact]
        public async Task DerivedReadXml_DiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Unsafe(string s)
        {
            MyDataSet dataSet = new MyDataSet();
            dataSet.ReadXml(s);
        }
    }

    public class MyDataSet : DataSet
    {
    }
}",
                GetCSharpResultAt(12, 13, "XmlReadMode DataSet.ReadXml(string fileName)"));
        }

        [Fact]
        public async Task DerivedReadXmlEvenWithReadXmlSchema_DiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Unsafe(string s)
        {
            MyDataSet dataSet = new MyDataSet();
            dataSet.ReadXmlSchema("""");
            dataSet.ReadXml(s);
        }
    }

    public class MyDataSet : DataSet
    {
    }
}",
                GetCSharpResultAt(13, 13, "XmlReadMode DataSet.ReadXml(string fileName)"));
        }

        [Fact]
        public async Task RejectChanges_NoDiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.IO;
using System.Data;

namespace Blah
{
    public class Program
    {
        public void Safe(Stream s)
        {
            DataSet dataSet = new DataSet();
            dataSet.RejectChanges();
        }
    }
}");
        }

        [Fact]
        public async Task AutogeneratedProbablyForGui1_DiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
namespace Blah
{
    /// <summary>  
    ///Represents a strongly typed in-memory cache of data.  
    ///</summary>  
    [global::System.Serializable()]  
    [global::System.ComponentModel.DesignerCategoryAttribute(""code"")]  
    [global::System.ComponentModel.ToolboxItem(true)]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute(""GetTypedDataSetSchema"")]
    [global::System.Xml.Serialization.XmlRootAttribute(""Package"")]
    [global::System.ComponentModel.Design.HelpKeywordAttribute(""vs.data.DataSet"")]
    public partial class Something : global::System.Data.DataSet {

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]  
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""System.Data.Design.TypedDataSetGenerator"", ""4.0.0.0"")]  
        protected override void ReadXmlSerializable(global::System.Xml.XmlReader reader) {  
            if ((this.DetermineSchemaSerializationMode(reader) == global::System.Data.SchemaSerializationMode.IncludeSchema)) {  
                this.Reset();  
                global::System.Data.DataSet ds = new global::System.Data.DataSet();  
                ds.ReadXml(reader);  
                if ((ds.Tables[""Something""] != null)) {  
                    //// base.Tables.Add(new SomethingTable(ds.Tables[""Something""]));
                }
                this.DataSetName = ds.DataSetName;  
                this.Prefix = ds.Prefix;  
                this.Namespace = ds.Namespace;  
                this.Locale = ds.Locale;  
                this.CaseSensitive = ds.CaseSensitive;  
                this.EnforceConstraints = ds.EnforceConstraints;  
                this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);  
                this.InitVars();  
            }  
            else {  
                this.ReadXml(reader);  
                this.InitVars();  
            }  
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]  
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""System.Data.Design.TypedDataSetGenerator"", ""4.0.0.0"")]  
        internal void InitVars() {  
            //this.InitVars(true);  
        }
    }
}",
                GetCSharpAutogeneratedResultAt(21, 17, "XmlReadMode DataSet.ReadXml(XmlReader reader)"),
                GetCSharpAutogeneratedResultAt(35, 17, "XmlReadMode DataSet.ReadXml(XmlReader reader)"));
        }

        [Fact]
        public async Task AutogeneratedProbablyForGui2_DiagnosticAsync()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
namespace Blah
{
    /// <summary>  
    ///Represents a strongly typed in-memory cache of data.  
    ///</summary>  
    [global::System.Serializable()]  
    [global::System.ComponentModel.DesignerCategoryAttribute(""code"")]  
    [global::System.ComponentModel.ToolboxItem(true)]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute(""GetTypedDataSetSchema"")]
    [global::System.Xml.Serialization.XmlRootAttribute(""Package"")]
    [global::System.ComponentModel.Design.HelpKeywordAttribute(""vs.data.DataSet"")]
    public partial class Something : global::System.Data.DataSet {

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override void ReadXmlSerializable(global::System.Xml.XmlReader reader) {
            if ((this.DetermineSchemaSerializationMode(reader) == global::System.Data.SchemaSerializationMode.IncludeSchema)) {
                this.Reset();
                global::System.Data.DataSet ds = new global::System.Data.DataSet();
                ds.ReadXml(reader);
                if ((ds.Tables[""Something""] != null)) {
                    //// base.Tables.Add(new SomethingTable(ds.Tables[""Something""]));
                }
                this.DataSetName = ds.DataSetName;
                this.Prefix = ds.Prefix;
                this.Namespace = ds.Namespace;
                this.Locale = ds.Locale;
                this.CaseSensitive = ds.CaseSensitive;
                this.EnforceConstraints = ds.EnforceConstraints;
                this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);
                this.InitVars();
            }
            else {
                this.ReadXml(reader);
                this.InitVars();
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]  
        internal void InitVars() {  
            //this.InitVars(true);  
        }
    }
}",
                GetCSharpAutogeneratedResultAt(20, 17, "XmlReadMode DataSet.ReadXml(XmlReader reader)"),
                GetCSharpAutogeneratedResultAt(34, 17, "XmlReadMode DataSet.ReadXml(XmlReader reader)"));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic(DoNotUseDataSetReadXml.RealMethodUsedDescriptor)
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);

        private static DiagnosticResult GetCSharpAutogeneratedResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic(DoNotUseDataSetReadXml.RealMethodUsedInAutogeneratedDescriptor)
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);
    }
}
