using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Pages
{
    public partial class MyBodyPage : Page
    {
        private ObservableCollection<MealEntry> Meals = new();
        private ObservableCollection<GoalEntry> Goals = new();
        private int _waterCupsToday = 0;

        public MyBodyPage()
        {
            InitializeComponent();
            Goals = new ObservableCollection<GoalEntry>
            {
                new GoalEntry { Title = "Calorie Limit", Description = "Stay under 2000 calories." }
            };
            GoalsPanel.ItemsSource = Goals;
            DatePickerSelectedDate.SelectedDate = DateTime.Today;
            LoadForDate(DateTime.Today);
        }

        /// <summary>
        /// Loads meals and water for a specific date.
        /// Replace this with real data loading logic.
        /// </summary>
        private void LoadForDate(DateTime date)
        {
            Meals = new ObservableCollection<MealEntry>
            {
                new MealEntry { MealTime = "Breakfast", Description = $"Example meal on {date:MM/dd/yyyy}", Calories = 300 }
            };

            _waterCupsToday = 0;

            FoodIntakePanel.ItemsSource = Meals;
            UpdateWaterDisplay();
        }

        private void DatePickerSelectedDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerSelectedDate.SelectedDate.HasValue)
                LoadForDate(DatePickerSelectedDate.SelectedDate.Value);
        }

        // Water Intake Handlers
        private void UpdateWaterDisplay()
        {
            TxtWaterCups.Text = $"{_waterCupsToday} cup{(_waterCupsToday == 1 ? "" : "s")}";
        }

        private void BtnPlusWater_Click(object sender, RoutedEventArgs e)
        {
            _waterCupsToday++;
            UpdateWaterDisplay();
        }

        private void BtnMinusWater_Click(object sender, RoutedEventArgs e)
        {
            if (_waterCupsToday > 0)
                _waterCupsToday--;
            UpdateWaterDisplay();
        }

        // Meal Handlers
        private void BtnAddMeal_Click(object sender, RoutedEventArgs e)
        {
            Meals.Add(new MealEntry
            {
                MealTime = TxtMealTime.Text,
                Description = TxtDescription.Text,
                Calories = int.TryParse(TxtCalories.Text, out int cals) ? cals : 0
            });
        }

        private void BtnEditMeal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MealEntry meal)
            {
                // For demo: update to show edited
                meal.MealTime = "Edited Meal";
                meal.Description = "Edited Description";
                meal.Calories = 999;
                FoodIntakePanel.Items.Refresh();
            }
        }

        private void BtnDeleteMeal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MealEntry meal)
                Meals.Remove(meal);
        }

        // Goal Handlers
        private void BtnAddGoal_Click(object sender, RoutedEventArgs e)
        {
            Goals.Add(new GoalEntry
            {
                Title = TxtGoalTitle.Text,
                Description = TxtGoalDescription.Text
            });
        }

        private void BtnEditGoal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GoalEntry goal)
            {
                goal.Title = "Edited Goal";
                goal.Description = "Edited Description";
                GoalsPanel.Items.Refresh();
            }
        }

        private void BtnDeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GoalEntry goal)
                Goals.Remove(goal);
        }

        // You can expand these later to save/load basic info
        private void LoadBasicInfo()
        {
            // For example:
            // TxtName.Text = Load from user profile
            // TxtHeight.Text = ...
            // CmbHeightUnit.SelectedIndex = ...
        }
    }

    public class MealEntry
    {
        public string MealTime { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Calories { get; set; }
    }

    public class GoalEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

