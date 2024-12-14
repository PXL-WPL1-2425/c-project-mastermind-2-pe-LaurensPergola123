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

        int attempts = 0;
        int countDown = 10;
        int totalScore = 100;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            StartCountDown();
        }


        // --------------------------- Game Initialization Methods ---------------------------

        private void InitializeGame()
        {
            StartGame();
            Random number = new Random();
            secretCode = Enumerable.Range(0, 4)
                             .Select(_ => colors[number.Next(colors.Length)])
                             .ToArray();
            cheatCode.Text = string.Join(" ", secretCode);
            Title = ($"{attempts}");
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
                if (attempts >= 10)
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
            // Voeg de huidige score toe aan de highscores
            AddHighscore(spelerNaam, attempts, totalScore);

            // Toon een bericht met de optie om een nieuw spel te starten
            if (MessageBox.Show($"Game Over! De code was: {string.Join(" ", secretCode)}\nWil je een nieuw spel starten?",
                                "Game Over",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                StartNewGame_Click(null, null); // Start een nieuw spel via het menu
            }
            else
            {
                this.Close(); // Sluit de applicatie als de speler niet wil doorgaan
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

            if (attempts >= 10)
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
            if (totalScore < 0) totalScore = 0; // Ensure score doesn't go negative

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

        // --------------------------- Debugging Methods ---------------------------

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
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                Toggledebug();
            }
        }

        // --------------------------- Event Handlers ---------------------------

        private void color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse clickedEllipse = sender as Ellipse;
            int currentColorIndex = ellipseColor.IndexOf(clickedEllipse.Fill);
            int nextColorIndex = (currentColorIndex + 1) % ellipseColor.Count;
            clickedEllipse.Fill = ellipseColor[nextColorIndex];
        }

       
        // --------------------------- Menu Options ---------------------------

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       

        private string StartGame()
        {
            string naam = string.Empty;

            while (true)
            {
                naam = Microsoft.VisualBasic.Interaction.InputBox(
                    "Welkom bij Mastermind!\n\nVoer je naam in om te beginnen (druk op Annuleren om het spel niet te starten):",
                    "Speler Naam",
                    ""
                );

                if (string.IsNullOrWhiteSpace(naam))
                {
                    if (MessageBox.Show("Je hebt geen naam ingevoerd. Wil je het spel starten zonder te spelen?",
                                        "Geen Naam Ingegeven",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                        

                    {
                        
                        spelerNaam = null; // Geen naam, spel start niet
                        return null;
                    }
                }
                else
                {
                    spelerNaam = naam; // Stel de naam in
                    MessageBox.Show($"Welkom, {spelerNaam}! Veel succes met Mastermind!", "Speler Naam Geaccepteerd", MessageBoxButton.OK, MessageBoxImage.Information);
                    return spelerNaam;
                }
            }
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


        // Sorteer highscores op basis van score
        private void SortHighscores()
        {
            highscores = highscores
                .Where(h => h != null)
                .OrderByDescending(h => int.Parse(h.Split('-')[2].Trim().Split('/')[0]))
                .Concat(Enumerable.Repeat<string>(null, highscores.Length))
                .Take(highscores.Length)
                .ToArray();
        }


        private void StartNewGame_Click(object sender, RoutedEventArgs e)
        {
            string naam = StartGame();

            if (!string.IsNullOrEmpty(naam))
            {
                spelerNaam = naam;
                Title = $"Mastermind - Speler: {spelerNaam}";
                InitializeGame();
                StartCountDown();
            }
            else
            {
                MessageBox.Show("Het spel is niet gestart. Kies opnieuw 'Nieuw Spel' in het menu om te beginnen.",
                                "Spel Niet Gestart",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void HighScores_Click(object sender, RoutedEventArgs e)
        {
            // Controleer of er highscores zijn
            if (highscores.All(h => h == null))
            {
                MessageBox.Show("Er zijn nog geen highscores!", "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Toon de highscores
            string highscoreTekst = string.Join("\n", highscores.Where(h => h != null));
            MessageBox.Show(highscoreTekst, "Highscores", MessageBoxButton.OK, MessageBoxImage.Information);
        }



    }
}
