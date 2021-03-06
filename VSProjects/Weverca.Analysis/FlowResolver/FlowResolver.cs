/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
using PHP.Core.Reflection;
using Weverca.Analysis.ExpressionEvaluator;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// This class is used for evaluating conditions and assumptions.
    /// According to the result of the assumption the environment inside of the code block is set up.
    /// </summary>
    public class FlowResolver : FlowResolverBase
    {
        private readonly Dictionary<string, ProgramPointGraph> sharedProgramPoints = new Dictionary<string, ProgramPointGraph>();

        private readonly HashSet<string> sharedFiles = new HashSet<string>();

        #region FlowResolverBase overrides

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="log">The evaluation log of the code constructs.</param>
        /// <returns>
        ///   <c>false</c> if condition cannot be ever satisfied, true otherwise.
        /// </returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, EvaluationLog log)
        {
            // Deprecated code that supported assumptions that are not decomposed using logical operators (&&, ||, xor)
            // But it had very limited abstract state refinemend
            /*
            AssumptionConditionExecuterDepr conditionParts = new AssumptionConditionExecuterDepr(condition.Form, outSet, log, condition.Parts);
            return conditionParts.MakeAssumption(null);
             */

            // Non-primitive conditions in assumptions are now resolved on the level of control-flow graph
            PHP.Core.Debug.Assert(condition.Parts.Count() == 1);
            PHP.Core.Debug.Assert(condition.Form == ConditionForm.All || condition.Form == ConditionForm.None);

            var assumptionExecuter = new AssumptionExecuter(condition.Form, condition.Parts.First().SourceElement, log, outSet);
            AssumptionExecuter.PossibleValues enterAssumption = assumptionExecuter.IsSatisfied();

            if (enterAssumption == AssumptionExecuter.PossibleValues.OnlyFalse) return false;
            if (enterAssumption == AssumptionExecuter.PossibleValues.Unknown) 
            { 
                 assumptionExecuter.RefineState();
            }

            return true;
        }

        /// <inheritdoc />
        public override void CallDispatchMerge(ProgramPointBase beforeCall, FlowOutputSet afterCall, IEnumerable<ExtensionPoint> dispatchedExtensions)
        {
            var ends = dispatchedExtensions.Select(c => c.Graph.End.OutSet).Where(a => a != null).ToArray();

            var callType = dispatchedExtensions.First().Type;

            switch (callType)
            {
                case ExtensionType.ParallelEval:
                case ExtensionType.ParallelInclude:
                    //merging from includes behaves like usual 
                    //program points extend
                    afterCall.Extend(ends);
                    break;
                case ExtensionType.ParallelCall:
                    //merging from calls needs special behaviour
                    //from memory model (there are no propagation of locales e.g)
                    afterCall.MergeWithCallLevel(beforeCall, ends);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>
        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            if (FlagsHandler.IsDirty(includeFile.PossibleValues, FlagType.FilePathDirty))
            { 
                AnalysisWarningHandler.SetWarning(flow.OutSet,new AnalysisSecurityWarning(flow.CurrentScript.FullName,flow.CurrentPartial,flow.CurrentProgramPoint, FlagType.FilePathDirty, ""));
            }

            bool isAlwaysConcrete = true;
            //extend current program point as Include
            List<string> files = FunctionResolver.GetFunctionNames(includeFile, flow, out isAlwaysConcrete);

            if (isAlwaysConcrete == false)
            {
                AnalysisWarningHandler.SetWarning(flow.OutSet,new AnalysisWarning(flow.CurrentScript.FullName,"Couldn't resolve all possible includes",flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.COULDNT_RESOLVE_ALL_INCLUDES));
            }


            IncludingEx includeExpression = flow.CurrentPartial as IncludingEx;

            foreach (var branchKey in flow.ExtensionKeys)
            {
                if (!files.Remove(branchKey as string))
                {
                    //this include is now not resolved as possible include branch
                    flow.RemoveExtension(branchKey);
                }
            }

            int numberOfWarnings = 0;
            var includedFiles = flow.OutSet.GetControlVariable(new VariableName(".includedFiles")).ReadMemory(flow.OutSet.Snapshot);
			foreach (var file in files)
            {
                var fileInfo = findFile(flow, file);

                if (fileInfo == null)
                {
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName,"The file " + file + " to be included not found", flow.CurrentProgramPoint.Partial, flow.CurrentProgramPoint, AnalysisWarningCause.FILE_TO_BE_INCLUDED_NOT_FOUND));
                    numberOfWarnings++;
                    continue;
                }

                string fileName = fileInfo.FullName;

				// Handling include_once, require_once
				var varIncluded = flow.OutSet.GetControlVariable(new VariableName(fileName));
				if (includeExpression.InclusionType == InclusionTypes.IncludeOnce || includeExpression.InclusionType == InclusionTypes.RequireOnce)
				{
					var includedInfo = varIncluded.ReadMemory (flow.OutSet.Snapshot);
					if (includedInfo != null) 
					{
						var includeType = (includeExpression.InclusionType == InclusionTypes.IncludeOnce) ? "include_once" : "require_once";
						if (includedInfo.PossibleValues.Count () > 1) {
							//TODO: report or not?
							//AnalysisWarningHandler.SetWarning (flow.OutSet, new AnalysisWarning (flow.CurrentScript.FullName, includeType + " is called more times in some program paths with the file " + fileName, flow.CurrentProgramPoint.Partial, flow.CurrentProgramPoint, AnalysisWarningCause.INCLUDE_REQUIRE_ONCE_CALLED_MORE_TIMES_WITH_SAME_FILE));
							// TODO: include the file or not??
							continue;
						} else 
						{
							if (!(includedInfo.PossibleValues.First () is UndefinedValue)) 
							{
								//TODO: report or not?
								//AnalysisWarningHandler.SetWarning (flow.OutSet, new AnalysisWarning (flow.CurrentScript.FullName, includeType + " is called more times with the file " + fileName, flow.CurrentProgramPoint.Partial, flow.CurrentProgramPoint, AnalysisWarningCause.INCLUDE_REQUIRE_ONCE_CALLED_MORE_TIMES_WITH_SAME_FILE));
								continue;
							}
						}


					}
				}


				// Avoid recursive includes
                // the file was never included
                int numberOfIncludes = -1;
                // TODO: optimization - avoid iterating all included files
                //  - make .includedFiles associative array with names of functions as indexes
                foreach (InfoValue<NumberOfCalls<string>> includeInfo in includedFiles.PossibleValues.Where(a => (a is InfoValue<NumberOfCalls<string>>)))
                {
                    if (includeInfo.Data.Callee == fileName)
                    {
                        numberOfIncludes = Math.Max(numberOfIncludes, includeInfo.Data.TimesCalled);
                    }
                }
                if (numberOfIncludes >= 0)
                {
                    if (numberOfIncludes > 2 || sharedFiles.Contains(fileName))
                    {

                        if (sharedFiles.Contains(fileName))
                        {
                            //set graph sharing for this function
                            if (!sharedProgramPoints.ContainsKey(fileName))
                            {
                                try
                                {
                                    //create single graph instance
                                    sharedProgramPoints[fileName] = ProgramPointGraph.FromSource(ControlFlowGraph.ControlFlowGraph.FromFile(fileInfo));
                                }
                                catch (ControlFlowGraph.ControlFlowException)
                                {
                                    numberOfWarnings++;
                                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Control flow graph creation error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.CFG_EXCEPTION_IN_INCLUDE_OR_EVAL));
                                }
                                catch (Parsers.ParserException)
                                {
                                    numberOfWarnings++;
                                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Parser error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.PARSER_EXCEPTION_IN_INCLUDE_OR_EVAL));
                                }
                            }

                            //get shared instance of program point graph
							flow.AddExtension(fileName, sharedProgramPoints[fileName], ExtensionType.ParallelInclude);
                            continue;
                        }
                        else
                        {
                            sharedFiles.Add(fileName);
                        }

                    }
                }

                try
                {
					// Write information about inclusion of the file
					varIncluded.WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(flow.OutSet.Snapshot.CreateBool(true)));

                    //Create graph for every include - NOTE: we can share pp graphs
                    var cfg = ControlFlowGraph.ControlFlowGraph.FromFile(fileInfo);
                    var ppGraph = ProgramPointGraph.FromSource(cfg);
					flow.AddExtension(fileName, ppGraph, ExtensionType.ParallelInclude);
                }
                catch (ControlFlowGraph.ControlFlowException)
                {
                    numberOfWarnings++;
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Control flow graph creation error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.CFG_EXCEPTION_IN_INCLUDE_OR_EVAL));
                }
                catch (Parsers.ParserException)
                {
                    numberOfWarnings++;
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Parser error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.PARSER_EXCEPTION_IN_INCLUDE_OR_EVAL));
                }
            }

            if (numberOfWarnings > 0 && (includeExpression.InclusionType == InclusionTypes.Require || includeExpression.InclusionType == InclusionTypes.RequireOnce))
            {
                if (numberOfWarnings == files.Count && isAlwaysConcrete == true)
                {
                    fatalError(flow, true);
                }
                else
                {
                    fatalError(flow, false);
                }
            }

        }

        /// <summary>
        /// Finds the file to be included
        /// </summary>
        private FileInfo findFile(FlowController flow, string fileName)
        {
            FileInfo fileInfo;
            // the file has relative path and it is in main script directory
            try
            {
                fileInfo = new FileInfo(flow.EntryScript.DirectoryName + Path.DirectorySeparatorChar + fileName);
                if (fileInfo.Exists) return fileInfo;
            }
            catch (System.NotSupportedException)
            {
                // not a well-formed file name, try it differently
            }

            
            // the file has relative path and it is in current script directory
            try
            {
                fileInfo = new FileInfo(flow.CurrentProgramPoint.OwningPPGraph.OwningScript.DirectoryName + Path.DirectorySeparatorChar + fileName);
                if (fileInfo.Exists) return fileInfo;
            }
            catch (System.NotSupportedException)
            {
                // not a well-formed file name, try it differently
            }


            // the file has absolute path
			if (fileName.Length == 0)
				return null;
            try
            {
                fileInfo = new FileInfo(fileName);
                if (fileInfo.Exists) return fileInfo;
            }
            catch (System.NotSupportedException)
            {
                // not a well-formed file name, give up
            }

            return null;
        }

        #region exception handling

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

        /// <summary>
        /// Process throw statement according to current flow
        /// </summary>
        /// <param name="flow">Flow controller which provides API usefull for throw resolvings</param>
        /// <param name="outSet">Flow output set</param>
        /// <param name="throwStmt">Processed throw statement</param>
        /// <param name="throwedValue">Value that was supplied into throw statement</param>
        /// <returns>
        /// All possible ThrowInfo branches
        /// </returns>
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
            int numberOfWarnings = 0;
            foreach (Value value in throwedValue.PossibleValues)
            {
                bool foundMatch = false;
                if (value is ObjectValue)
                {
                    TypeValue type = outSet.ObjectType(value as ObjectValue);
                    var exceptionName = new QualifiedName(new Name("Exception"));
                    if (type.Declaration.BaseClasses.Where(a => a.Equals(exceptionName)).Count() == 0 && !type.QualifiedName.Equals(exceptionName))
                    {
                        AnalysisWarningHandler.SetWarning(outSet, new AnalysisWarning(flow.CurrentScript.FullName, "Only objects derived from Exception can be thrown", throwStmt, flow.CurrentProgramPoint, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
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
                    AnalysisWarningHandler.SetWarning(outSet, new AnalysisWarning(flow.CurrentScript.FullName, "Only objects can be thrown", throwStmt, flow.CurrentProgramPoint, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
                    numberOfWarnings++;
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

            if (numberOfWarnings >= throwedValue.Count)
            {
                fatalError(flow, true);
            }
            else if (numberOfWarnings > 0)
            {
                fatalError(flow, false);
            }

            return res;
        }

        private void fatalError(FlowController flow,bool removeFlowChildren)
        {
            var catchedType = new GenericQualifiedName(new QualifiedName(new Name(string.Empty)));
            var catchVariable = new VariableIdentifier(string.Empty);
            var description = new CatchBlockDescription(flow.ProgramEnd, catchedType, catchVariable);
            var info = new ThrowInfo(description, new MemoryEntry());

            var throws = new ThrowInfo[] { info };
            flow.SetThrowBranching(throws, removeFlowChildren);
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
            outSet.GetVariable(catchPoint.CatchDescription.CatchVariable).WriteMemory(outSet.Snapshot, catchPoint.ThrowedValue);
        }
        #endregion

        

        /// <inheritdoc />
        public override void Eval(FlowController flow, MemoryEntry code)
        {
            var flags=FlagsHandler.GetFlags(code.PossibleValues);
            if (flags.isDirty(FlagType.FilePathDirty) || flags.isDirty(FlagType.SQLDirty) || flags.isDirty(FlagType.FilePathDirty))
            {
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisSecurityWarning(flow.CurrentScript.FullName, "Eval shoudn't contain anything from user input", flow.CurrentPartial, flow.CurrentProgramPoint, FlagType.HTMLDirty));
            }

            double evalDepth=0;
            var maxValue = new MaxValueVisitor(flow.OutSet);
            evalDepth = maxValue.Evaluate(flow.OutSet.GetControlVariable(FunctionResolver.evalDepth).ReadMemory(flow.OutSet.Snapshot));
            if (evalDepth > 3)
            {
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, @"Eval cannot be called in ""eval recursion"" more than 3 times", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.TOO_DEEP_EVAL_RECURSION));
                return;
            }


            StringConverter converter = new StringConverter();
            converter.SetContext(flow);

            bool isAllwasConcrete = true;
            var codes = new HashSet<string>();
            foreach (StringValue possibleFile in converter.Evaluate(code, out isAllwasConcrete))
            {
                codes.Add(string.Format("<? {0}; ?>",possibleFile.Value));
            }

            if (isAllwasConcrete == false)
            {
                AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Couldn't resolve all possible evals", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.COULDNT_RESOLVE_ALL_EVALS));
            }

            foreach (var branchKey in flow.ExtensionKeys)
            {
                if(branchKey is string)
                {
                    if (!codes.Remove(branchKey as string))
                    {
                        //this eval is now not resolved as possible eval branch
                        flow.RemoveExtension(branchKey);
                    }
                }
            }
            int numberOfWarnings = 0;
            foreach (var sourceCode in codes)
            {
                try
                {
                    var cfg = ControlFlowGraph.ControlFlowGraph.FromSource(sourceCode, flow.CurrentScript.FullName);
                    var ppGraph = ProgramPointGraph.FromSource(cfg);
                    flow.AddExtension(sourceCode, ppGraph, ExtensionType.ParallelEval);
                }
                catch (ControlFlowGraph.ControlFlowException)
                {
                    numberOfWarnings++;
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Control flow graph creation error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.CFG_EXCEPTION_IN_INCLUDE_OR_EVAL));
                }
                catch(Parsers.ParserException)
                {
                    numberOfWarnings++;
                    AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisWarning(flow.CurrentScript.FullName, "Parser error", flow.CurrentPartial, flow.CurrentProgramPoint, AnalysisWarningCause.PARSER_EXCEPTION_IN_INCLUDE_OR_EVAL));
                }
            }
            
            if (numberOfWarnings > 0)
            {
                if (numberOfWarnings == codes.Count && isAllwasConcrete==true)
                {
                    fatalError(flow,true);
                }
                else
                {
                    fatalError(flow, false);
                }
            }

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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            TryBlockStack other = obj as TryBlockStack;
            if (other == this)
                return true;

			if (other == null)
				return base.Equals(other);

            if (obj != null)
            {
                if (this.blocks.Count != other.blocks.Count)
                    return false;
                for (int i = 0; i < this.blocks.Count; i++)
                {
                    var thisBlocks = this.blocks[i].ToList();
                    var otherBlocks = this.blocks[i].ToList();
                    if (thisBlocks.Count() != otherBlocks.Count())
                        return false;
                    for (int j = 0; j < thisBlocks.Count(); j++)
                    {
                        if (!thisBlocks[j].Equals(otherBlocks[j]))
                            return false;
                    }

                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int res = 0;
            foreach (var i in blocks)
            {
                foreach (var j in i)
                {
                    res += j.GetHashCode();
                }
            }
            return res;
        }

    }

}