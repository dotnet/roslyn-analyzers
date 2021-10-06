// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace AnalyzerCodeGenerator
{
    internal struct CheckMessages
    {
        public string Id { get; set; }
        public Dictionary<string, string> Messages { get; set; } 
    }
}
