using Godot;
using System;

public partial class Main : Node
{
	[Export]
	public PackedScene enemy_scene { get; set; }
	
	private int _score;
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
	
	public void GameOver() {
		GetNode<Timer>("enemy_timer").Stop();
		GetNode<Timer>("score_timer").Stop();
		GetNode<Hud>("Hud").ShowGameOver();
	}
	
	public void NewGame() {
		_score = 0;
		
		GetTree().CallGroup("enemies", Node.MethodName.QueueFree);
		
		Hud hud = GetNode<Hud>("Hud");
		hud.UpdateScore(_score);
		hud.ShowMessage("Get Ready!");
		
		Troll troll = GetNode<Troll>("troll");
		troll.Position = GetNode<Marker2D>("start_position").Position;
		
		GetNode<Timer>("start_timer").Start();
		GD.Print("Game Started");
	}
	
	private void _on_enemy_timer_timeout()
	{
		Enemy enemy = enemy_scene.Instantiate<Enemy>();
		
		PathFollow2D spawn_location = GetNode<PathFollow2D>("enemy_path/enemy_spawn_location");
		spawn_location.ProgressRatio = GD.Randf();
		
		float direction = spawn_location.Rotation + Mathf.Pi / 2;
		
		enemy.Position = spawn_location.Position;
		enemy.Rotation = direction;
		
		Vector2 velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
		enemy.LinearVelocity = velocity.Rotated(direction);
		
		AddChild(enemy);
	}


	private void _on_score_timer_timeout()
	{
		_score++;
		GetNode<Hud>("Hud").UpdateScore(_score);
	}


	private void _on_start_timer_timeout()
	{
		GetNode<Timer>("enemy_timer").Start();
		GetNode<Timer>("score_timer").Start();
	}
	
	private void _on_hud_start_game()
	{
		NewGame();
	}
}
