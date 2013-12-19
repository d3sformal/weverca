﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class TypeMethodResolver : AbstractValueVisitor
    {
        private List<FunctionValue> _methods=new List<FunctionValue>();
        private readonly SnapshotBase Snapshot;

        private TypeMethodResolver( SnapshotBase snapshot)
        {
            Snapshot = snapshot;
        }

        internal static IEnumerable<FunctionValue> ResolveMethods(TypeValue type,SnapshotBase snapshot)
        {
            var resolver = new TypeMethodResolver(snapshot);
            type.Accept(resolver);

            return resolver._methods;
        }

        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Given value has not supported type");
        }

        public override void VisitTypeValue(TypeValue typeValue)
        {
            throw new NotImplementedException("Visiting given typeValue has not been implemented yet");
        }

        public override void VisitNativeTypeValue(TypeValue value)
        {
            var nativeDecl = value.Declaration;

            foreach (var methodInfo in nativeDecl.ModeledMethods)
            {
                //TODO resolve invoking element
                //var analyzer = new NativeAnalyzer(methodInfo.Method, new StringLiteral(new PHP.Core.Parsers.Position(),"to be implemented"));
                var analyzer = new NativeAnalyzer(methodInfo.Value.Method, new StringLiteral(new PHP.Core.Parsers.Position(), nativeDecl.QualifiedName.Name+"::"+methodInfo.Value.Name));
                _methods.Add(Snapshot.CreateFunction(methodInfo.Value.Name, analyzer));
            }

            foreach (var methodValue in nativeDecl.SourceCodeMethods.Values)
            {
                _methods.Add(methodValue);
            }

        }

    }
}
