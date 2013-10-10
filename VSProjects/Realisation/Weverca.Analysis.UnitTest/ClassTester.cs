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
            TypeValue B=it.Current as TypeValue;
            Debug.Equals(B.Declaration.Fields.Count,2);
            Debug.Assert(B.Declaration.Fields[new VariableName("x")] != null);
            Debug.Assert(B.Declaration.Fields[new VariableName("y")] != null);

            it = (outset.ResolveType(new QualifiedName(new Name("A"))).GetEnumerator());
            it.MoveNext();
            TypeValue A = it.Current as TypeValue;
            Debug.Equals(A.Declaration.Fields.Count, 1);
            Debug.Assert(A.Declaration.Fields[new VariableName("x")]!=null);

        }
    }
}
