using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FitLab.Pages
{
    public partial class WorkoutPlanPage : Page
    {
        private User _currentUser = new();

        public WorkoutPlanPage()
        {
            InitializeComponent();
            LoadUserAndRefresh();
        }

        private void LoadUserAndRefresh()
        {
            Debug.WriteLine("[WorkoutPlanPage] Loading user...");
            var currentUserId = new LocalDatabaseService().LoadCurrentUserId();
            var loadedUser = currentUserId.HasValue ? new LocalDatabaseService().LoadFirstUser() : null;
            _currentUser = loadedUser ?? throw new InvalidOperationException("Failed to load user.");

            _currentUser.WorkoutPlan ??= new WorkoutPlan();

            Debug.WriteLine($"[WorkoutPlanPage] Loaded user with {_currentUser.WorkoutPlan.Days.Count} workout days.");
            RefreshAllDays();
        }

        private void AddExercise_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            var parent = button.Parent;
            if (parent is StackPanel panel && panel.Parent is Border border)
            {
                var expander = VisualTreeHelper.GetParent(border);
                while (expander is not Expander && expander != null)
                    expander = VisualTreeHelper.GetParent(expander);

                if (expander is Expander exp)
                {
                    string day = exp.Header?.ToString() ?? "";
                    string section = "";

                    foreach (var child in panel.Children)
                    {
                        if (child is TextBlock tb && tb.Text is string label)
                        {
                            if (label.Contains("Warmup")) section = "Warmup";
                            else if (label.Contains("Main")) section = "Main";
                            else if (label.Contains("Cooldown")) section = "Cooldown";
                        }
                    }

                    Debug.WriteLine($"[WorkoutPlanPage] Opening modal for {day} - {section}");

                    if (string.IsNullOrEmpty(day) || string.IsNullOrEmpty(section))
                    {
                        Debug.WriteLine("[WorkoutPlanPage] Missing day or section, aborting modal.");
                        return;
                    }

                    var modal = new ExerciseModal(day, section);
                    bool? result = modal.ShowDialog();

                    if (result == true)
                    {
                        Debug.WriteLine("[WorkoutPlanPage] Modal returned true, refreshing data from DB.");
                        LoadUserAndRefresh();
                    }
                }
            }
        }

        private void RefreshAllDays()
        {
            foreach (var day in _currentUser.WorkoutPlan.Days.Select(d => d.DayOfWeek))
            {
                Debug.WriteLine($"[WorkoutPlanPage] Refreshing day: {day}");
                RefreshUIForDay(day);
            }
        }

        private void RefreshUIForDay(string day)
        {
            var daily = _currentUser.WorkoutPlan.Days.FirstOrDefault(d => d.DayOfWeek == day);
            if (daily == null)
            {
                Debug.WriteLine($"[WorkoutPlanPage] No daily workout found for {day}");
                return;
            }

            string[] sections = new[] { "Warmup", "Main", "Cooldown" };

            foreach (var section in sections)
            {
                var itemsControl = FindName($"{day}{section}Exercises") as ItemsControl;
                if (itemsControl == null)
                {
                    Debug.WriteLine($"[WorkoutPlanPage] Could not find ItemsControl for {day}{section}Exercises");
                    continue;
                }

                List<Exercise>? exercises = section switch
                {
                    "Warmup" => daily.Warmup,
                    "Main" => daily.Main,
                    "Cooldown" => daily.Cooldown,
                    _ => null
                };

                itemsControl.Items.Clear();

                if (exercises == null || exercises.Count == 0)
                {
                    Debug.WriteLine($"[WorkoutPlanPage] {day} - {section} is empty.");
                    continue;
                }

                Debug.WriteLine($"[WorkoutPlanPage] {day} - {section} has {exercises.Count} exercises.");

                foreach (var exercise in exercises)
                {
                    var container = new Border
                    {
                        Margin = new Thickness(5),
                        BorderBrush = Brushes.MediumPurple,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(8)
                    };

                    var stack = new StackPanel();

                    var deleteBtn = new Button
                    {
                        Content = "❌",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Width = 25,
                        Height = 25,
                        Tag = exercise.Guid
                    };
                    deleteBtn.Click += (s, e) => DeleteExercise(day, section, (Guid)((Button)s).Tag);
                    stack.Children.Add(deleteBtn);

                    stack.Children.Add(new TextBlock { Text = $"Name: {exercise.Name}", FontWeight = FontWeights.Bold });
                    stack.Children.Add(new TextBlock { Text = $"Muscle Group: {exercise.MuscleGroup}" });
                    stack.Children.Add(new TextBlock { Text = $"Difficulty: {exercise.Difficulty}" });
                    stack.Children.Add(new TextBlock { Text = $"Type: {string.Join(", ", exercise.Type)}" });
                    stack.Children.Add(new TextBlock { Text = $"Equipment: {string.Join(", ", exercise.Equipment)}" });
                    stack.Children.Add(new TextBlock { Text = $"Description: {exercise.Description}", TextWrapping = TextWrapping.Wrap });

                    container.Child = stack;
                    itemsControl.Items.Add(container);
                }
            }
        }

        private void DeleteExercise(string day, string section, Guid guid)
        {
            var daily = _currentUser.WorkoutPlan.Days.FirstOrDefault(d => d.DayOfWeek == day);
            if (daily == null) return;

            var list = section switch
            {
                "Warmup" => daily.Warmup,
                "Main" => daily.Main,
                "Cooldown" => daily.Cooldown,
                _ => null
            };

            if (list == null) return;

            var target = list.FirstOrDefault(e => e.Guid == guid);
            if (target != null)
            {
                list.Remove(target);
                new LocalDatabaseService().SaveUser(_currentUser);
                RefreshUIForDay(day);
            }
        }

    }
}