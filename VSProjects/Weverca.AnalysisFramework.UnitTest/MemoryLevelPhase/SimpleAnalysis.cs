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

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;


namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Initializer used for setting environment because of testing purposes
    /// </summary>
    /// <param name="outSet">Initialized output set</param>
    delegate void EnvironmentInitializer(FlowOutputSet outSet);

    /// <summary>
    /// Contains methods that makes it possible to setup an analysis for testing
    /// </summary>
    internal interface TestAnalysisSettings
    {
        /// <summary>
        /// Set code included for given file name
        /// </summary>
        /// <param name="fileName">>Name of included file</param>
        /// <param name="fileCode">PHP code of included file</param>
        void SetInclude(string fileName, string fileCode);
        /// <summary>
        /// Set code included for given file name
        /// </summary>
        /// <param name="fileName">Name of included file</param>
        /// <param name="fileCode">PHP code of included file</param>
        void SetFunctionShare(string functionName);

        /// <summary>
        /// Set limit count of commits for single snapshot, when widening start to be processed
        /// </summary>
        /// <param name="limit">Count of commits</param>
        void SetWideningLimit(int limit);

        /// <summary>
        /// Sets limit count of size of memory entry to aply simplification.
        /// </summary>
        /// <param name="limit">The limit.</param>
        void SetSimplifyLimit(int limit);
    }

    class SimpleAnalysis : ForwardAnalysisBase, TestAnalysisSettings
    {
        private readonly SimpleFlowResolver _flowResolver;

        private readonly SimpleFunctionResolver _functionResolver;

        public SimpleAnalysis(ControlFlowGraph.ControlFlowGraph entryCFG, Weverca.MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            : base(entryCFG, memoryModel.CreateSnapshot)
        {
            _flowResolver = new SimpleFlowResolver();
            _functionResolver = new SimpleFunctionResolver(initializer);
            initializer(EntryInput);
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

        protected override MemoryAssistantBase createAssistant()
        {
            return new SimpleAssistant();
        }

        #endregion

        #region Analysis settings routines

        public void SetInclude(string fileName, string fileCode)
        {
            _flowResolver.SetInclude(fileName, fileCode);
        }

        public void SetFunctionShare(string functionName)
        {
            _functionResolver.SetFunctionShare(functionName);
        }

        public void SetWideningLimit(int limit)
        {
            WideningLimit = limit;
        }

        #endregion


        public void SetSimplifyLimit(int limit)
        {
            SimplifyLimit = limit;
        }
    }

    class SimpleInfo : InfoDataBase
    {
        internal readonly bool XssSanitized;

        internal SimpleInfo(bool xssSanitized)
        {
            XssSanitized = xssSanitized;
        }

        protected override int getHashCode()
        {
            return XssSanitized.GetHashCode();
        }

        protected override bool equals(InfoDataBase other)
        {
            var o = other as SimpleInfo;
            if (o == null)
                return false;

            return o.XssSanitized == XssSanitized;
        }
    }
}