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
    /// Implemented by Rvalue program points, that can be used as items   ($usedItem[$index])
    /// </summary>
    public interface ItemUseable
    {
        MemoryEntry ItemUseValue(FlowController flow);
    }

    /// <summary>
    /// Implemented by program points, that can create alias values
    /// </summary>
    public interface AliasProvider
    {
        IEnumerable<AliasValue> CreateAlias(FlowController flow);
    }

    /// <summary>
    /// Implemented by program points, that can be resolved as variable entry
    /// </summary>
    public interface VariableBased : AliasProvider
    {
        VariableEntry VariableEntry { get; }
        RValuePoint ThisObj { get; }
    }


    /// <summary>
    /// Base class for RValue program points
    /// <remarks>RValue program points can be asked for MemoryEntry value</remarks>
    /// </summary>
    public abstract class RValuePoint : ProgramPointBase
    {
        public virtual MemoryEntry Value { get; protected set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Base class for LValue program points
    /// <remarks>LValue program points can be assigned by MemoryEntry value or alias</remarks>
    /// </summary>
    public abstract class LValuePoint : ProgramPointBase
    {
        public abstract void Assign(FlowController flow, MemoryEntry entry);
        public abstract void AssignAlias(FlowController flow, IEnumerable<AliasValue> aliases);
    }

    /// <summary>
    /// Base class for Alias program points
    /// <remarks>Alias program points can be assked for AliasValues</remarks>
    /// </summary>
    public abstract class AliasPoint : ProgramPointBase
    {
        public virtual IEnumerable<AliasValue> Aliases { get; protected set; }
    }

    public abstract class RCallPoint : RValuePoint
    {
        /// <summary>
        /// Arguments specified for call - private storage
        /// </summary>
        private readonly RValuePoint[] _arguments;

        /// <summary>
        /// Arguments specified for call
        /// </summary>
        public IEnumerable<RValuePoint> Arguments { get { return _arguments; } }

        public readonly CallSignature? CallSignature;

        /// <summary>
        /// This object for call
        /// <remarks>If there is no this object, is null</remarks>
        /// </summary>
        public readonly RValuePoint ThisObj;

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

        internal RCallPoint(RValuePoint thisObj,CallSignature? callSignature, RValuePoint[] arguments)
        {
            CallSignature = callSignature;
            ThisObj = thisObj;
            _arguments = arguments;
        }

        /// <summary>
        /// Prepare arguments into flow controller
        /// </summary>
        internal void PrepareArguments()
        {
            //TODO better argument handling avoid copying values
            var argumentValues = new MemoryEntry[_arguments.Length];
            for (int i = 0; i < _arguments.Length; ++i)
            {
                argumentValues[i] = _arguments[i].Value;
            }

            if (ThisObj != null)
            {
                Flow.CalledObject = ThisObj.Value;
            }
            Flow.Arguments = argumentValues;
        }
    }

    /// <summary>
    /// Base class for RValue points that are based on variable entry
    /// </summary>
    public abstract class RVariableEntryPoint : RValuePoint, ItemUseable, VariableBased
    {
        public RValuePoint ThisObj { get; private set; }

        public VariableEntry VariableEntry { get; protected set; }

        internal RVariableEntryPoint(RValuePoint thisObj)
        {
            NeedsExpressionEvaluator = true;
            ThisObj = thisObj;
        }

        protected void resolveValue()
        {
            if (ThisObj == null)
            {
                Value = Services.Evaluator.ResolveVariable(VariableEntry);
            }
            else
            {
                Value = Services.Evaluator.ResolveField(ThisObj.Value, VariableEntry);
            }
        }

        public MemoryEntry ItemUseValue(FlowController flow)
        {
            if (ThisObj != null)
            {
                throw new NotImplementedException();
            }
            return flow.Services.Evaluator.ResolveIndexedVariable(VariableEntry);
        }

        public IEnumerable<AliasValue> CreateAlias(FlowController flow)
        {
            if (ThisObj == null)
            {
                return flow.Services.Evaluator.ResolveAlias(VariableEntry);
            }
            else
            {
                return flow.Services.Evaluator.ResolveAliasedField(ThisObj.Value, VariableEntry);
            }
        }
    }

    /// <summary>
    /// Base class for LValue points that are based on VariableEntry
    /// </summary>
    public abstract class LVariableEntryPoint : LValuePoint, VariableBased
    {
        public VariableEntry VariableEntry { get; protected set; }
        public RValuePoint ThisObj { get; protected set; }

        internal LVariableEntryPoint(RValuePoint thisObj)
        {
            ThisObj = thisObj;
        }

        public override void Assign(FlowController flow, MemoryEntry entry)
        {
            if (ThisObj == null)
            {
                flow.Services.Evaluator.Assign(VariableEntry, entry);
            }
            else
            {
                flow.Services.Evaluator.FieldAssign(ThisObj.Value, VariableEntry, entry);
            }
        }

        public override void AssignAlias(FlowController flow, IEnumerable<AliasValue> aliases)
        {
            if (ThisObj == null)
            {
                flow.Services.Evaluator.AliasAssign(VariableEntry, aliases);
            }
            else
            {
                flow.Services.Evaluator.AliasedFieldAssign(ThisObj.Value, VariableEntry, aliases);
            }
        }

        public IEnumerable<AliasValue> CreateAlias(FlowController flow)
        {
            if (ThisObj == null)
            {
                return flow.Services.Evaluator.ResolveAlias(VariableEntry);
            }
            else
            {
                return flow.Services.Evaluator.ResolveAliasedField(ThisObj.Value, VariableEntry);
            }
        }
    }

}
