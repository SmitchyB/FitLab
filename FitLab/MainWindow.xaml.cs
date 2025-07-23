using FitLab.appstate;
using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using FitLab.Helpers;
using System.Diagnostics;
using System.Windows;

namespace FitLab
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var db = new LocalDatabaseService();
            var user = db.LoadFirstUser();

            GlobalCache.AllExercises = LocalDatabaseService.LoadExercises();
            Debug.WriteLine($"[INIT] Preloaded {GlobalCache.AllExercises.Count} exercises.");

            if (user != null)
            {
                SessionState.CurrentWeek = CalculateCurrentWeek.GetWeekNumber(user.CreatedOn, TimeZoneInfo.Local);
                SessionState.CurrentWorkoutDay = CalculateCurrentDay.GetCurrentDayNumber(user.CreatedOn, user.WorkoutPlan.PlanLength);

                Header.Visibility = Visibility.Visible;
                MainFrame.Navigate(new Pages.HomePage());
            }
            else
            {
                Header.Visibility = Visibility.Collapsed;
                MainFrame.Navigate(new Pages.UserIntake());
            }
        }
    }
}
