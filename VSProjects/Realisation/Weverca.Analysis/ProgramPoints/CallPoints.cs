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
    public class FunctionCallPoint : RValuePoint
    {
        public readonly DirectFcnCall FunctionCall;

        /// <summary>
        /// Arguments specified for call
        /// </summary>
        public IEnumerable<RValuePoint> Arguments { get { return _arguments; } }

        /// <summary>
        /// This object for call
        /// <remarks>If there is no this object, is null</remarks>
        /// </summary>
        public readonly RValuePoint ThisObj;

        public override LangElement Partial { get { return FunctionCall; } }

        private readonly RValuePoint[] _arguments;

        public override MemoryEntry Value
        {
            get
            {
                //Return value is obtained from sink
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal FunctionCallPoint(DirectFcnCall functionCall, RValuePoint baseObj, RValuePoint[] arguments)
        {
            NeedsFunctionResolver = true;

            ThisObj = baseObj;
            FunctionCall = functionCall;
            _arguments = arguments;
        }

        protected override void flowThrough()
        {
            PrepareArguments(ThisObj, _arguments, Flow);

            if (ThisObj == null)
            {
                Services.FunctionResolver.Call(FunctionCall.QualifiedName, Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.MethodCall(ThisObj.Value, FunctionCall.QualifiedName, Flow.Arguments);
            }
        }

        internal static void PrepareArguments(RValuePoint thisObj, RValuePoint[] arguments, FlowController flow)
        {
            //TODO better argument handling avoid copying values
            var argumentValues = new MemoryEntry[arguments.Length];
            for (int i = 0; i < arguments.Length; ++i)
            {
                argumentValues[i] = arguments[i].Value;
            }

            if (thisObj != null)
            {
                flow.CalledObject = thisObj.Value;
            }
            flow.Arguments = argumentValues;
        }
    }

   

    public class IndirectFunctionCallPoint : RValuePoint
    {
        public readonly IndirectFcnCall FunctionCall;

        /// <summary>
        /// Indirect name of call
        /// </summary>
        public readonly RValuePoint Name;

        /// <summary>
        /// Arguments specified for call
        /// </summary>
        public IEnumerable<RValuePoint> Arguments { get { return _arguments; } }

        /// <summary>
        /// This object for call
        /// <remarks>If there is no this object, is null</remarks>
        /// </summary>
        public readonly RValuePoint ThisObj;
        
        public override LangElement Partial { get { return FunctionCall; } }

        private readonly RValuePoint[] _arguments;

        public override MemoryEntry Value
        {
            get
            {
                //Return value is obtained from sink
                return Extension.Sink.Value;
            }
            protected set
            {
                throw new NotSupportedException("Cannot set value of call node");
            }
        }

        internal IndirectFunctionCallPoint(IndirectFcnCall functionCall, RValuePoint name, RValuePoint[] arguments)
        {
            NeedsFunctionResolver = true;

            Name = name;
            FunctionCall = functionCall;
            _arguments = arguments;
        }

        protected override void flowThrough()
        {
            FunctionCallPoint.PrepareArguments(ThisObj, _arguments, Flow);
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
