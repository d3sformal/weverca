using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{    
    /// <summary>
    /// Index to the memory structure
    /// Provides linkink in structure and avoids modification of pointing objecs on change of targeting
    /// </summary>
    class MemoryIndex
    {
        public ReadOnlyCollection<IndexSegment> MemoryPath {get; private set; }
        public int Length { get { return MemoryPath.Count; } }

        #region Index Factories

        public static MemoryIndex MakeIndexAnyVariable()
        {
            return new MemoryIndex(new IndexSegment(PathType.Variable));
        }

        public static MemoryIndex MakeIndexVariable(string name)
        {
            return new MemoryIndex(new IndexSegment(name, PathType.Variable));
        }

        public static MemoryIndex MakeIndexAnyField(MemoryIndex parentIndex)
        {
            return new MemoryIndex(parentIndex, new IndexSegment(PathType.Field));
        }

        public static MemoryIndex MakeIndexField(MemoryIndex parentIndex, string name)
        {
            return new MemoryIndex(parentIndex, new IndexSegment(name, PathType.Field));
        }

        public static MemoryIndex MakeIndexAnyIndex(MemoryIndex parentIndex)
        {
            return new MemoryIndex(parentIndex, new IndexSegment(PathType.Index));
        }

        public static MemoryIndex MakeIndexIndex(MemoryIndex parentIndex, string name)
        {
            return new MemoryIndex(parentIndex, new IndexSegment(name, PathType.Index));
        }

        #endregion

        private MemoryIndex(IndexSegment pathName)
        {
            List<IndexSegment> path = new List<IndexSegment>();
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        private MemoryIndex(MemoryIndex parentIndex, IndexSegment pathName)
        {
            List<IndexSegment> path = new List<IndexSegment>(parentIndex.MemoryPath);
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            MemoryIndex otherIndex = obj as MemoryIndex;

            if (otherIndex == null || otherIndex.Length != this.Length)
            {
                return false;
            }

            for (int x = this.Length - 1; x >= 0; x--)
            {
                if (! this.MemoryPath[x].Equals(otherIndex.MemoryPath[x]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach (IndexSegment name in MemoryPath)
            {
                uint val = (uint)(hashcode ^ name.GetHashCode());

                //Left bit rotation - there is no built implementation in C#
                hashcode = (int)((val << 1) | (val >> (32 - 1)));
            }

            return hashcode;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (IndexSegment name in MemoryPath)
            {
                builder.Append(name.ToString());
            }

            return builder.ToString();
        }
    }

    class IndexSegment
    {
        public static string UNDEFINED_STR = "?";

        public string Name { get; private set; }
        public PathType Type { get; private set; }
        public bool IsAny { get; private set; }

        public IndexSegment(String name, PathType type)
        {
            Name = name;
            Type = type;
            IsAny = false;
        }

        public IndexSegment(PathType type)
        {
            Name = UNDEFINED_STR;
            Type = type;
            IsAny = true;
        }

        public override bool Equals(object obj)
        {
            IndexSegment otherPath = obj as IndexSegment;

            if (otherPath == null || otherPath.Type != this.Type && otherPath.IsAny != this.IsAny)
            {
                return false;
            }

            if (!IsAny)
            {
                return otherPath.Name.Equals(this.Name);
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (IsAny)
            {
                return Type.GetHashCode();
            }
            else
            {
                return Type.GetHashCode() | Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case PathType.Index:
                    return String.Format("[{0}]", Name);

                case PathType.Field:
                    return String.Format("->{0}", Name);

                default:
                    return Name;
            }
        }
    }
}
