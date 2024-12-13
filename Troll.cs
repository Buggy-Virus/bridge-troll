using System;
using Godot;

public partial class Troll : CharacterBody2D
{
	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int speed { get; set; } = 400; // (pixels/sec)

	private Vector2 _screen_size;
	private AnimatedSprite2D _animated_sprite;

	private Vector2 _target;

	public override void _Ready()
	{
		_screen_size = GetViewportRect().Size;
		_animated_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("right_click"))
		{
			_target = GetGlobalMousePosition();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = Position.DirectionTo(_target) * speed;
		if (Position.DistanceTo(_target) > 5)
		{
			MoveAndSlide();
			if (Velocity.X < 0)
			{
				_animated_sprite.FlipH = true;
			}
			else
			{
				_animated_sprite.FlipH = false;
			}
			_animated_sprite.Play();
		}
		else
		{
			_animated_sprite.Stop();
		}
	}
}
