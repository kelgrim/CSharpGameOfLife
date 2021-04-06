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


    Control loadMenu;
    Control saveMenu;
    ItemList itemList;

    public override void _Ready()
    {
        tileMap = GetNode<TileMap>("TileMap");

        loadMenu = GetNode<Control>("LoadMenu");
        loadMenu.Visible = false;
        itemList = loadMenu.GetNode<ItemList>("TextureRect/ItemList");

        saveMenu = GetNode<Control>("SaveMenu");
        saveMenu.Visible = false;

        gridWidth = screenWidth / cellSize;
        gridHeight = screenHeight / cellSize;

        currentCells = new bool[gridWidth, gridHeight];
        updatedCells = new bool[gridWidth, gridHeight];


        if (defaultLoad == null)
        {
            ClearMap();
            UpdateCurrentCells();
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

            if (!loadMenu.Visible && !saveMenu.Visible)
            {
                Vector2 position = GetGlobalMousePosition();
                Vector2 gridPosition = GetGridPositionFromGlobalPosition(position);

                tileMap.SetCell((int)gridPosition.x, (int)gridPosition.y, 0);

                UpdateCurrentCells();
            }
        }

        if (Input.IsActionPressed("right_click"))
        {
            if (!loadMenu.Visible && !saveMenu.Visible)
            {
                isPaused = true;
                Vector2 position = GetGlobalMousePosition();
                Vector2 gridPosition = GetGridPositionFromGlobalPosition(position);

                tileMap.SetCell((int)gridPosition.x, (int)gridPosition.y, 1);
                UpdateCurrentCells();
            }
        }

        if (Input.IsActionJustPressed("randomize"))
        {
            isPaused = true;
            RandomizeMap();
            UpdateCurrentCells();
        }

        if (Input.IsActionJustPressed("clear_screen"))
        {
            isPaused = true;
            ClearMap();
            UpdateCurrentCells();
        }

        if (Input.IsActionJustPressed("pause_toggle"))
        {
            if (!loadMenu.Visible && !saveMenu.Visible)
            {
                isPaused = !isPaused;
            }
            else isPaused = true;
        }


        if (Input.IsActionJustPressed("quick_save"))
        {
            isPaused = true;
            if (!SaveToFile("quicksave.json"))
            {
                GD.Print("Saving file failed");
            }
        }

        if (Input.IsActionJustPressed("quick_load"))
        {
            isPaused = true;
            LoadFromFile("quicksave.json");
        }

        if (Input.IsActionJustPressed("save_to_file"))
        {
            if (!saveMenu.Visible)
            {
                saveMenu.Visible = true;
            }
            else saveMenu.Visible = false;

        }

        if (Input.IsActionJustPressed("load_from_file"))
        {
            if (!loadMenu.Visible)
            {
                isPaused = true;

                itemList.Clear();
                var savedFiles = GetSavedFiles();
                foreach (var file in savedFiles)
                {
                    itemList.AddItem(file);
                }
                loadMenu.Visible = true;
            }

            else loadMenu.Visible = false;

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
                if (x == 0 & y == 0) { continue; }
                else
                {
                    int checkX = xPos + x;
                    int checkY = yPos + y;

                    if (checkX == -1)
                    {
                        checkX = gridWidth - 1;
                    }

                    if (checkX == gridWidth)
                    {
                        checkX = 0;
                    }

                    if (checkY == -1)
                    {
                        checkY = gridHeight - 1;
                    }

                    if (checkY == gridHeight)
                    {
                        checkY = 0;
                    }

                    int cellValue = tileMap.GetCell(checkX, checkY);
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

    public Godot.Collections.Array<string> GetSavedFiles()
    {
        Godot.Collections.Array<string> result = new Godot.Collections.Array<string>();

        Directory dir = new Directory();
        if (!dir.DirExists(folder))
        {
            return result; // result is empty array
        }

        Error e1 = dir.Open(folder);
        Error e2 = dir.ListDirBegin();

        if (e1 == Error.Ok && e2 == Error.Ok)
        {
            string current = dir.GetNext();
            while (current != "")
            {
                if (dir.CurrentIsDir())
                {
                    // Shouldn't happen. Print a warning, but add nothing
                }
                else result.Add(current);

                current = dir.GetNext();

            }

            return result; // Here there should actually be values in the result
        }
        else
        {
            return result; // result is empty array
        }

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


    public void ClearMap()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tileMap.SetCell(x, y, 1);
            }
        }
    }

    public void _on_TextureButton_button_up()
    {
        if (itemList.IsAnythingSelected())
        {
            int[] indices = itemList.GetSelectedItems();
            if (indices.Length > 0)
            {
                string fileText = itemList.GetItemText(indices[0]);
                bool isLoaded = LoadFromFile(fileText);
                if (isLoaded)
                {
                    UpdateCurrentCells();
                    loadMenu.Visible = false;
                }
                else GD.Print("WARNING - ILLEGAL FILE LOADED");

            }

        }
    }

    public void _on_SaveButton_button_up()
    {
        var textBox = GetNode<TextEdit>("SaveMenu/TextureRect/TextEdit");
        string file = textBox.Text;
        if (file != null && file != "")
        {
            SaveToFile(file + ".json");
            saveMenu.Visible = false;
        }


    }
}
