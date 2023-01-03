// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ReleaseNotesUtil
{
    [DataContract]
    internal sealed class RuleFileContent
    {
        [DataMember]
        public List<RuleInfo> Rules { get; set; } = new List<RuleInfo>();
    }
}
