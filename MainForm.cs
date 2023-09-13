using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    public class MainForm : Form
    {
        private const int TileSize = 32;
        private const int GridSize = 25;

        private List<Point> snake;
        private Point food;
        private Point direction;
        private System.Windows.Forms.Timer gameTimer;
        private Random random;
        private bool gameStarted;
        private int score;

        private CustomPanel gamePanel;
        private Label scoreLabel;
        private Label currentScoreLabel;

        private Image appleImage;
        private readonly Image snakeImage;

        public MainForm()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            InitializeGame();

            snakeImage = Image.FromFile("Images/snake.png");
            // Establecer el icono del programa
            Icon = ConvertImageToIcon(snakeImage ?? throw new ArgumentNullException(nameof(snakeImage), "La imagen de la manzana no puede ser nula."));
        }

        static Icon ConvertImageToIcon(Image image)
        {
            Bitmap bitmap = (Bitmap)image;
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            return icon;
        }

        private void InitializeComponent()
        {
            ClientSize = new Size(GridSize * TileSize, GridSize * TileSize);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            gamePanel = new CustomPanel()
            {
                Location = new Point(0, 0),
                Size = new Size(GridSize * TileSize, GridSize * TileSize),
                BackColor = Color.Green,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(4),
            };
            gamePanel.Paint += DrawGame;
            Controls.Add(gamePanel);

            scoreLabel = new Label()
            {
                AutoSize = true
            };
            scoreLabel.Font = new Font(scoreLabel.Font.FontFamily, 14);
            scoreLabel.Location = new Point(10, 10);
            scoreLabel.Text = "Score: 0";
            Controls.Add(scoreLabel);

            currentScoreLabel = new Label()
            {
                AutoSize = true
            };
            currentScoreLabel.Font = new Font(currentScoreLabel.Font.FontFamily, 14);
            currentScoreLabel.Location = new Point(10, 40);
            currentScoreLabel.Text = "Current Score: 0";
            Controls.Add(currentScoreLabel);

            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown([AllowNull] object sender, KeyEventArgs e)
        {
            if (!gameStarted)
            {
                gameTimer.Start();
                gameStarted = true;
            }

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (direction.Y != 1 || snake.Count == 1)
                        direction = new Point(0, -1);
                    break;
                case Keys.Down:
                    if (direction.Y != -1 || snake.Count == 1)
                        direction = new Point(0, 1);
                    break;
                case Keys.Left:
                    if (direction.X != 1 || snake.Count == 1)
                        direction = new Point(-1, 0);
                    break;
                case Keys.Right:
                    if (direction.X != -1 || snake.Count == 1)
                        direction = new Point(1, 0);
                    break;
            }
        }

        private void InitializeGame()
        {
            appleImage = Image.FromFile("Images/apple.png");

            snake = new List<Point>()
            {
                new Point(GridSize / 2, GridSize / 2)
            };
            food = GenerateFoodPosition();

            direction = new Point(1, 0);
            gameTimer = new System.Windows.Forms.Timer()
            {
                Interval = 100
            };
            gameTimer.Tick += GameTick;

            score = 0;
            scoreLabel.Text = "Score: 0";
            currentScoreLabel.Text = "Current Score: 0";
        }

        private void GameTick([AllowNull] object sender, EventArgs e)
        {
            Point newHead = new(snake[0].X + direction.X, snake[0].Y + direction.Y);

            if (newHead.X < 0 || newHead.X >= GridSize || newHead.Y < 0 || newHead.Y >= GridSize)
            {
                EndGame();
                return;
            }

            if (snake.Contains(newHead))
            {
                EndGame();
                return;
            }

            snake.Insert(0, newHead);

            if (newHead == food)
            {
                score++;
                scoreLabel.Text = "Score: " + score.ToString();
                currentScoreLabel.Text = "Current Score: " + score.ToString();
                food = GenerateFoodPosition();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }

            gamePanel.Invalidate();
        }

        private void EndGame()
        {
            gameTimer.Stop();
            MessageBox.Show("¡Has perdido! Tu puntuación final es: " + score.ToString());
            InitializeGame();
            Application.Exit();
        }

        private void DrawGame([AllowNull] object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            canvas.Clear(Color.Black);

            float appleScale = 1.2f;
            int appleSize = (int)(TileSize * appleScale);

            for (int i = 0; i < snake.Count; i++)
            {
                int colorValue = 255 - i * (255 / snake.Count);
                Color segmentColor = Color.FromArgb(colorValue, colorValue, colorValue);

                if (i == 0)
                {
                    canvas.FillRectangle(new SolidBrush(segmentColor), snake[i].X * TileSize, snake[i].Y * TileSize, TileSize, TileSize);
                }
                else
                {
                    canvas.FillRectangle(new SolidBrush(segmentColor), snake[i].X * TileSize, snake[i].Y * TileSize, TileSize, TileSize);
                }
            }

            canvas.DrawImage(appleImage, food.X * TileSize, food.Y * TileSize, appleSize, appleSize);

            // Dibujar el borde
            using Pen borderPen = new(Color.Green, 5);
            canvas.DrawRectangle(borderPen, 0, 0, GridSize * TileSize, GridSize * TileSize);

            // Dibujar el marcador de puntuación
            using Font font = new(scoreLabel.Font.FontFamily, 18, FontStyle.Bold);
            using SolidBrush brush = new(Color.White);
            canvas.DrawString("Score: " + score.ToString(), font, brush, new PointF(10, 10));
        }

        private Point GenerateFoodPosition()
        {
            random = new Random();
            int x = random.Next(1, GridSize - 1);
            int y = random.Next(1, GridSize - 1);
            return new Point(x, y);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class CustomPanel : Panel
    {
        public CustomPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using Pen pen = new Pen(Color.Gray, 15);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
