using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Function call representation
    /// </summary>
    public class FunctionCallPoint : RCallPoint
    {
        public readonly DirectFcnCall FunctionCall;

        public override LangElement Partial { get { return FunctionCall; } }

        internal FunctionCallPoint(DirectFcnCall functionCall, ValuePoint thisObj, ValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
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
                Services.FunctionResolver.MethodCall(ThisObj.Value.ReadMemory(InSet.Snapshot), FunctionCall.QualifiedName, Flow.Arguments);
            }
        }

        protected override void extendOutput()
        {
            OutSet.StartTransaction();
            //TODO change call handling
            OutSet.ExtendAsCall(_inSet, Flow.CalledObject, Flow.Arguments);
            //  OutSet.ExtendAsCall(InSet, Flow.CalledObject, Flow.Arguments);
        }
    }



    public class IndirectFunctionCallPoint : RCallPoint
    {
        public readonly IndirectFcnCall FunctionCall;

        /// <summary>
        /// Indirect name of call
        /// </summary>
        public readonly ValuePoint Name;

        public override LangElement Partial { get { return FunctionCall; } }

        internal IndirectFunctionCallPoint(IndirectFcnCall functionCall, ValuePoint name, ValuePoint thisObj, ValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
            Name = name;
            FunctionCall = functionCall;
        }

        protected override void flowThrough()
        {
            PrepareArguments();
            if (ThisObj == null)
            {
                Services.FunctionResolver.IndirectCall(Name.Value.ReadMemory(InSet.Snapshot), Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.IndirectMethodCall(ThisObj.Value.ReadMemory(InSet.Snapshot), Name.Value.ReadMemory(InSet.Snapshot), Flow.Arguments);
            }
        }

        protected override void extendOutput()
        {
            OutSet.StartTransaction();
            //TODO change call handling
            OutSet.ExtendAsCall(_inSet, Flow.CalledObject, Flow.Arguments);
            //  OutSet.ExtendAsCall(InSet, Flow.CalledObject, Flow.Arguments);
        }
    }

}
