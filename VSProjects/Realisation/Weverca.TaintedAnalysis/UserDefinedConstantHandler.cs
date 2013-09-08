using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
{
    /// <summary>
    /// Provides the functionality to insert and retrieve constant values from memoryModel.
    /// </summary>
    class UserDefinedConstantHandler
    {
        /// <summary>
        /// Gets Constant value from FlowOutputSet
        /// </summary>
        /// <param name="outset">FlowOutputSet, which contains values</param>
        /// <param name="name">Constant name</param>
        /// <returns>List of possible values</returns>
        public static List<Value> getConstant(FlowOutputSet outset, QualifiedName name)
        {
            List<Value> result = new List<Value>();
            outset.FetchFromGlobal(new VariableName(".constants"));
            foreach (Value value in outset.ReadValue(new VariableName(".constants")).PossibleValues)
            {
                if (value is AssociativeArray)
                {
                    AssociativeArray constArray = (AssociativeArray)value;
                    //case insensitive constants
                    foreach (Value it in outset.GetIndex(constArray, outset.CreateIndex("." + name.Name.LowercaseValue)).PossibleValues)
                    {
                        if (!(it is UndefinedValue))
                        {
                            result.Add(it);
                        }
                    }
                    //case sensitive constant
                    foreach (Value it in outset.GetIndex(constArray, outset.CreateIndex(name.Name.Value)).PossibleValues)
                    {
                        if (!(it is UndefinedValue))
                        {
                            result.Add(it);
                        }
                    }
                }
            }
            if (result.Count == 0)
            {
                result.Add(outset.UndefinedValue);
                
            }

          

            return result;
        }

        /// <summary>
        /// Inserts new constant into FlowOutputSet.
        /// </summary>
        /// <param name="outset">FlowOutputSet, where to isert the values.</param>
        /// <param name="name">Constant name.</param>
        /// <param name="value">constant value</param>
        /// <param name="caseInsensitive">determins if the constant is case sensitive of insensitive</param>
        public static void insertConstant(FlowOutputSet outset, QualifiedName name, MemoryEntry value, bool caseInsensitive = false)
        {
            outset.FetchFromGlobal(new VariableName(".constants"));
            foreach (Value array in outset.ReadValue(new VariableName(".constants")).PossibleValues)
            {
                if (array is AssociativeArray)
                {
                    AssociativeArray constArray = (AssociativeArray)array;
                    ContainerIndex index;
                    if (caseInsensitive == true)
                    {
                        index = outset.CreateIndex("." + name.Name.LowercaseValue);
                    }
                    else
                    {
                        index = outset.CreateIndex(name.Name.Value);
                    }
                    MemoryEntry entry = outset.GetIndex(constArray, index);
                    if (entry.PossibleValues.Count() == 0 || (entry.PossibleValues.Count() == 1 && entry.PossibleValues.ElementAt(0).Equals(outset.UndefinedValue)))
                    {

                        //replace undefined values with string ""
                        outset.SetIndex(constArray, index, value);
                    }
                }
            }
        }
    }
}
