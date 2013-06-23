using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.ControlFlowGraph;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class DeclarationResolver
    {
        public FlowControler Flow { get; private set; }
        public LangElement Element { get; private set; }

        internal void SetContext(FlowControler flow, LangElement element)
        {
            Flow = flow;
            Element = element;
        }
            
        /// <summary>
        /// Return possible names of function which is identified by given functionName
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public abstract string[] GetFunctionNames(MemoryEntry functionName);

        public abstract FlowInputSet PrepareCallInput(FunctionDecl function, MemoryEntry[] args);

        public abstract BasicBlock GetEntryPoint(FunctionDecl function);

        public abstract void CallDispatch(QualifiedName name, MemoryEntry[] args);

        public abstract void CallDispatch(MemoryEntry functionName, MemoryEntry[] args);

    }
}
