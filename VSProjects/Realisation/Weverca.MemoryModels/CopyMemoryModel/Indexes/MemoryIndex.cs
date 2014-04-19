using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Represents index to memory location in the structure of copy memory model.
    /// 
    /// Instances of this class are the key part of memory model. Their ussage is to provide basic targeting between snapshot
    /// interface and memory representation and also to link distinct memory locations to support aliasing, indexing and object fields.
    /// Indexes are also used for merge algorithms in order to easyly map memory locations between different snapshots.
    /// 
    /// Every memory location contains root informations and index path. These informations with the type of index object are used for
    /// test the equality of MemoryIndexes to determine whether to instances points to the same memory location.
    /// 
    /// * Root informations are defined in each implementation and describes where to begin sequence of memory lookup (e.g. variable
    /// name or id of temporary location). 
    /// 
    /// * Index path contains sequence of indexes of PHP associative array which can be accesed traversing the array memory tree from the root.
    /// Each index can be single string for known memory location or any index when no string is set.
    /// 
    /// There are several classes which are derived from this abstract class which is used to distinguish between different types of memory location
    /// (as variables, controlls, temporary locations, and objecs). 
    /// 
    /// This class and all children are imutable classes and cannot be changed.
    /// </summary>
    public abstract class MemoryIndex
    {
        /// <summary>
        /// Gets the collection of indexes in array path.
        /// </summary>
        /// <value>
        /// The memory path.
        /// </value>
        public ReadOnlyCollection<IndexSegment> MemoryPath { get; private set; }


        /// <summary>
        /// Gets the length of memory path.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get { return MemoryPath.Count; } }


        /// <summary>
        /// Gets the call level of memory index which defines the deph of the call stack.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        public int CallLevel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndex"/> class with empty path.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        protected MemoryIndex(int callLevel)
        {
            CallLevel = callLevel;
            List<IndexSegment> path = new List<IndexSegment>();
            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndex"/> class with single index in memory path.
        /// </summary>
        /// <param name="indexSegment">First index in memory path.</param>
        /// <param name="callLevel">The call level.</param>
        protected MemoryIndex(IndexSegment indexSegment, int callLevel)
        {
            CallLevel = callLevel;
            List<IndexSegment> path = new List<IndexSegment>();
            path.Add(indexSegment);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndex"/> class from the given parent index and
        /// adds new index segment at the end of the memory path.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="indexSegment">The index segment.</param>
        protected MemoryIndex(MemoryIndex parentIndex, IndexSegment indexSegment)
        {
            CallLevel = parentIndex.CallLevel;
            List<IndexSegment> path = new List<IndexSegment>(parentIndex.MemoryPath);
            path.Add(indexSegment);

            MemoryPath = new ReadOnlyCollection<IndexSegment>(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndex"/> class. Path is constructed from the
        /// given list of index segments.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callLevel">The call level.</param>
        protected MemoryIndex(IEnumerable<IndexSegment> path, int callLevel)
        {
            CallLevel = callLevel;
            MemoryPath = new ReadOnlyCollection<IndexSegment>(path.ToArray());
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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
                if (!this.MemoryPath[x].Equals(otherIndex.MemoryPath[x]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (IndexSegment name in MemoryPath)
            {
                builder.Append(name.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the call level prefix which can be used in to string methods of child class.
        /// </summary>
        /// <value>
        /// The call level prefix.
        /// </value>
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

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>Newly created memory index with the same buth but the last is ANY.</returns>
        public abstract MemoryIndex ToAny();

        /// <summary>
        /// Creates new path based on path of this object where the last index is any.
        /// Can be used in implementation of ToAny method.
        /// </summary>
        /// <returns>Path based on path of this object where the last index is any.</returns>
        public List<IndexSegment> ListToAny()
        {
            List<IndexSegment> list = new List<IndexSegment>(MemoryPath);
            if (list.Count > 0)
            {
                list[list.Count - 1] = new IndexSegment();
            }
            return list;
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>Newly created unknown root index with no index path.</returns>
        public abstract MemoryIndex CreateUnknownIndex();

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>>Newly created root index with given name and no index path</returns>
        public abstract MemoryIndex CreateIndex(string name);


        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        public abstract MemoryIndex CreateWithCallLevel(int callLevel);

        /// <summary>
        /// Determines whether this index is part of acces path of the other index.
        /// </summary>
        /// <param name="otherIndex">Index of the other.</param>
        /// <returns>True whether this index is prefix of the given one.</returns>
        internal virtual bool IsPrefixOf(MemoryIndex otherIndex)
        {
            if (otherIndex == null || otherIndex.Length < this.Length)
            {
                return false;
            }

            if (otherIndex.GetType() != this.GetType() || otherIndex.CallLevel != this.CallLevel)
            {
                return false;
            }

            for (int x = this.Length - 1; x >= 0; x--)
            {
                if (!this.MemoryPath[x].Equals(otherIndex.MemoryPath[x]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified collection of indexes contains prefix of this index.
        /// </summary>
        /// <param name="indexes">The collection of indexes.</param>
        /// <returns>True if the specified collection of indexes contains the prefix; otherwise false.</returns>
        internal bool ContainsPrefix(IEnumerable<MemoryIndex> indexes)
        {
            foreach (MemoryIndex index in indexes)
            {
                if (index.IsPrefixOf(this))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the indexes from given collection with this index as prefix.
        /// </summary>
        /// <param name="indexes">The collection of indexes to remove from.</param>
        internal void RemoveIndexesWithPrefix(ICollection<MemoryIndex> indexes)
        {
            List<MemoryIndex> toRemoveList = new List<MemoryIndex>();
            foreach (MemoryIndex index in indexes)
            {
                if (this.IsPrefixOf(index))
                {
                    toRemoveList.Add(index);
                }
            }

            foreach (MemoryIndex toRemove in toRemoveList)
            {
                indexes.Remove(toRemove);
            }
        }
    }

    /// <summary>
    /// Specifies the root informations for memory indexes which starts in single variable
    /// with specified name.
    /// </summary>
    public abstract class NamedIndex : MemoryIndex
    {
        /// <summary>
        /// Gets the root index with empty index path
        /// </summary>
        /// <value>
        /// The memory root.
        /// </value>
        public IndexSegment MemoryRoot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIndex"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="callLevel">The call level.</param>
        protected NamedIndex(IndexSegment root, int callLevel)
            : base(callLevel)
        {
            MemoryRoot = root;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="pathName">Name of the path.</param>
        protected NamedIndex(NamedIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
            MemoryRoot = parentIndex.MemoryRoot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="path">The path.</param>
        protected NamedIndex(NamedIndex parentIndex, List<IndexSegment> path)
            : base(path, parentIndex.CallLevel)
        {
            MemoryRoot = parentIndex.MemoryRoot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="callLevel">The call level.</param>
        protected NamedIndex(NamedIndex parentIndex, int callLevel)
            : base(parentIndex.MemoryPath, callLevel)
        {
            MemoryRoot = parentIndex.MemoryRoot;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ MemoryRoot.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Determines whether this index is part of acces path of the other index.
        /// </summary>
        /// <param name="otherIndex">Index of the other.</param>
        /// <returns>
        /// True whether this index is prefix of the given one.
        /// </returns>
        internal override bool IsPrefixOf(MemoryIndex otherIndex)
        {
            NamedIndex namedIndex = otherIndex as NamedIndex;
            if (namedIndex != null)
            {
                return MemoryRoot.Equals(namedIndex.MemoryRoot) && base.IsPrefixOf(otherIndex);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>
        /// Newly created memory index with the same buth but the last is ANY.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override MemoryIndex ToAny()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>
        /// Newly created unknown root index with no index path.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override MemoryIndex CreateUnknownIndex()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// &gt;Newly created root index with given name and no index path
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override MemoryIndex CreateIndex(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override MemoryIndex CreateWithCallLevel(int callLevel)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Identifies memory locations rooted in the PHP object instance. Every root index represents one of
    /// field (named or any) associated with the object with given ObjectValue or associative array
    /// rooted in some field of object.
    /// 
    /// Every object index is located at global call level in memory stack.
    /// </summary>
    public class ObjectIndex : NamedIndex
    {
        /// <summary>
        /// Gets the object this index is rooted in.
        /// </summary>
        /// <value>
        /// The object.
        /// </value>
        public ObjectValue Object { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIndex"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="pathName">Name of the path.</param>
        public ObjectIndex(ObjectValue obj, IndexSegment pathName)
            : base(pathName, Snapshot.GLOBAL_CALL_LEVEL)
        {
            Object = obj;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="pathName">Name of the path.</param>
        public ObjectIndex(ObjectIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
            Object = parentIndex.Object;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="path">The path.</param>
        public ObjectIndex(ObjectIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
            Object = parentIndex.Object;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="callLevel">The call level.</param>
        public ObjectIndex(ObjectIndex parentIndex, int callLevel)
            : base(parentIndex, callLevel)
        {
            Object = parentIndex.Object;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Object.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Determines whether this index is part of acces path of the other index.
        /// </summary>
        /// <param name="otherIndex">Index of the other.</param>
        /// <returns>
        /// True whether this index is prefix of the given one.
        /// </returns>
        internal override bool IsPrefixOf(MemoryIndex otherIndex)
        {
            ObjectIndex objIndex = otherIndex as ObjectIndex;
            if (objIndex != null)
            {
                return Object.Equals(objIndex.Object) && base.IsPrefixOf(otherIndex);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>
        /// Newly created memory index with the same buth but the last is ANY.
        /// </returns>
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{3}{2}->{0}{1}", MemoryRoot.Name, base.ToString(), Object.UID, CallLevelPrefix);
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>New unknown root index with no index path.</returns>
        public static MemoryIndex CreateUnknown(ObjectValue obj)
        {
            return new ObjectIndex(obj, new IndexSegment());
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="name">The name.</param>
        /// <returns>New root index with given name and no index path.</returns>
        public static MemoryIndex Create(ObjectValue obj, string name)
        {
            return new ObjectIndex(obj, new IndexSegment(name));
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>
        /// Newly created unknown root index with no index path.
        /// </returns>
        public override MemoryIndex CreateUnknownIndex()
        {
            return new ObjectIndex(this, new IndexSegment());
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// &gt;Newly created root index with given name and no index path
        /// </returns>
        public override MemoryIndex CreateIndex(string name)
        {
            return new ObjectIndex(this, new IndexSegment(name));
        }

        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        public override MemoryIndex CreateWithCallLevel(int callLevel)
        {
            return new ObjectIndex(this, callLevel);
        }
    }

    /// <summary>
    /// Identifies memory locations rooted as local or global PHP variable. Every variable has string name
    /// (or any), cal level and array path where the data can be located.
    /// 
    /// This is the most common type of memory index which can be used to represent PHP memory space 
    /// withou object. Objects then can be stored as values in the variable location and their data
    /// accessed using object memory index instance.
    /// </summary>
    public class VariableIndex : NamedIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableIndex"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="callLevel">The call level.</param>
        public VariableIndex(IndexSegment root, int callLevel)
            : base(root, callLevel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="pathName">Name of the path.</param>
        public VariableIndex(VariableIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="path">The path.</param>
        public VariableIndex(VariableIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="callLevel">The call level.</param>
        public VariableIndex(VariableIndex parentIndex, int callLevel)
            : base(parentIndex, callLevel)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{2}${0}{1}", MemoryRoot.Name, base.ToString(), CallLevelPrefix);
        }

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>
        /// Newly created memory index with the same buth but the last is ANY.
        /// </returns>
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

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New unknown root index with no index path.</returns>
        public static MemoryIndex CreateUnknown(int callLevel)
        {
            return new VariableIndex(new IndexSegment(), callLevel);
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New root index with given name and no index path.</returns>
        public static MemoryIndex Create(string name, int callLevel)
        {
            return new VariableIndex(new IndexSegment(name), callLevel);
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>
        /// Newly created unknown root index with no index path.
        /// </returns>
        public override MemoryIndex CreateUnknownIndex()
        {
            return new VariableIndex(this, new IndexSegment());
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// &gt;Newly created root index with given name and no index path
        /// </returns>
        public override MemoryIndex CreateIndex(string name)
        {
            return new VariableIndex(this, new IndexSegment(name));
        }

        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        public override MemoryIndex CreateWithCallLevel(int callLevel)
        {
            return new VariableIndex(this, callLevel);
        }
    }

    /// <summary>
    /// Identifies temporary location. Every newly created index has unique ID which identifies it and
    /// can contain index path as ordinary variables. In snapshot is not possible to merge two different
    /// indexes which was created in different snapshots.
    /// 
    /// Temporary locations are used to make deep copy of transfered data to another location
    /// to prevent change of data and cycle dependence creation when the write operation access
    /// memory index which is prefix of source indexes.
    /// 
    /// Temporary locations also stores other memory which is not assigned to any location but which
    /// has to be shared between several snapshots (e.g. anonymus arrays, foreach arguments, ...).
    /// </summary>
    public class TemporaryIndex : MemoryIndex
    {
        /// <summary>
        /// Temporary index identifier counter.
        /// Newly created index stores actual value an increments this variable for another one.
        /// </summary>
        private static int GLOBAL_ROOT_ID = 0;

        /// <summary>
        /// The root unique identifier
        /// </summary>
        private int rootId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryIndex"/> class.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        public TemporaryIndex(int callLevel)
            : base(callLevel)
        {
            rootId = GLOBAL_ROOT_ID;
            GLOBAL_ROOT_ID++;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="pathName">Name of the path.</param>
        public TemporaryIndex(TemporaryIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
            rootId = parentIndex.rootId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="path">The path.</param>
        public TemporaryIndex(TemporaryIndex parentIndex, List<IndexSegment> path)
            : base(path, parentIndex.CallLevel)
        {
            rootId = parentIndex.rootId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="callLevel">The call level.</param>
        public TemporaryIndex(TemporaryIndex parentIndex, int callLevel)
            : base(parentIndex.MemoryPath, callLevel)
        {
            rootId = parentIndex.rootId;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ rootId.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Determines whether this index is part of acces path of the other index.
        /// </summary>
        /// <param name="otherIndex">Index of the other.</param>
        /// <returns>
        /// True whether this index is prefix of the given one.
        /// </returns>
        internal override bool IsPrefixOf(MemoryIndex otherIndex)
        {
            TemporaryIndex tempIndex = otherIndex as TemporaryIndex;
            if (tempIndex != null)
            {
                return rootId.Equals(tempIndex.rootId) && base.IsPrefixOf(otherIndex);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("TEMP--{2}${0}{1}", rootId, base.ToString(), CallLevelPrefix);
        }

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>
        /// Newly created memory index with the same buth but the last is ANY.
        /// </returns>
        /// <exception cref="System.Exception">Undefined ANY variable in temporary collection</exception>
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

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>
        /// Newly created unknown root index with no index path.
        /// </returns>
        public override MemoryIndex CreateUnknownIndex()
        {
            return new TemporaryIndex(this, new IndexSegment());
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// &gt;Newly created root index with given name and no index path
        /// </returns>
        public override MemoryIndex CreateIndex(string name)
        {
            return new TemporaryIndex(this, new IndexSegment(name));
        }

        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        public override MemoryIndex CreateWithCallLevel(int callLevel)
        {
            return new TemporaryIndex(this, callLevel);
        }
    }

    /// <summary>
    /// Similar to VariableIndex to identify memory location which is stored as analysis controll variable.
    /// </summary>
    public class ControlIndex : NamedIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlIndex"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="callLevel">The call level.</param>
        public ControlIndex(IndexSegment root, int callLevel)
            : base(root, callLevel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="pathName">Name of the path.</param>
        public ControlIndex(ControlIndex parentIndex, IndexSegment pathName)
            : base(parentIndex, pathName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="path">The path.</param>
        public ControlIndex(ControlIndex parentIndex, List<IndexSegment> path)
            : base(parentIndex, path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlIndex"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="callLevel">The call level.</param>
        public ControlIndex(ControlIndex parentIndex, int callLevel)
            : base(parentIndex, callLevel)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("CTRL--{2}${0}{1}", MemoryRoot.Name, base.ToString(), CallLevelPrefix);
        }

        /// <summary>
        /// Converts this memory index that at the end of acces path is any segment.
        /// When the path is empty, root informations are converted into any.
        /// </summary>
        /// <returns>
        /// Newly created memory index with the same buth but the last is ANY.
        /// </returns>
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

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New unknown root index with no index path.</returns>
        public static MemoryIndex CreateUnknown(int callLevel)
        {
            return new ControlIndex(new IndexSegment(), callLevel);
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New root index with given name and no index path.</returns>
        public static MemoryIndex Create(string name, int callLevel)
        {
            return new ControlIndex(new IndexSegment(name), callLevel);
        }

        /// <summary>
        /// Creates the unknown root index with no index path.
        /// </summary>
        /// <returns>
        /// Newly created unknown root index with no index path.
        /// </returns>
        public override MemoryIndex CreateUnknownIndex()
        {
            return new ControlIndex(this, new IndexSegment());
        }

        /// <summary>
        /// Creates the root index with given name and no index path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// &gt;Newly created root index with given name and no index path
        /// </returns>
        public override MemoryIndex CreateIndex(string name)
        {
            return new ControlIndex(this, new IndexSegment(name));
        }

        /// <summary>
        /// Creates new index with the same path and specified call level.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        /// <returns>
        /// Newly created memory index with the same path and specified call level.
        /// </returns>
        public override MemoryIndex CreateWithCallLevel(int callLevel)
        {
            return new ControlIndex(this, callLevel);
        }
    }

    /// <summary>
    /// Represents single index in memory location acces path. Contains name of index or whether this is
    /// undefined index.
    /// </summary>
    public class IndexSegment
    {
        /// <summary>
        /// The label of undefined index.
        /// </summary>
        public static string UNDEFINED_STR = "?";

        /// <summary>
        /// Gets the name of index.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }


        /// <summary>
        /// Gets a value indicating whether index is any.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is any; otherwise, <c>false</c>.
        /// </value>
        public bool IsAny { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSegment"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public IndexSegment(String name)
        {
            Name = name;
            IsAny = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSegment"/> class. New instance represents any index.
        /// </summary>
        public IndexSegment()
        {
            Name = UNDEFINED_STR;
            IsAny = true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("[{0}]", Name);
        }
    }
}
