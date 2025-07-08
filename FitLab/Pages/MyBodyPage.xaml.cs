using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FitLab.Pages
{
    public partial class MyBodyPage : Page
    {
        // Data Collections
        private readonly ObservableCollection<MealEntry> Meals = new();
        private readonly ObservableCollection<GoalEntry> Goals = new();
        private int _waterCupsToday = 0;
        private int _currentWeek = 1;

        public MyBodyPage()
        {
            InitializeComponent();

            // Example default goals
            Goals = new ObservableCollection<GoalEntry>
            {
                new() { Description = "Stay under 2000 calories.", Timeframe = "1 Month" }
            };

            GoalsPanel.ItemsSource = Goals;
            DatePickerSelectedDate.SelectedDate = DateTime.Today;

            // Initialize week display
            TxtCurrentWeek.Text = $"Week {_currentWeek}";

            LoadForDate(DateTime.Today);
        }

        #region Models

        public class MealEntry
        {
            public string MealTime { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string PortionSize { get; set; } = string.Empty;
            public int Calories { get; set; }
        }

        public class GoalEntry
        {
            public string Description { get; set; } = string.Empty;
            public string Timeframe { get; set; } = string.Empty;
        }

        #endregion

        #region Meal Logic

        private void LoadForDate(DateTime date)
        {
            Meals.Clear();
            Meals.Add(new MealEntry
            {
                MealTime = "Breakfast",
                Description = $"Example meal on {date:MM/dd/yyyy}",
                PortionSize = "1 cup",
                Calories = 300
            });

            _waterCupsToday = 0;

            FoodIntakePanel.ItemsSource = Meals;
            UpdateWaterDisplay();
        }

        private void BtnAddMeal_Click(object sender, RoutedEventArgs e)
        {
            Meals.Add(new MealEntry
            {
                MealTime = (CmbMealTime.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                Description = TxtDescription.Text,
                PortionSize = TxtPortionSize.Text,
                Calories = int.TryParse(TxtCalories.Text, out int cals) ? cals : 0
            });

            CmbMealTime.SelectedIndex = -1;
            TxtDescription.Text = "";
            TxtPortionSize.Text = "";
            TxtCalories.Text = "";
        }

        private void BtnEditMeal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MealEntry meal)
            {
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

        #endregion

        #region Water Logic

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
            {
                _waterCupsToday--;
                UpdateWaterDisplay();
            }
        }

        #endregion

        #region Goal Logic

        private void BtnAddGoal_Click(object sender, RoutedEventArgs e)
        {
            Goals.Add(new GoalEntry
            {
                Description = TxtGoalDescription.Text,
                Timeframe = TxtGoalTimeframe.Text
            });

            TxtGoalDescription.Text = "";
            TxtGoalTimeframe.Text = "";
        }

        private void BtnEditGoal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GoalEntry goal)
            {
                TxtGoalDescription.Text = goal.Description;
                TxtGoalTimeframe.Text = goal.Timeframe;
                Goals.Remove(goal);
            }
        }

        private void BtnDeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GoalEntry goal)
                Goals.Remove(goal);
        }

        #endregion

        #region Date Picker

        private void DatePickerSelectedDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerSelectedDate.SelectedDate.HasValue)
                LoadForDate(DatePickerSelectedDate.SelectedDate.Value);
        }

        #endregion

        #region Week Selector Logic

        private void BtnWeekBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWeek > 1)
                _currentWeek--;

            TxtCurrentWeek.Text = $"Week {_currentWeek}";
        }

        private void BtnWeekForward_Click(object sender, RoutedEventArgs e)
        {
            _currentWeek++;
            TxtCurrentWeek.Text = $"Week {_currentWeek}";
        }

        #endregion

        #region Image Upload Logic

        private void LoadImage(Image targetImage)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select an Image"
            };

            if (dlg.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(dlg.FileName));
                targetImage.Source = bitmap;
            }
        }

        // Before Pictures
        private void BtnUploadFrontal_Click(object sender, RoutedEventArgs e) => LoadImage(ImgFrontalPreview);
        private void BtnUploadLeft_Click(object sender, RoutedEventArgs e) => LoadImage(ImgLeftPreview);
        private void BtnUploadRight_Click(object sender, RoutedEventArgs e) => LoadImage(ImgRightPreview);
        private void BtnUploadBack_Click(object sender, RoutedEventArgs e) => LoadImage(ImgBackPreview);

        // Weekly Progress Pictures
        private void BtnUploadProgressFrontal_Click(object sender, RoutedEventArgs e) => LoadImage(ImgProgressFrontal);
        private void BtnUploadProgressLeft_Click(object sender, RoutedEventArgs e) => LoadImage(ImgProgressLeft);
        private void BtnUploadProgressRight_Click(object sender, RoutedEventArgs e) => LoadImage(ImgProgressRight);
        private void BtnUploadProgressBack_Click(object sender, RoutedEventArgs e) => LoadImage(ImgProgressBack);

        #endregion
    }
}
