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
                createRepresentation(item.Value);
                appendLine("");
            }
        }

        private void appendLine(string line)
        {
            result.AppendLine(line);
        }

        private void createRepresentation(IReadonlyIndexContainer indexContainer)
        {
            createIndexRepresentation(indexContainer.UnknownIndex);

            foreach (var item in indexContainer.Indexes)
            {
                MemoryIndex index = item.Value;
                createIndexRepresentation(index);
            }
        }

        private void createIndexRepresentation(MemoryIndex index)
        {
            result.AppendFormat("{0}: {{ ", index);

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
