using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using Kursach;

namespace kursach
{
    public partial class Form1 : Form
    {
        private GameLogic game;
        private System.Timers.Timer gameTimer;
        private Random random;
        private List<Action> patterns;
        private Label livingCellsLabel; // Метка для отображения количества живых клеток

        private const int CellSize = 8; // Размер клетки
        private const int GridWidth = 100; // Ширина сетки
        private const int GridHeight = 100; // Высота сетки

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            game = new GameLogic(GridWidth, GridHeight); // Поле 100x100
            gameTimer = new System.Timers.Timer { Interval = 500 };
            gameTimer.Elapsed += OnGameTick;

            // Включаем встроенную двойную буферизацию
            this.DoubleBuffered = true;

            // Инициализация случайного генератора
            random = new Random();

            // Список паттернов
            patterns = new List<Action>
            {
                game.LoadSpaceshipPattern,
                game.LoadPulsarPattern,
                game.LoadPentadecathlonPattern,
                game.LoadGosperGliderGun,
                game.LoadExpandingPattern // Новый паттерн
            };


            // Метка для количества живых клеток
            livingCellsLabel = new Label
            {
                Text = "Living Cells: 0",
                Location = new Point(500, 820),
                Size = new Size(200, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Кнопки управления
            Button startButton = new Button { Text = "Start", Location = new Point(10, 820), Size = new Size(80, 30) };
            startButton.Click += (s, e) => gameTimer.Start();

            Button pauseButton = new Button { Text = "Pause", Location = new Point(100, 820), Size = new Size(80, 30) };
            pauseButton.Click += (s, e) => gameTimer.Stop();

            Button patternButton = new Button { Text = "Pattern", Location = new Point(190, 820), Size = new Size(80, 30) };
            patternButton.Click += (s, e) =>
            {
                int index = random.Next(patterns.Count);
                patterns[index](); // Выбор случайного паттерна
                this.Invalidate();
                UpdateLivingCellsLabel(); // Обновляем количество живых клеток
            };

            TrackBar speedControl = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Location = new Point(280, 820),
                Size = new Size(200, 30)
            };
            speedControl.Scroll += (s, e) => gameTimer.Interval = speedControl.Value * 100;

            // Добавление элементов на форму
            this.Controls.Add(startButton);
            this.Controls.Add(pauseButton);
            this.Controls.Add(patternButton);
            this.Controls.Add(speedControl);
            this.Controls.Add(livingCellsLabel);

            this.Paint += DrawGrid;
        }

        private void OnGameTick(object sender, ElapsedEventArgs e)
        {
            game.NextGeneration(); // Обновляем игровое состояние
            UpdateLivingCellsLabel(); // Обновляем количество живых клеток
            this.Invoke(new Action(this.Invalidate)); // Перерисовка формы
        }

        private void DrawGrid(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor); // Очистка фона

            for (int x = 0; x < game.Width; x++)
            {
                for (int y = 0; y < game.Height; y++)
                {
                    Rectangle cellRect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                    g.FillRectangle(game.IsCellAlive(x, y) ? Brushes.Black : Brushes.White, cellRect);
                    g.DrawRectangle(Pens.Gray, cellRect);
                }
            }
        }

        private void UpdateLivingCellsLabel()
        {
            livingCellsLabel.Text = $"Living Cells: {game.LivingCellsCount}";
        }
    }
}
