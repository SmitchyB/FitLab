using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Components
{
    public partial class CompleteExModal : UserControl
    {
        private readonly Exercise _exercise;
        private readonly string _section;
        private readonly Action? OnComplete;

        public CompleteExModal(Exercise exercise, string section, Action? onComplete = null)
        {
            InitializeComponent();
            _exercise = exercise;
            _section = section;
            OnComplete = onComplete;

            TxtName.Text = _exercise.Name;
            TxtSection.Text = _section;
            TxtType.Text = string.Join(", ", _exercise.Type);
            RenderInputsForTrackingMetrics(_exercise.TrackingMetrics);
        }

        private void RenderInputsForTrackingMetrics(List<string> metrics)
        {
            InputPanel.Children.Clear();

            foreach (var metric in metrics)
            {
                string? label = metric switch
                {
                    "Sets" => "Sets:",
                    "Reps Per Set" => "Reps Per Set:",
                    "Weight Used" => "Weight Used (lbs):",
                    "Rest Between Sets" => "Rest Between Sets (sec):",
                    "RPE" => "RPE:",
                    "Failure Reached" => "Failure Reached (true/false):",
                    "Hold Duration" => "Hold Duration (seconds):",
                    "Discomfort Level" => "Discomfort Level:",
                    "Duration" => "Duration (minutes):",
                    "Intensity" => "Intensity (1–10):",
                    "Distance" => "Distance (miles):",
                    "Speed" => "Speed (mph):",
                    _ => null
                };

                if (label != null)
                    InputPanel.Children.Add(CreateLabeledTextbox(label));
            }
        }

        private static StackPanel CreateLabeledTextbox(string label)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White
            });

            panel.Children.Add(new TextBox
            {
                Height = 25,
                Tag = label,
                Background = System.Windows.Media.Brushes.White
            });

            return panel;
        }

        private Dictionary<string, string> GetTextBoxValues()
        {
            var map = new Dictionary<string, string>();

            foreach (var child in InputPanel.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var inner in sp.Children)
                    {
                        if (inner is TextBox tb && tb.Tag is string tag)
                        {
                            map[tag] = tb.Text;
                        }
                    }
                }
            }

            return map;
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var db = new LocalDatabaseService();
            var user = db.LoadFirstUser();
            if (user == null) return;

            var inputValues = GetTextBoxValues();

            // Remove the colon from the label to use as clean key
            var cleanedMetrics = inputValues.ToDictionary(
                kv => kv.Key.TrimEnd(':'),
                kv => kv.Value
            );

            var existing = user.CompletedExercises.FirstOrDefault(e => e.ExerciseId == _exercise.Guid);
            if (existing == null)
            {
                existing = new CompletedExercise
                {
                    ExerciseId = _exercise.Guid,
                    SubType = string.Join(", ", _exercise.Type)
                };
                user.CompletedExercises.Add(existing);
            }

            existing.Entries.Add(new CompletedExerciseEntry
            {
                DateCompleted = DateTime.UtcNow,
                Metrics = cleanedMetrics
            });

            db.SaveUser(user);
            OnComplete?.Invoke();
            Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
