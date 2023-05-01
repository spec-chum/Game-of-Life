using Raylib_cs;
using System.Numerics;

namespace GameOfLife;

internal static class Program
{
    private const int Width = 800;
    private const int Height = 800;
    private const int GridSize = 50;
    private const int CellSize = Width / GridSize;

    private static uint[,] _oldGrid = new uint[GridSize, GridSize];
    private static readonly uint[,] _currentGrid = new uint[GridSize, GridSize];
    private static readonly uint _alive = (uint)Raylib.ColorToInt(Color.BLUE);
    private static readonly uint _dead = (uint)Raylib.ColorToInt(Color.BLACK);
    private static readonly Color _gridColor = Color.DARKGRAY;

    private static bool _isRunning;

    private static void Main()
    {
        Raylib.InitWindow(Width, Height, "Game of Life - Edit");
        Raylib.SetTargetFPS(10);

        ResetGrid();

        Vector2 mousePos;
        while (!Raylib.WindowShouldClose())
        {
            _oldGrid = (uint[,])_currentGrid.Clone();

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                _isRunning = !_isRunning;
                Raylib.SetWindowTitle($"Game of Life - {(_isRunning ? "Running" : "Edit")}");
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
            {
                ResetGrid();
                _isRunning = false;
                Raylib.SetWindowTitle("Game of Life - Edit");
            }

            if (!_isRunning)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    mousePos = Raylib.GetMousePosition() / CellSize;
                    _currentGrid[(int)mousePos.X, (int)mousePos.Y] = _alive;
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
                {
                    mousePos = Raylib.GetMousePosition() / CellSize;
                    _currentGrid[(int)mousePos.X, (int)mousePos.Y] = _dead;
                }
            }
            else
            {
                Run();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(_dead));
            DrawGrid();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static void Run()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int aliveNeighbours = GetNumberOfAliveNeighbours(x, y);
                if (_oldGrid[x, y] == _alive)
                {
                    if (aliveNeighbours is 2 or 3)
                    {
                        continue;
                    }
                    else
                    {
                        _currentGrid[x, y] = _dead;
                    }
                }
                else
                {
                    if (aliveNeighbours == 3)
                    {
                        _currentGrid[x, y] = _alive;
                    }
                }
            }
        }
    }

    private static int GetNumberOfAliveNeighbours(int x, int y)
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

                if (_oldGrid[neighbourX, neighbourY] == _alive)
                {
                    aliveNeighbours++;
                }
            }
        }

        return aliveNeighbours;
    }

    private static void ResetGrid()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                _currentGrid[x, y] = _dead;
            }
        }
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

    private static void DrawGridCells()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Raylib.DrawRectangle(x * CellSize, y * CellSize, CellSize, CellSize, Raylib.GetColor(_currentGrid[x, y]));
            }
        }
    }

    private static void DrawGrid()
    {
        DrawGridCells();
        DrawGridLines();
    }
}