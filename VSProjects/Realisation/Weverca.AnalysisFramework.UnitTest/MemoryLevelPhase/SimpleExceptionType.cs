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