using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

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
            var it = (outset.ResolveType(new QualifiedName(new Name("B"))).GetEnumerator());
            it.MoveNext();
            TypeValue B = it.Current as TypeValue;
            Debug.Equals(B.Declaration.Fields.Count, 2);
            Debug.Assert(B.Declaration.Fields[new VariableName("x")] != null);
            Debug.Assert(B.Declaration.Fields[new VariableName("y")] != null);

            it = (outset.ResolveType(new QualifiedName(new Name("A"))).GetEnumerator());
            it.MoveNext();
            TypeValue A = it.Current as TypeValue;
            Debug.Equals(A.Declaration.Fields.Count, 1);
            Debug.Assert(A.Declaration.Fields[new VariableName("x")] != null);

        }
        
        //class having abstract class 

        //class not implementing interface method

        //enxtending final class

        //extending final method


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
    

    }
}
