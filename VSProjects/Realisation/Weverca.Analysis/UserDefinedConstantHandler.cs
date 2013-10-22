using System.Collections.Generic;
using System.Linq;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Provides the functionality to insert and retrieve constant values from memoryModel.
    /// </summary>
    internal class UserDefinedConstantHandler
    {
        private static readonly VariableName constantVariable = new VariableName("constants");

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
            var context=outset.Snapshot;
            var values = new HashSet<Value>();
            
            var constantArrays = outset.ReadControlVariable(constantVariable);
            Debug.Assert(constantArrays.IsDefined(context), "Internal array of constants is always defined");
                      
            var caseInsensitiveConstant=constantArrays.ReadIndex(context,new MemberIdentifier("." + name.Name.LowercaseValue));
            if(caseInsensitiveConstant.IsDefined(context)){
                entry=caseInsensitiveConstant.ReadMemory(context);
                isNotDefined=false;
                return true;
            }

            //else there can be case sensitive constant

            var caseSensitiveConstant=constantArrays.ReadIndex(context,new MemberIdentifier(name.Name.Value));
            if(caseSensitiveConstant.IsDefined(context)){
                entry=caseSensitiveConstant.ReadMemory(context);
                isNotDefined=false;
                return true;
            }
            
           // Undefined constant is interpreted as a string
            isNotDefined = true;
            var stringValue = outset.CreateString(name.Name.Value);
            entry=new MemoryEntry(stringValue);
            
            return false;
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
            ReadWriteSnapshotEntryBase constant;
            var constantArrays = outset.ReadControlVariable(constantVariable);
            if (caseInsensitive == true)
            {
                constant = constantArrays.ReadIndex(outset.Snapshot, new MemberIdentifier("." + name.Name.LowercaseValue));
            }
            else
            {
                constant = constantArrays.ReadIndex(outset.Snapshot, new MemberIdentifier(name.Name.Value));
            }
            if (!constant.IsDefined(outset.Snapshot))
            {
                constant.WriteMemory(outset.Snapshot, value);
            }

        }

    }
}
