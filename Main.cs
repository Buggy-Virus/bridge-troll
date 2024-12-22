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
        public PlayerData player_data;
        private MainMenu main_menu_;
        private BuildMode build_mode_;
        private DayMode day_mode_;

        private GameState active_state_;
        private Node active_mode_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            game_board = GetNode<GameBoard>("GameBoard");
            player_data = GetNode<PlayerData>("PlayerData");
            main_menu_ = GetNode<MainMenu>("MainMenu");
            day_mode_ = GetNode<DayMode>("DayMode");
            build_mode_ = GetNode<BuildMode>("BuildMode");
            DisableNode2D(day_mode_);
            DisableNode2D(build_mode_);

            EnableMainMenu();
        }

        private void DisableMode(GameState game_state)
        {
            if (active_state_ == GameState.MAIN_MENU)
            {
                DisableMainMenu();
            }
            else
            {
                DisableNode2D((Node2D)active_mode_);
            }
        }

        private void DisableNode2D(Node2D node)
        {
            if (node != null)
            {
                node.Visible = false;
                node.SetProcess(false);
                node.SetProcessInput(false);
                node.SetPhysicsProcess(false);
            }
        }

        private void EnableNode2D(Node2D node)
        {
            node.Visible = true;
            node.SetProcess(true);
            node.SetProcessInput(true);
            node.SetPhysicsProcess(true);
            active_mode_ = node;
        }

        private void EnableMainMenu()
        {
            GD.Print("Main Menu");
            DisableMode(active_state_);
            active_state_ = GameState.MAIN_MENU;
            main_menu_.Visible = true;
            main_menu_.SetProcess(true);
            main_menu_.SetProcessInput(true);
            active_mode_ = main_menu_;
        }

        private void DisableMainMenu()
        {
            main_menu_.Visible = false;
            main_menu_.SetProcess(false);
            main_menu_.SetProcessInput(false);
        }

        private void EnableBuildMode()
        {
            GD.Print("Build Mode");
            DisableMode(active_state_);
            active_state_ = GameState.BUILD_MODE;
            EnableNode2D(build_mode_);
        }

        private void EnableDayMode()
        {
            GD.Print("Day Mode");
            DisableMode(active_state_);
            active_state_ = GameState.DAY_MODE;
            EnableNode2D(day_mode_);
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
            else if (@event.IsActionPressed("day_mode") && active_state_ != GameState.MAIN_MENU)
            {
                EnableMainMenu();
            }
        }

        private void _on_main_menu_new_game_started(string new_game_name)
        {
            player_data.name = new_game_name;
            day_mode_.troll_.name = new_game_name;
            EnableDayMode();
        }
    }
}
