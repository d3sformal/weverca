using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;
using PHP.Core;
using PHP.Core.AST;
using System.Collections.ObjectModel;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// This class is used for evaluating conditions and assumptions.
    /// According to the result of the assumption the environment inside of the code block is set up.
    /// </summary>
    public class FlowResolver : FlowResolverBase
    {
        #region FlowResolverBase overrides

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="log"></param>
        /// <returns>
        ///   <c>false</c> if condition cannot be ever satisfied, true otherwise.
        /// </returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log)
        {
            ConditionParts conditionParts = new ConditionParts(condition.Form, outSet, log, condition.Parts);

            return conditionParts.MakeAssumption(null);
        }

        /// <summary>
        /// Is called after each invoked call - has to merge data from dispatched calls into callerOutput
        /// </summary>
        /// <param name="callerOutput">Output of caller, which dispatch calls</param>
        /// <param name="dispatchedExtensions">Program point graphs obtained during analysis</param>
        /// <param name="dispatchType">Type of merged call</param>
        public override void CallDispatchMerge(FlowOutputSet callerOutput, IEnumerable<ExtensionPoint> dispatchedExtensions)
        {
            var ends = dispatchedExtensions.Select(c => c.Graph.End.OutSet).ToArray();
            callerOutput.MergeWithCallLevel(ends);
        }

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>
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
                var fileInfo = findFile(flow, file);
                if (fileInfo == null)
                {
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning("The file " + file + " was included and not found", flow.ProgramPoint.Partial, AnalysisWarningCause.FILE_TO_BE_INCLUDED_NOT_FOUND));
                    continue;
                }

                //Create graph for every include - NOTE: we can share pp graphs
                var cfg = ControlFlowGraph.ControlFlowGraph.FromFilename(fileInfo.FullName);
                var ppGraph = ProgramPointGraph.FromSource(cfg, fileInfo);
                flow.AddExtension(file, ppGraph, ExtensionType.ParallelInclude);
            }
        }

        /// <summary>
        /// Finds the file to be included
        /// </summary>
        private FileInfo findFile(FlowController flow, string fileName)
        {
            // the file has relative path and it is in main script directory
            var fileInfo = new FileInfo(ForwardAnalysisServices.EntryScript.DirectoryName + "/" + fileName);
            if (fileInfo.Exists) return fileInfo;

            // the file has relative path and it is in current script directory
            fileInfo = new FileInfo(flow.ProgramPoint.ppGraph.OwningScript.DirectoryName + "/" + fileName);
            if (fileInfo.Exists) return fileInfo;

            // the file has absolute path
            fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists) return fileInfo;

            return null;
        }

        /// <inheritdoc />
        public override void TryScopeStart(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts)
        {
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            List<Value> result = new List<Value>();
            foreach (var stack in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                result.Add(outSet.CreateInfo(new TryBlockStack((stack as InfoValue<TryBlockStack>).Data, catchBlockStarts)));
            }
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(result));
        }

        /// <inheritdoc />
        public override void TryScopeEnd(FlowOutputSet outSet, IEnumerable<CatchBlockDescription> catchBlockStarts)
        {
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            List<Value> result = new List<Value>();
            foreach (var value in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                TryBlockStack stack = (value as InfoValue<TryBlockStack>).Data;
                result.Add(outSet.CreateInfo(stack.Pop()));
            }
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(result));
        }

        /// <inheritdoc />
        public override IEnumerable<ThrowInfo> Throw(FlowController flow, FlowOutputSet outSet, ThrowStmt throwStmt, MemoryEntry throwedValue)
        {
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            var stack = new List<HashSet<CatchBlockDescription>>();

            foreach (var value in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                if (stack.Count == 0)
                {
                    for (int i = 0; i < (value as InfoValue<TryBlockStack>).Data.blocks.Count; i++)
                    {
                        stack.Add(new HashSet<CatchBlockDescription>());
                    }
                }
                for (int i = 0; i < (value as InfoValue<TryBlockStack>).Data.blocks.Count; i++)
                {
                    foreach (var block in (value as InfoValue<TryBlockStack>).Data.blocks[i])
                        stack[i].Add(block);
                }
            }

            Dictionary<CatchBlockDescription, List<Value>> result = new Dictionary<CatchBlockDescription, List<Value>>();

            foreach (Value value in throwedValue.PossibleValues)
            {
                bool foundMatch = false;
                if (value is ObjectValue)
                {
                    TypeValue type = outSet.ObjectType(value as ObjectValue);
                    var exceptionName=new QualifiedName(new Name("Exception"));
                    if (type.Declaration.BaseClasses.Where(a => a.Equals(exceptionName)).Count() == 0 && !type.QualifiedName.Equals(exceptionName))
                    {
                        AnalysisWarningHandler.SetWarning(outSet, new AnalysisWarning("Only objects derived from Exception can be thrown", throwStmt, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
                        foundMatch = false;
                    }
                    else
                    {
                        for (int i = stack.Count - 1; i >= 0; i--)
                        {
                            foreach (var block in stack[i])
                            {
                                if (type.QualifiedName == block.CatchedType.QualifiedName)
                                {
                                    var key = block;
                                    if (!result.ContainsKey(key))
                                    {
                                        result[key] = new List<Value>();
                                    }
                                    result[key].Add(value);
                                    foundMatch = true;
                                }
                                else
                                {
                                    for (int j = type.Declaration.BaseClasses.Count - 1; j >= 0; j--)
                                    {
                                        if (type.Declaration.BaseClasses[j] == block.CatchedType.QualifiedName)
                                        {
                                            var key = block;
                                            if (!result.ContainsKey(key))
                                            {
                                                result[key] = new List<Value>();
                                            }
                                            result[key].Add(value);
                                            foundMatch = true;
                                            break;
                                        }
                                    }
                                }
                                if (foundMatch)
                                    break;
                            }
                            if (foundMatch)
                                break;
                        }
                    }

                }
                else if (value is AnyObjectValue || value is AnyValue)
                {
                    for (int i = stack.Count - 1; i >= 0; i--)
                    {
                        foreach (var block in stack[i])
                        {
                            var key = block;

                            if (!result.ContainsKey(key))
                            {
                                result[key] = new List<Value>();
                            }
                            result[key].Add(value);
                            foundMatch = true;
                        }
                    }
                }
                else
                {
                    AnalysisWarningHandler.SetWarning(outSet, new AnalysisWarning("Only objects can be thrown", throwStmt, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
                    foundMatch = false;

                }
                if (!foundMatch)
                {
                    var key = new CatchBlockDescription(flow.ProgramEnd, new GenericQualifiedName(new QualifiedName(new Name(""))), new VariableIdentifier(""));
                    if (!result.ContainsKey(key))
                    {
                        result[key] = new List<Value>();
                    }
                    result[key].Add(value);
                }
            }

            List<ThrowInfo> res = new List<ThrowInfo>();

            foreach (var entry in result)
            {
                res.Add(new ThrowInfo(entry.Key, new MemoryEntry(entry.Value)));
            }

            return res;
        }

        /// <inheritdoc />
        public override void Catch(CatchPoint catchPoint, FlowOutputSet outSet)
        {
            if (catchPoint.CatchDescription.CatchedType.QualifiedName.Equals(new QualifiedName(new Name(""))))
            {
                return;
            }
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            var stack = new List<HashSet<CatchBlockDescription>>();

            foreach (var value in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                if (stack.Count == 0)
                {
                    for (int i = 0; i < (value as InfoValue<TryBlockStack>).Data.blocks.Count; i++)
                    {
                        stack.Add(new HashSet<CatchBlockDescription>());
                    }
                }
                for (int i = 0; i < (value as InfoValue<TryBlockStack>).Data.blocks.Count; i++)
                {
                    foreach (var block in (value as InfoValue<TryBlockStack>).Data.blocks[i])
                        stack[i].Add(block);
                }
            }

            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (stack[i].Where(a => a.CatchedType.QualifiedName.Equals(catchPoint.CatchDescription.CatchedType.QualifiedName)).Count() > 0)
                {
                    stack.RemoveLast();
                    break;
                }
                stack.RemoveLast();
            }

            outSet.GetControlVariable(new VariableName(".catchBlocks")).WriteMemory(outSet.Snapshot, new MemoryEntry(outSet.CreateInfo(new TryBlockStack(stack))));

        }

        #endregion
    }



    /// <summary>
    /// Class representing stack of try with catch bloks.
    /// It is imutable.
    /// </summary>
    public class TryBlockStack
    {
        /// <summary>
        /// stack storing try and catch information
        /// </summary>
        public readonly ReadOnlyCollection<IEnumerable<CatchBlockDescription>> blocks;

        /// <summary>
        /// Create new instace of CatchBlocks
        /// </summary>
        /// <param name="blocks">Stack</param>
        public TryBlockStack(IEnumerable<IEnumerable<CatchBlockDescription>> blocks)
        {
            this.blocks = new ReadOnlyCollection<IEnumerable<CatchBlockDescription>>(new List<IEnumerable<CatchBlockDescription>>(blocks));
        }

        /// <summary>
        /// Create new instace of CatchBlocks. It inserts new block on the old stack.
        /// </summary>
        /// <param name="blocks">old stack</param>
        /// <param name="catchBlocks">new inserted information</param>
        public TryBlockStack(TryBlockStack blocks, IEnumerable<CatchBlockDescription> catchBlocks)
        {
            var list = new List<IEnumerable<CatchBlockDescription>>(blocks.blocks);
            list.Add(catchBlocks);
            this.blocks = new ReadOnlyCollection<IEnumerable<CatchBlockDescription>>(list);
        }

        /// <summary>
        /// Create new instance with empty stack
        /// </summary>
        public TryBlockStack()
        {
            blocks = new ReadOnlyCollection<IEnumerable<CatchBlockDescription>>(new List<IEnumerable<CatchBlockDescription>>());
        }

        /// <summary>
        /// Pop the current stack
        /// </summary>
        /// <returns>new Catchblocks with last item removed</returns>
        public TryBlockStack Pop()
        {
            var list = new List<IEnumerable<CatchBlockDescription>>(blocks);
            list.RemoveAt(list.Count - 1);
            return new TryBlockStack(list);
        }
    }

}
