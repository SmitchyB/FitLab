using FitLab.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FitLab.Pages
{
    public partial class MyBodyPage : Page
    {
        private readonly LocalDatabaseService _db = new();
        private User _user;

        public MyBodyPage()
        {
            InitializeComponent(); // THIS LINE IS MANDATORY

            // Load user from DB
            _user = _db.LoadFirstUser() ?? new User();

            // Populate fields
            TxtName.Text = _user.Name;
            DatePickerDOB.SelectedDate = _user.DateOfBirth;
            // Gender
            foreach (ComboBoxItem item in CmbGender.Items)
            {
                if ((item.Content?.ToString() ?? "") == _user.Gender.Replace(" (On Hormones)", "").Replace(" (Not on Hormones)", ""))
                {
                    CmbGender.SelectedItem = item;
                    break;
                }
            }

            if (_user.Gender.Contains("Trans-Feminine") || _user.Gender.Contains("Trans-Masculine"))
            {
                ChkOnHormones.Visibility = Visibility.Visible;
                ChkOnHormones.IsChecked = _user.Gender.Contains("On Hormones");
            }

            // Height display in Feet/Inches by default
            CmbHeightUnit.SelectedIndex = 2;
            UpdateHeightDisplay(_user.HeightInches);
            PanelFeetInches.Visibility = Visibility.Visible;
        }
        private void UpdateHeightDisplay(double heightInches)
        {
            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected)
            {
                var unit = selected.Content?.ToString() ?? "";

                if (unit == "Centimeters")
                {
                    TxtHeightPrimary.Text = Math.Round(Components.Conversions.InchesToCm(heightInches), 1).ToString();
                }
                else if (unit == "Inches")
                {
                    TxtHeightPrimary.Text = Math.Round(heightInches, 1).ToString();
                }
                else if (unit == "Feet/Inches")
                {
                    int feet = (int)(heightInches / 12);
                    double inches = heightInches - (feet * 12);
                    TxtHeightFeet.Text = feet.ToString();
                    TxtHeightInches.Text = Math.Round(inches, 1).ToString();
                }
            }
        }

        private void CmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected)
            {
                var value = selected.Content?.ToString() ?? "";
                if (value == "Trans-Feminine" || value == "Trans-Masculine")
                {
                    ChkOnHormones.Visibility = Visibility.Visible;
                }
                else
                {
                    ChkOnHormones.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CmbHeightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hide panels
            PanelSingleInput.Visibility = Visibility.Collapsed;
            PanelFeetInches.Visibility = Visibility.Collapsed;

            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected)
            {
                var value = selected.Content?.ToString() ?? "";

                if (value == "Centimeters" || value == "Inches")
                {
                    PanelSingleInput.Visibility = Visibility.Visible;
                }
                else if (value == "Feet/Inches")
                {
                    PanelFeetInches.Visibility = Visibility.Visible;
                }

                // Update displayed values from stored height
                UpdateHeightDisplay(_user.HeightInches);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

            TxtName.IsReadOnly = false;
            CmbGender.IsEnabled = true;
            DatePickerDOB.IsEnabled = true;

            // Height inputs enabled
            TxtHeightPrimary.IsReadOnly = false;
            TxtHeightFeet.IsReadOnly = false;
            TxtHeightInches.IsReadOnly = false;

            BtnUpdate.Visibility = Visibility.Visible;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

            // Save inputs to _user
            _user.Name = TxtName.Text.Trim();
            _user.DateOfBirth = DatePickerDOB.SelectedDate ?? DateTime.MinValue;

            // Gender
            if (CmbGender.SelectedItem is ComboBoxItem selected)
            {
                var genderBase = selected.Content?.ToString() ?? "";
                if (genderBase == "Trans-Feminine" || genderBase == "Trans-Masculine")
                {
                    _user.Gender = genderBase + (ChkOnHormones.IsChecked == true ? " (On Hormones)" : " (Not on Hormones)");
                }
                else
                {
                    _user.Gender = genderBase;
                }
            }

            // Height
            if (CmbHeightUnit.SelectedItem is ComboBoxItem heightSelected)
            {
                var unit = heightSelected.Content?.ToString() ?? "";

                if (unit == "Centimeters")
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double cm))
                        _user.HeightInches = Components.Conversions.CmToInches(cm);
                }
                else if (unit == "Inches")
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double inches))
                        _user.HeightInches = inches;
                }
                else if (unit == "Feet/Inches")
                {
                    if (int.TryParse(TxtHeightFeet.Text, out int feet) && double.TryParse(TxtHeightInches.Text, out double inches))
                        _user.HeightInches = Components.Conversions.FeetInchesToInches(feet, inches);
                }
            }

            // Save to DB
            _db.SaveUser(_user);

            // Disable editing
            TxtName.IsReadOnly = true;
            CmbGender.IsEnabled = false;
            DatePickerDOB.IsEnabled = false;

            TxtHeightPrimary.IsReadOnly = true;
            TxtHeightFeet.IsReadOnly = true;
            TxtHeightInches.IsReadOnly = true;

            BtnUpdate.Visibility = Visibility.Collapsed;
        }
    }
}
