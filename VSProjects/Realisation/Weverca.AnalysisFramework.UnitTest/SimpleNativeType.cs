using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.UnitTest
{
    static class SimpleNativeType
    {
        public static ClassDecl CreateType()
        {
            var nativeType=new QualifiedName(new Name("NativeType"));
            var methods = new Dictionary<MethodIdentifier, MethodInfo>();
            methods.Add(new MethodIdentifier(nativeType, new Name("__construct")), method("__construct",_method___construct));
            methods.Add(new MethodIdentifier(nativeType, new Name("GetValue")), method("GetValue", _method_GetValue));
            var declaration = new ClassDecl(nativeType, methods, new Dictionary<MethodIdentifier,MethodDecl>(), new Dictionary<FieldIdentifier, ConstantInfo>(), new Dictionary<FieldIdentifier, FieldInfo>(), null, false, false, false);
            return declaration;
        }


        private static MethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            return new MethodInfo(new Name(name), new QualifiedName(new Name("NativeType")), Visibility.PUBLIC, analyzer, new List<MethodArgument>());
        }

        private static void _method___construct(FlowController flow)
        {
            var outSet = flow.OutSet;

            var thisEntry = outSet.ReadValue(new VariableName("this"));
            var thisObj=thisEntry.PossibleValues.First() as ObjectValue;
            var index=outSet.CreateIndex("_value");
            outSet.SetField(thisObj, index, outSet.ReadValue(new VariableName(".arg0")));
            outSet.Assign(outSet.ReturnValue, thisEntry);
        }

        private static void _method_GetValue(FlowController flow)
        {
            var outSet=flow.OutSet;

            var thisObj = outSet.ReadValue(new VariableName("this")).PossibleValues.First() as ObjectValue;
            var index = outSet.CreateIndex("_value");
            outSet.Assign(outSet.ReturnValue, outSet.GetField(thisObj,index));
        }
    }
}
