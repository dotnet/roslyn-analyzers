using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Test.Utilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AccessibilityDataAttribute : DataAttribute
    {
        public AccessibilityDataAttribute(Accessibility accessibility, bool expectDiagnostic)
        {
            Accessibility = accessibility;
            ExpectDiagnostic = expectDiagnostic;
        }

        public Accessibility Accessibility { get; }
        public bool ExpectDiagnostic { get; }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return new[]
            {
                new[]
                {
                    AccessibilityToCSharp(Accessibility), AccessibilityToVB(Accessibility), ExpectDiagnostic ? "[|" : "", ExpectDiagnostic ? "|]" : ""
                }
            };
        }

        private static string AccessibilityToCSharp(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.Public => "public",
                _ => throw new ArgumentException("Invalid accessibility " + accessibility, nameof(accessibility))
            };
        }

        private static string AccessibilityToVB(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Private => "Private",
                Accessibility.Protected => "Protected",
                Accessibility.Friend => "Friend",
                Accessibility.ProtectedOrFriend => "Protected Friend",
                Accessibility.ProtectedAndFriend => "Private Protected",
                Accessibility.Public => "Public",
                _ => throw new ArgumentException("Invalid accessibility " + accessibility, nameof(accessibility))
            };
        }

    }
}
