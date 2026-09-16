using System;
using System.Collections.Generic;
using Godot;

namespace BridgeTroll
{
    public partial class Inventory : Node
    {
        [Signal]
        public delegate void InventoryChangedEventHandler();

        [Signal]
        public delegate void SlotChangedEventHandler(int slotIndex);

        [Signal]
        public delegate void ItemAddedEventHandler(Item item, int quantity);

        [Signal]
        public delegate void ItemRemovedEventHandler(Item item, int quantity);

        [Export]
        public int SlotCount
        {
            get => _slotCount;
            set
            {
                if (value > 0)
                {
                    _slotCount = value;
                    ResizeSlots(_slotCount);
                }
            }
        }

        private int _slotCount = 8;
        private InventorySlot[] _slots;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public Inventory()
        {
            InitializeSlots(_slotCount);
        }

        public Inventory(int slotCount)
        {
            _slotCount = Math.Max(1, slotCount);
            InitializeSlots(_slotCount);
        }

        public override void _Ready()
        {
            if (_slots == null || _slots.Length == 0)
            {
                InitializeSlots(_slotCount);
            }

            // Register any Item child nodes already in the scene tree into available slots
            foreach (Node child in GetChildren())
            {
                if (child is Item item && !ContainsItemInstance(item))
                {
                    AddItem(item);
                }
            }
        }

        private void InitializeSlots(int count)
        {
            _slots = new InventorySlot[count];
            for (int i = 0; i < count; i++)
            {
                _slots[i] = new InventorySlot(i);
            }
        }

        private void ResizeSlots(int newCount)
        {
            if (_slots == null)
            {
                InitializeSlots(newCount);
                return;
            }

            if (newCount == _slots.Length)
            {
                return;
            }

            InventorySlot[] newSlots = new InventorySlot[newCount];
            int copyCount = Math.Min(_slots.Length, newCount);
            for (int i = 0; i < copyCount; i++)
            {
                newSlots[i] = _slots[i];
            }
            for (int i = copyCount; i < newCount; i++)
            {
                newSlots[i] = new InventorySlot(i);
            }

            // Any items in slots truncated by shrinking must be cleaned up
            for (int i = newCount; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    _slots[i].Item.QueueFree();
                    _slots[i].Clear();
                }
            }

