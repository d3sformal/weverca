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
using System.IO;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents method which creates empty flow info set.
    /// </summary>    
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet EmptySetDelegate();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>    
    public class ForwardAnalysisServices
    {
        /// <summary>
        /// Worklist where program points that needs to be processed are kept
        /// </summary>
        private readonly WorkList _workList;

        /// <summary>
        /// Available flow resolver obtained from analysis
        /// </summary>
        internal readonly FlowResolverBase FlowResolver;

        /// <summary>
        /// Available expression evaluator obtained from analysis
        /// </summary>
        internal readonly ExpressionEvaluatorBase Evaluator;

        /// <summary>
        /// Available function resolver obtained from analysis
        /// </summary>
        internal readonly FunctionResolverBase FunctionResolver;

        /// <summary>
        /// Available empty set creator obtained from analysis
        /// </summary>
        internal readonly EmptySetDelegate CreateEmptySet;

        /// <summary>
        /// End point of program point graph
        /// </summary>
        internal ProgramPointBase ProgramEnd { get; private set; }

        internal ForwardAnalysisServices(WorkList workList, FunctionResolverBase functionResolver, ExpressionEvaluatorBase evaluator, EmptySetDelegate emptySet, FlowResolverBase flowResolver)
        {
            _workList = workList;
            CreateEmptySet = emptySet;
            FlowResolver = flowResolver;
            FunctionResolver = functionResolver;
            Evaluator = evaluator;
        }

        internal bool ConfirmAssumption(FlowController flow, AssumptionCondition condition)
        {
            return FlowResolver.ConfirmAssumption(flow.OutSet, condition, flow.Log);
        }

        internal void FlowThrough(ProgramPointBase programPoint)
        {
            FlowResolver.FlowThrough(programPoint);
        }

		internal void EnqueueEntryPoint(ProgramPointBase entryPoint, ProgramPointBase exitPoint)
        {
			_workList.AddEntryPoint(entryPoint, exitPoint);
        }

        internal void EnqueueChildren(ProgramPointBase programPoint) 
        {
            _workList.AddChildren(programPoint);
        }

        internal void SetProgramEnd(ProgramPointBase programEnd)
        {
            ProgramEnd = programEnd;
        }

        /// <summary>
        /// Set services for all points in given graph
        /// </summary>
        /// <param name="ppGraph">Graph which program points will be set</param>
        internal void SetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                SetServices(point);
            }
        }

        internal void SetServices(ProgramPointBase point)
        {
            point.SetServices(this);
            point.SetMode(SnapshotMode.MemoryLevel);
        }

        /// <summary>
        /// Unset services for all points in given graph
        /// </summary>
        /// <param name="ppGraph">Graph which program points will be unset</param>
        internal void UnSetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                point.SetServices(null);
            }
        }
    }
}