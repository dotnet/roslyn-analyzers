// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{

    public class ConsiderPassingBaseTypesAsParametersTests : DiagnosticAnalyzerTestBase
    {
        #region Test Harness

        internal static readonly string CA1011Name = ConsiderPassingBaseTypesAsParametersAnalyzer.RuleId;
        internal static readonly string CA1011Message = MicrosoftApiDesignGuidelinesAnalyzersResources.ConsiderPassingBaseTypesAsParametersMessage;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConsiderPassingBaseTypesAsParametersAnalyzer();
        }

        // Message Args: paramName, methodName, paramType, baseType
        private static DiagnosticResult GetCA1011CSharpResultAt(int line, int column, params string[] messageArgs)
        {
            return GetCSharpResultAt(line, column, CA1011Name, string.Format(CA1011Message, messageArgs));
        }

        private static DiagnosticResult GetCA1011BasicResultAt(int line, int column, params string[] messageArgs)
        {
            return GetBasicResultAt(line, column, CA1011Name, string.Format(CA1011Message, messageArgs));
        }

        #endregion

        #region Simple C# Tests

        const string CommonSomething = @"
    using System;
    using System.Threading.Tasks;

    class SomethingBase
    {
        public int Value = 42;
        public void SomeAction() { }
        public int PropVal { get; set; }
        public string PropGetVal { get; set; }
        public event EventHandler OnNothingGood;
        public virtual void SomeVirtualAction() { }
    }
    class Something: SomethingBase {
        public int SubValue = 32;
        public override void SomeVirtualAction() { }
    }
