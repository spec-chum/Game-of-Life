using Raylib_cs;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameOfLife;

internal static class Program
{
    private class Game
    {
        public uint[] currentGrid = new uint[GridSize * GridSize];
        public uint[] oldGrid = new uint[GridSize * GridSize];
        public bool isRunning;
        public int fps;
    }

    private const int Width = 960;
    private const int Height = 960;
    private const int GridSize = 64;
    private const int CellSize = Width / GridSize;
    private const string TitleString = "Game of Life - ";
    private const string RunningString = TitleString + "Running";
    private const string EditString = TitleString + "Edit";
    private const string SpeedString = "Speed: ";

    private static readonly uint CellAlive = (uint)Raylib.ColorToInt(Color.BLUE);
    private static readonly uint CellDead = (uint)Raylib.ColorToInt(Color.BLACK);
    private static readonly Color GridColor = Color.DARKGRAY;
    private static readonly Color TextColor = Color.DARKGREEN;

    private static void Main()
    {
        Game game = new Game
        {
            isRunning = false,
            fps = 10
        };

        Raylib.InitWindow(Width, Height, EditString);
        Raylib.SetTargetFPS(game.fps);

        ResetGrid(game);

        while (!Raylib.WindowShouldClose())
        {
            CopyCurrentGrid(game);

            CheckKeyboardControls(game);
            CheckMouseControls(game);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(CellDead));
            UpdateGrid(game);
            DrawGridLines();
            Raylib.DrawText(SpeedString + game.fps, 0, 0, 24, TextColor);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static ref uint GetCellRef(uint[] grid, int x, int y)
    {
        ref uint gridRef = ref MemoryMarshal.GetArrayDataReference(grid);
        return ref Unsafe.Add(ref gridRef, (uint)((y * GridSize) + x));
    }

    private static uint GetCellValue(uint[] grid, int x, int y)
    {
        ref uint gridRef = ref MemoryMarshal.GetArrayDataReference(grid);
        return Unsafe.Add(ref gridRef, (uint)((y * GridSize) + x));
    }

    private static void CheckMouseControls(Game game)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            Vector2 mousePos = Raylib.GetMousePosition() / CellSize;
            GetCellRef(game.currentGrid, (int)mousePos.X, (int)mousePos.Y) = CellAlive;
        }
        else if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 mousePos = Raylib.GetMousePosition() / CellSize;
            GetCellRef(game.currentGrid, (int)mousePos.X, (int)mousePos.Y) = CellDead;
        }
    }

    private static void CheckKeyboardControls(Game game)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            game.isRunning = !game.isRunning;
            Raylib.SetWindowTitle($"{(game.isRunning ? RunningString : EditString)}");
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            ResetGrid(game);
            game.isRunning = false;
            Raylib.SetWindowTitle(EditString);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_ADD))
        {
            game.fps += 5;
            Raylib.SetTargetFPS(game.fps);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_KP_SUBTRACT))
        {
            game.fps = Math.Max(game.fps - 5, 5);
            Raylib.SetTargetFPS(game.fps);
        }
    }

    private static void UpdateGrid(Game game)
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                ref uint currentCellColor = ref GetCellRef(game.currentGrid, x, y);

                if (game.isRunning)
                {
                    switch (GetNumberOfAliveNeighbours(game, x, y))
                    {
                        case 2 when GetCellValue(game.oldGrid, x, y) == CellAlive:
                        case 3:
                            currentCellColor = CellAlive;
                            break;
                        default:
                            currentCellColor = CellDead;
                            break;
                    }
                }

                Raylib.DrawRectangle(x * CellSize, y * CellSize, CellSize, CellSize, Raylib.GetColor(currentCellColor));
            }
        }
    }

    private static int GetNumberOfAliveNeighbours(Game game, int x, int y)
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

                if (GetCellValue(game.oldGrid, (x + i + GridSize) % GridSize, (y + j + GridSize) % GridSize) == CellAlive)
                {
                    aliveNeighbours++;
                }
            }
        }

        return aliveNeighbours;
    }

    private static void ResetGrid(Game game)
    {
        Array.Fill(game.currentGrid, CellDead);
    }

    private static void CopyCurrentGrid(Game game)
    {
        Array.Copy(game.currentGrid, game.oldGrid, game.currentGrid.Length);
    }

    private static void DrawGridLines()
    {
        Raylib.DrawRectangleLines(0, 0, Width, Height, GridColor);

        for (int cell = 0; cell < GridSize; cell++)
        {
            Raylib.DrawLine(0, cell * CellSize, Width, cell * CellSize, GridColor);
            Raylib.DrawLine(cell * CellSize, 0, cell * CellSize, Height, GridColor);
        }
    }
}