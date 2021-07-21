// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.BufferBlockCopyLengthAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.BufferBlockCopyLengthAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class BufferBlockCopyLengthTests
    {
        [Fact]
        public async Task SrcIsByteArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        byte[] src = new byte[] {1, 2, 3, 4};
        byte[] dst = new byte[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, src.Length);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Byte() {1, 2, 3, 4}
        Dim dst = New Byte() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, src.Length)
    End Sub
End Module
");
        }

        [Fact]
        public async Task DstIsByteArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        byte[] src = new byte[] {1, 2, 3, 4};
        byte[] dst = new byte[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Byte() {1, 2, 3, 4}
        Dim dst = New Byte() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, dst.Length)
    End Sub
End Module
");
        }

        [Fact]
        public async Task SrcIsSbyteArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        sbyte[] src = new sbyte[] {1, 2, 3, 4};
        sbyte[] dst = new sbyte[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, src.Length);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New SByte() {1, 2, 3, 4}
        Dim dst = New SByte() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, src.Length)
    End Sub
End Module
");
        }

        [Fact]
        public async Task DstIsSbyteArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        sbyte[] src = new sbyte[] {1, 2, 3, 4};
        sbyte[] dst = new sbyte[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New SByte() {1, 2, 3, 4}
        Dim dst = New SByte() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, dst.Length)
    End Sub
End Module
");
        }

        [Fact]
        public async Task SrcIsIntArray()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, [|src.Length|]);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, [|src.Length|])
    End Sub
End Module
");
        }

        [Fact]
        public async Task OperandIsNotSrcOrDst()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        int[] test = new int[] {5, 6, 7, 8};
        
        Buffer.BlockCopy(src, 0, dst, 0, test.Length);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}
        Dim test = New Integer() {5, 6, 7, 8}

        Buffer.BlockCopy(src, 0, dst, 0, test.Length)
    End Sub
End Module
");
        }

        [Fact]
        public async Task SrcNumOfBytesAsReference()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        int numOfBytes = src.Length;
        
        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|]);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}
        Dim numOfBytes = src.Length

        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|])
    End Sub
End Module
");
        }

        [Fact]
        public async Task DstNumOfBytesAsReference()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        int numOfBytes = dst.Length;
        
        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|]);
    }
}
");
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}
        Dim numOfBytes = dst.Length

        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|])
    End Sub
End Module
");
        }

        [Fact]
        public async Task NumOfBytesAsLiteralConst()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, 8);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, 8)
    End Sub
End Module
");
        }

        [Fact]
        public async Task NumOfBytesAsMultipleDeclarations()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        int test = 4, numOfBytes = src.Length;

        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|]);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}
        Dim test = 4, numOfBytes = src.Length

        Buffer.BlockCopy(src, 0, dst, 0, [|numOfBytes|])
    End Sub
End Module
");
        }

        [Fact]
        public async Task NumOfBytesAsConstLiteral()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        int numOfBytes = 8;

        Buffer.BlockCopy(src, 0, dst, 0, numOfBytes);
    }
}
");
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}
        Dim numOfBytes = 8

        Buffer.BlockCopy(src, 0, dst, 0, numOfBytes)
    End Sub
End Module
");
        }

        [Fact]
        public async Task NumOfBytesAsClassConstProperty()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    private const int field = 3;
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};

        Buffer.BlockCopy(src, 0, dst, 0, Program.field);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Private Const field As Integer = 8
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, Program.field)
    End Sub
End Module
");
        }

        [Fact]
        public async Task NumOfBytesAsMethodInvocation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(src, 0, dst, 0, GetNumOfBytes());
    }
    
    static int GetNumOfBytes()
    {
        return 8;
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        Buffer.BlockCopy(src, 0, dst, 0, GetNumOfBytes())
    End Sub

    Function GetNumOfBytes()
        Return 8
    End Function
End Module
");
        }

        [Fact]
        public async Task SrcAndDstAsLiteralArrays()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        Buffer.BlockCopy(new int[] {1, 2, 3, 4}, 0, new int[] {0, 0, 0, 0}, 0, 8);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Buffer.Blockcopy(new Integer() {1, 2, 3, 4}, 0, new Integer() {0, 0, 0, 0}, 0, 8)
    End Sub
End Module
");
        }

        [Fact]
        public async Task NamedArgumentsNotInOrder()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        
        Buffer.BlockCopy(srcOffset: 0, src: src, count: [|src.Length|], dstOffset: 0, dst: dst);
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        Buffer.BlockCopy(srcOffset:=0, src:=src, count:=[|src.Length|], dstOffset:=0, dst:=dst)
    End Sub
End Module
");
        }

        [Fact]
        public async Task NonLocalArrays()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int[] src = new int[] {1, 2, 3, 4};
        int[] dst = new int[] {0, 0, 0, 0};
        
        SomeFunction(src, dst);
    }

    static void SomeFunction(int[] src, int[] dst)
    {
        Buffer.BlockCopy(src, 0, dst, 0, [|src.Length|]);
    }
}
");
            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Module Program
    Sub Main(args As String())
        Dim src = New Integer() {1, 2, 3, 4}
        Dim dst = New Integer() {0, 0, 0, 0}

        SomeFunction(src, dst)
    End Sub

    Sub SomeFunction(ByRef src As Integer(), ByRef dst As Integer())
        Buffer.BlockCopy(src, 0, dst, 0, [|src.Length|])
    End Sub
End Module
");
        }
    }
}