using System;
using System.Globalization;
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
                // $a[] = value;
                // Creates new index that is biggest integer index in $a + 1 and writes the value to this index

                // TODO: 
                // What is should do
                // Iterate through all arrays in UsedItem
                    // for each array iterate through all indices and find the biggest integer index
                    // for each array resolve the index
                // LValue should represent all these indices

                // What it does
                // Iterate through all indices of all arrays in UsedItem and find the biggest integer index
                // Resolve this index in all arrays

                // TODO: causes non-termination: while() $a[] = 1;
                // Do this only for non-widened arrays. Then write to unknown index
                // Now arrays are not widened => we use just unknown index
                /*
                IndexIdentifier = new MemberIdentifier (System.Convert.ToString(
                    UsedItem.Value.BiggestIntegerIndex(OutSnapshot, Services.Evaluator) + 1, 
                    CultureInfo.InvariantCulture));
                    */

                // use the unknown index
                IndexIdentifier = MemberIdentifier.getUnknownMemberIdentifier();
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
