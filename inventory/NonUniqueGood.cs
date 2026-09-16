using Godot;

namespace BridgeTroll
{
    public abstract partial class NonUniqueGood : Item
    {
        private int _quantity = 1;

        [Export]
        public override int Quantity
        {
            get => _quantity;
            set => _quantity = value;
        }

        public override bool IsUnique => false;

        public NonUniqueGood()
        {
            MaxStackSize = 9999;
        }
    }
}
