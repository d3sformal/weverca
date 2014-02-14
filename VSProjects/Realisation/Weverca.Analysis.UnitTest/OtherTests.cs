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

            class b
            {
                
                static function b()
                {
                    $p=self::$x;
                }

            }
            b::b();
        ";


        [TestMethod]
        public void AccesStaticPrivateField4()
        {
            var outset = TestUtils.Analyze(AccesStaticPrivateFieldTest4);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ACCESSING_INACCESSIBLE_FIELD)==false);
        }
 
    }
}
