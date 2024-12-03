using Godot;
using System;

public partial class Hud : CanvasLayer
{
	[Signal]
	public delegate void StartGameEventHandler();
	
	public void ShowMessage(string text)
	{
		Label message = GetNode<Label>("message");
		message.Text = text;
		message.Show();

		GetNode<Timer>("message_timer").Start();
	}
	
	async public void ShowGameOver()
	{
		ShowMessage("Game Over");

		Timer message_timer = GetNode<Timer>("message_timer");
		await ToSignal(message_timer, Timer.SignalName.Timeout);

		Label message = GetNode<Label>("message");
		message.Text = "Game Over!";
		message.Show();

		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
		GetNode<Button>("start_button").Show();
	}
	
	public void UpdateScore(int score)
	{
		GetNode<Label>("score_label").Text = score.ToString();
	}
	
	private void _on_message_timer_timeout()
	{
		GetNode<Label>("message").Hide();
	}
	
	private void _on_start_button_pressed()
	{
		GetNode<Button>("start_button").Hide();
		EmitSignal(SignalName.StartGame);
	}
}
