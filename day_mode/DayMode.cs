using System;
using BridgeTroll;
using Godot;
using Godot.Collections;

namespace BridgeTroll
{
    enum DayState
    {
        NONE = 0,
        NO_SELECTION,
        TROLL_SELECTED,
        TROLL_GRAPPLING,
        MOB_SELECTED,
        ALLY_SELECTED,
        BUILDING_SELECTED,
        TROLL_FIGHTING,
    }

    public partial class DayMode : Node2D
    {
        [Export]
        public PackedScene debug_mob_packed_scene { get; set; }

        [Export]
        public PackedScene war_mob_packed_scene { get; set; }
        public Dictionary<MobType, float> spawn_probabilities = new()
        {
            { MobType.DEBUG, 0.3f },
            { MobType.WAR_MOB, 0.3f },
        };
        public Dictionary<MobType, PackedScene> mob_packed_scenes = new();

        private void BuildMobPackedScenes()
        {
            mob_packed_scenes = new()
            {
                { MobType.DEBUG, debug_mob_packed_scene },
                { MobType.WAR_MOB, war_mob_packed_scene },
            };
        }

        private PackedScene GetMobPackedScene(MobType mob_type)
        {
            return mob_packed_scenes[mob_type];
        }

        private Node2D mobs_parent;

        public Troll troll_;

        private PathFollow2D left_spawn_;
        private PathFollow2D right_spawn_;

        private MouseArea mouse_area_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            troll_ = GetNode<Troll>("Troll");

            left_spawn_ = GetNode<PathFollow2D>("LeftSide/PathFollow2D");
            right_spawn_ = GetNode<PathFollow2D>("RightSide/PathFollow2D");
            mobs_parent = GetNode<Node2D>("Mobs");
            mouse_area_ = GetNode<MouseArea>("MouseArea");

            BuildMobPackedScenes();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("left_click"))
            {
                foreach (Area2D overlapping_area in mouse_area_.GetOverlappingAreas())
                {
                    GD.Print("Overlapped: ", overlapping_area);
                    if (overlapping_area.GetParent().IsInGroup("Mobs"))
                    {
                        GD.Print(overlapping_area.GetParent());
                    }
                }
            }
            else if (@event.IsActionPressed("right_click"))
            {
                foreach (Area2D overlapping_area in mouse_area_.GetOverlappingAreas())
                {
                    if (overlapping_area.GetParent().IsInGroup("NPCs"))
                    {
                        troll_.StartHuntingCharacter((Character)overlapping_area.GetParent());
                        return;
                    }
                }

                troll_.SetTargetPosition(GetGlobalMousePosition());
                troll_.EnterWalkingState();
            }

            if (troll_.state == CharacterState.GRAPPLING)
            {
                if (
                    @event.IsActionPressed("extort")
                    && troll_.decision_list.Contains(VictimOption.EXTORT)
                )
                {
                    troll_.StartExtortCharacter(troll_.grappled_character);
                }
                else if (
                    @event.IsActionPressed("mug") && troll_.decision_list.Contains(VictimOption.MUG)
                )
                {
                    troll_.StartMugCharacter(troll_.grappled_character);
                }
                else if (
                    @event.IsActionPressed("eat") && troll_.decision_list.Contains(VictimOption.EAT)
                )
                {
                    troll_.StartEatCharacter(troll_.grappled_character);
                }
                else if (
                    @event.IsActionPressed("release")
                    && troll_.decision_list.Contains(VictimOption.RELEASE)
                )
                {
                    troll_.EnterNoneState();
                }
            }
        }

        public void SpawnMob(MobType mob_type)
        {
            Mob new_mob = GetMobPackedScene(mob_type).Instantiate<Mob>();
            mobs_parent.AddChild(new_mob);

            PathFollow2D spawn_side;
            PathFollow2D other_side;
            if (GD.Randf() > 0.5f)
            {
                spawn_side = left_spawn_;
                other_side = right_spawn_;
            }
            else
            {
                spawn_side = right_spawn_;
                other_side = left_spawn_;
            }
            spawn_side.ProgressRatio = GD.Randf();
            new_mob.Position = spawn_side.Position;
            new_mob.SetTargetPosition(new Vector2(other_side.Position.X, new_mob.Position.Y));
        }

        private void _on_spawn_timer_timeout()
        {
            foreach (var spawn_rate in spawn_probabilities)
            {
                if (GD.Randf() < spawn_rate.Value)
                {
                    SpawnMob(spawn_rate.Key);
                }
            }
        }
    }
}
