using System;
using Godot;

public partial class RewardFloater : RigidBody2D
{
    [Export]
    public float experience_float_speed = -50f;

    [Export]
    public float gold_float_speed = -65f;

    [Export]
    public float horizontal_range = 55f;
    public Label label;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Position += new Vector2((GD.Randf() * horizontal_range) - (horizontal_range / 2), 0f);
        label = GetNode<Label>("Label");
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    }

    public void MakeExperienceFloater(int experience_amount)
    {
        label.Set("theme_override_colors/font_color", new Color("ef64e6"));
        label.Text = $"+{experience_amount}XP";
        LinearVelocity = new(0, experience_float_speed);
    }

    public void MakeGoldFloater(int gold_amount)
    {
        label.Set("theme_override_colors/font_color", new Color("f6b627"));
        label.Text = $"+{gold_amount}GP";
        LinearVelocity = new(0, gold_float_speed);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void _on_timer_timeout()
    {
        GD.Print("Timer Timeout!");
        QueueFree();
    }
}
