using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit.Sdk;

namespace Test.Utilities
{
    /// <summary>
    /// Represents the target of an accessibility test.
    /// </summary>
    public enum AccessibilityTestTarget
    {
        /// <summary>
        /// A class's accessibility is being changed. Only public and internal accessibility levels will be tested.
        /// </summary>
        Class,

        /// <summary>
        /// A class member or nested class's accessibility is being changed. All accessibility levels will be tested.
        /// </summary>
        InsideClass,
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types

    /// <summary>
    /// Represents the context of an accessibility test.
    /// </summary>
    public struct AccessibilityContext
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public AccessibilityContext(Accessibility accessibility, bool expectDiagnostic)
        {
            Accessibility = accessibility;
            ExpectDiagnostic = expectDiagnostic;
        }

#pragma warning disable IDE0055 // Fix formatting
        /// <summary>
        /// Gets the source string that represents the accessibility level being tested in VB.
        /// </summary>
        public string AccessVB => Accessibility switch
        {
            Accessibility.Private => "Private",
            Accessibility.Protected => "Protected",
            Accessibility.Friend => "Friend",
            Accessibility.ProtectedOrFriend => "Protected Friend",
            Accessibility.ProtectedAndFriend => "Private Protected",
            Accessibility.Public => "Public",
            _ => throw new InvalidOperationException($"Invalid accessibility '{Accessibility}'")
        };

        /// <summary>
        /// Gets the source string that represents the accessibility level being tested in C#.
        /// </summary>
        public string AccessCS => Accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Public => "public",
            _ => throw new InvalidOperationException($"Invalid accessibility '{Accessibility}'")
        };
#pragma warning restore IDE0055 // Fix formatting


        /// <summary>
        /// Gets the markup string that represents the left boundary of the expected diagnostic range. When no diagnostic is expected, this string will be empty.
        /// </summary>
        public string Left(bool useBrace = false, string id = null)
        {
            return useBrace ?
                ExpectDiagnostic ? $"{{|{id}:" : string.Empty :
                ExpectDiagnostic ? "[|" : string.Empty;
        }

        /// <summary>
        /// Gets the markup string that represents the left boundary of the expected diagnostic range. When no diagnostic is expected, this string will be empty.
        /// </summary>
        public string Right(bool useBrace = false)
        {
            return useBrace ?
                ExpectDiagnostic ? "|}" : string.Empty :
                ExpectDiagnostic ? "|}" : string.Empty;
        }


        /// <summary>
        /// Gets the accessibility level being tested.
        /// </summary>
        public Accessibility Accessibility { get; }


        /// <summary>
        /// Gets a value indicating whether a diagnostic output should be expected. If true, <see cref="Left(bool, string)"/> and <see cref="GetRight()"/> will contain appropriate test markup strings.
        /// </summary>
        public bool ExpectDiagnostic { get; }

        public override string ToString() => Accessibility.ToString();
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AccessibilityTestAttribute : DataAttribute
    {
        public AccessibilityTestAttribute(AccessibilityTestTarget target)
        {
            Target = target;
        }

        public AccessibilityTestTarget Target { get; }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true) };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false) };

            // Non-nested classes can only have the above two visibilities
            if (Target == AccessibilityTestTarget.Class) yield break;

            yield return new object[] { new AccessibilityContext(Accessibility.Private, false) };
            yield return new object[] { new AccessibilityContext(Accessibility.Protected, true) };
            yield return new object[] { new AccessibilityContext(Accessibility.ProtectedOrInternal, true) };
            yield return new object[] { new AccessibilityContext(Accessibility.ProtectedAndInternal, false) };
        }
    }
}
