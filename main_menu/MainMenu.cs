using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class MainMenu : Control
    {
        [Signal]
        public delegate void NewGameStartedEventHandler(string new_game_name);

        private NameInput name_input_;

        private VBoxContainer menu_button_parent_;

        private Button new_game_button_;
        private Button load_game_button_;
        private Button settings_button_;
        private Button achievements_button_;
        private Button report_bug_button_;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            name_input_ = GetNode<NameInput>("NameInput");
            menu_button_parent_ = GetNode<VBoxContainer>("VBoxContainer");
            new_game_button_ = menu_button_parent_.GetNode<Button>("NewGameButton");
            load_game_button_ = menu_button_parent_.GetNode<Button>("LoadGameButton");
            settings_button_ = menu_button_parent_.GetNode<Button>("SettingsButton");
            achievements_button_ = menu_button_parent_.GetNode<Button>("AchievementsButton");
            report_bug_button_ = menu_button_parent_.GetNode<Button>("ReportBugButton");

            new_game_button_.Pressed += OnNewGameButtonPressed;
            name_input_.NameAccepted += OnNameAccepted;

            CloseNameInput();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }

        private void ShowMenuButtons()
        {
            foreach (Control menu_button in menu_button_parent_.GetChildren())
            {
                menu_button.Visible = false;
                menu_button.SetProcessInput(false);
            }
        }

        private void HideMenuButtons()
        {
            foreach (Control menu_button in menu_button_parent_.GetChildren())
            {
                menu_button.Visible = true;
                menu_button.SetProcessInput(true);
            }
        }

        private void OpenNameInput()
        {
            name_input_.Visible = true;
            name_input_.SetProcessInput(true);
            name_input_.SetProcess(true);
        }

        private void CloseNameInput()
        {
            name_input_.Visible = false;
            name_input_.SetProcessInput(false);
            name_input_.SetProcess(false);
        }

        private void OnNewGameButtonPressed()
        {
            HideMenuButtons();
            OpenNameInput();
        }

        private void OnNameAccepted(string new_game_name)
        {
            EmitSignal(SignalName.NewGameStarted, new_game_name);
        }
    }
}
