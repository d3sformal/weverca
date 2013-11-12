using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels
{
    /// <summary>
    /// Enumeration class containing instances representing memory models.
    /// </summary>
    public abstract class MemoryModels
    {
        /// <summary>
        /// Virtual reference memory model (Weverca.MemoryModels.VirtualReferenceModel)
        /// </summary>
        public static readonly MemoryModels VirtualReferenceMM = new VirtualReferenceMMCl();
        /// <summary>
        /// Copy memory model (Weverca.MemoryModels.VirtualReferenceModel)
        /// </summary>
        public static readonly MemoryModels CopyMM = new CopyMMCl();

        /// <summary>
        /// Creates a snapshot of given memory model.
        /// </summary>
        /// <returns>a snapshot of given memory model</returns>
        public abstract SnapshotBase CreateSnapshot();

        private MemoryModels() { }

        private class VirtualReferenceMMCl : MemoryModels
        {
            public override SnapshotBase CreateSnapshot()
            {
                return new Weverca.MemoryModels.VirtualReferenceModel.Snapshot();
            }
        }
        private class CopyMMCl : MemoryModels
        {
            public override SnapshotBase CreateSnapshot()
            {
                return new Weverca.MemoryModels.CopyMemoryModel.Snapshot();
            }
        }
    }

    
}
