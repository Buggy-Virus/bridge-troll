using Godot;

namespace BridgeTroll
{
    public partial class Wood : NonUniqueGood
    {
        public Wood() : this(1) { }

        public Wood(int amount)
        {
            Type = ItemType.WOOD;
            ItemName = "Wood";
            Description = "Sturdy timber used for building structures and fortifications.";
            MaxStackSize = 99999;
            Quantity = amount;
            Name = "Wood";
        }
    }
}
