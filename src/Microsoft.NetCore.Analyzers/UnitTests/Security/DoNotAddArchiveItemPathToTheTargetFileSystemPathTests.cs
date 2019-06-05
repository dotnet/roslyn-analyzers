// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotAddArchiveItemPathToTheTargetFileSystemPathTests : TaintedDataAnalyzerTestBase
    {
        protected override DiagnosticDescriptor Rule => DoNotAddArchiveItemPathToTheTargetFileSystemPath.Rule;

        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string zipArchiveEntryAndZipFileExtensionsCSharpSourceCode = @"
namespace System.IO.Compression
{
    public class ZipArchiveEntry
    {
        public string FullName { get; }
    }

    public static class ZipFileExtensions
    {
        public static void ExtractToFile (this ZipArchiveEntry source, string destinationFileName)
        {
        }
    }
}";
            this.VerifyCSharp(
                new[] { source, zipArchiveEntryAndZipFileExtensionsCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void Test_Sink_ZipArchiveEntry_ExtractToFile_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry)
    {
        zipArchiveEntry.ExtractToFile(zipArchiveEntry.FullName);
    }
}",
            GetCSharpResultAt(8, 39, 8, 9, "string ZipArchiveEntry.FullName", "", "void ZipFileExtensions.ExtractToFile(ZipArchiveEntry source, string destinationFileName)", ""));
        }

        [Fact]
        public void Test_Sink_File_Open_WithStringAndFileModeParameters_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry, FileMode mode)
    {
        File.Open(zipArchiveEntry.FullName, mode);
    }
}",
            GetCSharpResultAt(9, 19, 9, 9, "string ZipArchiveEntry.FullName", "", "FileStream File.Open(string path, FileMode mode)", ""));
        }

        [Fact]
        public void Test_Sink_File_Open_WithStringAndFileModeAndFileAccessParameters_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry, FileMode mode, FileAccess access)
    {
        File.Open(zipArchiveEntry.FullName, mode, access);
    }
}",
            GetCSharpResultAt(9, 19, 9, 9, "string ZipArchiveEntry.FullName", "", "FileStream File.Open(string path, FileMode mode, FileAccess access)", ""));
        }

        [Fact]
        public void Test_Sink_File_Open_WithStringAndFileModeAndFileAccessAndFileShareParamters_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry, FileMode mode, FileAccess access, FileShare share)
    {
        File.Open(zipArchiveEntry.FullName, mode, access, share);
    }
}",
            GetCSharpResultAt(9, 19, 9, 9, "string ZipArchiveEntry.FullName", "", "FileStream File.Open(string path, FileMode mode, FileAccess access, FileShare share)", ""));
        }

        [Fact]
        public void Test_Sink_FileStream_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry, FileMode mode)
    {
        var fileStream = new FileStream(zipArchiveEntry.FullName, mode);
    }
}",
            GetCSharpResultAt(9, 41, 9, 26, "string ZipArchiveEntry.FullName", "", "FileStream.FileStream(string path, FileMode mode)", ""));
        }

        [Fact]
        public void Test_Sink_FileInfo_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry)
    {
        var fileInfo = new FileInfo(zipArchiveEntry.FullName);
    }
}",
            GetCSharpResultAt(9, 37, 9, 24, "string ZipArchiveEntry.FullName", "", "FileInfo.FileInfo(string fileName)", ""));
        }

        //Ideally, we wouldn't generate a diagnostic in this case.
        [Fact]
        public void Test_Sanitizer_String_StartsWith_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry)
    {
        var destinationFileName = zipArchiveEntry.FullName;

        if(destinationFileName.StartsWith(""Start""))
        {
            zipArchiveEntry.ExtractToFile(destinationFileName);
        }
    }
}",
            GetCSharpResultAt(9, 35, 13, 13, "string ZipArchiveEntry.FullName", "", "void ZipFileExtensions.ExtractToFile(ZipArchiveEntry source, string destinationFileName)", ""));
        }

        [Fact]
        public void Test_Sink_ZipArchiveEntry_ExtractToFile_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry, string destinationFileName)
    {
        zipArchiveEntry.ExtractToFile(destinationFileName);
    }
}");
        }

        [Fact]
        public void Test_Sanitizer_Path_GetFileName_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.IO;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry)
    {
        var destinationFileName = Path.GetFileName(zipArchiveEntry.FullName);
        zipArchiveEntry.ExtractToFile(destinationFileName);
    }
}");
        }

        [Fact]
        public void Test_Sanitizer_String_Substring_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.IO.Compression;

class TestClass
{
    public void TestMethod(ZipArchiveEntry zipArchiveEntry)
    {
        var destinationFileName = zipArchiveEntry.FullName;
        zipArchiveEntry.ExtractToFile(destinationFileName.Substring(1));
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotAddArchiveItemPathToTheTargetFileSystemPath();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotAddArchiveItemPathToTheTargetFileSystemPath();
        }
    }
}
