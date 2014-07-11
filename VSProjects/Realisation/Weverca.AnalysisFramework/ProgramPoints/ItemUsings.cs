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
        /// <summary>
        /// Item use element represented by current point
        /// </summary>
        public readonly ItemUse ItemUse;

        /// <summary>
        /// Used item value
        /// </summary>
        public readonly ValuePoint UsedItem;

        /// <summary>
        /// Index value representation
        /// </summary>
        public readonly ValuePoint Index;

        /// <summary>
        /// Identifier of index available for item using
        /// </summary>
        public MemberIdentifier IndexIdentifier;

        /// <inheritdoc />
        public override LangElement Partial { get { return ItemUse; } }

        internal ItemUsePoint(ItemUse itemUse, ValuePoint usedItem, ValuePoint index)
        {
            ItemUse = itemUse;
            UsedItem = usedItem;
            Index = index;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
			if (Index == null) 
			{
				// TODO: workaround, not correct!!
				// converts the expression $a[] to $a[0]
				// this is not correct, $a[] should be converted to $a[largest index of $a]
				IndexIdentifier = Services.Evaluator.MemberIdentifier(new MemoryEntry(OutSnapshot.CreateInt(0)));
			}
			else 
			{
            	IndexIdentifier = Services.Evaluator.MemberIdentifier(Index.Value.ReadMemory(OutSnapshot));
			}
            LValue = Services.Evaluator.ResolveIndex(UsedItem.Value, IndexIdentifier);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitItemUse(this);
        }
    }
}
