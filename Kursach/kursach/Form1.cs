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
        private Label iterationCounterLabel; // Метка для отображения количества итераций
        private int iterationCounter = 0; // Счётчик итераций

        private const int CellSize = 8; // Размер клетки
        private const int GridWidth = 100; // Ширина сетки
        private const int GridHeight = 100; // Высота сетки

        public Form1()
        {
            InitializeComponent(); // Инициализация элементов управления
            InitializeGame(); // Инициализация игры
        }

        private void InitializeGame()
        {
            game = new GameLogic(GridWidth, GridHeight);
            gameTimer = new System.Timers.Timer { Interval = 500 };
            gameTimer.Elapsed += OnGameTick;

            this.DoubleBuffered = true;
            random = new Random();
            patterns = new List<Action>
    {
        game.LoadSpaceshipPattern,
        game.LoadPulsarPattern,
        game.LoadPentadecathlonPattern,
        game.LoadGosperGliderGun,
        game.LoadExpandingPattern
    };

            // Метка для отображения количества живых клеток
            livingCellsLabel = new Label
            {
                Text = "Живых клеток: 0",
                Location = new Point(250, 880), // Чуть левее и ниже
                Size = new Size(200, 30),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            // Метка для отображения количества итераций
            iterationCounterLabel = new Label
            {
                Text = "Итерации: 0",
                Location = new Point(10, 900),
                Size = new Size(200, 30),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            // Кнопки управления
            Button startButton = new Button
            {
                Text = "Старт",
                Location = new Point(10, 820),
                Size = new Size(80, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            startButton.Click += (s, e) => gameTimer.Start();

            Button pauseButton = new Button
            {
                Text = "Пауза",
                Location = new Point(100, 820),
                Size = new Size(80, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pauseButton.Click += (s, e) => gameTimer.Stop();

            Button patternButton = new Button
            {
                Text = "Шаблон",
                Location = new Point(190, 820),
                Size = new Size(80, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            patternButton.Click += (s, e) =>
            {
                int index = random.Next(patterns.Count);
                patterns[index](); // Выбор случайного шаблона
                this.Invalidate();
                ResetIterationCounter();
            };

            // Кнопка для сброса счётчика итераций
            Button resetIterationsButton = new Button
            {
                Text = "Сбросить итерации",
                Location = new Point(10, 870),
                Size = new Size(140, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            resetIterationsButton.Click += (s, e) => ResetIterationCounter();

            // Кнопка "Правила игры в жизнь"
            Button rulesButton = new Button
            {
                Text = "Правила",
                Location = new Point(300, 820),
                Size = new Size(100, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            rulesButton.Click += (s, e) => ShowRulesWindow();

            TrackBar speedControl = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Location = new Point(420, 820),
                Size = new Size(200, 30)
            };
            speedControl.Scroll += (s, e) => gameTimer.Interval = speedControl.Value * 100;

            // Добавление элементов на форму
            this.Controls.Add(startButton);
            this.Controls.Add(pauseButton);
            this.Controls.Add(patternButton);
            this.Controls.Add(resetIterationsButton);
            this.Controls.Add(rulesButton);
            this.Controls.Add(speedControl);
            this.Controls.Add(livingCellsLabel);
            this.Controls.Add(iterationCounterLabel);

            this.Paint += DrawGrid;
        }



        private void OnGameTick(object sender, ElapsedEventArgs e)
        {
            game.NextGeneration(); // Обновляем игровое состояние
            iterationCounter++; // Увеличиваем счётчик итераций
            UpdateLivingCellsLabel(); // Обновляем количество живых клеток
            UpdateIterationCounterLabel(); // Обновляем счётчик итераций
            this.Invoke(new Action(this.Invalidate)); // Перерисовка формы
        }

        private void UpdateLivingCellsLabel()
        {
            livingCellsLabel.Text = $"Живых клеток: {game.LivingCellsCount}";
        }

        private void UpdateIterationCounterLabel()
        {
            iterationCounterLabel.Text = $"Итераций: {iterationCounter}";
        }

        private void ResetIterationCounter()
        {
            iterationCounter = 0;
            UpdateIterationCounterLabel();
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

        private void ShowRulesWindow()
        {
            Form rulesForm = new Form
            {
                Text = "Правила игры в жизнь",
                Size = new Size(500, 400)
            };

            Label rulesLabel = new Label
            {
                Text = "Игра «Жизнь» была создана Джоном Конвеем в 1970 году.\n\n" +
                       "Правила:\n" +
                       "1. Любая живая клетка с двумя или тремя живыми соседями остаётся живой.\n" +
                       "2. Любая мёртвая клетка с тремя живыми соседями становится живой.\n" +
                       "3. Все остальные живые клетки умирают в следующем поколении.\n" +
                       "4. Все остальные мёртвые клетки остаются мёртвыми.",
                AutoSize = true,
                Location = new Point(10, 10),
                Font = new Font("Arial", 10)
            };

            rulesForm.Controls.Add(rulesLabel);
            rulesForm.ShowDialog();
        }

    }
}
