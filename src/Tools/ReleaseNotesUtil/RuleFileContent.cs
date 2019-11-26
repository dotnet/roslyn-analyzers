using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ReleaseNotesUtil
{
    [DataContract]
    internal class RuleFileContent
    {
        [DataMember]
        public List<RuleInfo>? Rules { get; set; }
    }
}
