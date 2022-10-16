using Godot;
using System;

public class CharacterScript : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    protected int speed;
    protected int gameSpeed = 1;
    protected int health;
    protected Vector2 moveDir = new Vector2(0,0);
    protected Vector2 moveVelocity;
    protected TileMap mazeTm;
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
