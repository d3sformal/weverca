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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Base class for resolvers controlling program flow computation
    /// </summary>
    public abstract class FlowResolverBase
    {
        /// <summary>
        /// Represents method which is used for confirming assumption condition. 
        /// Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>  
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="log">Provide access to values computed during analysis</param>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public abstract bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log);

        /// <summary>
        /// Is called after each call invoked in beforeCall is completed. Has to merge data from 
        /// dispatched calls into the afterCall.
        /// </summary>
        /// <param name="beforeCall">The program point that dispatch calls</param>
        /// <param name="afterCall">The output set of the program point after the call (ExtensionSinkPoint)</param>
        /// <param name="dispatchedExtensions">Program point graphs obtained during analysis</param>
        public abstract void CallDispatchMerge(ProgramPointBase beforeCall, FlowOutputSet afterCall, IEnumerable<ExtensionPoint> dispatchedExtensions);
        
        /// <summary>
        /// Reports about try block scope start
        /// </summary>
        /// <param name="catchBlockStarts">Catch blocks associated with starting try block</param>
        /// <param name="outSet">Flow output set</param>
        public abstract void TryScopeStart(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts);

        /// <summary>
        /// Reports about try block scope end
        /// </summary>
        /// <param name="catchBlockStarts">Catch blocks associated with ending try block</param> 
        /// <param name="outSet">Flow output set</param>
        public abstract void TryScopeEnd(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts);

        /// <summary>
        /// Process throw statement according to current flow
        /// </summary>        
        /// <param name="throwStmt">Processed throw statement</param>
        /// <param name="throwedValue">Value that was supplied into throw statement</param>
        /// <param name="flow">Flow controller which provides API usefull for throw resolvings</param>
        /// <param name="outSet">Flow output set</param>
        /// <returns>All possible ThrowInfo branches</returns>
        public abstract IEnumerable<ThrowInfo> Throw(FlowController flow, FlowOutputSet outSet, ThrowStmt throwStmt, MemoryEntry throwedValue);

        /// <summary>
        /// Process catch statement according to given catch block
        /// </summary>
        /// <param name="catchPoint">Point describing state of current catch block</param>
        /// <param name="outSet">Flow output set</param>
        public abstract void Catch(CatchPoint catchPoint, FlowOutputSet outSet);

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>        
        public abstract void Include(FlowController flow, MemoryEntry includeFile);

        /// <summary>
        /// Is called for resolving eval expression. Should be resolved in similar way as include
        /// </summary>
        /// <param name="flow">Flow controller where eval extensions can be stored</param>
        /// <param name="code">Evaluated code</param>
        public abstract void Eval(FlowController flow, MemoryEntry code);

        /// <summary>
        /// Handler called before programPoint analysis starts 
        /// </summary>
        /// <param name="programPoint">Analyzed program point</param>   
        public virtual void FlowThrough(ProgramPointBase programPoint)
        {
            //By default there is nothing to do           
        }


    }
}