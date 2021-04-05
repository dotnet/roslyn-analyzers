// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace GenerateDocumentationAndConfigFilesForBrokenRuntime
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            // Delegate to the actual tool implementation
            return GenerateDocumentationAndConfigFiles.Program.Main(args);
        }
    }
}