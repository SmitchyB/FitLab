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
{
    public partial class WorkoutPlanPage : Page
    {
        private User _user = null!;
        private bool _isEditing = false;

        public WorkoutPlanPage()
        {
            InitializeComponent();
            LoadUserData();
            RenderPlan();
        }

        private void LoadUserData()
        {
            var db = new LocalDatabaseService();
            _user = db.LoadFirstUser() ?? new User();
            _user.WorkoutPlan ??= new WorkoutPlan { PlanLength = 7 };
        }

        private void RenderPlan()
        {
            DailyPlanItems.Items.Clear();
            CycleLengthDropdown.SelectedIndex = _user.WorkoutPlan.PlanLength switch
            {
                7 => 0,
                10 => 1,
                14 => 2,
                _ => 0
            };

            for (int i = 1; i <= _user.WorkoutPlan.PlanLength; i++)
            {
                var day = _user.WorkoutPlan.Days.FirstOrDefault(d => d.DayNumber == i);
                if (day == null)
                {
                    day = new DailyWorkout { DayNumber = i };
                    _user.WorkoutPlan.Days.Add(day);
                }

                var control = new DailyExercisePlanner(day, _isEditing);
                DailyPlanItems.Items.Add(control);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = true;
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            CycleLengthDropdown.IsEnabled = true;
            RenderPlan();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            LoadUserData();
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            CycleLengthDropdown.IsEnabled = false;
            RenderPlan();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CycleLengthDropdown.SelectedItem is ComboBoxItem selected && int.TryParse(selected.Content.ToString(), out int newLength))
            {
                _user.WorkoutPlan.PlanLength = newLength;
                _user.WorkoutPlan.Days = _user.WorkoutPlan.Days.Where(d => d.DayNumber <= newLength).ToList();
            }

            foreach (DailyExercisePlanner planner in DailyPlanItems.Items)
            {
                if (!planner.ValidateDay())
                {
                    MessageBox.Show($"Day {planner.Day.DayNumber} must be marked as Rest or have at least one exercise.");
                    return;
                }
            }

            var db = new LocalDatabaseService();
            db.SaveUser(_user);

            // 🔁 Update session workout day number based on created date + new cycle length
            SessionState.CurrentWorkoutDay = CalculateCurrentDay.GetCurrentDayNumber(
                _user.CreatedOn,
                _user.WorkoutPlan.PlanLength
            );

            _isEditing = false;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            CycleLengthDropdown.IsEnabled = false;
            RenderPlan();
        }

        private void CycleLengthDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditing || CycleLengthDropdown.SelectedItem is not ComboBoxItem selected) return;

            if (int.TryParse(selected.Content.ToString(), out int newLength))
            {
                _user.WorkoutPlan.PlanLength = newLength;

                // Add missing days
                for (int i = 1; i <= newLength; i++)
                {
                    if (!_user.WorkoutPlan.Days.Any(d => d.DayNumber == i))
                        _user.WorkoutPlan.Days.Add(new DailyWorkout { DayNumber = i });
                }

                // Trim days beyond plan length
                _user.WorkoutPlan.Days = _user.WorkoutPlan.Days
                    .Where(d => d.DayNumber <= newLength)
                    .OrderBy(d => d.DayNumber)
                    .ToList();

                RenderPlan();
            }
        }

    }
}