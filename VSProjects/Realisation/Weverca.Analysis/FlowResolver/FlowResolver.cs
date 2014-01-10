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
            //TODO: How to resolve not-bool conditions, like if (1) etc.?
            //TODO: if(False) there is empty avaluated parts --> is evaluated like "can be true".

            ConditionParts conditionParts = new ConditionParts(condition.Form, outSet.Snapshot, log, condition.Parts);
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

        /// <summary>
        /// Reports about try block scope start
        /// </summary>
        /// <param name="outSet"></param>
        /// <param name="catchBlockStarts">Catch blocks associated with starting try block</param>
        public override void TryScopeStart(FlowOutputSet outSet, IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>> catchBlockStarts)
        {
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            List<Value> result = new List<Value>();
            foreach(var stack in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                result.Add(outSet.CreateInfo(new CatchBlocks((stack as InfoValue<CatchBlocks>).Data, catchBlockStarts)));
            }
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(result));
        }

        /// <summary>
        /// Reports about try block scope end
        /// </summary>
        /// <param name="outSet"></param>
        /// <param name="catchBlockStarts">Catch blocks associated with ending try block</param>
        public override void TryScopeEnd(FlowOutputSet outSet, IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>> catchBlockStarts)
        {
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
             List<Value> result = new List<Value>();
             foreach (var value in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
             {
                 CatchBlocks stack = (value as InfoValue<CatchBlocks>).Data;
                result.Add(outSet.CreateInfo(stack.Pop()));
             }
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(result));
        }

        /// <summary>
        /// Process throw statement according to current flow
        /// </summary>
        /// <param name="outSet">Flow output set</param>
        /// <param name="throwStmt">Processed throw statement</param>
        /// <param name="throwedValue">Value that was supplied into throw statement</param>
        /// <returns>
        /// All possible catch block starts
        /// </returns>
        public override IEnumerable<ProgramPointBase> Throw(FlowOutputSet outSet, PHP.Core.AST.ThrowStmt throwStmt, MemoryEntry throwedValue)
        {
            List<ProgramPointBase> result = new List<ProgramPointBase>();
            var catchBlocks = outSet.GetControlVariable(new VariableName(".catchBlocks"));
            List<List<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>> stack = new List<List<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>>();
           
            foreach(var value in catchBlocks.ReadMemory(outSet.Snapshot).PossibleValues)
            {
                if (stack.Count == 0)
                {
                    for (int i = 0; i < (value as InfoValue<CatchBlocks>).Data.blocks.Count;i++ )
                    {
                        stack.Add(new List<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>());
                    }
                }
                for (int i = 0; i < (value as InfoValue<CatchBlocks>).Data.blocks.Count; i++)
                {
                    stack[i].AddRange((value as InfoValue<CatchBlocks>).Data.blocks[i]);
                }
            }

            bool foundMatch = false;
            while (stack.Count > 0)
            {
                foreach (Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase> catchBlock in stack.Last())
                {
                    foreach (Value value in throwedValue.PossibleValues)
                    {
                        if (value is ObjectValue)
                        {
                            TypeValue type = outSet.ObjectType(value as ObjectValue);
                            if (type.QualifiedName == catchBlock.Item1.QualifiedName)
                            {
                                result.Add(catchBlock.Item2);
                                foundMatch = true;
                            }
                            else
                            {
                                for (int i = type.Declaration.BaseClasses.Count - 1; i >= 0; i--)
                                {
                                    if (type.Declaration.BaseClasses[i] == catchBlock.Item1.QualifiedName)
                                    {
                                        result.Add(catchBlock.Item2);
                                        foundMatch = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (value is AnyObjectValue || value is AnyValue)
                        {
                            result.Add(catchBlock.Item2);
                            foundMatch = true;
                        }
                        else
                        {
                            //warning only objects can be thrown
                        }
                    }
                }
                stack.RemoveAt(stack.Count - 1);
                if (foundMatch)
                {
                    break;
                }
            }
            catchBlocks.WriteMemory(outSet.Snapshot, new MemoryEntry(outSet.CreateInfo(new CatchBlocks(stack as IEnumerable<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>>))));
            return result;
        }
        
        #endregion
    }


    public class CatchBlocks
    {
        public readonly ReadOnlyCollection<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>> blocks;
        public CatchBlocks(IEnumerable<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>> blocks)
        {
            this.blocks = new ReadOnlyCollection<IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>>>(new List<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>>(blocks));
        }
        public CatchBlocks(CatchBlocks blocks,IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>> catchBlocks)
        {
            List<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>> list = new List<IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>>>(blocks.blocks);
            list.Add(catchBlocks);
            this.blocks = new ReadOnlyCollection<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>>(list);
        }
        public CatchBlocks()
        {
            blocks = new ReadOnlyCollection<IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>>>(new List<IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>>>());
        }
        public CatchBlocks Pop()
        {
            List<IEnumerable<Tuple<PHP.Core.GenericQualifiedName, ProgramPointBase>>> list = new List<IEnumerable<Tuple<GenericQualifiedName, ProgramPointBase>>>(blocks);
            list.RemoveAt(list.Count - 1);
            return new CatchBlocks(list);
        }
    }

}
