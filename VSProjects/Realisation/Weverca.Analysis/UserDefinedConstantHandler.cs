using System.Collections.Generic;
using System.Linq;

using PHP.Core;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
{
    /// <summary>
    /// Provides the functionality to insert and retrieve constant values from memoryModel.
    /// </summary>
    internal class UserDefinedConstantHandler
    {
        private static readonly VariableName constantVariable = new VariableName(".constants");

        /// <summary>
        /// Gets constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet which contains values</param>
        /// <param name="name">Constant name</param>
        /// <returns>Memory entry of possible values</returns>
        public static MemoryEntry GetConstant(FlowOutputSet outset, QualifiedName name)
        {
            MemoryEntry entry;
            bool isAlwaysDefined;
            TryGetConstant(outset, name, out entry, out isAlwaysDefined);
            return entry;
        }

        /// <summary>
        /// Tries to get constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet which contains values</param>
        /// <param name="name">Constant name</param>
        /// <param name="entry">Memory entry of possible values</param>
        /// <param name="isNotDefined">Indicates whether all values are derived from constant name</param>
        /// <returns><c>true</c> if constant is defined in every pass, otherwise <c>false</c></returns>
        public static bool TryGetConstant(FlowOutputSet outset, QualifiedName name,
            out MemoryEntry entry, out bool isNotDefined)
        {
            var values = new HashSet<Value>();
            outset.FetchFromGlobal(constantVariable);
            var constantArrays = outset.ReadValue(constantVariable);
            Debug.Assert(constantArrays.Count > 0, "Internal array of constants is always defined");

            var isAlwaysDefined = true;
            isNotDefined = true;

            foreach (var value in constantArrays.PossibleValues)
            {
                var constantArray = value as AssociativeArray;
                if (constantArray != null)
                {
                    MemoryEntry arrayEntry;

                    // Case-insensitive constants
                    var index = outset.CreateIndex("." + name.Name.LowercaseValue);
                    if (outset.TryGetIndex(constantArray, index, out arrayEntry))
                    {
                        if (isNotDefined)
                        {
                            isNotDefined = false;
                        }

                        values.UnionWith(arrayEntry.PossibleValues);
                    }
                    else
                    {
                        // Case-sensitive constant
                        index = outset.CreateIndex(name.Name.Value);
                        if (outset.TryGetIndex(constantArray, index, out arrayEntry))
                        {
                            if (isNotDefined)
                            {
                                isNotDefined = false;
                            }

                            values.UnionWith(arrayEntry.PossibleValues);
                        }
                        else
                        {
                            if (isAlwaysDefined)
                            {
                                isAlwaysDefined = false;
                            }

                            // Undefined constant is interpreted as a string
                            var stringValue = outset.CreateString(name.Name.Value);
                            values.Add(stringValue);
                        }
                    }
                }
            }

            entry = new MemoryEntry(values);
            return isAlwaysDefined;
        }

        /// <summary>
        /// Inserts new constant into FlowOutputSet.
        /// </summary>
        /// <param name="outset">FlowOutputSet, where to insert the values.</param>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value</param>
        /// <param name="caseInsensitive">Determines if the constant is case sensitive of insensitive</param>
        public static void insertConstant(FlowOutputSet outset, QualifiedName name,
            MemoryEntry value, bool caseInsensitive = false)
        {
            outset.FetchFromGlobal(constantVariable);
            var constantArrays = outset.ReadValue(constantVariable);

            foreach (var array in constantArrays.PossibleValues)
            {
                var constantArray = array as AssociativeArray;
                if (constantArray != null)
                {
                    ContainerIndex index;
                    if (caseInsensitive == true)
                    {
                        index = outset.CreateIndex("." + name.Name.LowercaseValue);
                    }
                    else
                    {
                        index = outset.CreateIndex(name.Name.Value);
                    }

                    if (!outset.ArrayIndexExists(constantArray, index))
                    {
                        // TODO: Replace undefined values with string ""? Really?
                        outset.SetIndex(constantArray, index, value);
                    }
                }
            }
        }
        
    }
}
