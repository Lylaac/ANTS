using Godot;
using System;

public partial class MainMenu : Control
{
	public PackedScene simScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void startButton()
	{
		menuManager.previousScene = "res://menus/startMenu/MainMenu.tscn";
		GetTree().ChangeSceneToFile("res://menus/simSettingsMenu/SimulationSettingsMenu.tscn");
	}
	public void loadButton()
	{
		menuManager.previousScene = "res://menus/startMenu/MainMenu.tscn";
		GetTree().ChangeSceneToFile("res://menus/loadMenu/LoadMenu.tscn");
	}
	public void optionsButton()
	{
		menuManager.previousScene = "res://menus/startMenu/MainMenu.tscn";
		GetTree().ChangeSceneToFile("res:///menus/optionsMenu/OptionsMenu.tscn");
	}
	public void quitButton()
	{
		GetTree().Quit();
	}
}
