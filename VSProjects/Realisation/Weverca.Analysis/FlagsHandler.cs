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
    /// <summary>
    /// Type of flag
    /// </summary>
    public enum FlagType
    {
        /// <summary>
        /// Indicates that value shouldn't go into browser
        /// </summary>
        HTMLDirty = 1,

        /// <summary>
        /// Indicates that value shoudn't go into database
        /// </summary>
        SQLDirty = 2,

        /// <summary>
        /// Indicates that value shoudn't be used as file name
        /// </summary>
        FilePathDirty = 4
    }

    /// <summary>
    /// Static class which provides functionality handling flags in memory entries
    /// </summary>
    public static class FlagsHandler
    {
        #region flag merging

        /// <summary>
        /// Merges flags for all values in argument
        /// </summary>
        /// <param name="source">Values</param>
        /// <returns>Merged flags</returns>
        public static Flags GetFlags(IEnumerable<Value> source)
        {
            Dictionary<FlagType, bool> flags = GetFlagsFromValues(source);
            return new Flags(flags);
        }

        /// <summary>
        /// Merges flags for all values in argument
        /// </summary>
        /// <param name="source">Values</param>
        /// <returns>Dictionary containing flag information</returns>
        public static Dictionary<FlagType, bool> GetFlagsFromValues(params Value[] source)
        {
            return GetFlagsFromValues(source as IEnumerable<Value>);
        }

        /// <summary>
        /// Merges flags for all values in argument
        /// </summary>
        /// <param name="source">Values</param>
        /// <returns>Dictionary containing flag information</returns>
        public static Dictionary<FlagType, bool> GetFlagsFromValues(IEnumerable<Value> source)
        {
            var flags = Flags.CreateCleanFlags();
            foreach (Value value in source)
            {
                if (value.GetInfo<Flags>() != null)
                {
                    mergeFlags(flags, value.GetInfo<Flags>());
                }
            }

            return flags;
        }

        /// <summary>
        /// Merges two flags into one
        /// </summary>
        /// <param name="dictFlag">flags represented with dictionary</param>
        /// <param name="flag">flag represented by Flag object</param>
        /// <returns>Merged flags stored in dictionary</returns>
        private static Dictionary<FlagType, bool> mergeFlags(Dictionary<FlagType, bool> dictFlag, Flags flag)
        {
            Array values = FlagType.GetValues(typeof(FlagType));
            foreach (FlagType val in values)
            {
                dictFlag[val] |= flag.isDirty(val);
            }
            return dictFlag;
        }

        #endregion

        #region flag copying

        /// <summary>
        /// Merges flags from source and copies them into dest, arguments in dest are not modified, new values are created
        /// </summary>
        /// <param name="source">Source values</param>
        /// <param name="dest">Values, where to copy flags</param>
        /// <returns>Values from dest argument with new flags</returns>
        public static IEnumerable<Value> CopyFlags(IEnumerable<Value> source, IEnumerable<Value> dest)
        {
            List<Value> result = new List<Value>();


            Flags newFlag = GetFlags(source);
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

        /// <summary>
        /// Merges flags from source and copies them into dest, arguments in dest are not modified, new values are created
        /// </summary>
        /// <param name="source">Source value</param>
        /// <param name="dest">Values, where to copy flags</param>
        /// <returns>Values from dest argument with new flags</returns>
        public static IEnumerable<Value> CopyFlags(Value source, IEnumerable<Value> dest)
        {
            List<Value> sourceList = new List<Value>();
            sourceList.Add(source);
            return CopyFlags(sourceList, dest);
        }

        /// <summary>
        ///  Merges flags from source and copies them into dest, arguments in dest are not modified, new values are created
        /// </summary>
        /// <param name="source">Source values</param>
        /// <param name="dest">Value, where to copy flags</param>
        /// <returns>Values from dest argument with new flags</returns>
        public static Value CopyFlags(IEnumerable<Value> source, Value dest)
        {
            List<Value> destList = new List<Value>();
            destList.Add(dest);
            return CopyFlags(source, destList).First();
        }

        /// <summary>
        /// Merges flags from source and copies them into dest, arguments in dest are not modified, new values are created
        /// </summary>
        /// <param name="source">Source value</param>
        /// <param name="dest">Value, where to copy flags</param>
        /// <returns>Values from dest argument with new flags</returns>
        public static Value CopyFlags(Value source, Value dest)
        {
            List<Value> sourceList = new List<Value>();
            sourceList.Add(source);
            List<Value> destList = new List<Value>();
            destList.Add(dest);
            return CopyFlags(sourceList, destList).First();
        }

        #endregion

        /// <summary>
        /// Indicates if value contains Dirty flag
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="dirty">Dirty type</param>
        /// <returns>true if value contains Dirty flag</returns>
        public static bool IsDirty(Value value,FlagType dirty)
        {
            if (value.GetInfo<Flags>() == null)
            {
                return false;
            }
            else 
            {
                return value.GetInfo<Flags>().isDirty(dirty);
            }
        }

        /// <summary>
        /// Indicates if one of values contains Dirty flag
        /// </summary>
        /// <param name="entry">Values</param>
        /// <param name="dirty">Dirty type</param>
        /// <returns>true if one of values contains Dirty flag</returns>
        public static bool IsDirty(IEnumerable<Value> entry, FlagType dirty)
        {
            bool res = false;
            foreach (var value in entry)
            {
                res |= IsDirty(value, dirty);
            }
            return res;
        }

        /// <summary>
        /// Cleans source values from specified flagtype
        /// </summary>
        /// <param name="source">Values</param>
        /// <param name="dirty">Dirty type</param>
        /// <returns>Cleaned values, which doesn't contains specified type of flag</returns>
        public static IEnumerable<Value> Clean(IEnumerable<Value> source, FlagType dirty)
        {
            List<Value> result = new List<Value>();
            foreach(Value value in source)
            {
                var flag = GetFlagsFromValues(value);
                flag[dirty] = false;
                result.Add(value.SetInfo(new Flags(flag)));
            }
            return result;
        }
    }


    /// <summary>
    /// Class which stores iformation about  flags, 
    /// It inherits from InfoDataBase, so it can be stored in Value
    /// </summary>
    public class Flags : InfoDataBase
    {
        /// <summary>
        /// Structure, which store all information about flags
        /// </summary>
        private readonly Dictionary<FlagType, bool> dirtyFlags;

        /// <summary>
        /// Create new dictionary, with all clean flags 
        /// </summary>
        /// <returns>Clean flags</returns>
        public static Dictionary<FlagType, bool> CreateCleanFlags()
        {
            var flags = new Dictionary<FlagType, bool>();
            Array values = FlagType.GetValues(typeof(FlagType));
            foreach (FlagType val in values)
            {
                flags.Add(val, false);
            }
            return flags;
        }

        /// <summary>
        /// Create new dictionary, with all dirty flags 
        /// </summary>
        /// <returns>Dirt flags</returns>
        public static Dictionary<FlagType, bool> CreateDirtyFlags()
        {
            var flags = new Dictionary<FlagType, bool>();
            Array values = FlagType.GetValues(typeof(FlagType));
            foreach (FlagType val in values)
            {
                flags.Add(val, true);
            }
            return flags;
        }

        /// <summary>
        /// Create new instance of Flag, without any dirty flags
        /// </summary>
        public Flags()
        {
            dirtyFlags = CreateCleanFlags();
        }

        /// <summary>
        /// Create new instance of Flag, dirty flags are set from argument
        /// </summary>
        /// <param name="dirtyFlags">dirty flags</param>
        public Flags(Dictionary<FlagType, bool> dirtyFlags)
        {
            this.dirtyFlags = dirtyFlags;
        }

        /// <summary>
        /// Idicates if this flags contains dirty flag
        /// </summary>
        /// <param name="dirty">DirtyType</param>
        /// <returns>true if flags contains dirty flag</returns>
        public bool isDirty(FlagType dirty)
        {
            return dirtyFlags[dirty];
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override bool equals(InfoDataBase other)
        {
            return getHashCode() == (other as Flags).getHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is InfoDataBase)
                return equals(obj as InfoDataBase);
            else
                return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return getHashCode();
        }

    }

}
