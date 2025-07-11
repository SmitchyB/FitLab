using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

/// <summary>
/// This is the HeaderControl for the FitLab application. 
/// Displays the header with navigation buttons and a menu toggle with animations.
/// </summary>
namespace FitLab.Components
{
    public partial class HeaderControl : UserControl
    {
        private bool isMenuVisible = false; // Tracks the visibility state of the menu
        // Constructor initializes the component and sets up the event handlers.
        public HeaderControl()
        {
            InitializeComponent(); // Initialize the component
        }
        // Event handler for the menu button click event.
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMenuVisible) // If the menu is currently visible, hide it; otherwise, show it.
            {
                var hide = (Storyboard)this.Resources["HideMenuStoryboard"]; // Retrieve the storyboard for hiding the menu
                hide.Begin(); // Start the hide animation
            }
            else // If the menu is not visible, show it.
            {
                var show = (Storyboard)this.Resources["ShowMenuStoryboard"];// Retrieve the storyboard for showing the menu
                show.Begin(); // Start the show animation
            }

            isMenuVisible = !isMenuVisible; // Toggle the visibility state
        }
        // Event handler for the navigation buttons click events.
        private static void NavigateTo<T>() where T : Page, new() 
        {
            if (Application.Current.MainWindow is MainWindow window) // Check if the current application window is of type MainWindow
                if (window != null) // Check if the main window is not null
            {
                if (window.MainFrame.Content is T) // If the current content of the MainFrame is already of type T
                {
                    window.MainFrame.Refresh(); // Refresh the current page
                }
                else // If the current content is not of type T
                {
                    window.MainFrame.Navigate(new T()); // Navigate to the new page of type T
                }
            }
        }
        // Event hanlder for the home button click event.
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo<FitLab.Pages.HomePage>(); // Navigate to the HomePage
        }
        // Event handler for the my body button click event.
        private void BtnMyBody_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo<FitLab.Pages.MyBodyPage>(); // Navigate to the MyBodyPage
        }
        // Event handler for the my growth button click event.
        private void BtnMyGrowth_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo<FitLab.Pages.MyGrowthPage>(); // Navigate to the MyGrowthPage
        }
        // Event handler for the workout plan button click event.
        private void BtnWorkoutPlan_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo<FitLab.Pages.WorkoutPlanPage>(); // Navigate to the WorkoutPlanPage
        }
        // Event handler for the nutrition button click event.
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo<FitLab.Pages.SettingsPage>(); // Navigate to the SettingsPage
        }

    }
}