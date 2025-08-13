using System;
using System.Windows;
using System.Windows.Controls;
using FitLab.Data;

namespace FitLab.Components
{
    // this is the user control for planning a daily workout
    public partial class DailyExercisePlanner : UserControl
    {
        public DailyWorkout Day { get; private set; } // The workout day being planned
        private readonly bool _isEditing; // Flag to indicate if the planner is in editing mode

        // Constructor that initializes the planner with a DailyWorkout object and an editing flag
        public DailyExercisePlanner(DailyWorkout day, bool isEditing)
        {
            InitializeComponent();
            Day = day; // Set the workout day
            _isEditing = isEditing; // Set the editing mode
            Loaded += OnLoaded; // Subscribe to the Loaded event to initialize the UI
        }

        // Event handler for when the control is loaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = Day; // Set the DataContext to the Day object for data binding
            RestDayCheckbox.IsChecked = Day.IsRestDay; // Initialize the rest day checkbox based on the Day object
            RestDayCheckbox.IsEnabled = _isEditing; // Enable or disable the rest day checkbox based on editing mode
            WarmupAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed; // Show or hide the Warmup add button based on editing mode
            MainAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed; // Show or hide the Main add button based on editing mode
            CooldownAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed; // Show or hide the Cooldown add button based on editing mode
            UpdateUI(); // Update the UI to reflect the current state of the Day object
        }
        // Method to update the UI based on the current state of the Day object
        private void UpdateUI()
        {
            RestDayMessage.Visibility = Day.IsRestDay ? Visibility.Visible : Visibility.Collapsed; // Show or hide the rest day message based on the Day object
            WorkoutSections.Visibility = Day.IsRestDay ? Visibility.Collapsed : Visibility.Visible; // Show or hide the workout sections based on the Day object
            WarmupList.ItemsSource = Day.Warmup; // Bind the Warmup list to the Warmup exercises in the Day object
            MainList.ItemsSource = Day.Main; // Bind the Main list to the Main exercises in the Day object
            CooldownList.ItemsSource = Day.Cooldown; // Bind the Cooldown list to the Cooldown exercises in the Day object
        }
        // Event handlers for the rest day checkbox to update the Day object and UI accordingly
        private void RestDayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Day.IsRestDay = true; // Set the Day object to rest day
            UpdateUI(); // Update the UI to reflect the rest day state
        }
        // Unchecked event handler for the rest day checkbox
        private void RestDayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Day.IsRestDay = false; // Set the Day object to not rest day
            UpdateUI(); // Update the UI to reflect the non-rest day state
        }
        // Event handler for adding an exercise to the workout sections
        private void AddExercise_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing || Day.IsRestDay) return; // If not in editing mode or if it's a rest day, do nothing

            if (sender is Button btn && btn.Tag is string section) //if the sender is a button with a section tag
            {
                var modal = new ExerciseModal(Day.DayNumber, section); // Create a new ExerciseModal for the specified section
                if (modal.ShowDialog() == true && modal.SelectedExercise != null) // Show the modal and check if an exercise was selected
                {
                    switch (section) // Add the selected exercise to the appropriate section of the Day object
                    {
                        case "Warmup": Day.Warmup.Add(modal.SelectedExercise); break; // Add to Warmup section
                        case "Main": Day.Main.Add(modal.SelectedExercise); break; // Add to Main section
                        case "Cooldown": Day.Cooldown.Add(modal.SelectedExercise); break; // Add to Cooldown section
                    }
                    UpdateUI(); // Update the UI to reflect the changes
                }
            }
        }
        //Boolean method to validate if the Day object has any exercises or is a rest day
        public bool ValidateDay()
        {
            return Day.IsRestDay || Day.Warmup.Count > 0 || Day.Main.Count > 0 || Day.Cooldown.Count > 0; // Return true if it's a rest day or if there are exercises in any section
        }
    }
}
