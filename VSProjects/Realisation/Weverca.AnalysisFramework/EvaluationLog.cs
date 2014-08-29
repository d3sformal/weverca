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

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Provide access to values computed for every partial. These values are
    /// </summary>
    public class EvaluationLog
    {
        //=========This is obsolete===========


        #region Internal methods for associating partials
        /// <summary>
        /// Associate value for given partial. The value was computed for given partial.
        /// <remarks>This is only workaround because of old "API" back compatibility</remarks>
        /// </summary>
        /// <param name="partial">Partial which value is associated</param>
        /// <param name="value">Associated value</param>
        [Obsolete("Use constructor with specified expression parts instead)")]
        internal void AssociateValue(LangElement partial, MemoryEntry value)
        {
            var point = new TestMemoryEntryPoint(partial, value);
            associatePoint(point);
        }

        #endregion


        [Obsolete("Use constructor with specified expression parts. It doesn't need explicit value association")]
        internal EvaluationLog()
        {
            _owner = new TestMemoryEntryPoint(null, null);
            _owner.Initialize(new FlowOutputSet(null), new FlowOutputSet(null));
        }

        //====================================

        #region Partial associations members

        readonly Dictionary<LangElement, LValuePoint> _lValues = new Dictionary<LangElement, LValuePoint>();

        readonly Dictionary<LangElement, ValuePoint> _rValues = new Dictionary<LangElement, ValuePoint>();

        readonly Dictionary<LangElement, ProgramPointBase> _points = new Dictionary<LangElement, ProgramPointBase>();

        readonly ProgramPointBase _owner;
        #endregion

        /// <summary>
        /// Expects expression parts that are already not connected into PPG !!!
        /// </summary>
        /// <param name="expressionParts">Parts of condition expression</param>
        internal EvaluationLog(IEnumerable<ProgramPointBase> expressionParts)
        {
            foreach (var part in expressionParts)
            {
                AssociatePointHierarchy(part);
            }
        }

        /// <summary>
        /// Associate points in given sub graph into log.
        /// </summary>
        /// <param name="subgraph">Associated subgraph</param>
        internal void AssociatePointHierarchy(ProgramPointBase subgraph)
        {
            var associated = new HashSet<ProgramPointBase>();
            var toAssociate = new Queue<ProgramPointBase>();
            toAssociate.Enqueue(subgraph);

            while (toAssociate.Count > 0)
            {
                var point = toAssociate.Dequeue();
                if (point.Partial != null)
                {
                    associatePoint(point);
                }

                foreach (var relatedPoint in point.FlowParents.Concat(point.FlowChildren))
                {
                    if (associated.Contains(relatedPoint))
                    {
                        continue;
                    }
                    associated.Add(relatedPoint);

                    toAssociate.Enqueue(relatedPoint);
                }
            }
        }

        private void associatePoint(ProgramPointBase point)
        {
            var partial = point.Partial;
            _points[partial] = point;

            var lValue = point as LValuePoint;
            if (lValue != null)
                _lValues[partial] = lValue;

            var rValue = point as ValuePoint;
            if (rValue != null)
                _rValues[partial] = rValue;
        }


        #region Public methods for retrieving associations for partials


        /// <summary>
        /// Reads the snapshot entry. It is similar to <see cref="GetSnapshotEntry"/> but intended for read-only access.
        /// </summary>
        /// <param name="partial">Partial which variable will be returned</param>
        /// <returns>Associated variable, or null if there is no associated variable</returns>
        public ReadSnapshotEntryBase ReadSnapshotEntry(LangElement partial)
        {
            ValuePoint rValue;
            if (_rValues.TryGetValue(partial, out rValue))
            {
                return rValue.Value;
            }

            return null;
        }

        /// <summary>
        /// Get LValue associated (computed during analysis) for given partial.         
        /// </summary>
        /// <param name="partial">Partial which LValue will be returned</param>
        /// <returns>Associated LValue, or null if there is no associated LValue</returns>
        public ReadWriteSnapshotEntryBase GetSnapshotEntry(LangElement partial)
        {
            LValuePoint varLike;
            if (_lValues.TryGetValue(partial, out varLike))
            {
                return varLike.LValue;
            }

            ProgramPointBase point;
            if (_points.TryGetValue(partial, out point))
            {
                var lValuePoint = point as LValuePoint;

                if (lValuePoint != null)
                {
                    //associated program point hasn't been used as LValue, but can be assigned to
                    return lValuePoint.LValue;
                }
            }

            return null;
        }
        #endregion
    }
}