";

        [Fact]
        public void TestCSCheckForBaseMemberReferences()
        {
            // Test methods which only access base members (i.e. should report diagnostic)
            var code = CommonSomething + @"
    class BaseMemberReferences
    {
        int FieldRef(Something p1) { return p1.Value; }
        void MethodBind(Something p2) { p2.SomeAction(); }
        int PropertyRef(Something p3) { return p3.PropVal; }
        string ROPropertyRef(Something p4) { return p4.PropGetVal ?? ""Nop""; }
        string MultiMemberRef(Something p5) { return p5.PropGetVal ?? p5.Value.ToString(); }
    }
";
            var derivedClass = "Something";
            var baseClass = "SomethingBase";

            VerifyCSharp(code,
                GetCA1011CSharpResultAt(21, 32, "p1", "FieldRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(22, 35, "p2", "MethodBind", derivedClass, baseClass),
                GetCA1011CSharpResultAt(23, 35, "p3", "PropertyRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(24, 40, "p4", "ROPropertyRef", derivedClass, baseClass),
                GetCA1011CSharpResultAt(25, 41, "p5", "MultiMemberRef", derivedClass, baseClass));
        }

        [Fact]
        public void TestCSCheckForDerivedMemberReferences()
        {
            // Test methods which access base AND derived members (i.e. should NOT report diagnostic)
            var code = CommonSomething + @"
    class DerivedMemberReferences
    {
        int FieldRef(Something p1) { return p1.SubValue; }
        void MethodBind(Something p2) { p2.SomeVirtualAction(); }
        int PropertyAndField(Something p3) { return p3.PropVal + p3.SubValue; }

        string AccessInLambda(Something p5) {
            Func<string> getV = () => p5.SubValue.ToString();
            return p5.PropGetVal ?? getV();
        }

        async Task<string> AccessInAsyncTask(Something p6)
        {
            return p6.PropGetVal ?? await Task.Run(() => p6.SubValue.ToString());
        }
    }
";
            VerifyCSharp(code);
        }

        #endregion

        #region Port FxCop Tests

        // From BadDesignCS\UnitTests.cs
        [Fact]
        public void FxCopBadDesignUnitTests()
        {
            string code = @"
using System;
using System.IO;

namespace FxCop.Tests
{
    public sealed class MyReader
    {
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void Read(FileStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Console.WriteLine(this);
            byte[] myList = new byte[1024];
            int numBytesToRead = (int)myList.Length;
            int numBytesRead = 0;
            stream.ReadAsync(myList, numBytesRead, numBytesToRead);
        }
 
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void ReadLine(FileStream stream)
        {
            Console.WriteLine(this);
            using (StreamReader reader = new StreamReader(stream))
            {
                reader.ReadLine();
            }
        }
        
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void GenericRead<T>(T stream) where T : FileStream
        {
         Console.WriteLine(this);
         byte[] myList = new byte[1024];
         int numBytesToRead = (int)myList.Length;
         int numBytesRead = 0;
         stream.ReadAsync(myList, numBytesRead, numBytesToRead);
        }

    }
}";
            VerifyCSharp(code,
                GetCA1011CSharpResultAt(10, 37, "stream", "Read", "System.IO.FileStream", "System.IO.Stream"),
                GetCA1011CSharpResultAt(21, 41, "stream", "ReadLine", "System.IO.FileStream", "System.IO.Stream"));
        }

        // From BadDesignCS\ConsiderPassingBaseTypesAsParametersTests.cs
        [Fact]
        public void FxCopBadDesignAnalyzerTests()
        {
            string code = @"
using System;
using System.Collections;
using System.IO;

namespace FxCop.Tests
{
    public interface IInterface1<in T>
    {
    }

    public interface IInterface2<out T>
    {
    }

    public interface IInterface3<in T> : IInterface1<T>
    {
    }

    public interface IInterface4<out T> : IInterface2<T>
    {
    }

    public class BaseWithNothing
    {
    }

    public class FirstDerivedWithInterfaces<T, U> : BaseWithNothing, IInterface1<T>, IInterface2<U>
    {
    }

    public class SecondDerivedWithInterfaces<T, U> : FirstDerivedWithInterfaces<T, U>, IInterface3<T>, IInterface4<U>
    {
    }

    public static class UsingBaseClassesOnly
    {
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule should fire because having FirstDerivedWithInterfaces<string, string> passed in would work just fine.
            HelperMethod1(data);
            HelperMethod2(data);
        }

        public static void HelperMethod1(FirstDerivedWithInterfaces<string, string> data)
        {
            Console.WriteLine(data);
        }

        public static void HelperMethod2(BaseWithNothing data)
        {
            Console.WriteLine(data);
        }
    }

    public static class UsingInterfaceTypesOnly
    {
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule should fire because having IInterface3<string> passed in would work just fine.
            HelperMethod1(data);
            HelperMethod2(data);
        }

        public static void HelperMethod1(IInterface1<string> data)
        {
            Console.WriteLine(data);
        }

        public static void HelperMethod2(IInterface3<string> data)
        {
            Console.WriteLine(data);
        }
    }

    public static class ContravariantInterfaceTypes
    {
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<object, object> data)
        {
            // The rule should fire because having IInterface3<IComparable> passed in would work just fine.
            // It uses the contravariance behavior of IInterface3 and IInterface1.
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(IInterface1<string> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(IInterface3<IComparable> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(IInterface3<string> data)
        {
            Console.WriteLine(data);
        }
    }

    public static class CovariantInterfaceTypes
    {
        // [ExpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule should fire because having IInterface4<IComparable> passed in would work just fine.
            // It uses the covariance behavior of IInterface4 and IInterface2.
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(IInterface2<IComparable> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(IInterface4<object> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(IInterface4<IComparable> data)
        {
            Console.WriteLine(data);
        }
    }
}
";
            VerifyCSharp(code,
                GetCA1011CSharpResultAt(39, 88, "data", "ViolatingMethod", "FxCop.Tests.SecondDerivedWithInterfaces<string, string>", "FxCop.Tests.FirstDerivedWithInterfaces<string, string>"));
        }

        // From GoodDesignCS\UnitTests.cs
        [Fact]
        public void FxCopGoodDesignUnitTests()
        {
            string code = @"
using System;
using System.IO;

namespace Microsoft.FxCop.Tests.Design.Good
{
    public class BaseConsiderPassingBaseTypesAsParametersTest
    {
        public void BaseMethod()
        {
            Console.WriteLine(this);
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public virtual void OtherBaseMethod(FileStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Console.WriteLine(this);
            byte[] myList = new byte[1024];
            int numBytesToRead = (int)myList.Length;
            int numBytesRead = 0;
            stream.Read(myList, numBytesRead, numBytesToRead);
        }
    }

    public sealed class ConsiderPassingBaseTypesAsParametersTest : BaseConsiderPassingBaseTypesAsParametersTest
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void Read(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            Console.WriteLine(this);
            byte[] myList = new byte[1024];
            int numBytesToRead = (int)myList.Length;
            int numBytesRead = 0;
            input.Read(myList, numBytesRead, numBytesToRead);
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void ReadLine(Stream input)
        {
            Console.WriteLine(this);
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(input);
                reader.ReadLine();
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void Bar()
        {
            Console.WriteLine(this);
            //shouldn't fire on this pointers
            this.BaseMethod();
        }

        void DummyMethod(BaseConsiderPassingBaseTypesAsParametersTest ourBase, OtherBaseConsiderPassingBaseTypesAsParametersTest otherBase)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(ourBase);
            System.Console.WriteLine(otherBase);
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public override void OtherBaseMethod(FileStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Console.WriteLine(this);
            byte[] myList = new byte[1024];
            int numBytesToRead = (int)myList.Length;
            int numBytesRead = 0;
            stream.Read(myList, numBytesRead, numBytesToRead);
        }


        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void MyBar(OtherBaseConsiderPassingBaseTypesAsParametersTest otherBase)
        {
            if (otherBase == null) throw new ArgumentNullException(nameof(otherBase));
            Console.WriteLine(this);
            DummyMethod(otherBase, otherBase.TheBase as OtherBaseConsiderPassingBaseTypesAsParametersTest);
        }
    }

    public sealed class OtherBaseConsiderPassingBaseTypesAsParametersTest : BaseConsiderPassingBaseTypesAsParametersTest
    {
        public BaseConsiderPassingBaseTypesAsParametersTest TheBase
        {
            get { Console.WriteLine(this); return new OtherBaseConsiderPassingBaseTypesAsParametersTest(); }
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public override void OtherBaseMethod(FileStream stream)
        {
        }
    }
}
";
            VerifyCSharp(code);
        }

        // From GoodDesignCS\ConsiderPassingBaseTypesAsParametersTests.cs
        [Fact]
        public void FxCopGoodDesignAnalyzerTests()
        {
            string code = @"
using System;
using System.Collections;
using System.IO;

namespace Microsoft.FxCop.Tests.Design.Good.ConsiderPassingBaseTypesAsParametersTests
{
    public class ItemCollection : CollectionBase
    {
        public int Add(CollectionItem value)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(value);
            return 0;
        }
        public bool Contains(CollectionItem value)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(value);
            return true;
        }
        public int IndexOf(CollectionItem value)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(value);
            return 0;
        }
        public void Insert(int index, CollectionItem value)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(index);
            System.Console.WriteLine(value);
        }
        public void Remove(CollectionItem value)
        {
            System.Console.WriteLine(this);
            System.Console.WriteLine(value);
        }
        public bool IsFixedSize
        {
            get { System.Console.WriteLine(this); return true; }
            set
            {
                System.Console.WriteLine(this);
                System.Console.WriteLine(value);
            }
        }
        public bool IsReadOnly
        {
            get { System.Console.WriteLine(this); return true; }
            set
            {
                System.Console.WriteLine(this);
                System.Console.WriteLine(value);
            }
        }
        public CollectionItem this[int index]
        {
            // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
            get { return (CollectionItem)((IList)this)[index]; }
            set { ((IList)this)[index] = value; }
        }
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void CopyTo(CollectionItem[] array, int index)
        { ((ICollection)this).CopyTo(array, index); }
    }

    public delegate void ProgressUpdateCaller(SmoothProgressBar bar);

    public class ValidParameterType
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public void ProgressUpdate(SmoothProgressBar bar)
        {
            if (bar == null) throw new ArgumentNullException(nameof(bar));
            if (bar.InvokeRequired)
            {
                bar.BeginInvoke(new ProgressUpdateCaller(ProgressUpdate), new object[] { bar });
            }
            else
            {
                ++bar.Value;
            }
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static int DoSomething(HasPublicField input)
        {
            if (input == null) return 0;
            return input.Field;
        }

        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public virtual void VirtualMethodsCanHaveDerivedParameterTypes(FileStream stream)
        {
            Console.WriteLine(stream.ReadByte());
        }
    }

    public static class Debug
    {
        public static void Print(string value)
        {
            // DDB 190946 - ConsiderPassingBaseTypesAsParameters ICE's when visiting a method that calls an method that takes an __arglist
            Print(value, __arglist(value));
        }

        public static void Print(string value, __arglist)
        {
            Console.WriteLine(value);
        }
    }

    public interface IInterface1<in T>
    {
    }

    public interface IInterface2<out T>
    {
    }

    public interface IInterface3<in T> : IInterface1<T>
    {
    }

    public interface IInterface4<out T> : IInterface2<T>
    {
    }

    public class BaseWithNothing
    {
    }

    public class FirstDerivedWithInterfaces<T, U> : BaseWithNothing, IInterface1<T>, IInterface2<U>
    {
    }

    public class SecondDerivedWithInterfaces<T, U> : FirstDerivedWithInterfaces<T, U>, IInterface3<T>, IInterface4<U>
    {
    }

    public static class UsingBaseClassesOnly
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule doesn't fire for this method because HelperMethod1 takes in a type of the same type
            // as data
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(SecondDerivedWithInterfaces<string, string> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(BaseWithNothing data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(FirstDerivedWithInterfaces<string, string> data)
        {
            Console.WriteLine(data);
        }
    }

    public static class UsingInterfaceTypesOnly
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule doesn't fire for this method because IInterface2 and IInterface3 are on different inheritance chain.
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(IInterface1<string> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(IInterface2<string> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(IInterface3<string> data)
        {
            Console.WriteLine(data);
        }
    }

    public static class ContravariantInterfaceTypes
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(IInterface3<IComparable> data)
        {
            // The rule doesn't fire for this method because IInterface3<object> is assignable to all other interface usages and
            // IInterface3<IComparable> is being used.
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(IInterface1<string> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(IInterface3<IComparable> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(IInterface3<string> data)
        {
            Console.WriteLine(data);
        }
    }

    public static class CovariantInterfaceTypes
    {
        // [UnexpectedWarning(ConsiderPassingBaseTypesAsParameters, DesignRules)]
        public static void ViolatingMethod(SecondDerivedWithInterfaces<string, string> data)
        {
            // The rule doesn't fire for this method because IInterface4<IComparable> and IInterface4<ICloneable> aren't assignable to each
            // other.
            HelperMethod1(data);
            HelperMethod2(data);
            HelperMethod3(data);
        }

        public static void HelperMethod1(IInterface2<IComparable> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod2(IInterface4<ICloneable> data)
        {
            Console.WriteLine(data);
        }
        public static void HelperMethod3(IInterface4<ICloneable> data)
        {
            Console.WriteLine(data);
        }
    }
}
";
            VerifyCSharp(code);
        }

        #endregion
    }
}