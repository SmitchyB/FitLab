using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;


namespace FitLab.Pages
{
    /// <summary>
    /// This page is used for user intake, collecting information about the user
    /// and creating a user in the local database.
    /// </summary>
    public partial class UserIntake : Page
    {
        private readonly User _newUser = new(); // Represents the new user being created during intake
        private readonly List<Goal> _goals = new(); // List of goals added by the user during intake
        private readonly List<ProgressPicture> _intakePictures = new(); // List of progress pictures added by the user during intake

        //Initialize the intake page
        public UserIntake()
        {
            InitializeComponent();
        }
        // Event handlers for the intake step for first name
        private void NextFirstName(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text)) // Check if the first name is empty
            {
                MessageBox.Show("Please enter your first name."); // Show a message if the first name is empty
                return; // Exit the method if the first name is empty
            }

            _newUser.Name = TxtFirstName.Text.Trim(); // Set the user's name from the input field

            StepFirstName.Visibility = Visibility.Collapsed; // Hide the first name step
            StepGender.Visibility = Visibility.Visible; // Show the next step for gender selection
        }
        // Event handler for the gender selection combo box
        private void CmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected) // Check if an item is selected in the
            {
                var value = selected.Content?.ToString() ?? ""; // Get the selected item's content as a string
                if (value == "Trans-Feminine" || value == "Trans-Masculine") // Check if the selected item is Trans-Feminine or Trans-Masculine
                {
                    ChkOnHormones.Visibility = Visibility.Visible; // Show the "On Hormones" checkbox
                }
                else // If the selected item is not one of those two
                {
                    ChkOnHormones.Visibility = Visibility.Collapsed; // Hide the "On Hormones" checkbox
                }
            }
        }
        // Event handler for the "Next" button on the Gender step
        private void NextGender(object sender, RoutedEventArgs e) 
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected) // Check if an item is selected in the combo box
            {
                _newUser.Gender = selected.Content?.ToString() ?? ""; // Get the selected item's content as a string
                if (_newUser.Gender == "Trans-Feminine" || _newUser.Gender == "Trans-Masculine") // Check if the selected item is Trans-feminine or Trans-masculine
                {
                    _newUser.Gender += ChkOnHormones.IsChecked == true ? " (On Hormones)" : " (Not on Hormones)"; // Append "(On Hormones)" or "(Not on Hormones)" based on the checkbox state
                }
            }
            else // If no item is selected in the combo box
            {
                MessageBox.Show("Please select your gender."); // Show a message prompting the user to
                return; // Exit the method if no item is selected
            }

            StepGender.Visibility = Visibility.Collapsed; // Hide the gender step
            StepDOB.Visibility = Visibility.Visible; // Show the next step for date of birth selection
        }
        // Event handler for the date of birth selection
        private void NextDOB(object sender, RoutedEventArgs e)
        {
            if (DatePickerDOB.SelectedDate is DateTime dob) //Check if a date is selected in the DatePicker
            {
                _newUser.DateOfBirth = dob; // Set the user's date of birth from the selected date
            }
            else // If no date is selected in the DatePicker
            {
                MessageBox.Show("Please select your date of birth."); // Show a message prompting the user to select a date
                return; // Exit the method if no date is selected
            }

            StepDOB.Visibility = Visibility.Collapsed; // Hide the date of birth step
            StepHeight.Visibility = Visibility.Visible; // Show the next step for height input
        }
        // Event handler for the height unit selection combo box
        private void CmbHeightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtCmInstruction.Visibility = Visibility.Collapsed; // Hide the centimeters instruction text
            TxtInchesInstruction.Visibility = Visibility.Collapsed; // Hide the inches instruction text
            TxtFeetInchesInstruction.Visibility = Visibility.Collapsed; // Hide the feet/inches instruction text

            PanelFeetLabel.Visibility = Visibility.Collapsed; // Hide the feet label panel
            PanelInchesLabel.Visibility = Visibility.Collapsed; // Hide the inches label panel
            PanelSingleInput.Visibility = Visibility.Collapsed; // Hide the single input panel

            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected) // Check if an item is selected in the combo box
            {
                var value = selected.Content?.ToString() ?? ""; // Get the selected item's content as a string

                if (value == "Centimeters") // Check if the selected item is "Centimeters"
                {
                    TxtCmInstruction.Visibility = Visibility.Visible; // Show the centimeters instruction text
                    PanelSingleInput.Visibility = Visibility.Visible; // Show the single input panel
                }
                else if (value == "Inches") // Check if the selected item is "Inches"
                {
                    TxtInchesInstruction.Visibility = Visibility.Visible;// Show the inches instruction text
                    PanelSingleInput.Visibility = Visibility.Visible; // Show the single input panel
                }
                else if (value == "Feet/Inches") //Check if the selected item is "Feet/Inches"
                {
                    TxtFeetInchesInstruction.Visibility = Visibility.Visible; // Show the feet/inches instruction text
                    PanelFeetLabel.Visibility = Visibility.Visible; // Show the feet label panel
                    PanelInchesLabel.Visibility = Visibility.Visible; // Show the inches label panel
                }
            }
        }
        // Event handler for the "Next" button on the Height step
        private void NextHeight(object sender, RoutedEventArgs e)
        {
            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected) // Check if an item is selected in the combo box
            {
                var unit = selected.Content?.ToString() ?? ""; // Get the selected item's content as a string
                double heightInches = 0; // Variable to store the height in inches

                if (unit == "Centimeters") // Check if the selected item is "Centimeters"
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double cm)) // Try to parse the primary height input as a double
                        heightInches = Conversions.CmToInches(cm); // Convert centimeters to inches
                    else // If parsing fails
                    {
                        MessageBox.Show("Enter a valid number."); // Show a message prompting the user to enter a valid number
                        return; // Exit the method if parsing fails
                    }
                }
                else if (unit == "Inches") // Check if the selected item is "Inches"
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double inches)) // Try to parse the primary height input as a double
                        heightInches = inches; // Set heightInches to the parsed value
                    else // If parsing fails
                    {
                        MessageBox.Show("Enter a valid number."); // Show a message prompting the user to enter a valid number
                        return; // Exit the method if parsing fails
                    }
                }
                else if (unit == "Feet/Inches") // Check if the selected item is "Feet/Inches"
                {
                    if (int.TryParse(TxtHeightPrimary.Text, out int feet) && double.TryParse(TxtHeightSecondary.Text, out double inches)) // Try to parse the primary height input as an integer (feet) and the secondary input as a double (inches)
                        heightInches = Conversions.FeetInchesToInches(feet, inches); // Convert feet and inches to total inches
                    else // If parsing fails
                    {
                        MessageBox.Show("Enter valid feet and inches."); // Show a message prompting the user to enter valid feet and inches
                        return; // Exit the method if parsing fails
                    }
                }

                _newUser.HeightInches = heightInches; // Set the user's height in inches from the calculated value
            }
            else // If no item is selected in the combo box
            {
                MessageBox.Show("Select a unit for height."); // Show a message prompting the user to select a unit for height
                return; // Exit the method if no item is selected
            }

            StepHeight.Visibility = Visibility.Collapsed; // Hide the height step
            StepWeight.Visibility = Visibility.Visible; // Show the next step for weight input
        }
        // Event handler for the weight unit selection combo box
        private void NextWeight(object sender, RoutedEventArgs e) 
        {
            if (CmbWeightUnit.SelectedItem is ComboBoxItem selected) // Check if an item is selected in the combo box
            {
                var unit = selected.Content?.ToString() ?? ""; // Get the selected item's content as a string
                double weightLbs = 0; // Variable to store the weight in pounds

                if (double.TryParse(TxtWeight.Text, out double entered)) // Try to parse the weight input as a double
                {
                    weightLbs = unit == "Kilograms" ? Conversions.KgToLbs(entered) : entered; // Convert kilograms to pounds if the selected unit is "Kilograms"
                }
                else // If parsing fails
                {
                    MessageBox.Show("Enter a valid number."); // Show a message prompting the user to enter a valid number
                    return; // Exit the method if parsing fails
                }
                // Set the user's weight in pounds from the calculated value
                _newUser.WeightHistory.Add(new WeightEntry
                {
                    Date = DateTime.UtcNow, //only saves as a placeholder, actual date will be set after finishing the intake so that CreatedOn date and first weight entry match.
                    WeightLbs = weightLbs // Set the weight in pounds
                });
            }
            else // If no item is selected in the combo box
            {
                MessageBox.Show("Select a unit for weight."); // Show a message prompting the user to select a unit for weight
                return; // Exit the method if no item is selected
            }

            StepWeight.Visibility = Visibility.Collapsed; // Hide the weight step
            StepGoals.Visibility = Visibility.Visible; // Show the next step for goals input
        }
        // Event handler for the "Add Goal" button click
        private void AddGoal(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtGoalDescription.Text) && !string.IsNullOrWhiteSpace(TxtGoalTimeframe.Text)) // Check if both goal description and timeframe are provided
            {
                var goal = new Goal // Create a new Goal object with the provided description and timeframe
                {
                    Description = TxtGoalDescription.Text.Trim(), // Trim whitespace from the description
                    Timeframe = TxtGoalTimeframe.Text.Trim() // Trim whitespace from the timeframe
                };

                _goals.Add(goal); // Add the new goal to the list of goals
                GoalsList.Items.Add($"{goal.Description} ({goal.Timeframe})"); // Add the goal to the ListBox for display

                TxtGoalDescription.Text = ""; // Clear the goal description input field
                TxtGoalTimeframe.Text = ""; // Clear the goal timeframe input field
            }
            else // If either the goal description or timeframe is empty
            {
                MessageBox.Show("Enter both description and timeframe."); // Show a message prompting the user to enter both description and timeframe
            }
        }
        // Event handler for the "Next" button on the Goals step
        private void NextGoals(object sender, RoutedEventArgs e)
        {
            _newUser.Goals = _goals; // Set the user's goals from the list of goals added during intake

            StepGoals.Visibility = Visibility.Collapsed; // Hide the goals step
            StepPictures.Visibility = Visibility.Visible; // Show the next step for uploading pictures
        }
        // Event handler for the "Upload Frontal" button click
        private void UploadFrontal(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog // Open a file dialog to select an image file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", // Filter for image files
                Title = "Select Frontal picture" // Title of the file dialog
            };

            if (dlg.ShowDialog() == true) // Show the dialog and check if the user selected a file
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads"; // Define the uploads folder path
                string destFileName = System.IO.Path.Combine( // Combine the uploads folder path with a new file name
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // Use a GUID to ensure a unique file name

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true); // Copy the selected file to the uploads folder with the new file name

                _intakePictures.Add(new ProgressPicture // Add a new ProgressPicture object to the intake pictures list
                {
                    FilePath = destFileName, // Set the file path to the copied file
                    DateTaken = DateTime.UtcNow, // Set the date taken to the current UTC time
                    Type = "Frontal" // Set the type to "Frontal"
                });

                ImgFrontalPreview.Source = new BitmapImage(new Uri(destFileName)); // Set the image source for the Frontal preview to the copied file
                TxtFrontalFileName.Text = System.IO.Path.GetFileName(destFileName); // Set the text of the Frontal file name to the name of the copied file
            }
        }
        // Event handler for the "Upload Left" button click
        private void UploadLeft(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog // Open a file dialog to select an image file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", // Filter for image files
                Title = "Select Left picture" // Title of the file dialog
            };

            if (dlg.ShowDialog() == true) // Show the dialog and check if the user selected a file
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads"; // Define the uploads folder path
                string destFileName = System.IO.Path.Combine( // Combine the uploads folder path with a new file name
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // Use a GUID to ensure a unique file name

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true); // Copy the selected file to the uploads folder with the new file name

                _intakePictures.Add(new ProgressPicture // Add a new ProgressPicture object to the intake pictures list
                {
                    FilePath = destFileName, // Set the file path to the copied file
                    DateTaken = DateTime.UtcNow, // Set the date taken to the current UTC time
                    Type = "Left" // Set the type to "Left"
                });

                ImgLeftPreview.Source = new BitmapImage(new Uri(destFileName)); // Set the image source for the Left preview to the copied file
                TxtLeftFileName.Text = System.IO.Path.GetFileName(destFileName); // Set the text of the Left file name to the name of the copied file
            }
        }
        // Event handler for the "Upload Right" button click
        private void UploadRight(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog // Open a file dialog to select an image file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", // Filter for image files
                Title = "Select Right picture" // Title of the file dialog
            };

            if (dlg.ShowDialog() == true) // Show the dialog and check if the user selected a file
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads"; // Define the uploads folder path
                string destFileName = System.IO.Path.Combine( // Combine the uploads folder path with a new file name
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // Use a GUID to ensure a unique file name

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true); // Copy the selected file to the uploads folder with the new file name

                _intakePictures.Add(new ProgressPicture // Add a new ProgressPicture object to the intake pictures list
                {
                    FilePath = destFileName, // Set the file path to the copied file
                    DateTaken = DateTime.UtcNow, // Set the date taken to the current UTC time
                    Type = "Right" // Set the type to "Right"
                });

                ImgRightPreview.Source = new BitmapImage(new Uri(destFileName)); // Set the image source for the Right preview to the copied file
                TxtRightFileName.Text = System.IO.Path.GetFileName(destFileName); // Set the text of the Right file name to the name of the copied file
            }
        }
        // Event handler for the "Upload Back" button click
        private void UploadBack(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog // Open a file dialog to select an image file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", // Filter for image files
                Title = "Select Back picture" // Title of the file dialog
            };

            if (dlg.ShowDialog() == true) // Show the dialog and check if the user selected a file
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads"; // Define the uploads folder path
                string destFileName = System.IO.Path.Combine( // Combine the uploads folder path with a new file name
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // Use a GUID to ensure a unique file name

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true); // Copy the selected file to the uploads folder with the new file name

                _intakePictures.Add(new ProgressPicture // Add a new ProgressPicture object to the intake pictures list
                {
                    FilePath = destFileName, // Set the file path to the copied file
                    DateTaken = DateTime.UtcNow, // Set the date taken to the current UTC time
                    Type = "Back" // Set the type to "Back"
                });

                ImgBackPreview.Source = new BitmapImage(new Uri(destFileName)); // Set the image source for the Back preview to the copied file
                TxtBackFileName.Text = System.IO.Path.GetFileName(destFileName); // Set the text of the Back file name to the name of the copied file
            }
        }

        private void FinishIntake(object sender, RoutedEventArgs e) // This method is called when the user clicks the "Finish Intake" button
        {
            _newUser.CompletedIntake = true; // Mark the user as having completed the intake process
            // Spoof CreatedOn to 1 week ago
            _newUser.CreatedOn = DateTime.UtcNow.AddDays(-7); // Set the CreatedOn date to 1 week ago purely for testing purposes

            if (_newUser.WeightHistory.Count > 0) // Check if the user has entered any weight history
            {
                _newUser.WeightHistory[0].Date = _newUser.CreatedOn; // Set the date of the first weight entry to the CreatedOn date
            }
            if (_intakePictures.Count > 0) // If there are intake pictures, set the date taken for each picture to the CreatedOn date
            {
                foreach (var pic in _intakePictures) // Iterate through each intake picture
                {
                    pic.DateTaken = _newUser.CreatedOn; // Set the date taken for each picture to the CreatedOn date
                }

                _newUser.WeeklyProgressPictures.Add(new WeeklyProgress // Add a new WeeklyProgress entry to the user's weekly progress pictures
                {
                    WeekNumber = 0, // Set the week number to 0 for the initial intake pictures
                    Pictures = _intakePictures // Assign the list of intake pictures to the WeeklyProgress entry
                });
            }
            var db = new LocalDatabaseService(); // Create an instance of the LocalDatabaseService to interact with the local database
            db.SaveUser(_newUser); // Save the new user to the local database
            db.SaveCurrentUserId(_newUser.Id); // Save the current user ID to the local database

            ((MainWindow)Application.Current.MainWindow).Header.Visibility = Visibility.Visible; //Make the header visible in the main window
            NavigationService.Navigate(new HomePage()); // Navigate to the HomePage after completing the intake process
        }
    }
}
