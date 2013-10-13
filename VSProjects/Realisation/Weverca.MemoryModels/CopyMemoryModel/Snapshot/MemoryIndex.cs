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
        public ReadOnlyCollection<PathName> MemoryPath {get; private set; }
        public int Length { get { return MemoryPath.Count; } }

        #region Index Factories

        public static MemoryIndex MakeIndexAnyVariable()
        {
            return new MemoryIndex(new PathName(PathType.Variable));
        }

        public static MemoryIndex MakeIndexVariable(string name)
        {
            return new MemoryIndex(new PathName(name, PathType.Variable));
        }

        public static MemoryIndex MakeIndexAnyField(MemoryIndex parentIndex)
        {
            return new MemoryIndex(parentIndex, new PathName(PathType.Variable));
        }

        public static MemoryIndex MakeIndexField(MemoryIndex parentIndex, string name)
        {
            return new MemoryIndex(parentIndex, new PathName(name, PathType.Variable));
        }

        public static MemoryIndex MakeIndexAnyIndex(MemoryIndex parentIndex)
        {
            return new MemoryIndex(parentIndex, new PathName(PathType.Variable));
        }

        public static MemoryIndex MakeIndexIndex(MemoryIndex parentIndex, string name)
        {
            return new MemoryIndex(parentIndex, new PathName(name, PathType.Variable));
        }

        #endregion

        private MemoryIndex(PathName pathName)
        {
            List<PathName> path = new List<PathName>();
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<PathName>(path);
        }

        private MemoryIndex(MemoryIndex parentIndex, PathName pathName)
        {
            List<PathName> path = new List<PathName>(parentIndex.MemoryPath);
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<PathName>(path);
        }

        public override bool Equals(object obj)
        {
            MemoryIndex otherIndex = obj as MemoryIndex;

            if (otherIndex == null || otherIndex.Length != this.Length)
            {
                return false;
            }

            for (int x = 0; x < this.Length; x++)
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
            foreach (PathName name in MemoryPath)
            {
                hashcode = hashcode ^ name.GetHashCode();
            }

            return hashcode;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (PathName name in MemoryPath)
            {
                builder.Append(name.ToString());
            }

            return builder.ToString();
        }
    }

    class PathName
    {
        public static string UNDEFINED_STR = "?";

        public string Name { get; private set; }
        public PathType Type { get; private set; }
        public bool IsAny { get; private set; }

        public PathName(String name, PathType type)
        {
            Name = name;
            Type = type;
            IsAny = false;
        }

        public PathName(PathType type)
        {
            Name = UNDEFINED_STR;
            Type = type;
            IsAny = true;
        }

        public override bool Equals(object obj)
        {
            PathName otherPath = obj as PathName;

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
                return Type.GetHashCode() ^ Name.GetHashCode();
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

    enum PathType
    {
        Variable, Field, Index
    }
}
