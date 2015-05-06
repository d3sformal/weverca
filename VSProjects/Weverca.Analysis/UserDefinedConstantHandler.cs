/*
Copyright (c) 2012-2014 Marcel Kikta

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
        public static readonly VariableName constantVariable = new VariableName(".constants");

        /// <summary>
        /// Gets constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet which contains values</param>
        /// <param name="name">Constant name</param>
        /// <returns>Memory entry of possible values</returns>
        public static MemoryEntry GetConstant(FlowOutputSet outset, QualifiedName name)
        {
            MemoryEntry entry;
            TryGetConstant(outset, name, out entry);
            return entry;
        }

        /// <summary>
        /// Tries to get constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet which contains values</param>
        /// <param name="name">Constant name</param>
        /// <param name="entry">Memory entry of possible values</param>
        /// <returns><c>true</c> if constant is never defined, otherwise <c>true</c></returns>
        public static bool TryGetConstant(FlowOutputSet outset, QualifiedName name,
            out MemoryEntry entry)
        {
            var context=outset.Snapshot;
            var values = new HashSet<Value>();
            
            var constantArrays = outset.ReadControlVariable(constantVariable);
            Debug.Assert(constantArrays.IsDefined(context), "Internal array of constants is always defined");
                      
            var caseInsensitiveConstant=constantArrays.ReadIndex(context,new MemberIdentifier("." + name.Name.LowercaseValue));
            if(caseInsensitiveConstant.IsDefined(context)){
                entry=caseInsensitiveConstant.ReadMemory(context);
                return false;
            }

            //else there can be case sensitive constant

            var caseSensitiveConstant = constantArrays.ReadIndex(context, new MemberIdentifier("#" + name.Name.Value));
            if(caseSensitiveConstant.IsDefined(context)){
                entry=caseSensitiveConstant.ReadMemory(context);
                return false;
            }
            
           // Undefined constant is interpreted as a string
            var stringValue = outset.CreateString(name.Name.Value);
            entry=new MemoryEntry(stringValue);
            
            return true;
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
                constant = constantArrays.ReadIndex(outset.Snapshot, new MemberIdentifier("#"+name.Name.Value));
            }
            if (!constant.IsDefined(outset.Snapshot))
            {
                constant.WriteMemory(outset.Snapshot, value);
            }

        }

    }
}
