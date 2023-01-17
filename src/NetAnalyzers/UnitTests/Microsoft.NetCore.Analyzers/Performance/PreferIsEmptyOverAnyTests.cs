// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferLengthCountIsEmptyOverAnyAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpPreferLengthCountIsEmptyOverAnyFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.PreferLengthCountIsEmptyOverAnyAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicPreferLengthCountIsEmptyOverAnyFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class PreferIsEmptyOverAnyTests
    {
        private static readonly DiagnosticResult ExpectedDiagnostic = new DiagnosticResult(PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyDescriptor).WithLocation(0);

        [Fact]
        public Task TestLocalDeclarationAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public void M() {
        var array = ImmutableArray<int>.Empty;
        _ = {|#0:array.Any()|};
    }
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public void M() {
        var array = ImmutableArray<int>.Empty;
        _ = !array.IsEmpty;
    }
}";
            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestLocalDeclarationAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function M()
        Dim array = ImmutableArray(Of Integer).Empty
        Dim x = {|#0:array.Any()|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function M()
        Dim array = ImmutableArray(Of Integer).Empty
        Dim x = Not array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task TestParameterDeclarationAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return {|#0:array.Any()|};
    }
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return !array.IsEmpty;
    }
}";

            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestParameterDeclarationAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return {|#0:array.Any()|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return Not array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task TestNegatedAnyAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool IsEmpty(ImmutableArray<int> array) {
        return !{|#0:array.Any()|};
    }
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool IsEmpty(ImmutableArray<int> array) {
        return array.IsEmpty;
    }
}";

            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestNegatedAnyAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function IsEmpty(array As ImmutableArray(Of Integer)) As Boolean
        Return Not {|#0:array.Any()|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function IsEmpty(array As ImmutableArray(Of Integer)) As Boolean
        Return array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task DontWarnOnChainedLinqWithAnyAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return array.Select(x => x).Any();
    }
}";

            return VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public Task VbDontWarnOnChainedLinqWithAnyAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return array.Select(Function(x) x).Any()
    End Function
End Class";

            return VerifyVB.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public Task DontWarnOnAnyWithPredicateAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return array.Any(x => x > 5);
    }
}";

            return VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public Task VbDontWarnOnAnyWithPredicateAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return array.Any(Function(x) x > 5)
    End Function
End Class";

            return VerifyVB.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public Task TestQualifiedCallAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return {|#0:Enumerable.Any(array)|};
    }
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return !array.IsEmpty;
    }
}";

            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestQualifiedCallAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return {|#0:Enumerable.Any(array)|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return Not array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task TestFullyQualifiedCallAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return {|#0:System.Linq.Enumerable.Any(array)|};
    }
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public bool HasContents(ImmutableArray<int> array) {
        return !array.IsEmpty;
    }
}";

            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestFullyQualifiedCallAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return {|#0:System.Linq.Enumerable.Any(array)|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return Not array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestWithoutParenthesesAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return {|#0:array.Any|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return Not array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestNegatedWithoutParenthesesAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return Not {|#0:array.Any|}
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function HasContents(array As ImmutableArray(Of Integer)) As Boolean
        Return array.IsEmpty
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task TestPassedAsArgumentAsync()
        {
            const string code = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public void Run(ImmutableArray<int> array) {
        X({|#0:array.Any()|});
    }

    public void X(bool b) => throw null;
}";
            const string fixedCode = @"
using System.Collections.Immutable;
using System.Linq;

public class Tests {
    public void Run(ImmutableArray<int> array) {
        X(!array.IsEmpty);
    }

    public void X(bool b) => throw null;
}";

            return VerifyCS.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }

        [Fact]
        public Task VbTestPassedAsArgumentAsync()
        {
            const string code = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function Run(array As ImmutableArray(Of Integer))
        X({|#0:array.Any|})
    End Function

    Public Function X(b As Boolean)
        Throw New System.Exception()
    End Function
End Class";
            const string fixedCode = @"
Imports System.Collections.Immutable
Imports System.Linq

Public Class Tests
    Public Function Run(array As ImmutableArray(Of Integer))
        X(Not array.IsEmpty)
    End Function

    Public Function X(b As Boolean)
        Throw New System.Exception()
    End Function
End Class";

            return VerifyVB.VerifyCodeFixAsync(code, ExpectedDiagnostic, fixedCode);
        }
    }
}