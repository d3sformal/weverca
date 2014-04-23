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
		/// Program point where to jump
        /// </summary>
        public readonly ProgramPointBase TargetPoint;

        /// <summary>
        /// Creates new instance of CatchBlockDescription
        /// </summary>
        /// <param name="targetPoint">Point where to jump</param>
        /// <param name="catchedType">Type of Exception</param>
        /// <param name="catchVariable">Name of catched variables</param>
        public CatchBlockDescription(ProgramPointBase targetPoint, GenericQualifiedName catchedType, VariableIdentifier catchVariable)
        {
            TargetPoint = targetPoint;
            CatchedType = catchedType;

            if (CatchedType.QualifiedName.Name.Value == null)
            {
                CatchedType = new GenericQualifiedName(new QualifiedName(new Name("$noname")));
            }

            CatchVariable = catchVariable;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return TargetPoint.GetHashCode() + CatchedType.GetHashCode();
        }

        /// <inheritdoc />
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
