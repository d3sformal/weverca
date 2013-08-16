﻿using System;
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


    class VariableInfoHandler 
    {

        public static void setDirty(FlowController flow, Value value)
        {
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                setDirty(flow, value, val);
            }
        }


        private static Dictionary<DirtyType, bool> MergeAndCreateVariableInfo(FlowController flow, Value value)
        {

            InfoValue[] infos = flow.OutSet.ReadInfo(value);
            Dictionary<DirtyType, bool> dirtyFlags = VariableInfo.CreateDirtyFlags();

            foreach (var info in infos)
            {
                if (info.GetType() == typeof(InfoValue<VariableInfo>))
                {
                    Array values = DirtyType.GetValues(typeof(DirtyType));
                    foreach (DirtyType val in values)
                    {
                        dirtyFlags[val] |= ((InfoValue<VariableInfo>)info).Data.isDirty(val);
                    }
                }
            }
            return dirtyFlags;
        }

        public static void setDirty(FlowController flow, Value value, DirtyType dirty)
        {
            VariableInfo newInfo = new VariableInfo(MergeAndCreateVariableInfo(flow,value));
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[] {flow.OutSet.CreateInfo(newInfo)});
        }

        public static void setClean(FlowController flow, Value value, DirtyType dirty)
        {
            var flags = MergeAndCreateVariableInfo(flow, value);
            flags[dirty] = false;
            VariableInfo newInfo = new VariableInfo(flags);
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[] { flow.OutSet.CreateInfo(newInfo) });
        }

        public static bool isDirty(FlowController flow, Value value, DirtyType dirty)
        { 
            bool result=false;
            foreach(InfoValue info in flow.OutSet.ReadInfo(value))
            {
                if (info.GetType() == typeof(InfoValue<VariableInfo>))
                {
                    result |= ((InfoValue<VariableInfo>)info).Data.isDirty(dirty);
                }
            }
            return result;
        }

        public static void CopyFlags(FlowController flow, Value source, Value value)
        {
            CopyFlags(flow, new MemoryEntry(source), value);
        }

        public static void CopyFlags(FlowController flow, IEnumerable<MemoryEntry> source, MemoryEntry target)
        {
            foreach (var value in target.PossibleValues)
            {
                CopyFlags(flow, source, value);
            }
        }

        public static void CopyFlags(FlowController flow, MemoryEntry source, MemoryEntry target)
        {
            foreach (var value in target.PossibleValues)
            {
                CopyFlags(flow, source, value);
            }
        }

        public static void CopyFlags(FlowController flow, IEnumerable<MemoryEntry> source, Value value)
        {
            var dirtyFlags = VariableInfo.CreateDirtyFlags();
            foreach (var entry in source)
            {
                var functionResult = copyFlags(flow, entry);
                dirtyFlags = mergeFlags(dirtyFlags, functionResult);
            }

         
            VariableInfo newInfo = new VariableInfo(dirtyFlags);
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[] { flow.OutSet.CreateInfo(newInfo) });
        }

        public static void CopyFlags(FlowController flow, MemoryEntry source, Value value)
        {
            var dirtyFlags = copyFlags(flow, source);
           
            VariableInfo newInfo = new VariableInfo(dirtyFlags);
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[] { flow.OutSet.CreateInfo(newInfo) });
            
        }

        private static Dictionary<DirtyType, bool> copyFlags(FlowController flow, MemoryEntry source)
        {
            var dirtyFlags = VariableInfo.CreateDirtyFlags();
            foreach (Value value in source.PossibleValues)
            {
                var functionResult=MergeAndCreateVariableInfo(flow, value);
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
    class VariableInfo
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

        public VariableInfo()
        {
            dirtyFlags = CreateDirtyFlags();
        }

        public VariableInfo(Dictionary<DirtyType, bool> dirtyFlags)
        {
            this.dirtyFlags = dirtyFlags;
        }

        public bool isDirty(DirtyType dirty)
        {
            return dirtyFlags[dirty];
        }
    }
}
