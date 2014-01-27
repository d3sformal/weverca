﻿using System;
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
    public enum DirtyType
    {
        HTMLDirty = 1, SQLDirty = 2, FilePathDirty = 4
    }


    public static class FlagsHandler
    {
        public static Flag GetFlags(IEnumerable<Value> source)
        {
            Dictionary<DirtyType, bool> flags = Flag.CreateCleanFlags();
            foreach (Value value in source)
            {
                if (value.GetInfo<Flag>() != null)
                {
                    mergeFlags(flags, value.GetInfo<Flag>());
                }
            }
            return new Flag(flags);
        }

        public static IEnumerable<Value> CopyFlags(IEnumerable<Value> source, IEnumerable<Value> dest)
        {
            List<Value> result = new List<Value>();


            Flag newFlag = GetFlags(source);
            foreach (Value value in dest)
            {
                if (ValueTypeResolver.CanBeDirty(value))
                {
                    result.Add(value.SetInfo(newFlag));
                }
                else
                {
                    result.Add(value);
                }
            }

            return result;
        }

        public static Dictionary<DirtyType, bool> GetFlagsFromValues(params Value[] source)
        {
            return GetFlagsFromValues(source as IEnumerable<Value>);
        }

        public static Dictionary<DirtyType, bool> GetFlagsFromValues(IEnumerable<Value> source)
        {
            var flags = Flag.CreateCleanFlags();
            foreach (Value value in source)
            {
                if (value.GetInfo<Flag>() != null)
                {
                    mergeFlags(flags, value.GetInfo<Flag>());
                }
            }

            return flags;
        }

        public static IEnumerable<Value> CopyFlags(Value source, IEnumerable<Value> dest)
        {
            List<Value> sourceList = new List<Value>();
            sourceList.Add(source);
            return CopyFlags(sourceList, dest);
        }

        public static Value CopyFlags(IEnumerable<Value> source, Value dest)
        {
            List<Value> destList = new List<Value>();
            destList.Add(dest);
            return CopyFlags(source, destList).First();
        }

        public static Value CopyFlags(Value source, Value dest)
        {
            List<Value> sourceList = new List<Value>();
            sourceList.Add(source);
            List<Value> destList = new List<Value>();
            destList.Add(dest);
            return CopyFlags(sourceList, destList).First();
        }

        public static bool IsDirty(Value value,DirtyType dirty)
        {
            if (value.GetInfo<Flag>() == null)
            {
                return false;
            }
            else 
            {
                return value.GetInfo<Flag>().isDirty(dirty);
            }
        }

        public static bool IsDirty(IEnumerable<Value> entry, DirtyType dirty)
        {
            bool res = false;
            foreach (var value in entry)
            {
                res |= IsDirty(value, dirty);
            }
            return res;
        }

        public static IEnumerable<Value> Clean(IEnumerable<Value> source, DirtyType dirty)
        {
            List<Value> result = new List<Value>();
            foreach(Value value in source)
            {
                var flag = GetFlagsFromValues(value);
                flag[dirty] = false;
                result.Add(value.SetInfo(new Flag(flag)));
            }
            return result;
        }

        private static Dictionary<DirtyType, bool> mergeFlags(Dictionary<DirtyType, bool> dictFlag, Flag flag)
        {
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                dictFlag[val] |= flag.isDirty(val);
            }
            return dictFlag;
        }

    }

    public class Flag : InfoDataBase
    {
        private readonly Dictionary<DirtyType, bool> dirtyFlags;
        public static Dictionary<DirtyType, bool> CreateCleanFlags()
        {
            var flags = new Dictionary<DirtyType, bool>();
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                flags.Add(val, false);
            }
            return flags;
        }

        public static Dictionary<DirtyType, bool> CreateDirtyFlags()
        {
            var flags = new Dictionary<DirtyType, bool>();
            Array values = DirtyType.GetValues(typeof(DirtyType));
            foreach (DirtyType val in values)
            {
                flags.Add(val, true);
            }
            return flags;
        }

        public Flag()
        {
            dirtyFlags = CreateCleanFlags();
        }

        public Flag(Dictionary<DirtyType, bool> dirtyFlags)
        {
            this.dirtyFlags = dirtyFlags;
        }

        public bool isDirty(DirtyType dirty)
        {
            return dirtyFlags[dirty];
        }
        protected override int getHashCode()
        {
            int result = 0;
            foreach (var flag in dirtyFlags)
            {
                if (flag.Value == true)
                {
                    result += (int)flag.Key;
                }
            }
            return result;
        }

        protected override bool equals(InfoDataBase other)
        {
            return getHashCode() == (other as Flag).getHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is InfoDataBase)
                return equals(obj as InfoDataBase);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return getHashCode();
        }

    }

}
