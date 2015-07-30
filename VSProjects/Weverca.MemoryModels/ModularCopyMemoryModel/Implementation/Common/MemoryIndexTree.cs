using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    /// <summary>
    /// Memory index tree contains collection of indexes and its connection to the tree structure 
    /// by their memory path. The tree is used when some operation needs to traverse the memory tree
    /// by paths specified by a given set of indexes. It prevents to traverse unnecessary parts 
    /// of memory tree. Used by merge algorithm to determine which indexes should be visited by 
    /// the algorithm.
    /// 
    /// Memory index tree builds part of memory tree from roots to the all stored indexes.
    /// </summary>
    public class MemoryIndexTree : MemoryIndexVisitor, ICollection<MemoryIndex>
    {
        private HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
        private Dictionary<int, MemoryIndexTreeStackContext> memoryStack = new Dictionary<int, MemoryIndexTreeStackContext>();
        private Dictionary<ObjectValue, MemoryIndexTreeNode> objectTreeRoots = new Dictionary<ObjectValue, MemoryIndexTreeNode>();

        /// <summary>
        /// Gets the collection of all memor stack context which contains some indexes stored
        /// within this instance.
        /// </summary>
        /// <value>
        /// The memory stack.
        /// </value>
        public IEnumerable<KeyValuePair<int, MemoryIndexTreeStackContext>> MemoryStack { get { return memoryStack; } }

        /// <summary>
        /// Gets the list of all objects which indexes are stored within this instance.
        /// </summary>
        /// <value>
        /// The object tree roots.
        /// </value>
        public IEnumerable<KeyValuePair<ObjectValue, MemoryIndexTreeNode>> ObjectTreeRoots { get { return objectTreeRoots; } }

        /// <summary>
        /// Gets the list of indexes stored with this instance of a index tree. All indexes which
        /// were added into the collection.
        /// </summary>
        /// <value>
        /// The the list of indexes stored with this instance..
        /// </value>
        public IEnumerable<MemoryIndex> StoredIndexes { get { return indexes; } }
        
        /// <inheritdoc />
        public int Count { get { return indexes.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return false; } }
        
        /// <summary>
        /// Gets the stack level for ven index. Finds existing call level or creates new one 
        /// if is missing.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The stack level for the given index.</returns>
        private MemoryIndexTreeStackContext getStackLevel(MemoryIndex index)
        {
            MemoryIndexTreeStackContext stack = null;
            if (!memoryStack.TryGetValue(index.CallLevel, out stack))
            {
                stack = new MemoryIndexTreeStackContext();
                memoryStack[index.CallLevel] = stack;
            }
            return stack;
        }

        /// <summary>
        /// Adds to root.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="root">The root.</param>
        private void addToRoot(MemoryIndex index, MemoryIndexTreeNode root)
        {
            MemoryIndexTreeNode node = root;
            foreach (IndexSegment segment in index.MemoryPath)
            {
                if (segment.IsAny)
                {
                    node = node.GetOrCreateAny();
                }
                else
                {
                    node = node.GetOrCreateChild(segment.Name);
                }
            }
            node.Index = index;
        }

        #region ICollection Impl

        /// <inheritdoc />
        public bool Contains(MemoryIndex item)
        {
            return indexes.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(MemoryIndex[] array, int arrayIndex)
        {
            indexes.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<MemoryIndex> GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(MemoryIndex index)
        {
            index.Accept(this);
            indexes.Add(index);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Method is not implemented</exception>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Method is not implemented</exception>
        public bool Remove(MemoryIndex item)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #region MemoryIndexVisitor Impl

        /// <inheritdoc />
        public void VisitObjectIndex(ObjectIndex index)
        {
            MemoryIndexTreeNode root = null;
            if (!objectTreeRoots.TryGetValue(index.Object, out root))
            {
                root = new MemoryIndexTreeNode();
                objectTreeRoots[index.Object] = root;
            }

            if (index.MemoryRoot.IsAny)
            {
                addToRoot(index, root.GetOrCreateAny());
            }
            else
            {
                addToRoot(index, root.GetOrCreateChild(index.MemoryRoot.Name));
            }

        }

        /// <inheritdoc />
        public void VisitVariableIndex(VariableIndex index)
        {
            MemoryIndexTreeStackContext stackContext = getStackLevel(index);

            if (index.MemoryRoot.IsAny)
            {
                addToRoot(index, stackContext.VariablesTreeRoot.GetOrCreateAny());
            }
            else
            {
                addToRoot(index, stackContext.VariablesTreeRoot.GetOrCreateChild(index.MemoryRoot.Name));
            }
        }

        /// <inheritdoc />
        public void VisitTemporaryIndex(TemporaryIndex index)
        {
            MemoryIndexTreeStackContext stackContext = getStackLevel(index);
            addToRoot(index, stackContext.TemporaryTreeRoot);
        }

        /// <inheritdoc />
        public void VisitControlIndex(ControlIndex index)
        {
            MemoryIndexTreeStackContext stackContext = getStackLevel(index);

            if (index.MemoryRoot.IsAny)
            {
                addToRoot(index, stackContext.ControlsTreeRoot.GetOrCreateAny());
            }
            else
            {
                addToRoot(index, stackContext.ControlsTreeRoot.GetOrCreateChild(index.MemoryRoot.Name));
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents single stack context to contain roots of all indexes stored within the memory tree.
    /// </summary>
    public class MemoryIndexTreeStackContext
    {
        /// <summary>
        /// Gets the root node for variable indexes.
        /// </summary>
        /// <value>
        /// The variables tree root.
        /// </value>
        public MemoryIndexTreeNode VariablesTreeRoot { get; private set; }

        /// <summary>
        /// Gets the root node for control variable indexes.
        /// </summary>
        /// <value>
        /// The controls tree root.
        /// </value>
        public MemoryIndexTreeNode ControlsTreeRoot { get; private set; }

        /// <summary>
        /// Gets root node for temporary variable indexes.
        /// </summary>
        /// <value>
        /// The temporary tree root.
        /// </value>
        public MemoryIndexTreeNode TemporaryTreeRoot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndexTreeStackContext"/> class.
        /// </summary>
        public MemoryIndexTreeStackContext()
        {
            VariablesTreeRoot = new MemoryIndexTreeNode();
            ControlsTreeRoot = new MemoryIndexTreeNode();
            TemporaryTreeRoot = new MemoryIndexTreeNode();
        }
    }

    /// <summary>
    /// Represents single node of the memory tree. Each node contains the next segment of
    /// a memory path from the root to some memory index stored within the tree.
    /// </summary>
    public class MemoryIndexTreeNode
    {
        /// <summary>
        /// Gets or sets the index represented by this node. Can be null if the index is not stored within
        /// the tree (in that case this node represents a segment from path).
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public MemoryIndex Index { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>
        /// The parent node.
        /// </value>
        public MemoryIndexTreeNode ParentNode { get; set; }

        /// <summary>
        /// Gets or sets the child node which continues thru the unknown branch. Can be null if tree does 
        /// not contain any index which is part of this unknown branch.
        /// </summary>
        /// <value>
        /// Any child.
        /// </value>
        public MemoryIndexTreeNode AnyChild { get; set; }


        /// <summary>
        /// Gets the collection of child nodes.
        /// </summary>
        /// <value>
        /// The collection of child nodes.
        /// </value>
        public Dictionary<string, MemoryIndexTreeNode> ChildNodes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryIndexTreeNode"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="index">The index.</param>
        public MemoryIndexTreeNode(MemoryIndexTreeNode parent = null, MemoryIndex index = null)
        {
            Index = index;
            ParentNode = parent;
            AnyChild = null;
            ChildNodes = new Dictionary<string, MemoryIndexTreeNode>();
        }

        /// <summary>
        /// Gets the or create any branch.
        /// </summary>
        /// <returns>The tree node of the any branch.</returns>
        public MemoryIndexTreeNode GetOrCreateAny()
        {
            if (AnyChild == null)
            {
                AnyChild = new MemoryIndexTreeNode(this);
            }
            return AnyChild;
        }

        /// <summary>
        /// Gets the or create child with specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Tree node which ontains the next segment identified by given name.</returns>
        public MemoryIndexTreeNode GetOrCreateChild(string name)
        {
            MemoryIndexTreeNode node = null;
            if (!ChildNodes.TryGetValue(name, out node))
            {
                node = new MemoryIndexTreeNode(this);
                ChildNodes[name] = node;
            }
            return node;
        }
    }
}
