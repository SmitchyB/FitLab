using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using FitLab.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Pages
{   // This page allows users to view and edit their workout plan.
    public partial class WorkoutPlanPage : Page
    {
        private User _user = null!; // Initialized in LoadUserData
        private bool _isEditing = false; // Tracks if the user is currently editing the plan
        public WorkoutPlanPage()
        {
            InitializeComponent(); 
            LoadUserData(); // Load user data from the local database
            RenderPlan(); // Render the workout plan based on the loaded user data
        }
        // Loads the user data from the local database.
        private void LoadUserData()
        {
            var db = new LocalDatabaseService(); // Create an instance of the database service
            _user = db.LoadFirstUser() ?? new User(); // If no user is found, create a new User instance
            _user.WorkoutPlan ??= new WorkoutPlan { PlanLength = 7 }; // Ensure the WorkoutPlan is initialized, defaulting to a 7-day plan if not set
        }
        // Renders the workout plan by populating the DailyPlanItems list with DailyExercisePlanner controls.
        private void RenderPlan()
        {
            DailyPlanItems.Items.Clear(); // Clear existing items in the DailyPlanItems list
            CycleLengthDropdown.SelectedIndex = _user.WorkoutPlan.PlanLength switch // Set the selected index based on the current plan length
            {
                7 => 0,
                10 => 1,
                14 => 2,
                _ => 0
            };
            for (int i = 1; i <= _user.WorkoutPlan.PlanLength; i++) // Loop through each day in the workout plan
            {
                var day = _user.WorkoutPlan.Days.FirstOrDefault(d => d.DayNumber == i); // Find the DailyWorkout for the current day number
                if (day == null) // If no DailyWorkout exists for this day, create a new one
                {
                    day = new DailyWorkout { DayNumber = i }; // Initialize a new DailyWorkout with the current day number
                    _user.WorkoutPlan.Days.Add(day); // Add the new DailyWorkout to the user's workout plan
                }
                var control = new DailyExercisePlanner(day, _isEditing); // Create a new DailyExercisePlanner control for the current day
                DailyPlanItems.Items.Add(control); // Add the control to the DailyPlanItems list
            }
        }
        // Event handler for the Edit button click event.
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = true; // Set the editing mode to true
            EditButton.Visibility = Visibility.Collapsed; // Hide the Edit button
            SaveButton.Visibility = Visibility.Visible; // Show the Save button
            CancelButton.Visibility = Visibility.Visible; // Show the Cancel button
            CycleLengthDropdown.IsEnabled = true; // Enable the CycleLengthDropdown for editing
            RenderPlan(); // Re-render the plan to reflect the editing mode
        }
        // Event handler for the Cancel button click event.
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false; // Set the editing mode to false
            LoadUserData(); // Reload the user data to discard any changes made during editing
            SaveButton.Visibility = Visibility.Collapsed; // Hide the Save button
            CancelButton.Visibility = Visibility.Collapsed; // Hide the Cancel button
            EditButton.Visibility = Visibility.Visible; // Show the Edit button
            CycleLengthDropdown.IsEnabled = false; // Disable the CycleLengthDropdown
            RenderPlan(); // Re-render the plan to reflect the non-editing mode
        }
        // Event handler for the Save button click event.
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CycleLengthDropdown.SelectedItem is ComboBoxItem selected && int.TryParse(selected.Content.ToString(), out int newLength)) // Check if a valid length is selected
            {
                _user.WorkoutPlan.PlanLength = newLength; // Update the workout plan length
                _user.WorkoutPlan.Days = _user.WorkoutPlan.Days.Where(d => d.DayNumber <= newLength).ToList(); // Trim the days to match the new plan length
            }
            foreach (DailyExercisePlanner planner in DailyPlanItems.Items) // Iterate through each DailyExercisePlanner control in the DailyPlanItems list
            {
                if (!planner.ValidateDay()) // Validate the day to ensure it has either a rest day marked or at least one exercise
                {
                    MessageBox.Show($"Day {planner.Day.DayNumber} must be marked as Rest or have at least one exercise."); // Show an error message if validation fails
                    return;
                }
            }
            var db = new LocalDatabaseService(); // Create an instance of the database service
            db.SaveUser(_user); // Save the updated user data to the local database
            SessionState.CurrentWorkoutDay = CalculateCurrentDay.GetCurrentDayNumber(_user.CreatedOn, _user.WorkoutPlan.PlanLength, TimeZoneInfo.Local);
            _isEditing = false; // Set the editing mode to false
            SaveButton.Visibility = Visibility.Collapsed; // Hide the Save button
            CancelButton.Visibility = Visibility.Collapsed; // Hide the Cancel button
            EditButton.Visibility = Visibility.Visible; // Show the Edit button
            CycleLengthDropdown.IsEnabled = false; // Disable the CycleLengthDropdown
            RenderPlan(); // Re-render the plan to reflect the saved changes
        }
        // Event handler for the CycleLengthDropdown selection change event.
        private void CycleLengthDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditing || CycleLengthDropdown.SelectedItem is not ComboBoxItem selected) return;  // If not in editing mode or no valid item is selected, do nothing
            if (int.TryParse(selected.Content.ToString(), out int newLength)) // Try to parse the selected item's content as an integer
            {
                _user.WorkoutPlan.PlanLength = newLength; // Update the workout plan length
                for (int i = 1; i <= newLength; i++) // Loop through each day up to the new plan length
                {
                    if (!_user.WorkoutPlan.Days.Any(d => d.DayNumber == i)) // If no DailyWorkout exists for this day number
                        _user.WorkoutPlan.Days.Add(new DailyWorkout { DayNumber = i }); // Create a new DailyWorkout and add it to the plan
                }
                _user.WorkoutPlan.Days = _user.WorkoutPlan.Days // Filter the days to only include those within the new plan length
                    .Where(d => d.DayNumber <= newLength) // Ensure the days are within the new plan length
                    .OrderBy(d => d.DayNumber) // Order the days by their day number
                    .ToList(); // Convert the filtered days to a list
                RenderPlan(); // Re-render the plan to reflect the updated plan length
            }
        }

    }
}