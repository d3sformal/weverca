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
    /// Representation of item use ($usedItem[$index]), that can be asked for value
    /// TODO: Needs to implement thisObj support
    /// </summary>
    public class ItemUsePoint : LValuePoint
    {
        public readonly ItemUse ItemUse;

        /// <summary>
        /// Used item value
        /// </summary>
        public readonly ValuePoint UsedItem;

        /// <summary>
        /// Index value representation
        /// </summary>
        public readonly ValuePoint Index;

        public MemberIdentifier IndexIdentifier;

        public override LangElement Partial { get { return ItemUse; } }

        internal ItemUsePoint(ItemUse itemUse, ValuePoint usedItem, ValuePoint index)
        {
            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }

        protected override void flowThrough()
        {
            IndexIdentifier = Services.Evaluator.MemberIdentifier(Index.Value.ReadMemory(InSnapshot));
            LValue = Services.Evaluator.ResolveIndex(UsedItem.Value, IndexIdentifier);
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitItemUse(this);
        }
    }
}
