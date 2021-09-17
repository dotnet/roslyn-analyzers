﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpDetectPreviewFeatureAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public partial class DetectPreviewFeatureUnitTests
    {
        [Fact]
        public async Task TestEventWithPreviewRemove()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
           class Publisher
           {
               public static event EventHandler<EventArgs> RaiseCustomEvent
               {
                   add { }
                   [RequiresPreviewFeatures]
                   remove { }
               }
           }
        
           class Subscriber
           {
               private readonly string _id;
        
               public Subscriber(string id, Publisher pub)
               {
                   _id = id;
        
                   Publisher.RaiseCustomEvent += HandleCustomEvent;
                   {|#0:Publisher.RaiseCustomEvent -= HandleCustomEvent|};
               }
        
               void HandleCustomEvent(object sender, EventArgs e)
               {
               }
           }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "remove_RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestEventWithPreviewAdd()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
           class Publisher
           {
               public static event EventHandler<EventArgs> RaiseCustomEvent
               {
                   [RequiresPreviewFeatures]
                   add { }
                   remove { }
               }
           }
        
           class Subscriber
           {
               private readonly string _id;
        
               public Subscriber(string id, Publisher pub)
               {
                   _id = id;
        
                   {|#0:Publisher.RaiseCustomEvent += HandleCustomEvent|};
                   Publisher.RaiseCustomEvent -= HandleCustomEvent;
               }
        
               void HandleCustomEvent(object sender, EventArgs e)
               {
               }
           }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "add_RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestEventWithPreviewAddAndRemove()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
           class Publisher
           {
               public static event EventHandler<EventArgs> RaiseCustomEvent
               {
                   [RequiresPreviewFeatures]
                   add { }
                   [RequiresPreviewFeatures]
                   remove { }
               }
           }
        
           class Subscriber
           {
               private readonly string _id;
        
               public Subscriber(string id, Publisher pub)
               {
                   _id = id;
        
                   {|#0:Publisher.RaiseCustomEvent += HandleCustomEvent|};
                   {|#1:Publisher.RaiseCustomEvent -= HandleCustomEvent|};
               }
        
               void HandleCustomEvent(object sender, EventArgs e)
               {
               }
           }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "add_RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "remove_RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestEventWithCustomAddAndRemove()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
           class Publisher
           {
               [RequiresPreviewFeatures]
               public static event EventHandler<EventArgs> RaiseCustomEvent
               {
                   add { }
                   remove { }
               }
        
               public static void EventHandler(object sender, EventArgs e) { }
           }
        
           class Subscriber
           {
               private readonly string _id;
        
               public Subscriber(string id, Publisher pub)
               {
                   _id = id;
        
                   {|#0:Publisher.RaiseCustomEvent|} += HandleCustomEvent;
                   {|#1:Publisher.RaiseCustomEvent|} -= HandleCustomEvent;
               }
        
               void HandleCustomEvent(object sender, EventArgs e)
               {
               }
           }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "RaiseCustomEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestEvent()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {

            class Program
            {
                public Program()
                {
                }

                public delegate void SampleEventHandler(object sender, bool e);

                [RequiresPreviewFeatures]
                public static event SampleEventHandler StaticSampleEvent;

                [RequiresPreviewFeatures]
                public event SampleEventHandler SampleEvent;

                public static void HandleEvent(object sender, bool e)
                {

                }
                static void Main(string[] args)
                {
                    {|#0:StaticSampleEvent|}?.Invoke(new Program(), new bool());

                    Program program = new Program();
                    {|#1:program.SampleEvent|} += HandleEvent;
                    {|#2:program.SampleEvent|} -= HandleEvent;
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "StaticSampleEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "SampleEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "SampleEvent"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }

        [Fact]
        public async Task TestEventWithPreviewEventHandler()
        {
            var csInput = @" 
        using System.Runtime.Versioning; using System;
        namespace Preview_Feature_Scratch
        {
            class Publisher
            {
                public event EventHandler<{|#0:PreviewEventArgs|}> RaiseCustomEvent;
#nullable enable
                public event EventHandler<{|#4:PreviewEventArgs|}?>? RaiseCustomEventNullable;
#nullable disable
         
                public void DoSomething()
                {
                    OnRaiseCustomEvent({|#1:new PreviewEventArgs()|});
                }
         
                protected virtual void OnRaiseCustomEvent(EventArgs e)
                {
                    EventHandler<PreviewEventArgs> raiseEvent = RaiseCustomEvent;
         
                    if (raiseEvent != null)
                    {
                    }
                }
            }
         
            [RequiresPreviewFeatures]
            public class PreviewEventArgs : EventArgs
            {
         
            }

            class Program
            {
                public Program()
                {
                }

                static void Main(string[] args)
                {
                }
            }

            class Subscriber
            {
                private readonly string _id;
         
                public Subscriber(string id, Publisher pub)
                {
                    _id = id;
         
                    pub.RaiseCustomEvent += {|#2:HandleCustomEvent|};
                    pub.RaiseCustomEvent -= {|#3:HandleCustomEvent|};
                }
         
                void HandleCustomEvent(object sender, EventArgs e)
                {
                }
            }
        }";

            var test = TestCS(csInput);
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(0).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "RaiseCustomEvent", "PreviewEventArgs"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.FieldOrEventIsPreviewTypeRule).WithLocation(4).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_fieldOrEventIsPreviewTypeMessage, "RaiseCustomEventNullable", "PreviewEventArgs"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(1).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewEventArgs"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(2).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewEventArgs"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic(DetectPreviewFeatureAnalyzer.GeneralPreviewFeatureAttributeRule).WithLocation(3).WithArguments(string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesMessage, "PreviewEventArgs"), string.Format(CultureInfo.CurrentCulture, (string)DetectPreviewFeatureAnalyzer.s_detectPreviewFeaturesUrl, DetectPreviewFeatureAnalyzer.DefaultURL)));
            await test.RunAsync();
        }
    }
}
