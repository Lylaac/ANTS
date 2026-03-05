using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Escape))
		{
			GetTree().Paused = true;
			Show();
		}
	}

	public void continueButton()
	{
		Hide();
		GetTree().Paused = false;
	}

	public void optionsButton()
	{
		GetTree().Paused = false;
		menuManager.previousScene = "res://Simulation/Simulation.tscn";
		GetTree().ChangeSceneToFile("res:///menus/optionsMenu/OptionsMenu.tscn");
	}

	public void saveButton()
	{
		GetTree().Paused = false;
		menuManager.previousScene = "res://Simulation/Simulation.tscn";
		GetTree().ChangeSceneToFile("res:///menus/saveSimMenu/SaveSimulationMenu.tscn");
	}

	public void quitButton()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res:///menus/startMenu/MainMenu.tscn");
	}
}
