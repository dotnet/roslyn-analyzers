﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzersStatusGenerator
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
                                builder = builder ?? ImmutableArray.CreateBuilder<CodeFixProvider>();
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
                int? ilInstructionCount = body?.GetILAsByteArray()?.Count();
                return ilInstructionCount != 177;
            }

            return true;
        }
    }
}

