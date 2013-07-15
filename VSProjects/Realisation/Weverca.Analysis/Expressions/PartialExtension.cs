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
    public class PartialExtension
    {
        private Dictionary<LangElement, ProgramPointGraph> _branches = new Dictionary<LangElement, ProgramPointGraph>();
        private Dictionary<ProgramPointGraph, FlowOutputSet> _inputs = new Dictionary<ProgramPointGraph, FlowOutputSet>();

        internal readonly AbstractSnapshot ExtensionInput;
        internal readonly AbstractSnapshot ExtensionOutput;

        public ISnapshotReadonly Input { get { return ExtensionInput; } }
        public ISnapshotReadonly Output { get { return ExtensionOutput; } }

        public bool IsEmpty { get { return _branches.Count == 0; } }

        internal PartialExtension(AbstractSnapshot input, AbstractSnapshot output)
        {
            ExtensionInput = input;
            ExtensionOutput = output;
        }

        /// <summary>
        /// Keys registering extension branches
        /// </summary>
        public IEnumerable<LangElement> BranchingKeys { get { return _branches.Keys; } }
        /// <summary>
        /// All registered branches
        /// </summary>
        public IEnumerable<ProgramPointGraph> Branches { get { return _branches.Values; } }

        public ProgramPointGraph GetBranch(LangElement key)
        {
            ProgramPointGraph result;
            _branches.TryGetValue(key, out result);

            return result;
        }

        internal void AddBranch(LangElement key, ProgramPointGraph branch, FlowOutputSet input)
        {
            _branches.Add(key, branch);
            _inputs.Add(branch, input);
            branch.AddExtension(this);
        }

        internal void RemoveBranch(LangElement key)
        {
            var branch = GetBranch(key);
            _branches.Remove(key);
            _inputs.Remove(branch);
            branch.RemoveExtension(this);
        }

        internal FlowOutputSet GetInput(ProgramPointGraph branch)
        {
            FlowOutputSet input;
            _inputs.TryGetValue(branch, out input);
            return input;
        }
    }
}
