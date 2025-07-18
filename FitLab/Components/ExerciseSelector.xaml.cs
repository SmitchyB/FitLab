using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Components
{
    /// <summary>
    /// Interaction logic for ExerciseSelector.xaml
    /// </summary>
    public partial class ExerciseSelector : UserControl
    {
        private readonly LocalDatabaseService _db = new();
        private List<Exercise> _allExercises = new();
        public event Action<Exercise>? ExerciseAdded;

        public ExerciseSelector()
        {
            InitializeComponent();
            Loaded += ExerciseSelector_Loaded;
        }

        private void ExerciseSelector_Loaded(object sender, RoutedEventArgs e)
        {
            _allExercises = LocalDatabaseService.LoadExercises();

            Debug.WriteLine($"[LOG] Loaded {_allExercises.Count} exercises.");
            if (_allExercises.Count == 0)
            {
                MessageBox.Show("No exercises loaded. Check that 'exercises.json' exists and is in the correct output directory.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var muscles = _allExercises.Select(e => e.MuscleGroup).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();
            var equipment = _allExercises.SelectMany(e => e.Equipment).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();
            var difficulty = _allExercises.Select(e => e.Difficulty).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();
            var types = _allExercises.SelectMany(e => e.Type).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s).ToList();

            Debug.WriteLine($"[LOG] Muscles: {string.Join(", ", muscles)}");
            Debug.WriteLine($"[LOG] Equipment: {string.Join(", ", equipment)}");
            Debug.WriteLine($"[LOG] Difficulty: {string.Join(", ", difficulty)}");
            Debug.WriteLine($"[LOG] Types: {string.Join(", ", types)}");

            CmbFilterMuscle.ItemsSource = muscles;
            CmbFilterEquip.ItemsSource = equipment;
            CmbFilterDifficulty.ItemsSource = difficulty;
            CmbFilterType.ItemsSource = types;
            CmbCustomMuscle.ItemsSource = muscles;

            UpdateExerciseList();

            CmbFilterMuscle.SelectionChanged += FilterChanged;
            CmbFilterEquip.SelectionChanged += FilterChanged;
            CmbFilterDifficulty.SelectionChanged += FilterChanged;
            CmbFilterType.SelectionChanged += FilterChanged;
        }

        private void UpdateExerciseList()
        {
            var filtered = _allExercises;

            if (CmbFilterMuscle.SelectedItem is string m)
                filtered = filtered.Where(e => e.MuscleGroup == m).ToList();
            if (CmbFilterEquip.SelectedItem is string eq)
                filtered = filtered.Where(e => e.Equipment.Contains(eq)).ToList();
            if (CmbFilterDifficulty.SelectedItem is string d)
                filtered = filtered.Where(e => e.Difficulty == d).ToList();
            if (CmbFilterType.SelectedItem is string t)
                filtered = filtered.Where(e => e.Type.Contains(t)).ToList();

            var formatted = filtered.Select(e =>
                $"{e.Name} - {string.Join("/", e.Type)} - {e.MuscleGroup} - {string.Join("/", e.Equipment)} - {e.Difficulty}"
            ).ToList();

            Debug.WriteLine($"[LOG] {formatted.Count} exercises after filtering.");

            CmbExerciseSelect.ItemsSource = formatted;
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateExerciseList();
        }

        private void BtnAddToWorkout_Click(object sender, RoutedEventArgs e)
        {
            var custom = new Exercise
            {
                Name = TxtCustomName.Text.Trim(),
                MuscleGroup = CmbCustomMuscle.SelectedItem as string ?? "",
                Equipment = new List<string> { TxtCustomEquip.Text.Trim() },
                Difficulty = CmbCustomDifficulty.Text,
                Type = new List<string> { CmbCustomType.Text },
                Description = TxtCustomDesc.Text.Trim()
            };

            Debug.WriteLine($"[LOG] Custom Exercise Added: {custom.Name}");

            ExerciseAdded?.Invoke(custom);
        }
    }
}
