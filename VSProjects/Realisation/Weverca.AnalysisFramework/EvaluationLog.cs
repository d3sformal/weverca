using System;
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

        /// <summary>
        /// Associate variable for given partial. The variable was computed for given partial.
        /// <remarks>This is only workaround because of old "API" back compatibility</remarks>
        /// </summary>
        /// <param name="partial">Partial which variable is associated</param>
        /// <param name="value">Associated variable</param>
        [Obsolete("Use constructor with specified expression parts instead)")]
        internal void AssociateVariable(LangElement partial, VariableIdentifier variable)
        {
            var point = new TestVariablePoint(partial, variable);
            associatePoint(point);
        }

        #endregion
        

        [Obsolete("Use constructor with specified expression parts. It doesn't need explicit value association")]
        internal EvaluationLog()
        {
            _owner = new TestMemoryEntryPoint(null, null);
            _owner.Initialize(new FlowOutputSet(null), new FlowOutputSet(null));
        }

        /// <summary>
        /// Get value associated (computed during analysis) for given partial.         
        /// </summary>
        /// <param name="partial">Partial which value will be returned</param>
        /// <returns>Associated value, or null if there is no associated value</returns>
        [Obsolete("Use read snapshot entry instead. (Change because of new Snapshot access API)")]
        public MemoryEntry GetValue(LangElement partial)
        {
            var entry = ReadSnapshotEntry(partial);
            if (entry == null)
                return null;

            return entry.ReadMemory(_owner.InSnapshot);
        }
        //====================================

        #region Partial associations members
        
        readonly Dictionary<LangElement, LValuePoint> _lValues = new Dictionary<LangElement, LValuePoint>();

        readonly Dictionary<LangElement, ValuePoint> _rValues = new Dictionary<LangElement, ValuePoint>();

        readonly Dictionary<LangElement, ProgramPointBase> _points = new Dictionary<LangElement, ProgramPointBase>();

        readonly  ProgramPointBase _owner;
        #endregion

        /// <summary>
        /// Expects expression parts that are already not connected into PPG !!!
        /// </summary>
        /// <param name="expressionParts">Parts of condition expression</param>
        internal EvaluationLog(ProgramPointBase owner, IEnumerable<ProgramPointBase> expressionParts)
        {
            _owner = owner; 
            foreach (var part in expressionParts)
            {
                associatePointHierarchy(part);
            }
        }

        private void associatePointHierarchy(ProgramPointBase part)
        {
            var toAssociate = new Queue<ProgramPointBase>();
            toAssociate.Enqueue(part);

            while (toAssociate.Count > 0)
            {
                var point = toAssociate.Dequeue();
                if (point.Partial != null)
                {
                    associatePoint(point);
                }

                foreach (var parent in point.FlowParents)
                {
                    toAssociate.Enqueue(parent);
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
        /// Get variable associated (computed during analysis) for given partial.         
        /// </summary>
        /// <param name="partial">Partial which variable will be returned</param>
        /// <returns>Associated variable, or null if there is no associated variable</returns>
        public ReadWriteSnapshotEntryBase GetVariable(LangElement partial)
        {
            LValuePoint varLike;
            if (_lValues.TryGetValue(partial, out varLike))
            {
                return varLike.LValue;
            }

            ProgramPointBase point;
            if (_points.TryGetValue(partial, out point))
            {
                var lValue = point as LValuePoint;

                if (lValue != null)
                {
                    throw new NotSupportedException("Requested point allows assigning, but not directly via variable - request for API change");
                }
            }

            return null;
        }
        #endregion
    }
}
