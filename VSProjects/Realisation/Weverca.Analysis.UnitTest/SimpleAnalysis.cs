using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.ProgramPoints;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Initializer used for setting environment because of testing purposes
    /// </summary>
    /// <param name="outSet">Initialized output set</param>
    delegate void EnvironmentInitializer(FlowOutputSet outSet);

    class SimpleAnalysis : ForwardAnalysisBase
    {
        private readonly EnvironmentInitializer _initializer;

        private readonly SimpleFlowResolver _flowResolver;

        private readonly SimpleFunctionResolver _functionResolver;

        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG, EnvironmentInitializer initializer)
            : base(entryCFG)
        {
            _initializer = initializer;
            _flowResolver = new SimpleFlowResolver();
            _functionResolver = new SimpleFunctionResolver(_initializer);
        }

        #region Resolvers that are used during analysis

        protected override ExpressionEvaluatorBase createExpressionEvaluator()
        {
            return new SimpleExpressionEvaluator();
        }

        protected override FlowResolverBase createFlowResolver()
        {
            return _flowResolver;
        }

        protected override FunctionResolverBase createFunctionResolver()
        {
            return _functionResolver;
        }

        protected override SnapshotBase createSnapshot()
        {
            return new Weverca.MemoryModels.VirtualReferenceModel.Snapshot();
        }
        #endregion

        #region Analysis settings routines

        internal void SetInclude(string fileName, string fileCode)
        {
            _flowResolver.SetInclude(fileName, fileCode);
        }

        internal void SetFunctionShare(string functionName)
        {
            _functionResolver.SetFunctionShare(functionName);
        }

        #endregion
    }

    class SimpleInfo
    {
        internal readonly bool XssSanitized;

        internal SimpleInfo(bool xssSanitized)
        {
            XssSanitized = xssSanitized;
        }
    }

    class CatchBlockInfo
    {
        internal readonly GenericQualifiedName InputClass;

        internal readonly ProgramPointBase CatchStart;


        public CatchBlockInfo(GenericQualifiedName inputClass, ProgramPointBase catchStart)
        {
            InputClass = inputClass;
            CatchStart = catchStart;
        }        
    }  
}
