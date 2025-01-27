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
        [Signal]
        public delegate void DayEndedEventHandler();

        private PlayerData player_data_;

        [Export]
        public PackedScene debug_player_data_scene { get; set; }

        [Export]
        public PackedScene debug_game_board_scene { get; set; }

        [Export]
        public PackedScene peasant_scene { get; set; }

        [Export]
        public PackedScene thug_scene { get; set; }
        public Dictionary<Mob.Type, float> spawn_probabilities = new()
        {
            { Mob.Type.PEASANT, 0.5f },
            { Mob.Type.THUG, 0.3f },
        };
        public Dictionary<Mob.Type, PackedScene> mob_packed_scenes = new();

        private void BuildMobPackedScenes()
        {
            mob_packed_scenes = new()
            {
                { Mob.Type.PEASANT, peasant_scene },
                { Mob.Type.THUG, thug_scene },
            };
        }

        private PackedScene GetMobPackedScene(Mob.Type mob_type)
        {
            return mob_packed_scenes[mob_type];
        }

        private Node2D mobs_parent;

        public Troll troll_;

        private PathFollow2D left_spawn_;
        private PathFollow2D right_spawn_;

        private MouseArea mouse_area_;

        private Timer spawn_timer_;
        private Timer day_timer_;

        private SummaryPopup summary_popup_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Node parent = GetParent();
            if (parent is not Main)
            {
                GameBoard game_board = debug_game_board_scene.Instantiate<GameBoard>();
                AddChild(game_board);
                MoveChild(game_board, 0);
            }

            troll_ = GetNode<Troll>("Troll");

            left_spawn_ = GetNode<PathFollow2D>("LeftSide/PathFollow2D");
            right_spawn_ = GetNode<PathFollow2D>("RightSide/PathFollow2D");
            mobs_parent = GetNode<Node2D>("Mobs");
            mouse_area_ = GetNode<MouseArea>("MouseArea");
            spawn_timer_ = GetNode<Timer>("SpawnTimer");
            day_timer_ = GetNode<Timer>("DayTimer");
            summary_popup_ = GetNode<SummaryPopup>("SummaryPopup");

            day_timer_.Timeout += OnDayTimerTimeout;
            summary_popup_.ok_button.Pressed += OnSummaryPopupOkButtonPressed;

            CloseSummaryPopup();

            BuildMobPackedScenes();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }

        public void BeginDay()
        {
            CloseSummaryPopup();
            spawn_timer_.Start();
            day_timer_.Start();
        }

        public void EndDay()
        {
            foreach (Node2D mob in mobs_parent.GetChildren())
            {
                mob.QueueFree();
            }

            EmitSignal(SignalName.DayEnded);
        }

        private void OpenSummaryPopup()
        {
            summary_popup_.Visible = true;
            summary_popup_.SetProcessInput(true);
        }

        private void CloseSummaryPopup()
        {
            summary_popup_.Visible = false;
            summary_popup_.SetProcessInput(false);
        }

        private bool PressedValidAction(InputEvent @event, VictimOption option)
        {
            return @event.IsActionPressed(option.ToString())
                && troll_.decision_list.Contains(option);
        }

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

            // TODO: Could get rid of this grossness by making a dictionary from
            // VictimOption to the relevant method.
            if (troll_.state == CharacterState.GRAPPLING)
            {
                if (PressedValidAction(@event, VictimOption.EXTORT))
                {
                    troll_.StartExtortCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.MUG))
                {
                    troll_.StartMugCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.PRESS))
                {
                    troll_.StartPressCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.ROB))
                {
                    troll_.StartRobCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.EAT))
                {
                    troll_.StartEatCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.ACCEPT))
                {
                    troll_.StartAcceptCharacter(troll_.victim_character);
                }
                else if (PressedValidAction(@event, VictimOption.RELEASE))
                {
                    troll_.StartReleaseCharacter(troll_.victim_character);
                }
            }
        }

        public void SpawnMob(Mob.Type mob_type)
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

        private void OnDayTimerTimeout()
        {
            spawn_timer_.Stop();
            OpenSummaryPopup();
        }

        private void OnSummaryPopupOkButtonPressed()
        {
            EndDay();
        }
    }
}
