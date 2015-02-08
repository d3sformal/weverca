/*
Copyright (c) 2012-2014 Pavel Bastecky.

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
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors
{
    /// <summary>
    /// Value visitor to get all composed values (arrays and objects) from the given memory entry.
    /// </summary>
    public class CollectComposedValuesVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// List of associative arrays which was found in entry.
        /// </summary>
        public readonly HashSet<AssociativeArray> Arrays = new HashSet<AssociativeArray>();

        /// <summary>
        /// List of object values which was found in entry.
        /// </summary>
        public readonly HashSet<ObjectValue> Objects = new HashSet<ObjectValue>();

        /// <summary>
        /// List of scalar values which was found in entry.
        /// </summary>
        public readonly HashSet<Value> Values = new HashSet<Value>();

        /// <summary>
        /// Gets or sets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        public Snapshot Snapshot { get; set; }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            Values.Add(value);
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            Objects.Add(value);
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            Arrays.Add(value);
        }

        /// <summary>
        /// Returns identifiers of all fields of objects which was found in memory entry.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Identifiers of all fields of objects which was found in memory entry</returns>
        public IEnumerable<VariableIdentifier> CollectFields(Snapshot snapshot)
        {
            HashSet<VariableIdentifier> fields = new HashSet<VariableIdentifier>();
            foreach (ObjectValue objectValue in Objects)
            {
                IObjectDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(objectValue);
                foreach (var index in descriptor.Indexes)
                {
                    fields.Add(new VariableIdentifier(index.Key));
                }
            }

            return fields;
        }

        /// <summary>
        /// Returns identifiers of all indexes of arrays which was found in memory entry.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Identifiers of all indexes of arrays which was found in memory entry</returns>
        public IEnumerable<MemberIdentifier> CollectIndexes(Snapshot snapshot)
        {
            HashSet<MemberIdentifier> indexes = new HashSet<MemberIdentifier>();
            foreach (AssociativeArray arrayValue in Arrays)
            {
                IArrayDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(arrayValue);
                foreach (var index in descriptor.Indexes)
                {
                    indexes.Add(new MemberIdentifier(index.Key));
                }
            }

            indexes.Add (MemberIdentifier.getUnknownMemberIdentifier());
            return indexes;
        }

        /// <summary>
        /// Resolves the types for collected objects.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Types for collected objects.</returns>
        public IEnumerable<TypeValue> ResolveObjectsTypes(Snapshot snapshot)
        {
            HashSet<TypeValue> types = new HashSet<TypeValue>();
            foreach (ObjectValue objectValue in Objects)
            {
                IObjectDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(objectValue);
                types.Add(descriptor.Type);
            }

            return types;
        }
    }
}