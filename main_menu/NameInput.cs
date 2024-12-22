using System;
using BridgeTroll;
using Godot;

namespace BridgeTroll
{
    public partial class NameInput : CanvasLayer
    {
        [Signal]
        public delegate void name_acceptedEventHandler(string name);

        public Random random = new();

        public LineEdit name_input_;
        private Button accept_button_;

        private string[] _bookend_letters =
        {
            // "B",
            // "C",
            "D",
            // "G",
            "K",
            // "M",
            // "N",
            // "P",
            "Q",
            // "S",
            "T",
            // "V",
            "X",
            // "Z",
        };
        private string[] _modifying_letters = { "H", "R" };
        private string[] _vowels = { "E", "O", "U", "Y" };

        public string name = "";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            name_input_ = GetNode<LineEdit>("Panel/LineEdit");
            accept_button_ = GetNode<Button>("Panel/Button");
        }

        string GetBookendLetter()
        {
            return _bookend_letters[random.Next(0, _bookend_letters.Length)];
        }

        string GetModifyingLetter()
        {
            return _modifying_letters[random.Next(0, _modifying_letters.Length)];
        }

        string GetVowel()
        {
            return _vowels[random.Next(0, _vowels.Length)];
        }

        void AddNextLetter()
        {
            if (name.Length == 0 || name.Length == 3)
            {
                name += GetBookendLetter();
            }
            else if (name.Length == 1)
            {
                name += GetModifyingLetter();
            }
            else if (name.Length == 2)
            {
                name += GetVowel();
            }
        }

        void RemoveLastLetter()
        {
            if (name.Length > 0)
            {
                name = name.Remove(name.Length - 1);
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) { }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("letter_keys"))
            {
                AddNextLetter();
            }
            if (@event.IsActionPressed("backspace_key"))
            {
                RemoveLastLetter();
            }
            name_input_.Text = name;
        }

        private void _on_button_pressed()
        {
            if (name_input_.Text != "")
            {
                EmitSignal(SignalName.name_accepted, name_input_.Text);
            }
        }
    }
}
