using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FitLab.Pages
{
    public partial class HomePage : Page
    {
        private readonly User _currentUser;

        public HomePage()
        {
            InitializeComponent();

            var db = new LocalDatabaseService();
            _currentUser = db.LoadFirstUser() ?? throw new Exception("No user found.");

            WelcomeText.Text = $"Welcome back, {_currentUser.Name ?? "User"}!";
            WeekText.Text = $"Week {SessionState.CurrentWeek} of your workout journey";
            DayText.Text = $"Day {SessionState.CurrentWorkoutDay} of your current workout plan";

            var workoutDayNum = SessionState.CurrentWorkoutDay;
            var dayData = _currentUser.WorkoutPlan?.Days.FirstOrDefault(d => d.DayNumber == workoutDayNum);

            if (dayData != null)
            {
                foreach (var ex in dayData.Warmup)
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Warmup"));
                foreach (var ex in dayData.Main)
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Main"));
                foreach (var ex in dayData.Cooldown)
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Cooldown"));
            }

        }
        private StackPanel CreateExerciseNode(Exercise ex, string sectionName)
        {
            var outerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var textPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 600 // Adjust as needed based on your layout
            };

            var mainInfo = new TextBlock
            {
                Text = $"{ex.Name} - {string.Join(", ", ex.Type)} - {ex.MuscleGroup} - {ex.Difficulty} - {string.Join(", ", ex.Equipment)}",
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };

            var description = new TextBlock
            {
                Text = ex.Description,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.LightGray,
                FontSize = 12
            };

            var btn = new Button
            {
                Width = 45,
                Height = 45,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Padding = new Thickness(5),
                Margin = new Thickness(10, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = Cursors.Hand
            };

            var img = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/CheckIcon.png")),
                Width = 30,
                Height = 30,
                Stretch = Stretch.Uniform
            };

            btn.Content = img;
            btn.Click += (s, e) =>
            {
                var modal = new CompleteExModal(ex, sectionName, () =>
                {
                    ExerciseCompletionModal.Visibility = Visibility.Collapsed;
                    ModalOverlay.Visibility = Visibility.Collapsed;
                });

                ExerciseCompletionModal.Content = modal;
                ExerciseCompletionModal.Visibility = Visibility.Visible;
                ModalOverlay.Visibility = Visibility.Visible;
            };


            textPanel.Children.Add(mainInfo);
            textPanel.Children.Add(description);
            outerPanel.Children.Add(textPanel);
            outerPanel.Children.Add(btn);

            return outerPanel;
        }


    }
}
