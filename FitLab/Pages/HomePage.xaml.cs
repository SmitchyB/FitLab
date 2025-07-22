using FitLab.Components;
using FitLab.Data;
using System;
using System.Windows;
using System.Windows.Controls;

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
            WeekText.Text = $"Week {CalculateCurrentWeek.GetWeekNumber(_currentUser.CreatedOn, TimeZoneInfo.Local)}";

            LoadTodayWorkoutPlanUI();
        }
        private void LoadTodayWorkoutPlanUI()
        {
            DailyWorkoutItems.Items.Clear();

            var today = DateTime.Today.DayOfWeek.ToString();
            var todayPlan = _currentUser.WorkoutPlan.Days.FirstOrDefault(d => d.DayOfWeek == today);

            if (todayPlan == null) return;

            var allExercises = todayPlan.Warmup.Concat(todayPlan.Main).Concat(todayPlan.Cooldown).ToList();

            foreach (var exercise in allExercises)
            {
                var container = new Border
                {
                    Margin = new Thickness(5),
                    BorderBrush = System.Windows.Media.Brushes.MediumPurple,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8)
                };

                var stack = new StackPanel();

                var checkBtn = new Button
                {
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Tag = exercise.Guid,
                    Content = new Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri("A:\\DotNetApps\\FitLab\\FitLab\\Assets\\Images\\CheckIcon.png")),
                        Width = 20,
                        Height = 20
                    }
                };

                checkBtn.Click += (s, e) =>
                {
                    // TODO: Open completion modal logic
                };

                stack.Children.Add(checkBtn);
                stack.Children.Add(new TextBlock { Text = $"Name: {exercise.Name}", FontWeight = FontWeights.Bold });
                stack.Children.Add(new TextBlock { Text = $"Muscle Group: {exercise.MuscleGroup}" });
                stack.Children.Add(new TextBlock { Text = $"Difficulty: {exercise.Difficulty}" });
                stack.Children.Add(new TextBlock { Text = $"Type: {string.Join(", ", exercise.Type)}" });
                stack.Children.Add(new TextBlock { Text = $"Equipment: {string.Join(", ", exercise.Equipment)}" });
                stack.Children.Add(new TextBlock { Text = $"Description: {exercise.Description}", TextWrapping = TextWrapping.Wrap });

                container.Child = stack;
                DailyWorkoutItems.Items.Add(container);
            }
        }

    }
}
