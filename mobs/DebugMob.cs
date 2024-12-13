using System;
using Godot;

public partial class DebugMob : CharacterBody2D
{
	public const float speed = 100.0f;
	public const float run_speed = 300.0f;

	public Vector2 goal_target_ = new(100, 100);
	public NavigationAgent2D navigation_agent_;
	public AnimatedSprite2D animated_sprite_;
	public CollisionShape2D collision_shape;

	public Troll troll;
	private float scare_range_ = 100.0f;

	public float spawn_probability = 0.8f;
	public int max_instances = 10;

	public override void _Ready()
	{
		navigation_agent_ = GetNode<NavigationAgent2D>("NavigationAgent2D");
		animated_sprite_ = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		collision_shape = GetNode<CollisionShape2D>("CollisionShape2D");
		animated_sprite_.Play("walk");
	}

	public void SetTarget(Vector2 goal_target)
	{
		goal_target_ = goal_target;
		navigation_agent_.TargetPosition = goal_target_;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Position.DistanceTo(goal_target_) < 5)
		{
			GD.Print("Leaving the map!");
			QueueFree();
		}

		if (troll is not null && Position.DistanceTo(troll.Position) < scare_range_)
		{
			GD.Print("Scary!");
			Velocity = -Position.DirectionTo(troll.Position) * run_speed;
			animated_sprite_.Play("run");
		}
		else
		{
			Velocity = Position.DirectionTo(navigation_agent_.GetNextPathPosition()) * speed;
			animated_sprite_.Play("walk");
		}

		MoveAndSlide();
	}
}
