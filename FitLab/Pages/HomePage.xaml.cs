using FitLab.AppState;
using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace FitLab.Pages
{
    //This page serves as the home dashboard, showing daily quick actions for the workout plan, water intake, and food intake. Also shows quick actions for weekly weight, measurements, and progress pictures.
    public partial class HomePage : Page
    {
        private readonly User _currentUser; // current user loaded from the database
        private readonly LocalDatabaseService _db = new(); // database service for saving/loading user data
        private ObservableCollection<Dish> _currentEditingDishesHome = new(); // dishes being edited in the home food intake quick action
        private string _quickWeightUnit = "Lbs"; // default weight unit for quick action
        private string _measUnitHome = "Inches"; // default measurement unit for home quick action
        private WeeklyProgress? _quickProgressWeekCache = null; // cache for the current week's progress pictures
        private readonly string _uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads"; // folder for saving uploaded progress pictures
        public HomePage()
        {
            InitializeComponent();
            _currentUser = _db.LoadFirstUser() ?? throw new Exception("No user found."); // Ensure we have a user loaded
            var tz = TimeZoneInfo.Local;
            if (FitLab.AppState.SessionState.CurrentWeek == 0 ||
                FitLab.AppState.SessionState.CurrentAbsoluteDay == 0 ||
                FitLab.AppState.SessionState.CurrentWorkoutDay == 0)
            {
                FitLab.AppState.SessionState.CurrentWeek =
                    FitLab.Components.CalculateCurrentWeek.GetWeekNumber(_currentUser.CreatedOn, tz);

                FitLab.AppState.SessionState.CurrentWorkoutDay =
                    FitLab.Helpers.CalculateCurrentDay.GetCurrentDayNumber(
                        _currentUser.CreatedOn,
                        _currentUser.WorkoutPlan?.PlanLength ?? 1,
                        tz);

                var createdLocal = _currentUser.CreatedOn.Kind == DateTimeKind.Utc
                    ? TimeZoneInfo.ConvertTimeFromUtc(_currentUser.CreatedOn, tz)
                    : (_currentUser.CreatedOn.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(_currentUser.CreatedOn, DateTimeKind.Local)
                        : _currentUser.CreatedOn);

                FitLab.AppState.SessionState.CurrentAbsoluteDay =
                    FitLab.Helpers.CalculateCurrentDay.GetAbsoluteDayNumber(createdLocal, tz);
            }
            //Welcome Text setup
            WelcomeText.Text = $"Welcome {_currentUser.Name ?? "User"} to Week {SessionState.CurrentWeek}/Day {SessionState.CurrentAbsoluteDay} of your workout journey! " +
                               $"This is Day {SessionState.CurrentWorkoutDay} of your workout plan.";
            var workoutDayNum = SessionState.CurrentWorkoutDay; // uses SessionState to get the current workout day number
            var dayData = _currentUser.WorkoutPlan?.Days.FirstOrDefault(d => d.DayNumber == workoutDayNum); // get the workout plan for the current day
            if (dayData != null) //if the workout plan for today exists
            {
                foreach (var ex in dayData.Warmup) // add warmup exercises to the quick action list
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Warmup")); // uses CreateExerciseNode to create a UI node for each exercise
                foreach (var ex in dayData.Main) // add main exercises to the quick action list
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Main")); // uses CreateExerciseNode to create a UI node for each exercise
                foreach (var ex in dayData.Cooldown) // add cooldown exercises to the quick action list
                    ExerciseQuickList.Children.Add(CreateExerciseNode(ex, "Cooldown")); // uses CreateExerciseNode to create a UI node for each exercise
            }
            TxtHomeWaterDayLabel.Text = $"Day {SessionState.CurrentAbsoluteDay}"; // uses SessionState to get the current absolute day number for the water intake quick action
            var today = GetTodayLocalDate(); // gets today's date in local time
            var todayEntry = _currentUser.WaterIntake.FirstOrDefault(w => w.Date.Date == today); // checks if there is an entry for today in the user's water intake records
            if (todayEntry == null) // if there is no entry for today
            {
                todayEntry = new DailyWaterIntake // create a new entry for today
                {
                    Date = today, // sets the date to today
                    Cups = 0 // initializes the cups to 0
                };
                _currentUser.WaterIntake.Add(todayEntry); // adds the new entry to the user's water intake records
                _db.SaveUser(_currentUser); // saves the updated user data to the database
            }
            LoadWaterIntakeForToday(); // loads today's water intake from the user's records
            SetupFoodQuickAction(); // sets up the food intake quick action
            SetupWeeklyWeightQuickAction(); // sets up the weekly weight quick action
            SetupWeeklyMeasurementsQuickAction(); // sets up the weekly measurements quick action
            SetupWeeklyProgressQuickAction(); // sets up the weekly progress pictures quick action
        }
        //Creates the UI node for an exercise in the quick action list.
        private StackPanel CreateExerciseNode(Exercise ex, string sectionName)
        {
            var outerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5), HorizontalAlignment = HorizontalAlignment.Stretch }; // container row for text + button
            var textPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center, Width = 600 }; // holds title and description
            var mainInfo = new TextBlock { Text = $"{ex.Name} - {string.Join(", ", ex.Type)} - {ex.MuscleGroup} - {ex.Difficulty} - {string.Join(", ", ex.Equipment)}", Foreground = Brushes.White, FontWeight = FontWeights.Bold }; // exercise summary line
            var description = new TextBlock { Text = ex.Description, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.LightGray, FontSize = 12 }; // exercise description
            var btn = new Button { Width = 110, Height = 36, Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(0), Margin = new Thickness(10, 5, 0, 5), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center, Cursor = Cursors.Hand, SnapsToDevicePixels = true, UseLayoutRounding = true }; // right-side action area
            void MarkCompleted()
            {
                btn.IsEnabled = false; // disable interaction
                btn.Cursor = Cursors.Arrow; // normal cursor
                btn.Height = 36; // ensure consistent size
                btn.Padding = new Thickness(10, 4, 10, 4); // padding around text
                btn.Background = Brushes.Black; // black background
                btn.BorderBrush = Brushes.LimeGreen; // green border
                btn.BorderThickness = new Thickness(1); // 1px border thickness
                btn.Content = new TextBlock { Text = "Completed", Foreground = Brushes.LimeGreen, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center }; // green text label
            }
            if (IsExerciseCompletedToday(ex.Guid)) { MarkCompleted(); } // already completed
            else
            {
                btn.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/CheckIcon.png")), Width = 24, Height = 24, Stretch = Stretch.Uniform }; // check icon
                btn.Click += (s, e) =>
                {
                    var modal = new CompleteExModal(ex, sectionName,
                        onComplete: () => { ExerciseCompletionModal.Visibility = Visibility.Collapsed; ModalOverlay.Visibility = Visibility.Collapsed; MarkCompleted(); }, // hide modal + overlay, mark complete
                        onCancel: () => { ExerciseCompletionModal.Visibility = Visibility.Collapsed; ModalOverlay.Visibility = Visibility.Collapsed; }); // hide modal + overlay
                    ExerciseCompletionModal.Content = modal; // load modal
                    ExerciseCompletionModal.Visibility = Visibility.Visible; // show modal
                    ModalOverlay.Visibility = Visibility.Visible; // show overlay
                };
            }
            textPanel.Children.Add(mainInfo); // add title
            textPanel.Children.Add(description); // add description
            outerPanel.Children.Add(textPanel); // add text panel
            outerPanel.Children.Add(btn); // add button
            return outerPanel; // return row
        }
        // ===== Water Intake (Home Quick Action) =====
        //Loads today's water intake from the user data and displays it in the home quick action.
        private void LoadWaterIntakeForToday()
        {
            var today = GetTodayLocalDate(); // gets today's date in local time
            var entry = _currentUser.WaterIntake.FirstOrDefault(w => w.Date.Date == today); // checks if there is an entry for today in the user's water intake records
            int cups = entry?.Cups ?? 0; // gets the number of cups from the entry, or 0 if there is no entry
            TxtHomeWaterCups.Text = $"{cups} Cup(s)"; // sets the text of the water cups label to the number of cups
        }
        // Adjusts the number of cups for today's water intake by a specified delta.
        private void AdjustCupsHome(int delta) 
        {
            var today = GetTodayLocalDate(); // gets today's date in local time
            var entry = _currentUser.WaterIntake.FirstOrDefault(w => w.Date.Date == today); // checks if there is an entry for today in the user's water intake records
            if (entry == null) // if there is no entry for today
            {
                entry = new DailyWaterIntake // create a new entry for today
                {
                    Date = today, // sets the date to today
                    Cups = 0 // initializes the cups to 0
                };
                _currentUser.WaterIntake.Add(entry); // adds the new entry to the user's water intake records
            }
            entry.Cups = Math.Max(0, entry.Cups + delta); // adjusts the cups by the specified delta, ensuring it does not go below 0
            TxtHomeWaterCups.Text = $"{entry.Cups} Cup(s)"; // updates the text of the water cups label to the new number of cups
            _db.SaveUser(_currentUser); // saves the updated user data to the database
        }
        // Buttons for water increase and decrease actions.
        private void BtnHomeWaterIncrease_Click(object sender, RoutedEventArgs e) => AdjustCupsHome(1);
        private void BtnHomeWaterDecrease_Click(object sender, RoutedEventArgs e) => AdjustCupsHome(-1);
        // Gets today's date in local time
        private static DateTime GetTodayLocalDate()
        {
            var nowUtc = DateTime.UtcNow; // gets the current time in UTC
            return TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZoneInfo.Local).Date; // converts the UTC time to local time and returns the date part
        }
        private void SetupFoodQuickAction()
        {
            TxtHomeFoodDayLabel.Text = $"Day {SessionState.CurrentAbsoluteDay}"; // use session state to get the current absolute day number
        }
        // Button click handlers for adding meals and dishes in the home food intake quick action.
        private void BtnHomeAddMeal_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingDishesHome = new ObservableCollection<Dish>(); // reset the current editing dishes collection
            PanelDishesHome.ItemsSource = _currentEditingDishesHome; // bind the dishes list to the UI
            CmbMealTimeHome.SelectedIndex = -1; // reset the meal time selection
            PanelAddMealHome.Visibility = Visibility.Visible; // show the add meal panel
        }
        // Button click handlers for adding dishes in the home food intake quick action.
        private void BtnAddDishHome_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingDishesHome.Add(new Dish()); // add a new empty dish to the current editing dishes collection
        }
        // Button click handlers for removing dishes in the home food intake quick action.
        private void BtnRemoveDishHome_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Dish dish) // get the dish from the button's DataContext
                _currentEditingDishesHome.Remove(dish); // remove the dish from the current editing dishes collection
        }
        // Button click handler for saving the meal in the home food intake quick action.
        private void BtnSaveMealHome_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMealTimeHome.SelectedItem is not ComboBoxItem selectedItem) // check if a meal time is selected
            { 
                MessageBox.Show("Select a meal time."); // show a message if no meal time is selected
                return;
            }
            string mealTime = selectedItem.Content?.ToString() ?? ""; // get the selected meal time from the ComboBoxItem
            var today = GetTodayLocalDate(); // gets today's date in local time
            var entry = _currentUser.FoodIntake.FirstOrDefault(x => x.Date.Date == today); // checks if there is an entry for today in the user's food intake records
            if (entry == null) // if there is no entry for today
            {
                entry = new DailyFoodIntake { Date = today, Meals = new List<Meal>() }; // create a new entry for today
                _currentUser.FoodIntake.Add(entry); // adds a new entry for today to the user's food intake records
            }
            entry.Meals.Add(new Meal // create a new meal with the selected meal time and the current editing dishes
            {
                MealTime = mealTime, // sets the meal time to the selected value
                Dishes = _currentEditingDishesHome.ToList() // converts the current editing dishes collection to a list
            });
            _db.SaveUser(_currentUser); // saves the updated user data to the database
            PanelAddMealHome.Visibility = Visibility.Collapsed; // hides the add meal panel
            MessageBox.Show("Meal saved."); // shows a message indicating the meal was saved
        }
        //Gets the start date of the week based on the user's creation date, adjusted to local time.
        private DateTime GetWeekStartLocal()
        {
            var created = _currentUser.CreatedOn; // gets the user's creation date
            if (created.Kind == DateTimeKind.Utc) // if the creation date is in UTC
                return TimeZoneInfo.ConvertTimeFromUtc(created, TimeZoneInfo.Local).Date; // convert it to local time and return the date part
            if (created.Kind == DateTimeKind.Unspecified) // if the creation date is unspecified
                return DateTime.SpecifyKind(created, DateTimeKind.Local).Date; // specify it as local time and return the date part
            return created.Date; // Local
        }

        private DateTime GetDateForWeek(int weekIndex) => GetWeekStartLocal().AddDays(weekIndex * 7); // calculates the start date of the specified week index based on the user's creation date
        // Boolean helper to check if the current week has a weight entry.
        private bool HasCurrentWeekWeight()
        {
            var weekDate = GetDateForWeek(FitLab.AppState.SessionState.CurrentWeek); // gets the start date of the current week based on the session state
            return _currentUser.WeightHistory.Any(w => w.Date.Date == weekDate.Date); // checks if there is any weight entry for the current week by comparing the date part
        }
        // Sets up the quick action for entering weekly weight.
        private void SetupWeeklyWeightQuickAction()
        {
            TxtWeeklyWeightWeekLabel.Text = "Current Week"; // sets the label for the weekly weight quick action to "Current Week"
            if (CmbQuickWeightUnit.SelectedIndex < 0) CmbQuickWeightUnit.SelectedIndex = 0; // ensures the quick weight unit ComboBox has a default selection
            CmbQuickWeightUnit.SelectionChanged += (s, e) => // handles the selection change event for the quick weight unit ComboBox
            {
                if (CmbQuickWeightUnit.SelectedItem is ComboBoxItem item) // checks if the selected item is a ComboBoxItem
                    _quickWeightUnit = item.Content?.ToString() ?? "Lbs"; // sets the quick weight unit based on the selected item, defaulting to "Lbs" if no item is selected
            };
            BorderWeeklyWeightQuick.Visibility = HasCurrentWeekWeight() // checks if the current week already has a weight entry
                ? Visibility.Collapsed // if it does, hides the quick action panel
                : Visibility.Visible; // if it doesn't, shows the quick action panel for entering weekly weight
        }
        // Button click handler for saving the quick weekly weight entry.
        private void BtnQuickWeightSave_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtQuickWeeklyWeight.Text.Trim(), out var entered) || entered <= 0)
            {
                MessageBox.Show("Enter a valid weight."); // shows a message if the entered weight is not a valid positive number
                return;
            }
            double weightLbs = _quickWeightUnit == "Kilograms" // converts the entered weight to pounds if the selected unit is kilograms
                ? FitLab.Components.Conversions.KgToLbs(entered) // otherwise uses the entered value directly
                : entered; // uses the entered value directly
            var weekDate = GetDateForWeek(FitLab.AppState.SessionState.CurrentWeek); // gets the start date of the current week based on the session state
            if (!HasCurrentWeekWeight()) // checks if the current week does not already have a weight entry
            {
                _currentUser.WeightHistory.Add(new WeightEntry // adds a new weight entry for the current week
                {
                    Date = weekDate, // sets the date to the start of the current week
                    WeightLbs = weightLbs // sets the weight in pounds
                });
                _db.SaveUser(_currentUser); // saves the updated user data to the database
            }
            BorderWeeklyWeightQuick.Visibility = Visibility.Collapsed; // hides the quick action panel after saving the weight
            MessageBox.Show("Weekly weight saved."); // shows a message indicating the weekly weight was saved
        }
        // Boolean helper to check if the current week has body measurements.
        private bool HasCurrentWeekMeasurements()
        { 
            _currentUser.BodyMeasurements ??= new List<WeeklyBodyMeasurement>(); // ensures the BodyMeasurements list is initialized
            var weekDate = GetDateForWeek(SessionState.CurrentWeek); // gets the start date of the current week based on the session state
            return _currentUser.BodyMeasurements.Any(x => x.Date.Date == weekDate.Date); // checks if there is any body measurement entry for the current week by comparing the date part
        }
        // Sets up the quick action for entering weekly body measurements.
        private void SetupWeeklyMeasurementsQuickAction()
        {
            TxtWeeklyMeasWeekLabel.Text = "Current Week"; // sets the label for the weekly measurements quick action to "Current Week"
            if (CmbMeasUnitHome.SelectedIndex < 0) CmbMeasUnitHome.SelectedIndex = 0; // ensures the measurement unit ComboBox has a default selection
            CmbMeasUnitHome.SelectionChanged += (s, e) => // handles the selection change event for the measurement unit ComboBox
            {
                if (CmbMeasUnitHome.SelectedItem is ComboBoxItem item) // checks if the selected item is a ComboBoxItem
                    _measUnitHome = item.Content?.ToString() ?? "Inches"; // sets the measurement unit based on the selected item, defaulting to "Inches" if no item is selected
            };
            BorderWeeklyMeasurementsQuick.Visibility = HasCurrentWeekMeasurements() // checks if the current week already has body measurements
                ? Visibility.Collapsed // if it does, hides the quick action panel
                : Visibility.Visible; // if it doesn't, shows the quick action panel for entering weekly body measurements
        }
        // Checkbox handlers for showing/hiding advanced measurements in the home quick action.
        private void ChkAdvancedMeasHome_Checked(object sender, RoutedEventArgs e)
            => PanelAdvancedMeasHome.Visibility = Visibility.Visible;
        // Checkbox handler for hiding advanced measurements in the home quick action.
        private void ChkAdvancedMeasHome_Unchecked(object sender, RoutedEventArgs e)
            => PanelAdvancedMeasHome.Visibility = Visibility.Collapsed;
        // Helper method to parse measurement input and convert if necessary.
        private bool TryParse(string txt, out double val)
        {
            if (double.TryParse(txt.Trim(), out val)) //If the input can be parsed as a double
            {
                if (_measUnitHome == "Centimeters") //If the selected measurement unit is centimeters
                    val = FitLab.Components.Conversions.CmToInches(val); // convert to inches
                return val > 0; // return true if the value is greater than 0
            }
            return false; // return false if parsing fails
        }
        // Button click handler for saving the quick weekly body measurements entry.
        private void BtnSaveMeasHome_Click(object sender, RoutedEventArgs e)
        {
            bool advanced = ChkAdvancedMeasHome.IsChecked == true; // checks if the advanced measurements checkbox is checked
            if (!TryParse(TxtChestHome.Text, out var chest) || // checks if the chest measurement is a valid number
                !TryParse(TxtWaistHome.Text, out var waist) || // checks if the waist measurement is a valid number
                !TryParse(TxtHipsHome.Text, out var hips)) // checks if the hips measurement is a valid number
            {
                MessageBox.Show("Enter valid numbers for Chest, Waist, and Hips."); // shows a message if any of the basic required fields are not valid numbers
                return;
            }
            double neck = 0, shoulders = 0, upperArm = 0, forearm = 0, wrist = 0, thigh = 0, calf = 0, ankle = 0; // initialize advanced measurements to 0
            if (advanced) // if advanced measurements are enabled
            {
                if (!TryParse(TxtNeckHome.Text, out neck) || // checks if the neck measurement is a valid number
                    !TryParse(TxtShouldersHome.Text, out shoulders) || // checks if the shoulders measurement is a valid number
                    !TryParse(TxtUpperArmHome.Text, out upperArm) || // checks if the upper arm measurement is a valid number
                    !TryParse(TxtForearmHome.Text, out forearm) || // checks if the forearm measurement is a valid number
                    !TryParse(TxtWristHome.Text, out wrist) || // checks if the wrist measurement is a valid number
                    !TryParse(TxtThighHome.Text, out thigh) || // checks if the thigh measurement is a valid number
                    !TryParse(TxtCalfHome.Text, out calf) || // checks if the calf measurement is a valid number
                    !TryParse(TxtAnkleHome.Text, out ankle)) // checks if the ankle measurement is a valid number
                {
                    MessageBox.Show("Advanced is on: all fields are required. Check your inputs."); // shows a message if any of the advanced fields are not valid numbers
                    return;
                }
            }
            var weekDate = GetDateForWeek(FitLab.AppState.SessionState.CurrentWeek); // gets the start date of the current week based on the session state
            if (HasCurrentWeekMeasurements()) // checks if the current week already has body measurements
            {
                BorderWeeklyMeasurementsQuick.Visibility = Visibility.Collapsed; // hides the quick action panel if measurements already exist
                return; 
            }
            _currentUser.BodyMeasurements ??= new List<WeeklyBodyMeasurement>(); // ensures the BodyMeasurements list is initialized
            _currentUser.BodyMeasurements.Add(new WeeklyBodyMeasurement // adds a new body measurement entry for the current week
            {
                Date = weekDate,
                Chest = chest,
                Waist = waist,
                Hips = hips,
                Neck = advanced ? neck : (double?)null,
                Shoulders = advanced ? shoulders : (double?)null,
                UpperArm = advanced ? upperArm : (double?)null,
                Forearm = advanced ? forearm : (double?)null,
                Wrist = advanced ? wrist : (double?)null,
                Thigh = advanced ? thigh : (double?)null,
                Calf = advanced ? calf : (double?)null,
                Ankle = advanced ? ankle : (double?)null
            });
            _db.SaveUser(_currentUser); // saves the updated user data to the database
            BorderWeeklyMeasurementsQuick.Visibility = Visibility.Collapsed; // hides the quick action panel after saving the measurements
            MessageBox.Show("Weekly measurements saved."); // shows a message indicating the weekly measurements were saved
        }
        // Gets or creates the current week's progress pictures, initializing if necessary.
        private WeeklyProgress GetOrCreateCurrentWeekProgress()
        {
            var week = FitLab.AppState.SessionState.CurrentWeek; // gets the current week number from the session state

            _currentUser.WeeklyProgressPictures ??= new List<WeeklyProgress>(); // ensures the WeeklyProgressPictures list is initialized
            while (_currentUser.WeeklyProgressPictures.Count <= week) // ensures there is an entry for the current week
                _currentUser.WeeklyProgressPictures.Add(new WeeklyProgress { Pictures = new List<ProgressPicture>() }); // adds new entries until the list has enough weeks
            var wp = _currentUser.WeeklyProgressPictures[week]; // gets the progress pictures for the current week
            if (wp.Pictures == null) wp.Pictures = new List<ProgressPicture>(); // ensures the Pictures list is initialized
            return wp; // returns the progress pictures for the current week
        }
        //Boolean helper to check if the current week's progress pictures contain all four required types.
        private bool HasAllFour(WeeklyProgress wp)
        {
            var set = wp.Pictures.Select(p => p.Type).ToHashSet(StringComparer.OrdinalIgnoreCase); // creates a set of picture types for the current week
            return set.Contains("Frontal") && set.Contains("Left") && set.Contains("Right") && set.Contains("Back"); // checks if all four required picture types are present in the set
        }
        // Sets up the quick action for uploading weekly progress pictures.
        private void SetupWeeklyProgressQuickAction()
        {
            TxtWeeklyProgressWeekLabel.Text = "Current Week"; // sets the label for the weekly progress quick action to "Current Week"
            _quickProgressWeekCache = GetOrCreateCurrentWeekProgress(); // gets or creates the current week's progress pictures
            ResetProgressPreviewsAndButtons(); // resets the progress picture previews and upload buttons
            foreach (var pic in _quickProgressWeekCache.Pictures) // iterates through the pictures for the current week
            {
                ShowPicturePreviewHome(pic.Type, pic.FilePath); // shows the picture preview for each uploaded picture
                HideUploadButtonHome(pic.Type); // hides the upload button for each uploaded picture type
            }
            BorderWeeklyProgressQuick.Visibility = HasAllFour(_quickProgressWeekCache) // checks if all four required picture types are present
                ? Visibility.Collapsed // if all four pictures are uploaded, hides the quick action panel
                : Visibility.Visible; // if not all four pictures are uploaded, shows the quick action panel for uploading progress pictures
        }
        // Resets the progress picture previews and upload buttons in the home quick action.
        private void ResetProgressPreviewsAndButtons()
        {
            ImgFrontalPreviewHome.Visibility = Visibility.Collapsed; // hides the frontal picture preview
            ImgLeftPreviewHome.Visibility = Visibility.Collapsed; // hides the left picture preview
            ImgRightPreviewHome.Visibility = Visibility.Collapsed; // hides the right picture preview
            ImgBackPreviewHome.Visibility = Visibility.Collapsed; // hides the back picture preview
            BtnUploadFrontalHome.Visibility = Visibility.Visible; // shows the upload button for the frontal picture
            BtnUploadLeftHome.Visibility = Visibility.Visible; // shows the upload button for the left picture
            BtnUploadRightHome.Visibility = Visibility.Visible; // shows the upload button for the right picture
            BtnUploadBackHome.Visibility = Visibility.Visible; // shows the upload button for the back picture
        }
        // Hides the upload button for a specific picture type in the home quick action.
        private void HideUploadButtonHome(string type)
        {
            switch (type)
            {
                case "Frontal": BtnUploadFrontalHome.Visibility = Visibility.Collapsed; break;
                case "Left": BtnUploadLeftHome.Visibility = Visibility.Collapsed; break;
                case "Right": BtnUploadRightHome.Visibility = Visibility.Collapsed; break;
                case "Back": BtnUploadBackHome.Visibility = Visibility.Collapsed; break;
            }
        }
        // Shows the picture preview for a specific picture type in the home quick action.
        private void ShowPicturePreviewHome(string type, string path)
        {
            var bmp = new BitmapImage(new Uri(path)); // creates a BitmapImage from the file path
            switch (type)
            {
                case "Frontal": ImgFrontalPreviewHome.Source = bmp; ImgFrontalPreviewHome.Visibility = Visibility.Visible; break;
                case "Left": ImgLeftPreviewHome.Source = bmp; ImgLeftPreviewHome.Visibility = Visibility.Visible; break;
                case "Right": ImgRightPreviewHome.Source = bmp; ImgRightPreviewHome.Visibility = Visibility.Visible; break;
                case "Back": ImgBackPreviewHome.Source = bmp; ImgBackPreviewHome.Visibility = Visibility.Visible; break;
            }
        }
        // Completes the weekly progress if all four required pictures are uploaded.
        private void CompleteIfAllFour()
        {
            if (_quickProgressWeekCache == null) return; // checks if the quick progress week cache is not null
            if (HasAllFour(_quickProgressWeekCache)) // checks if all four picture types are present
            {
                _db.SaveUser(_currentUser); // saves the updated user data to the database
                BorderWeeklyProgressQuick.Visibility = Visibility.Collapsed; // hides the quick action panel for weekly progress pictures
                MessageBox.Show("Weekly progress pictures saved."); // shows a message indicating the weekly progress pictures were saved
            }
        }
        // Button click handlers for uploading progress pictures in the home quick action.
        private void BtnUploadFrontalHome_Click(object sender, RoutedEventArgs e) => UploadProgressPhotoHome("Frontal");
        private void BtnUploadLeftHome_Click(object sender, RoutedEventArgs e) => UploadProgressPhotoHome("Left");
        private void BtnUploadRightHome_Click(object sender, RoutedEventArgs e) => UploadProgressPhotoHome("Right");
        private void BtnUploadBackHome_Click(object sender, RoutedEventArgs e) => UploadProgressPhotoHome("Back");
        // Uploads a progress photo for the specified type in the home quick action.
        private void UploadProgressPhotoHome(string type)
        {
            var week = FitLab.AppState.SessionState.CurrentWeek; // gets the current week number from the session state
            _quickProgressWeekCache = GetOrCreateCurrentWeekProgress(); // gets or creates the current week's progress pictures
            if (_quickProgressWeekCache.Pictures.Any(p => p.Type.Equals(type, StringComparison.OrdinalIgnoreCase))) // checks if the current week already has a picture of the specified type
            {
                MessageBox.Show($"{type} picture already uploaded for this week."); // shows a message if the picture of the specified type is already uploaded for the current week
                return;
            }
            var dlg = new Microsoft.Win32.OpenFileDialog // creates a file dialog to select an image file
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = $"Select {type} picture"
            };
            if (dlg.ShowDialog() == true) // shows the dialog and checks if the user selected a file
            {
                Directory.CreateDirectory(_uploadsFolder); // ensures the uploads folder exists
                var destFile = System.IO.Path.Combine(
                    _uploadsFolder, $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}"); // creates a unique destination file path in the uploads folder
                File.Copy(dlg.FileName, destFile, true); // copies the selected file to the destination path, overwriting if it already exists
                _quickProgressWeekCache.Pictures.Add(new ProgressPicture // adds a new progress picture entry for the current week
                {
                    FilePath = destFile, // sets the file path to the copied file
                    DateTaken = DateTime.UtcNow, // sets the date taken to the current UTC time
                    Type = type // sets the type to the specified type
                });
                ShowPicturePreviewHome(type, destFile); // shows the picture preview for the uploaded picture
                HideUploadButtonHome(type); // hides the upload button for the uploaded picture type
                _currentUser.WeeklyProgressPictures[week] = _quickProgressWeekCache; // updates the user's weekly progress pictures with the current week's progress
                _db.SaveUser(_currentUser); // saves the updated user data to the database
                CompleteIfAllFour(); // checks if all four required pictures are uploaded and completes the weekly progress if so
            } 
        }
        // Converts any DateTime to a local date safely.
        private static DateTime ToLocalDate(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local).Date; // convert UTC to local
            if (dt.Kind == DateTimeKind.Local) return dt.Date; // already local
            return DateTime.SpecifyKind(dt, DateTimeKind.Local).Date; // assume local if unspecified
        }
        // Checks if the exercise is completed today.
        private bool IsExerciseCompletedToday(Guid exId)
        {
            var today = GetTodayLocalDate(); // get today's local date
            var ce = _currentUser.CompletedExercises.FirstOrDefault(c => c.ExerciseId == exId); // find completion record
            return ce != null && ce.Entries.Any(en => ToLocalDate(en.DateCompleted) == today); // match on date
        }
    }
}

