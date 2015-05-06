/*
Copyright (c) 2012-2014 Miroslav Vodolan, David Skorvaga.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class IndicatorCodeMetricsTests
    {
        #region Eval indicator tests

        private readonly IEnumerable<SourceTest> evalPositiveTests = new SourceTest[]
        {
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

        private readonly IEnumerable<SourceTest> evalNegativeTests = new SourceTest[]
        {
            new SourceTest("No eval is present", TestingUtilities.HelloWorldSource)
        };

        #endregion Eval indicator tests

        #region Session indicator tests

        private readonly IEnumerable<SourceTest> sessionPositiveTests = new SourceTest[]
        {
            new SourceTest("Session function call outside function/method", @"
                session_start();
            "),

            new SourceTest("Session function call inside global function", @"
                function test($param){
                    session_write_close();
                }
            "),

            new SourceTest("Session function call inside method", @"
                class testClass{
                    function testMethod($param){
                        session_unset();
                    }
                }
            ")
        };

        private readonly IEnumerable<SourceTest> sessionNegativeTests = new SourceTest[]
        {
            new SourceTest("No session is present", TestingUtilities.HelloWorldSource)
        };

        #endregion Session indicator tests

        #region SuperGlobal variable indicator tests

        private readonly IEnumerable<SourceTest> superGlobalVarPositiveTests = new SourceTest[]
        {
            new SourceTest("Super global outside function/method", @"
                $_POST['test']='hello';
            "),

            new SourceTest("Super global inside global function", @"
                function test($param){
                    $_GET['test']='world';
                }
            "),

            new SourceTest("Super global inside method", @"
                class testClass{
                    function testMethod($param){
                        $_SESSION['test']='!';
                    }
                }
            "),

            new SourceTest("Super global as rvalue", @"
                class testClass{
                    function testMethod($param){
                        $test=$GLOBALS['test'];
                    }
                }
            ")
        };

        private readonly IEnumerable<SourceTest> superGlobalVarNegativeTests = new SourceTest[]
        {
            new SourceTest("No session is present", TestingUtilities.HelloWorldSource),

            new SourceTest("No super global variable", @"
                $notSuperGlobal=3;
                function test($param){
                    global $notSuperGlobal;
                    $notSuperGlobal=$notSuperGlobal+1;
                }
            "),
        };

        #endregion SuperGlobal variable indicator tests

        #region ClassPresence tests

        private readonly SourceTest[] classPresencePositiveTests = new SourceTest[]
        {
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
                }"),
            new SourceTest("interface only", @"
                interface iTemplate
                {
                    public function setVariable($name, $var);
                    public function getHtml($template);
                }")
        };

        private readonly SourceTest[] classPresenceNegativeTests = new SourceTest[]
        {
            new SourceTest("function only", @"
                function setVariable($name, $var)
                {
                    $this->vars[$name] = $var;
                }")
        };

        #endregion ClassPresence tests

        #region DynamicDereference tests

        private readonly SourceTest[] dynamicDereferencePositiveTests = new SourceTest[]
        {
            new SourceTest("basic test", " $aaa = 12; $c = \"aaa\"; $b = $$c;"),
            new SourceTest("just access", " $aaa = 12; $c = \"aaa\"; $$c;"),
            new SourceTest("tripple dereference", " $aaa = \"bbb\"; $bbb = 12; $c = \"aaa\"; $b = $$$c;"),
            new SourceTest("class-function test",
                " class A { function b() {$aaa = 12; $c = \"aaa\"; $b = $$c;}}")
        };

        private readonly SourceTest[] dynamicDereferenceNegativeTests = new SourceTest[]
        {
            new SourceTest("array test", " $aaa = 12; $c = \"aaa\"; $b = $c[0];"),
            new SourceTest("basic negative test", "$c = \"aaa\";"),
        };

        #endregion DynamicDereference tests

        #region MySQL tests

        private readonly SourceTest[] mySqlPositiveTests = new SourceTest[]
        {
            new SourceTest("mysql_connect", "$link = @mysql_connect(\"localhost\", \"login\", \"password\") or die (\"<p>Connection failed!</p>\");"),
            new SourceTest("mysql_query", "$result = mysql_query(\"SELECT * FROM Table\");")
        };

        #endregion MySQL tests

        #region Dynamic function call tests

        private readonly SourceTest[] dynamicCallPositiveTests = new SourceTest[]
        {
            new SourceTest("basic test", @"
                $fxname = ""helloWorld"";

                function helloWorld(){
                    echo ""What a beautiful world!"";
                }

                $fxname(); //echos What a beautiful world!"),
            new SourceTest("inside class", @"
                class A{
                    function Foo(){ }
                }
                $instance = new A();
                $fxname = ""Foo"";
                $instance->$fxname();"),
            new SourceTest("dynamic creation", @"
                $fxname = ""A"";

                $instance = new $fxname();")
        };

        private readonly SourceTest[] dynamicCallNegativeTests = new SourceTest[]
        {
            new SourceTest("basic negative test", @"
                function helloWorld(){
                    echo ""What a beautiful world!"";
                }

                helloWorld(); //echos What a beautiful world!"),
            new SourceTest("no function call test", "$c = \"aaa\";"),
        };

        #endregion Dynamic function call tests

        #region Alias tests

        private readonly SourceTest[] aliasPositiveTests = new SourceTest[]
        {
            new SourceTest("basic test", @"
                    $a = 1;
                    $b = &$a;"),
            new SourceTest("alias to array", @"
                    $a = [];
                    $b = &$a[1];"),
            new SourceTest("class field alias", @"
                    class A {
                        public $field = ""asdf"";
                    }
                    $a = new A();
                    $b = &$a->field;"),
            new SourceTest("class indirect field alias", @"
                    class A {
                        public $field = ""asdf"";
                    }
                    $a = new A();
                    $c = ""field"";
                    $b = &$a->$c;")
        };

        private readonly SourceTest[] aliasNegativeTests = new SourceTest[]
        {
            new SourceTest("basic test", @"
                $a = 1;
                $b = $a;")
        };

        #endregion Alias tests

        [TestMethod]
        public void Eval()
        {
            var hasEval = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Eval);
            var doesntHaveEval = TestingUtilities.GetNegation(hasEval);

            TestingUtilities.RunTests(hasEval, evalPositiveTests);
            TestingUtilities.RunTests(doesntHaveEval, evalNegativeTests);
        }

        [TestMethod]
        public void Session()
        {
            var hasSession = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Session);
            var doesntHaveSession = TestingUtilities.GetNegation(hasSession);

            TestingUtilities.RunTests(hasSession, sessionPositiveTests);
            TestingUtilities.RunTests(doesntHaveSession, sessionNegativeTests);
        }

        [TestMethod]
        public void SuperGlobalVar()
        {
            var hasSuperGlobalVar = TestingUtilities.GetContainsIndicatorPredicate(
                ConstructIndicator.SuperGlobalVariable);
            var doesntHaveSuperGlobalVar = TestingUtilities.GetNegation(hasSuperGlobalVar);

            TestingUtilities.RunTests(hasSuperGlobalVar, superGlobalVarPositiveTests);
            TestingUtilities.RunTests(doesntHaveSuperGlobalVar, superGlobalVarNegativeTests);
        }

        [TestMethod]
        public void ClassPresence()
        {
            var hasClass = TestingUtilities.GetContainsIndicatorPredicate(
                ConstructIndicator.ClassOrInterface);
            var doesntHaveClass = TestingUtilities.GetNegation(hasClass);

            TestingUtilities.RunTests(hasClass, classPresencePositiveTests);
            TestingUtilities.RunTests(doesntHaveClass, classPresenceNegativeTests);
        }

        [TestMethod]
        public void MySqlFunctions()
        {
            var hasMySqlFunction = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.MySql);
            var doesntHaveMySqlFunction = TestingUtilities.GetNegation(hasMySqlFunction);

            TestingUtilities.RunTests(hasMySqlFunction, mySqlPositiveTests);
            TestingUtilities.RunTests(doesntHaveMySqlFunction, sessionPositiveTests);
            TestingUtilities.RunTests(doesntHaveMySqlFunction, evalPositiveTests);
        }

        [TestMethod]
        public void DynamicDereference()
        {
            var hasDynamicDereference = TestingUtilities.GetContainsIndicatorPredicate(
                ConstructIndicator.DynamicDereference);
            var doesntHaveDynamicDereference = TestingUtilities.GetNegation(hasDynamicDereference);

            TestingUtilities.RunTests(hasDynamicDereference, dynamicDereferencePositiveTests);
            TestingUtilities.RunTests(doesntHaveDynamicDereference, dynamicDereferenceNegativeTests);
        }

        [TestMethod]
        public void DynamicCalls()
        {
            var hasDynamicCall = TestingUtilities.GetContainsIndicatorPredicate(
                ConstructIndicator.DynamicCall);
            var doesntHaveDynamicCall = TestingUtilities.GetNegation(hasDynamicCall);

            TestingUtilities.RunTests(hasDynamicCall, dynamicCallPositiveTests);
            TestingUtilities.RunTests(doesntHaveDynamicCall, dynamicCallNegativeTests);
        }

        [TestMethod]
        public void Alias()
        {
            var hasAlias = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.References);
            var doesntHaveAlias = TestingUtilities.GetNegation(hasAlias);

            TestingUtilities.RunTests(hasAlias, aliasPositiveTests);
            TestingUtilities.RunTests(doesntHaveAlias, aliasNegativeTests);
        }
    }
}