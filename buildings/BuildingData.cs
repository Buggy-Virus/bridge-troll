using Godot;

namespace BridgeTroll
{
    [GlobalClass]
    public partial class BuildingData : Resource
    {
        [Export]
        public string Id { get; set; } = "";

        [Export]
        public string DisplayName { get; set; } = "";

        [Export]
        public Building.Type BuildingType { get; set; } = Building.Type.DEBUG;

        [Export]
        public int WoodCost { get; set; } = 10;

        [Export]
        public float LaborStrength { get; set; } = 20f;

        [Export]
        public Vector2I FootprintSize { get; set; } = Vector2I.One;

        [Export]
        public PackedScene BuildingScene { get; set; }

        [Export]
        public Texture2D Icon { get; set; }

        [Export]
        public bool IsUnlockedByDefault { get; set; } = false;

        public LaborCost LaborCost => new LaborCost(LaborStrength);

        public BuildingData() { }

        public BuildingData(string id, string displayName, Building.Type buildingType, int woodCost, float laborStrength, Vector2I footprintSize, PackedScene scene, Texture2D icon = null, bool isUnlockedByDefault = false)
        {
            Id = id;
            DisplayName = displayName;
            BuildingType = buildingType;
            WoodCost = woodCost;
            LaborStrength = laborStrength;
            FootprintSize = footprintSize;
            BuildingScene = scene;
            Icon = icon;
            IsUnlockedByDefault = isUnlockedByDefault;
        }
    }
}
