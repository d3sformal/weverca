using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.ProgramPoints;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Controlling flow actions during analysis
    /// </summary>
    public class SimpleFlowResolver : FlowResolverBase
    {
        private readonly static VariableName CatchBlocks_Storage = new VariableName(".catch_blocks");

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
                    willAssume = needsAll(outSet.Snapshot, condition.Parts);
                    break;
                case ConditionForm.None:
                    willAssume = needsNone(outSet.Snapshot, condition.Parts);
                    break;
                case ConditionForm.SomeNot:
                    willAssume = needsSomeNot(outSet.Snapshot, condition.Parts);
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

        public override void CallDispatchMerge(FlowOutputSet callerOutput, IEnumerable<ExtensionPoint> dispatchedExtensions)
        {
            var ends = (from callOutput in dispatchedExtensions where callOutput.Graph.End.OutSet != null select callOutput.Graph.End.OutSet as ISnapshotReadonly).ToArray();

            //TODO determine correct extension type
            var callType = dispatchedExtensions.First().Type;

            switch (callType)
            {
                case ExtensionType.ParallelEval:
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

                default:
                    throw new NotImplementedException();
            }
        }

        public override void TryScopeStart(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts)
        {
            var blockStarts = new List<InfoValue>();
            //NOTE this is only simple implementation without resolving try block stack
            foreach (var blockStart in catchBlockStarts)
            {
                var blockValue = outSet.CreateInfo(blockStart);
                blockStarts.Add(blockValue);
            }

            var catchBlocks = outSet.GetControlVariable(CatchBlocks_Storage);
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(blockStarts));
        }

        public override void TryScopeEnd(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockEnds)
        {
            //NOTE in simple implementation we don't resolve try block stack
            var endingBlocks = new HashSet<CatchBlockDescription>(catchBlockEnds);

            var catchBlocks = outSet.GetControlVariable(CatchBlocks_Storage);
            var catchBlockValues = catchBlocks.ReadMemory(outSet.Snapshot);

            var remainingCatchBlocks = new List<InfoValue>();
            foreach (InfoValue<CatchBlockDescription> block in catchBlockValues.PossibleValues)
            {
                if (!endingBlocks.Contains(block.Data))
                    remainingCatchBlocks.Add(block);
            }

            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(remainingCatchBlocks));
        }

        public override IEnumerable<ThrowInfo> Throw(FlowController flow, FlowOutputSet outSet, ThrowStmt throwStmt, MemoryEntry throwedValue)
        {
            //TODO this is only simple implementation
            var exceptionObj = (ObjectValue)throwedValue.PossibleValues.First();

            var catchBlocks = outSet.ReadControlVariable(CatchBlocks_Storage).ReadMemory(outSet.Snapshot);

            var throwBranches = new List<ThrowInfo>();
            //find catch blocks with valid scope and matching catch condition
            foreach (InfoValue<CatchBlockDescription> blockInfo in catchBlocks.PossibleValues)
            {
                var throwedType = outSet.ObjectType(exceptionObj).QualifiedName;

                //check catch condition
                if (blockInfo.Data.CatchedType.QualifiedName != throwedType)
                    continue;

                var branch = new ThrowInfo(blockInfo.Data, throwedValue);
                throwBranches.Add(branch);
            }

            return throwBranches;
        }

        public override void Catch(CatchPoint catchPoint, FlowOutputSet outSet)
        {
            //TODO this is simple catch demonstration - there should be catch stack unrolling

            var catchVariable = catchPoint.CatchDescription.CatchVariable;
            var hasCatchVariable = catchVariable != null;

            if (hasCatchVariable)
            {
                var catchVar = outSet.GetVariable(catchPoint.CatchDescription.CatchVariable);
                catchVar.WriteMemory(outSet.Snapshot, catchPoint.ThrowedValue);
            }
        }

        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            //extend current program point as Include

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
                var cfg = AnalysisTestUtils.CreateCFG(_includes[file], null);
                var ppGraph = ProgramPointGraph.FromSource(cfg);
                flow.AddExtension(file, ppGraph, ExtensionType.ParallelInclude);
            }
        }

        public override void Eval(FlowController flow, MemoryEntry code)
        {
            //extend current program point as Eval

            var codes = new HashSet<string>();
            foreach (StringValue possibleFile in code.PossibleValues)
            {
                codes.Add(possibleFile.Value);
            }

            foreach (var branchKey in flow.ExtensionKeys)
            {
                if (!codes.Remove(branchKey as string))
                {
                    //this eval is now not resolved as possible eval branch
                    flow.RemoveExtension(branchKey);
                }
            }

            foreach (var sourceCode in codes)
            {
                //Create graph for every evaluated code - NOTE: we can share pp graphs
                var cfg = AnalysisTestUtils.CreateCFG(sourceCode, null);
                var ppGraph = ProgramPointGraph.FromSource(cfg);
                flow.AddExtension(sourceCode, ppGraph, ExtensionType.ParallelEval);
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

        private bool needsAll(SnapshotBase input, IEnumerable<Expressions.Postfix> conditionParts)
        {
            //we are searching for one part, that can be evaluated only as false

            foreach (var evaluatedPart in conditionParts)
            {
                var value = _log.ReadSnapshotEntry(evaluatedPart.SourceElement).ReadMemory(input);
                if (evalOnlyFalse(value))
                {
                    return false;
                }
            }

            //cant disprove any part
            return true;
        }

        private bool needsNone(SnapshotBase input, IEnumerable<Expressions.Postfix> conditionParts)
        {
            //we are searching for one part, that can be evaluated only as false

            foreach (var evaluatedPart in conditionParts)
            {
                var value = _log.ReadSnapshotEntry(evaluatedPart.SourceElement).ReadMemory(input);
                if (canEvalTrue(value))
                {
                    return false;
                }
            }

            //cant disprove any part
            return true;
        }

        private bool needsSomeNot(SnapshotBase input, IEnumerable<Expressions.Postfix> conditionParts)
        {
            //we are searching for one part, that can be evaluated as false

            foreach (var evaluatedPart in conditionParts)
            {
                var value = _log.ReadSnapshotEntry(evaluatedPart.SourceElement).ReadMemory(input);
                if (canEvalFalse(value))
                {
                    return true;
                }
            }

            //cant disprove any part
            return false;
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

                if (value is AnyValue)
                {
                    return false;
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

        private bool canEvalFalse(MemoryEntry evaluatedPart)
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
                        // false found
                        return true;
                    }
                }

                if (value is AnyValue)
                {
                    return true;
                }

                if (value is UndefinedValue)
                {
                    //undefined value is evaluated as false
                    return true;
                }
            }

            return false;
        }

        private bool canEvalTrue(MemoryEntry evaluatedPart)
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
                        // false found
                        continue;
                    }
                }

                if (value is UndefinedValue)
                {
                    //undefined value is evaluated as false
                    continue;
                }


                //other values can be true
                return true;
            }

            return false;
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
                    var sourceElement = condition.Parts.First().SourceElement;
                    var binary = sourceElement as BinaryEx;

                    assumeBinary(binary);
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
            ReadWriteSnapshotEntryBase lValue;
            MemoryEntry value;

            var paramValue = _log.ReadSnapshotEntry(right).ReadMemory(_outSet.Snapshot);
            var call = left as DirectFcnCall;
            if (call != null && call.QualifiedName.Name.Value == "abs")
            {
                var absParam = call.CallSignature.Parameters[0];
                lValue = _log.GetSnapshotEntry(absParam.Expression);



                value = getReverse_abs(paramValue);
            }
            else
            {
                lValue = _log.GetSnapshotEntry(left);
                value = paramValue;
            }

            if (lValue != null && value != null)
            {
                lValue.WriteMemory(_outSet.Snapshot, value, true);
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
