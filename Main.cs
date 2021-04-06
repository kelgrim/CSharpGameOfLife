using Godot;
using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Main : Node2D
{
    public float timeBetweenUpdates = 0.1f;
    private float timeCount = 0.0f;

    int cellSize = 16;
    int screenWidth = 1920;
    int screenHeight = 1080;

    int gridWidth;
    int gridHeight;

    float randomizeWithSimplexThreshold = 0.1f;
    bool isPaused = true;

    [JsonProperty]
    bool[,] currentCells;
    bool[,] updatedCells;

    TileMap tileMap;

    string defaultLoad = "double_gliders.json";


    //Save Game params
    string folder = "res://saves/";

    public override void _Ready()
    {
        tileMap = GetNode<TileMap>("TileMap");

        gridWidth = screenWidth / cellSize;
        gridHeight = screenHeight / cellSize;

        currentCells = new bool[gridWidth, gridHeight];
        updatedCells = new bool[gridWidth, gridHeight];


        if (defaultLoad == null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    tileMap.SetCell(x, y, 1);
                }
            }
        }
        else
        {
            LoadFromFile(defaultLoad);
            UpdateCurrentCells();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("left_click"))
        {
            isPaused = true;

            Vector2 position = GetGlobalMousePosition();
            Vector2 gridPosition = GetGridPositionFromGlobalPosition(position);

            tileMap.SetCell((int)gridPosition.x, (int)gridPosition.y, 0);

            UpdateCurrentCells();
        }

        if (Input.IsActionPressed("right_click"))
        {
            isPaused = true;
            Vector2 position = GetGlobalMousePosition();
            Vector2 gridPosition = GetGridPositionFromGlobalPosition(position);

            tileMap.SetCell((int)gridPosition.x, (int)gridPosition.y, 1);
            UpdateCurrentCells();
        }

        if (Input.IsActionJustPressed("randomize"))
        {
            isPaused = true;
            RandomizeMap();
            UpdateCurrentCells();
        }

        if (Input.IsActionJustPressed("pause_toggle"))
        {
            isPaused = !isPaused;
        }


        if (Input.IsActionJustPressed("save"))
        {
            isPaused = true;
            if (!SaveToFile("saved01.json"))
            {
                GD.Print("Saving file failed");
            }
        }

        if (Input.IsActionJustPressed("load"))
        {
            isPaused = true;
            LoadFromFile("saved01.json");
        }

        if (!isPaused)
        {
            timeCount += delta;

            if (timeCount > timeBetweenUpdates)
            {
                timeCount -= timeBetweenUpdates;


                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        int livingNeighbours = GetLivingNeighboursAmount(x, y);
                        bool isAlive = tileMap.GetCell(x, y) == 0;

                        if (isAlive)
                        {
                            if (livingNeighbours < 2)
                            {
                                updatedCells[x, y] = false;
                            }
                            else if (livingNeighbours > 3)
                            {
                                updatedCells[x, y] = false;
                            }
                            else updatedCells[x, y] = true;
                        }
                        else
                        {
                            if (livingNeighbours == 3)
                            {
                                updatedCells[x, y] = true;
                            }
                            else updatedCells[x, y] = false;
                        }
                    }
                }

                DrawMap(updatedCells);
                currentCells = updatedCells;
            }
        }
    }


    public void DrawMap(bool[,] mapData)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (mapData[x, y])
                {
                    tileMap.SetCell(x, y, 0);
                }
                else tileMap.SetCell(x, y, 1);

            }
        }
    }


    public int GetLivingNeighboursAmount(int xPos, int yPos)
    {
        int result = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 & y == 0) { }
                else
                {
                    int cellValue = tileMap.GetCell(xPos + x, yPos + y);
                    if (cellValue == 0)
                    {
                        result++;
                    }
                }
            }
        }

        return result;
    }

    public void UpdateCurrentCells()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tileMap.GetCell(x, y) == 0)
                {
                    currentCells[x, y] = true;
                }
                else currentCells[x, y] = false;
            }
        }
    }

    public Vector2 GetGridPositionFromGlobalPosition(Vector2 position)
    {
        int resultX = (int)Mathf.Floor(position.x / cellSize);
        int resultY = (int)Mathf.Floor(position.y / cellSize);

        return new Vector2(resultX, resultY);

    }

    public Vector2 GetVectorFromPositionId(int positionId)
    {
        int x = (int)Mathf.Floor(positionId % gridWidth);
        int y = (int)Mathf.Floor(positionId / gridWidth);

        return new Vector2(x, y);
    }

    public int GetPositionIdFromVector(Vector2 from)
    {
        int result = 0;

        result += (int)from.x;
        result += (int)from.y * gridWidth;

        return result;
    }


    public bool SaveToFile(string filename)
    {
        Directory dir = new Directory();
        if (!dir.DirExists(folder))
        {
            dir.MakeDirRecursive(folder);
        }

        File file = new File();
        Error e = file.Open(folder + filename, File.ModeFlags.Write);
        if (e == Error.Ok)
        {
            String fileContent = JsonConvert.SerializeObject(this);
            file.StoreString(fileContent);
            file.Close();
            GD.Print("saved");
            return true;
        }
        else
        {
            GD.Print("File error: " + e);
            return false;
        }
    }

    public bool LoadFromFile(string filename)
    {
        Directory dir = new Directory();
        if (!dir.DirExists(folder))
        {
            return false;
        }

        File file = new File();
        Error e = file.Open(folder + filename, File.ModeFlags.Read);

        if (e == Error.Ok)
        {
            String data = file.GetAsText();
            Main loadedTiles = JsonConvert.DeserializeObject<Main>(data);
            DrawMap(loadedTiles.currentCells);
            file.Close();
            return true;
        }
        else return false;
    }

    public void RandomizeMap()
    {
        int usedSeed = (int)GD.RandRange(0, 100);
        RandomizeMap(usedSeed);
    }

    public void RandomizeMap(int seed)
    {
        //GD.Randomize();
        GD.Print(seed);
        Godot.OpenSimplexNoise noise = new Godot.OpenSimplexNoise();
        noise.Seed = seed;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (Math.Abs(noise.GetNoise2d(x, y)) < randomizeWithSimplexThreshold)
                {
                    tileMap.SetCell(x, y, 0);
                }
                else tileMap.SetCell(x, y, 1);
            }
        }
    }
}
