using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.Output.Output;

namespace Weverca.App
{
    class SnapshotTextGenerator
    {
        private Snapshot snapshot;
        private StringBuilder result;

        private OutputBase output;

        private Dictionary<ObjectValue, List<MemoryIndex>> objects = new Dictionary<ObjectValue, List<MemoryIndex>>();

        public SnapshotTextGenerator(OutputBase output)
        {
            this.output = output;
        }

        public void GenerateSnapshotText(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            this.result = new StringBuilder();

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                output.EmptyLine();
                output.Headline2("Local variables");
                createRepresentation(snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyVariables);
            }
            output.EmptyLine();
            output.Headline2("Global variables");
            createRepresentation(snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyVariables);

            if (snapshot.CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                output.EmptyLine();
                output.Headline2("Local controls");
                createRepresentation(snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyControllVariables);
            }

            output.EmptyLine();
            output.Headline2("Global controls");
            createRepresentation(snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyControllVariables);

            output.EmptyLine();
            output.Headline2("Objects");
            createObjectsRepresentation();


            /*output.EmptyLine();
            output.Headline2("===ARRAYS===");
            createArraysRepresentation();


            output.EmptyLine();
            output.Headline2("===ALIASES===");
            createAliasesRepresentation();*/
        }

        private void createObjectMapping() {
            foreach (var item in snapshot.Structure.Readonly.IndexDefinitions)
            {
                IIndexDefinition definition = item.Value;
                MemoryIndex index = item.Key;

                if (definition.Objects != null && definition.Objects.Count > 0)
                {
                    foreach (ObjectValue objectValue in definition.Objects)
                    {
                        List<MemoryIndex> indexes;
                        if (!objects.TryGetValue(objectValue, out indexes))
                        {
                            indexes = new List<MemoryIndex>();
                            objects.Add(objectValue, indexes);
                        }
                        indexes.Add(index);
                    }
                }
            }
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

        private void createObjectsRepresentation()
        {
            createObjectMapping();

            foreach (var item in snapshot.Structure.Readonly.ObjectDescriptors)
            {
                ObjectValue objectValue = item.Key;
                IObjectDescriptor descriptor = item.Value;

                output.line();
                output.variable(objectValue.ToString());
                output.line();

                output.hint("Object type: ");
                if (descriptor.Type != null)
                {
                    output.info(descriptor.Type.QualifiedName.ToString());
                }
                else
                {
                    output.error("Object has no type");
                }
                output.line();

                output.hint("Parent indexes: ");
                List<MemoryIndex> indexes;
                if (objects.TryGetValue(objectValue, out indexes))
                {
                    foreach (MemoryIndex index in indexes)
                    {
                        output.variable(index.ToString());
                        output.info(", ");
                    }
                }
                else
                {
                    output.error("Object not referenced");
                }

                output.Indent();
                output.line();

                createRepresentation(item.Value);

                output.Dedent();
                output.line();
            }
        }

        private void createArraysRepresentation()
        {
            foreach (var item in snapshot.Structure.Readonly.ArrayDescriptors)
            {
                IArrayDescriptor descriptor = item.Value;
                AssociativeArray associativeArray = item.Key;

                createIndexRepresentation(descriptor.UnknownIndex, String.Format("{0}[?]", associativeArray.UID));

                foreach (var indexItem in descriptor.Indexes)
                {
                    MemoryIndex index = indexItem.Value;
                    string indexName = indexItem.Key;
                    createIndexRepresentation(index, String.Format("{0}[{1}]", associativeArray.UID, indexName));
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
            output.line();
            output.variable(name);
            
            MemoryEntry dataEntry;
            if (snapshot.Data.Readonly.TryGetMemoryEntry(index, out dataEntry))
            {
                output.line();
                output.hint("Data: ");
                output.info(dataEntry.ToString());
            }


            MemoryEntry infoEntry;
            if (snapshot.Infos.Readonly.TryGetMemoryEntry(index, out infoEntry))
            {
                output.line();
                output.hint("Info: ");
                output.info(infoEntry.ToString());
            }

            IIndexDefinition definition;
            if (snapshot.Structure.Readonly.TryGetIndexDefinition(index, out definition) && definition.Array != null)
            {
                var aliases = definition.Aliases;
                if (aliases != null && aliases.HasAliases)
                {
                    if (aliases.MustAliases.Count > 0)
                    {
                        output.line();
                        output.hint("Must aliases: ");
                        foreach (var alias in aliases.MustAliases)
                        {
                            output.variable(alias.ToString());
                            output.info(", ");
                        }
                    }

                    if (aliases.MayAliases.Count > 0)
                    {
                        output.line();
                        output.hint("May aliases: ");
                        foreach (var alias in aliases.MayAliases)
                        {
                            output.variable(alias.ToString());
                            output.info(", ");
                        }
                    }
                }

                IArrayDescriptor descriptor;
                if (snapshot.Structure.Readonly.TryGetDescriptor(definition.Array, out descriptor))
                {
                    output.Indent();
                    output.line();
                    createRepresentation(descriptor);
                    output.Dedent();
                }
            }
            
            output.line();
        }
    }
}
