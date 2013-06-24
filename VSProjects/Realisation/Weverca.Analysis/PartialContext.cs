using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    class PartialContext
    {
        public ProgramPoint Source { get { return _source; } }

        public LangElement CurrentPartial { get; private set; }

        public FlowInputSet InSet { get { return _source.InSet; } }

        public FlowOutputSet OutSet { get { return _source.OutSet; } }

        public bool IsComplete { get { return CurrentPartial == null; } }

        public bool IsCondition { get { return _source.IsCondition; } }

        ProgramPoint _source;

        IEnumerator<Postfix> _conditionParts;
        int _postfixIndex = 0;

        public PartialContext(ProgramPoint source)
        {
            _source = source;
            OutSet.StartTransaction();

            if (IsCondition)
            {
                _conditionParts = _source.Condition.Parts.GetEnumerator();
                _conditionParts.MoveNext();
            }

            fetchCurrentPartial();
        }

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

            if (IsComplete)
            {
                OutSet.CommitTransaction();
            }
        }

        private bool moveNextPostfix()
        {
            if (!IsCondition)
            {
                return false;   
            }

            return _conditionParts.MoveNext();
        }

        private void fetchCurrentPartial()
        {
            CurrentPartial = getCurrentPartial();
        }

        private LangElement getCurrentPartial()
        {
            if (_source.IsEmpty)
            {
                return null;
            }
            return getCurrentPostfix().GetElement(_postfixIndex);
        }

        private Postfix getCurrentPostfix()
        {
            if (IsCondition)
            {
                return _conditionParts.Current;
            }
            return _source.Statement;
        }

        
    }
}
