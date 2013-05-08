using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class IndicatorCodeMetricTests
    {

        #region Eval indicator tests
        readonly IEnumerable<SourceTest> EvalPositiveTests = new SourceTest[]{
            new SourceTest("Test on eval detection outside function/method.", @"
                eval('$x=3');
            "),

            new SourceTest("Test on eval detection inside global function", @"
                function test($param){
                    eval($param);
                }
            "),

            new SourceTest("Test on eval detection inside method declaration", @"
                class testClass{
                    function testMethod($param){
                        eval($param);
                    }
                }
            ")
        };

        readonly IEnumerable<SourceTest> EvalNegativeTests = new SourceTest[]{
            new SourceTest("No eval is present",TestingUtilities.HelloWorldSource)
        };

        #endregion


        #region Session indicator tests
        readonly IEnumerable<SourceTest> SessionPositiveTests = new SourceTest[]{
            new SourceTest("Session function call outside function/method",@"
                session_start();
            "),
            new SourceTest("Session function call inside global function",@"
                function test($param){
                    session_write_close();
                }
            "),
            new SourceTest("Session function call inside method",@"
                class testClass{
                    function testMethod($param){
                        session_unset();
                    }
                }
            ")
        };

        readonly IEnumerable<SourceTest> SessionNegativeTests = new SourceTest[]{
            new SourceTest("No session is present",TestingUtilities.HelloWorldSource)
        };
        #endregion

        #region SuperGlobal variable indicator tests
        readonly IEnumerable<SourceTest> SuperGlobalVarPositiveTests = new SourceTest[]{
            new SourceTest("Super global outside function/method",@"
                $_POST['test']='hello';
            "),
            new SourceTest("Super global inside global function",@"
                function test($param){
                    $_GET['test']='world';
                }
            "),
            new SourceTest("Super global inside method",@"
                class testClass{
                    function testMethod($param){
                        $_SESSION['test']='!';
                    }
                }
            "),
            new SourceTest("Super global as rvalue",@"
                class testClass{
                    function testMethod($param){
                        $test=$GLOBALS['test'];
                    }
                }
            ")
        };

        readonly IEnumerable<SourceTest> SuperGlobalVarNegativeTests = new SourceTest[]{
            new SourceTest("No session is present",TestingUtilities.HelloWorldSource),
            new SourceTest("No super global variable",@"
                $notSuperGlobal=3;
                function test($param){
                    global $notSuperGlobal;
                    $notSuperGlobal=$notSuperGlobal+1;
                }
            "),
        };
        #endregion

        #region ClassPresence tests

        readonly SourceTest[] classPresencePositiveTests = new SourceTest[] {
            new SourceTest("basic class", @"
                class TestClass{
	                public static function TestMethod($Prava, $Patka){
		                return false;
		            }
	            }"),
            new SourceTest("class implementing interface", @"
                interface iTemplate
                {
                    public function setVariable($name, $var);
                    public function getHtml($template);
                }

                // Implement the interface
                // This will work
                class Template implements iTemplate
                {
                    private $vars = array();
  
                    public function setVariable($name, $var)
                    {
                        $this->vars[$name] = $var;
                    }
  
                    public function getHtml($template)
                    {
                        foreach($this->vars as $name => $value) {
                            $template = str_replace('{' . $name . '}', $value, $template);
                        }
 
                        return $template;
                    }
                }")
        };

        readonly SourceTest[] classPresenceNegativeTests = new SourceTest[] {
            new SourceTest("function only", @"
                function setVariable($name, $var)
                {
                    $this->vars[$name] = $var;
                }"),
            new SourceTest("interface only", @"
                interface iTemplate
                {
                    public function setVariable($name, $var);
                    public function getHtml($template);
                }")
        };

        #endregion

        #region MySQL tests

        readonly SourceTest[] mySQLPositiveTests = new SourceTest[] {
            new SourceTest("mysql_connect", "$link = @mysql_connect(\"localhost\", \"login\", \"password\") or die (\"<p>Connection failed!</p>\");"),
            new SourceTest("mysql_query", "$result = mysql_query(\"SELECT * FROM Table\");")
        };

        #endregion

        [TestMethod]
        public void Eval()
        {
            var hasEval = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Eval);
            var doesntHaveEval = TestingUtilities.GetNegation(hasEval);

            TestingUtilities.RunTests(hasEval, EvalPositiveTests);
            TestingUtilities.RunTests(doesntHaveEval, EvalNegativeTests);
        }

        [TestMethod]
        public void Session()
        {
            var hasSession = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Session);
            var doesntHaveSession = TestingUtilities.GetNegation(hasSession);

            TestingUtilities.RunTests(hasSession, SessionPositiveTests);
            TestingUtilities.RunTests(doesntHaveSession, SessionNegativeTests);
        }

        [TestMethod]
        public void SuperGlobalVar()
        {
            var hasSuperGlobalVar= TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.SuperGlobalVariable);
            var doesntHaveSuperGlobalVar = TestingUtilities.GetNegation(hasSuperGlobalVar);

            TestingUtilities.RunTests(hasSuperGlobalVar, SuperGlobalVarPositiveTests);
            TestingUtilities.RunTests(doesntHaveSuperGlobalVar, SuperGlobalVarNegativeTests);
        }

        [TestMethod]
        public void ClassPresence()
        {
            var hasClass = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Class);
            var doesntHaveClass = TestingUtilities.GetNegation(hasClass);

            TestingUtilities.RunTests(hasClass, classPresencePositiveTests);
            TestingUtilities.RunTests(doesntHaveClass, classPresenceNegativeTests);
        }

        [TestMethod]
        public void MySQLFunctions()
        {
            var hasMySQLFunction = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.MySQL);
            var doesntHaveMySQLFunction = TestingUtilities.GetNegation(hasMySQLFunction);

            TestingUtilities.RunTests(hasMySQLFunction, mySQLPositiveTests);
            TestingUtilities.RunTests(doesntHaveMySQLFunction, SessionPositiveTests);
            TestingUtilities.RunTests(doesntHaveMySQLFunction, EvalPositiveTests);
        }
    }
}
