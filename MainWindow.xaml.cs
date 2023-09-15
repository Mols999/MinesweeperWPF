using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MinesweeperWPF
{
    public partial class MainWindow : Window
    {
        // Constants to define grid size and mine count
        private const int gridSize = 9;
        private const int mineCount = 10;

        // Game state variables
        private bool isGameOver = false;
        private bool gameStarted = false;
        private DateTime startTime;
        private DispatcherTimer timer = new DispatcherTimer();

        // Arrays to store grid buttons, mine locations, and revealed cells
        private Button[,] gridButtons = new Button[gridSize, gridSize];
        private bool[,] isMine = new bool[gridSize, gridSize];
        private bool[,] isRevealed = new bool[gridSize, gridSize];

      
     

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Initialize the grid
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Button button = new Button
                    {
                        Name = $"btnMineField_{x}_{y}",
                        Width = 30,
                        Height = 30,
                        Tag = new Point(x, y),
                        Content = "",
                    };
                    button.Click += BtnMineField_Click;
                    gameGrid.Children.Add(button);
                    Grid.SetRow(button, x);
                    Grid.SetColumn(button, y);
                    gridButtons[x, y] = button;
                }
            }

            // Randomly place mines on the grid
            Random random = new Random();
            int minesPlaced = 0;
            while (minesPlaced < mineCount)
            {
                int x = random.Next(0, gridSize);
                int y = random.Next(0, gridSize);
                if (!isMine[x, y])
                {
                    isMine[x, y] = true;
                    minesPlaced++;
                }
            }

            // Initialize the timer and start the game
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer1_Tick;
            timer.Start();
            startTime = DateTime.Now;
            gameStarted = true;
        }

        // Event handler for mouse clicks on grid cells
        private void BtnMineField_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver)
                return;

            Button clickedButton = (Button)sender;
            Point position = (Point)clickedButton.Tag;
            int x = (int)position.X;
            int y = (int)position.Y;

            if (!isRevealed[x, y])
            {
                if (isMine[x, y])
                {
                    // Handle game over (player clicked on a mine)
                    EndGame(false);
                }
                else
                {
                    // Reveal the cell and check neighboring mines
                    RevealCell(x, y);

                    if (CheckForVictory())
                    {
                        // Player has won the game
                        EndGame(true);
                    }
                }
            }
        }

        private void EndGame(bool playerWon)
        {
            isGameOver = true;

            string message;
            string elapsedTimeStr = lblElapsedTime.Content.ToString();

            if (playerWon)
            {
                message = $"Congratulations! You won!\nTime: {elapsedTimeStr}";
            }
            else
            {
                message = $"Game Over. You clicked on a mine.\nTime: {elapsedTimeStr}";

                // Highlight all mine cells when the player loses
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        if (isMine[x, y])
                        {
                            gridButtons[x, y].Background = Brushes.Red;
                        }
                    }
                }
            }

            // Show a message box to inform the player
            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private int CountNeighboringMines(int x, int y)
        {
            int count = 0;

            // Offsets for neighboring cells
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            // Check each neighboring cell for mines
            for (int i = 0; i < 8; i++)
            {
                int newX = x + dx[i];
                int newY = y + dy[i];

                if (newX >= 0 && newX < gridSize && newY >= 0 && newY < gridSize)
                {
                    if (isMine[newX, newY])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void RevealCell(int x, int y)
        {
            if (isRevealed[x, y] || isGameOver)
                return;

            isRevealed[x, y] = true;

            gridButtons[x, y].Background = Brushes.White;

            int neighboringMines = CountNeighboringMines(x, y);

            if (neighboringMines > 0)
            {
                gridButtons[x, y].Content = neighboringMines.ToString();
            }
            else
            {
                // Recursive reveal of neighboring cells with no mines
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int newX = x + i;
                        int newY = y + j;

                        if (newX >= 0 && newX < gridSize && newY >= 0 && newY < gridSize)
                        {
                            if (newX == x && newY == y)
                                continue;

                            RevealCell(newX, newY);
                        }
                    }
                }
            }
        }

        private bool CheckForVictory()
        {
            int nonMineCellsCount = gridSize * gridSize - mineCount;
            int revealedNonMineCellsCount = 0;

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (!isMine[x, y] && isRevealed[x, y])
                    {
                        revealedNonMineCellsCount++;
                    }
                }
            }

            return revealedNonMineCellsCount == nonMineCellsCount;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (gameStarted && !isGameOver)
            {
                // Update elapsed time on the UI
                TimeSpan elapsedTime = DateTime.Now - startTime;
                lblElapsedTime.Content = $"Time: {elapsedTime.ToString(@"hh\:mm\:ss")}";
            }
        }
    }
}
