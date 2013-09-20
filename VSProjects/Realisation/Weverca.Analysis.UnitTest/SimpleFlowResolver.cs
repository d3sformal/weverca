using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.ProgramPoints;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Controlling flow actions during analysis
    /// </summary>
    class SimpleFlowResolver : FlowResolverBase
    {
        private readonly Dictionary<string, string> _includes = new Dictionary<string, string>();

        private FlowOutputSet _outSet;
        private EvaluationLog _log;

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log)
        {
            _outSet = outSet;
            _log = log;

            bool willAssume;
            switch (condition.Form)
            {
                case ConditionForm.All:
                    willAssume = needsAll(condition.Parts);
                    break;

                default:
                    //we has to assume, because we can't disprove assumption
                    willAssume = true;
                    break;
            }

            if (willAssume)
            {
                processAssumption(condition);
            }

            return willAssume;
        }

        public override void CallDispatchMerge(FlowOutputSet callerOutput, IEnumerable<ProgramPointGraph> dispatchedProgramPointGraphs, ExtensionType callType)
        {
            var ends = (from callOutput in dispatchedProgramPointGraphs select callOutput.End.OutSet as ISnapshotReadonly).ToArray();

            switch (callType)
            {
                case ExtensionType.ParallelInclude:
                    //merging from includes behaves like usual 
                    //program points extend
                    callerOutput.Extend(ends);
                    break;

                case ExtensionType.ParallelCall:
                    //merging from calls needs special behaviour
                    //from memory model (there are no propagation of locales e.g)
                    callerOutput.MergeWithCallLevel(ends);
                    break;

                default :
                    throw new NotImplementedException();
            }
        }

        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            //extend current program point as Include
            flow.SetExtensionType(ExtensionType.ParallelInclude);

            var files = new HashSet<string>();
            foreach (StringValue possibleFile in includeFile.PossibleValues)
            {
                files.Add(possibleFile.Value);
            }

            foreach (var branchKey in flow.ExtensionKeys)
            {
                if (!files.Remove(branchKey as string))
                {
                    //this include is now not resolved as possible include branch
                    flow.RemoveExtension(branchKey);
                }
            }

            foreach (var file in files)
            {
                //Create graph for every include - NOTE: we can share pp graphs
                var cfg = AnalysisTestUtils.CreateCFG(_includes[file]);
                var ppGraph = new ProgramPointGraph(cfg.start, null);
                flow.AddExtension(file, ppGraph);
            }
        }

        /// <summary>
        /// Set code included for given file name (used for include testing)
        /// </summary>
        /// <param name="fileName">Name of included file</param>
        /// <param name="fileCode">PHP code of included file</param>
        internal void SetInclude(string fileName, string fileCode)
        {
            _includes.Add(fileName, fileCode);
        }

        #region Assumption helpers

        private bool needsAll(IEnumerable<Expressions.Postfix> conditionParts)
        {
            //we are searching for one part, that can be evaluated only as false

            foreach (var evaluatedPart in conditionParts)
            {
                var value = _log.GetValue(evaluatedPart.SourceElement);
                if (evalOnlyFalse(value))
                {
                    return false;
                }
            }

            //can disprove some part
            return true;
        }

        private bool evalOnlyFalse(MemoryEntry evaluatedPart)
        {
            if (evaluatedPart == null)
            {
                //Possible cause of this is that evaluatedPart doesn't contains expression
                //Syntax error
                throw new NotSupportedException("Can assume only expression - PHP syntax error");
            }
            foreach (var value in evaluatedPart.PossibleValues)
            {
                var boolean = value as BooleanValue;
                if (boolean != null)
                {
                    if (!boolean.Value)
                    {
                        //false cannot be evaluted as true
                        continue;
                    }
                }

                if (value is UndefinedValue)
                {
                    //undefined value is evaluated as false
                    continue;
                }

                //This part can be evaluated as true
                return false;
            }

            //some of possible values cant be evaluted as true
            return true;
        }

        /// <summary>
        /// Assume valid condition into output set
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="expressionParts"></param>
        private void processAssumption(AssumptionCondition condition)
        {
            if (condition.Form == ConditionForm.All)
            {
                if (condition.Parts.Count() == 1)
                {
                    assumeBinary(condition.Parts.First().SourceElement as BinaryEx);
                }
            }
        }

        private void assumeBinary(BinaryEx exp)
        {
            if (exp == null)
            {
                return;
            }
            switch (exp.PublicOperation)
            {
                case Operations.Equal:
                    assumeEqual(exp.LeftExpr, exp.RightExpr);
                    break;
            }
        }

        private void assumeEqual(LangElement left, LangElement right)
        {
            VariableEntry leftVar;
            MemoryEntry value;

            var call = left as DirectFcnCall;
            if (call != null && call.QualifiedName.Name.Value == "abs")
            {
                var absParam = call.CallSignature.Parameters[0];
                leftVar = _log.GetVariable(absParam.Expression);
                value = getReverse_abs(_log.GetValue(right));
            }
            else
            {
                leftVar = _log.GetVariable(left);
                value = _log.GetValue(right);
            }

            if (leftVar != null && value != null)
            {
                foreach (var possibleVar in leftVar.PossibleNames)
                {
                    _outSet.Assign(possibleVar, value);
                }

            }
        }

        private MemoryEntry getReverse_abs(MemoryEntry entry)
        {
            var values = new HashSet<Value>();
            foreach (IntegerValue value in entry.PossibleValues)
            {
                values.Add(_outSet.CreateInt(value.Value));
                values.Add(_outSet.CreateInt(-value.Value));
            }

            return new MemoryEntry(values);
        }

        #endregion
    }
}
