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
        public static NativeTypeDecl CreateType()
        {
            var methods = new NativeMethodInfo[]{
                method("__construct",_method___construct),
            };

            var declaration = new NativeTypeDecl(new QualifiedName(new Name("Exception")), methods, new List<MethodDecl>(), new Dictionary<VariableName, MemoryEntry>(), new Dictionary<VariableName, NativeFieldInfo>(), null, false, false);
            return declaration;
        }

        private static NativeMethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            return new NativeMethodInfo(new Name(name), analyzer);
        }

        private static void _method___construct(FlowController flow)
        {
        }
    }
}
