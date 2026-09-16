using System;

namespace BridgeTroll
{
    public class InventorySlot
    {
        public int SlotIndex { get; }
        public Item Item { get; set; }

        public bool IsEmpty => Item == null;

        public int Quantity => Item?.Quantity ?? 0;

        public InventorySlot(int slotIndex)
        {
            SlotIndex = slotIndex;
            Item = null;
        }

        public bool CanAccept(Item incomingItem)
        {
            if (incomingItem == null)
            {
                return false;
            }

            if (IsEmpty)
            {
                return true;
            }

            // Unique items cannot stack with anything
            if (Item.IsUnique || incomingItem.IsUnique)
            {
                return false;
            }

            // Non-unique items must match type and have room under MaxStackSize
            return Item.Type == incomingItem.Type && Item.Quantity < Item.MaxStackSize;
        }

        public int GetRemainingCapacity()
        {
            if (IsEmpty)
            {
                return int.MaxValue;
            }

            if (Item.IsUnique)
            {
                return 0;
            }

            return Math.Max(0, Item.MaxStackSize - Item.Quantity);
        }

        public void Clear()
        {
            Item = null;
        }
    }
}
