using Raylib_cs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameOfLife;

internal static class Program
{
    private const int Width = 960;
    private const int Height = 960;
    private const int GridSize = 64;
    private const int CellSize = Width / GridSize;
    private const string TitleString = "Game of Life - ";
    private const string RunningString = TitleString + "Running";
    private const string EditString = TitleString + "Edit";

    private static readonly uint _alive = (uint)Raylib.ColorToInt(Color.BLUE);
    private static readonly uint _dead = (uint)Raylib.ColorToInt(Color.BLACK);
    private static readonly Color _gridColor = Color.DARKGRAY;

    private static bool _isRunning;
    private static int _fps = 10;

    private static void Main()
    {
        Raylib.InitWindow(Width, Height, EditString);
        Raylib.SetTargetFPS(_fps);

        Span<uint> oldGridSpan = stackalloc uint[GridSize * GridSize];
        Span<uint> currentGridSpan = stackalloc uint[GridSize * GridSize];

        ResetGrid(currentGridSpan);

        while (!Raylib.WindowShouldClose())
        {
            currentGridSpan.CopyTo(oldGridSpan);

            CheckKeyboardControls(currentGridSpan);

            if (_isRunning)
            {
                Run(oldGridSpan, currentGridSpan);
            }
            else
            {
                CheckMouseControls(currentGridSpan);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(_dead));
            DrawGrid(currentGridSpan);
            Raylib.DrawText($"Speed: {_fps}", 0, 0, 24, Color.DARKPURPLE);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static ref uint GetCellRef(Span<uint> grid, int x, int y)
    {
        ref uint gridRef = ref MemoryMarshal.GetReference(grid);
        return ref Unsafe.Add(ref gridRef, (uint)((y * GridSize) + x));
    }

    private static uint GetCell(ReadOnlySpan<uint> grid, int x, int y)
    {
        ref uint gridRef = ref MemoryMarshal.GetReference(grid);
        return Unsafe.Add(ref gridRef, (uint)((y * GridSize) + x));
    }

    private static void CheckMouseControls(Span<uint> currentGridSpan)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mousePos = Raylib.GetMousePosition() / CellSize;
            GetCellRef(currentGridSpan, (int)mousePos.X, (int)mousePos.Y) = _alive;
        }
        else if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            var mousePos = Raylib.GetMousePosition() / CellSize;
            GetCellRef(currentGridSpan, (int)mousePos.X, (int)mousePos.Y) = _dead;
        }
    }

    private static void CheckKeyboardControls(Span<uint> currentGridSpan)
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
            _fps = Math.Max(_fps - 5, 5);
            Raylib.SetTargetFPS(_fps);
        }
    }

    private static void Run(ReadOnlySpan<uint> oldGrid, Span<uint> currentGrid)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                switch (GetNumberOfAliveNeighbours(oldGrid, x, y))
                {
                    case 2 when GetCell(oldGrid, x, y) == _alive:
                    case 3:
                        GetCellRef(currentGrid, x, y) = _alive;
                        break;
                    default:
                        GetCellRef(currentGrid, x, y) = _dead;
                        break;
                }
            }
        }
    }

    private static int GetNumberOfAliveNeighbours(ReadOnlySpan<uint> oldGrid, int x, int y)
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

                if (GetCell(oldGrid, (x + i + GridSize) % GridSize, (y + j + GridSize) % GridSize) == _alive)
                {
                    aliveNeighbours++;
                }
            }
        }

        return aliveNeighbours;
    }

    private static void ResetGrid(Span<uint> currentGrid)
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

    private static void DrawGridCells(ReadOnlySpan<uint> currentGrid)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Raylib.DrawRectangle(x * CellSize, y * CellSize, CellSize, CellSize, Raylib.GetColor(GetCell(currentGrid, x, y)));
            }
        }
    }

    private static void DrawGrid(ReadOnlySpan<uint> currentGrid)
    {
        DrawGridCells(currentGrid);
        DrawGridLines();
    }
}