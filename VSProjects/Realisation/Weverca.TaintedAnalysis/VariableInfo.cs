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

        public static void setDirty(FlowController flow, Value value, DirtyType dirty)
        {
            InfoValue<VariableInfo>[] InfoValue = (InfoValue<VariableInfo>[])flow.OutSet.ReadInfo(value);
            Dictionary<DirtyType, bool> dirtyFlags = VariableInfo.CreateDirtyFlags();

            foreach (var info in InfoValue)
            {
                Array values = DirtyType.GetValues(typeof(DirtyType));
                foreach (DirtyType val in values)
                {
                    dirtyFlags[val] |= info.Data.isDirty(val);
                }
            }

            VariableInfo newInfo = new VariableInfo(dirtyFlags);
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[1] {flow.OutSet.CreateInfo(newInfo)});
        }

        public static void setClean(FlowController flow, Value value, DirtyType dirty)
        {
            InfoValue<VariableInfo>[] InfoValue = (InfoValue<VariableInfo>[])flow.OutSet.ReadInfo(value);
            Dictionary<DirtyType, bool> dirtyFlags = VariableInfo.CreateDirtyFlags();

            foreach (var info in InfoValue)
            {
                Array values = DirtyType.GetValues(typeof(DirtyType));
                foreach (DirtyType val in values)
                {
                    dirtyFlags[val] |= info.Data.isDirty(val);
                }
            }
            dirtyFlags[dirty] = false;
            VariableInfo newInfo = new VariableInfo(dirtyFlags);
            flow.OutSet.SetInfo(value, new InfoValue<VariableInfo>[1] { flow.OutSet.CreateInfo(newInfo) });
        }

        public static bool isDirty(FlowController flow, Value value, DirtyType dirty)
        { 
            bool result=false;
            foreach(InfoValue info in flow.OutSet.ReadInfo(value))
            {
                result|=((InfoValue<VariableInfo>)info).Data.isDirty(dirty);
            }
            return result;
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
