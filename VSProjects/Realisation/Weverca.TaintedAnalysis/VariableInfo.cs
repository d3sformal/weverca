using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis;

using Weverca.Analysis.Memory;

using PHP.Core;

namespace Weverca.TaintedAnalysis
{
    enum DirtyType { 
        HTMLDirty, SQLDirty, FilePathDirty
    }


    class ValueInfoHandler 
    {

        public static void setDirty(FlowOutputSet outSet, Value value)
        {
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                setDirty(outSet, value, val);
            }
        }


        private static Dictionary<DirtyType, bool> MergeAndCreateVariableInfo(FlowOutputSet outSet, Value value)
        {

            InfoValue[] infos = outSet.ReadInfo(value);
            Dictionary<DirtyType, bool> dirtyFlags = ValueInfo.CreateDirtyFlags();

            foreach (var info in infos)
            {
                if (info is InfoValue<ValueInfo>)
                {
                    Array values = DirtyType.GetValues(typeof(DirtyType));
                    foreach (DirtyType val in values)
                    {
                        dirtyFlags[val] |= ((InfoValue<ValueInfo>)info).Data.isDirty(val);
                    }
                }
            }
            return dirtyFlags;
        }

        public static void setDirty(FlowOutputSet outSet, Value value, DirtyType dirty)
        {
            var flags = MergeAndCreateVariableInfo(outSet, value);
            flags[dirty] = true;
            ValueInfo newInfo = new ValueInfo(flags);
            outSet.SetInfo(value, new InfoValue<ValueInfo>[] {outSet.CreateInfo(newInfo)});
        }

        public static void setClean(FlowOutputSet outSet, Value value, DirtyType dirty)
        {
            var flags = MergeAndCreateVariableInfo(outSet, value);
            flags[dirty] = false;
            ValueInfo newInfo = new ValueInfo(flags);
            outSet.SetInfo(value, new InfoValue<ValueInfo>[] { outSet.CreateInfo(newInfo) });
        }

        public static bool isDirty(FlowOutputSet outSet, Value value, DirtyType dirty)
        { 
            bool result=false;
            foreach(InfoValue info in outSet.ReadInfo(value))
            {
                if (info is InfoValue<ValueInfo>)
                {
                    result |= ((InfoValue<ValueInfo>)info).Data.isDirty(dirty);
                }
            }
            return result;
        }

        public static void CopyFlags(FlowOutputSet outSet, Value source, Value value)
        {
            CopyFlags(outSet, new MemoryEntry(source), value);
        }

        public static void CopyFlags(FlowOutputSet outSet, IEnumerable<MemoryEntry> source, MemoryEntry target)
        {
            foreach (var value in target.PossibleValues)
            {
                CopyFlags(outSet, source, value);
            }
        }

        public static void CopyFlags(FlowOutputSet outSet, MemoryEntry source, MemoryEntry target)
        {
            foreach (var value in target.PossibleValues)
            {
                CopyFlags(outSet, source, value);
            }
        }

        public static void CopyFlags(FlowOutputSet outSet, IEnumerable<MemoryEntry> source, Value value)
        {
            var dirtyFlags = ValueInfo.CreateDirtyFlags();
            foreach (var entry in source)
            {
                var functionResult = copyFlags(outSet, entry);
                dirtyFlags = mergeFlags(dirtyFlags, functionResult);
            }

         
            ValueInfo newInfo = new ValueInfo(dirtyFlags);
            outSet.SetInfo(value, new InfoValue<ValueInfo>[] { outSet.CreateInfo(newInfo) });
        }

        public static void CopyFlags(FlowOutputSet outSet, MemoryEntry source, Value value)
        {
            var dirtyFlags = copyFlags(outSet, source);
           
            ValueInfo newInfo = new ValueInfo(dirtyFlags);
            outSet.SetInfo(value, new InfoValue<ValueInfo>[] { outSet.CreateInfo(newInfo) });
            
        }

        private static Dictionary<DirtyType, bool> copyFlags(FlowOutputSet outSet, MemoryEntry source)
        {
            var dirtyFlags = ValueInfo.CreateDirtyFlags();
            foreach (Value value in source.PossibleValues)
            {
                var functionResult=MergeAndCreateVariableInfo(outSet, value);
                dirtyFlags = mergeFlags(dirtyFlags, functionResult);
            }
            return dirtyFlags;
        }

        private static Dictionary<DirtyType, bool> mergeFlags(Dictionary<DirtyType, bool> flag1,Dictionary<DirtyType, bool> flag2)
        {
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                flag1[val] |= flag2[val];
            }
            return flag1;
        }

    }
    
    /// <summary>
    /// Information about variable
    /// </summary>
    class ValueInfo
    {
        private Dictionary<DirtyType,bool> dirtyFlags;

        public static Dictionary<DirtyType, bool> CreateDirtyFlags()
        {
            var flags = new Dictionary<DirtyType, bool>();
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                flags.Add(val, false);
            }
            return flags;
        }

        public ValueInfo()
        {
            dirtyFlags = CreateDirtyFlags();
        }

        public ValueInfo(Dictionary<DirtyType, bool> dirtyFlags)
        {
            this.dirtyFlags = dirtyFlags;
        }

        public bool isDirty(DirtyType dirty)
        {
            return dirtyFlags[dirty];
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var flag in dirtyFlags)
            {
                result.AppendFormat("{0}:{1}, ", flag.Key, flag.Value);
            }

            result.Length-=2;
            result.Append("");

            return result.ToString();
        }
    }
}
