using Godot;
using System;

public partial class Enemy : RigidBody2D
{
	private AnimatedSprite2D _animated_sprite;
	
	public const float Speed = 300.0f;
	
	public override void _Ready()
	{
		_animated_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_animated_sprite.Play("walk");
	}
	
	private void _on_visible_on_screen_notifier_2d_screen_exited()
	{
		QueueFree();
	}
}
