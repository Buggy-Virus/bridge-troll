using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class DayMode : Node2D
    {
        [Export]
        public PackedScene mob_packed_scene_ { get; set; }
        public Troll troll_;

        private Node2D mobs_parent;

        private PathFollow2D left_spawn_;
        private PathFollow2D right_spawn_;

        private DebugMob virtual_mob_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            troll_ = GetNode<Troll>("Troll");

            left_spawn_ = GetNode<PathFollow2D>("LeftSide/PathFollow2D");
            right_spawn_ = GetNode<PathFollow2D>("RightSide/PathFollow2D");
            mobs_parent = GetNode<Node2D>("Mobs");

            virtual_mob_ = mob_packed_scene_.Instantiate<DebugMob>();
            AddChild(virtual_mob_);
            virtual_mob_.Visible = false;
            virtual_mob_.collision_shape.Disabled = true;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }

        private void _on_step_timer_timeout()
        {
            if (GD.Randf() < virtual_mob_.spawn_probability)
            {
                DebugMob new_mob = mob_packed_scene_.Instantiate<DebugMob>();
                mobs_parent.AddChild(new_mob);
                new_mob.troll = troll_;

                if (GD.Randf() > 0.5f)
                {
                    left_spawn_.ProgressRatio = GD.Randf();
                    new_mob.Position = left_spawn_.Position;
                    new_mob.SetTarget(new Vector2(right_spawn_.Position.X, new_mob.Position.Y));
                }
                else
                {
                    right_spawn_.ProgressRatio = GD.Randf();
                    new_mob.Position = right_spawn_.Position;
                    new_mob.SetTarget(new Vector2(left_spawn_.Position.X, new_mob.Position.Y));
                }
            }
        }
    }
}
