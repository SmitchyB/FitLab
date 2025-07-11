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
        //Main Window constructor
        public MainWindow()
        {
            InitializeComponent(); // Initialize the main window components

            var db = new LocalDatabaseService(); // Create an instance of the LocalDatabaseService to interact with the database
            var user = db.LoadFirstUser(); // Load the first user from the database

            if (user != null) // Check if a user was successfully loaded
            {
                Header.Visibility = Visibility.Visible; // Show the header if a user is loaded
                MainFrame.Navigate(new Pages.HomePage()); // Navigate to the HomePage if a user is loaded
            }
            else // If no user is loaded
            {
                Header.Visibility = Visibility.Collapsed; // Hide the header
                MainFrame.Navigate(new Pages.UserIntake()); // Navigate to the UserIntake page to allow user creation
            }
        }
    }
}
