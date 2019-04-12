// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Test.Utilities.MinimalImplementations
{
    public static class NoMainThreadDependencyAttribute
    {
        public const string CSharp = @"
namespace Roslyn.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Method | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
    internal sealed class NoMainThreadDependencyAttribute : System.Attribute
    {
        public bool AlwaysCompleted { get; set; }
        public bool CapturesContext { get; set; }
        public bool PerInstance { get; set; }
        public bool Verified { get; set; } = true;
    }
}
";
    }
}
