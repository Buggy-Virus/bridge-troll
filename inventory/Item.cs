using Godot;

namespace BridgeTroll
{
    public abstract partial class Item : Node
    {
        [Export]
        public ItemType Type { get; protected set; } = ItemType.NONE;

        [Export]
        public string ItemName { get; set; } = string.Empty;

        [Export]
        public string Description { get; set; } = string.Empty;

        [Export]
        public int MaxStackSize { get; protected set; } = 1;

        public abstract bool IsUnique { get; }

        public virtual int Quantity
        {
            get => 1;
            set { }
        }
    }
}
