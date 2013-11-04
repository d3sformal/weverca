using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;

using Weverca.AnalysisFramework.Memory;

using PHP.Core;
using Weverca.Analysis.ExpressionEvaluator;

namespace Weverca.Analysis
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
            //todo este neje iste ze sa bude pouzivate nove api, je mozne ze to bude zabudovane vo viacfazovej analyze
            InfoValue[] infos = outSet.ReadInfo(value);
            Dictionary<DirtyType, bool> dirtyFlags = FlagsInfoValue.CreateDirtyFlags();

            foreach (var info in infos)
            {
                if (info is InfoValue<FlagsInfoValue>)
                {
                    Array values = DirtyType.GetValues(typeof(DirtyType));
                    foreach (DirtyType val in values)
                    {
                        dirtyFlags[val] |= ((InfoValue<FlagsInfoValue>)info).Data.isDirty(val);
                    }
                }
            }
            return dirtyFlags;
        }

        public static void setDirty(FlowOutputSet outSet, Value value, DirtyType dirty)
        {
            var flags = MergeAndCreateVariableInfo(outSet, value);
            flags[dirty] = true;
            FlagsInfoValue newInfo = new FlagsInfoValue(flags);
            setInfoAndKeppOtherInfos(outSet, value, newInfo);    
        }

        public static void setClean(FlowOutputSet outSet, Value value, DirtyType dirty)
        {
            var flags = MergeAndCreateVariableInfo(outSet, value);
            flags[dirty] = false;
            FlagsInfoValue newInfo = new FlagsInfoValue(flags);
            setInfoAndKeppOtherInfos(outSet, value, newInfo);    
        }

        public static bool isDirty(FlowOutputSet outSet, Value value, DirtyType dirty)
        { 
            bool result=false;
            
            foreach(InfoValue info in outSet.ReadInfo(value))
            {
                if (info is InfoValue<FlagsInfoValue>)
                {
                    result |= ((InfoValue<FlagsInfoValue>)info).Data.isDirty(dirty);
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
            var dirtyFlags = FlagsInfoValue.CreateDirtyFlags();
            foreach (var entry in source)
            {
                var functionResult = copyFlags(outSet, entry);
                dirtyFlags = mergeFlags(dirtyFlags, functionResult);
            }

         
            FlagsInfoValue newInfo = new FlagsInfoValue(dirtyFlags);
            setInfoAndKeppOtherInfos(outSet, value, newInfo);    
        }

        public static void CopyFlags(FlowOutputSet outSet, MemoryEntry source, Value value)
        {
            var dirtyFlags = copyFlags(outSet, source);
           
            FlagsInfoValue newInfo = new FlagsInfoValue(dirtyFlags);
            setInfoAndKeppOtherInfos(outSet, value, newInfo);            
        }

        private static Dictionary<DirtyType, bool> copyFlags(FlowOutputSet outSet, MemoryEntry source)
        {
            var dirtyFlags = FlagsInfoValue.CreateDirtyFlags();
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

        private static void setInfoAndKeppOtherInfos(FlowOutputSet outSet, Value value, FlagsInfoValue newInfo)
        {
            List<InfoValue> newInfos = new List<InfoValue>();
            foreach (var info in outSet.ReadInfo(value))
            {
                if (!(info is InfoValue<FlagsInfoValue>))
                {
                    newInfos.Add(info);
                }
            }
            bool atleastOneFlagTrue = false;
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                atleastOneFlagTrue |= newInfo.isDirty(val);
            }
            //store only if the value contains at least on drity flag
            if (atleastOneFlagTrue)
            {
                newInfos.Add(outSet.CreateInfo(newInfo));
            }
            if (newInfos.Count != 0)
            {
                if (ValueTypeResolver.CanBeDirty(value))
                {
                    outSet.SetInfo(value, newInfos.ToArray());
                }
            }
        }

    }
    
    /// <summary>
    /// Information about variable
    /// </summary>
    class FlagsInfoValue
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

        public FlagsInfoValue()
        {
            dirtyFlags = CreateDirtyFlags();
        }

        public FlagsInfoValue(Dictionary<DirtyType, bool> dirtyFlags)
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
