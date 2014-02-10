using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{

    public enum RootIndexType
    {
        Variable, Object, Temporary
    }

    /// <summary>
    /// Index to the memory structure
    /// Provides linkink in structure and avoids modification of pointing objecs on change of targeting
    /// </summary>
    public abstract class MemoryIndex
    {
        public ReadOnlyCollection<IndexSegment> MemoryPath {get; private set; }
        public int Length { get { return MemoryPath.Count; } }
        public int CallLevel { get; private set; }

        protected MemoryIndex(int callLevel)
        {
            CallLevel = callLevel;
            List<IndexSegment> path = new List<IndexSegment>();
            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        protected MemoryIndex(IndexSegment pathName, int callLevel)
        {
            CallLevel = callLevel;
            List<IndexSegment> path = new List<IndexSegment>();
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        protected MemoryIndex(MemoryIndex parentIndex, IndexSegment pathName)
        {
            CallLevel = parentIndex.CallLevel;
            List<IndexSegment> path = new List<IndexSegment>(parentIndex.MemoryPath);
            path.Add(pathName);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        protected MemoryIndex(List<IndexSegment> path, int callLevel)
        {
            CallLevel = callLevel;
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

            if (otherIndex.GetType() != this.GetType() || otherIndex.CallLevel != this.CallLevel)
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
            int hashcode = this.GetType().GetHashCode() + CallLevel;
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

        public string CallLevelPrefix
        {
            get
            {
                if (CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
                {
                    return CallLevel + ".";
                }
                else
                {
                    return "";
                }
            }
        }

        public abstract MemoryIndex ToAny();

        public List<IndexSegment> ListToAny()
        {
            List<IndexSegment> list = new List<IndexSegment>(MemoryPath);
            if (list.Count > 0)
            {
                list[list.Count - 1] = new IndexSegment();
            }
            return list;
        }

        public abstract MemoryIndex CreateUnknownIndex();
        public abstract MemoryIndex CreateIndex(string name);
    }

    public abstract class NamedIndex : MemoryIndex
    {
        public IndexSegment MemoryRoot { get; private set; }

        protected NamedIndex(IndexSegment root, int callLevel)
            : base(callLevel)
        {
            MemoryRoot = root;
        }

        protected NamedIndex(NamedIndex parentIndex, IndexSegment pathName) : base(parentIndex, pathName)
        {
            MemoryRoot = parentIndex.MemoryRoot;
        }

        protected NamedIndex(NamedIndex parentIndex, List<IndexSegment> path)
            : base(path, parentIndex.CallLevel)
        {
            MemoryRoot = parentIndex.MemoryRoot;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ MemoryRoot.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NamedIndex otherIndex = obj as NamedIndex;
            if (otherIndex != null)
            {
                return MemoryRoot.Equals(otherIndex.MemoryRoot) && base.Equals(obj);
            }
            else
            {
                return false;
            }
        }
    }

    public class ObjectIndex : NamedIndex
    {
        public ObjectValue Object { get; private set; }

        public ObjectIndex(ObjectValue obj, IndexSegment pathName)
            : base(pathName, Snapshot.GLOBAL_CALL_LEVEL)
        {
            Object = obj;
        }

        public ObjectIndex(ObjectIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
            Object = parentIndex.Object;
        }

        public ObjectIndex(ObjectIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
            Object = parentIndex.Object;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Object.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ObjectIndex otherIndex = obj as ObjectIndex;
            if (otherIndex != null)
            {
                return Object.Equals(otherIndex.Object) && base.Equals(obj);
            }
            else
            {
                return false;
            }
        }

        public override MemoryIndex ToAny()
        {
            if (MemoryPath.Count == 0)
            {
                return new ObjectIndex(Object, new IndexSegment());
            }
            else
            {
                return new ObjectIndex(this, ListToAny());
            }
        }

        public override string ToString()
        {
            return String.Format("{3}{2}->{0}{1}", MemoryRoot.Name, base.ToString(), Object.UID, CallLevelPrefix);
        }

        public static MemoryIndex CreateUnknown(ObjectValue obj)
        {
            return new ObjectIndex(obj, new IndexSegment());
        }

        public static MemoryIndex Create(ObjectValue obj, string name)
        {
            return new ObjectIndex(obj, new IndexSegment(name));
        }

        public override MemoryIndex CreateUnknownIndex()
        {
            return new ObjectIndex(this, new IndexSegment());
        }

        public override MemoryIndex CreateIndex(string name)
        {
            return new ObjectIndex(this, new IndexSegment(name));
        }
    }

    public class VariableIndex : NamedIndex
    {
        public VariableIndex(IndexSegment root, int callLevel)
            : base(root, callLevel)
        {
        }

        public VariableIndex(VariableIndex parentIndex, IndexSegment pathName) : base(parentIndex, pathName)
        {
        }

        public VariableIndex(VariableIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
        }

        public override string ToString()
        {
            return String.Format("{2}${0}{1}", MemoryRoot.Name, base.ToString(), CallLevelPrefix);
        }

        public override MemoryIndex ToAny()
        {
            if (MemoryPath.Count == 0)
            {
                return new VariableIndex(new IndexSegment(), this.CallLevel);
            }
            else
            {
                return new VariableIndex(this, ListToAny());
            }
        }

        public static MemoryIndex CreateUnknown(int callLevel)
        {
            return new VariableIndex(new IndexSegment(), callLevel);
        }

        public static MemoryIndex Create(string name, int callLevel)
        {
            return new VariableIndex(new IndexSegment(name), callLevel);
        }

        public override MemoryIndex CreateUnknownIndex()
        {
            return new VariableIndex(this, new IndexSegment());
        }

        public override MemoryIndex CreateIndex(string name)
        {
            return new VariableIndex(this, new IndexSegment(name));
        }
    }

    public class TemporaryIndex : MemoryIndex
    {
        private static int GLOBAL_ROOT_ID = 0;

        private int rootId;

        public TemporaryIndex(int callLevel)
            : base(callLevel)
        {
            rootId = GLOBAL_ROOT_ID;
            GLOBAL_ROOT_ID++;
        }

        public TemporaryIndex(TemporaryIndex parentIndex, IndexSegment pathName) : base(parentIndex, pathName)
        {
            rootId = parentIndex.rootId;
        }

        public TemporaryIndex(TemporaryIndex parentIndex, List<IndexSegment> path)
            : base(path, parentIndex.CallLevel)
        {
            rootId = parentIndex.rootId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ rootId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TemporaryIndex otherIndex = obj as TemporaryIndex;
            if (otherIndex != null)
            {
                return rootId.Equals(otherIndex.rootId) && base.Equals(obj);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return String.Format("TEMP--{2}${0}{1}", rootId, base.ToString(), CallLevelPrefix);
        }

        public override MemoryIndex ToAny()
        {
            if (MemoryPath.Count == 0)
            {
                throw new Exception("Undefined ANY variable in temporary collection");
            }
            else
            {
                return new TemporaryIndex(this, ListToAny());
            }
        }

        public override MemoryIndex CreateUnknownIndex()
        {
            return new TemporaryIndex(this, new IndexSegment());
        }

        public override MemoryIndex CreateIndex(string name)
        {
            return new TemporaryIndex(this, new IndexSegment(name));
        }
    }

    public class ControlIndex : NamedIndex
    {
        public ControlIndex(IndexSegment root, int callLevel)
            : base(root, callLevel)
        {
        }

        public ControlIndex(ControlIndex parentIndex, IndexSegment pathName) : base(parentIndex, pathName)
        {
        }

        public ControlIndex(ControlIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
        }

        public override string ToString()
        {
            return String.Format("CTRL--{2}${0}{1}", MemoryRoot.Name, base.ToString(), CallLevelPrefix);
        }

        public override MemoryIndex ToAny()
        {
            if (MemoryPath.Count == 0)
            {
                return new ControlIndex(new IndexSegment(), this.CallLevel);
            }
            else
            {
                return new ControlIndex(this, ListToAny());
            }
        }

        public static MemoryIndex CreateUnknown(int callLevel)
        {
            return new ControlIndex(new IndexSegment(), callLevel);
        }

        public static MemoryIndex Create(string name, int callLevel)
        {
            return new ControlIndex(new IndexSegment(name), callLevel);
        }

        public override MemoryIndex CreateUnknownIndex()
        {
            return new ControlIndex(this, new IndexSegment());
        }

        public override MemoryIndex CreateIndex(string name)
        {
            return new ControlIndex(this, new IndexSegment(name));
        }
    }

    public class IndexSegment
    {
        public static string UNDEFINED_STR = "?";

        public string Name { get; private set; }
        public bool IsAny { get; private set; }

        public IndexSegment(String name)
        {
            Name = name;
            IsAny = false;
        }

        public IndexSegment()
        {
            Name = UNDEFINED_STR;
            IsAny = true;
        }

        public override bool Equals(object obj)
        {
            IndexSegment otherPath = obj as IndexSegment;

            if (otherPath == null || otherPath.IsAny != this.IsAny)
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
                return 0;
            }
            else
            {
                return Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}]", Name);
        }
    }
}
