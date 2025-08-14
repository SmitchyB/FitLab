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
        private readonly Exercise _exercise; // exercise being completed
        private readonly string _section; // section name (Warmup/Main/Cooldown)
        private readonly Action? OnComplete; // callback for completion
        private readonly Action? OnCancel; // callback for cancel
        public CompleteExModal(Exercise exercise, string section, Action? onComplete, Action? onCancel)
        {
            InitializeComponent(); // init UI
            _exercise = exercise; // store exercise
            _section = section; // store section
            OnComplete = onComplete; // store completion callback
            OnCancel = onCancel; // store cancel callback
            TxtName.Text = _exercise.Name; // set exercise name
            TxtSection.Text = _section; // set section name
            TxtType.Text = string.Join(", ", _exercise.Type); // set type text
            RenderInputsForTrackingMetrics(_exercise.TrackingMetrics); // create input fields
        }
        private void RenderInputsForTrackingMetrics(List<string> metrics)
        {
            InputPanel.Children.Clear(); // clear old inputs
            foreach (var metric in metrics) // loop metrics
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
                if (label != null) InputPanel.Children.Add(CreateLabeledTextbox(label)); // add label+textbox
            }
        }
        private static StackPanel CreateLabeledTextbox(string label)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) }; // stack label + box
            panel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.White }); // add label
            panel.Children.Add(new TextBox { Height = 25, Tag = label, Background = System.Windows.Media.Brushes.White }); // add textbox
            return panel; // return row
        }
        private Dictionary<string, string> GetTextBoxValues()
        {
            var map = new Dictionary<string, string>(); // label -> value
            foreach (var child in InputPanel.Children) // loop children
            {
                if (child is StackPanel sp) // if stack panel
                {
                    foreach (var inner in sp.Children) // loop label + box
                    {
                        if (inner is TextBox tb && tb.Tag is string tag) map[tag] = tb.Text; // map label to text
                    }
                }
            }
            return map; // return collected values
        }
        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var db = new LocalDatabaseService(); // create DB service
            var user = db.LoadFirstUser(); // load user
            if (user == null) return; // no user
            var inputValues = GetTextBoxValues(); // read inputs
            var cleanedMetrics = inputValues.ToDictionary(kv => kv.Key.TrimEnd(':'), kv => kv.Value); // strip colons from keys
            var existing = user.CompletedExercises.FirstOrDefault(e2 => e2.ExerciseId == _exercise.Guid); // find completion record
            if (existing == null) // none found
            {
                existing = new CompletedExercise { ExerciseId = _exercise.Guid, SubType = string.Join(", ", _exercise.Type) }; // create record
                user.CompletedExercises.Add(existing); // add to list
            }
            existing.Entries.Add(new CompletedExerciseEntry { DateCompleted = DateTime.UtcNow, Metrics = cleanedMetrics }); // add entry
            db.SaveUser(user); // save user
            OnComplete?.Invoke(); // call completion callback
            Visibility = Visibility.Collapsed; // hide modal
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke(); // call cancel callback
            Visibility = Visibility.Collapsed; // hide modal
        }
    }

}
