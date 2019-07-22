// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseAutoValidateAntiforgeryTokenTests : DiagnosticAnalyzerTestBase
    {
        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string microsoftAspNetCoreCSharpSourceCode = @"
using System;
using System.Threading.Tasks;

class MyValidateAntiForgeryAttribute : Attribute
{
}

namespace Microsoft.AspNetCore
{
    namespace Antiforgery
    {
        using Microsoft.AspNetCore.Http;

        public interface IAntiforgery
        {
            Task ValidateRequestAsync (HttpContext httpContext);
        }

        namespace Internal
        {
            using Microsoft.AspNetCore.Http;

            public class DefaultAntiforgery : IAntiforgery
            {
                public Task ValidateRequestAsync (HttpContext httpContext)
                {
                    return null;
                }
            }
        }
    }

    namespace Mvc
    {
        public class AcceptedAtActionResult
        {
        }

        public abstract class ControllerBase
        {
            public virtual AcceptedAtActionResult AcceptedAtAction (string actionName)
            {
                return null;
            }
        }

        public abstract class Controller : ControllerBase
        {
        }

        public class HttpPostAttribute : Attribute
        {
        }

        public class HttpPutAttribute : Attribute
        {
        }

        public class HttpDeleteAttribute : Attribute
        {
        }

        public class HttpPatchAttribute : Attribute
        {
        }

        public class HttpGetAttribute : Attribute
        {
        }

        public sealed class NonActionAttribute : Attribute
        {
        }

        namespace Filters
        {
            public class FilterCollection : System.Collections.ObjectModel.Collection<IFilterMetadata>
            {
                public FilterCollection ()
                {
                }

                public IFilterMetadata Add<TFilterType> () where TFilterType : IFilterMetadata
                {
                    return null;
                }

                public IFilterMetadata Add (Type filterType)
                {
                    return null;
                }
            }

            public interface IFilterMetadata
            {
            }

            public class AuthorizationFilterContext
            {
            }

            public interface IAsyncAuthorizationFilter : IFilterMetadata
            {
                Task OnAuthorizationAsync (AuthorizationFilterContext context);
            }
        }
    }

    namespace Http
    {
        public abstract class HttpContext
        {
        }
    }
}";
            this.VerifyCSharp(
                new[] { source, microsoftAspNetCoreCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_NotCallMethodsOf_DescedantOfIAntiForgery_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public DefaultAntiforgery defaultAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        return null;
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}",
            GetCSharpResultAt(26, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpDelete"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpPostAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpPost]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPost"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpPostAndHttpGetAttributes_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpGet]
    [HttpPost]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(13, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPost"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpPutAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpPut]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPut"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpDeleteAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(13, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpDelete"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpPatchAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpPatch]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(13, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPatch"));
        }

        [Fact]
        public void Test_ChildrenOfController_ActionMethodWithHttpPostAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : Controller
{
    [HttpPost]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPost"));
        }

        [Fact]
        public void Test_ChildrenOfController_ActionMethodWithHttpPutAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : Controller
{
    [HttpPut]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpPut"));
        }

        [Fact]
        public void Test_ChildrenOfController_ActionMethodWithHttpDeleteAttribute_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : Controller
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 44, UseAutoValidateAntiforgeryToken.Rule, "AcceptedAtAction", "HttpDelete"));
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodNotDerivedFromParentClass_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass : ControllerBase
{
}

