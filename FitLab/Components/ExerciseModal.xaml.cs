using FitLab.appstate;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FitLab.Components
{
    public partial class ExerciseModal : Window
    {
        public Exercise? SelectedExercise { get; private set; }

        private readonly int _dayNumber;
        private readonly string _section;

        private readonly List<Exercise> _allExercises = new();
        private List<Exercise> _filteredExercises = new();
        private readonly HashSet<string> selectedMuscles = new();
        private readonly HashSet<string> selectedEquipments = new();
        private readonly HashSet<string> selectedDifficulties = new();
        private readonly HashSet<string> selectedTypes = new();

        public ExerciseModal(int dayNumber, string section)
        {
            InitializeComponent();

            _dayNumber = dayNumber;
            _section = section;

            GlobalCache.Reload();
            _allExercises = GlobalCache.AllExercises.ToList();
            _filteredExercises = _allExercises.ToList();

            SearchBox.TextChanged += (s, e) => ApplyFilters();
            PopulateFilters();
            RefreshExerciseList();
            PopulateCreateTabDropdowns();
        }

        private void PopulateFilters()
        {
            void AddFilterItems(ItemsControl container, IEnumerable<string> items, HashSet<string> stateSet)
            {
                container.Items.Clear();
                foreach (var item in items.OrderBy(x => x))
                {
                    var cb = new CheckBox
                    {
                        Content = item,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9a42ff"))
                    };
                    cb.Checked += (s, e) => { stateSet.Add(item); ApplyFilters(); };
                    cb.Unchecked += (s, e) => { stateSet.Remove(item); ApplyFilters(); };
                    container.Items.Add(cb);
                }
            }

            var allMuscles = _allExercises.Select(e => e.MuscleGroup).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();
            var allEquipment = _allExercises.SelectMany(e => e.Equipment).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();
            var allDifficulties = _allExercises.Select(e => e.Difficulty).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();
            var allTypes = _allExercises.SelectMany(e => e.Type).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();

            AddFilterItems(MuscleFilters, allMuscles, selectedMuscles);
            AddFilterItems(EquipmentFilters, allEquipment, selectedEquipments);
            AddFilterItems(DifficultyFilters, allDifficulties, selectedDifficulties);
            AddFilterItems(TypeFilters, allTypes, selectedTypes);
        }

        private void ApplyFilters()
        {
            string search = SearchBox.Text.Trim();

            _filteredExercises = _allExercises
                .Where(e =>
                    (string.IsNullOrEmpty(search) ||
                     e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                    (selectedMuscles.Count == 0 || selectedMuscles.Contains(e.MuscleGroup)) &&
                    (selectedEquipments.Count == 0 || e.Equipment.Any(eq => selectedEquipments.Contains(eq))) &&
                    (selectedDifficulties.Count == 0 || selectedDifficulties.Contains(e.Difficulty)) &&
                    (selectedTypes.Count == 0 || e.Type.Any(t => selectedTypes.Contains(t)))
                )
                .ToList();

            RefreshExerciseList();
        }

        private void RefreshExerciseList()
        {
            ExerciseList.ItemsSource = null;
            ExerciseList.ItemsSource = _filteredExercises;
            ExerciseList.DisplayMemberPath = "Name";
        }

        private void AddSelected_Click(object sender, RoutedEventArgs e)
        {
            Exercise? selected = SelectedExercise ?? ExerciseList.SelectedItem as Exercise;

            if (selected == null)
            {
                MessageBox.Show("Please select an exercise to add.");
                return;
            }

            SelectedExercise = selected;
            DialogResult = true;
            Close();
        }

        private void PopulateCreateTabDropdowns()
        {
            MuscleInput.ItemsSource = _allExercises
                .Select(e => e.MuscleGroup)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            TypeInput.ItemsSource = _allExercises
                .SelectMany(e => e.Type)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            DifficultyInput.ItemsSource = _allExercises
                .Select(e => e.Difficulty)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            EquipmentInput.ItemsSource = _allExercises
                .SelectMany(e => e.Equipment)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }


        private void CreateExercise_Click(object sender, RoutedEventArgs e)
        {
            var name = ExerciseNameInput.Text.Trim();
            var muscle = MuscleInput.SelectedItem as string;
            var type = TypeInput.SelectedItem as string;
            var difficulty = DifficultyInput.SelectedItem as string;
            var description = DescriptionInput.Text.Trim();
            var equipmentText = EquipmentInput.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(muscle) || string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Name, muscle group, and type are required.");
                return;
            }

            var newExercise = new Exercise
            {
                Name = name,
                MuscleGroup = muscle,
                Type = new List<string> { type },
                Difficulty = difficulty ?? "",
                Description = description,
                Equipment = string.IsNullOrWhiteSpace(equipmentText)
                    ? new List<string>()
                    : equipmentText.Split(',').Select(e => e.Trim()).ToList()
            };

            GlobalCache.AllExercises.Add(newExercise);
            new LocalDatabaseService().SaveExercise(newExercise);

            SelectedExercise = newExercise;

            // Reuse add logic
            AddSelected_Click(sender, e);
        }

    }
}
