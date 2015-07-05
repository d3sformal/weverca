/*
Copyright (c) 2012-2014 David Hauzar.

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels
{
    /// <summary>
    /// Enumeration class containing default instances representing memory models.
    /// </summary>
    public abstract class MemoryModels
    {
        /// <summary>
        /// Virtual reference memory model (Weverca.MemoryModels.VirtualReferenceModel)
        /// </summary>
        public static readonly MemoryModelFactory VirtualReferenceMM = new VirtualReferenceMMCl();
        /// <summary>
        /// Copy memory model (Weverca.MemoryModels.VirtualReferenceModel)
        /// </summary>
        public static readonly MemoryModelFactory CopyMM = new CopyMMCl();
        /// <summary>
        /// Modular copy memory model (Weverca.MemoryModels.ModularCopyMemoryModel)
        /// </summary>
        public static readonly MemoryModelFactory ModularCopyMM 
            = Weverca.MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.CopyImplementation;

        /// <summary>
        /// Creates a snapshot of given memory model.
        /// </summary>
        /// <returns>a snapshot of given memory model</returns>
        public abstract SnapshotBase CreateSnapshot();

        private MemoryModels() { }

        private class VirtualReferenceMMCl : MemoryModelFactory
        {
            public SnapshotBase CreateSnapshot()
            {
                return new Weverca.MemoryModels.VirtualReferenceModel.Snapshot();
            }

            public override string ToString()
            {
                return "Virtual reference memory model";
            }

        }
        private class CopyMMCl : MemoryModelFactory
        {
            public SnapshotBase CreateSnapshot()
            {
                return new Weverca.MemoryModels.CopyMemoryModel.Snapshot();
            }

            public override string ToString()
            {
                return "Copy memory model";
            }
        }
    }

    
}
