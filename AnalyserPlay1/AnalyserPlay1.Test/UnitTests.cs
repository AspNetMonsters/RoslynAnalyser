using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using AnalyserPlay1;

namespace AnalyserPlay1.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void No_diagnostic_for_allow_anonymous()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Authorization;

    namespace ConsoleApplication1
    {
        [AllowAnonymous]
        class TypeNameController
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AnalyserPlay1",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Diagnostic_for_no_annotation()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Authorization;

    namespace ConsoleApplication1
    {
        class TypeNameController
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AnalyserPlay1",
                Message = String.Format("Controller '{0}' is missing explicit authorization annotations", "TypeNameController"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AnalyserPlay1CodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AnalyserPlay1Analyzer();
        }
    }
}