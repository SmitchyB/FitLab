using System;
using System.Windows;
using System.Windows.Controls;
using FitLab.Data;

namespace FitLab.Components
{
    public partial class DailyExercisePlanner : UserControl
    {
        public DailyWorkout Day { get; private set; }
        private readonly bool _isEditing;

        public DailyExercisePlanner(DailyWorkout day, bool isEditing)
        {
            InitializeComponent();
            Day = day;
            _isEditing = isEditing;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = Day;
            RestDayCheckbox.IsChecked = Day.IsRestDay;
            RestDayCheckbox.IsEnabled = _isEditing;
            WarmupAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed;
            MainAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed;
            CooldownAddButton.Visibility = _isEditing ? Visibility.Visible : Visibility.Collapsed;
            UpdateUI();
        }


        private void UpdateUI()
        {
            RestDayMessage.Visibility = Day.IsRestDay ? Visibility.Visible : Visibility.Collapsed;
            WorkoutSections.Visibility = Day.IsRestDay ? Visibility.Collapsed : Visibility.Visible;

            WarmupList.ItemsSource = Day.Warmup;
            MainList.ItemsSource = Day.Main;
            CooldownList.ItemsSource = Day.Cooldown;
        }

        private void RestDayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Day.IsRestDay = true;
            UpdateUI();
        }

        private void RestDayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Day.IsRestDay = false;
            UpdateUI();
        }

        private void AddExercise_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing || Day.IsRestDay) return;

            if (sender is Button btn && btn.Tag is string section)
            {
                var modal = new ExerciseModal(Day.DayNumber, section);
                if (modal.ShowDialog() == true && modal.SelectedExercise != null)
                {
                    switch (section)
                    {
                        case "Warmup": Day.Warmup.Add(modal.SelectedExercise); break;
                        case "Main": Day.Main.Add(modal.SelectedExercise); break;
                        case "Cooldown": Day.Cooldown.Add(modal.SelectedExercise); break;
                    }
                    UpdateUI();
                }
            }
        }


        public bool ValidateDay()
        {
            return Day.IsRestDay || Day.Warmup.Count > 0 || Day.Main.Count > 0 || Day.Cooldown.Count > 0;
        }
    }
}
