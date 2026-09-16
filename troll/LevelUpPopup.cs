using System;
using Godot;

namespace BridgeTroll
{
    public partial class LevelUpPopup : Control
    {
        [Signal]
        public delegate void LevelUpConfirmedEventHandler();

        [Signal]
        public delegate void LevelUpDismissedEventHandler();

        public Troll target_troll;

        public int points_available = 0;
        public int points_remaining = 0;

        public int allocated_strength = 0;
        public int allocated_charisma = 0;
        public int allocated_intelligence = 0;

        // UI node references
        public Label title_label;
        public Label points_label;

        public Label str_value_label;
        public Button str_up_button;
        public Button str_down_button;

        public Label char_value_label;
        public Button char_up_button;
        public Button char_down_button;

        public Label int_value_label;
        public Button int_up_button;
        public Button int_down_button;

        public Button confirm_button;
        public Button dismiss_button;

        public override void _Ready()
        {
            title_label = GetNodeOrNull<Label>("Panel/TitleLabel");
            points_label = GetNodeOrNull<Label>("Panel/PointsLabel");

            str_value_label = GetNodeOrNull<Label>("Panel/GridContainer/StrValueLabel");
            str_up_button = GetNodeOrNull<Button>("Panel/GridContainer/StrUpButton");
            str_down_button = GetNodeOrNull<Button>("Panel/GridContainer/StrDownButton");

            char_value_label = GetNodeOrNull<Label>("Panel/GridContainer/CharValueLabel");
            char_up_button = GetNodeOrNull<Button>("Panel/GridContainer/CharUpButton");
            char_down_button = GetNodeOrNull<Button>("Panel/GridContainer/CharDownButton");

            int_value_label = GetNodeOrNull<Label>("Panel/GridContainer/IntValueLabel");
            int_up_button = GetNodeOrNull<Button>("Panel/GridContainer/IntUpButton");
            int_down_button = GetNodeOrNull<Button>("Panel/GridContainer/IntDownButton");

            confirm_button = GetNodeOrNull<Button>("Panel/ButtonContainer/ConfirmButton");
            dismiss_button = GetNodeOrNull<Button>("Panel/ButtonContainer/DismissButton");

            if (str_up_button != null) str_up_button.Pressed += OnStrUpPressed;
            if (str_down_button != null) str_down_button.Pressed += OnStrDownPressed;

            if (char_up_button != null) char_up_button.Pressed += OnCharUpPressed;
            if (char_down_button != null) char_down_button.Pressed += OnCharDownPressed;

            if (int_up_button != null) int_up_button.Pressed += OnIntUpPressed;
            if (int_down_button != null) int_down_button.Pressed += OnIntDownPressed;

            if (confirm_button != null) confirm_button.Pressed += OnConfirmPressed;
            if (dismiss_button != null) dismiss_button.Pressed += OnDismissPressed;

            Visible = false;
        }

        /// <summary>
        /// Displays the level up popup centered on the screen for the specified troll.
        /// </summary>
        public void Open(Troll troll)
        {
            target_troll = troll;
            int earnedPoints = troll.GetStatPointsEarnedForLevel(troll.level);
            points_available = earnedPoints;
            points_remaining = earnedPoints;

            allocated_strength = 0;
            allocated_charisma = 0;
            allocated_intelligence = 0;

            UpdateUI();
            Visible = true;
        }

        /// <summary>
        /// Dismisses the popup without applying stat changes or spending the level up.
        /// </summary>
        public void Dismiss()
        {
            Visible = false;
            EmitSignal(SignalName.LevelUpDismissed);
        }

        public void UpdateUI()
        {
            int baseStr = target_troll != null ? target_troll.base_strength : 10;
            int baseChar = target_troll != null ? target_troll.base_charisma : 10;
            int baseInt = target_troll != null ? target_troll.base_intelligence : 10;

            if (title_label != null)
            {
                int lvl = target_troll != null ? target_troll.level : 1;
                title_label.Text = $"Level Up! (Level {lvl})";
            }

            if (points_label != null)
            {
                points_label.Text = $"Points to Allocate: {points_remaining}";
            }

            if (str_value_label != null)
            {
                str_value_label.Text = $"{baseStr} (+{allocated_strength})";
            }

            if (char_value_label != null)
            {
                char_value_label.Text = $"{baseChar} (+{allocated_charisma})";
            }

            if (int_value_label != null)
            {
                int_value_label.Text = $"{baseInt} (+{allocated_intelligence})";
            }

            if (str_down_button != null) str_down_button.Disabled = allocated_strength <= 0;
            if (char_down_button != null) char_down_button.Disabled = allocated_charisma <= 0;
            if (int_down_button != null) int_down_button.Disabled = allocated_intelligence <= 0;

            if (str_up_button != null) str_up_button.Disabled = points_remaining <= 0;
            if (char_up_button != null) char_up_button.Disabled = points_remaining <= 0;
            if (int_up_button != null) int_up_button.Disabled = points_remaining <= 0;

            if (confirm_button != null)
            {
                confirm_button.Disabled = points_remaining > 0;
            }
        }

        private void OnStrUpPressed()
        {
            if (points_remaining > 0)
            {
                allocated_strength++;
                points_remaining--;
                UpdateUI();
            }
        }

        private void OnStrDownPressed()
        {
            if (allocated_strength > 0)
            {
                allocated_strength--;
                points_remaining++;
                UpdateUI();
            }
        }

        private void OnCharUpPressed()
        {
            if (points_remaining > 0)
            {
                allocated_charisma++;
                points_remaining--;
                UpdateUI();
            }
        }

        private void OnCharDownPressed()
        {
            if (allocated_charisma > 0)
            {
                allocated_charisma--;
                points_remaining++;
                UpdateUI();
            }
        }

        private void OnIntUpPressed()
        {
            if (points_remaining > 0)
            {
                allocated_intelligence++;
                points_remaining--;
                UpdateUI();
            }
        }

        private void OnIntDownPressed()
        {
            if (allocated_intelligence > 0)
            {
                allocated_intelligence--;
                points_remaining++;
                UpdateUI();
            }
        }

        private void OnConfirmPressed()
        {
            if (target_troll != null)
            {
                target_troll.ApplyStatPoints(allocated_strength, allocated_charisma, allocated_intelligence);
                target_troll.ConsumePendingLevelUp();
            }

            EmitSignal(SignalName.LevelUpConfirmed);

            if (target_troll != null && target_troll.HasPendingLevelUps())
            {
                Open(target_troll);
            }
            else
            {
                Dismiss();
            }
        }

        private void OnDismissPressed()
        {
            Dismiss();
        }
    }
}
