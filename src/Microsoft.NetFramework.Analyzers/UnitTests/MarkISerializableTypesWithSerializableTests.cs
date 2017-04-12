﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Test.Utilities;

namespace Microsoft.NetFramework.Analyzers.UnitTests
{
    public partial class MarkISerializableTypesWithSerializableTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SerializationRulesDiagnosticAnalyzer();
        }

        #region CA2237

        [Fact]
        public void CA2237SerializableMissingAttr()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;
                public class CA2237SerializableMissingAttr : ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }",
                GetCA2237CSharpResultAt(4, 30, "CA2237SerializableMissingAttr"));

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization
                Public Class CA2237SerializableMissingAttr
                    Implements ISerializable
                
                    Protected Sub New(context As StreamingContext, info As SerializationInfo)
                    End Sub

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class",
                GetCA2237BasicResultAt(4, 30, "CA2237SerializableMissingAttr"));
        }

        [Fact]
        public void CA2237SerializableInternal()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;
                class CA2237SerializableInternal : ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }");

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization
                Friend Class CA2237SerializableInternal 
                    Implements ISerializable
                
                    Protected Sub New(context As StreamingContext, info As SerializationInfo)
                    End Sub

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class");
        }

        [Fact]
        public void CA2237SerializableProperWithScope()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;

                [|class CA2237SerializableInternal : ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }|]

                [Serializable]
                public class CA2237SerializableProper : ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }");

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization

                [|Friend Class CA2237SerializableInternal 
                    Implements ISerializable
                
                    Protected Sub New(context As StreamingContext, info As SerializationInfo)
                    End Sub

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class|]

                <Serializable>
                Public Class CA2237SerializableProper 
                    Implements ISerializable

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class");
        }

        [Fact]
        public void CA2237SerializableWithBase()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;
                public class CA2237SerializableWithBase : Base, ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }
                public class Base { }");

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization
                Public Class CA2237SerializableWithBase
                    Inherits Base 
                    Implements ISerializable
                
                    Protected Sub New(context As StreamingContext, info As SerializationInfo)
                    End Sub

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class
                Public Class Base
                End Class");
        }

        [Fact]
        public void CA2237SerializableWithBaseAttr()
        {
            VerifyCSharp(@"
                using System;
                using System.Runtime.Serialization;
                public class CA2237SerializableWithBaseAttr : BaseAttr, ISerializable
                {
                    public void GetObjectData(SerializationInfo info, StreamingContext context)
                    {
                        throw new NotImplementedException();
                    }
                }
                [Serializable]
                public class BaseAttr { }",
                GetCA2237CSharpResultAt(4, 30, "CA2237SerializableWithBaseAttr"));

            VerifyBasic(@"
                Imports System
                Imports System.Runtime.Serialization
                Public Class CA2237SerializableWithBaseAttr
                    Inherits BaseWithAttr 
                    Implements ISerializable
                
                    Protected Sub New(context As StreamingContext, info As SerializationInfo)
                    End Sub

                    Public Sub GetObjectData(info as SerializationInfo, context as StreamingContext) Implements ISerializable.GetObjectData
                        throw new NotImplementedException()
                    End Sub
                End Class
                <Serializable>
                Public Class BaseWithAttr
                End Class",
                GetCA2237BasicResultAt(4, 30, "CA2237SerializableWithBaseAttr"));
        }

        internal static readonly string CA2237Message = MicrosoftNetFrameworkAnalyzersResources.MarkISerializableTypesWithSerializableMessage;

        private static DiagnosticResult GetCA2237CSharpResultAt(int line, int column, string objectName)
        {
            return GetCSharpResultAt(line, column, SerializationRulesDiagnosticAnalyzer.RuleCA2237Id, string.Format(CA2237Message, objectName));
        }

        private static DiagnosticResult GetCA2237BasicResultAt(int line, int column, string objectName)
        {
            return GetBasicResultAt(line, column, SerializationRulesDiagnosticAnalyzer.RuleCA2237Id, string.Format(CA2237Message, objectName));
        }

        #endregion
    }
}
