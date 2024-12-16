using System;
using Godot;

public partial class MouseArea : Area2D
{
    public CollisionShape2D collision_shape;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        collision_shape = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Position = GetGlobalMousePosition();
    }
}
