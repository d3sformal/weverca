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
            var methods = new Dictionary<MethodIdentifier, MethodInfo>();
            var exceptionIdentifier=new QualifiedName(new Name("Exception"));
            methods.Add(new MethodIdentifier(exceptionIdentifier,new Name("__construct")), method("__construct", _method___construct));
                

            var declaration = new ClassDecl(
                exceptionIdentifier, 
                methods,
                new Dictionary<MethodIdentifier, FunctionValue>(), 
                new Dictionary<FieldIdentifier, ConstantInfo>(), 
                new Dictionary<FieldIdentifier, FieldInfo>(), 
                new List<QualifiedName>(), 
                false, false, false);
            return declaration;
        }

        private static MethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            var exceptionIdentifier = new QualifiedName(new Name("Exception"));
            return new MethodInfo(new Name(name), exceptionIdentifier, Visibility.PUBLIC, analyzer, new List<MethodArgument>());
        }

        private static void _method___construct(FlowController flow)
        {
        }
    }
}
