using CommunityToolkit.HighPerformance;
using Raylib_cs;

namespace GameOfLife;

internal static class Program
{
    private const int Width = 800;
    private const int Height = 800;
    private const int GridSize = 50;
    private const int CellSize = Width / GridSize;
    private const string TitleString = "Game of Life - ";
    private const string RunningString = TitleString + "Running";
    private const string EditString = TitleString + "Edit";

    private static readonly uint[,] _oldGrid = new uint[GridSize, GridSize];
    private static readonly uint[,] _currentGrid = new uint[GridSize, GridSize];
    private static readonly uint _alive = (uint)Raylib.ColorToInt(Color.BLUE);
    private static readonly uint _dead = (uint)Raylib.ColorToInt(Color.BLACK);
    private static readonly Color _gridColor = Color.DARKGRAY;

    private static bool _isRunning;
    private static int _fps = 10;

    private static void Main()
    {
        Raylib.InitWindow(Width, Height, EditString);
        Raylib.SetTargetFPS(_fps);

        Span2D<uint> oldGridSpan = _oldGrid;
        Span2D<uint> currentGridSpan = _currentGrid;

        ResetGrid(currentGridSpan);

        while (!Raylib.WindowShouldClose())
        {
            currentGridSpan.CopyTo(oldGridSpan);

            CheckKeyboardControls(currentGridSpan);

            if (!_isRunning)
            {
                CheckMouseControls(currentGridSpan);
            }
            else
            {
                Run(oldGridSpan, currentGridSpan);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(_dead));
            DrawGrid(currentGridSpan);
            Raylib.DrawText($"Speed: {_fps}", 0, 0, 24, Color.DARKPURPLE);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static void CheckMouseControls(Span2D<uint> currentGridSpan)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mousePos = Raylib.GetMousePosition() / CellSize;
            currentGridSpan[(int)mousePos.X, (int)mousePos.Y] = _alive;
        }
        else if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            var mousePos = Raylib.GetMousePosition() / CellSize;
            currentGridSpan[(int)mousePos.X, (int)mousePos.Y] = _dead;
        }
    }

    private static void CheckKeyboardControls(Span2D<uint> currentGridSpan)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            _isRunning = !_isRunning;
            Raylib.SetWindowTitle($"{(_isRunning ? RunningString : EditString)}");
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            ResetGrid(currentGridSpan);
            _isRunning = false;
            Raylib.SetWindowTitle(EditString);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_ADD))
        {
            _fps += 5;
            Raylib.SetTargetFPS(_fps);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_SUBTRACT))
        {
            _fps = Math.Max(_fps - 5, 0);
            Raylib.SetTargetFPS(_fps);
        }
    }

    private static void Run(ReadOnlySpan2D<uint> oldGrid, Span2D<uint> currentGrid)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                switch (GetNumberOfAliveNeighbours(oldGrid, x, y))
                {
                    case 2 when oldGrid[x, y] == _alive:
                    case 3:
                        currentGrid[x, y] = _alive;
                        break;

                    default:
                        currentGrid[x, y] = _dead;
                        break;
                }
            }
        }
    }

    private static int GetNumberOfAliveNeighbours(ReadOnlySpan2D<uint> oldGrid, int x, int y)
    {
        int aliveNeighbours = 0;

        if (x is 0 or GridSize - 1 || y is 0 or GridSize - 1)
        {
            ReadOnlySpan<int> xValues = stackalloc int[] { (x + GridSize - 1) % GridSize, x, (x + 1) % GridSize };
            ReadOnlySpan<int> yValues = stackalloc int[] { (y + GridSize - 1) % GridSize, y, (y + 1) % GridSize };

            for (int i = 0; i < yValues.Length; i++)
            {
                for (int j = 0; j < xValues.Length; j++)
                {
                    if (oldGrid[xValues[j], yValues[i]] == _alive)
                    {
                        aliveNeighbours++;
                    }
                }
            }
        }
        else
        {
            foreach (uint cell in oldGrid.Slice(x - 1, y - 1, 3, 3))
            {
                if (cell == _alive)
                {
                    aliveNeighbours++;
                }
            }
        }

        if (oldGrid[x, y] == _alive)
        {
            aliveNeighbours--;
        }

        return aliveNeighbours;
    }

    private static void ResetGrid(Span2D<uint> currentGrid)
    {
        currentGrid.Fill(_dead);
    }

    private static void DrawGridLines()
    {
        Raylib.DrawRectangleLines(0, 0, Width, Height, _gridColor);

        for (int cell = 0; cell < GridSize; cell++)
        {
            Raylib.DrawLine(0, cell * CellSize, Width, cell * CellSize, _gridColor);
            Raylib.DrawLine(cell * CellSize, 0, cell * CellSize, Height, _gridColor);
        }
    }

    private static void DrawGridCells(ReadOnlySpan2D<uint> currentGrid)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Raylib.DrawRectangle(x * CellSize, y * CellSize, CellSize, CellSize, Raylib.GetColor(currentGrid[x, y]));
            }
        }
    }

    private static void DrawGrid(ReadOnlySpan2D<uint> currentGrid)
    {
        DrawGridCells(currentGrid);
        DrawGridLines();
    }
}