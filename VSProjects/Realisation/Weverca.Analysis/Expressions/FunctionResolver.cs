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
    public abstract class FunctionResolver
    {   
        /// <summary>
        /// Return possible names of function which is identified by given functionName
        /// </summary>
        /// <param name="possibleFunctionNames"></param>
        /// <returns></returns>
        public abstract QualifiedName[] GetFunctionNames(MemoryEntry possibleFunctionNames);


        public abstract ProgramPointGraph InitializeCall(FlowOutputSet callInput, QualifiedName name);

        public abstract MemoryEntry ResolveReturnValue(ProgramPointGraph[] programPointGraph);
    }
}
