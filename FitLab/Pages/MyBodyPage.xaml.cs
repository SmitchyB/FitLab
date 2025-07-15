using FitLab.AppState;
using FitLab.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        private ObservableCollection<Dish> _currentEditingDishes = new();
        private Meal? _mealBeingEdited = null;
        private readonly string _uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads";
        private WeeklyProgress? _currentWeekProgress = null;
        private int _currentPictureWeek = 0;
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
            _goals = new List<Goal>(_user.Goals); // Load goals from the user data
            RefreshGoalsList(); // Refresh the goals list to display current goals
            DatePickerSelectedDate.SelectedDate = DateTime.Today; // Set the selected date for water intake to today
            LoadWaterIntakeForDate(DateTime.Today); // Load water intake for today
            DatePickerFoodDate.SelectedDate = DateTime.Today; // Set the selected date for food intake to today
            LoadMealsForDate(DateTime.Today); // Load meals for today
            LoadBeforePictures(); // Load before pictures for the user
            LoadCurrentPictureWeek(); // Load the current picture week to display progress pictures
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
        // Refreshes the goals list displayed in the UI.
        private void RefreshGoalsList()
        {
            GoalsList.Items.Clear(); // Clear the existing items in the goals list

            foreach (var goal in _goals) // Iterate through each goal in the user's goals
            {
                string status = FitLab.Components.GoalTimeRemainder.GetTimeRemainingString(goal); // Get the time remaining for the goal as a string
                GoalsList.Items.Add($"{goal.Description} - {status}"); // Add the goal description and status to the goals list
            }
        }
        // Event handler for adding or updating a goal.
        private void AddOrUpdateGoal(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtGoalDescription.Text) // Check if the goal description is not empty
                && int.TryParse(TxtGoalTimeframeAmount.Text.Trim(), out int amount) // Try to parse the timeframe amount as an integer
                && CmbGoalTimeframeUnit.SelectedItem is ComboBoxItem selectedUnit) // Check if a timeframe unit is selected
            {
                if (_editingGoal == null) // If we are not currently editing an existing goal
                {
                    var goal = new Goal // Create a new goal object
                    {
                        Description = TxtGoalDescription.Text.Trim(), // Set the goal description
                        TimeframeAmount = amount, // Set the timeframe amount
                        TimeframeUnit = selectedUnit.Content?.ToString() ?? "", // Set the timeframe unit
                        Date = DateTime.UtcNow // Set the date to the current UTC time
                    };
                    _goals.Add(goal); // Add the new goal to the local goals list
                }
                else // If we are editing an existing goal
                {
                    _editingGoal.Description = TxtGoalDescription.Text.Trim(); // Update the goal description
                    _editingGoal.TimeframeAmount = amount; // Update the timeframe amount
                    _editingGoal.TimeframeUnit = selectedUnit.Content?.ToString() ?? ""; // Update the timeframe unit
                    _editingGoal = null; // Clear the editing state
                    BtnAddOrUpdateGoal.Content = "Add Goal"; // Reset the button text to "Add Goal"
                }
                SaveGoals(); // Save the updated goals to the database
                RefreshGoalsList(); // Refresh the goals list to display the updated goals
                TxtGoalDescription.Text = ""; // Clear the goal description input field
                TxtGoalTimeframeAmount.Text = ""; // Clear the timeframe amount input field
                CmbGoalTimeframeUnit.SelectedIndex = -1; // Reset the timeframe unit selection to none
            }
            else // If any of the required fields are empty or invalid
            {
                MessageBox.Show("Please enter description, amount, and timeframe."); // Show an error message prompting the user to fill in the required fields
            }
        }
        // Event handler for editing a selected goal from the goals list.
        private void EditGoal(object sender, RoutedEventArgs e) 
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count) // Check if a goal is selected in the goals list
            {
                _editingGoal = _goals[GoalsList.SelectedIndex]; // Set the editing goal to the selected goal

                TxtGoalDescription.Text = _editingGoal.Description; // Populate the goal description input field with the selected goal's description
                TxtGoalTimeframeAmount.Text = _editingGoal.TimeframeAmount.ToString(); // Populate the timeframe amount input field with the selected goal's timeframe amount

                foreach (ComboBoxItem item in CmbGoalTimeframeUnit.Items) // Iterate through the items in the timeframe unit combo box
                {
                    if (item.Content?.ToString() == _editingGoal.TimeframeUnit) // Check if the item's content matches the selected goal's timeframe unit
                    {
                        CmbGoalTimeframeUnit.SelectedItem = item; // Set the selected item in the combo box to the matching item
                        break; // Exit the loop once the matching item is found
                    }
                }

                BtnAddOrUpdateGoal.Content = "Update Goal"; // Change the button text to "Update Goal" to indicate that we are editing an existing goal
            }
        }
        // Event handler for deleting a selected goal from the goals list.
        private void DeleteGoal(object sender, RoutedEventArgs e) 
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count) // Check if a goal is selected in the goals list
            {
                var result = MessageBox.Show("Are you sure you want to delete this goal?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning); // Show confirmation dialog
                if (result != MessageBoxResult.Yes) return; // If the user did not confirm, exit the method
                if (_editingGoal == _goals[GoalsList.SelectedIndex]) // If the goal being edited is the one selected for deletion
                {
                    _editingGoal = null; // Clear the editing state
                    BtnAddOrUpdateGoal.Content = "Add Goal"; // Reset the button text to "Add Goal"
                }

                _goals.RemoveAt(GoalsList.SelectedIndex); // Remove the selected goal from the local goals list

                SaveGoals(); // Save the updated goals to the database
                RefreshGoalsList(); // Refresh the goals list to display the updated goals
            }
        }
        // Event handler for completing a selected goal from the goals list.
        private void CompleteGoal(object sender, RoutedEventArgs e)
        {
            if (GoalsList.SelectedIndex >= 0 && GoalsList.SelectedIndex < _goals.Count) // Check if a goal is selected in the goals list
            {
                var goal = _goals[GoalsList.SelectedIndex]; // Get the selected goal from the local goals list
                goal.IsCompleted = true; // Mark the goal as completed
                goal.CompletedOn = DateTime.UtcNow; // Set the completion date to the current UTC time

                SaveGoals(); // Save the updated goals to the database
                RefreshGoalsList(); // Refresh the goals list to display the updated goals
            }
        }
        //Helper method for saving the goals to the user data and database.
        private void SaveGoals()
        {
            _user.Goals = _goals; // Update the user's goals with the local goals list
            _db.SaveUser(_user); // Save the updated user data to the database
        }
        // Event handler for the Date picker back button
        private void BtnDateBack_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerSelectedDate.SelectedDate.HasValue) // Check if a date is selected in the date picker
                DatePickerSelectedDate.SelectedDate = DatePickerSelectedDate.SelectedDate.Value.AddDays(-1); // Move the selected date back by one day
        }
        //Event handler for the Date picker forward button
        private void BtnDateForward_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerSelectedDate.SelectedDate.HasValue) // Check if a date is selected in the date picker
                DatePickerSelectedDate.SelectedDate = DatePickerSelectedDate.SelectedDate.Value.AddDays(1); // Move the selected date forward by one day
        }
        // Event handler for the Date picker selection change
        private void DatePickerSelectedDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadWaterIntakeForDate(DatePickerSelectedDate.SelectedDate ?? DateTime.Today); // Load the water intake for the selected date or default to today if no date is selected
        }
        // This method loads the water intake for a specific date and updates the UI accordingly.
        private void LoadWaterIntakeForDate(DateTime date)
        {
            var entry = _user.WaterIntake.FirstOrDefault(w => w.Date.Date == date.Date); // Find the water intake entry for the selected date
            int cups = entry?.Cups ?? 0; // Get the number of cups consumed on that date, defaulting to 0 if no entry exists
            TxtWaterCups.Text = $"{cups} Cup(s)"; // Update the water cups text with the number of cups consumed on that date
        }
        // Event handler for increasing the water cups count
        private void BtnIncreaseCups_Click(object sender, RoutedEventArgs e)
        {
            AdjustCups(1); // Call the AdjustCups method with a delta of 1 to increase the cups count
        }
        // Event handler for decreasing the water cups count
        private void BtnDecreaseCups_Click(object sender, RoutedEventArgs e)
        {
            AdjustCups(-1); // Call the AdjustCups method with a delta of -1 to decrease the cups count
        }
        // This method adjusts the water cups count for the selected date by a specified delta (positive or negative).
        private void AdjustCups(int delta)
        {
            var date = DatePickerSelectedDate.SelectedDate ?? DateTime.Today; // Get the selected date from the date picker or default to today if no date is selected
            var entry = _user.WaterIntake.FirstOrDefault(w => w.Date.Date == date.Date); // Find the water intake entry for the selected date

            if (entry == null) // If no entry exists for the selected date
            {
                entry = new DailyWaterIntake // Create a new DailyWaterIntake entry for the selected date
                {
                    Date = date.Date, // Set the date to the selected date
                    Cups = 0 // Initialize the cups count to 0
                };
                _user.WaterIntake.Add(entry); // Add the new entry to the user's water intake list
            }

            entry.Cups = Math.Max(0, entry.Cups + delta); // Update the cups count by adding the delta, ensuring it does not go below 0
            TxtWaterCups.Text = $"{entry.Cups} Cup(s)"; // Update the water cups text with the new cups count
            _db.SaveUser(_user); // Save the updated user data to the database
        }
        // Event handler for navigating backwards through food intake dates
        private void BtnFoodDateBack_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerFoodDate.SelectedDate.HasValue) // Check if a date is selected in the food date picker
                DatePickerFoodDate.SelectedDate = DatePickerFoodDate.SelectedDate.Value.AddDays(-1); // Move the selected date back by one day
        }
        //Event handler for navigating forwards through food intake dates
        private void BtnFoodDateForward_Click(object sender, RoutedEventArgs e)
        {
            if (DatePickerFoodDate.SelectedDate.HasValue) // Check if a date is selected in the food date picker
                DatePickerFoodDate.SelectedDate = DatePickerFoodDate.SelectedDate.Value.AddDays(1); // Move the selected date forward by one day
        }
        // Event handler for the food date picker selection change
        private void DatePickerFoodDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMealsForDate(DatePickerFoodDate.SelectedDate ?? DateTime.Today); // Load the meals for the selected date or default to today if no date is selected
        }
        // This method loads the meals for a specific date and updates the UI accordingly.
        private void LoadMealsForDate(DateTime date)
        {
            var entry = _user.FoodIntake.FirstOrDefault(x => x.Date.Date == date.Date); // Find the food intake entry for the selected date
            if (entry != null) // If an entry exists for the selected date
            {
                ListMeals.ItemsSource = entry.Meals; // Set the ItemsSource of the ListMeals control to the meals for that date
            }
            else // If no entry exists for the selected date
            {
                ListMeals.ItemsSource = null; // Clear the ItemsSource of the ListMeals control
            }
        }
        // Event handler for showing the Add Meal panel
        private void BtnShowAddMeal_Click(object sender, RoutedEventArgs e)
        {
            _mealBeingEdited = null; // Reset the meal being edited to null
            _currentEditingDishes = new ObservableCollection<Dish>(); // Initialize the current editing dishes collection to an empty ObservableCollection
            PanelDishes.ItemsSource = _currentEditingDishes; // Set the ItemsSource of the PanelDishes control to the current editing dishes collection
            CmbMealTime.SelectedIndex = -1; // Reset the selected meal time in the ComboBox to none
            PanelAddMeal.Visibility = Visibility.Visible; // Show the Add Meal panel
        }
        // Event handlers for adding and dishes in the Add Meal panel
        private void BtnAddDish_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingDishes.Add(new Dish()); // Add a new Dish object to the current editing dishes collection
        }
        // Event handler for removing a dish from the Add Meal panel
        private void BtnRemoveDish_Click(object sender, RoutedEventArgs e)
        {
            var dish = ((FrameworkElement)sender).DataContext as Dish; // Get the Dish object from the DataContext of the clicked element
            if (dish != null) // If the dish is not null
            {
                _currentEditingDishes.Remove(dish); // Remove the dish from the current editing dishes collection
            }
        }
        // Event handler for saving a meal in the Add Meal panel
        private void BtnSaveMeal_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMealTime.SelectedItem is not ComboBoxItem selectedItem) // Check if a meal time is selected in the ComboBox
            {
                MessageBox.Show("Select a meal time."); // Show an error message if no meal time is selected
                return; // Exit the method if no meal time is selected
            }

            string mealTime = selectedItem.Content?.ToString() ?? ""; // Get the selected meal time as a string, defaulting to an empty string if not found
            var date = DatePickerFoodDate.SelectedDate ?? DateTime.Today; // Get the selected date from the food date picker or default to today if no date is selected

            var entry = _user.FoodIntake.FirstOrDefault(x => x.Date.Date == date.Date); // Find the food intake entry for the selected date
            if (entry == null) // If no entry exists for the selected date
            {
                entry = new DailyFoodIntake // Create a new DailyFoodIntake entry for the selected date
                {
                    Date = date.Date, // Set the date to the selected date
                    Meals = new List<Meal>() // Initialize the meals list to an empty list
                };
                _user.FoodIntake.Add(entry); // Add the new entry to the user's food intake list
            }

            if (_mealBeingEdited != null) // If we are editing an existing meal
            {
                _mealBeingEdited.MealTime = mealTime; // Update the meal time of the meal being edited
                _mealBeingEdited.Dishes = _currentEditingDishes.ToList(); // Update the dishes of the meal being edited with the current editing dishes collection
            }
            else // If we are adding a new meal
            {
                entry.Meals.Add(new Meal // Create a new Meal object and add it to the meals list of the entry
                {
                    MealTime = mealTime, // Set the meal time to the selected meal time
                    Dishes = _currentEditingDishes.ToList() // Set the dishes of the meal to the current editing dishes collection
                });
            }

            _db.SaveUser(_user); // Save the updated user data to the database

            PanelAddMeal.Visibility = Visibility.Collapsed; // Hide the Add Meal panel
            _mealBeingEdited = null; // Reset the meal being edited to null

            LoadMealsForDate(date); // Reload the meals for the selected date to reflect the changes made
        }
        // Event handlers for editing and deleting meals in the ListMeals control
        private void BtnEditMeal_Click(object sender, RoutedEventArgs e)
        {
            var meal = ((FrameworkElement)sender).DataContext as Meal; // Get the Meal object from the DataContext of the clicked element
            if (meal == null) return; // If the meal is null, exit the method

            _mealBeingEdited = meal; // Set the meal being edited to the selected meal
            _currentEditingDishes = new ObservableCollection<Dish>(meal.Dishes); // Initialize the current editing dishes collection with the dishes of the selected meal
            PanelDishes.ItemsSource = _currentEditingDishes; // Set the ItemsSource of the PanelDishes control to the current editing dishes collection

            foreach (ComboBoxItem item in CmbMealTime.Items) // Iterate through the items in the meal time ComboBox
            {
                if ((item.Content?.ToString() ?? "") == meal.MealTime) // Check if the item's content matches the meal time of the selected meal
                {
                    CmbMealTime.SelectedItem = item; // Set the selected item in the ComboBox to the matching item
                    break; // Exit the loop once the matching item is found
                }
            }

            PanelAddMeal.Visibility = Visibility.Visible; // Show the Add Meal panel to allow editing of the selected meal
        }
        // Event handler for deleting a meal from the ListMeals control
        private void BtnDeleteMeal_Click(object sender, RoutedEventArgs e)
        {
            var meal = ((FrameworkElement)sender).DataContext as Meal; // Get the Meal object from the DataContext of the clicked element
            if (meal == null) return; // If the meal is null, exit the method

            var date = DatePickerFoodDate.SelectedDate ?? DateTime.Today; // Get the selected date from the food date picker or default to today if no date is selected
            var entry = _user.FoodIntake.FirstOrDefault(x => x.Date.Date == date.Date); // Find the food intake entry for the selected date
            if (entry != null) // If an entry exists for the selected date
            {
                entry.Meals.Remove(meal); // Remove the selected meal from the meals list of the entry
                _db.SaveUser(_user); // Save the updated user data to the database
                LoadMealsForDate(date); // Reload the meals for the selected date to reflect the changes made
            }
        }
        // Event handler for loading the Before pictures when the page is loaded
        private void LoadBeforePictures()
        {
            PanelBeforePictures.Children.Clear(); // Clear any existing pictures in the Before Pictures panel

            if (_user.WeeklyProgressPictures.Count > 0) // Check if there are any weekly progress pictures
            {
                var beforeProgress = _user.WeeklyProgressPictures[0]; // Get the first weekly progress pictures (which is considered "Before" pictures)
                foreach (var pic in beforeProgress.Pictures) // Iterate through each picture in the before progress
                {
                    var img = new Image //Create a new Image control for each picture
                    {
                        Width = 120, // Set the width of the image
                        Height = 120, // Set the height of the image
                        Margin = new Thickness(5), // Set the margin around the image
                        Stretch = System.Windows.Media.Stretch.Uniform, // Set the stretch mode to uniform to maintain aspect ratio
                        Source = new BitmapImage(new Uri(pic.FilePath)) // Set the source of the image to the file path of the picture
                    };
                    PanelBeforePictures.Children.Add(img); // Add the image to the Before Pictures panel
                }
            }
        }
        // Event handler for the Picture Week Back button click
        private void BtnPictureWeekBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPictureWeek > 0) // Ensure we don't go back past week 0
                _currentPictureWeek--; // Decrement the current picture week

            LoadCurrentPictureWeek(); // Load the current picture week to update the UI
        }
        // Event handler for the Picture Week Forward button click
        private void BtnPictureWeekForward_Click(object sender, RoutedEventArgs e)
        {
            _currentPictureWeek++; // Increment the current picture week
            LoadCurrentPictureWeek(); // Load the current picture week to update the UI
        }
        // This method loads the current picture week and updates the UI accordingly.
        private void LoadCurrentPictureWeek()
        {
            TxtCurrentPictureWeek.Text = $"Week {_currentPictureWeek}"; // Display the current picture week

            ResetCurrentWeekUI(); // Reset the UI for the current week to its default state

            if (_currentPictureWeek == 0) // If the current picture week is 0 (Before pictures set in user intake)
            {
                if (_user.WeeklyProgressPictures.Count > 0) // Check if there are any weekly progress pictures
                {
                    var beforeProgress = _user.WeeklyProgressPictures[0]; // Get the first weekly progress pictures
                    foreach (var pic in beforeProgress.Pictures)   // Iterate through each picture in the before progress
                    {
                        ShowPicturePreview(pic.Type, pic.FilePath); // Show the picture preview for each picture
                    }
                }
                BtnUploadFrontal.Visibility = Visibility.Collapsed; // Hide the Frontal upload button
                BtnUploadLeft.Visibility = Visibility.Collapsed; // Hide the Left upload button
                BtnUploadRight.Visibility = Visibility.Collapsed; // Hide the Right upload button
                BtnUploadBack.Visibility = Visibility.Collapsed; // Hide the Back upload button
                return; // Exit the method
            }

            if (_user.WeeklyProgressPictures.Count > _currentPictureWeek) //Check if there are pictures for the current week
            {
                _currentWeekProgress = _user.WeeklyProgressPictures[_currentPictureWeek]; // Get the weekly progress pictures for the current week
            }
            else // If no pictures exist for the current week
            {
                _currentWeekProgress = new WeeklyProgress(); // Create a new WeeklyProgress object for the current week
            }

            foreach (var pic in _currentWeekProgress.Pictures) //Iterate through each picture in the current week's progress
            {
                ShowPicturePreview(pic.Type, pic.FilePath); // Show the picture preview for each picture
                HideUploadButton(pic.Type); // Hide the upload button for the picture type that has already been uploaded
            }

            if (_currentPictureWeek != SessionState.CurrentWeek) // If the current picture week is not the current session week
            {
                BtnUploadFrontal.Visibility = Visibility.Collapsed; // Hide the Frontal upload button
                BtnUploadLeft.Visibility = Visibility.Collapsed; // Hide the Left upload button
                BtnUploadRight.Visibility = Visibility.Collapsed; // Hide the Right upload button
                BtnUploadBack.Visibility = Visibility.Collapsed; // Hide the Back upload button
            }
            else // If the current picture week is the current session week
            {
                if (!BtnUploadFrontal.IsVisible) // Check if the Frontal upload button is not visible
                    BtnUploadFrontal.Visibility = Visibility.Visible; // Show the Frontal upload button
                if (!BtnUploadLeft.IsVisible) // Check if the Left upload button is not visible
                    BtnUploadLeft.Visibility = Visibility.Visible; // Show the Left upload button
                if (!BtnUploadRight.IsVisible) // Check if the Right upload button is not visible
                    BtnUploadRight.Visibility = Visibility.Visible; // Show the Right upload button
                if (!BtnUploadBack.IsVisible) // Check if the Back upload button is not visible
                    BtnUploadBack.Visibility = Visibility.Visible; // Show the Back upload button
            }
        }
        // This method hides the upload button for a specific picture type.
        private void HideUploadButton(string type)
        {
            switch (type) // Check the type of picture and hide the corresponding upload button
            {
                case "Frontal": 
                    BtnUploadFrontal.Visibility = Visibility.Collapsed;// Hide the Frontal upload button
                    break;
                case "Left": 
                    BtnUploadLeft.Visibility = Visibility.Collapsed; // Hide the Left upload button
                    break;
                case "Right":
                    BtnUploadRight.Visibility = Visibility.Collapsed; // Hide the Right upload button
                    break;
                case "Back":
                    BtnUploadBack.Visibility = Visibility.Collapsed; // Hide the Back upload button
                    break;
            }
        }
        // This method resets the UI for the current week by hiding all picture previews and enabling all upload buttons.
        private void ResetCurrentWeekUI()
        {
            ImgFrontalPreview.Visibility = Visibility.Collapsed; // Hide the Frontal picture preview
            ImgLeftPreview.Visibility = Visibility.Collapsed; // Hide the Left picture preview
            ImgRightPreview.Visibility = Visibility.Collapsed; // Hide the Right picture preview
            ImgBackPreview.Visibility = Visibility.Collapsed; // Hide the Back picture preview

            BtnUploadFrontal.Visibility = Visibility.Visible; // Show the Frontal upload button
            BtnUploadLeft.Visibility = Visibility.Visible; // Show the Left upload button
            BtnUploadRight.Visibility = Visibility.Visible; // Show the Right upload button
            BtnUploadBack.Visibility = Visibility.Visible; // Show the Back upload button

            BtnUploadFrontal.IsEnabled = true; // Enable the Frontal upload button
            BtnUploadLeft.IsEnabled = true; // Enable the Left upload button
            BtnUploadRight.IsEnabled = true; // Enable the Right upload button
            BtnUploadBack.IsEnabled = true; // Enable the Back upload button
        }
        // This method shows a picture preview based on the type of picture and its file path.
        private void ShowPicturePreview(string type, string filePath)
        {
            var img = new BitmapImage(new Uri(filePath)); // Create a BitmapImage from the file path
            switch (type)
            {
                case "Frontal":
                    ImgFrontalPreview.Source = img; // Set the source of the Frontal picture preview to the BitmapImage
                    ImgFrontalPreview.Visibility = Visibility.Visible; // Make the Frontal picture preview visible
                    break;
                case "Left":
                    ImgLeftPreview.Source = img; // Set the source of the Left picture preview to the BitmapImage
                    ImgLeftPreview.Visibility = Visibility.Visible; // Make the Left picture preview visible
                    break;
                case "Right":
                    ImgRightPreview.Source = img; // Set the source of the Right picture preview to the BitmapImage
                    ImgRightPreview.Visibility = Visibility.Visible; // Make the Right picture preview visible
                    break;
                case "Back":
                    ImgBackPreview.Source = img; // Set the source of the Back picture preview to the BitmapImage
                    ImgBackPreview.Visibility = Visibility.Visible; // Make the Back picture preview visible
                    break;
            }
        }
        // Event handlers for uploading pictures for different types (Frontal, Left, Right, Back)
        private void BtnUploadFrontal_Click(object sender, RoutedEventArgs e)
        {
            UploadPicture("Frontal");
        }
        private void BtnUploadLeft_Click(object sender, RoutedEventArgs e)
        {
            UploadPicture("Left");
        }
        private void BtnUploadRight_Click(object sender, RoutedEventArgs e)
        {
            UploadPicture("Right");
        }
        private void BtnUploadBack_Click(object sender, RoutedEventArgs e)
        {
            UploadPicture("Back");
        }
        //Helper method for uploading a picture of a specific type (Frontal, Left, Right, Back).
        private void UploadPicture(string type)
        {
            var currentWeek = FitLab.AppState.SessionState.CurrentWeek; // Get the current week from the session state

            if (_currentPictureWeek != SessionState.CurrentWeek) // Check if the current picture week is not the current session week
            {
                MessageBox.Show("You can only upload pictures for the current week."); // Show an error message if the current picture week is not the current session week
                return;
            }
            if (_currentWeekProgress != null && _currentWeekProgress.Pictures.Any(p => p.Type == type)) // Check if the current week progress already contains a picture of the specified type
            {
                MessageBox.Show($"{type} picture already uploaded."); // Show an error message if a picture of the specified type has already been uploaded for the current week
                return;
            }
            if (_currentWeekProgress == null) // Check if the current week progress is null
            {
                _currentWeekProgress = new WeeklyProgress(); // Create a new WeeklyProgress object if it is null
            }
            var dlg = new Microsoft.Win32.OpenFileDialog // Create a new OpenFileDialog to select a picture file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp", // Set the filter to allow only image files
                Title = $"Select {type} picture" // Set the title of the dialog to indicate which type of picture to select
            };
            if (dlg.ShowDialog() == true) // Show the dialog and check if the user selected a file
            {
                var destFileName = System.IO.Path.Combine(
                    _uploadsFolder, 
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // Generate a unique destination file name in the uploads folder

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true); // Copy the selected file to the destination file name, overwriting if it already exists

                _currentWeekProgress.Pictures.Add(new ProgressPicture // Create a new ProgressPicture object and add it to the current week's progress pictures
                {
                    FilePath = destFileName, // Set the file path to the destination file name
                    DateTaken = DateTime.UtcNow, // Set the date taken to the current UTC time
                    Type = type // Set the type of the picture (Frontal, Left, Right, Back)
                });
                if (_user.WeeklyProgressPictures.Count <= _currentPictureWeek) // Check if the user's weekly progress pictures count is less than or equal to the current picture week
                {
                    while (_user.WeeklyProgressPictures.Count < _currentPictureWeek) //While the user's weekly progress pictures count is less than the current picture week
                    {
                        _user.WeeklyProgressPictures.Add(new WeeklyProgress()); // Add empty WeeklyProgress objects until the count matches the current picture week
                    } 
                    _user.WeeklyProgressPictures.Add(_currentWeekProgress); // Add the current week's progress pictures to the user's weekly progress pictures
                }

                _db.SaveUser(_user); // Save the updated user data to the database
                LoadCurrentPictureWeek(); // Load the current picture week to update the UI with the newly uploaded picture
            }
        }
    }
}
