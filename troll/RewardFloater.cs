using System;
using Godot;

public partial class RewardFloater : RigidBody2D
{
    [Export]
    public float float_speed = -50f;

    [Export]
    public float horizontal_range = 50f;
    public Label label;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Position += new Vector2((GD.Randf() * horizontal_range) + (horizontal_range / 2), 0f);
        LinearVelocity = new(0, float_speed);
        label = GetNode<Label>("Label");
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    }

    public void MakeExperienceFloater(int experience_amount)
    {
        label.AddThemeColorOverride("experience_text", new("b716ff"));
        label.Text = $"+{experience_amount}XP";
    }

    public void MakeGoldFloater(int gold_amount)
    {
        label.AddThemeColorOverride("gold_text", new("d8a100"));
        label.Text = $"+{gold_amount}GP";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void _on_timer_timeout()
    {
        GD.Print("Timer Timeout!");
        QueueFree();
    }
}
