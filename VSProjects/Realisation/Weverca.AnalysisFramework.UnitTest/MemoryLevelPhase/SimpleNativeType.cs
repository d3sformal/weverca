/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.UnitTest
{
    static class SimpleNativeType
    {
        readonly static VariableIdentifier thisIdentifier = new VariableIdentifier("this");
        readonly static VariableIdentifier valueIdentifier = new VariableIdentifier("_value");

        public static ClassDecl CreateType()
        {
            var nativeType = new QualifiedName(new Name("NativeType"));
            var methods = new Dictionary<MethodIdentifier, MethodInfo>();
            methods.Add(new MethodIdentifier(nativeType, new Name("__construct")), method("__construct", _method___construct));
            methods.Add(new MethodIdentifier(nativeType, new Name("GetValue")), method("GetValue", _method_GetValue));
            var declaration = new ClassDecl(nativeType, 
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
            return new MethodInfo(new Name(name), new QualifiedName(new Name("NativeType")), Visibility.PUBLIC, analyzer, new List<MethodArgument>());
        }

        private static void _method___construct(FlowController flow)
        {
            var outSet = flow.OutSet;
            var outSnapshot = outSet.Snapshot;

            var arg = outSet.ReadVariable(new VariableIdentifier(".arg0")).ReadMemory(outSnapshot);
            var thisEntry = outSet.GetVariable(thisIdentifier);
            var field = thisEntry.ReadField(outSnapshot, valueIdentifier);

            field.WriteMemory(outSnapshot, arg);

            FunctionResolverBase.SetReturn(outSet, thisEntry.ReadMemory(outSnapshot));
        }

        private static void _method_GetValue(FlowController flow)
        {
            var outSet = flow.OutSet;
            var outSnapshot = outSet.Snapshot;

            var thisEntry = outSet.GetVariable(thisIdentifier);
            var value = thisEntry.ReadField(outSnapshot, valueIdentifier);

            FunctionResolverBase.SetReturn(outSet, value.ReadMemory(outSnapshot));
        }
    }
}