// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

// TODO: Remove this global suppression file when the analyzer is updated to stop reporting on char.ToString() without culture
// See https://github.com/dotnet/roslyn-analyzers/pull/2938
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DefineAccessorsForAttributeArgumentsFixer.AddAccessor(Microsoft.CodeAnalysis.Document,Microsoft.CodeAnalysis.SyntaxNode,System.Threading.CancellationToken)~System.Threading.Tasks.Task{Microsoft.CodeAnalysis.Document}")]
