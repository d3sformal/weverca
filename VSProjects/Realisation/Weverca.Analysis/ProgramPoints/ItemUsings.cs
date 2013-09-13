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
    /// Representation of item use ($usedItem[$index]), that can be assigned
    /// TODO: Needs to implement thisObj support
    /// </summary>
    public class LItemUsePoint : LValuePoint
    {
        public readonly ItemUse ItemUse;

        /// <summary>
        /// Used item value
        /// </summary>
        public readonly RValuePoint UsedItem;

        /// <summary>
        /// Index value representation
        /// </summary>
        public readonly RValuePoint Index;

        /// <summary>
        /// Value of index computed during flowThrough
        /// </summary>
        public MemoryEntry IndexedValue { get; private set; }

        public override LangElement Partial { get { return ItemUse; } }

        internal LItemUsePoint(ItemUse itemUse, RValuePoint usedItem, RValuePoint index)
        {
            NeedsExpressionEvaluator = true;

            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }


        public override void Assign(FlowController flow, MemoryEntry entry)
        {
            flow.Services.Evaluator.IndexAssign(IndexedValue, Index.Value, entry);
        }

        protected override void flowThrough()
        {
            var initializable = UsedItem as ItemUseable;
            if (initializable != null)
            {
                IndexedValue = initializable.ItemUseValue(Flow);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void AssignAlias(FlowController flow, IEnumerable<AliasValue> aliases)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Representation of item use ($usedItem[$index]), that can be asked for value
    /// TODO: Needs to implement thisObj support
    /// </summary>
    public class RItemUsePoint : RValuePoint
    {
        public readonly ItemUse ItemUse;

        /// <summary>
        /// Used item value
        /// </summary>
        public readonly RValuePoint UsedItem;

        /// <summary>
        /// Index value representation
        /// </summary>
        public readonly RValuePoint Index;

        public override LangElement Partial { get { return ItemUse; } }

        internal RItemUsePoint(ItemUse itemUse, RValuePoint usedItem, RValuePoint index)
        {
            NeedsExpressionEvaluator = true;

            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }

        protected override void flowThrough()
        {
            Value = Services.Evaluator.ResolveIndex(UsedItem.Value, Index.Value);
        }

    }
}
