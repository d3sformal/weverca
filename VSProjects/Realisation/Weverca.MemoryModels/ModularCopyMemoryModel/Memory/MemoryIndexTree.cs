using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Memory
{
    public class MemoryIndexTree : MemoryIndexVisitor, ICollection<MemoryIndex>
    {
        HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();

        Dictionary<int, MemoryIndexTreeStackContext> memoryStack = new Dictionary<int, MemoryIndexTreeStackContext>();
        Dictionary<ObjectValue, MemoryIndexTreeNode> objectTreeRoots = new Dictionary<ObjectValue,MemoryIndexTreeNode>();

        public IEnumerable<KeyValuePair<int, MemoryIndexTreeStackContext>> MemoryStack { get { return memoryStack; } }
        public IEnumerable<KeyValuePair<ObjectValue, MemoryIndexTreeNode>> ObjectTreeRoots { get { return objectTreeRoots; } }

        public MemoryIndexTree()
        {

        }

        public void Add(MemoryIndex index)
        {
            index.Accept(this);
            indexes.Add(index);
        }

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

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(MemoryIndex item)
        {
            return indexes.Contains(item);
        }

        public void CopyTo(MemoryIndex[] array, int arrayIndex)
        {
            indexes.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return indexes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(MemoryIndex item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<MemoryIndex> GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indexes.GetEnumerator();
        }


        public void VisitObjectIndex(ObjectIndex index)
        {
            MemoryIndexTreeNode root = null;
            if (!objectTreeRoots.TryGetValue(index.Object, out root))
            {
                root = new MemoryIndexTreeNode();
                objectTreeRoots[index.Object] = root;
            }

            addToRoot(index, root);
        }

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

        public void VisitTemporaryIndex(TemporaryIndex index)
        {
            MemoryIndexTreeStackContext stackContext = getStackLevel(index);
            addToRoot(index, stackContext.TemporaryTreeRoot);
        }

        public void VisitControlIndex(ControlIndex index)
        {
            MemoryIndexTreeStackContext stackContext = getStackLevel(index);
            addToRoot(index, stackContext.ControlsTreeRoot);
        }
    }

    public class MemoryIndexTreeStackContext
    {
        public MemoryIndexTreeNode VariablesTreeRoot { get; private set; }
        public MemoryIndexTreeNode ControlsTreeRoot { get; private set; }
        public MemoryIndexTreeNode TemporaryTreeRoot { get; private set; }

        public MemoryIndexTreeStackContext()
        {
            VariablesTreeRoot = new MemoryIndexTreeNode();
            ControlsTreeRoot = new MemoryIndexTreeNode();
            TemporaryTreeRoot = new MemoryIndexTreeNode();
        }
    }

    public class MemoryIndexTreeNode
    {
        public MemoryIndex Index { get; set; }
        public MemoryIndexTreeNode ParentNode { get; set; }

        public MemoryIndexTreeNode AnyChild { get; set; }
        public Dictionary<string, MemoryIndexTreeNode> ChildNodes { get; private set; }

        public MemoryIndexTreeNode(MemoryIndexTreeNode parent = null, MemoryIndex index = null)
        {
            Index = index;
            ParentNode = parent;
            AnyChild = null;
            ChildNodes = new Dictionary<string, MemoryIndexTreeNode>();
        }

        public MemoryIndexTreeNode GetOrCreateAny()
        {
            if (AnyChild == null)
            {
                AnyChild = new MemoryIndexTreeNode(this);
            }
            return AnyChild;
        }

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
