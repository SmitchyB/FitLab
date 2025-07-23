using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using System;
using System.Windows.Controls;

namespace FitLab.Pages
{
    public partial class HomePage : Page
    {
        private readonly User _currentUser;

        public HomePage()
        {
            InitializeComponent();

            var db = new LocalDatabaseService();
            _currentUser = db.LoadFirstUser() ?? throw new Exception("No user found.");

            WelcomeText.Text = $"Welcome back, {_currentUser.Name ?? "User"}!";
            WeekText.Text = $"Week {SessionState.CurrentWeek} of your workout journey";
            DayText.Text = $"Day {SessionState.CurrentWorkoutDay} of your current workout plan";

            // You can populate DailyWorkoutItems here if needed
        }
    }
}
