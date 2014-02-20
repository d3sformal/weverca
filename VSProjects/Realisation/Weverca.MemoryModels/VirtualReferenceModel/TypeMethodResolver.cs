using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    /// <summary>
    /// Visitor resolving method types
    /// </summary>
    class TypeMethodResolver : AbstractValueVisitor
    {
        /// <summary>
        /// Resolved methods
        /// </summary>
        private List<FunctionValue> _methods = new List<FunctionValue>();

        /// <summary>
        /// Context snapshot
        /// </summary>
        private readonly SnapshotBase _snapshot;

        private TypeMethodResolver(SnapshotBase snapshot)
        {
            _snapshot = snapshot;
        }

        internal static IEnumerable<FunctionValue> ResolveMethods(TypeValue type, SnapshotBase snapshot)
        {
            var resolver = new TypeMethodResolver(snapshot);
            type.Accept(resolver);

            return resolver._methods;
        }

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Given value of type " + value.GetType() + " has not supported type for method resolving");
        }

        /// <inheritdoc />
        public override void VisitTypeValue(TypeValue typeValue)
        {
            throw new NotSupportedException("Visiting given TypeValue " + typeValue.GetType() + " is not supported yet");
        }

        /// <inheritdoc />
        public override void VisitNativeTypeValue(TypeValue value)
        {
            var nativeDecl = value.Declaration;

            foreach (var methodInfo in nativeDecl.ModeledMethods)
            {
                //TODO resolve invoking element
                //var analyzer = new NativeAnalyzer(methodInfo.Method, new StringLiteral(new PHP.Core.Parsers.Position(),"to be implemented"));
                var analyzer = new NativeAnalyzer(methodInfo.Value.Method, new StringLiteral(new PHP.Core.Parsers.Position(), nativeDecl.QualifiedName.Name + "::" + methodInfo.Value.Name));
                _methods.Add(_snapshot.CreateFunction(methodInfo.Value.Name, analyzer));
            }

            foreach (var methodValue in nativeDecl.SourceCodeMethods.Values)
            {
                _methods.Add(methodValue);
            }

        }

        #endregion
    }
}
