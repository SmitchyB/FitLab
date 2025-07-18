using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitLab.Pages
{
    /// <summary>
    /// Interaction logic for WorkoutPlanPage.xaml
    /// </summary>
    public partial class WorkoutPlanPage : Page
    {
        public WorkoutPlanPage()
        {
            InitializeComponent();
            var allExercises = LocalDatabaseService.LoadExercises();
        }

        private void AddExercise_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            var parent = VisualTreeHelper.GetParent(button) as Panel;
            var selector = parent?.Children.OfType<ExerciseSelector>().FirstOrDefault();

            if (selector != null)
            {
                selector.Visibility = Visibility.Visible;
                button.Visibility = Visibility.Collapsed;

                selector.ExerciseAdded += (exercise) =>
                {
                    selector.Visibility = Visibility.Collapsed;
                    button.Visibility = Visibility.Visible;
                };
            }
            if (selector != null)
            {
                selector.Visibility = Visibility.Visible;
                button.Visibility = Visibility.Collapsed;

                // Forcefully call Loaded logic
                selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

                selector.ExerciseAdded += (exercise) =>
                {
                    selector.Visibility = Visibility.Collapsed;
                    button.Visibility = Visibility.Visible;
                };
            }

        }
    }
}
