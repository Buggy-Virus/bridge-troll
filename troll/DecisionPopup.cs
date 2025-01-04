using System;
using System.Collections.Generic;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class DecisionPopup : Control
    {
        [Export]
        public PackedScene option_label_packed_scene;

        private Label offered_payment_label_;
        private Control option_labels_parent_;

        public void UpdateOptions(List<VictimOption> victim_options)
        {
            foreach (var child in option_labels_parent_.GetChildren())
            {
                child.QueueFree();
            }

            float position_offset = 20f;
            Vector2 last_position = option_labels_parent_.Position;
            foreach (var option in victim_options)
            {
                Label option_label = option_label_packed_scene.Instantiate<Label>();
                option_labels_parent_.AddChild(option_label);

                string option_text = option.ToString();
                Godot.Collections.Array<InputEvent> input_events = InputMap.ActionGetEvents(
                    option_text
                );
                string option_input_text =
                    input_events.Count > 0 ? input_events[0].AsText() : "NONE";
                string option_input_simple_text = option_input_text.Substring(0, 1);

                option_label.Text = $"{option_input_simple_text}: {option_text}";

                option_label.Position = new(last_position.X, last_position.Y + position_offset);
                last_position = option_label.Position;
            }
        }

        public void UpdateOfferedPayment(int payment)
        {
            offered_payment_label_.Text = $"Payment: {payment}";
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            offered_payment_label_ = GetNode<Label>("OfferedPaymentLabel");
            option_labels_parent_ = GetNode<Control>("OptionLabels");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }
    }
}
