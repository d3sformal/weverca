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

        protected FlowInputSet InputSet
        {
            get
            {
                return _analysis.GetInSet(_currentPoint);
            }
        }
        protected FlowInputSet OutputSet
        {
            get
            {
                return _analysis.GetOutSet(_currentPoint);
            }
        }

        protected SnapshotBase Input
        {
            get { return InputSet.Snapshot; }
        }

        protected SnapshotBase Output
        {
            get { return OutputSet.Snapshot; }
        }

        #endregion
    }
}