            _slots = newSlots;
            EmitSignal(SignalName.InventoryChanged);
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                return null;
            }
            return _slots[index];
        }

        public int GetFirstEmptySlotIndex()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool ContainsItemInstance(Item item)
        {
            if (item == null) return false;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item == item)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetItemCount(ItemType type)
        {
            int total = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].Item.Type == type)
                {
                    total += _slots[i].Item.Quantity;
                }
            }
            return total;
        }

        public bool AddItem(Item item)
        {
            return AddItem(item, out _);
        }

        public bool AddItem(Item item, out int remainingQuantity)
        {
            remainingQuantity = 0;
            if (item == null)
            {
                return false;
            }

            if (item.IsUnique)
            {
                int emptySlotIndex = GetFirstEmptySlotIndex();
                if (emptySlotIndex == -1)
                {
                    remainingQuantity = 1;
                    return false;
                }

                _slots[emptySlotIndex].Item = item;
                AttachItemNode(item);
                EmitSignal(SignalName.SlotChanged, emptySlotIndex);
                EmitSignal(SignalName.ItemAdded, item, 1);
                EmitSignal(SignalName.InventoryChanged);
                return true;
            }
            else
            {
                int amountToAdd = item.Quantity;

                // 1. Try filling existing non-full stacks of the same type
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (amountToAdd <= 0) break;

                    InventorySlot slot = _slots[i];
                    if (!slot.IsEmpty && !slot.Item.IsUnique && slot.Item.Type == item.Type)
                    {
                        int space = slot.GetRemainingCapacity();
                        if (space > 0)
                        {
                            int add = Math.Min(space, amountToAdd);
                            slot.Item.Quantity += add;
                            amountToAdd -= add;
                            EmitSignal(SignalName.SlotChanged, i);
                        }
                    }
                }

                // 2. If amount remaining, allocate new empty slots
                while (amountToAdd > 0)
                {
                    int emptySlotIndex = GetFirstEmptySlotIndex();
                    if (emptySlotIndex == -1)
                    {
                        break;
                    }

                    int stackCapacity = item.MaxStackSize;
                    int add = Math.Min(stackCapacity, amountToAdd);

                    if (item.GetParent() == null && amountToAdd == item.Quantity)
                    {
                        // Direct assignment of original item node
                        item.Quantity = add;
                        _slots[emptySlotIndex].Item = item;
                        AttachItemNode(item);
                    }
                    else
                    {
                        // Create a new node instance of this item type for the new slot
                        NonUniqueGood newStack = CreateNonUniqueInstance(item.Type, add);
                        _slots[emptySlotIndex].Item = newStack;
                        AttachItemNode(newStack);
                    }

                    amountToAdd -= add;
                    EmitSignal(SignalName.SlotChanged, emptySlotIndex);
                }

                // If original item was completely merged into existing stacks and is not in a slot, free it
                if (amountToAdd <= 0 && !ContainsItemInstance(item))
                {
                    item.QueueFree();
                }

                remainingQuantity = amountToAdd;
                int added = item.Quantity - remainingQuantity;
                if (added > 0)
                {
                    EmitSignal(SignalName.ItemAdded, item, added);
                    EmitSignal(SignalName.InventoryChanged);
                }

                return remainingQuantity == 0;
            }
        }

        public bool RemoveItem(Item item)
        {
            if (item == null) return false;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item == item)
                {
                    _slots[i].Clear();
                    if (item.GetParent() == this)
                    {
                        RemoveChild(item);
                    }
                    item.QueueFree();
                    EmitSignal(SignalName.SlotChanged, i);
                    EmitSignal(SignalName.ItemRemoved, item, item.Quantity);
                    EmitSignal(SignalName.InventoryChanged);
                    return true;
                }
            }
            return false;
        }

        public int RemoveItemType(ItemType type, int amount)
        {
            if (amount <= 0) return 0;

            int removedTotal = 0;
            for (int i = _slots.Length - 1; i >= 0; i--)
            {
                if (removedTotal >= amount) break;

                InventorySlot slot = _slots[i];
                if (!slot.IsEmpty && slot.Item.Type == type)
                {
                    int needed = amount - removedTotal;
                    int toRemove = Math.Min(needed, slot.Item.Quantity);

                    slot.Item.Quantity -= toRemove;
                    removedTotal += toRemove;

                    if (slot.Item.Quantity <= 0)
                    {
                        Item removedItem = slot.Item;
                        slot.Clear();
                        if (removedItem.GetParent() == this)
                        {
                            RemoveChild(removedItem);
                        }
                        removedItem.QueueFree();
                    }

                    EmitSignal(SignalName.SlotChanged, i);
                }
            }

            if (removedTotal > 0)
            {
                EmitSignal(SignalName.InventoryChanged);
            }

            return removedTotal;
        }

        public bool TransferSlotTo(Inventory target, int sourceSlotIndex, int amount = 1)
        {
            if (target == null || sourceSlotIndex < 0 || sourceSlotIndex >= _slots.Length)
            {
                return false;
            }

            InventorySlot sourceSlot = _slots[sourceSlotIndex];
            if (sourceSlot.IsEmpty)
            {
                return false;
            }

            Item item = sourceSlot.Item;

            if (item.IsUnique)
            {
                int targetSlot = target.GetFirstEmptySlotIndex();
                if (targetSlot == -1)
                {
                    return false; // Target full
                }

                sourceSlot.Clear();
                if (item.GetParent() == this)
                {
                    item.Reparent(target);
                }
                else
                {
                    target.AttachItemNode(item);
                }

                target._slots[targetSlot].Item = item;

                EmitSignal(SignalName.SlotChanged, sourceSlotIndex);
                EmitSignal(SignalName.ItemRemoved, item, 1);
                EmitSignal(SignalName.InventoryChanged);

                target.EmitSignal(SignalName.SlotChanged, targetSlot);
                target.EmitSignal(SignalName.ItemAdded, item, 1);
                target.EmitSignal(SignalName.InventoryChanged);

                return true;
            }
            else
            {
                int toTransfer = Math.Min(amount, item.Quantity);
                if (toTransfer <= 0) return false;

                NonUniqueGood transferNode = CreateNonUniqueInstance(item.Type, toTransfer);
                if (target.AddItem(transferNode, out int remaining))
                {
                    // Transferred all
                    item.Quantity -= toTransfer;
                    if (item.Quantity <= 0)
                    {
                        sourceSlot.Clear();
                        if (item.GetParent() == this)
                        {
                            RemoveChild(item);
                        }
                        item.QueueFree();
                    }

                    EmitSignal(SignalName.SlotChanged, sourceSlotIndex);
                    EmitSignal(SignalName.ItemRemoved, item, toTransfer);
                    EmitSignal(SignalName.InventoryChanged);
                    return true;
                }
                else
                {
                    int actuallyAdded = toTransfer - remaining;
                    if (actuallyAdded > 0)
                    {
                        item.Quantity -= actuallyAdded;
                        if (item.Quantity <= 0)
                        {
                            sourceSlot.Clear();
                            if (item.GetParent() == this)
                            {
                                RemoveChild(item);
                            }
                            item.QueueFree();
                        }

                        EmitSignal(SignalName.SlotChanged, sourceSlotIndex);
                        EmitSignal(SignalName.ItemRemoved, item, actuallyAdded);
                        EmitSignal(SignalName.InventoryChanged);
                        return true;
                    }

                    // Nothing could be transferred (target completely full)
                    transferNode.QueueFree();
                    return false;
                }
            }
        }

        public void TransferAllTo(Inventory target)
        {
            if (target == null) return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    TransferSlotTo(target, i, _slots[i].Quantity);
                }
            }
        }

        // --- Gold specific helpers for mob / player compatibility ---

        public int GetGold()
        {
            return GetItemCount(ItemType.GOLD);
        }

        public void SetGold(int amount)
        {
            int current = GetGold();
            if (amount == current) return;

            if (amount > current)
            {
                AddGold(amount - current);
            }
            else
            {
                RemoveGold(current - amount);
            }
        }

        public bool AddGold(int amount)
        {
            if (amount <= 0) return false;
            Gold goldItem = new Gold(amount);
            return AddItem(goldItem);
        }

        public int RemoveGold(int amount)
        {
            return RemoveItemType(ItemType.GOLD, amount);
        }

        public bool TransferGold(Inventory target, int amount)
        {
            if (target == null || amount <= 0) return false;

            int currentGold = GetGold();
            int toTransfer = Math.Min(amount, currentGold);
            if (toTransfer <= 0) return false;

            if (target.AddGold(toTransfer))
            {
                RemoveGold(toTransfer);
                return true;
            }

            return false;
        }

        // --- Internal Node management ---

        private void AttachItemNode(Item item)
        {
            if (item.GetParent() == null)
            {
                AddChild(item);
            }
            else if (item.GetParent() != this)
            {
                item.Reparent(this);
            }
        }

        private NonUniqueGood CreateNonUniqueInstance(ItemType type, int quantity)
        {
            switch (type)
            {
                case ItemType.GOLD:
                    return new Gold(quantity);
                default:
                    throw new ArgumentException($"Unknown non-unique item type: {type}");
            }
        }
    }
}
