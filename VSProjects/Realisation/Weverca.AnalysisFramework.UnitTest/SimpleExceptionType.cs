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
        public static ObjectDecl CreateType()
        {
            var methods = new NativeMethodInfo[]{
                method("__construct",_method___construct),
            };

            var declaration = new ObjectDecl(new QualifiedName(new Name("Exception")), methods, new List<MethodDecl>(), new Dictionary<VariableName, ConstantInfo>(), new Dictionary<VariableName, NativeFieldInfo>(), null, false, false);
            return declaration;
        }

        private static NativeMethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            return new NativeMethodInfo(new Name(name),Visibility.PUBLIC, analyzer);
        }

        private static void _method___construct(FlowController flow)
        {
        }
    }
}
