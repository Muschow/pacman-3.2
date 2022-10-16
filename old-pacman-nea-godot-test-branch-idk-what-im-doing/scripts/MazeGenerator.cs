using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : TileMap
{
	const int path = 0;
	const int wall = 1;
	const int node = 0; //0 blank 1 green //2 green 3 blank
	[Export] private int width = 31;
	[Export] public int height = 19;
	[Export] private float tickDuration = 0.0f; //was 0.03f
	
	[Export] public int mazeOriginY = 0;
	[Export] private int maxNumMazes = 3; //maybe make public

	//private int mazeOriginX = 0; //most likely delete this
	private bool generationComplete = false;
	public int mazesOnScreen = 0; //have the ghost maze wall decrease this number when passing a maze chunk mazeOnScreen -= 1; //maybe make public

	//int[,] mazeArray = new int[height*maxNumMazes,width];
	static Vector2 north = new Vector2(0, -1);
	static Vector2 east = new Vector2(1, 0);
	static Vector2 south = new Vector2(0, 1);
	static Vector2 west = new Vector2(-1, 0);
	Vector2[] directions = new Vector2[] { north, east, south, west };
	List<Vector2> visited = new List<Vector2>();
	Stack<Vector2> rdfStack = new Stack<Vector2>();

	List<Vector2> wallEdgeList = new List<Vector2>();
	private int backtrackCount = 0;
	private void CorrectMazeSize()
	{
		if (width % 2 != 1)
		{
			width -= 1;
		}
		if (height % 2 != 1)
		{
			height -= 1;
		}
		GD.Print("width " + width);
		GD.Print("height " + height);
	}

	private void CreateStartingGrid()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				//wall tile edges
				if (i == 0 || j == 0 || i == width - 1 || j == height - 1)
				{
					SetCell(i, j + mazeOriginY, wall);
					//mazeArray[j+mazeOriginY,i] = wall;

					Vector2 wallEdge = new Vector2(i, j + mazeOriginY);
					wallEdgeList.Add(wallEdge);
				}
				//alternating wall tiles
				else if (i % 2 == 0 || j % 2 == 0)
				{
					SetCell(i, j + mazeOriginY, wall);
					//mazeArray[j+mazeOriginY, i] = wall;
				}
				//path tiles
				else
				{
					SetCell(i, j + mazeOriginY, path);
					//mazeArray[j+mazeOriginY, i] = path;
				}

			}
		}
	}

	private void AddLoops(Vector2 currentV)
	{
		bool complete = false;

		for (int i = 0; i < directions.Length; i++)
		{
			if (!complete)
			{
				Vector2 newCell = new Vector2(currentV + directions[i]);
				if ((GetCellv(newCell) == wall) && (!wallEdgeList.Contains(newCell)) && (!visited.Contains(newCell)))
				{
					SetCellv(currentV + directions[i], path);
					//SetCellv(currentV + directions[i]*2, node); //nodeTilemap.
					AddNode(currentV+directions[i]*2);
					AddNode(currentV);
					//SetCellv(currentV,node);    //nodeTilemap.
					complete = true;
				}
			}
		}
	}

	private void JoinMazes()
	{
		Random rnd = new Random();
		List<Vector2> usedCells = new List<Vector2>();

		int oldY = mazeOriginY + height - 1;
		//GD.Print("Maze+height " + oldY); //debug

		double numHoles = Math.Round((double)width / 4); //maybe have numHoles be width/4 rounded up/down to nearest integer
		while (usedCells.Count < numHoles * 3)
		{
			int cellX = rnd.Next(1, width);
			Vector2 cell = new Vector2(cellX, oldY);

			if ((GetCellv(cell + south)) == path && (GetCellv(cell + north) == path) && (!usedCells.Contains(cell)))
			{
				SetCellv(cell, path);
				AddNode(cell+north);
				AddNode(cell+south);
				//SetCellv(cell+north,node);  //nodetilemap.
				//SetCellv(cell+south,node);  //nodetilemap.
				usedCells.Add(cell);
				usedCells.Add(cell + east);
				usedCells.Add(cell + west);
				//GD.Print("SetCellx path:" + cell); //debug
			}
			//GD.Print("usedCellsCount: "+usedCells.Count); //debug
		}
	}

	public Vector2 SetSpawn(bool spawnPacman){
		int x = 0;
		int y = 0;
		
		if (spawnPacman)
			y = height-2;

		Random rnd = new Random();
		while (GetCell(x,y) == wall){
			x = rnd.Next(1,width);
			if (!spawnPacman)
				y = rnd.Next(1,height-2);
		}

		Vector2 spawnLoc = new Vector2(x,y);
		GD.Print("spawn" +spawnLoc); //debug
		
		spawnLoc = new Vector2((spawnLoc*CellSize)+(CellSize/2));
		
		GD.Print("MTWspawnLoc: "+spawnLoc); //debug
		return spawnLoc;
	}

	private void AddNode(Vector2 nodeLocation){
		var nodeTilemap = GetParent().GetNode<Godot.TileMap>("NodeTilemap");
		nodeTilemap.SetCellv(nodeLocation,node);
	}

	public Vector2 IsTileFree(Vector2 pos, Vector2 dir){
		Vector2 currentTile = WorldToMap(pos);
		int nextTileType = GetCellv(currentTile+dir);
		Vector2 nextTilePos;
		
		if (nextTileType != wall) 
		{
			nextTilePos = MapToWorld(currentTile+dir)+CellSize/2;
		}
		else
		{
			nextTilePos = Relocate(pos);
		}
		return nextTilePos;
	}

	public Vector2 Relocate(Vector2 pos){
		Vector2 tile = WorldToMap(pos);
		return MapToWorld(tile) + CellSize/2;
	}

	private void rdfInit()
	{

		generationComplete = false;


		CorrectMazeSize();
		CreateStartingGrid();

		//startVector x and y must be odd, between 1+mazeOriginX/Y & height-1 / width-1 
		Vector2 startVector = new Vector2(1, mazeOriginY + 1); //Choose the initial cell,
		GD.Print("StartV: " + startVector); //debug

		visited.Add(startVector); //Mark initial cell as visited,
		rdfStack.Push(startVector); //and push it to the stack,

		rdfStep();
	}

	private void rdfStep()
	{
		Vector2 prev = new Vector2(0,0);
		while (!generationComplete)
		{
			Vector2 curr = rdfStack.Pop(); //Pop a cell from the stack and make it a current cell.
			Vector2 next = new Vector2(0, 0);
			
			bool found = false;

			//check neighbours in random order //N,E,S,W walls instead of their paths, so *2
			Random rnd = new Random();
			var rndDirections = directions.OrderBy(_ => rnd.Next()).ToList(); //found this online, randomly shuffle the list.

			for (int i = 0; i < rndDirections.Count; i++)
			{
				next = 2 * rndDirections[i];
				if (GetCellv(curr + next) == path && (!visited.Contains(curr + next)))
				{ //If the current cell has any neighbours which have not been visited,
					found = true;
					break; //Choose one of the unvisited neighbours (next),
				}
			}

			if (found)
			{
				//GD.Print("curr"+curr); //debug
				//GD.Print("prevnext "+prev); //debug
				//GD.Print("currnext "+next); //debug
				
				if (prev != next){
					//SetCellv(curr,node); //nodetilemap.
					AddNode(curr);
					//GD.Print("setcell"); //debug
				}
				prev = next;
				

				rdfStack.Push(curr); //Push the current cell to the stack,
				SetCellv(curr + (next / 2), path); // Remove the wall between the current cell and the chosen cell,
				visited.Add(curr + next); //Mark the chosen cell as visited,
				rdfStack.Push(curr + next); //and push it to the stack.  
				backtrackCount = 0;
			}
			else
			{
				backtrackCount++;
				if (backtrackCount == 1)
				{
					AddLoops(curr);
				}
			}

			if (rdfStack.Count <= 0)
			{ //While stack is not empty, (if stack is empty)
				AddLoops(curr);
				generationComplete = true;
				mazesOnScreen++;
				//GD.Print("mazesOnScreen: "+mazesOnScreen); //debug

				if (mazesOnScreen > 1)
					JoinMazes();

				GD.Print("Maze Generation Complete!"); //debug
				return;
			}
		}

	}


	//Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.PrintRaw("test");
		GD.Randomize();
		rdfInit();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.

	//double timeSinceTick = 0.0;
	public override void _Process(float delta)
	{
		if (mazesOnScreen < maxNumMazes)
		{
			//GD.Print("MazeOriginY: " + mazeOriginY); //debug
			mazeOriginY -= height - 1;
			rdfInit();
			GD.Print("MazeOriginY: " + mazeOriginY); //debug
		}
		else if (mazesOnScreen == maxNumMazes){
			//printGraph();
		}
		

	}
}
