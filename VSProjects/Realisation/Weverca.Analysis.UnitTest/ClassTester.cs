using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ClassTester
    {

        string ClassExtendTest = @"
           class A
           {
               public $x=4;
           }
           class B extends A
           {
               public $y=4;
           }
        ";

        [TestMethod]
        public void ClassExtend()
        {
            var outset = TestUtils.Analyze(ClassExtendTest);
            var classB = new QualifiedName(new Name("B"));
            var classA = new QualifiedName(new Name("A"));          

            var it = (outset.ResolveType(classB).GetEnumerator());
            it.MoveNext();
            TypeValue B = it.Current as TypeValue;
            Debug.Equals(B.Declaration.Fields.Count, 2);
            Debug.Assert(B.Declaration.Fields[new FieldIdentifier(classA, new VariableName("x"))] != null);
            Debug.Assert(B.Declaration.Fields[new FieldIdentifier(classB, new VariableName("y"))] != null);

            it = (outset.ResolveType(classA).GetEnumerator());
            it.MoveNext();
            TypeValue A = it.Current as TypeValue;
            Debug.Equals(A.Declaration.Fields.Count, 1);
            Debug.Assert(A.Declaration.Fields[new FieldIdentifier(classA, new VariableName("x"))] != null);

        }
        
        //interface tests

        string InterfaceContainingFieldTest = @"
           interface A
           {
               public $x=4;
           }
        ";
        [TestMethod]
        public void InterfaceContainingField()
        {
            var outset = TestUtils.Analyze(InterfaceContainingFieldTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS));
        }

        string InterfaceContainingContantTest = @"
           interface A
           {
               const x=4;
           }
        ";
        [TestMethod]
        public void InterfaceContainingConstant()
        {
            var outset = TestUtils.Analyze(InterfaceContainingContantTest);
            if (TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_CONTAIN_FIELDS) == true)
            {
                Debug.Fail();
            }
        }

        string InterfaceContainingFinalMethodTest = @"
           interface A
           {
              final function x();
           }
        ";
        [TestMethod]
        public void InterfaceContainingFinalMethod()
        {
            var outset = TestUtils.Analyze(InterfaceContainingFinalMethodTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_BE_FINAL));
        }

        string InterfaceContainingPrivateMethodTest = @"
           interface A
           {
              private function x();
           }
        ";
        [TestMethod]
        public void InterfaceContainingPrivateMethod()
        {
            var outset = TestUtils.Analyze(InterfaceContainingPrivateMethodTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_METHOD_MUST_BE_PUBLIC));
        }

        string InterfaceDoesntExistTest = @"
           interface A extends p
           {
           }
        ";
        [TestMethod]
        public void InterfaceDoesntExist()
        {
            var outset = TestUtils.Analyze(InterfaceDoesntExistTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_DOESNT_EXIST));
        }

        string InterfaceCannotInheritFunctionTest = @"
           interface a
           {
                function x();
           }
           interface b
           {
                function x();
           }

           interface c extends a,b
           {
           }
        ";
        [TestMethod]
        public void InterfaceCannotInheritFunction()
        {
            var outset = TestUtils.Analyze(InterfaceCannotInheritFunctionTest);
            if (TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION) == true) {
                Debug.Fail();
            }
        }

        string InterfaceCannotInheritFunctionTest2 = @"
           interface a
           {
                function x();
           }
           interface b
           {
                function x($a);
           }

           interface c extends a,b
           {
           }
        ";
        [TestMethod]
        public void InterfaceCannotInheritFunction2()
        {
            var outset = TestUtils.Analyze(InterfaceCannotInheritFunctionTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION));
            
        }

        string InterfaceInheritTest = @"
           interface z
           {
                function w();
           }
           interface a
           {
                function x();
           }
           interface b
           {
                function y();
           }

           interface c extends a,b
           {
                function z();
           }

           interface d extends c,z
           {

           }        
    
        ";

        [TestMethod]
        public void InterfaceInherit()
        {
            var outset = TestUtils.Analyze(InterfaceInheritTest);
            var enumerator=outset.ResolveType(new QualifiedName(new Name("d"))).GetEnumerator();
            enumerator.MoveNext();
            var typeValue = enumerator.Current;
            Debug.Assert((typeValue as TypeValue).Declaration.SourceCodeMethods.Count==4);
        }

        string InterfaceMethodCannotHaveImplementationTest = @"
           interface a
           {
                function x(){}
           }
           
        ";
        [TestMethod]
        public void InterfaceMethodCannotHaveImplementation()
        {
            var outset = TestUtils.Analyze(InterfaceMethodCannotHaveImplementationTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_METHOD_CANNOT_HAVE_IMPLEMENTATION));

        }

        string InterfaceCannotInheritMehodTrueTest = @"
        interface a extends ArrayAccess
        {
        function offsetGet(&$a);
        }";

        [TestMethod]
        public void InterfaceCannotInheritMehodTrue()
        {
            var outset = TestUtils.Analyze(InterfaceCannotInheritMehodTrueTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION));

        }

        string InterfaceCannotInheritMehodFalseTest = @"
        interface a extends ArrayAccess
        {
        function offsetGet($a);
        }";

        [TestMethod]
        public void InterfaceCannotInheritMehodFalse()
        {
            var outset = TestUtils.Analyze(InterfaceCannotInheritMehodFalseTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.INTERFACE_CANNOT_OVER_WRITE_FUNCTION)==false);

        }

        string InterfaceCannotOverrideConstant = @"
        interface a 
        {
        const a=4;
        }
        interface b extends a 
        {
        const a=4;
        }

        ";

        [TestMethod]
        public void InterfaceCannotOverrideConstantTest()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideConstant);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT) == true);

        }

        string InterfaceOverridesConstant = @"
        interface a 
        {
        const a=4;
        }
        interface b extends a 
        {
        const b=4;
        }

        ";

        [TestMethod]
        public void InterfaceOverridesConstantTest()
        {
            var outset = TestUtils.Analyze(InterfaceOverridesConstant);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT) == false);

        }


        string InterfaceCannotOverrideFunctionTest1 = @"
        interface a 
        {
            static function x();
        }
        interface b extends a 
        {
            function x();
        }

        ";

        [TestMethod]
        public void InterfaceCannotOverrideFunction1()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideFunctionTest1);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC));
        }

        string InterfaceCannotOverrideFunctionTest2 = @"
        interface a 
        {
            static function x();
        }
        interface b extends a 
        {
            static function x();
        }

        ";

        [TestMethod]
        public void InterfaceCannotOverrideFunction2()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideFunctionTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC) == false);
        }

        string InterfaceCannotOverrideFunctionTest3 = @"
        interface a 
        {
            function x();
        }
        interface b extends a 
        {
            function x();
        }

        ";

        [TestMethod]
        public void InterfaceCannotOverrideFunction3()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideFunctionTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC) == false);
        }

        string InterfaceCannotOverrideFunctionTest4 = @"
        interface a 
        {
             function x();
        }
        interface b extends a 
        {
            static function x();
        }

        ";

        [TestMethod]
        public void InterfaceCannotOverrideFunction4()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideFunctionTest4);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC));
        }

        
    }
}
