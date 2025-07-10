using FitLab.Data;
using System.Reflection.PortableExecutable;
using System.Windows;
using FitLab.Components;
using System.Diagnostics;
using FitLab.AppState;

namespace FitLab
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var db = new LocalDatabaseService();
            var user = db.LoadFirstUser();

            if (user != null)
            {
                Debug.WriteLine($"[FitLab] MainWindow: Loaded user Id {user.Id}");
                Header.Visibility = Visibility.Visible;
                MainFrame.Navigate(new Pages.HomePage());
            }
            else
            {
                Debug.WriteLine("[FitLab] MainWindow: No user loaded, showing intake.");
                Header.Visibility = Visibility.Collapsed;
                MainFrame.Navigate(new Pages.UserIntake());
            }
        }


    }
}
