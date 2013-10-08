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
        public static NativeTypeDecl CreateType()
        {
            var methods = new NativeMethodInfo[]{
                method("__construct",_method___construct),
                method("GetValue",_method_GetValue)

            };

            var declaration = new NativeTypeDecl(new QualifiedName(new Name("NativeType")), methods, new List<MethodDecl>(), new Dictionary<VariableName, MemoryEntry>(), new Dictionary<VariableName, NativeFieldInfo>(), null, false, false);
            return declaration;
        }


        private static NativeMethodInfo method(string name, NativeAnalyzerMethod analyzer)
        {
            return new NativeMethodInfo(new Name(name), analyzer);
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
