using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class Thug : Mob
    {
        public override void UniqueReady()
        {
            surrender_hit_points = 3;
            is_defensive = true;
        }
    }
}