using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    static class SimpleExceptionType
    {
        public static ClassDecl CreateType()
        {
            var methods = new MethodInfo[]{
                method("__construct",_method___construct),
            };

            var declaration = new ClassDecl(new QualifiedName(new Name("Exception")), methods, new List<MethodDecl>(), new Dictionary<VariableName, ConstantInfo>(), new Dictionary<VariableName, FieldInfo>(), null, false, false);
            return declaration;
        }

        private static MethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            return new MethodInfo(new Name(name),Visibility.PUBLIC, analyzer);
        }

        private static void _method___construct(FlowController flow)
        {
        }
    }
}
