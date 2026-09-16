using Godot;

namespace BridgeTroll
{
    public partial class Gold : NonUniqueGood
    {
        public Gold() : this(1) { }

        public Gold(int amount)
        {
            Type = ItemType.GOLD;
            ItemName = "Gold";
            Description = "Standard shiny currency used for tolls, bribes, and trade.";
            MaxStackSize = 99999;
            Quantity = amount;
            Name = "Gold";
        }
    }
}
