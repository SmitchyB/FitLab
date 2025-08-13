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
    // Modal for selecting or creating exercises
    public partial class ExerciseModal : Window
    {
        public Exercise? SelectedExercise { get; private set; } // The exercise selected by the user
        private readonly int _dayNumber; // The day number for which the exercise is being selected
        private readonly string _section; // The section of the workout plan (e.g., "Warmup", "Main", "Cooldown")
        private readonly List<Exercise> _allExercises = new(); // All exercises loaded from the global cache
        private List<Exercise> _filteredExercises = new(); // Exercises filtered based on user input and selected filters
        private readonly HashSet<string> selectedMuscles = new(); // Selected muscle groups for filtering
        private readonly HashSet<string> selectedEquipments = new(); // Selected equipment for filtering
        private readonly HashSet<string> selectedDifficulties = new(); // Selected difficulties for filtering
        private readonly HashSet<string> selectedTypes = new(); // Selected types for filtering
        private static readonly List<string> ValidTrackingMetrics = new() // Valid tracking metrics that can be selected when creating a new exercise
        {
            "Sets",
            "Reps Per Set",
            "Weight Used",
            "Rest Between Sets",
            "RPE",
            "Failure Reached",
            "Hold Duration",
            "Discomfort Level",
            "Duration",
            "Intensity",
            "Distance",
            "Speed"
        };
        // Constructor that initializes the modal with the specified day number and section
        public ExerciseModal(int dayNumber, string section)
        {
            InitializeComponent(); 
            _dayNumber = dayNumber; // The day number for which the exercise is being selected
            _section = section; // The section of the workout plan (e.g., "Warmup", "Main", "Cooldown")
            GlobalCache.Reload(); // Reload all exercises from the global cache
            _allExercises = GlobalCache.AllExercises.ToList(); // Get all exercises from the global cache
            _filteredExercises = _allExercises.ToList(); // Initialize filtered exercises to all exercises
            SearchBox.TextChanged += (s, e) => ApplyFilters(); // Apply filters when the search box text changes
            PopulateFilters(); // Populate the filter checkboxes with available options
            RefreshExerciseList(); // Refresh the exercise list to show all exercises initially
            PopulateCreateTabDropdowns(); // Populate the dropdowns in the create tab with available options
            PopulateTrackingMetricCheckboxes(); // Populate the tracking metrics checkboxes in the create tab
        }
        // Event handler for when the modal is loaded
        private void PopulateFilters()
        {
            void AddFilterItems(ItemsControl container, IEnumerable<string> items, HashSet<string> stateSet) // adds the filter items to the container
            {
                container.Items.Clear(); // Clear existing items in the container
                foreach (var item in items.OrderBy(x => x)) // Order items alphabetically before adding
                {
                    var cb = new CheckBox // Create a new checkbox for each item
                    {
                        Content = item, // Set the content of the checkbox to the item
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9a42ff")) // Set the foreground color of the checkbox
                    };
                    cb.Checked += (s, e) => { stateSet.Add(item); ApplyFilters(); }; // Add event handler for when the checkbox is checked
                    cb.Unchecked += (s, e) => { stateSet.Remove(item); ApplyFilters(); }; // Add event handler for when the checkbox is unchecked
                    container.Items.Add(cb); // Add the checkbox to the container
                }
            }
            var allMuscles = _allExercises.Select(e => e.MuscleGroup).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(); // Get all unique muscle groups from the exercises
            var allEquipment = _allExercises.SelectMany(e => e.Equipment).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(); // Get all unique equipment from the exercises
            var allDifficulties = _allExercises.Select(e => e.Difficulty).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(); // Get all unique difficulties from the exercises
            var allTypes = _allExercises.SelectMany(e => e.Type).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(); // Get all unique types from the exercises
            AddFilterItems(MuscleFilters, allMuscles, selectedMuscles); // Add muscle group checkboxes to the MuscleFilters container
            AddFilterItems(EquipmentFilters, allEquipment, selectedEquipments); // Add equipment checkboxes to the EquipmentFilters container
            AddFilterItems(DifficultyFilters, allDifficulties, selectedDifficulties); // Add difficulty checkboxes to the DifficultyFilters container
            AddFilterItems(TypeFilters, allTypes, selectedTypes); // Add type checkboxes to the TypeFilters container
        }
        //Applies the selected filters to the list of exercises and refreshes the displayed list
        private void ApplyFilters()
        {
            string search = SearchBox.Text.Trim(); // Get the trimmed search text from the search box
            _filteredExercises = _allExercises // Filter the list of exercises based on the search text and selected filters
                .Where(e => // Check if the exercise matches the search text and selected filters
                    (string.IsNullOrEmpty(search) || // Check if the search text is empty or if the exercise name contains the search text
                     e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) && // Check if the exercise name contains the search text (case-insensitive)
                    (selectedMuscles.Count == 0 || selectedMuscles.Contains(e.MuscleGroup)) && // Check if the selected muscle groups match the exercise's muscle group
                    (selectedEquipments.Count == 0 || e.Equipment.Any(eq => selectedEquipments.Contains(eq))) && // Check if the selected equipment matches any of the exercise's equipment
                    (selectedDifficulties.Count == 0 || selectedDifficulties.Contains(e.Difficulty)) && // Check if the selected difficulties match the exercise's difficulty
                    (selectedTypes.Count == 0 || e.Type.Any(t => selectedTypes.Contains(t))) // Check if the selected types match any of the exercise's types
                )
                .ToList(); // Convert the filtered exercises to a list
            RefreshExerciseList(); // Refresh the displayed list of exercises with the filtered results
        }
        // Refreshes the exercise list displayed in the modal
        private void RefreshExerciseList()
        {
            ExerciseList.ItemsSource = null; // Clear the current items source of the exercise list
            ExerciseList.ItemsSource = _filteredExercises; // Set the items source to the filtered exercises
            ExerciseList.DisplayMemberPath = "Name"; // Set the display member path to the Name property of the Exercise class
        }
        // Event handler for when the user clicks the "Add Selected" button
        private void AddSelected_Click(object sender, RoutedEventArgs e)
        {
            Exercise? selected = SelectedExercise ?? ExerciseList.SelectedItem as Exercise; // Get the selected exercise from the list or the currently selected exercise
            if (selected == null) // If no exercise is selected, show a message and return
            {
                MessageBox.Show("Please select an exercise to add."); // Show a message box prompting the user to select an exercise
                return;
            }
            SelectedExercise = selected; // Set the SelectedExercise property to the selected exercise
            DialogResult = true; // Set the dialog result to true to indicate a successful selection
            Close(); // Close the modal window
        }
        // Populates the dropdowns in the "Create Exercise" tab with available options
        private void PopulateCreateTabDropdowns()
        {
            MuscleInput.ItemsSource = _allExercises // Get all unique muscle groups from the exercises
                .Select(e => e.MuscleGroup) // Select the MuscleGroup property from each exercise
                .Where(e => !string.IsNullOrWhiteSpace(e)) // Filter out any empty or whitespace muscle groups
                .Distinct() // Ensure uniqueness of muscle groups
                .OrderBy(e => e) // Order the muscle groups alphabetically
                .ToList(); // Convert the result to a list

            TypeInput.ItemsSource = _allExercises // Get all unique types from the exercises
                .SelectMany(e => e.Type) // Select the Type property from each exercise (which is a list)
                .Where(e => !string.IsNullOrWhiteSpace(e)) // Filter out any empty or whitespace types
                .Distinct() // Ensure uniqueness of types
                .OrderBy(e => e) // Order the types alphabetically
                .ToList(); // Convert the result to a list

            DifficultyInput.ItemsSource = _allExercises // Get all unique difficulties from the exercises
                .Select(e => e.Difficulty) // Select the Difficulty property from each exercise
                .Where(e => !string.IsNullOrWhiteSpace(e)) // Filter out any empty or whitespace difficulties
                .Distinct() // Ensure uniqueness of difficulties
                .OrderBy(e => e) // Order the difficulties alphabetically
                .ToList(); // Convert the result to a list

            EquipmentInput.ItemsSource = _allExercises // Get all unique equipment from the exercises
                .SelectMany(e => e.Equipment) // Select the Equipment property from each exercise (which is a list)
                .Where(e => !string.IsNullOrWhiteSpace(e)) // Filter out any empty or whitespace equipment
                .Distinct() // Ensure uniqueness of equipment
                .OrderBy(e => e) // Order the equipment alphabetically
                .ToList(); // Convert the result to a list
        }
        // Populates the tracking metrics checkboxes in the "Create Exercise" tab with valid options
        private void PopulateTrackingMetricCheckboxes()
        {
            TrackingMetricsPanel.Children.Clear(); // Clear any existing checkboxes in the TrackingMetricsPanel
            foreach (var metric in ValidTrackingMetrics) // Iterate through each valid tracking metric
            {
                var cb = new CheckBox // Create a new checkbox for the metric
                {
                    Content = metric,// Set the content of the checkbox to the metric name
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9a42ff")), // Set the foreground color of the checkbox
                    Margin = new Thickness(5) // Set the margin around the checkbox
                }; 
                TrackingMetricsPanel.Children.Add(cb); // Add the checkbox to the TrackingMetricsPanel
            }
        }
        // Event handler for when the user clicks the "Create Exercise" button
        private void CreateExercise_Click(object sender, RoutedEventArgs e)
        {
            var name = ExerciseNameInput.Text.Trim(); // Get the trimmed name from the ExerciseNameInput textbox
            var muscle = MuscleInput.SelectedItem as string; // Get the selected muscle group from the MuscleInput dropdown
            var type = TypeInput.SelectedItem as string; // Get the selected type from the TypeInput dropdown 
            var difficulty = DifficultyInput.SelectedItem as string; // Get the selected difficulty from the DifficultyInput dropdown
            var description = DescriptionInput.Text.Trim(); // Get the trimmed description from the DescriptionInput textbox
            var equipmentText = EquipmentInput.Text.Trim(); // Get the trimmed equipment text from the EquipmentInput textbox
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(muscle) || string.IsNullOrEmpty(type)) // Check if the name, muscle group, and type are provided
            {
                MessageBox.Show("Name, muscle group, and type are required."); // Show a message box prompting the user to provide required fields
                return;
            }
            var selectedMetrics = TrackingMetricsPanel.Children // Get all children of the TrackingMetricsPanel (which are checkboxes)
                .OfType<CheckBox>() // Filter to only include CheckBox controls
                .Where(cb => cb.IsChecked == true) // Check if the checkbox is checked
                .Select(cb => cb.Content?.ToString()) // Select the content of the checked checkboxes (which are the metric names)
                .Where(m => !string.IsNullOrWhiteSpace(m)) // Filter out any empty or whitespace metric names
                .ToList(); // Convert the result to a list of strings
            if (selectedMetrics.Count == 0) // Check if no tracking metrics were selected
            {
                MessageBox.Show("Please select at least one tracking metric."); // Show a message box prompting the user to select at least one tracking metric
                return;
            }
            var newExercise = new Exercise // Create a new Exercise object with the provided details
            {
                Guid = Guid.NewGuid(), // Generate a new unique identifier for the exercise
                Name = name, // Set the name of the exercise
                MuscleGroup = muscle, // Set the muscle group of the exercise
                Type = new List<string> { type }, // Set the type of the exercise (as a list)
                Difficulty = difficulty ?? "", // Set the difficulty of the exercise (or an empty string if not provided)
                Description = description, // Set the description of the exercise
                Equipment = string.IsNullOrWhiteSpace(equipmentText) // Check if the equipment text is empty or whitespace
                    ? new List<string>() // If empty, set Equipment to an empty list
                    : equipmentText.Split(',').Select(e => e.Trim()).ToList(), // Otherwise, split the equipment text by commas and trim whitespace
                TrackingMetrics = selectedMetrics // Set the tracking metrics to the selected metrics from the checkboxes
                    .Where(m => !string.IsNullOrWhiteSpace(m)) // Filter out any empty or whitespace metric names
                    .Select(m => m!) // Ensure non-nullable strings
                    .ToList() // Convert the selected metrics to a list of strings
            };
            GlobalCache.AllExercises.Add(newExercise); // Add the new exercise to the global cache
            LocalDatabaseService.SaveExercise(newExercise); // Save the new exercise to the local database
            SelectedExercise = newExercise; // Set the SelectedExercise property to the newly created exercise
            AddSelected_Click(sender, e); // Call the AddSelected_Click method to finalize the selection and close the modal
        }
    }
}
