/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using PHP.Core;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Visitor used for analysis of created program point graphs.
    /// </summary>
    public abstract class NextPhaseAnalyzer : ProgramPointVisitor
    {
        private NextPhaseAnalysis _analysis;

        private ProgramPointBase _currentPoint;

        #region Internal methods for analysis handling

        internal void Initialize(NextPhaseAnalysis analysis)
        {
            _analysis = analysis;
        }

        internal void FlowThrough(ProgramPointBase point)
        {
            _currentPoint = point;

            point.Accept(this);
        }

        #endregion

        #region API exposed for analysis implementing

        /// <summary>
        /// Input set of current program point
        /// </summary>
        protected FlowInputSet InputSet
        {
            get
            {
                return _analysis.GetInSet(_currentPoint);
            }
        }

        /// <summary>
        /// Output set of current program point
        /// </summary>
        protected FlowOutputSet OutputSet
        {
            get
            {
                return _analysis.GetOutSet(_currentPoint);
            }
        }

        /// <summary>
        /// Input snapshot that can be used as input context of current program point
        /// </summary>
        protected SnapshotBase Input
        {
            get { return InputSet.Snapshot; }
        }

        /// <summary>
        /// Output snapshot that can be used as output context of current program point
        /// </summary>
        protected SnapshotBase Output
        {
            get { return OutputSet.Snapshot; }
        }

        #endregion

		#region Function handling

		/// <summary>
		/// Visits an extension sink point
		/// </summary>
		/// <param name="p">point to visit</param>
		public override void VisitExtensionSink(ExtensionSinkPoint p)
		{
			_currentPoint = p;
			var ends = p.OwningExtension.Branches.Select(c => c.Graph.End.OutSet).ToArray();
			OutputSet.MergeWithCallLevel(ends);

			p.ResolveReturnValue();
		}

		/// <summary>
		/// Visits an extension point
		/// </summary>
		/// <param name="p">point to visit</param>
		public override void VisitExtension(ExtensionPoint p)
		{
			_currentPoint = p;

			if (p.Graph.FunctionName == null) 
			{
				return;
			}

			var declaration = p.Graph.SourceObject;
			var signature = getSignature(declaration);
			var callPoint = p.Caller as RCallPoint;
			if (callPoint != null) 
			{
				if (signature.HasValue) 
				{
					// We have names for passed arguments
					setNamedArguments (OutputSet, callPoint.CallSignature, signature.Value, p.Arguments);
				} else 
				{
					// There are no names - use numbered arguments
					setOrderedArguments (OutputSet, p.Arguments, declaration);
				}
			}

		}
			
		private void setNamedArguments(FlowOutputSet callInput, CallSignature? callSignature, Signature signature, IEnumerable<ValuePoint> arguments)
		{
			int i = 0;
			foreach (var arg in arguments)
			{
				if (i >= signature.FormalParams.Count)
					break;
				var param = signature.FormalParams[i];
				var argumentVar = callInput.GetVariable(new VariableIdentifier(param.Name));

				var argumentValue = arg.Value.ReadMemory(Output);
				argumentVar.WriteMemory(callInput.Snapshot, argumentValue);

				++i;
			}
			// TODO: if (arguments.Count() < signature.FormalParams.Count) and exists i > arguments.Count() signature.FormalParams[i].InitValue != null

		}

		private void setOrderedArguments(FlowOutputSet callInput, IEnumerable<ValuePoint> arguments, LangElement declaration)
		{
			var index = 0;
			foreach (var arg in arguments)
			{
				var argVar = argument(index);
				var argumentEntry = callInput.GetVariable(new VariableIdentifier(argVar));

				//assign value for parameter
				var argumentValue = arg.Value.ReadMemory(Output);
				argumentEntry.WriteMemory(callInput.Snapshot, argumentValue);

				++index;
			}
		}

		private static VariableName argument(int index)
		{
			if (index < 0)
			{
				throw new NotSupportedException("Cannot get argument variable for negative index");
			}

			return new VariableName(".arg" + index);
		}

		private Signature? getSignature(LangElement declaration)
		{
			// TODO: Resolving via visitor might be better
			var methodDeclaration = declaration as MethodDecl;
			if (methodDeclaration != null)
			{
				return methodDeclaration.Signature;
			}
			else
			{
				var functionDeclaration = declaration as FunctionDecl;
				if (functionDeclaration != null)
				{
					return functionDeclaration.Signature;
				}
			}

			return null;
		}

		#endregion
    }
}