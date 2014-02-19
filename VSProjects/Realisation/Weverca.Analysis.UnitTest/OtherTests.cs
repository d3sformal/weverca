using System;
using System.Text;
using System.Linq;
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
    public class Other
    {
        string DieTest = @"
        $result=0;
        die();
        $result=1;
        ";


        [TestMethod]
        public void Die()
        {
            var result = TestUtils.ResultTest(DieTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }

        string AccessProtectedFieldTest = @"
            class a
            {
                protected $x;
            }
            $a=new a();
            $a->x=4; 
        ";


        [TestMethod]
        public void AccessProtectedField()
        {
            var outset = TestUtils.Analyze(AccessProtectedFieldTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

       

        string AccessProtectedFieldTest2 = @"
            class a
            {
                protected $x;
            }
            $a=new a();
            $res=$a->x; 
        ";


        [TestMethod]
        public void AccessProtectedField2()
        {
            var outset = TestUtils.Analyze(AccessProtectedFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

        string AccesPrivateFieldTest = @"
            class a
            {
                private $x;
            }
            $a=new a();
            $res=$a->x; 
        ";


        [TestMethod]
        public void AccessPrivateField()
        {
            var outset = TestUtils.Analyze(AccesPrivateFieldTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

        string AccessPrivateFieldTest2 = @"
            class a
            {
                private $x;
            }

            class b extends a
            {
                
                function b()
                {
                    $p=$this->x;
                }

            }
            $b=new b();
            $b->b();
        ";


        [TestMethod]
        public void AccessPrivateField2()
        {
            var outset = TestUtils.Analyze(AccessPrivateFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

        string AccessPrivateFieldTest3 = @"
            class a
            {
                private $x;

                function b()
                {
                    $p=$this->x;
                }
            }
            $a=new a();
            $a->b(); 
        ";


        [TestMethod]
        public void AccessPrivateField3()
        {
            var outset = TestUtils.Analyze(AccessPrivateFieldTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD) == false);
        }

        string AccessProtectedFieldTest3 = @"
            class a
            {
                protected $x;
            }

            class b extends a
            {
                
                function b()
                {
                    $p=$this->$x;
                }

            }
            $b=new b();
            $b->b();
        ";


        [TestMethod]
        public void AccessProtectedField3()
        {
            var outset = TestUtils.Analyze(AccessProtectedFieldTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD) == false);
        }


       

        string AccesStaticPrivateFieldTest = @"
            class a
            {
                static private $x;
            }
            $res=a::$x; 
        ";


        [TestMethod]
        public void AccesStaticPrivateField()
        {
            var outset = TestUtils.Analyze(AccesStaticPrivateFieldTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }


        string AccesStaticPrivateFieldTest2 = @"
            class a
            {
                static private $x;
            }

            class b  extends a
            {
                
                static function b()
                {
                    $p=self::$x;
                }

            }
            b::b();
        ";


        [TestMethod]
        public void AccesStaticPrivateField2()
        {
            var outset = TestUtils.Analyze(AccesStaticPrivateFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

        string AccesStaticPrivateFieldTest3 = @"
            class a
            {
                static private $x;
            }

            class b extends a
            {
                
                static function b()
                {
                    $p=self::$x;
                }

            }
            b::b();
        ";


        [TestMethod]
        public void AccesStaticPrivateField3()
        {
            var outset = TestUtils.Analyze(AccesStaticPrivateFieldTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD));
        }

        string AccesStaticPrivateFieldTest4 = @"
            class a
            {
                static protected $x;
            }

            class b extends a
            {
                
                static function c()
                {
                    $p=self::$x;
                }

            }
            b::c();
        ";


        [TestMethod]
        public void AccesStaticPrivateField4()
        {
            var outset = TestUtils.Analyze(AccesStaticPrivateFieldTest4);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD)==false);
        }

        string MethodVisibilityTest = @"

            class b
            {
                
                private static function x()
                {
                   
                }

            }
            b::x();
        ";


        [TestMethod]
        public void MethodVisibility()
        {
            var outset = TestUtils.Analyze(MethodVisibilityTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD));
        }


        string MethodVisibilityTest2 = @"

            class b
            {
                
                public static function x()
                {
                   
                }

            }
            b::x();
        ";


        [TestMethod]
        public void MethodVisibility2()
        {
            var outset = TestUtils.Analyze(MethodVisibilityTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD)==false);
        }

        string MethodVisibilityTest3 = @"

            class b
            {
                
                function x()
                {
                   
                }

            }
            $b=new b();
            $b->x();
        ";


        [TestMethod]
        public void MethodVisibility3()
        {
            var outset = TestUtils.Analyze(MethodVisibilityTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD) == false);
        }


        string MethodVisibilityTest4 = @"
            class a
            {
                private function y()
                {

                }
            }
            class b extends a
            {
                
                function x()
                {
                   $this->y();
                }

            }
            $b=new b();
            $b->x();
        ";


        [TestMethod]
        public void MethodVisibility4()
        {
            var outset = TestUtils.Analyze(MethodVisibilityTest4);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD));
        }

        string MethodVisibilityTest5 = @"

           class a
            {
                protected function x()
                {

                }
            }
            class b extends a
            {
                
                function x()
                {
                   $this->y();
                }

            }
            $b=new b();
            $b->x();
        ";


        [TestMethod]
        public void MethodVisibility5()
        {
            var outset = TestUtils.Analyze(MethodVisibilityTest5);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.CALLING_INACCESSIBLE_METHOD) == false);
        }

        string NumberOfArgumentTest = @"

         function a($a,$b)
         {
         }
         a(4);
        ";


        [TestMethod]
        public void NumberOfArgument()
        {
            var outset = TestUtils.Analyze(NumberOfArgumentTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        string NumberOfArgumentTest2 = @"

         function a($a,$b)
         {
         }
         a(4,$a,$a,$a);
        ";


        [TestMethod]
        public void NumberOfArgument2()
        {
            var outset = TestUtils.Analyze(NumberOfArgumentTest2);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }


        string NumberOfArgumentTest3 = @"

         function a($a,$b=4)
         {
            global $result;
            $result=$b;
         }
         a(10);
        ";


        [TestMethod]
        public void NumberOfArgument3()
        {
            var outset = TestUtils.Analyze(NumberOfArgumentTest3);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS)==false);
            TestUtils.testValue(outset.ReadVariable(new VariableIdentifier("result")).ReadMemory(outset.Snapshot).PossibleValues.First(), 4);
        }


    }
}
