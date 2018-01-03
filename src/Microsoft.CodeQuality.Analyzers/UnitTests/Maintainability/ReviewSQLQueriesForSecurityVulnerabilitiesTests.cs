using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.Maintainability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.Maintainability
{
    public class ReviewSQLQueriesForSecurityVulnerabilitiesTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer() => new ReviewSqlQueriesForSecurityVulnerabilities();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ReviewSqlQueriesForSecurityVulnerabilities();

        private new DiagnosticResult GetCSharpResultAt(int line, int column, string invokedSymbol, string containingMethod) =>
            GetCSharpResultAt(line, column, ReviewSqlQueriesForSecurityVulnerabilities.Rule, invokedSymbol, containingMethod);

        private new DiagnosticResult GetBasicResultAt(int line, int column, string invokedSymbol, string containingMethod) =>
            GetBasicResultAt(line, column, ReviewSqlQueriesForSecurityVulnerabilities.Rule, invokedSymbol, containingMethod);

        private const string SetupCodeCSharp = @"
using System.Data;

class Command : IDbCommand
{
    public string CommandText { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int CommandTimeout { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public CommandType CommandType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IDbConnection Connection { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public IDataParameterCollection Parameters => throw new System.NotImplementedException();

    public IDbTransaction Transaction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public UpdateRowSource UpdatedRowSource { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void Cancel()
    {
        throw new System.NotImplementedException();
    }

    public IDbDataParameter CreateParameter()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public int ExecuteNonQuery()
    {
        throw new System.NotImplementedException();
    }

    public IDataReader ExecuteReader()
    {
        throw new System.NotImplementedException();
    }

    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
        throw new System.NotImplementedException();
    }

    public object ExecuteScalar()
    {
        throw new System.NotImplementedException();
    }

    public void Prepare()
    {
        throw new System.NotImplementedException();
    }
}

class Adapter : IDataAdapter
{
    public MissingMappingAction MissingMappingAction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public MissingSchemaAction MissingSchemaAction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public ITableMappingCollection TableMappings => throw new System.NotImplementedException();

    public int Fill(DataSet dataSet)
    {
        throw new System.NotImplementedException();
    }

    public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
    {
        throw new System.NotImplementedException();
    }

    public IDataParameter[] GetFillParameters()
    {
        throw new System.NotImplementedException();
    }

    public int Update(DataSet dataSet)
    {
        throw new System.NotImplementedException();
    }
}";

        [Fact]
        public void DbCommand_CommandText_StringLiteral_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Test
{{
    void M1()
    {{
        Command c = new Command();
        c.CommandText = ""asdf"";
    }}
}}");
        }

        [Fact]
        public void DbCommand_ConstructorParameter_StringLiteral_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string parameter)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        Command c = new Command1(""asdf"");
    }}
}}
");
        }

        [Fact]
        public void DbCommand_CommandText_ClassConstant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Test
{{
    const string str = """";

    void M1()
    {{
        Command c = new Command();
        c.CommandText = str;
    }}
}}");
        }

        [Fact]
        public void DbCommand_ConstructorParameter_ClassConstant_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string parameter)
    {{
    }}
}}

class Test
{{
    const string str = """";

    void M1()
    {{
        Command c = new Command1(str);
    }}
}}
");
        }

        [Fact]
        public void DbCommand_ConstructorParameter_CallingAnotherConstructor_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    static string static_str = """";
    public Command1(string command, string cmd)
    {{
    }}
    public Command1(string parameter) : this(parameter, static_str)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        Command c = new Command1("""");
    }}
}}
");
        }

        [Fact]
        public void DbCommand_CommandText_LocalVariable_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Test
{{
    void M1()
    {{
        Command c = new Command();
        var str = """";
        c.CommandText = str;
    }}
}}",
            GetCSharpResultAt(92, 9, "string Command.CommandText", "M1"));
        }

        [Fact]
        public void DbCommand_ConstructorParameter_LocalVariable_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string parameter)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        var str = """";
        Command c = new Command1(str);
    }}
}}
",
            GetCSharpResultAt(98, 21, "Command1.Command1(string parameter)", "M1"));
        }

        [Fact]
        public void DbCommand_CommandText_Parameter_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Test
{{
    void M1(string str)
    {{
        Command c = new Command();
        c.CommandText = str;
    }}
}}",
            GetCSharpResultAt(91, 9, "string Command.CommandText", "M1"));
        }

        [Fact]
        public void DbCommand_ConstructorParameter_Parameter_Diagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string parameter)
    {{
    }}
}}

class Test
{{
    void M1(string str)
    {{
        Command c = new Command1(str);
    }}
}}
",
            GetCSharpResultAt(97, 21, "Command1.Command1(string parameter)", "M1"));
        }

        [Fact]
        public void DbCommand_ConstructorParameter_MultipleParameters_NeitherNamedCommandOrCmd_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string parameter1, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = """";
        Command c = new Command1(str, str);
    }}
}}
");
        }

        [Fact]
        public void DbCommand_ConstructorParameter_MultipleParameters_OneNamedCmd_WithStringLiteral_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = """";
        Command c = new Command1("""", str);
    }}
}}
");
        }

        [Fact]
        public void DbCommand_ConstructorParameter_MultipleParameters_OneNamedCmd_WithLocal_NoDiagnostic()
        {
            VerifyCSharp($@"
{SetupCodeCSharp}

class Command1 : Command
{{
    public Command1(string cmd, string parameter2)
    {{
    }}
}}

class Test
{{
    void M1()
    {{
        string str = """";
        Command c = new Command1(str, str);
    }}
}}
",
            GetCSharpResultAt(98, 21, "Command1.Command1(string cmd, string parameter2)", "M1"));
        }
    }
}
