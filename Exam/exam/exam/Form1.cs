using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace exam
{
    public partial class Form1 : Form
    {
        private GameLogic game;
        private Button[,] gridButtons;
        private int gridSize = 4;
        private int timeLeft;
        private int attemptsLeft;
        private int currentLevel;
        private const int maxLevels = 5;
        private double memorizationTime = 1.5; 
        private System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        private Label lblDifficulty;
        private Button btnEasy;
        private Button btnMedium;
        private Button btnHard;

        public Form1()
        {
            InitializeComponent();
            game = new GameLogic(gridSize);
            gridButtons = new Button[gridSize, gridSize];
            currentLevel = 1;
            attemptsLeft = 3;
            InitializeGame();
            InitializeTimer();
            InitializeDifficultyButtons();
        }

        private void InitializeGame()
        {
            CreateGrid();
            StartNewLevel();
        }

        private void InitializeTimer()
        {
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimerTick;
        }

        private void InitializeDifficultyButtons()
        {
            lblDifficulty = new Label
            {
                Text = "Выберите уровень сложности",
                Location = new Point(20, 330), 
                AutoSize = true
            };
            this.Controls.Add(lblDifficulty);

            btnEasy = new Button
            {
                Text = "Легкий",
                Location = new Point(20, 360), 
                Size = new Size(80, 30)
            };
            btnEasy.Click += (s, e) => SetDifficulty(1.5);
            this.Controls.Add(btnEasy);

            btnMedium = new Button
            {
                Text = "Средний",
                Location = new Point(120, 360),
                Size = new Size(80, 30)
            };
            btnMedium.Click += (s, e) => SetDifficulty(1.2);
            this.Controls.Add(btnMedium);

            btnHard = new Button
            {
                Text = "Сложный",
                Location = new Point(220, 360), 
                Size = new Size(80, 30)
            };
            btnHard.Click += (s, e) => SetDifficulty(0.8);
            this.Controls.Add(btnHard);
        }

        private void SetDifficulty(double time)
        {
            memorizationTime = time;
            MessageBox.Show($"Выбран уровень сложности: {time} сек. на запоминание");
        }

        private void CreateGrid()
        {
            int buttonSize = 60;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(j * buttonSize + 20, i * buttonSize + 50),
                        BackColor = Color.LightGray,
                        Tag = new Point(i, j)
                    };
                    btn.Click += GridButtonClick;
                    this.Controls.Add(btn);
                    gridButtons[i, j] = btn;
                }
            }
        }

        private void StartNewLevel()
        {
            if (currentLevel > maxLevels)
            {
                MessageBox.Show("Поздравляем! Вы прошли все уровни!");
                currentLevel = 1;
                attemptsLeft = 3;
            }
            ClearGrid();
            game.GenerateNewPattern(currentLevel + 2);
            ShowPattern();
            timeLeft = 30;
            lblLevel.Text = "Уровень: " + currentLevel;
            lblAttempts.Text = "Попытки: " + attemptsLeft;
            gameTimer.Start();
            System.Windows.Forms.Timer displayTimer = new System.Windows.Forms.Timer { Interval = (int)(memorizationTime * 1000) };
            displayTimer.Tick += (s, e) => { HidePattern(); displayTimer.Stop(); };
            displayTimer.Start();
        }

        private void GameTimerTick(object? sender, EventArgs e)
        {
            timeLeft--;
            lblTimer.Text = "Время: " + timeLeft;

            if (timeLeft <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("Время вышло! Начнем заново.");
                currentLevel = 1;
                attemptsLeft = 3;
                StartNewLevel();
            }
        }

        private void ShowPattern()
        {
            foreach (var pos in game.Pattern)
            {
                gridButtons[pos.X, pos.Y].BackColor = Color.Orange;
            }
        }

        private void HidePattern()
        {
            foreach (var btn in gridButtons)
            {
                btn.BackColor = Color.LightGray;
            }
        }

        private void ClearGrid()
        {
            foreach (var btn in gridButtons)
            {
                btn.BackColor = Color.LightGray;
            }
        }

        private void GridButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Point pos)
            {
                if (game.CheckPosition(pos))
                {
                    btn.BackColor = Color.Green;
                    if (game.IsLevelCompleted())
                    {
                        gameTimer.Stop();
                        MessageBox.Show("Уровень пройден!");
                        currentLevel++;
                        StartNewLevel();
                    }
                }
                else
                {
                    btn.BackColor = Color.Red;
                    attemptsLeft--;
                    lblAttempts.Text = "Попытки: " + attemptsLeft;
                    if (attemptsLeft == 0)
                    {
                        gameTimer.Stop();
                        MessageBox.Show("Вы проиграли! Начнем заново.");
                        currentLevel = 1;
                        attemptsLeft = 3;
                        StartNewLevel();
                    }
                }
            }
        }
    }
}
