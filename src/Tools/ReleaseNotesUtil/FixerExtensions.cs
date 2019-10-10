using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable CA1031 // Do not catch general exception types

namespace ReleaseNotesUtil
{
    public static class FixerExtensions
    {
        /// <summary>
        /// Get all the <see cref="CodeFixProvider"/>s that are implemented in the given <see cref="AnalyzerFileReference"/>
        /// </summary>
        /// <returns>An array of <see cref="CodeFixProvider"/>s</returns>
        public static ImmutableArray<CodeFixProvider> GetFixers(this AnalyzerFileReference analyzerFileReference)
        {
            if (analyzerFileReference == null)
            {
                return ImmutableArray<CodeFixProvider>.Empty;
            }

            ImmutableArray<CodeFixProvider>.Builder builder = null;

            try
            {
                Assembly analyzerAssembly = analyzerFileReference.GetAssembly();
                IEnumerable<TypeInfo> typeInfos = analyzerAssembly.DefinedTypes;

                foreach (TypeInfo typeInfo in typeInfos)
                {
                    if (typeInfo.IsSubclassOf(typeof(CodeFixProvider)))
                    {
                        try
                        {
                            ExportCodeFixProviderAttribute attribute = typeInfo.GetCustomAttribute<ExportCodeFixProviderAttribute>();
                            if (attribute != null)
                            {
                                builder ??= ImmutableArray.CreateBuilder<CodeFixProvider>();
                                var fixer = (CodeFixProvider)Activator.CreateInstance(typeInfo.AsType());
                                if (HasImplementation(fixer))
                                {
                                    builder.Add(fixer);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }

            return builder != null ? builder.ToImmutable() : ImmutableArray<CodeFixProvider>.Empty;
        }

        /// <summary>
        /// Check the method body of the Initialize method of an analyzer and if that's empty,
        /// then the analyzer hasn't been implemented yet.
        /// </summary>
        private static bool HasImplementation(CodeFixProvider fixer)
        {
            MethodInfo method = fixer.GetType().GetTypeInfo().GetMethod("RegisterCodeFixesAsync");
            AsyncStateMachineAttribute stateMachineAttr = method?.GetCustomAttribute<AsyncStateMachineAttribute>();
            MethodInfo moveNextMethod = stateMachineAttr?.StateMachineType.GetTypeInfo().GetDeclaredMethod("MoveNext");
            if (moveNextMethod != null)
            {
                MethodBody body = moveNextMethod.GetMethodBody();
                int? ilInstructionCount = body?.GetILAsByteArray()?.Length;
                return ilInstructionCount != 177;
            }

            // See if the method body is:
            // {
            //     return Task.CompletedTask;
            // }
            byte[] methodBodyIL = method?.GetMethodBody()?.GetILAsByteArray();
            if (methodBodyIL != null
                && methodBodyIL.Length == 6
                && methodBodyIL[0] == 0x28    // call <method>
                && methodBodyIL[5] == 0x2a)   // ret
            {
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse<byte>(methodBodyIL, 1, sizeof(Int32));
                }

                int metadataToken = BitConverter.ToInt32(methodBodyIL, 1);
                MethodBase calledMethod = method.Module.ResolveMethod(metadataToken);
                if (calledMethod != null
                    && calledMethod.DeclaringType.FullName == "System.Threading.Tasks.Task"
                    && calledMethod.Name == "get_CompletedTask")
                {
                    return false;
                }
            }

            return true;
        }
    }
}