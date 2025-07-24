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
            RenderInputsForTypes(_exercise.Type);
        }

        private void RenderInputsForTypes(List<string> types)
        {
            InputPanel.Children.Clear();

            foreach (var type in types)
            {
                switch (type)
                {
                    case "Strength":
                        InputPanel.Children.Add(CreateLabeledTextbox("Sets:"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Reps Per Set:"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Weight Used (lbs):"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Rest Between Sets (sec):"));
                        break;

                    case "Cardio":
                        InputPanel.Children.Add(CreateLabeledTextbox("Duration (minutes):"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Distance (miles):"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Average Heart Rate:"));
                        break;

                    case "Flexibility":
                        InputPanel.Children.Add(CreateLabeledTextbox("Hold Duration (seconds):"));
                        InputPanel.Children.Add(CreateLabeledTextbox("Notes:"));
                        break;
                }
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

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var db = new LocalDatabaseService();
            var user = db.LoadFirstUser();
            if (user == null || _exercise.Type.Count == 0) return;

            // Use the first type as the primary one
            string primaryType = _exercise.Type[0];

            CompletedExercise? record = primaryType switch
            {
                "Strength" => BuildStrength(),
                "Cardio" => BuildCardio(),
                "Flexibility" => BuildFlexibility(),
                _ => null
            };


            if (record == null) return;

            record.ExerciseId = _exercise.Guid;
            record.SubType = primaryType;
            record.CompletionTimes.Add(DateTime.UtcNow);

            user.CompletedExercises.Add(record);
            db.SaveUser(user);

            OnComplete?.Invoke();

            Visibility = Visibility.Collapsed;
        }


        private CompletedStrengthExercise BuildStrength()
        {
            var inputs = GetTextBoxValues();
            return new CompletedStrengthExercise
            {
                Sets = new List<int> { int.Parse(inputs["Sets:"]) },
                RepsPerSet = new List<int> { int.Parse(inputs["Reps Per Set:"]) },
                WeightUsed = new List<double> { double.Parse(inputs["Weight Used (lbs):"]) },
                RestBetweenSets = new List<TimeSpan> { TimeSpan.FromSeconds(int.Parse(inputs["Rest Between Sets (sec):"])) }
            };
        }

        private CompletedCardioExercise BuildCardio()
        {
            var inputs = GetTextBoxValues();
            return new CompletedCardioExercise
            {
                Durations = new List<TimeSpan> { TimeSpan.FromMinutes(double.Parse(inputs["Duration (minutes):"])) },
                Distances = new List<double> { double.Parse(inputs["Distance (miles):"]) },
                AvgHeartRates = new List<int> { int.Parse(inputs["Average Heart Rate:"]) }
            };
        }

        private CompletedFlexibilityExercise BuildFlexibility()
        {
            var inputs = GetTextBoxValues();
            return new CompletedFlexibilityExercise
            {
                HoldDurations = new List<TimeSpan> { TimeSpan.FromSeconds(double.Parse(inputs["Hold Duration (seconds):"])) },
                Notes = new List<string> { inputs.TryGetValue("Notes:", out var note) ? note ?? string.Empty : string.Empty }
            };
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
