// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueAnalyzer,
    Microsoft.NetCore.Analyzers.Runtime.DoNotPassMutableValueTypesByValueFixer>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class DoNotPassMutableValueTypesByValueTests
    {

    }
}
