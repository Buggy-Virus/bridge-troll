using System;
using Godot;

namespace BridgeTroll
{
    public abstract partial class UniqueGood : Item
    {
        [Export]
        public string UniqueId { get; protected set; } = Guid.NewGuid().ToString();

        [Export]
        public string CustomName { get; set; } = string.Empty;

        [Export]
        public int Durability { get; set; } = 100;

        [Export]
        public int MaxDurability { get; set; } = 100;

        public override bool IsUnique => true;

        public override int Quantity
        {
            get => 1;
            set { } // Unique goods are always single items
        }

        public UniqueGood()
        {
            MaxStackSize = 1;
        }
    }
}
