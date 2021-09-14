// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace AnalyzerCodeGenerator
{
    public class CheckData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Messages { get; set; }
        public string AnalyzerProject { get; set; }
        public string Category { get; set; }
        public PortStatus Port { get; set; }
        public Priority OriginalPriority { get; set; }
        public Priority RevisedPriority { set; get; }
    }
}
