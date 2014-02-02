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
            if (TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION) == true) {
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
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION));
            
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
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION));

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
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION)==false);

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

        private bool staticRedeclarationCorrect(FlowOutputSet outset, bool nonStaticWithStatic, bool staticWithNonStatic)
        {
            return TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC) == nonStaticWithStatic &&
                TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC) == staticWithNonStatic;
        }

        [TestMethod]
        public void InterfaceCannotOverrideFunction1()
        {
            var outset = TestUtils.Analyze(InterfaceCannotOverrideFunctionTest1);
            Debug.Assert(staticRedeclarationCorrect(outset, false, true));
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
            Debug.Assert(staticRedeclarationCorrect(outset, false, false));
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
            Debug.Assert(staticRedeclarationCorrect(outset, false, false));
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
            Debug.Assert(staticRedeclarationCorrect(outset, true, false));
        }


        string ClassMultipleFieldDeclarationTest=@"
            class a
            {
                private $a,$a;
            }
        ";
        [TestMethod]
        public void ClassMultipleFieldDeclaration()
        {
            var outset = TestUtils.Analyze(ClassMultipleFieldDeclarationTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION));
        }

        string ClassMultipleConstDeclarationTest = @"
            class a
            {
                const a=4;
                const a=4;
            }
        ";
        [TestMethod]
        public void ClassMultipleConstDeclaration()
        {
            var outset = TestUtils.Analyze(ClassMultipleConstDeclarationTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION));
        }

        string ClassMultipleFunctionDeclarationTest = @"
            class a
            {
               static function a()
               {
               }
               function a()
               {
               } 
            }
        ";
        [TestMethod]
        public void ClassMultipleFunctionDeclaration()
        {
            var outset = TestUtils.Analyze(ClassMultipleFunctionDeclarationTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_MULTIPLE_FUNCTION_DECLARATION));
        }

        string ClassAbstractFunctionWithBodyTest = @"
            abstract class a
            {
               abstract function a()
               {
               } 
            }
        ";
        [TestMethod]
        public void ClassAbstractFunctionWithBody()
        {
            var outset = TestUtils.Analyze(ClassAbstractFunctionWithBodyTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ABSTRACT_METHOD_CANNOT_HAVE_BODY));
        }

        string ClassFunctionWithoutBodyTest = @"
            class a
            {
                function a();

            }
        ";
        [TestMethod]
        public void ClasFunctionWithoutBody()
        {
            var outset = TestUtils.Analyze(ClassFunctionWithoutBodyTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.NON_ABSTRACT_METHOD_MUST_HAVE_BODY));
        }

        string ClassWithAbstractFunctionTest = @"
            class a
            {
                abstract function a();

            }
        ";
        [TestMethod]
        public void ClassWithAbstractFunction()
        {
            var outset = TestUtils.Analyze(ClassWithAbstractFunctionTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.NON_ABSTRACT_CLASS_CONTAINS_ABSTRACT_METHOD));
        }

        string ClassInheritingConstantsFromInterfaceTest = @"
            interface b
            {
                const a=4;
            }
            class a implements b
            {
                const a=4;
            }
        ";
        [TestMethod]
        public void ClassInheritingConstantsFromInterface()
        {
            var outset = TestUtils.Analyze(ClassInheritingConstantsFromInterfaceTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT));
        }

        string ClassInheritingConstantsFromInterfaceTest2 = @"
            interface b
            {
                const a=4;
            }
            class a implements b
            {
                const b=4;
            }
        ";
        [TestMethod]
        public void ClassInheritingConstantsFromInterface2()
        {
            var outset = TestUtils.Analyze(ClassInheritingConstantsFromInterfaceTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT)==false);
        }

        string ClassExtendedInheritingConstantsFromInterfaceTest = @"
            interface b
            {
                const a=4;
            }
            class base
            {
                const a=4;
            }
            class a extends base implements b
            {
                
            }
        ";
        [TestMethod]
        public void ClassExtendedInheritingConstantsFromInterface()
        {
            var outset = TestUtils.Analyze(ClassExtendedInheritingConstantsFromInterfaceTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_INTERFACE_CONSTANT));
        }

        string ClassDoenstImplementAllFromInterfaceTest = @"
            interface i
            {
                function f();
            }
            class a implements i
            {

            }
        ";
        [TestMethod]
        public void ClassDoenstImplementAllFromInterface()
        {
            var outset = TestUtils.Analyze(ClassDoenstImplementAllFromInterfaceTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_DOENST_IMPLEMENT_ALL_INTERFACE_METHODS));
        }

        string ClassDoenstCorrectlyImplementInterfaceTest = @"
            interface i
            {
                function f();
            }
            class a implements i
            {
                static  function f(){}
            }
        ";
        [TestMethod]
        public void ClassDoenstCorrectlyImplementInterface()
        {
            var outset = TestUtils.Analyze(ClassDoenstCorrectlyImplementInterfaceTest);
            Debug.Assert(staticRedeclarationCorrect(outset, true, false));
        }

        string ClassDoenstCorrectlyImplementInterfaceTest2 = @"
            interface i
            {
                function f();
            }
            class a implements i
            {
                function f($x,$y){}
            }
        ";
        [TestMethod]
        public void ClassDoenstCorrectlyImplementInterface2()
        {
            var outset = TestUtils.Analyze(ClassDoenstCorrectlyImplementInterfaceTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION));
        }

        string ClassAllreadyExistsTest = @"
            class Exception
            {
               
            }
        ";
        [TestMethod]
        public void ClassAllreadyExists()
        {
            var outset = TestUtils.Analyze(ClassAllreadyExistsTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_ALLREADY_EXISTS));
        }

        string ClassDoesntExistTest = @"
            class x extends aan
            {
               
            }
        ";
        [TestMethod]
        public void ClassDoesntExist()
        {
            var outset = TestUtils.Analyze(ClassDoesntExistTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CLASS_DOESNT_EXIST));
        }

        string ClassCannotExtendFinalClassTest = @"
            final class aan
            {

            }

            class x extends aan
            {
               
            }
        ";
        [TestMethod]
        public void ClassCannotExtendFinalClass()
        {
            var outset = TestUtils.Analyze(ClassCannotExtendFinalClassTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.FINAL_CLASS_CANNOT_BE_EXTENDED));
        }

        string ClassCannotRedeclareStaticFieldTest = @"
            class a
            {
                public $x;
            }

            class b extends a
            {
                static $x;
            }
        ";
        [TestMethod]
        public void ClassCannotRedeclareStaticField()
        {
            var outset = TestUtils.Analyze(ClassCannotRedeclareStaticFieldTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_FIELD_WITH_STATIC));
        }

        string ClassCannotRedeclareStaticFieldTest2 = @"
            class a
            {
                public static $x;
            }

            class b extends a
            {
                public $x;
            }
        ";
        [TestMethod]
        public void ClassCannotRedeclareStaticField2()
        {
            var outset = TestUtils.Analyze(ClassCannotRedeclareStaticFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_FIELD_WITH_NON_STATIC));
        }

        string ClassCannotInheritFinalMethodTest = @"
            class a
            {
                public final function x(){}
            }

            class b extends a
            {
                public function x(){}
            }
        ";
        [TestMethod]
        public void ClassCannotInheritFinalMethod()
        {
            var outset = TestUtils.Analyze(ClassCannotInheritFinalMethodTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD));
        }


        string ClassCannotInheritFinalMethodTest2 = @"
            class b extends Exception 
            {
                public function getPrevious(){}
            }
        ";
        [TestMethod]
        public void ClassCannotInheritFinalMethod2()
        {
            var outset = TestUtils.Analyze(ClassCannotInheritFinalMethodTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_FINAL_METHOD));
        }

        string ClassCannotOverrideMethodWithAbstractTest = @"
            class a
            {
                 function x(){}
            }

            abstract class b extends a
            {
                abstract function x();
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideMethodWithAbstract()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideMethodWithAbstractTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT));
        }

        string ClassCannotOverrideMethodWithAbstractTest2 = @"
           
            abstract class b extends DAtetime
            {
                abstract function getOffset();
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideMethodWithAbstract2()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideMethodWithAbstractTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERRIDE_FUNCTION_WITH_ABSTRACT));
        }

        string ClassCannotOverrideMethodTest = @"
            class a
            {
                 function x($a){}
            }

            class b extends a
            {
                function x(&$a){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideMethod()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideMethodTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION));
        }

        string ClassCannotOverrideMethodTest2 = @"
           
            class b extends DAtetime
            {
                function getOffset($a){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideMethod2()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideMethodTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_OVERWRITE_FUNCTION));
        }


        string ClassCannotOverrideStaticMethodWithNonStaticTest = @"
            class a
            {
                 static function x($a){}
            }

            class b extends a
            {
                function x($a){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideStaticMethodWithNonStatic()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideStaticMethodWithNonStaticTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC));
        }

        string ClassCannotOverrideStaticMethodWithNonStaticTest2 = @"
           
            class b extends DAtetime
            {
                function getLastErrors(){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideStaticMethodWithNonStatic2()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideStaticMethodWithNonStaticTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_NON_STATIC_METHOD_WITH_STATIC));
        }

        string ClassCannotOverrideNonStaticMethodWithStaticTest = @"
            class a
            {
                  function x($a){}
            }

            class b extends a
            {
                static function x($a){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideNonStaticMethodWithStatic()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideNonStaticMethodWithStaticTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC));
        }

        string ClassCannotOverrideNonStaticMethodWithStaticTest2 = @"
           
            class b extends DAtetime
            {
                static function moDify ($x ){}
            }
        ";
        [TestMethod]
        public void ClassCannotOverrideNonStaticMethodWithStatic2()
        {
            var outset = TestUtils.Analyze(ClassCannotOverrideNonStaticMethodWithStaticTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_REDECLARE_STATIC_METHOD_WITH_NON_STATIC));
        }



        string ClassListOfInheritedClassesTest = @"
        class a {}
        class b extends a{}
        class c extends b{}
        class d extends c{}
        class e extends d{}
        ";
        [TestMethod]
        public void ClassListOfInheritedClasses()
        {
            var outset = TestUtils.Analyze(ClassListOfInheritedClassesTest);
            var a = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("a"))))[0];
            Debug.Assert(a.Declaration.BaseClasses.Count == 0);
            var b = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("b"))))[0];
            Debug.Assert(b.Declaration.BaseClasses.Count == 1);
            var c = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("c"))))[0];
            Debug.Assert(c.Declaration.BaseClasses.Count == 2);
            var d = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("d"))))[0];
            Debug.Assert(d.Declaration.BaseClasses.Count == 3);
            var e = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("e"))))[0];
            Debug.Assert(e.Declaration.BaseClasses.Count == 4);
       
        }

        string ClassListOfInheritedClassesTest2 = @"
        class a extends BadFunctionCallException{}
        class b extends a{}
        class c extends b{}
        class d extends c{}
        class e extends d{}
        ";
        [TestMethod]
        public void ClassListOfInheritedClasses2()
        {
            var outset = TestUtils.Analyze(ClassListOfInheritedClassesTest2);
            var a = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("a"))))[0];
            Debug.Assert(a.Declaration.BaseClasses.Count == 3);
            var e = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("e"))))[0];
            Debug.Assert(e.Declaration.BaseClasses.Count == 7);
           
        }

        string ClassListOfInheritedClassesTest3 = @"
        class a extends PharData{}
        ";
        [TestMethod]
        public void ClassListOfInheritedClasses3()
        {
            var outset = TestUtils.Analyze(ClassListOfInheritedClassesTest3);
            var a = new List<TypeValue>(outset.ResolveType(new QualifiedName(new Name("a"))))[0];
            Debug.Assert(a.Declaration.BaseClasses.Count == 6);
           
        }

        string InitAbstractClass = @"
        abstract class a{}
        $x=new a();
         ";
        
        [TestMethod]
        public void InitAbstractClassTest()
        {
            var outset = TestUtils.Analyze(InitAbstractClass);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_INSTANCIATE_ABSTRACT_CLASS));
        }

        string InitInterface = @"
        interface b{}
        $x=new b();
         ";

        [TestMethod]
        public void InitInterfaceTest()
        {
            var outset = TestUtils.Analyze(InitInterface);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_INSTANCIATE_INTERFACE));
        }

        string InitObject = @"
        class base
        {
        public $x=4;
        public $base=""aa"";
        }
        class a extends base
        {

        public $x=""s"";
        public $y=5;
        public $z=6;
        public $a=""a"";
        public $ppp;

        }

        $x=new a();
        $result1=$x->x;
        $result2=$x->base;
        $result3=$x->z;

        ";

        [TestMethod]
        public void InitObjectTest()
        {
            var outset = TestUtils.Analyze(InitObject);
            TestUtils.HasValues(outset.ReadVariable(new VariableIdentifier("result1")).ReadMemory(outset.Snapshot), "s");
            TestUtils.HasValues(outset.ReadVariable(new VariableIdentifier("result2")).ReadMemory(outset.Snapshot), "aa");
            TestUtils.HasValues(outset.ReadVariable(new VariableIdentifier("result3")).ReadMemory(outset.Snapshot), 6);
        }

    }
}
