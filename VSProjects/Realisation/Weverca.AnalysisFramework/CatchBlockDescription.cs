using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Description of catch block
    /// </summary>
    public class CatchBlockDescription
    {
        /// <summary>
        /// Type of object that is catched by catch block
        /// </summary>
        public readonly GenericQualifiedName CatchedType;

        /// <summary>
        /// Variable where catch block stores throwed value
        /// </summary>
        public readonly VariableIdentifier CatchVariable;

        /// <summary>
        /// Program point where
        /// </summary>
        public readonly ProgramPointBase TargetPoint;

        public CatchBlockDescription(ProgramPointBase targetPoint, GenericQualifiedName catchedType, VariableIdentifier catchVariable)
        {
            TargetPoint = targetPoint;
            CatchedType = catchedType;
            CatchVariable = catchVariable;
        }

        public override int GetHashCode()
        {
            return TargetPoint.GetHashCode() + CatchVariable.GetHashCode() + CatchedType.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            var o = obj as CatchBlockDescription;
            if (o == null)
                return false;

            return
                o.CatchedType.QualifiedName.Equals(CatchedType.QualifiedName) &&
                o.CatchVariable == CatchVariable &&
                o.TargetPoint == TargetPoint;
        }
    }
}
