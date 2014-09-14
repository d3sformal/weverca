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
    class PrintAlgorithm : IPrintAlgorithm, IAlgorithmFactory<IPrintAlgorithm>
    {
        private Snapshot snapshot;
        private StringBuilder result;

        public IPrintAlgorithm CreateInstance()
        {
            return new PrintAlgorithm();
        }

        public string SnapshotToString(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            this.result = new StringBuilder();

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                appendLine("===LOCALS===");
                createRepresentation(snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyVariables);
            }
            appendLine("===GLOBALS===");
            createRepresentation(snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyVariables);

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                appendLine("===LOCAL CONTROLS===");
                createRepresentation(snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyControllVariables);
            }

            appendLine("===GLOBAL CONTROLS===");
            createRepresentation(snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyControllVariables);

            appendLine("\n===ARRAYS===");
            createArraysRepresentation();

            appendLine("\n===FIELDS===");
            createFieldsRepresentation();

            appendLine("\n===ALIASES===");
            createAliasesRepresentation();

            return result.ToString();
        }

        private void createAliasesRepresentation()
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

        private void createFieldsRepresentation()
        {
            foreach (var item in snapshot.Structure.Readonly.ObjectDescriptors)
            {
                createRepresentation(item.Value);
                appendLine("");
            }
        }

        private void createArraysRepresentation()
        {
            foreach (var item in snapshot.Structure.Readonly.ArrayDescriptors)
            {
                IArrayDescriptor descriptor = item.Value;
                AssociativeArray associativeArray = item.Key;

                createIndexRepresentation(descriptor.UnknownIndex, String.Format("${0}[?]", associativeArray.UID));

                foreach (var indexItem in descriptor.Indexes)
                {
                    MemoryIndex index = indexItem.Value;
                    string indexName = indexItem.Key;
                    createIndexRepresentation(index, String.Format("${0}[{1}]", associativeArray.UID, indexName));
                }

                appendLine("");
            }
        }

        private void appendLine(string line)
        {
            result.AppendLine(line);
        }

        private void createRepresentation(IReadonlyIndexContainer indexContainer)
        {
            createIndexRepresentation(indexContainer.UnknownIndex, indexContainer.UnknownIndex.ToString());

            foreach (var item in indexContainer.Indexes)
            {
                MemoryIndex index = item.Value;
                createIndexRepresentation(index, index.ToString());
            }
        }

        private void createIndexRepresentation(MemoryIndex index, string name)
        {
            result.AppendFormat("{0}: {{ ", name);

            MemoryEntry dataEntry, infoEntry;
            if (snapshot.Data.Readonly.TryGetMemoryEntry(index, out dataEntry))
            {
				result.Append(dataEntry.ToString());
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