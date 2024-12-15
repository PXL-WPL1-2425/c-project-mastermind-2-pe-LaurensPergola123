using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private string[] colors = { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        private string[] secretCode;
        private List<Brush> ellipseColor = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.Orange, Brushes.White, Brushes.Green, Brushes.Blue };
        private string[] highscores = new string[15];
        private string spelerNaam;

        private int maxPogingen = 10; // Defaultwaarde
        public int MaxPogingen
        {
            get => maxPogingen;
            set
            {
                maxPogingen = value;
                Title = $"Mastermind - Max Pogingen: {maxPogingen}";
            }
        }

        int attempts = 0;
        int countDown = 10;
        int totalScore = 100;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Stel de DataContext in voor binding met de slider
          
        }

        // --------------------------- Game Initialization Methods ---------------------------

        private void InitializeGame()
        {
            Random number = new Random();
            secretCode = Enumerable.Range(0, 4)
                             .Select(_ => colors[number.Next(colors.Length)])
                             .ToArray();
            cheatCode.Text = string.Join(" ", secretCode);
            totalScore = 100;
            scoreLabel.Text = $"Score: {totalScore}";
            attempts = 0;
            countDown = 10;
            historyPanel.Children.Clear();
            ResetAllColors();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countDown--;
            timerCounter.Text = $"{countDown}";
            if (countDown == 0)
            {
                attempts++;
                timer.Stop();
                if (attempts >= maxPogingen)
                {
                    GameOver();
                    return;
                }
                MessageBox.Show("Poging kwijt");
                StopCountDown();
                UpdateTitle();
            }
        }

        private void StartCountDown()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StopCountDown()
        {
            timer.Stop();
            countDown = 10;
            timer.Start();
        }

        private void GameOver()
        {
            AddHighscore(spelerNaam, attempts, totalScore);

            if (MessageBox.Show($"Game Over! De code was: {string.Join(" ", secretCode)}\nWil je een nieuw spel starten?",
                                "Game Over",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                StartNewGame_Click(null, null);
            }
            else
            {
                this.Close();
            }
        }

        // --------------------------- Player Interaction Methods ---------------------------

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };
            string[] selectedColors = ellipses.Select(e => GetColorName(e.Fill)).ToArray();

            if (selectedColors.Any(color => color == "Transparent"))
            {
                MessageBox.Show("Selecteer vier kleuren!", "Foutief", MessageBoxButton.OK);
                return;
            }

            attempts++;
            UpdateTitle();

            if (attempts >= maxPogingen)
            {
                GameOver();
                return;
            }

            CheckGuess(selectedColors);
            UpdateScoreLabel(selectedColors);
            StopCountDown();
        }

        private void CheckGuess(string[] selectedColors)
        {
            int correctPosition = 0;
            int correctColor = 0;

            List<string> tempSecretCode = new List<string>(secretCode);
            List<string> tempPlayerGuess = new List<string>(selectedColors);

            List<Brush> feedbackBorders = new List<Brush>();

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] == tempSecretCode[i])
                {
                    correctPosition++;
                    feedbackBorders.Add(Brushes.DarkRed);
                    tempSecretCode[i] = null;
                    tempPlayerGuess[i] = null;
                }
                else
                {
                    feedbackBorders.Add(null);
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (tempPlayerGuess[i] != null && tempSecretCode.Contains(tempPlayerGuess[i]))
                {
                    int indexInSecretCode = tempSecretCode.IndexOf(tempPlayerGuess[i]);

                    if (indexInSecretCode >= 0)
                    {
                        feedbackBorders[i] = Brushes.Wheat;
                        tempSecretCode[indexInSecretCode] = null;
                    }
                }
            }

            for (int i = 0; i < tempPlayerGuess.Count; i++)
            {
                if (feedbackBorders[i] == null)
                {
                    feedbackBorders[i] = Brushes.Transparent;
                }
            }

            if (correctPosition == 4)
            {
                AddHighscore(spelerNaam, attempts, totalScore);
                timer.Stop();
                if (MessageBox.Show($"Proficiat! Je hebt de code gekraakt in {attempts} pogingen!\rSpel herstarten?",
                                    "WINNER WINNER CHICKEN DINNER", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    InitializeGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }

            AddAttemptToHistory(selectedColors, feedbackBorders);
        }

        private void AddAttemptToHistory(string[] selectedColors, List<Brush> feedbackBorders)
        {
            StackPanel attemptPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            for (int i = 0; i < selectedColors.Length; i++)
            {
                Ellipse colorBox = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = GetBrushFromColorName(selectedColors[i]),
                    StrokeThickness = 5,
                    Stroke = feedbackBorders[i]
                };
                attemptPanel.Children.Add(colorBox);
            }

            historyPanel.Children.Add(attemptPanel);
        }

        private void UpdateScoreLabel(string[] selectedColors)
        {
            int scorePenalty = 0;

            for (int i = 0; i < selectedColors.Length; i++)
            {
                if (selectedColors[i] == secretCode[i])
                {
                    continue;
                }
                else if (secretCode.Contains(selectedColors[i]))
                {
                    scorePenalty += 1;
                }
                else
                {
                    scorePenalty += 2;
                }
            }

            totalScore -= scorePenalty;
            if (totalScore < 0) totalScore = 0;

            scoreLabel.Text = $"Score: {totalScore}";
        }

        private void UpdateTitle()
        {
            this.Title = $"Poging {attempts}";
        }

        // --------------------------- Helper Methods ---------------------------

        private void ResetAllColors()
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };

            foreach (Ellipse ellipse in ellipses)
            {
                ellipse.Fill = Brushes.Red;
                ellipse.Stroke = Brushes.Transparent;
            }
        }

        private string GetColorName(Brush brush)
        {
            if (brush == Brushes.Red) return "Red";
            if (brush == Brushes.Yellow) return "Yellow";
            if (brush == Brushes.Orange) return "Orange";
            if (brush == Brushes.White) return "White";
            if (brush == Brushes.Green) return "Green";
            if (brush == Brushes.Blue) return "Blue";
            return "Transparent";
        }

        private Brush GetBrushFromColorName(string colorName)
        {
            switch (colorName)
            {
                case "Red": return Brushes.Red;
                case "Yellow": return Brushes.Yellow;
                case "Orange": return Brushes.Orange;
                case "White": return Brushes.White;
                case "Green": return Brushes.Green;
                case "Blue": return Brushes.Blue;
                default: return Brushes.Transparent;
            }
        }

        private void Toggledebug()
        {
            if (cheatCode.Visibility == Visibility.Hidden)
            {
                cheatCode.Visibility = Visibility.Visible;
            }
            else if (cheatCode.Visibility == Visibility.Visible)
            {
                cheatCode.Visibility = Visibility.Hidden;
            }
        }

        private void CheatCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (cheatCode.Visibility == Visibility.Hidden)
            {
                cheatCode.Visibility = Visibility.Visible;
                Title = "Mastermind (DEBUG MODE)";
            }
            else
            {
                cheatCode.Visibility = Visibility.Hidden;
                Title = "Mastermind";
            }
        }

        private void color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse clickedEllipse)
            {
                int currentColorIndex = ellipseColor.IndexOf(clickedEllipse.Fill);
                int nextColorIndex = (currentColorIndex + 1) % ellipseColor.Count;
                clickedEllipse.Fill = ellipseColor[nextColorIndex];
            }
        }

        private string StartGame()
        {
            string naam = Microsoft.VisualBasic.Interaction.InputBox(
                "Welkom bij Mastermind! Voer je naam in om te beginnen (druk op Annuleren om het spel niet te starten):",
                "Speler Naam", "");

            return string.IsNullOrWhiteSpace(naam) ? null : naam;
        }

        private void StartNewGame_Click(object sender, RoutedEventArgs e)
        {
            // Stop de timer als deze actief is
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            // Vraag de gebruiker om een naam in te voeren
            spelerNaam = StartGame();

            if (!string.IsNullOrEmpty(spelerNaam))
            {
                // Initialiseer het spel en start de timer als een naam is ingevoerd
                InitializeGame();
                StartCountDown();
            }
            else
            {
                // Als er geen naam is ingevoerd, geef een waarschuwing en laat de timer uit
                MessageBox.Show("Geen naam ingevoerd. Timer start niet.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SortHighscores()
        {
            highscores = highscores
                .Where(h => h != null)
                .OrderByDescending(h => int.Parse(h.Split('-')[2].Trim().Split('/')[0]))
                .Concat(Enumerable.Repeat<string>(null, highscores.Length))
                .Take(highscores.Length)
                .ToArray();
        }

        private void AddHighscore(string spelerNaam, int pogingen, int score)
        {
            // Maak de nieuwe highscore
            string nieuweScore = $"{spelerNaam} - {pogingen} pogingen - {score}/100";

            // Zoek de eerste lege plaats in de highscores-array
            for (int i = 0; i < highscores.Length; i++)
            {
                if (highscores[i] == null)
                {
                    highscores[i] = nieuweScore;
                    SortHighscores();
                    return;
                }
            }

            // Als de lijst vol is, vervang de slechtste score
            highscores[highscores.Length - 1] = nieuweScore;
            SortHighscores();
        }


        private void HighScores_Click(object sender, RoutedEventArgs e)
        {
            if (highscores.All(h => h == null))
            {
                MessageBox.Show("Er zijn nog geen highscores!", "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string highscoreTekst = string.Join("\n", highscores.Where(h => h != null));
            MessageBox.Show(highscoreTekst, "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
