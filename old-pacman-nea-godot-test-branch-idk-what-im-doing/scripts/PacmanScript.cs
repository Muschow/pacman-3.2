using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PacmanScript : CharacterScript
{
    private Godot.Collections.Array rays;
    private Camera2D pacmanCamera;
    private Vector2 nextDir = Vector2.Down;
    IDictionary<Vector2,RayCast2D[]> rayDict = new Dictionary<Vector2,RayCast2D[]>();
    
    // Called when the node enters the scene tree for the first time.
    public PacmanScript()
    {
        speed = 200 *gameSpeed; //gameSpeed is defined in CharacterScript
    }

    public void GetInput(){
        if (Input.IsActionJustPressed("move_up")){
            nextDir = Vector2.Up;
        }
        else if (Input.IsActionJustPressed("move_down")){
            nextDir = Vector2.Down;
        }
        else if (Input.IsActionJustPressed("move_right")){
            nextDir = Vector2.Right;
        }
        else if (Input.IsActionJustPressed("move_left")){
            nextDir = Vector2.Left;
        }

        checkCollision();
        moveVelocity = moveDir * speed;
        
    }

    private void checkCollision(){
        int noCollision = 0;
        for(int i = 0; i < rayDict[nextDir].Length; i++)
        {
        
            if ((rayDict[nextDir])[i].IsColliding())
                return;
            else
                noCollision++;

        }
        if (noCollision == 2){
            moveDir = nextDir;
        }
    }

    private void addRaystoDict(){
        RayCast2D[] upRays = new RayCast2D[2];
        RayCast2D[] downRays = new RayCast2D[2];
        RayCast2D[] rightRays = new RayCast2D[2];
        RayCast2D[] leftRays = new RayCast2D[2];

        int dictItem = -1;
        for(int i = 0; i < rays.Count; i++){
            
            if (i%2 == 0)
                dictItem++;

            if (dictItem == 0)
                upRays[i%2] = (RayCast2D)rays[i];
            else if (dictItem == 1)
                downRays[i%2] = (RayCast2D)rays[i];
            else if (dictItem == 2)
                rightRays[i%2] = (RayCast2D)rays[i];
            else if (dictItem == 3)
                leftRays[i%2] = (RayCast2D)rays[i];
        }

        rayDict.Add(Vector2.Up,upRays);
        rayDict.Add(Vector2.Down,downRays);
        rayDict.Add(Vector2.Right,rightRays);
        rayDict.Add(Vector2.Left,leftRays);
    }
    
    public override void _Ready()
    {
        mazeTm = GetNode<TileMap>("/root/Game/Maze/MazeTilemap");
        Position = (Vector2)mazeTm.Call("SetSpawn",true); 

        rays = GetTree().GetNodesInGroup("pacmanRays");
        addRaystoDict();

        pacmanCamera = GetNode<Camera2D>("Camera2D");
        //untether the pacmancamera to pacmans.x coordinate
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        GetInput();
        MoveAndSlide(moveVelocity);
    }
}
