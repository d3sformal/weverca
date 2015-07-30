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
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm
{
    
    class PrintAlgorithmFactory : IAlgorithmFactory<IPrintAlgorithm>
    {
        public IPrintAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new PrintAlgorithm(factories);
        }
    }

    /// <summary>
    /// Implementation of the print algorithm which creates representation of the snapshot to standard Weverca format.
    /// </summary>
    class PrintAlgorithm : AlgorithmBase, IPrintAlgorithm
    {
        public PrintAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        public string SnapshotToString(Snapshot snapshot)
        {
            StringBuilder result = new StringBuilder();

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                appendLine(result, "===LOCALS===");
                createRepresentation(snapshot, result, snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyVariables);
            }
            appendLine(result, "===GLOBALS===");
            createRepresentation(snapshot, result, snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyVariables);

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                appendLine(result, "===LOCAL CONTROLS===");
                createRepresentation(snapshot, result, snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyControllVariables);
            }

            appendLine(result, "===GLOBAL CONTROLS===");
            createRepresentation(snapshot, result, snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyControllVariables);

            appendLine(result, "\n===ARRAYS===");
            createArraysRepresentation(snapshot, result);

            appendLine(result, "\n===FIELDS===");
            createFieldsRepresentation(snapshot, result);

            appendLine(result, "\n===ALIASES===");
            createAliasesRepresentation(snapshot, result);

            return result.ToString();
        }

        private void createAliasesRepresentation(Snapshot snapshot, StringBuilder result)
        {
            foreach (var item in snapshot.Structure.Readonly.IndexDefinitions)
            {
                var aliases = item.Value.Aliases;
                if (aliases != null && (aliases.MayAliases.Count > 0 || aliases.MustAliases.Count > 0))
                {
                    MemoryIndex index = item.Key;
                    result.AppendFormat("{0}: {{ ", index);

                    result.Append(" MUST: ");
                    foreach (var alias in aliases.MustAliases)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;

                    result.Append(" | MAY: ");
                    foreach (var alias in aliases.MayAliases)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;
                    result.AppendLine(" }");
                }
            }
        }

        private void createFieldsRepresentation(Snapshot snapshot, StringBuilder result)
        {
            foreach (var item in snapshot.Structure.Readonly.ObjectDescriptors)
            {
                createRepresentation(snapshot, result, item.Value);
                appendLine(result, "");
            }
        }

        private void createArraysRepresentation(Snapshot snapshot, StringBuilder result)
        {
            foreach (var item in snapshot.Structure.Readonly.ArrayDescriptors)
            {
                IArrayDescriptor descriptor = item.Value;
                AssociativeArray associativeArray = item.Key;

                createIndexRepresentation(snapshot, result, descriptor.UnknownIndex, String.Format("{0}[?]", associativeArray.UID));

                foreach (var indexItem in descriptor.Indexes)
                {
                    MemoryIndex index = indexItem.Value;
                    string indexName = indexItem.Key;
                    createIndexRepresentation(snapshot, result, index, String.Format("{0}[{1}]", associativeArray.UID, indexName));
                }

                appendLine(result, "");
            }
        }

        private void appendLine(StringBuilder result, string line)
        {
            result.AppendLine(line);
        }

        private void createRepresentation(Snapshot snapshot, StringBuilder result, IReadonlyIndexContainer indexContainer)
        {
            createIndexRepresentation(snapshot, result, indexContainer.UnknownIndex, indexContainer.UnknownIndex.ToString());

            foreach (var item in indexContainer.Indexes)
            {
                MemoryIndex index = item.Value;
                createIndexRepresentation(snapshot, result, index, index.ToString());
            }
        }

        private void createIndexRepresentation(Snapshot snapshot, StringBuilder result, MemoryIndex index, string name)
        {
            result.AppendFormat("{0}: {{ ", name);

            MemoryEntry dataEntry, infoEntry;
            if (snapshot.Data.Readonly.TryGetMemoryEntry(index, out dataEntry))
            {
                result.Append(dataEntry.ToString(snapshot));
            }

            if (snapshot.Infos.Readonly.TryGetMemoryEntry(index, out infoEntry))
            {
                result.Append(" INFO: ");
				result.Append(infoEntry.ToString());
            }
            result.AppendLine(" }");
        }
    }
}