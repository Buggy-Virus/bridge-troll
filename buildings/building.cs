using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public enum BuildingType
    {
        DEBUG,
        SIGN,
        BARRICADE,
        FENCE,
        WATER_BARREL,
    }

    public partial class Building : Area2D
    {
        public int hit_points;
    }
}
