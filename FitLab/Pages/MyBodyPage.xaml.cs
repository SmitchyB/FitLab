using FitLab.AppState;
using FitLab.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Pages
{
    /// <summary>
    /// This page allows users to view and edit their body-related information such as name, date of birth, gender, height, and weight history.
    /// Future updates will include before and weekly progress images, goal management, water and food tracking, and more.
    /// </summary>
    public partial class MyBodyPage : Page
    {
        private readonly LocalDatabaseService _db = new(); // Local database service for loading and saving user data
        private readonly User _user;// Current user data loaded from the database
        private int _currentWeightWeek = 0; // Current week number for weight tracking, starting from 0
        private string _currentWeightUnit = "Lbs"; // Current weight unit, defaulting to pounds
        private Goal? _editingGoal = null;// currently editing goal
        private readonly List<Goal> _goals = new(); // local copy of goals


        public MyBodyPage()
        {
            InitializeComponent(); // Initialize the page components
            CmbWeightUnit.SelectionChanged += CmbWeightUnit_SelectionChanged; // Event handler for weight unit selection change

            // Load user from DB
            _user = _db.LoadFirstUser() ?? new User(); // Load the first user from the database or create a new user if none exists
            _currentWeightWeek = FitLab.AppState.SessionState.CurrentWeek; //Week 3 update: Set the current weight week based on the session state week # calculated on start of app

            // Populate fields
            TxtName.Text = _user.Name; // User's name
            DatePickerDOB.SelectedDate = _user.DateOfBirth; // User's date of birth
            // Gender
            foreach (ComboBoxItem item in CmbGender.Items) // Iterate through
            {
                if ((item.Content?.ToString() ?? "") == _user.Gender.Replace(" (On Hormones)", "").Replace(" (Not on Hormones)", "")) // Check if the item's content matches and ignore hormone status
                {
                    CmbGender.SelectedItem = item; // Set the selected item to the matching
                    break; // Exit loop once found
                }
            }

            if (_user.Gender.Contains("Trans-Feminine") || _user.Gender.Contains("Trans-Masculine")) // Check if the user's gender contains Trans-Feminine or Trans-Masculine
            {
                ChkOnHormones.Visibility = Visibility.Visible; // Show the hormone checkbox
                ChkOnHormones.IsChecked = _user.Gender.Contains("On Hormones"); // Set the checkbox state based on the users homrone status
            }

            // Height display in Feet/Inches by default
            CmbHeightUnit.SelectedIndex = 2; // Set the height unit to Feet/Inches by default
            UpdateHeightDisplay(_user.HeightInches); // Update the height display based on the user's height in inches
            PanelFeetInches.Visibility = Visibility.Visible; // Show the Feet/Inches panel
            UpdateWeightDisplay(); // Update the weight display based on the current weight week and unit

            _goals = new List<Goal>(_user.Goals);
            RefreshGoalsList();

        }
        // This method updates the height display based on the selected unit and the user's height in inches.
        private void UpdateHeightDisplay(double heightInches)
        {
            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected) // Check if a height unit is selected
            {
                var unit = selected.Content?.ToString() ?? ""; // Get the selected unit as a string

                if (unit == "Centimeters") // If the selected unit is Centimeters
                {
                    TxtHeightPrimary.Text = Math.Round(Components.Conversions.InchesToCm(heightInches), 1).ToString(); // Convert inches to centimeters and round to 1 decimal place
                }
                else if (unit == "Inches") // If the selected unit is Inches
                {
                    TxtHeightPrimary.Text = Math.Round(heightInches, 1).ToString(); // Round inches to 1 decimal place
                }
                else if (unit == "Feet/Inches") // If the selected unit is Feet/Inches
                {
                    var parts = Components.Conversions.InchesToFeetInches(heightInches); // Convert inches to feet and inches
                    TxtHeightFeet.Text = parts.Feet.ToString(); // Set the feet part
                    TxtHeightInches.Text = Math.Round(parts.Inches, 1).ToString(); // Set the inches part rounded to 1 decimal place
                }
            }
        }
        // Event handler for the Gender ComboBox selection change.
        private void CmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected) // Check for the selected option in the combo box
            {
                var value = selected.Content?.ToString() ?? ""; // Get the selected value as a string
                if (value == "Trans-Feminine" || value == "Trans-Masculine") // If the selected value is Trans-Feminine or Trans-Masculine
                {
                    ChkOnHormones.Visibility = Visibility.Visible; // Show the hormone checkbox
                }
                else // for all other options
                {
                    ChkOnHormones.Visibility = Visibility.Collapsed; // Hide the hormone checkbox
                }
            }
        }
        // Event handler for the Height Unit ComboBox selection change.
        private void CmbHeightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hide panels
            PanelSingleInput.Visibility = Visibility.Collapsed; // Hide the single input panel
            PanelFeetInches.Visibility = Visibility.Collapsed; // Hide the feet/inches panel
            
            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected) // Check if a height unit is selected
            {
                var value = selected.Content?.ToString() ?? ""; // Get the selected unit as a string

                if (value == "Centimeters" || value == "Inches") // If the selected unit is Centimeters or Inches
                {
                    PanelSingleInput.Visibility = Visibility.Visible; // Show the single input panel
                }
                else if (value == "Feet/Inches") // If the selected unit is Feet/Inches
                {
                    PanelFeetInches.Visibility = Visibility.Visible; // Show the feet/inches panel
                }

                // Update displayed values from stored height
                UpdateHeightDisplay(_user.HeightInches);
            }
        }
        // Event handler for the Edit button click.
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

            TxtName.IsReadOnly = false; // Allow editing of the name
            CmbGender.IsEnabled = true; // Enable the combo box for Gender selection
            DatePickerDOB.IsEnabled = true; // Enable the date picker for Date of Birth selection
            TxtHeightPrimary.IsReadOnly = false; // Allow editing of the primary height input
            TxtHeightFeet.IsReadOnly = false; // Allow editing of the feet input
            TxtHeightInches.IsReadOnly = false; // Allow editing of the inches input
            BtnUpdate.Visibility = Visibility.Visible; // Show the Update button to save changes
        }
        // Event handler for the Update button click.
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

            // Save inputs to _user
            _user.Name = TxtName.Text.Trim(); // Trim and save the name input
            _user.DateOfBirth = DatePickerDOB.SelectedDate ?? DateTime.MinValue; // Save the selected date of birth, defaulting to MinValue if not selected
            if (CmbGender.SelectedItem is ComboBoxItem selected) // Check for the selected option in the combo box
            {
                var genderBase = selected.Content?.ToString() ?? ""; // Get the selection as a string
                if (genderBase == "Trans-Feminine" || genderBase == "Trans-Masculine") // If the selected is Trans-Feminine or Trans-Masculine
                {
                    _user.Gender = genderBase + (ChkOnHormones.IsChecked == true ? " (On Hormones)" : " (Not on Hormones)"); // Append hormone status based on checkbox state
                }
                else // For all other options
                {
                    _user.Gender = genderBase; // Save the selected option
                }
            }

            if (CmbHeightUnit.SelectedItem is ComboBoxItem heightSelected) // Check if a height unit is selected
            {
                var unit = heightSelected.Content?.ToString() ?? ""; // Get the selected height unit as a string

                if (unit == "Centimeters") // If the selected unit is Centimeters
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double cm)) // Try to parse the primary height input as a double
                        _user.HeightInches = Components.Conversions.CmToInches(cm); // Convert centimeters to inches and save
                }
                else if (unit == "Inches") // If the selected unit is Inches
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double inches)) // Try to parse the primary height input as a double
                        _user.HeightInches = inches; // Save the inches directly
                }
                else if (unit == "Feet/Inches") // If the selected unit is Feet/Inches
                {
                    if (int.TryParse(TxtHeightFeet.Text, out int feet) && double.TryParse(TxtHeightInches.Text, out double inches)) // Try to parse the feet and inches inputs
                        _user.HeightInches = Components.Conversions.FeetInchesToInches(feet, inches); // Convert feet and inches to total inches and save
                }
            }

            _db.SaveUser(_user); // Save the updated user data to the database
            TxtName.IsReadOnly = true; // Make the name input read-only
            CmbGender.IsEnabled = false; // Disable the gender combo box
            DatePickerDOB.IsEnabled = false; // Disable the date picker for Date of Birth
            TxtHeightPrimary.IsReadOnly = true; // Make the primary height input read-only
            TxtHeightFeet.IsReadOnly = true; // Make the feet input read-only
            TxtHeightInches.IsReadOnly = true; // Make the inches input read-only
            BtnUpdate.Visibility = Visibility.Collapsed; // Hide the Update button after saving changes
        }
        // Event handler for the Weight Unit ComboBox selection change.
        private void CmbWeightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbWeightUnit.SelectedItem is ComboBoxItem selected) // Check if a weight unit is selected
                _currentWeightUnit = selected.Content?.ToString() ?? "Lbs"; // Get the selected weight unit as a string, defaulting to "Lbs" if not found
            UpdateWeightDisplay(); // Update the weight display based on the selected unit
        }
        // Event handlers for navigating through weight weeks
        private void BtnWeightWeekBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWeightWeek > 0) // Ensure we don't go back past week 0
                _currentWeightWeek--; // Decrement the current weight week

            UpdateWeightDisplay(); // Update the weight display after changing the week
        }
        // Event handler for moving forward through weight weeks
        private void BtnWeightWeekForward_Click(object sender, RoutedEventArgs e)
        {
            _currentWeightWeek++; // Increment the current weight week
            UpdateWeightDisplay(); // Update the weight display after changing the week
        }
        // This method updates the weight display based on the current weight week and unit.
        private void UpdateWeightDisplay()
        {
            TxtCurrentWeek.Text = $"Week {_currentWeightWeek}"; // Display the current weight week

            var startingWeightEntry = _user.WeightHistory.Count > 0 ? _user.WeightHistory[0] : null; // Get the starting weight entry, if it exists

            if (startingWeightEntry != null) // If a starting weight entry exists
            {
                double value = _currentWeightUnit == "Kilograms" // Check if the current weight unit is Kilograms
                    ? Components.Conversions.LbsToKg(startingWeightEntry.WeightLbs) // Convert pounds to kilograms
                    : startingWeightEntry.WeightLbs; // Use pounds directly if the unit is not Kilograms

                TxtStartingWeight.Text = $"Starting Weight: {Math.Round(value, 1)} {_currentWeightUnit}"; // Display the starting weight rounded to 1 decimal place with the current unit
            }
            else // If no starting weight entry exists
            {
                TxtStartingWeight.Text = "Starting Weight: Not Recorded"; // Display a message indicating that the starting weight is not recorded
            }

            WeightEntry? weekEntry = null; // Initialize the week entry to null

            if (_currentWeightWeek < _user.WeightHistory.Count) // If the current weight week is within the range of recorded weeks
            {
                weekEntry = _user.WeightHistory[_currentWeightWeek]; // Get the weight entry for the current week
            }

            if (weekEntry != null) // If a weight entry for the current week exists
            {
                TxtWeeklyWeight.IsReadOnly = !IsCurrentWeekEditable(weekEntry); // Set the read-only state of the weekly weight input based on whether the current week is editable

                double value = _currentWeightUnit == "Kilograms" // Check if the current weight unit is Kilograms
                    ? Components.Conversions.LbsToKg(weekEntry.WeightLbs) // Convert pounds to kilograms
                    : weekEntry.WeightLbs; // Use pounds directly if the unit is not Kilograms

                TxtWeeklyWeight.Text = Math.Round(value, 1).ToString(); // Display the weekly weight rounded to 1 decimal place
            }
            else // If no weight entry exists for the current week
            {
                TxtWeeklyWeight.IsReadOnly = !IsCurrentWeekEditable(null); // Set the read-only state of the weekly weight input based on whether the current week is editable
                TxtWeeklyWeight.Text = ""; // Clear the weekly weight input if no entry exists for the current week 
            }

            if (weekEntry == null && !TxtWeeklyWeight.IsReadOnly) // If no weight entry exists for the current week and the input is not read-only
            {
                BtnWeightSave.Visibility = Visibility.Visible; // Show the Save button to allow saving a new weight entry
            }
            else // If a weight entry exists for the current week or the input is read-only
            {
                BtnWeightSave.Visibility = Visibility.Collapsed; // Hide the Save button
            }
        }
        // This method checks if the current week is editable based on the user's weight entry.
        private bool IsCurrentWeekEditable(FitLab.Data.WeightEntry? entry)
        {
            var currentWeek = SessionState.CurrentWeek;//Week 3 update: Get the current week number from the session state


            if (_currentWeightWeek == 0) // If the current week is week 0
            {
                return entry == null; // Week 0 is editable only if no entry exists
            }
            return _currentWeightWeek == currentWeek && entry == null; // For all other weeks, they are editable only if the current week matches and no entry exists
        }
        // Event handler for the Save button click to save the weekly weight entry.
        private void BtnWeightSave_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtWeeklyWeight.Text.Trim(), out double entered)) //Try to parse the entered weight from the input field
            {
                MessageBox.Show("Enter a valid number."); // Show an error message if the input is not a valid number
                return; // Exit the method if the input is invalid
            }

            double weightLbs = _currentWeightUnit == "Kilograms" // Check if the current weight unit is Kilograms
                ? Components.Conversions.KgToLbs(entered) // Convert kilograms to pounds
                : entered; // Use the entered value directly if the unit is not Kilograms

            if (_currentWeightWeek < _user.WeightHistory.Count) //If the current weight week is within the range of recorded weeks
            {
                // Should not happen because I lock editing if it exists, but safeguard it anyway
                MessageBox.Show("This week's weight is already recorded."); // Show an error message if the weight for the current week is already recorded
                return; // Exit the method if the weight is already recorded
            }

            _user.WeightHistory.Add(new WeightEntry // Add a new weight entry to the user's weight history
            {
                Date = _user.CreatedOn.Date.AddDays(_currentWeightWeek * 7), // Set the date of the entry based on the user's creation date and the current week
                WeightLbs = weightLbs // Set the weight in pounds
            });

            _db.SaveUser(_user); // Save the updated user data to the database

            BtnWeightSave.Visibility = Visibility.Collapsed; // Hide the Save button after saving the weight entry
            UpdateWeightDisplay(); // Update the weight display to reflect the newly saved entry
        }
        private void RefreshGoalsList()
        {
            GoalsList.Items.Clear();

            foreach (var goal in _goals)
            {
                string status = FitLab.Components.GoalTimeRemainder.GetTimeRemainingString(goal);
                GoalsList.Items.Add($"{goal.Description} - {status}");
            }
        }

        private void AddOrUpdateGoal(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtGoalDescription.Text)
                && int.TryParse(TxtGoalTimeframeAmount.Text.Trim(), out int amount)
                && CmbGoalTimeframeUnit.SelectedItem is ComboBoxItem selectedUnit)
            {
                if (_editingGoal == null)
                {
                    // Adding new
                    var goal = new Goal
                    {
                        Description = TxtGoalDescription.Text.Trim(),
                        TimeframeAmount = amount,
                        TimeframeUnit = selectedUnit.Content?.ToString() ?? "",
                        Date = DateTime.UtcNow
                    };
                    _goals.Add(goal);
                }
                else
                {
                    // Updating existing
                    _editingGoal.Description = TxtGoalDescription.Text.Trim();
                    _editingGoal.TimeframeAmount = amount;
                    _editingGoal.TimeframeUnit = selectedUnit.Content?.ToString() ?? "";
                    // Do not touch Date or Completed fields
                    _editingGoal = null;
                    BtnAddOrUpdateGoal.Content = "Add Goal";
                }

                SaveGoals();
                RefreshGoalsList();

                TxtGoalDescription.Text = "";
                TxtGoalTimeframeAmount.Text = "";
                CmbGoalTimeframeUnit.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Please enter description, amount, and timeframe.");
            }
        }
        private void EditGoal(object sender, RoutedEventArgs e)
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count)
            {
                _editingGoal = _goals[GoalsList.SelectedIndex];

                TxtGoalDescription.Text = _editingGoal.Description;
                TxtGoalTimeframeAmount.Text = _editingGoal.TimeframeAmount.ToString();

                foreach (ComboBoxItem item in CmbGoalTimeframeUnit.Items)
                {
                    if (item.Content?.ToString() == _editingGoal.TimeframeUnit)
                    {
                        CmbGoalTimeframeUnit.SelectedItem = item;
                        break;
                    }
                }

                BtnAddOrUpdateGoal.Content = "Update Goal";
            }
        }
        private void DeleteGoal(object sender, RoutedEventArgs e)
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count)
            {
                // If the goal we're editing is the one being deleted, clear editing state
                if (_editingGoal == _goals[GoalsList.SelectedIndex])
                {
                    _editingGoal = null;
                    BtnAddOrUpdateGoal.Content = "Add Goal";
                }

                _goals.RemoveAt(GoalsList.SelectedIndex);

                SaveGoals();
                RefreshGoalsList();
            }
        }
        private void CompleteGoal(object sender, RoutedEventArgs e)
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count)
            {
                var goal = _goals[GoalsList.SelectedIndex];
                goal.IsCompleted = true;
                goal.CompletedOn = DateTime.UtcNow;

                SaveGoals();
                RefreshGoalsList();
            }
        }
        private void SaveGoals()
        {
            _user.Goals = _goals;
            _db.SaveUser(_user);
        }

    }
}
