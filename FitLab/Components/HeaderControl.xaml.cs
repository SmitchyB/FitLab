using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FitLab.Components
{
    public partial class HeaderControl : UserControl
    {
        private bool isMenuVisible = false;

        public HeaderControl()
        {
            InitializeComponent();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMenuVisible)
            {
                var hide = (Storyboard)this.Resources["HideMenuStoryboard"];
                hide.Begin();
            }
            else
            {
                var show = (Storyboard)this.Resources["ShowMenuStoryboard"];
                show.Begin();
            }

            isMenuVisible = !isMenuVisible;
        }

        private void NavigateTo(Page page)
        {
            var window = Application.Current.MainWindow as MainWindow;
            window?.MainFrame.Navigate(page);
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var window = Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                if (window.MainFrame.Content is FitLab.Pages.HomePage)
                {
                    window.MainFrame.Refresh();
                }
                else
                {
                    window.MainFrame.Navigate(new FitLab.Pages.HomePage());
                }
            }
        }

        private void BtnMyBody_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new FitLab.Pages.MyBodyPage());
        }

        private void BtnMyGrowth_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new FitLab.Pages.MyGrowthPage());
        }

        private void BtnWorkoutPlan_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new FitLab.Pages.WorkoutPlanPage());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new FitLab.Pages.SettingsPage());
        }
    }
}