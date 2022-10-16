using Godot;
using System;

public class GameScript : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private TileMap mazeTm;
	private KinematicBody2D pacman;
	private int mazeStartLoc = 0;
	PackedScene mazeScene = GD.Load<PackedScene>("res://scenes/Maze.tscn");
	
	// Called when the node enters the scene tree for the first time.
	
	
	public override void _Ready()
	{
		mazeTm = GetNode<TileMap>("/root/Game/Maze/MazeTilemap");
		pacman = GetNode<KinematicBody2D>("/root/Game/Pacman"); // res://scenes/Pacman.tscn
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void updateMazeOrigin(){
		int mazeOrigin = (int)mazeTm.Get("mazeOriginY");
		if (mazeOrigin < mazeStartLoc)
			mazeStartLoc = mazeOrigin;
		
	}
	public override void _Process(float delta)
	{
		updateMazeOrigin();
		GD.Print("mazeStartLoc"+mazeStartLoc);
		GD.Print("start+height-1 "+mazeStartLoc+18);
		GD.Print("playerPos "+Math.Floor(pacman.Position.y/32));
		
		if (Math.Floor(pacman.Position.y/32) == mazeStartLoc+18){
			Node mazeInstance = mazeScene.Instance(); //Node2D / Node
			//mazeInstance.Set("mazeOriginY",mazeStartLoc-19); //basically its overwriting the maze insttead of making a new one higher... something to do with mazeoriginy
			AddChild(mazeInstance);
			GD.Print("instanced!");
			//instance maze tscn
			//joinMazes where oldY = mazeoriginy+height-1 * 3??? not sure
		}
			
	}
}
