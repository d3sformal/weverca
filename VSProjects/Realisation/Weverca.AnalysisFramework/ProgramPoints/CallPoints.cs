using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.ProgramPoints
{
    /// <summary>
    /// Function call representation
    /// </summary>
    public class FunctionCallPoint : RCallPoint
    {
        public readonly DirectFcnCall FunctionCall;

        public override LangElement Partial { get { return FunctionCall; } }

        internal FunctionCallPoint(DirectFcnCall functionCall, RValuePoint thisObj, RValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
            NeedsFunctionResolver = true;

            FunctionCall = functionCall;
        }

        protected override void flowThrough()
        {
            PrepareArguments();

            if (ThisObj == null)
            {
                Services.FunctionResolver.Call(FunctionCall.QualifiedName, Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.MethodCall(ThisObj.Value, FunctionCall.QualifiedName, Flow.Arguments);
            }
        }
    }



    public class IndirectFunctionCallPoint : RCallPoint
    {
        public readonly IndirectFcnCall FunctionCall;

        /// <summary>
        /// Indirect name of call
        /// </summary>
        public readonly RValuePoint Name;

        public override LangElement Partial { get { return FunctionCall; } }

        internal IndirectFunctionCallPoint(IndirectFcnCall functionCall, RValuePoint name, RValuePoint thisObj, RValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
            NeedsFunctionResolver = true;

            Name = name;
            FunctionCall = functionCall;
        }

        protected override void flowThrough()
        {
            PrepareArguments();
            if (ThisObj == null)
            {
                Services.FunctionResolver.IndirectCall(Name.Value, Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.IndirectMethodCall(ThisObj.Value, Name.Value, Flow.Arguments);
            }
        }
    }

}
