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

    private static void Main()
    {
        Raylib.InitWindow(Width, Height, EditString);
        Raylib.SetTargetFPS(10);

        Span2D<uint> oldGridSpan = _oldGrid;
        Span2D<uint> currentGridSpan = _currentGrid;

        ResetGrid(currentGridSpan);

        while (!Raylib.WindowShouldClose())
        {
            currentGridSpan.CopyTo(oldGridSpan);

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

            if (!_isRunning)
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
            else
            {
                Run(oldGridSpan, currentGridSpan);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(_dead));
            DrawGrid(currentGridSpan);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
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

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int neighbourX = (x + i + GridSize) % GridSize;
                int neighbourY = (y + j + GridSize) % GridSize;

                if (oldGrid[neighbourX, neighbourY] == _alive)
                {
                    aliveNeighbours++;
                }
            }
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