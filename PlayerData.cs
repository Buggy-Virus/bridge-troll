using System;
using Godot;

public partial class PlayerData : Node2D
{
    public string troll_name = "unknown";

    public int total_gol = 0;
    public int total_experience = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
