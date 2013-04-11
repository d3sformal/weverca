using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Expressions
{
    public abstract class DeclarationResolver<FlowInfo>
    {
        public FlowControler<FlowInfo> Flow { get; private set; }
        public LangElement Element { get; private set; }

        internal void SetContext(FlowControler<FlowInfo> flow, LangElement element)
        {
            Flow = flow;
            Element = element;
        }

        public virtual void Declare(FunctionDecl x)
        {
            Flow.OutSet.SetDeclaration(x);
        }

        /// <summary>
        /// Return possible names of function which is identified by given functionName
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public abstract string[] GetFunctionNames(FlowInfo functionName);

        public abstract FlowInputSet<FlowInfo> PrepareCallInput(FunctionDecl function, FlowInfo[] args);

        public abstract BasicBlock GetEntryPoint(FunctionDecl function);

        public virtual void CallDispatch(QualifiedName name, FlowInfo[] args)
        {
            var dispatch = CreateDispatch(name, args);
            if (dispatch != null)
            {
                Flow.AddDispatch(new CallDispatch<FlowInfo>[] { dispatch });
            }
        }

        public virtual void CallDispatch(FlowInfo functionName, FlowInfo[] args)
        {
            var names = GetFunctionNames(functionName);
            if (names == null)
            {
                //cannot dispatch
                return;
            }

            var dispatches = new List<CallDispatch<FlowInfo>>();
            foreach (var name in names)
            {
                //TODO resolve namespaces
                var qualifiedName = new QualifiedName(new Name(name));
                var dispatch = CreateDispatch(qualifiedName, args);
                if (dispatch == null)
                    continue;
                dispatches.Add(dispatch);
            }

            Flow.AddDispatch(dispatches);
        }


        public CallDispatch<FlowInfo> CreateDispatch(QualifiedName name, FlowInfo[] args)
        {
            FunctionDecl decl;
            if (!Flow.InSet.TryGetFunction(name, out  decl))
                //TODO what to do, if declaration isn't found ?
                return null;

            var callInSet = PrepareCallInput(decl, args);
            var functionCFG = GetEntryPoint(decl);

            return new CallDispatch<FlowInfo>(functionCFG, callInSet);
        }

    }
}