class TestClass : ControllerBase
{
    [HttpPost]
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}",
            GetCSharpResultAt(12, 35, UseAutoValidateAntiforgeryToken.Rule, "SubAcceptedAtAction", "HttpPost"));
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_DirectlyCallMethodsOf_ChildrenOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public DefaultAntiforgery defaultAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return defaultAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_DirectlyCallMethodsOf_ChildrenOfIAntiForgeryImplicitly_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public IAntiforgery antiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return antiforgery.ValidateRequestAsync(httpContext);
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_DirectlyCallMethodsOf_DescedantOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public MyAntiforgery myAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;
        return myAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class MyAntiforgery : IAntiforgery
{
    public Task ValidateRequestAsync (HttpContext httpContext)
    {
        return null;
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_IndirectlyCallMethodsOf_ChildrenOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public DefaultAntiforgery defaultAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return ThirdMethod(httpContext);
    }

    public Task ThirdMethod(HttpContext httpContext)
    {
        return defaultAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIAsyncAuthorizationFilter_IndirectlyCallMethodsOf_DescedantOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public MyAntiforgery myAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return ThirdMethod(httpContext);
    }

    public Task ThirdMethod(HttpContext httpContext)
    {
        return myAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class MyAntiforgery : IAntiforgery
{
    public Task ValidateRequestAsync (HttpContext httpContext)
    {
        return null;
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_DescedantOfIAsyncAuthorizationFilter_DirectlyCallMethodsOf_ChildrenOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

public interface IMyAsyncAuthorizationFilter : IAsyncAuthorizationFilter
{
}

class FilterClass : IMyAsyncAuthorizationFilter 
{
    public DefaultAntiforgery defaultAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return defaultAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(FilterClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_AddIsAGenericMethod_ChildrenOfIAsyncAuthorizationFilter_DirectlyCallMethodsOf_ChildrenOfIAntiForgery_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class FilterClass : IAsyncAuthorizationFilter 
{
    public DefaultAntiforgery defaultAntiforgery;

    public Task OnAuthorizationAsync (AuthorizationFilterContext context)
    {
        HttpContext httpContext = null;

        return defaultAntiforgery.ValidateRequestAsync(httpContext);
    }
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add<FilterClass>();
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_ChildrenOfIFilterMetadata_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class MyValidateAntiForgeryClass : IFilterMetadata
{
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(MyValidateAntiForgeryClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_Add_DescedantOfIFilterMetadata_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

public interface IMyFilterMetadata : IFilterMetadata
{
}

class MyValidateAntiForgeryClass : IMyFilterMetadata
{
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add(typeof(MyValidateAntiForgeryClass));
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_AddIsAGenericMethod_ChildrenOfIFilterMetadata_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class MyValidateAntiForgeryClass : IFilterMetadata
{
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add<MyValidateAntiForgeryClass>();
    }
}");
        }

        [Fact]
        public void Test_GlobalAntiForgeryFilter_AddIsAGenericMethod_DescedantOfIFilterMetadata_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

public interface IMyFilterMetadata : IFilterMetadata
{
}

class MyValidateAntiForgeryClass : IMyFilterMetadata
{
}

class TestClass : ControllerBase
{
    [HttpDelete]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }

    public void TestMethod ()
    {
        var filterCollection = new FilterCollection ();
        filterCollection.Add<MyValidateAntiForgeryClass>();
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodNotPublic_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class TestClass : ControllerBase
{
    [HttpPost]
    private AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodIsStatic_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class TestClass : ControllerBase
{
    [HttpPost]
    public static AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithNonActionAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class MakeSureValidateAntiForgeryAttributeIsUsedSomeWhereClass
{
}

class TestClass : ControllerBase
{
    [NonAction]
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWitoutAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

class TestClass : ControllerBase
{
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_TypeWithValidateAntiForgeryAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class TestClass : ControllerBase
{
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithValidateAntiForgeryAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

class TestClass : ControllerBase
{
    [MyValidateAntiForgeryAttribute]
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithBothValidateAntiForgeryAndHttpPostAttributes_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

class TestClass : ControllerBase
{
    [MyValidateAntiForgeryAttribute]
    [HttpPost]
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_ChildrenOfControllerBase_ActionMethodWithHttpPostAttributeWhileTypeWithValidateAntiForgeryAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Mvc;

[MyValidateAntiForgeryAttribute]
class TestClass : ControllerBase
{
    [HttpPost]
    public AcceptedAtActionResult SubAcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        [Fact]
        public void Test_NotUsingValidateAntiForgeryAttribute_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Mvc;

class TestClass : ControllerBase
{
    [HttpPost]
    public override AcceptedAtActionResult AcceptedAtAction (string actionName)
    {
        return null;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseAutoValidateAntiforgeryToken();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseAutoValidateAntiforgeryToken();
        }
    }
}
