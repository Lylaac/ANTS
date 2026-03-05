using Godot;
using System;

public partial class LoadMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void quitButton()
	{
		GetTree().ChangeSceneToFile("res:///menus/startMenu/MainMenu.tscn");
	}
	public void loadButton()
	{
		GetTree().ChangeSceneToFile(menuManager.previousScene);
	}
}
