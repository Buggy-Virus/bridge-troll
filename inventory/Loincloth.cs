using Godot;

namespace BridgeTroll
{
    public partial class Loincloth : UniqueGood
    {
        [Export]
        public int Defense { get; set; } = 1;

        [Export]
        public string Color { get; set; } = "Brown";

        [Export]
        public string Material { get; set; } = "Leather";

        public Loincloth() : this("Ragged Loincloth", "Brown", 1, "Leather") { }

        public Loincloth(string customName, string color = "Brown", int defense = 1, string material = "Leather")
        {
            Type = ItemType.LOINCLOTH;
            ItemName = "Loincloth";
            CustomName = customName;
            Description = "A simple garment providing minimal protection and modest dignity.";
            Color = color;
            Defense = defense;
            Material = material;
            Durability = 50;
            MaxDurability = 50;
            Name = string.IsNullOrWhiteSpace(customName) ? "Loincloth" : customName;
        }
    }
}
