using System;

namespace Kursach
{
    public class GameLogic
    {
        private bool[,] grid;
        private bool[,] newGrid;

        public int Width { get; }
        public int Height { get; }
        public int LivingCellsCount { get; private set; }

        public GameLogic(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new bool[width, height];
            newGrid = new bool[width, height];
        }

        public bool IsCellAlive(int x, int y) => grid[x, y];

        public void NextGeneration()
        {
            LivingCellsCount = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int neighbors = CountNeighbors(x, y);
                    newGrid[x, y] = grid[x, y] ? neighbors == 2 || neighbors == 3 : neighbors == 3;

                    if (newGrid[x, y])
                        LivingCellsCount++;
                }
            }

            Array.Copy(newGrid, grid, grid.Length);
        }

        private int CountNeighbors(int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;

                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && grid[nx, ny])
                        count++;
                }
            }
            return count;
        }

        private void ClearGrid()
        {
            Array.Clear(grid, 0, grid.Length);
        }

        public void LoadSpaceshipPattern()
        {
            ClearGrid();
            grid[5, 5] = true;
            grid[6, 6] = true;
            grid[6, 7] = true;
            grid[5, 7] = true;
            grid[4, 7] = true;
        }

        public void LoadPulsarPattern()
        {
            ClearGrid();
            for (int i = 2; i <= 4; i++)
            {
                grid[i, 4] = true;
                grid[i, 10] = true;
                grid[4, i + 2] = true;
                grid[10, i + 2] = true;
            }
            for (int i = 8; i <= 10; i++)
            {
                grid[i, 4] = true;
                grid[i, 10] = true;
                grid[4, i - 6] = true;
                grid[10, i - 6] = true;
            }
        }

        public void LoadPentadecathlonPattern()
        {
            ClearGrid();
            grid[10, 10] = true;
            grid[11, 10] = true;
            grid[12, 10] = true;

            grid[13, 9] = true;
            grid[13, 11] = true;

            grid[14, 10] = true;
            grid[15, 10] = true;
            grid[16, 10] = true;
        }

        public void LoadGosperGliderGun()
        {
            ClearGrid();
            grid[5, 1] = true;
            grid[5, 2] = true;
            grid[6, 1] = true;
            grid[6, 2] = true;

            grid[5, 11] = true;
            grid[6, 11] = true;
            grid[7, 11] = true;
            grid[4, 12] = true;
            grid[8, 12] = true;
            grid[3, 13] = true;
            grid[9, 13] = true;
            grid[3, 14] = true;
            grid[9, 14] = true;
            grid[6, 15] = true;
            grid[4, 16] = true;
            grid[8, 16] = true;
            grid[5, 17] = true;
            grid[6, 17] = true;
            grid[7, 17] = true;
            grid[6, 18] = true;

            grid[3, 21] = true;
            grid[4, 21] = true;
            grid[5, 21] = true;
            grid[3, 22] = true;
            grid[4, 22] = true;
            grid[5, 22] = true;
            grid[2, 23] = true;
            grid[6, 23] = true;
            grid[1, 25] = true;
            grid[2, 25] = true;
            grid[6, 25] = true;
            grid[7, 25] = true;

            grid[3, 35] = true;
            grid[4, 35] = true;
            grid[3, 36] = true;
            grid[4, 36] = true;
        }

        public void LoadExpandingPattern()
        {
            ClearGrid();

            int centerX = Width / 2;
            int centerY = Height / 2;

            // Центральная "материнская" структура
            for (int x = centerX - 3; x <= centerX + 3; x++)
            {
                for (int y = centerY - 3; y <= centerY + 3; y++)
                {
                    grid[x, y] = true;
                }
            }

            // Добавляем небольшие дырки для генерации динамики
            grid[centerX, centerY] = false; // Центральная клетка пуста
            grid[centerX - 2, centerY - 2] = false;
            grid[centerX + 2, centerY + 2] = false;
            grid[centerX - 2, centerY + 2] = false;
            grid[centerX + 2, centerY - 2] = false;
        }


        private void AddGlider(int x, int y)
        {
            if (x >= 0 && x + 2 < Width && y >= 0 && y + 2 < Height)
            {
                grid[x + 1, y] = true;
                grid[x + 2, y + 1] = true;
                grid[x, y + 2] = true;
                grid[x + 1, y + 2] = true;
                grid[x + 2, y + 2] = true;
            }
        }

        private void AddGosperGliderGun(int x, int y)
        {
            if (x >= 0 && y >= 0 && x + 36 < Width && y + 9 < Height)
            {
                grid[x + 1, y + 5] = true;
                grid[x + 2, y + 5] = true;
                grid[x + 1, y + 6] = true;
                grid[x + 2, y + 6] = true;

                grid[x + 13, y + 3] = true;
                grid[x + 14, y + 3] = true;
                grid[x + 12, y + 4] = true;
                grid[x + 16, y + 4] = true;
                grid[x + 11, y + 5] = true;
                grid[x + 17, y + 5] = true;
                grid[x + 11, y + 6] = true;
                grid[x + 15, y + 6] = true;
                grid[x + 17, y + 6] = true;
                grid[x + 12, y + 7] = true;
                grid[x + 16, y + 7] = true;
                grid[x + 13, y + 8] = true;
                grid[x + 14, y + 8] = true;

                grid[x + 21, y + 3] = true;
                grid[x + 22, y + 3] = true;
                grid[x + 21, y + 4] = true;
                grid[x + 22, y + 4] = true;

                grid[x + 35, y + 1] = true;
                grid[x + 36, y + 1] = true;
                grid[x + 35, y + 2] = true;
                grid[x + 36, y + 2] = true;

                grid[x + 23, y + 1] = true;
                grid[x + 23, y + 2] = true;
                grid[x + 23, y + 3] = true;
                grid[x + 25, y + 0] = true;
                grid[x + 25, y + 4] = true;
                grid[x + 27, y + 0] = true;
                grid[x + 27, y + 4] = true;
                grid[x + 28, y + 2] = true;
            }
        }

    }
}
