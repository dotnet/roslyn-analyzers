// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace ReleaseNotesUtil
{
    /// <summary>
    /// Info that we care about from a DiagnosticDescriptor.
    /// </summary>
    [DataContract]
    internal class RuleInfo
    {
        public RuleInfo(string id, string title, string category, bool isEnabledByDefault, bool hasCodeFix, string messageFormat, string description, string helpLink)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            IsEnabledByDefault = isEnabledByDefault;
            HasCodeFix = hasCodeFix;
            MessageFormat = messageFormat ?? throw new ArgumentNullException(nameof(messageFormat));
            Description = description;
            HelpLink = helpLink;
        }

        public RuleInfo()
        {
        }

        [DataMember]
        public string? Id { get; set; }

        [DataMember]
        public string? Title { get; set; }

        [DataMember]
        public string? Category { get; set; }

        [DataMember]
        public bool IsEnabledByDefault { get; set; }

        [DataMember]
        public bool HasCodeFix { get; set; }

        [DataMember]
        public string? MessageFormat { get; set; }

        [DataMember]
        public string? Description { get; set; }

        [DataMember]
        public string? HelpLink { get; set; }

        // Computed properties.
        public string IdWithHelpLinkMarkdown
        {
            get
            {
                return !String.IsNullOrWhiteSpace(this.HelpLink)
                    ? $"[{this.Id}]({this.HelpLink})"
                    : this.Id ?? String.Empty;
            }
        }

        public string DescriptionOrMessageFormatMarkdown
        {
            get
            {
                return
                    (!String.IsNullOrWhiteSpace(this.Description)
                        ? this.Description
                        : this.MessageFormat)
                    ?? String.Empty;
            }
        }
    }
}