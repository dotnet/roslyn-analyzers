﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;

namespace AnalyzersStatusGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: AnalyzersStatusGenerator <list of analyzer dlls>");
                return;
            }

            Loader loader = new Loader();

            IEnumerable<AnalyzerFileReference> analyzerReferences = args
                .Select(dll => new AnalyzerFileReference(dll, loader));

            GenerateStatus(analyzerReferences);
        }

        /// <summary>
        /// Generate a json file that has the implementation status of the passed in analyzer references.
        /// </summary>
        private static void GenerateStatus(IEnumerable<AnalyzerFileReference> analyzerReferences)
        {
            DescriptorEqualityComparer comparer = new DescriptorEqualityComparer();

            var allAnalyzers = analyzerReferences
                .Select(analyzerReference => new { AnalyzerPackage = analyzerReference.Display, Analyzers = analyzerReference.GetAnalyzersForAllLanguages() });

            IEnumerable<string> fixableDiagnosticIds = analyzerReferences
                .SelectMany(analyzerReference => analyzerReference.GetFixers())
                .SelectMany(fixer => fixer.FixableDiagnosticIds)
                .Distinct();

            Dictionary<string, AnalyzersStatusInfo> diagnosticInfoMap = new Dictionary<string, AnalyzersStatusInfo>();
            foreach (var group in allAnalyzers)
            {
                foreach (DiagnosticAnalyzer analyzer in group.Analyzers)
                {
                    bool hasImplementation = HasImplementation(analyzer);
                    bool hasCSharpImplementation = hasImplementation && analyzer.GetType().GetCustomAttribute<DiagnosticAnalyzerAttribute>().Languages.Contains(LanguageNames.CSharp);
                    bool hasVBImplementation = hasImplementation && analyzer.GetType().GetCustomAttribute<DiagnosticAnalyzerAttribute>().Languages.Contains(LanguageNames.VisualBasic);

                    foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics.Distinct(comparer))
                    {
                        if (!diagnosticInfoMap.ContainsKey(descriptor.Id))
                        {
                            bool hasCodeFix = fixableDiagnosticIds.Contains(descriptor.Id);

                            // This assumes a convention similar to A.B.Analyzers and A.B.CSharp.Analyzers and A.B.VisualBasic.Analyzers
                            // Some common dlls might have a common prefix as well.
                            string analyzerPackage = group.AnalyzerPackage.Replace(".CSharp", string.Empty).Replace(".VisualBasic", string.Empty).Replace(".Common", string.Empty);

                            var diagnosticInfo = new AnalyzersStatusInfo
                            {
                                Id = descriptor.Id,
                                Category = descriptor.Category,
                                HasCSharpImplementation = hasCSharpImplementation,
                                HasVBImplementation = hasVBImplementation,
                                Name = analyzer.GetType().Name,
                                Title = descriptor.Title.ToString(),
                                HelpLink = descriptor.HelpLinkUri,
                                HasCodeFix = hasCodeFix,
                                IsEnabledByDefault = descriptor.IsEnabledByDefault.ToString(),
                                AnalyzerPackage = analyzerPackage
                            };
                            diagnosticInfoMap.Add(descriptor.Id, diagnosticInfo);
                        }
                        else
                        {
                            // Update the state of the existing info.
                            AnalyzersStatusInfo diagnosticInfo = diagnosticInfoMap[descriptor.Id];
                            diagnosticInfo.HasCSharpImplementation |= hasCSharpImplementation;
                            diagnosticInfo.HasVBImplementation |= hasVBImplementation;
                        }
                    }
                }
            }

            Console.WriteLine(JsonConvert.SerializeObject(new { Diagnostics = diagnosticInfoMap.Values }));
        }

        /// <summary>
        /// Check the method body of the Initialize method of an analyzer and if that's empty,
        /// then the analyzer hasn't been implemented yet.
        /// </summary>
        private static bool HasImplementation(DiagnosticAnalyzer analyzer)
        {
            MethodInfo method = analyzer.GetType().GetTypeInfo().GetMethod("Initialize");
            if (method != null)
            {
                MethodBody body = method.GetMethodBody();
                int? ilInstructionCount = body?.GetILAsByteArray()?.Count();
                return ilInstructionCount != 2;
            }

            return true;
        }
    }
}
