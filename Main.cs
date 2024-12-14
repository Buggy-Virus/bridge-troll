using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public enum GameState
    {
        DAY_MODE,
        NIGHT_MODE,
        BUILD_MODE,
        MAIN_MENU,
    }

    public partial class Main : Node2D
    {
        public GameBoard game_board;
        private BuildMode build_mode_;
        private DayMode day_mode_;

        private GameState active_state_;
        private Node2D active_mode_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            game_board = GetNode<GameBoard>("GameBoard");
            day_mode_ = GetNode<DayMode>("DayMode");
            build_mode_ = GetNode<BuildMode>("BuildMode");
            DisableNode(day_mode_);
            DisableNode(build_mode_);

            EnableBuildMode();
        }

        private void DisableNode(Node2D node)
        {
            if (node != null)
            {
                node.Visible = false;
                node.SetProcess(false);
                node.SetProcessInput(false);
                node.SetPhysicsProcess(false);
            }
        }

        private void EnableNode(Node2D node)
        {
            node.Visible = true;
            node.SetProcess(true);
            node.SetProcessInput(true);
            node.SetPhysicsProcess(true);
        }

        private void EnableBuildMode()
        {
            GD.Print("Build Mode");
            active_state_ = GameState.BUILD_MODE;
            DisableNode(active_mode_);
            EnableNode(build_mode_);
            active_mode_ = build_mode_;
        }

        private void EnableDayMode()
        {
            GD.Print("Day Mode");
            active_state_ = GameState.DAY_MODE;
            DisableNode(active_mode_);
            EnableNode(day_mode_);
            active_mode_ = day_mode_;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("build_mode") && active_state_ != GameState.BUILD_MODE)
            {
                EnableBuildMode();
            }
            else if (@event.IsActionPressed("day_mode") && active_state_ != GameState.DAY_MODE)
            {
                EnableDayMode();
            }
        }
    }
}
