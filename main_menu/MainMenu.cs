using System;
using Godot;

public partial class MainMenu : Control
{
    [Signal]
    public delegate void new_game_startedEventHandler(string new_game_name);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void _on_name_input_name_accepted(string name)
    {
        EmitSignal(SignalName.new_game_started, name);
    }

    public void _on_new_game_button_pressed() { }

    public void _on_load_game_button_pressed() { }

    public void _on_settings_button_pressed() { }

    public void _on_achievements_button_pressed() { }

    public void _on_report_bug_button_pressed() { }
}
