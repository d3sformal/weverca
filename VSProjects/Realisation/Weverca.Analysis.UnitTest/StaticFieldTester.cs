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
    public class StaticFieldTester
    {

        string StaticFieldSelfTest = @"
            class a
            {
                static $x;
                static function call()
                {
                    self::$x=4;
                }
            }
            a::call();
            $result=a::$x;
        ";

        [TestMethod]
        public void StaticFieldSelf()
        {
            var result = TestUtils.ResultTest(StaticFieldSelfTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        string StaticFieldParentTest = @"
            class base
            {
                static $x;
            }
    
            class a extends base
            {
                
                static function call()
                {
                    parent::$x=4;
                }
            }
            a::call();
            $result=base::$x;
        ";

        [TestMethod]
        public void StaticFieldParent()
        {
            var result = TestUtils.ResultTest(StaticFieldParentTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }


        string StaticFieldTest1 = @"
            class a
            {      
            }

            $result=a::$x;
        ";

        [TestMethod]
        public void StaticField1()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest1);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));
        }

        string StaticFieldTest2 = @"
            $result=a::$x;
        ";

        [TestMethod]
        public void StaticField2()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.CLASS_DOESNT_EXIST));
        }

        string StaticFieldTest3 = @"
            $result=MongoCursor ::$timeout;
        ";

        [TestMethod]
        public void StaticField3()
        {
            var result = TestUtils.ResultTest(StaticFieldTest3);
            TestUtils.testType(result, typeof(IntegerValue));

        }

        string StaticFieldTest4 = @"
            $result=Exception ::$timeout;
        ";

        [TestMethod]
        public void StaticField4()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest4);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));

        }


        string StaticFieldInitTest = @"
            class a
            {    
                static $x=4;  
            }

            $result=a::$x;
        ";

        [TestMethod]
        public void StaticFieldInit()
        {
            var result = TestUtils.ResultTest(StaticFieldInitTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        /// <summary>
        /// Inherited static fields that are not redefined are connected using aliasing.
        /// If a field is changed in one class, it is changed also in other classes
        /// </summary>
        string StaticInheritedFieldAliasingTest1 = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {


        }

        $result = b::$x;
        a::$x=2;
        $result+=b::$x;
        ";

        [TestMethod]
        public void StaticInheritedFieldAliasing1()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldAliasingTest1);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 6);

        }

        /// <summary>
        /// Inherited static fields that are not redefined are connected using aliasing.
        /// If a field is set to be an alias of another variable, it is no longer an alias of fields in other classes.
        /// </summary>
        string StaticInheritedFieldAliasingTest2 = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {


        }

        // a::$x will not be an alias of b::$x
        a::$x = &$c;
        // changes the value of $c and a::$x, does not change the value of b::$x
        $c = 5;
        $result = a::$x + b::$x;
        ";

        [TestMethod]
        public void StaticInheritedFieldAliasing2()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldAliasingTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 9);

        }

        /// <summary>
        /// Inherited static fields that are redefined are not connected using aliasing.
        /// </summary>
        string StaticInheritedFieldAliasingTest3 = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {
        public static $x=4;

        }

        // does not change b::$x
        a::$x = 5;
        $result = a::$x + b::$x;
        ";

        [TestMethod]
        public void StaticInheritedFieldAliasing3()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldAliasingTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 9);

        }

        string StaticInheritedFieldTest2 = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {
         public static $x=6;

        }

        $result = b::$x+a::$x;
        ";

        [TestMethod]
        public void StaticInheritedField2()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 10);

        }

        string StaticInheritedFieldTest3 = @"
        class a 
        {
        public static $x=2;

        }
        class b extends a 
        {

        }

        class c extends b 
        {

        }

         class d extends a 
        {

        }
        d::$x=3;
        $result = b::$x*a::$x*c::$x*d::$x;
        ";

        [TestMethod]
        public void StaticInheritedField3()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 81);

        }

        string IndirectObjectStaticTest = @"
        class a 
        {
        public static $x=2;

        }
        $a=new a();
        $result =$a::$x;
        ";

        [TestMethod]
        public void IndirectObjectStatic()
        {
            var result = TestUtils.ResultTest(IndirectObjectStaticTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);

        }

        string IndirectObjectStaticTest2 = @"
        class a 
        {
        public static $x=2;

        }
        $a=""a"";
        $a::$x=3;
        $result =$a::$x;
        ";

        [TestMethod]
        public void IndirectObjectStatic2()
        {
            var result = TestUtils.ResultTest(IndirectObjectStaticTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 3);

        }

        string IndirectObjectIndirectFieldTest = @"
        class a 
        {
        public static $x=2;

        }
        $a=new a();
		$x=""x"";
        $a::$$x=4;
		$result=$a::$$x;
        ";

        [TestMethod]
        public void IndirectObjectIndirectField()
        {
            var result = TestUtils.ResultTest(IndirectObjectIndirectFieldTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        string IndirectObjectIndirectFieldTest2 = @"
        class a 
        {
        public static $x=2;

        }
        $a=""a"";
		$x=""x"";
        $a::$$x=4;
		$result=$a::$$x;
        ";

        [TestMethod]
        public void IndirectObjectIndirectField2()
        {
            var result = TestUtils.ResultTest(IndirectObjectIndirectFieldTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }


        string DirectObjectIndirectFieldTest3 = @"
        class a 
        {
        public static $x=2;

        }
		$x=""x"";
        a::$$x=4;
		$result=a::$$x;
        ";

        [TestMethod]
        public void IndirectObjectIndirectField3()
        {
            var result = TestUtils.ResultTest(DirectObjectIndirectFieldTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        string DirectObjectIndirectFieldTest4 = @"
        class a 
        {
	        public static $x=2;
	        public static $y=3;
        }

        if(chdir(''))
        {
	        $x='x';
        }
        else
        {
	        $x='y';
        }
        $result=a::$$x;

        ";

        [TestMethod]
        public void IndirectObjectIndirectField4()
        {
            var outSet = TestUtils.Analyze(DirectObjectIndirectFieldTest4);
            var result = outSet.ReadVariable(new VariableIdentifier("result")).ReadMemory(outSet.Snapshot);
            TestUtils.HasValues(result, 2, 3);

        }


    }
}
