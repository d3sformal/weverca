using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Represents extension of program point graph for single partial (part of statement)
    /// NOTE:
    ///     Program point graph extending is caused because of call invocation or include
    /// </summary>
    public class PartialExtension<Key>
    {
        private Dictionary<Key, ProgramPointGraph> _branches = new Dictionary<Key, ProgramPointGraph>();
        private Dictionary<ProgramPointGraph, FlowOutputSet> _inputs = new Dictionary<ProgramPointGraph, FlowOutputSet>();

        internal readonly SnapshotBase ExtensionInput;
        internal readonly SnapshotBase ExtensionOutput;

        public ISnapshotReadonly Input { get { return ExtensionInput; } }
        public ISnapshotReadonly Output { get { return ExtensionOutput; } }

        public bool IsEmpty { get { return _branches.Count == 0; } }

        internal PartialExtension(SnapshotBase input, SnapshotBase output)
        {
            ExtensionInput = input;
            ExtensionOutput = output;
        }

        /// <summary>
        /// Keys registering extension branches
        /// </summary>
        public IEnumerable<Key> BranchingKeys { get { return _branches.Keys; } }
        /// <summary>
        /// All registered branches
        /// </summary>
        public IEnumerable<ProgramPointGraph> Branches { get { return _branches.Values; } }

        public ProgramPointGraph GetBranch(Key key)
        {
            ProgramPointGraph result;
            _branches.TryGetValue(key, out result);

            return result;
        }

        internal void AddBranch(Key key, ProgramPointGraph branch, FlowOutputSet input)
        {
            _branches.Add(key, branch);
            _inputs.Add(branch, input);            
        }

        internal void RemoveBranch(Key key)
        {
            var branch = GetBranch(key);
            _branches.Remove(key);
            _inputs.Remove(branch);            
        }

        internal FlowOutputSet GetInput(ProgramPointGraph branch)
        {
            FlowOutputSet input;
            _inputs.TryGetValue(branch, out input);
            return input;
        }
    }
}
