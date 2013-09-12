using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Represents context for LangElement part of Statement/Condition
    /// Partials (parts of postfix expression) are served in postfix order
    /// NOTE:
    ///     All condition parts are processed together as one postfix expression
    /// </summary>
  /*  class PartialContext
    {

        #region Private members
        /// <summary>
        /// Current programpoint source
        /// </summary>
        private readonly ProgramPointBase _source;
        /// <summary>
        /// Enumerator for keeping current postfix in condition
        /// </summary>
        private IEnumerator<Postfix> _conditionParts;
        /// <summary>
        /// Index of current partial in current postfix
        /// </summary>
        private int _postfixIndex = 0;
        #endregion

        #region Current partial processing members
        /// <summary>
        /// Program point which partials are analyzed
        /// </summary>
        internal ProgramPointBase Source { get { return _source; } }

        /// <summary>
        /// Current part of postfix expression
        /// </summary>
        internal LangElement CurrentPartial { get; private set; }

        /// <summary>
        /// Input set for current partial
        /// </summary>
        internal FlowInputSet InSet { get { return _source.InSet; } }

        /// <summary>
        /// Output set for current partial
        /// </summary>
        internal FlowOutputSet OutSet { get { return _source.OutSet; } }

        /// <summary>
        /// Determine that all partials has been processed
        /// </summary>
        internal bool IsComplete { get { return CurrentPartial == null; } }

        /// <summary>
        /// Determine if Source is condition
        /// </summary>
        internal bool IsCondition { get { return _source.IsCondition; } }
        #endregion

        /// <summary>
        /// Create partial context for given source program point
        /// </summary>
        /// <param name="source">Program point which statement/condition creates partial context</param>
        internal PartialContext(ProgramPointBase source)
        {
            _source = source;            

            if (IsCondition)
            {
                _conditionParts = _source.Condition.Parts.GetEnumerator();
                _conditionParts.MoveNext();
            }

            fetchCurrentPartial();
        }

        /// <summary>
        /// Move to next partial if possible
        /// </summary>
        public void MoveNextPartial()
        {
            CurrentPartial = null;
            if (_source.IsEmpty)
            {
                //nothing to move
                return;
            }

            do
            {
                var postfix = getCurrentPostfix();
                ++_postfixIndex;

                if (_postfixIndex < postfix.Length)
                {
                    fetchCurrentPartial();
                    break;
                }

                //hack - because of incrementing at loop begining
                _postfixIndex = -1;
            } while (moveNextPostfix());
        }

        /// <summary>
        /// Move to next posftfix if possible
        /// </summary>
        /// <returns>True if move has been done, false otherwise</returns>
        private bool moveNextPostfix()
        {
            if (!IsCondition)
            {
                return false;   
            }

            return _conditionParts.MoveNext();
        }

        /// <summary>
        /// Set CurrentPartial member
        /// </summary>
        private void fetchCurrentPartial()
        {
            CurrentPartial = getCurrentPartial();
        }

        /// <summary>
        /// Get current partial from current postfix
        /// </summary>
        /// <returns>Current partial</returns>
        private LangElement getCurrentPartial()
        {
            if (_source.IsEmpty)
            {
                return null;
            }
            return getCurrentPostfix().GetElement(_postfixIndex);
        }

        /// <summary>
        /// Get current postfix
        /// </summary>
        /// <returns>Current postfix</returns>
        private Postfix getCurrentPostfix()
        {
            if (IsCondition)
            {
                return _conditionParts.Current;
            }
            return _source.Statement;
        }

        
    }*/
}
